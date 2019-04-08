using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using NeutronEvents;
using HistoryManager = Neutron.Global.HistoryManager;

namespace Neutron.Forms
{
    public partial class FrmHotAction : MetroForm
    {

        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly GenericRepository<VelocityCode> _repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        private readonly GenericRepository<HeightCode> _repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
        private readonly GenericRepository<LocationCode> _repoLocationCode = new GenericRepository<LocationCode>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());
        private readonly GenericRepository<Station> _repoStation = new GenericRepository<Station>(new NeutronDb());
        private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());

        // private readonly HistoryManager _historyManager = new HistoryManager();
        private LocationsRepository _locationsRepository;
        private readonly InventoryRepository _repoInv = new InventoryRepository();
        private readonly ItemDefinitionsRepository _itemDefinitionsRepository = new ItemDefinitionsRepository();

        private BindingListView<ItemDefinitionView> bindingSourceItemDefinitionViewEquin = null;

        private readonly BindingSource _bindingSourceCurrent = new BindingSource();
        private readonly BindingSource _bindingSourceItemDefinitions = new BindingSource();
        private readonly BindingSource _bindingSourceNewLocations = new BindingSource();
        public bool CloseButtonPressed { get; set; }
        public RackOrderView CurrentItem;
        private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        readonly NeutronVariables _neutronVariables;
        DynamicLogger _logger;
        readonly StationView _station;
        string _imagesDirectory;
        readonly IAkaRepository _akaRepository;
        private readonly INomenclature _nomenclature;
        private readonly IJsonData _jsonData;
        private CostCenterManager _costCenterManager;
        private ItemDefinitionView _currentItemDefinition;
        private Location _currentLocation;
        private GridDataType _currentGridDataType = GridDataType.None;
        private bool _formLoading = true;
        private bool _hotPickButtonPressed = false;
        private bool _hotStoreButtonPressed = false;
        private InventoryManager _inventoryManager;
        private Stopwatch stopwatch;

        private string _currentGrid;

        public enum GridDataType
        {
            None,
            Item,
            Current,
            New
        }

        public delegate void UpdateDataGridDelegate(BindingSource bindingSource);


        public FrmHotAction(StationView station, IJsonData jsonData
            , IAkaRepository akaRepository, NeutronVariables neutronVariables, INomenclature nomenclature, string item = @"")
        {
            InitializeComponent();
            _station = station;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _akaRepository = akaRepository;
            _nomenclature = nomenclature;
            InitForm(item);
        }

        private void InitForm(string item)
        {
            KeyPreview = true;
            SetupLogger();
            SetupGridItemDefinition();

            UpdateNomenclature();
            LabelFormTitle.Text = "Hot Actions";
            HideTabControlTabs();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
            _locationsRepository = new LocationsRepository();
            FillComboBoxes();
            _inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
            InitialSearch(item);


        }

        private void InitialSearch(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                FindHotRecord();
                TextBoxFindItem.Focus();
            }
            else
            {
                TextBoxFindItem.Text = item.Trim();
                FindHotRecord(item.Trim().ToLower());
                TextBoxFindItem.Focus();
            }
        }

        private async Task FillCostCenterComboBox()
        {
            var costCenterPath = LoaderSettings.GetCostCenterPath();
            _costCenterManager = new CostCenterManager(costCenterPath);
            var costCenterList = await _costCenterManager.GetCostCenterListAsync();
            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
        }

        private void FillComboBoxes()
        {
            ComboBoxSizeCodeItem.DataSource = _repoSizeCode.All();
            ComboBoxSizeCodeItem.DisplayMember = "Name";
            ComboBoxSizeCodeItem.ValueMember = "Id";

            ComboBoxVelocityCodeItem.DataSource = _repoVelocityCode.All();
            ComboBoxVelocityCodeItem.DisplayMember = "Name";
            ComboBoxVelocityCodeItem.ValueMember = "Id";

            ComboBoxHeightCodeItem.DataSource = _repoHeightCode.All();
            ComboBoxHeightCodeItem.DisplayMember = "Name";
            ComboBoxHeightCodeItem.ValueMember = "Id";

            ComboBoxLocationCodeItem.DataSource = _repoLocationCode.All();
            ComboBoxLocationCodeItem.DisplayMember = "Name";
            ComboBoxLocationCodeItem.ValueMember = "Id";

            ComboBoxSizeCodeLocation.DataSource = _repoSizeCode.All();
            ComboBoxSizeCodeLocation.DisplayMember = "Name";
            ComboBoxSizeCodeLocation.ValueMember = "Id";

            ComboBoxVelocityCodeLocation.DataSource = _repoVelocityCode.All();
            ComboBoxVelocityCodeLocation.DisplayMember = "Name";
            ComboBoxVelocityCodeLocation.ValueMember = "Id";

            ComboBoxHeightCodeLocation.DataSource = _repoHeightCode.All();
            ComboBoxHeightCodeLocation.DisplayMember = "Name";
            ComboBoxHeightCodeLocation.ValueMember = "Id";

            ComboBoxLocationCodeLocation.DataSource = _repoLocationCode.All();
            ComboBoxLocationCodeLocation.DisplayMember = "Name";
            ComboBoxLocationCodeLocation.ValueMember = "Id";
        }

        private void SetupLogger()
        {
            string logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = @"HotAction";
            string logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        private void UpdateNomenclature()
        {
            MBHotAccept.Text = _nomenclature.MBPickAccept;
            MBHotAccept.Text = _nomenclature.MBStoreAccept;
            LabelDevice.Text = _nomenclature.LabelDevice;
            LabelTray.Text = _nomenclature.LabelTray;
            LabelOver.Text = _nomenclature.LabelOver;
            LabelBack.Text = _nomenclature.LabelBack;
        }

        //public int IndexOf(BindingSource bs, int value)
        //{
        //    int count = bs.Count;
        //    int itemIndex = -1;
        //    for (int i = 0; i < count; i++)
        //    {
        //        int rec = ((OrderView)bs[i]).Id;
        //        if (rec == value)
        //        {
        //            itemIndex = i;
        //            break;
        //        }
        //    }
        //    return itemIndex;
        //}

        //private int GetRecordCount(BindingSource bs)
        //{
        //    int count = bs.Count;
        //    LabelRecordCount.Text = $"Records: {count.ToString()}";
        //    return count;
        //}

        private void LoadCurrent(ItemDefinitionView item)
        {
            _currentItemDefinition = item;

            var recs = _repoInv.FindInventoryViewsByStation(item.Item, _station.StationId).ToList();
            var blv = new BindingListView<SqlInventoryView>(recs);
            _bindingSourceCurrent.DataSource = blv;
            MBCurrentLocations.Text = $"Current Locations ({_bindingSourceCurrent.Count})";
            if (_bindingSourceCurrent.Count > 0)
            {
                MBHotPick.Enabled = true;
                MBHotStore.Enabled = true;
                MBCurrentLocations.Enabled = true;
            }
            else
            {
                MBHotPick.Enabled = false;
                MBHotStore.Enabled = false;
                MBCurrentLocations.Enabled = false;
            }
        }

        private async Task LoadNewLocations(ItemDefinitionView item)
        {

            if (CheckBoxAll.Checked)
            {
                var views = await Task.Run(() => _locationsRepository.FindLocationViewsByStation(string.Empty, item.StationId));
                var blvAll = new BindingListView<LocationView>(views.ToList());
                _bindingSourceNewLocations.DataSource = blvAll;
            }
            else
            {
                var views = await Task.Run(() => _locationsRepository.GetAllLocationViewsExact(item.StationId,
                     item.SizeCodeId, item.VelocityCodeId, item.HeightCodeId, item.LocationCodeId, inUse: false));
                var blv = new BindingListView<LocationView>(views.ToList());
                _bindingSourceNewLocations.DataSource = blv;
            }

            //var blv = new BindingListView<LocationView>(views.ToList());
            //_bindingSourceNewLocations.DataSource = blv;

            MBNewLocations.Text = $"New Locations ({_bindingSourceNewLocations.Count})";
            if (_bindingSourceNewLocations.Count > 0)
            {
                MBHotStore.Enabled = true;
                MBNewLocations.Enabled = true;
            }
            else
            {
                MBHotPick.Enabled = false;
                MBHotStore.Enabled = false;
                MBNewLocations.Enabled = false;
            }
        }

        public void UpdateDataGrid(BindingSource bindingSource)
        {
            if (DataGridViewHot.InvokeRequired)
            {
                var d = new UpdateDataGridDelegate(UpdateDataGrid);
                this.BeginInvoke(d, new object[] { bindingSource });
            }
            else
            {
                DataGridViewHot.DataSource = bindingSource.DataSource;
                DataGridViewHot.ClearSelection();
            }
        }

        private void LoadItemDefinitions(int recId = 0)
        {
            Cursor.Current = Cursors.WaitCursor;

            //if (_currentGridDataType != GridDataType.Item)
            //{
            SetupGridItemDefinition();
            //}

            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            var idx = 0;
            var findWhat = TextBoxFindItem.Text.ToLower().Trim();
            //var find = _akaRepository.Get(findWhat);
            //TextBoxFindItem.Text = find;
            stopwatch.Restart();
            IEnumerable<ItemDefinitionView> views = new ItemDefinitionView[] { };


            if (_station.StationNumber >= 10)
            {
                var station = _repoStation.FindBy(r => r.StationNumber == 8).FirstOrDefault();
                if (station != null)
                {
                    views = _itemDefinitionsRepository.FindItemDefinitionViewsByStation(findWhat, station.Id);
                    bindingSourceItemDefinitionViewEquin = new BindingListView<ItemDefinitionView>(views.ToList());
                }
            }
            else
            {
                views = _itemDefinitionsRepository.FindItemDefinitionViewsByStation(findWhat, _station.StationId);
                bindingSourceItemDefinitionViewEquin = new BindingListView<ItemDefinitionView>(views.ToList());
            }



            _bindingSourceItemDefinitions.DataSource = bindingSourceItemDefinitionViewEquin;

            DataGridViewHot.DataSource = _bindingSourceItemDefinitions;
            // UpdateDataGrid(_bindingSourceItemDefinitions);

            if (GetRecordCount(_bindingSourceItemDefinitions) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceItemDefinitions, recId);
                }
                DataGridViewHot.FirstDisplayedScrollingRowIndex = DataGridViewHot.Rows[idx].Index;
                DataGridViewHot.Refresh();
                DataGridViewHot.CurrentCell = DataGridViewHot.Rows[idx].Cells[1];
                DataGridViewHot.Rows[idx].Selected = true;
                _currentItemDefinition =
                     ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
            }
            else
            {
                var item = _repoItemDefinition.FindBy(r => r.Item == findWhat).FirstOrDefault();
                if (item != null)
                {
                    MessageBox.Show($"{item.Item} is on Station {item.Station.StationNumber}");
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Item Definition Views Time: {stopwatch.ElapsedMilliseconds.ToString()}");

            DataGridViewHot.ClearSelection();
            Cursor.Current = Cursors.Default;
        }

        public int IndexOf(BindingSource bs, int value)
        {
            var count = bs.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((ObjectView<ItemDefinitionView>)bs.Current).Object.Id;
                if (rec != value) continue;
                itemIndex = i;
                break;
            }
            return itemIndex;
        }

        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"Records: {count.ToString()}";
            return count;
        }

        private void SetupGridItemDefinition()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_currentGridDataType == GridDataType.Item) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.Item;


            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = "Station",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "StationName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = "Item",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitOfIssueName",
                HeaderText = "Unit Of Issue",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "UnitOfIssueName"
            };
            DataGridViewHot.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = "Location Max",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMax"
            };
            DataGridViewHot.Columns.Add(col);



            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = "Size Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = "Velocity Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = "Height Code",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMin",
                HeaderText = "Location Min",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMin"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = "System Max",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMax"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = "System Min",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMin"
            };
            DataGridViewHot.Columns.Add(col);
            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "LocationCodeName",
            //    HeaderText = "Location Code",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "LocationCodeName"
            //};
            //DataGridViewHot.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "StorageTypeName",
            //    HeaderText = "Storage Type",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "StorageTypeName"
            //};
            //DataGridViewHot.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Weight",
            //    HeaderText = "Weight",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Weight"
            //};
            //DataGridViewHot.Columns.Add(col);

            //var ckcol = new DataGridViewCheckBoxColumn
            //{
            //    DataPropertyName = "Scale",
            //    HeaderText = "Scale",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
            //    Name = "Scale"
            //};
            //DataGridViewHot.Columns.Add(ckcol);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "Id",
                Visible = false,
                Name = "Id",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewHot.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }

            stopwatch.Stop();
            Console.WriteLine($"Time to build Item Grid: {stopwatch.ElapsedMilliseconds.ToString()}");
        }

        private void SetupGridNew()
        {
            if (_currentGridDataType == GridDataType.New) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.New;

            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Visible = true,
                Name = "Position",
                Text = "Position",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewHot.Columns.Add(bCol);

            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse"
                ,
                HeaderText = "In Use"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "InUse"
            };
            DataGridViewHot.Columns.Add(xcol);


            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName"
                ,
                HeaderText = "Station"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "StationName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1"
                ,
                HeaderText = _nomenclature.LabelDevice
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Loc1"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2"
                ,
                HeaderText = _nomenclature.LabelTray
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Loc2"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3"
                ,
                HeaderText = _nomenclature.LabelOver
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Loc3"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4"
                ,
                HeaderText = _nomenclature.LabelBack
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Loc4"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5"
                ,
                HeaderText = "Tag"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Loc5"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName"
                ,
                HeaderText = "Size Code"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName"
                ,
                HeaderText = "Velocity Code"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName"
                ,
                HeaderText = "Height Code"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName"
                ,
                HeaderText = "Location Code"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "LocationCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id"
                ,
                HeaderText = "Id"
                ,
                Visible = false
                ,
                Name = "Id"
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot"
                ,
                HeaderText = "Slot"
                ,
                Visible = true
                ,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                ,
                Name = "Slot"
            };
            DataGridViewHot.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }

        private void SetupGridCurrent()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_currentGridDataType == GridDataType.Current) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.Current;

            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewHot.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewHot.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = @"Item",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = @"Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = @"Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = @"Slot",
                Name = "Slot",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = @"Location Max",
                Name = "LocationMax",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = @"Received Date",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            col.DefaultCellStyle.Format = "MM-dd-yyyy hh:mm:ss";
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = @"Storage Type",
                Name = "StorageTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewHot.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }

        private void FrmHotAction_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }

        private void UpdateHotImage(string image)
        {
            Task.Run(() => _logger.Log($"Update Hot Images Start : [{DateTime.Now.ToLongTimeString()}]"));
            if (!string.IsNullOrEmpty(_imagesDirectory))
            {
                try
                {
                    string path = string.Concat(_imagesDirectory, image, str2: @".jpg");
                    if (File.Exists(path))
                    {
                        PictureBoxItemHotImage.Load(path);
                    }
                    else
                    {
                        path = string.Concat(_imagesDirectory, str1: @"Unknown.jpg");
                        if (File.Exists(path))
                        {
                            PictureBoxItemHotImage.Load(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting Hot Image.  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            Task.Run(() => _logger.Log($"Update Hot Images End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void ShowShi(int loc1, int loc2, int loc3, string loc4, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        ClearAllShi();

                        Task.Run(() => _logger.Log($"ShowShi HotAction {loc2} {loc3}"));

                        Task.Run(() => GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text));
                    }
                }
            }

        }

        private void ClearAllShi()
        {
            if (!_neutronVariables.DisplaysEnabled) return;
            if (GlobalVar.Displays == null) return;
            if (!_neutronVariables.ShiEnabled) return;
            Task.Run(() => _logger.Log("ClearAllShi HotAction"));
            GlobalVar.Displays.ClearAllShi();
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            if (_neutronVariables.ShuttleEnabled)
            {
                if (GlobalVar.Shuttle != null)
                {
                    if (moveDevice)
                    {
                        Task.Run(() => _logger.Log($"905 HOT Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}"));

                        var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4));

                        Task.Run(() => _logger.Log($"956 HOT Position Device Tray Response:{response.Result.AsString(EnumFormat.Description)}"));

                        if (response.Result != DeviceResponse.Success)
                        {
                            MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Information"
                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void MBFindItem_Click(object sender, EventArgs e)
        {
            FindItem();
        }

        private void FindItem()
        {
 Cursor.Current = Cursors.WaitCursor;
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            Cursor.Current = Cursors.Default;
        }


        private void FindHotRecord(string findWhat = @"")
        {

            try
            {
                if (!string.IsNullOrEmpty(findWhat))
                {
                    var find = _akaRepository.Get(findWhat);
                    TextBoxFindItem.Text = find;
                }
                else
                {
                    TextBoxFindItem.Text = findWhat;
                }
                LoadItemDefinitions();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hot Find Error: " + ex.Message);
            }
        }

        private void ButtonHotPickClear_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _bindingSourceCurrent.DataSource = null;
            MBCurrentLocations.Text = $"Current Locations ({_bindingSourceCurrent.Count})";

            _bindingSourceNewLocations.DataSource = null;
            MBNewLocations.Text = $"New Locations ({_bindingSourceNewLocations.Count})";

            TextBoxFindItem.Text = string.Empty;
            LoadItemDefinitions();
            TextBoxFindItem.Focus();
            Cursor.Current = Cursors.Default;
        }

        private async void MBHotPick_Click(object sender, EventArgs e)
        {
            _hotPickButtonPressed = true;
            _hotStoreButtonPressed = false;
            await FillCostCenterComboBox();
            CloseButtonPressed = false;
            RadioButtonCostCenter.Checked = true;
            HotAction.BackColor = Color.Red;
            LabelFormTitle.BackColor = Color.Red;
            LabelFormTitle.Text = "Hot Pick";
            ComboBoxCostCenter.Visible = true;
            TextBoxFindCostCenter.Visible = true;
            RadioButtonCostCenter.Visible = true;
            MBHotAccept.Text = _nomenclature.MBPickAccept;
            RadioButtonPick.Text = "Pick";
            _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;

            var loc1 = _currentInventoryView.Loc1;
            var loc2 = _currentInventoryView.Loc2;
            var loc3 = _currentInventoryView.Loc3;
            var loc4 = _currentInventoryView.Loc4;

            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

            ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                , _currentInventoryView.Loc4.ToString(), 1.ToString());

            await UpdateHotPickScreen(_currentInventoryView);
            tabControl1.SelectedTab = HotAction;
        }

        private async void MBHotStore_Click(object sender, EventArgs e)
        {
            _hotPickButtonPressed = false;
            _hotStoreButtonPressed = true;
            CloseButtonPressed = false;
            RadioButtonPick.Checked = true;
            HotAction.BackColor = Color.Green;
            LabelFormTitle.BackColor = Color.Green;
            LabelFormTitle.Text = "Hot Store";
            ComboBoxCostCenter.Visible = false;
            TextBoxFindCostCenter.Visible = false;
            RadioButtonCostCenter.Visible = false;
            MBHotAccept.Text = _nomenclature.MBStoreAccept;
            RadioButtonPick.Text = "Store";
            if (_currentGridDataType == GridDataType.Current)
            {
                _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;
                var loc1 = _currentInventoryView.Loc1;
                var loc2 = _currentInventoryView.Loc2;
                var loc3 = _currentInventoryView.Loc3;
                var loc4 = _currentInventoryView.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

                ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                    , _currentInventoryView.Loc4.ToString(), 1.ToString());

                await UpdateHotPickScreen(_currentInventoryView);
            }
            else
            {
                var location = ((ObjectView<LocationView>)_bindingSourceNewLocations.Current).Object;
                var itemDef = ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                _currentInventoryView = new SqlInventoryView
                {
                    ItemDefinitionId = itemDef.Id,
                    Item = itemDef.Item,
                    Description = itemDef.Description,
                    HeightCodeId = itemDef.HeightCodeId,
                    HeightCodeName = itemDef.HeightCodeName,
                    LocationCodeId = itemDef.LocationCodeId,
                    LocationCodeName = itemDef.LocationCodeName,
                    ReceivedDate = DateTime.Now,
                    SizeCodeId = itemDef.SizeCodeId,
                    SizeCodeName = itemDef.SizeCodeName,
                    VelocityCodeId = itemDef.VelocityCodeId,
                    VelocityCodeName = itemDef.VelocityCodeName,
                    StorageTypeId = itemDef.StorageTypeId,
                    StorageTypeName = itemDef.StorageTypeName,
                    StationId = itemDef.StationId,
                    Quantity = 0,
                    LocationId = location.Id,
                    Loc1 = location.Loc1,
                    Loc2 = location.Loc2,
                    Loc3 = location.Loc3,
                    Loc4 = location.Loc4,
                    Loc5 = location.Loc5,
                    InUse = location.InUse,
                    UnitOfIssueId = itemDef.UnitOfIssueId,
                    UnitOfIssueName = itemDef.UnitOfIssueName
                };

                var loc1 = _currentInventoryView.Loc1;
                var loc2 = _currentInventoryView.Loc2;
                var loc3 = _currentInventoryView.Loc3;
                var loc4 = _currentInventoryView.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

                ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                    , _currentInventoryView.Loc4.ToString(), 1.ToString());

                await UpdateHotPickScreen(_currentInventoryView);
            }

            //int loc1 = currentItem.Loc1;
            //int loc2 = currentItem.Loc2;

            //PositionDevice(loc1, loc2, moveDevice: true);

            //ShowShi(currentItem.Loc1, currentItem.Loc2, currentItem.Loc3
            //   , currentItem.Loc4.ToString(), 1.ToString());

            //UpdateHotPickScreen(currentItem);
            tabControl1.SelectedTab = HotAction;
        }

        private async Task UpdateHotPickScreen(SqlInventoryView invItem)
        {
            try
            {
                using (var db = new NeutronDb())
                {
                    var itemDefinition = await db.ItemDefinitions.FindAsync(invItem.ItemDefinitionId);
                    if (itemDefinition == null) throw new ArgumentNullException(nameof(itemDefinition));
                    var location = await db.Locations.FindAsync(invItem.LocationId);
                    if (location == null) throw new ArgumentNullException(nameof(location));
                    LabelHotPickDescription.Text = itemDefinition.Description;
                    LabelHotPickItem.Text = itemDefinition.Item;
                    LabelHotPickUOI.Text = itemDefinition.UnitOfIssue.Name;
                    TextBoxHotPickLoc1.Text = location.Loc1.ToString();
                    TextBoxHotPickLoc2.Text = location.Loc2.ToString();
                    TextBoxHotPickLoc3.Text = location.Loc3.ToString();
                    TextBoxHotPickLoc4.Text = location.Loc4.ToString();
                    TextBoxHotPickLoc5.Text = location.Loc5.ToString();

                    ComboBoxSizeCodeItem.SelectedIndex = ComboBoxSizeCodeItem.FindStringExact(itemDefinition.SizeCode.Name);
                    ComboBoxVelocityCodeItem.SelectedIndex = ComboBoxVelocityCodeItem.FindStringExact(itemDefinition.VelocityCode.Name);
                    ComboBoxHeightCodeItem.SelectedIndex = ComboBoxHeightCodeItem.FindStringExact(itemDefinition.HeightCode.Name);
                    ComboBoxLocationCodeItem.SelectedIndex = ComboBoxLocationCodeItem.FindStringExact(itemDefinition.LocationCode.Name);

                    ComboBoxSizeCodeLocation.SelectedIndex = ComboBoxSizeCodeLocation.FindStringExact(location.SizeCode.Name);
                    ComboBoxVelocityCodeLocation.SelectedIndex = ComboBoxVelocityCodeLocation.FindStringExact(location.VelocityCode.Name);
                    ComboBoxHeightCodeLocation.SelectedIndex = ComboBoxHeightCodeLocation.FindStringExact(location.HeightCode.Name);
                    ComboBoxLocationCodeLocation.SelectedIndex = ComboBoxLocationCodeLocation.FindStringExact(location.LocationCode.Name);

                    TextBoxHotPickQuantity.Text = string.Empty;
                    TextBoxHotPickQuantity.Focus();
                    TextBoxHotPickLocationQuantity.Text = invItem.Quantity.ToString();
                    TextBoxHotPickReceivedDate.Text = string.IsNullOrEmpty(invItem.ReceivedDate.ToString("d"))
                        ? ""
                        : invItem.ReceivedDate.ToShortDateString();
                    LabelPrimeBin.Visible = invItem.PrimeBin;
                    LabelStaticRelease.Text = invItem.StorageTypeName;

                    UpdateHotImage(invItem.Item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void RadioButtonHotAction(object sender, EventArgs e)
        {
            if (RadioButtonPick.Checked)
            {
                MBHotAccept.Text = RadioButtonPick.Text;
            }
            else if (RadioButtonWarranty.Checked)
            {
                MBHotAccept.Text = RadioButtonWarranty.Text;
            }
            else if (RadioButtonScrap.Checked)
            {
                MBHotAccept.Text = RadioButtonScrap.Text;
            }
            else if (RadioButtonOther.Checked)
            {
                MBHotAccept.Text = RadioButtonOther.Text;
            }
            else if (RadioButtonCostCenter.Checked)
            {
                MBHotAccept.Text = RadioButtonCostCenter.Text;
            }
        }

        private async void DataGridViewHot_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Cursor.Current = Cursors.WaitCursor;
            switch (_currentGridDataType)
            {
                case GridDataType.Item:
                    DataGridViewHot.Columns.Clear();
                    await LoadCurrentAndNew();
                    break;
                case GridDataType.New:
                    break;
                case GridDataType.Current:
                    break;
            }
            Cursor.Current = Cursors.Default;
        }

        private async Task LoadCurrentAndNew()
        {
            if (_currentGridDataType == GridDataType.Item)
            {
                if (_bindingSourceItemDefinitions.Count > 0)
                {
                    var s = ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    TextBoxFindItem.Text = s.Item;
                    stopwatch.Stop();
                    Console.WriteLine($"Speed: LoadCurrentAndNew: {stopwatch.ElapsedMilliseconds.ToString()}");
                    stopwatch.Restart();
                    // LoadItemDefinitions();
                    stopwatch.Stop();
                    Console.WriteLine($"Speed: LoadItemDefinitions: {stopwatch.ElapsedMilliseconds.ToString()}");
                    stopwatch.Restart();
                    LoadCurrent(s);
                    //LoadCurrent(_currentItemDefinition);
                    stopwatch.Stop();
                    Console.WriteLine($"Speed: LoadCurrent: {stopwatch.ElapsedMilliseconds.ToString()}");
                    stopwatch.Restart();
                    // await LoadNewLocations(_currentItemDefinition);
                    await LoadNewLocations(s);
                    stopwatch.Stop();
                    Console.WriteLine($"Speed: await LoadNewLocations: {stopwatch.ElapsedMilliseconds.ToString()}");
                    stopwatch.Restart();
                    if (_bindingSourceCurrent.Count > 0)
                    {
                        SetupGridCurrent();
                        stopwatch.Stop();
                        Console.WriteLine($"Speed: BindingSource Current Setup Grid: {stopwatch.ElapsedMilliseconds.ToString()}");
                        stopwatch.Restart();
                        DataGridViewHot.DataSource = _bindingSourceCurrent;
                        stopwatch.Stop();
                        Console.WriteLine($"Speed: BindingSource Current DataSource: {stopwatch.ElapsedMilliseconds.ToString()}");
                        stopwatch.Restart();
                        DataGridViewHot.ClearSelection();
                        stopwatch.Stop();
                        Console.WriteLine($"Speed: BindingSource Current Clear Selection: {stopwatch.ElapsedMilliseconds.ToString()}");
                    }
                    else if (_bindingSourceNewLocations.Count > 0)
                    {
                        SetupGridNew();
                        DataGridViewHot.DataSource = _bindingSourceNewLocations;
                        DataGridViewHot.ClearSelection();
                        stopwatch.Stop();
                        Console.WriteLine($"Speed: BindingSource New Locations: {stopwatch.ElapsedMilliseconds.ToString()}");
                    }
                }
            }
        }

        private void MBHotActionBack_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = false;
            ClearAllShi();
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            tabControl1.SelectedTab = HotPick;
        }

        private async void MBHotAccept_Click(object sender, EventArgs e)
        {
            await Accept();
        }

        private async Task Accept()
        {
            Cursor.Current = Cursors.WaitCursor;
            Inventory inv = null;
            ClearAllShi();
            var actionCode = GetHotActionCode();
            var pickQty = (TextBoxHotPickQuantity.Text).ParseInt();

            if (_currentInventoryView.Id == 0)  //New Inventory Record can't be for a HOT PICK
            {
                try
                {
                    var inventory = new Inventory
                    {
                        ItemDefinitionId = _currentInventoryView.ItemDefinitionId,
                        LocationId = _currentInventoryView.LocationId,
                        Quantity = pickQty,
                        StorageTypeId = _currentInventoryView.StorageTypeId,
                        ReceivedDate = DateTime.Now,
                        PrimeBin = _currentInventoryView.PrimeBin,
                        StationId = _currentInventoryView.StationId,
                    };
                    _repoInventory.Insert(inventory);
                    _locationsRepository.SetLocationInUse(inventory.LocationId, true);

                    GlobalVar.HistoryManager.SaveHistory(actionCode, inventory, pickQty);

                    inv = _repoInventory.FindByKey(inventory.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to create Inventory Item.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else  // Existing Inventory Record
            {
                try
                {
                    inv = _repoInventory.FindByKey(_currentInventoryView.Id);
                    if (inv != null)
                    {
                        if (_hotPickButtonPressed)
                        {
                            inv.Quantity -= pickQty;
                        }
                        else if (_hotStoreButtonPressed)
                        {
                            inv.Quantity += pickQty;
                        }

                        TextBoxHotPickLocationQuantity.Text = inv.Quantity.ToString();
                        _repoInventory.Update(inv);

                        if (RadioButtonCostCenter.Checked)
                        {
                            GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty, (string)ComboBoxCostCenter.SelectedValue);
                            //Mediator.GetInstance().OnBatchComplete(this);
                        }
                        else
                        {
                            GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty);
                        }

                        await Task.Run(() => _inventoryManager.ReleaseCheck(inv));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to update the quantity of Inventory Item.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }

            }

            if (inv != null) TextBoxFindItem.Text = inv.ItemDefinition.Item;

            LoadItemDefinitions();
            await LoadCurrentAndNew();
            LabelFormTitle.Text = "Hot Search";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = HotPick;

        }

        private ActionCode GetHotActionCode()
        {
            ActionCode result = ActionCode.PickHot;
            var radioButtons = new List<RadioButton> { RadioButtonPick, RadioButtonWarranty, RadioButtonScrap, RadioButtonOther, RadioButtonCostCenter };

            if (RadioButtonPick.Text == "Pick")
            {
                foreach (RadioButton item in radioButtons)
                {
                    if (item.Checked)
                    {
                        switch (item.Text)
                        {
                            case "Pick":
                                result = ActionCode.PickHot;
                                break;
                            case "Warranty":
                                result = ActionCode.WarrantyHotPick;
                                break;
                            case "Scrap":
                                result = ActionCode.ScrapHotPick;
                                break;
                            case "Other":
                                result = ActionCode.OtherHotPick;
                                break;
                            case "Cost Center":
                                result = ActionCode.CostCenterHotPick;
                                break;
                            default:
                                result = ActionCode.PickHot;
                                break;
                        }
                    }
                }
            }
            else if (RadioButtonPick.Text == "Store")
            {
                foreach (RadioButton item in radioButtons)
                {
                    if (item.Checked)
                    {
                        switch (item.Text)
                        {
                            case "Store":
                                result = ActionCode.StoreHot;
                                break;
                            case "Warranty":
                                result = ActionCode.WarrantyHotStore;
                                break;
                            case "Scrap":
                                result = ActionCode.ScrapHotStore;
                                break;
                            case "Other":
                                result = ActionCode.OtherHotStore;
                                break;
                            case "Cost Center":
                                result = ActionCode.CostCenterHotStore;
                                break;
                            default:
                                result = ActionCode.StoreHot;
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private void MBHotActionClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        private void PictureBoxItemHotImage_MouseEnter(object sender, EventArgs e)
        {
            if (_neutronVariables.AutoEnlargeImage)
            {
                PictureBoxItemHotImage.Location = new Point(318, 117);
                PictureBoxItemHotImage.Size = new Size(512, 512);
                PictureBoxItemHotImage.BringToFront();
            }
        }

        private void PictureBoxItemHotImage_MouseLeave(object sender, EventArgs e)
        {
            if (_neutronVariables.AutoEnlargeImage)
            {
                PictureBoxItemHotImage.Location = new Point(446, 373);
                PictureBoxItemHotImage.Size = new Size(256, 256);
                PictureBoxItemHotImage.BringToFront();
            }
        }

        private async void FrmHotAction_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                if (TextBoxFindItem.Focused)
                {
                    FindItem();
                }
                else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && MBHotAccept.Focused)
                {
                    await Accept();
                }
                else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && TextBoxHotPickQuantity.Focused)
                {
                    MBHotAccept.Focus();
                }

            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindItem.Text = "";
                TextBoxFindItem.Focus();
            }

            if (e.KeyCode == Keys.F12)
            {
                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _nomenclature))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MBCurrentLocations_Click(object sender, EventArgs e)
        {
            MBHotPick.Enabled = true;
            MBHotStore.Enabled = true;
            SetupGridCurrent();
            DataGridViewHot.DataSource = _bindingSourceCurrent;
        }

        private void MBNewLocations_Click(object sender, EventArgs e)
        {
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = true;
            SetupGridNew();
            DataGridViewHot.DataSource = _bindingSourceNewLocations;
        }

        private async void LabelHotPickItem_Click(object sender, EventArgs e)
        {
            await EditItemDefinition(_currentInventoryView.ItemDefinitionId);
        }


        private async void ButtonEditItemDefinition_Click(object sender, EventArgs e)
        {
            await EditItemDefinition(_currentInventoryView.ItemDefinitionId);
        }


        private async Task EditItemDefinition(int id)
        {
            Hide();
            using (var frm = new FrmEditItemDefinition(id))
            {
                var result = frm.ShowDialog();
                Show();
            }
            await UpdateHotPickScreen(_currentInventoryView);
        }


        private void ButtonEditLocationDefinition_Click(object sender, EventArgs e)
        {
            EditLocationDefinition(_currentInventoryView.LocationId);
        }

        private async void EditLocationDefinition(int id)
        {
            Hide();
            using (var frm = new FrmEditLocationDefinition(id))
            {
                var result = frm.ShowDialog();
                Show();
            }
            await UpdateHotPickScreen(_currentInventoryView);
        }

        private async void ComboBoxSizeCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.ItemDefinitions.FindAsync(_currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                rec.SizeCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxVelocityCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.ItemDefinitions.FindAsync(_currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                rec.VelocityCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxHeightCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.ItemDefinitions.FindAsync(_currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                rec.HeightCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxLocationCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.ItemDefinitions.FindAsync(_currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                rec.LocationCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxSizeCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.Locations.FindAsync(_currentInventoryView.LocationId);
                if (rec == null) return;
                rec.SizeCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxVelocityCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.Locations.FindAsync(_currentInventoryView.LocationId);
                if (rec == null) return;
                rec.VelocityCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxHeightCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.Locations.FindAsync(_currentInventoryView.LocationId);
                if (rec == null) return;
                rec.HeightCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxLocationCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = await db.Locations.FindAsync(_currentInventoryView.LocationId);
                if (rec == null) return;
                rec.LocationCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                await UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private void TextBoxHotPickQuantity_TextChanged(object sender, EventArgs e)
        {

        }

        private void FrmHotAction_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            _formLoading = false;
        }

        private async void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            await LoadNewLocations(_currentItemDefinition);
        }

        private void ButtonHotActionCount_Click(object sender, EventArgs e)
        {
            var inventoryId = _currentInventoryView.Id;
            var qty = OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                TextBoxHotPickLocationQuantity.Text = qty.ToString();
            }
        }

        private int OpenLocationCountForm(int inventoryId)
        {
            var qty = 0;
            using (var form = new FrmLocationCount())
            {

                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    qty = (form.NewQty).ParseInt();
                    LocationCount(inventoryId, qty);
                }
            }
            return qty;
        }

        private void LocationCount(int inventoryId, int qty)
        {
            var inv = _repoInventory.FindByKey(inventoryId);
            if (inv == null) return;
            var prevQty = inv.Quantity;
            inv.Quantity = qty;
            _repoInventory.Update(inv);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryModify, inv);

            var cnt = new LocationCount()
            {
                InventoryId = inv.Id,
                ItemDefinitionId = inv.ItemDefinitionId,
                LocationId = inv.LocationId,
                UserId = GlobalVar.User.Id,
                PreviousQty = prevQty,
                NewQty = qty,
                CountDate = DateTime.Now,
            };
            _repoLocationCount.Insert(cnt);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.LocationCount, cnt);
        }

        private void MBHotActionCount_Click(object sender, EventArgs e)
        {
            var inventoryId = _currentInventoryView.Id;
            var qty = OpenLocationCountForm(inventoryId);

            if (qty < 0) return;
            TextBoxHotPickLocationQuantity.Text = qty.ToString();
        }

        private void ComboBoxCostCenter_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void TextBoxHotPickQuantity_Leave(object sender, EventArgs e)
        {
            var tb = TextBoxHotPickQuantity.Text;
            ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3, _currentInventoryView.Loc4.ToString(), tb);
        }

        private void TextBoxFindCostCenter_Leave(object sender, EventArgs e)
        {
            ////file CostCenter Combo Box
            var search = TextBoxFindCostCenter.Text;
            var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
            ComboBoxCostCenter.DroppedDown = true;
        }

        private void ComboBoxCostCenter_TextChanged(object sender, EventArgs e)
        {
            MBHotAccept.Enabled = false;
            try
            {
                var t = ((ComboBox)sender).SelectedItem;

                if (t != null)
                {
                    var cost = (CostCenter)t;
                    var n = cost.Name;
                    if (!string.IsNullOrEmpty(n))
                    {
                        MBHotAccept.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error reading Cost Center Text Changed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
                MBHotAccept.Enabled = true;
            }
        }

        private void TextBoxFindCostCenter_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void TextBoxFindCostCenter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                //file CostCenter Combo Box
                var search = TextBoxFindCostCenter.Text;
                var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
                ComboBoxCostCenter.DataSource = costCenterList;
                ComboBoxCostCenter.DisplayMember = "Name";
                ComboBoxCostCenter.ValueMember = "Code";
                ComboBoxCostCenter.DroppedDown = true;
            }
        }

        private void HotAction_Enter(object sender, EventArgs e)
        {
            TextBoxFindCostCenter.Focus();
        }
    }
}
