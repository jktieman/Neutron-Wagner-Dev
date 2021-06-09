using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using CurrentDeviceIndicator;
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
using NeutronData.Migrations;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using NeutronDllu;
using DeviceType = NeutronCore.Enums.DeviceType;
using StorageType = Neutron.Enums.StorageType;
using StationType = NeutronCore.Enums;

namespace Neutron.Forms
{
    public partial class FrmHotAction : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _enumResourceManager;
        private ResourceManager _gridResourceManager;

        private readonly AkaRepository _repoAka = new AkaRepository();
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly GenericRepository<VelocityCode> _repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        private readonly GenericRepository<HeightCode> _repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());
        private readonly GenericRepository<LocationCode> _repoLocationCode = new GenericRepository<LocationCode>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails =
            new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());
        private readonly GenericRepository<Station> _repoStation = new GenericRepository<Station>(new NeutronDb());
        //private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private readonly StationRepository _stationRepository = new StationRepository();
        private LocationsRepository _locationsRepository;
        private readonly InventoryRepository _repoInv = new InventoryRepository();
        private readonly ItemDefinitionsRepository _itemDefinitionsRepository = new ItemDefinitionsRepository();
        private readonly BindingSource _bindingSourceCurrent = new BindingSource();
        private BindingSource _bindingSourceItemDefinitions = new BindingSource();
        private readonly BindingSource _bindingSourceNewLocations = new BindingSource();
        public bool CloseButtonPressed { get; set; }

        private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        readonly NeutronVariables _neutronVariables;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;

        private DynamicLogger _logger;
        readonly StationView _station;
        string _imagesDirectory;
        readonly IAkaRepository _akaRepository;
        private readonly IJsonData _jsonData;
        private CostCenterManager _costCenterManager;
        private ItemDefinitionView _currentItemDefinition;
        private GridDataType _currentGridDataType = GridDataType.None;
        private bool _formLoading = true;
        private bool _hotPickButtonPressed = false;
        private bool _hotStoreButtonPressed = false;
        private bool _useCostCenter = false;
        private InventoryManager _inventoryManager;
        private string _newLocationButtonText = "New Locations";
        private Dictionary<int, DeviceIndicator> _deviceIndicators;
        private readonly int[] _moveableDeviceTypes;
        private readonly Station _rackStation;
        private string _item;
        private int _initialQuantity;
        private int _quantityToPick;
        private readonly PickList _pickList;
        private LabelPrinterPreferences _labelPrinter;

        public enum GridDataType
        {
            None,
            Item,
            Current,
            New
        }
        public delegate void UpdateDataGridDelegate(BindingSource bindingSource);

        public FrmHotAction(StationView station, IJsonData jsonData
            , IAkaRepository akaRepository, NeutronVariables neutronVariables
            , ILacProcessor lacProcessor, IImageManager imageManager, string item = @"", int quantity = 1, PickList pickList = null)
        {
            InitializeComponent();

            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            _station = station;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _akaRepository = akaRepository;
            SetupLogger();
            Task.Run(() => _logger.Log("HotAction Constructor Start"));
            _rackStation = _stationRepository.GetRackStation();
            _moveableDeviceTypes = _stationRepository.GetMoveableDeviceTypeIds();
            _pickList = pickList;
            _item = item;
            _initialQuantity = quantity;
            _quantityToPick = quantity;
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
            InitForm();
            Task.Run(() => _logger.Log("HotAction Constructor Complete"));
        }
        private void InitForm()
        {
            if (_pickList == null)
            {
                KeyPreview = true;

                SetupGridItemDefinition();
                InitDeviceIndicators();
                _useCostCenter = _neutronVariables.UseCostCenter;
                LabelFormTitle.Text = _resourceManager.GetString("HotActions");
                LabelFormTitle.BackColor = Color.Red;
                HideTabControlTabs();
                mlUserInfo.Text = GlobalVar.User?.UserInfo;
                CloseButtonPressed = false;
                _imagesDirectory = LoaderSettings.GetImagesDirectory();
                _locationsRepository = new LocationsRepository();
                FillComboBoxes();
                _inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
                InitialSearch(_item);
                LabelStationName.Text = _station.Name;
                LabelStationName2.Text = _station.Name;
                if (_station.StationType.Id == (int)NeutronCore.Enums.StationType.Carousel
                    || _station.StationType.Id == (int)NeutronCore.Enums.StationType.Vertical) return;

                _newLocationButtonText = _resourceManager.GetString("AllLocations");
                MBNewLocations.Text = _newLocationButtonText;
            }
            else
            {
                _item = _pickList.Item;
                _initialQuantity = _pickList.Ordered.ParseInt();
                _quantityToPick = _pickList.Ordered.ParseInt();
                KeyPreview = true;
                SetupLogger();
                SetupGridItemDefinition();
                //InitDeviceIndicators();
                _useCostCenter = _neutronVariables.UseCostCenter;
                LabelFormTitle.Text = _resourceManager.GetString("HotActions");
                LabelFormTitle.BackColor = Color.Green;
                HideTabControlTabs();
                mlUserInfo.Text = GlobalVar.User?.UserInfo;
                CloseButtonPressed = false;
                _imagesDirectory = LoaderSettings.GetImagesDirectory();
                _locationsRepository = new LocationsRepository();
                FillComboBoxes();
                _inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);
                InitialSearch(_item);
                LabelStationName.Text = _station.Name;
                LabelStationName2.Text = _station.Name;
                if (_station.StationType.Id == (int)NeutronCore.Enums.StationType.Carousel
                    || _station.StationType.Id == (int)NeutronCore.Enums.StationType.Vertical) return;

                _newLocationButtonText = _resourceManager.GetString("AllLocations");
                MBNewLocations.Text = _newLocationButtonText;
                MBHotPick.Visible = false;
                TextBoxFindItem.ReadOnly = true;
                ButtonClearFindItem.Visible = false;
                MBFindItem.Visible = false;
                MBHotActionCount.Visible = false;
            }
        }

        private void InitDeviceIndicators()
        {
            if (HotAction.Controls.ContainsKey("PanelDeviceIndicators")) return;

            Console.WriteLine("Initialize Device Indicators - InitDeviceIndicators");
            _deviceIndicators = new Dictionary<int, DeviceIndicator>();

            var hardwareDevices = _station.HardwareDevices.Where(x => _moveableDeviceTypes.Contains(x.DeviceTypeId)).ToList();
            var numDevices = hardwareDevices.Count;
            var panel = new Panel();
            panel.Location = new Point(140, 0);
            panel.Size = new Size(860, 150);
            panel.BackColor = Color.Transparent;
            panel.Name = "PanelDeviceIndicators";
            var flashRate = _neutronVariables.DeviceFlashRate;
            foreach (var hardwareDevice in hardwareDevices)
            {
                var device = new DeviceIndicator(hardwareDevice.DeviceNumber, flashRate, Color.Yellow
                    , Color.Transparent);
                device.Name = $"DeviceIndicator{hardwareDevice.DeviceNumber}";
                device.DeviceNumber = hardwareDevice.DeviceNumber;
                device.Location = GetLocation(panel.Size.Width, numDevices, hardwareDevice.DeviceNumber);
                _deviceIndicators.Add(hardwareDevice.DeviceNumber, device);
                panel.Controls.Add(device);
            }
            HotAction.Controls.Add(panel);
        }
        private Point GetLocation(int sizeWidth, int numDevices, int deviceNumber)
        {
            Point point;
            var eachBlock = sizeWidth / numDevices;
            var centerBlock = eachBlock / 2;
            var positionInBlock = centerBlock - 60;
            if (deviceNumber == 1)
            {
                point = new Point(positionInBlock, 5);
            }
            else
            {
                var pos = positionInBlock + (deviceNumber - 1) * eachBlock;
                point = new Point(pos, 5);
            }
            return point;

        }

        private void InitialSearch(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
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
            if (!_useCostCenter) return;
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
            //Tray
            ComboBoxSizeCodeLocationTray.DataSource = _repoSizeCode.All();
            ComboBoxSizeCodeLocationTray.DisplayMember = "Name";
            ComboBoxSizeCodeLocationTray.ValueMember = "Id";
            ComboBoxVelocityCodeLocationTray.DataSource = _repoVelocityCode.All();
            ComboBoxVelocityCodeLocationTray.DisplayMember = "Name";
            ComboBoxVelocityCodeLocationTray.ValueMember = "Id";
            ComboBoxHeightCodeLocationTray.DataSource = _repoHeightCode.All();
            ComboBoxHeightCodeLocationTray.DisplayMember = "Name";
            ComboBoxHeightCodeLocationTray.ValueMember = "Id";
            ComboBoxLocationCodeLocationTray.DataSource = _repoLocationCode.All();
            ComboBoxLocationCodeLocationTray.DisplayMember = "Name";
            ComboBoxLocationCodeLocationTray.ValueMember = "Id";
        }
        private void SetupLogger()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            const string folderName = @"HotAction";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }
        private void LoadCurrent(ItemDefinitionView item)
        {
            Task.Run(() => _logger.Log($"Load Current By Item: {item.Item}  START"));
            _currentItemDefinition = item;
            var recs = _repoInv.GetAllInventoryViewsByItemDefinitionId(item.Id).ToList();
            var blv = new BindingListView<SqlInventoryView>(recs);
            _bindingSourceCurrent.DataSource = blv;
            var recordCount = GetRecordCount(recs);
            MBCurrentLocations.Text = $"{_resourceManager.GetString("CurrentLocations")} ({_bindingSourceCurrent.Count})";
            //if (_bindingSourceCurrent.Count > 0)
            //{
            //    MBHotPick.Enabled = true;
            //    MBHotStore.Enabled = true;
            //    MBCurrentLocations.Enabled = true;
            //}
            //else
            //{
            //    MBHotPick.Enabled = false;
            //    MBHotStore.Enabled = false;
            //    MBCurrentLocations.Enabled = false;
            //}
            Task.Run(() => _logger.Log($"Load Current By Item: {item.Item}  END"));
        }


        private void PrintLabel()
        {
            var upc = _repoAka.GetUpc(LabelHotPickItem.Text);

            var labelDetail = new LabelDetail()
            {
                Item = LabelHotPickItem.Text,
                Description = LabelHotPickDescription.Text,
                Quantity = Convert.ToInt32(TextBoxHotPickQuantity.Text),
                EmpId = GlobalVar.User.EmpId,
                Invoice = $"          ",
                Order = $"          ",
                LoadDate = DateTime.Now,
                Origin = $"  "
            };

            ToteToPrint.Print(1, 1, labelDetail, upc, _labelPrinter);
        }

        private async Task LoadNewLocations(ItemDefinitionView item)
        {
            Task.Run(() => _logger.Log($"Load New Locations By Item: {item.Item}  START"));
            var station = _repoStation.FindByKey(_station.StationId);
            if (_station.StationType.Id == (int)StationType.StationType.Supervisor)
            {

                if (_rackStation != null)
                {
                    station = _rackStation;
                    CheckBoxAll.Checked = true;
                    CheckBoxAll.Visible = false;
                }
            }
            else
            {
                CheckBoxAll.Visible = true;
            }
            if (CheckBoxAll.Checked)
            {
                var views = await Task.Run(() => _locationsRepository.FindLocationViewsByStation(station));
                var locationViews = views.ToList();
                var blvAll = new BindingListView<LocationView>(locationViews.ToList());
                _bindingSourceNewLocations.DataSource = blvAll;
                var recordCount = GetRecordCount(locationViews.ToList());
            }
            else
            {
                var views = await Task.Run(() => _locationsRepository.GetAllLocationViewsExact(station,
                     item.SizeCodeId, item.VelocityCodeId, item.HeightCodeId, item.LocationCodeId, inUse: false));
                var locationViews = views.ToList();
                var blv = new BindingListView<LocationView>(locationViews.ToList());
                _bindingSourceNewLocations.DataSource = blv;
                var recordCount = GetRecordCount(locationViews.ToList());
            }
            //var blv = new BindingListView<LocationView>(views.ToList());
            //_bindingSourceNewLocations.DataSource = blv;
            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            //if (_bindingSourceNewLocations.Count > 0)
            //{
            //    MBHotStore.Enabled = true;
            //    MBNewLocations.Enabled = true;
            //}
            //else
            //{
            //    MBHotPick.Enabled = false;
            //    MBHotStore.Enabled = false;
            //    MBNewLocations.Enabled = false;
            //}
            Task.Run(() => _logger.Log($"Load New Locations By Item: {item.Item}  END"));
        }

        private void LoadNewLocationsBySlot(string slot)
        {
            Task.Run(() => _logger.Log($"Load New Locations By Slot: {slot}  START"));
            IEnumerable<LocationView> views = null;  // = new List<LocationView>();
            var station = _repoStation.FindByKey(_station.StationId);
            if (_station.StationType.Id == (int)StationType.StationType.Supervisor)
            {

                if (_rackStation != null)
                {
                    station = _rackStation;
                    CheckBoxAll.Checked = true;
                    CheckBoxAll.Visible = false;
                }
            }
            else
            {
                CheckBoxAll.Visible = true;
            }
            if (CheckBoxAll.Checked)
            {

                views = _locationsRepository.FindLocationViewsByStationAndSlot(station, slot).ToList();
            }
            else
            {
                views = _locationsRepository.FindLocationViewsByStationAndSlot(station, slot).ToList();
                views = views.Where(r => r.InUse == false);
            }
            var locationViews = views.ToList();
            var blvAll = new BindingListView<LocationView>(locationViews.ToList());
            _bindingSourceNewLocations.DataSource = blvAll;
            var recordCount = GetRecordCount(locationViews.ToList());

            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            Task.Run(() => _logger.Log($"Load New Locations By Slot: {slot}  END"));
        }




        // Not Used
        public void UpdateDataGrid(BindingSource bindingSource)
        {
            if (DataGridViewHot.InvokeRequired)
            {
                var d = new UpdateDataGridDelegate(UpdateDataGrid);
                BeginInvoke(d, new object[] { bindingSource });
            }
            else
            {
                DataGridViewHot.DataSource = bindingSource.DataSource;
                DataGridViewHot.ClearSelection();
                var recordCount = GetRecordCount(bindingSource);
            }
        }

        private async Task LoadLocationData(string slot = @"")
        {
            var stationId = _station.StationType.Id == (int)StationType.StationType.Supervisor
                ? _rackStation.Id
                : _station.StationId;

            var inventory = _repoInventory.FindBy(r => r.Location.Slot.Contains(slot) && r.StationId == stationId).ToList();
            if (inventory.Any())
            {
                await LoadItemDefinitions(inventory.First().ItemDefinition.Item);
            }
        }

        private async Task LoadItemDefinitions(string find = @"", int recId = 0)
        {
            Task.Run(() => _logger.Log($"Load Item Definitions Find: {find}  START"));
            BindingListView<ItemDefinitionView> blv = null;
            Cursor.Current = Cursors.WaitCursor;
            SetupGridItemDefinition();
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            var idx = 0;
            var findWhat = string.IsNullOrEmpty(find) ? TextBoxFindItem.Text.ToLower().Trim() : find;

            IEnumerable<ItemDefinitionView> views;
            // if its a Supervisor station, load the Rack items
            if (_station.StationType.Id == (int)StationType.StationType.Supervisor)
            {
                if (_rackStation != null)
                {
                    views = _itemDefinitionsRepository.FindItemDefinitionViewsByStation(findWhat, _rackStation.Id);
                    blv = new BindingListView<ItemDefinitionView>(views.ToList());
                }
            }
            else
            {
                views = _itemDefinitionsRepository.FindItemDefinitionViewsByStation(findWhat, _station.StationId);
                blv = new BindingListView<ItemDefinitionView>(views.ToList());
            }

            _bindingSourceItemDefinitions = new BindingSource { DataSource = blv };


            DataGridViewHot.DataSource = _bindingSourceItemDefinitions;
            DataGridViewHot.Update();
            //UpdateDataGrid(_bindingSourceItemDefinitions);
            var recordCount = GetRecordCount(_bindingSourceItemDefinitions);
            if (recordCount > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceItemDefinitions, recId);
                }
                try
                {
                    DataGridViewHot.FirstDisplayedScrollingRowIndex = idx;
                    DataGridViewHot.Update();
                    DataGridViewHot.CurrentCell = DataGridViewHot.Rows[idx].Cells[1];
                    DataGridViewHot.Rows[idx].Selected = true;

                    _currentItemDefinition =
                         ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    if (recordCount == 1) await LoadCurrentAndNew();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}");
                    _currentGridDataType = GridDataType.None;
                    SetupGridItemDefinition();
                }
            }
            else
            {
                var item = _repoItemDefinition.FindBy(r => r.Item == findWhat).FirstOrDefault();
                if (item != null)
                {
                    MessageBox.Show($"{item.Item} {_resourceManager.GetString("Message0")} {item.Station.StationNumber}");
                }
                ClearCurrentAndNew();
            }
            DataGridViewHot.ClearSelection();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log($"Load Item Definitions Find: {find}  END"));
        }
        private void ClearCurrentAndNew()
        {
            //_bindingSourceCurrent.  .Clear();
            MBCurrentLocations.Text = $"{_resourceManager.GetString("CurrentLocations")} (0)";
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            MBCurrentLocations.Enabled = false;
            // _bindingSourceNewLocations.Clear();
            MBNewLocations.Text = $"{_newLocationButtonText} (0)";
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            MBNewLocations.Enabled = false;
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
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count.ToString()}";
            return count;
        }
        private int GetRecordCount(IReadOnlyCollection<object> bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count.ToString()}";
            return count;
        }
        private void SetupGridItemDefinition()
        {
            //if (_currentGridDataType == GridDataType.Item) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.Item;
            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = _gridResourceManager.GetString("StationName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "StationName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitOfIssueName",
                HeaderText = _gridResourceManager.GetString("UnitOfIssueName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "UnitOfIssueName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString("LocationMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMax"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMin",
                HeaderText = _gridResourceManager.GetString("LocationMin"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMin"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = _gridResourceManager.GetString("SystemMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMax"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = _gridResourceManager.GetString("SystemMin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMin"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
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
        private void SetupGridNew()
        {
            if (_currentGridDataType == GridDataType.New) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.New;
            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString("InUse"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "InUse"
            };
            DataGridViewHot.Columns.Add(xcol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = _gridResourceManager.GetString("StationName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "StationName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Slot"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _gridResourceManager.GetString("Loc1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc1"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _gridResourceManager.GetString("Loc2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc2"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _gridResourceManager.GetString("Loc3"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc3"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _gridResourceManager.GetString("Loc4"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc4"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _gridResourceManager.GetString("Loc5"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc5"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCodeName",
                HeaderText = _gridResourceManager.GetString("LocationCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "LocationCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
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
        private void SetupGridCurrent()
        {
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
                HeaderText = _gridResourceManager.GetString("Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString("Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Name = "Slot",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString("LocationMax"),
                Name = "LocationMax",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString("ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            col.DefaultCellStyle.Format = "MM-dd-yyyy hh:mm:ss";
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                Name = "StorageTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
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
            var controls = GetTabControls(this, typeof(TabControl));
            foreach (var control1 in controls)
            {
                var control = (TabControl)control1;
                control.Appearance = TabAppearance.FlatButtons;
                control.ItemSize = new Size(0, 1);
                control.SizeMode = TabSizeMode.Fixed;
                foreach (TabPage tab in control.TabPages)
                {
                    tab.Text = string.Empty;
                }
            }
        }

        private IEnumerable<Control> GetTabControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            var enumerable = controls.ToList();
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable).Where(c => c.GetType() == type);
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
                        GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text);
                    }
                }
            }
        }
        private void ClearAllShi()
        {
            Task.Run(() => _logger.Log($"Clear All Shi  START"));
            if (!_neutronVariables.DisplaysEnabled) return;
            if (GlobalVar.Displays == null) return;
            if (!_neutronVariables.ShiEnabled) return;
            GlobalVar.Displays.ClearAllShi();
            Task.Run(() => _logger.Log($"Clear All Shi  END"));
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            Task.Run(() => _logger.Log($"Position Device: {loc1} {loc2} {loc3} {loc4}  START"));
            if (_lacProcessor.MovePermitted(_station.StationNumber, loc1, loc2))
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
                                var resp = _enumResourceManager.GetString(response.Result.ToString());
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: _resourceManager.GetString("DeviceInformation")
                                                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"Location Access Denied");
            }
            Task.Run(() => _logger.Log($"Position Device END"));
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
            Task.Run(() => _logger.Log($"Find Hot Record: {findWhat} START"));
            try
            {
                if (!string.IsNullOrEmpty(findWhat))
                {
                    var akaFind = _akaRepository.Get(findWhat);
                    TextBoxFindItem.Text = akaFind;
                }
                else
                {
                    TextBoxFindItem.Text = findWhat;
                }
                LoadItemDefinitions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message3") + ex.Message);
            }
            Task.Run(() => _logger.Log("Find Hot Record: {findWhat} End"));
        }
        private void ButtonHotPickClear_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("Hot Pick Clear START"));
            Cursor.Current = Cursors.WaitCursor;
            _bindingSourceCurrent.DataSource = null;
            MBCurrentLocations.Text = $"{_resourceManager.GetString("CurrentLocations")} ({_bindingSourceCurrent.Count})";
            _bindingSourceNewLocations.DataSource = null;
            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            TextBoxFindItem.Text = string.Empty;
            LoadItemDefinitions();
            TextBoxFindItem.Focus();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log("Hot Pick Clear END"));
        }
        private async void MBHotPick_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("Hot Pick Button Pressed START"));
            _hotPickButtonPressed = true;
            _hotStoreButtonPressed = false;
            CloseButtonPressed = false;
            //Cost Center
            GroupBoxHotActions.Visible = _useCostCenter;
            if (_useCostCenter)
            {
                await FillCostCenterComboBox();
                RadioButtonCostCenter.Checked = true;
                ComboBoxCostCenter.Visible = true;
                TextBoxFindCostCenter.Visible = true;
                RadioButtonCostCenter.Visible = true;
                RadioButtonPick.Text = $"{_resourceManager.GetString("Pick")}";
                RadioButtonPick.Tag = "Pick";
            }
            _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;
            var loc1 = _currentInventoryView.Loc1;
            var loc2 = _currentInventoryView.Loc2;
            var loc3 = _currentInventoryView.Loc3;
            var loc4 = _currentInventoryView.Loc4;

            var device = _station.HardwareDevices.FirstOrDefault(d => d.DeviceNumber == loc1);

            if (device == null) //No Hardware devices
            {
                HotAction.BackColor = Color.Red;
                LabelFormTitle.BackColor = Color.Red;
                LabelFormTitle.Text = $"{_resourceManager.GetString("HotPick")}";
                MBHotAccept.Text = $"{_resourceManager.GetString("Accept")}";
                await UpdateHotPickScreen(_currentInventoryView);
                tabControl1.SelectedTab = HotAction;
            }
            else if (device.DeviceTypeId == (int)DeviceType.Shuttle)
            {
                if (_lacProcessor.MovePermitted(_station.StationNumber, loc1, loc2))
                {
                    HotActionTray.BackColor = Color.LightGray;
                    LabelFormTitle.BackColor = Color.Red;
                    LabelFormTitle.Text = $"{_resourceManager.GetString("HotPick")}";
                    MBHotAcceptTray.Text = $"{_resourceManager.GetString("Accept")}";
                    PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                    //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                    //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                    await UpdateHotPickScreenTray(_currentInventoryView);
                    tabControl1.SelectedTab = HotActionTray;
                }
                else
                {
                    //Access Denied
                    MessageBox.Show($"Location Access Denied");
                }
            }
            else if (device.DeviceTypeId == (int)DeviceType.Carousel)
            {
                if (_lacProcessor.MovePermitted(_station.StationNumber, loc1, loc2))
                {
                    HotAction.BackColor = Color.Red;
                    LabelFormTitle.BackColor = Color.Red;
                    LabelFormTitle.Text = $"{_resourceManager.GetString("HotPick")}";
                    MBHotAccept.Text = $"{_resourceManager.GetString("Accept")}";
                    UpdateCurrentDeviceIndicator();
                    PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                    ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                        , _currentInventoryView.Loc4.ToString(), 1.ToString());
                    await UpdateHotPickScreen(_currentInventoryView);
                    tabControl1.SelectedTab = HotAction;
                }
                else
                {
                    // Access Denied
                    MessageBox.Show($"Location Access Denied");
                }
            }
            else  // Rack or Supervisor
            {
                HotAction.BackColor = Color.Red;
                LabelFormTitle.BackColor = Color.Red;
                LabelFormTitle.Text = $"{_resourceManager.GetString("HotPick")}";
                MBHotAccept.Text = $"{_resourceManager.GetString("Accept")}";
                await UpdateHotPickScreen(_currentInventoryView);
                tabControl1.SelectedTab = HotAction;
            }
            Task.Run(() => _logger.Log("Hot Pick Button Press END"));
        }

        private async void MBHotStore_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("Hot Store Button Pressed START"));
            TextBoxHotPickQuantity.Text = _quantityToPick.ToString();
            _hotPickButtonPressed = false;
            _hotStoreButtonPressed = true;
            CloseButtonPressed = false;
            //Cost Center
            GroupBoxHotActions.Visible = false;
            //if (_useCostCenter)
            //{
            //    RadioButtonPick.Checked = true;
            //    ComboBoxCostCenter.Visible = false;
            //    TextBoxFindCostCenter.Visible = false;
            //    RadioButtonCostCenter.Visible = false;
            //}

            RadioButtonPick.Text = $"{_resourceManager.GetString("Store")}";
            RadioButtonPick.Tag = "Store";

            HotAction.BackColor = Color.Green;
            LabelFormTitle.BackColor = Color.Green;
            LabelFormTitle.Text = $"{_resourceManager.GetString("HotStore")}";
            MBHotAccept.Text = _resourceManager.GetString("Accept");
            MBHotAccept.Enabled = true;

            if (_currentGridDataType == GridDataType.Current)
            {
                _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;
                var loc1 = _currentInventoryView.Loc1;
                var loc2 = _currentInventoryView.Loc2;
                var loc3 = _currentInventoryView.Loc3;
                var loc4 = _currentInventoryView.Loc4;

                if (_moveableDeviceTypes.Contains(_station.StationType.Id))
                {
                    if (_lacProcessor.MovePermitted(_station.StationNumber, loc1, loc2))
                    {
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                        UpdateCurrentDeviceIndicator();

                        ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                            , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        await UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                    }
                    else
                    {
                        MessageBox.Show($"Location Access Denied");
                        tabControl1.SelectedTab = HotPick;
                    }
                }
                else
                {
                    await UpdateHotPickScreen(_currentInventoryView);
                    tabControl1.SelectedTab = HotAction;
                }
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

                if (_moveableDeviceTypes.Contains(_station.StationType.Id))
                {
                    if (_lacProcessor.MovePermitted(_station.StationNumber, location.Loc1, location.Loc2))
                    {
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                        ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                            , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        await UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                    }
                    else
                    {
                        MessageBox.Show($"Location Access Denied");
                        tabControl1.SelectedTab = HotPick;
                    }
                }
                else
                {
                    await UpdateHotPickScreen(_currentInventoryView);
                    tabControl1.SelectedTab = HotAction;
                }
            }
            Task.Run(() => _logger.Log("Hot Store Button Press END"));
        }

        private async Task UpdateHotPickScreen(SqlInventoryView invItem)
        {
            Task.Run(() => _logger.Log("Update Hot Pick Screen START"));
            try
            {
                using (var db = new NeutronDb())
                {
                    _initialQuantity = _quantityToPick;
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
                    LabelSlot.Text = location.Slot;
                    ComboBoxSizeCodeItem.SelectedIndex = ComboBoxSizeCodeItem.FindStringExact(itemDefinition.SizeCode.Name);
                    ComboBoxVelocityCodeItem.SelectedIndex = ComboBoxVelocityCodeItem.FindStringExact(itemDefinition.VelocityCode.Name);
                    ComboBoxHeightCodeItem.SelectedIndex = ComboBoxHeightCodeItem.FindStringExact(itemDefinition.HeightCode.Name);
                    ComboBoxLocationCodeItem.SelectedIndex = ComboBoxLocationCodeItem.FindStringExact(itemDefinition.LocationCode.Name);
                    ComboBoxSizeCodeLocation.SelectedIndex = ComboBoxSizeCodeLocation.FindStringExact(location.SizeCode.Name);
                    ComboBoxVelocityCodeLocation.SelectedIndex = ComboBoxVelocityCodeLocation.FindStringExact(location.VelocityCode.Name);
                    ComboBoxHeightCodeLocation.SelectedIndex = ComboBoxHeightCodeLocation.FindStringExact(location.HeightCode.Name);
                    ComboBoxLocationCodeLocation.SelectedIndex = ComboBoxLocationCodeLocation.FindStringExact(location.LocationCode.Name);
                    TextBoxHotPickQuantity.Text = _initialQuantity.ToString();
                    TextBoxHotPickQuantity.Focus();
                    TextBoxHotPickLocationQuantity.Text = invItem.Quantity.ToString();
                    TextBoxHotPickReceivedDate.Text = string.IsNullOrEmpty(invItem.ReceivedDate.ToString("d"))
                        ? ""
                        : invItem.ReceivedDate.ToShortDateString();
                    LabelPrimeBin.Visible = invItem.PrimeBin;
                    LabelStaticRelease.Text = invItem.StorageTypeName;
                    if (_neutronVariables.UseImages) PictureBoxItemHotImage.Load(_imageManager.GetImageFile(invItem.Item));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Task.Run(() => _logger.Log("Update Hot Pick Screen END"));
        }

        private async Task UpdateHotPickScreenTray(SqlInventoryView invItem)
        {
            Task.Run(() => _logger.Log("Update Hot Pick Screen Tray START"));
            try
            {
                using (var db = new NeutronDb())
                {
                    var itemDefinition = await db.ItemDefinitions.FindAsync(invItem.ItemDefinitionId);
                    if (itemDefinition == null) throw new ArgumentNullException(nameof(itemDefinition));
                    var location = await db.Locations.FindAsync(invItem.LocationId);
                    if (location == null) throw new ArgumentNullException(nameof(location));
                    LabelHotPickDescriptionTray.Text = itemDefinition.Description;
                    LabelHotPickItemTray.Text = itemDefinition.Item;
                    LabelHotPickUOITray.Text = itemDefinition.UnitOfIssue.Name;
                    TextBoxHotPickLoc1Tray.Text = location.Loc1.ToString();
                    TextBoxHotPickLoc2Tray.Text = location.Loc2.ToString();
                    TextBoxHotPickLoc3Tray.Text = location.Loc3.ToString();
                    TextBoxHotPickLoc4Tray.Text = location.Loc4.ToString();
                    TextBoxHotPickLoc5Tray.Text = location.Loc5.ToString();

                    Control c = Controls.Find($"LabelWidth{location.Loc3.ToString()}", true).First();
                    if (c != null)
                    {
                        var label = ((Label)c);
                        label.BackColor = Color.Red;
                        //label.Visible = true;
                        label.Refresh();
                    }

                    c = Controls.Find($"LabelDepth{location.Loc4.ToString()}", true).First();
                    if (c != null)
                    {
                        var label = ((Label)c);
                        label.BackColor = Color.Red;
                        //label.Visible = true;
                        label.Refresh();
                    }

                    LabelSlotTray.Text = location.Slot;
                    // ComboBoxSizeCodeItem.SelectedIndex = ComboBoxSizeCodeItem.FindStringExact(itemDefinition.SizeCode.Name);
                    // ComboBoxVelocityCodeItem.SelectedIndex = ComboBoxVelocityCodeItem.FindStringExact(itemDefinition.VelocityCode.Name);
                    // ComboBoxHeightCodeItem.SelectedIndex = ComboBoxHeightCodeItem.FindStringExact(itemDefinition.HeightCode.Name);
                    //  ComboBoxLocationCodeItem.SelectedIndex = ComboBoxLocationCodeItem.FindStringExact(itemDefinition.LocationCode.Name);
                    ComboBoxSizeCodeLocationTray.SelectedIndex = ComboBoxSizeCodeLocationTray.FindStringExact(location.SizeCode.Name);
                    ComboBoxVelocityCodeLocationTray.SelectedIndex = ComboBoxVelocityCodeLocationTray.FindStringExact(location.VelocityCode.Name);
                    ComboBoxHeightCodeLocationTray.SelectedIndex = ComboBoxHeightCodeLocationTray.FindStringExact(location.HeightCode.Name);
                    ComboBoxLocationCodeLocationTray.SelectedIndex = ComboBoxLocationCodeLocationTray.FindStringExact(location.LocationCode.Name);
                    TextBoxHotPickQuantityTray.Text = string.Empty;
                    TextBoxHotPickQuantityTray.Focus();
                    TextBoxHotPickLocationQuantityTray.Text = invItem.Quantity.ToString();
                    TextBoxHotPickReceivedDateTray.Text = string.IsNullOrEmpty(invItem.ReceivedDate.ToString("d"))
                        ? ""
                        : invItem.ReceivedDate.ToShortDateString();
                    LabelPrimeBinTray.Visible = invItem.PrimeBin;
                    LabelStaticReleaseTray.Text = invItem.StorageTypeName;
                    if (_neutronVariables.UseImages) PictureBoxItemHotImage.LoadAsync(_imageManager.GetImageFile(invItem.Item).ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Task.Run(() => _logger.Log("Update Hot Pick Screen Tray END"));
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
            Task.Run(() => _logger.Log("DataGrid View Hot Cell Click START"));
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
            Task.Run(() => _logger.Log("DataGrid View Hot Cell Click END"));
        }

        private async Task LoadCurrentAndNew()
        {
            Task.Run(() => _logger.Log("Load Current And New START"));
            if (_currentGridDataType == GridDataType.Item)
            {
                if (_bindingSourceItemDefinitions.Count > 0)
                {
                    var s = ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    TextBoxFindItem.Text = s.Item;
                    LoadCurrent(s);
                    await LoadNewLocations(s);
                    if (_bindingSourceCurrent.Count > 0)
                    {
                        SetupGridCurrent();
                        DataGridViewHot.DataSource = _bindingSourceCurrent;
                        var recordCount = GetRecordCount(_bindingSourceCurrent);
                        DataGridViewHot.SelectedRows[0].Selected = true;
                        //DataGridViewHot.ClearSelection();
                    }
                    else if (_bindingSourceNewLocations.Count > 0)
                    {
                        SetupGridNew();
                        DataGridViewHot.DataSource = _bindingSourceNewLocations;
                        var recordCount = GetRecordCount(_bindingSourceNewLocations);
                        DataGridViewHot.SelectedRows[0].Selected = true;
                        //DataGridViewHot.ClearSelection();
                    }

                    SetHotButtonStatus();
                }
            }
            Task.Run(() => _logger.Log("Load Current And New END"));
        }

        private void SetHotButtonStatus()
        {
            MBHotPick.Enabled = true;
            MBHotStore.Enabled = true;
            MBCurrentLocations.Enabled = true;
            MBNewLocations.Enabled = true;

            if (_bindingSourceNewLocations.Count == 0 && _bindingSourceCurrent.Count == 0)
            {
                MBHotPick.Enabled = false;
                MBHotStore.Enabled = false;
                MBNewLocations.Enabled = false;
                MBCurrentLocations.Enabled = false;
            }

            if (_bindingSourceCurrent.Count > 0 && _bindingSourceNewLocations.Count == 0)
            {
                MBHotPick.Enabled = true;
                MBHotStore.Enabled = true;
                MBCurrentLocations.Enabled = true;
                MBNewLocations.Enabled = false;
            }
            if (_bindingSourceCurrent.Count == 0 && _bindingSourceNewLocations.Count > 0)
            {
                MBHotPick.Enabled = false;
                MBHotStore.Enabled = true;
                MBCurrentLocations.Enabled = false;
                MBNewLocations.Enabled = true;
            }
        }

        private async void MBHotActionBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void Back()
        {
            Task.Run(() => _logger.Log("Hot Action Back Button Pressed START"));
            if (_pickList == null)
            {
                CloseButtonPressed = false;
                ClearAllShi();
                ClearAllDeviceIndicators();
                FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
                LabelFormTitle.Text = _resourceManager.GetString("HotActions");
                LabelFormTitle.BackColor = Color.Red;
                tabControl1.SelectedTab = HotPick;
            }
            else
            {
                CloseButtonPressed = false;

                LabelFormTitle.Text = _resourceManager.GetString("HotActions");
                LabelFormTitle.BackColor = Color.Green;
                tabControl1.SelectedTab = HotPick;
            }

            Task.Run(() => _logger.Log("Hot Action Back Button Pressed END"));
        }

        private async void MBHotAccept_Click(object sender, EventArgs e)
        {
            await Accept();
        }

        private async Task Accept()
        {
            Task.Run(() => _logger.Log("Hot Accept Button Pressed START"));
            ReplenOrderDetail orderDetail = null;
            var pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
            if (CheckForOverPick(pickQty)) return;

            Cursor.Current = Cursors.WaitCursor;
            Inventory inv = null;
            var actionCode = ActionCode.PickHot;
            if (_pickList == null)
            {
                actionCode = ActionCode.PickHot;

                ClearAllShi();
                ClearAllDeviceIndicators();

                if (_useCostCenter)
                {
                    actionCode = GetHotActionCode();
                }
                else
                {
                    if (_hotPickButtonPressed) actionCode = ActionCode.PickHot;
                    if (_hotStoreButtonPressed) actionCode = ActionCode.StoreHot;
                }
            }
            else
            {
                actionCode = ActionCode.StoreRack;
                orderDetail = _repoReplenOrderDetails.FindByKey(_pickList.OrderDetailId.ParseInt());

            }

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
                    if (inventory.Quantity > 0 || inventory.StorageTypeId == (int)StorageType.Static)
                    {
                        _repoInventory.Insert(inventory);

                        _locationsRepository.SetLocationInUse(inventory.LocationId, true);
                        inv = _repoInventory.FindByKey(inventory.Id);

                        if (_pickList == null)
                        {
                            GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty);
                        }
                        else
                        {
                            if (orderDetail != null) orderDetail.PickedQuantity += pickQty;
                            GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty, _pickList);
                        }

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString("Message4")}{Environment.NewLine}" +
                                    $"{ex.Message} {Environment.NewLine}" +
                                    $"{ex.InnerException}");
                }
            }
            else  // Existing Inventory Record
            {
                try
                {
                    inv = _repoInventory.FindByKey(_currentInventoryView.Id);
                    if (inv != null)
                    {
                        if (_pickList == null)
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

                            if (RadioButtonCostCenter.Checked && _useCostCenter && _hotPickButtonPressed)
                            {
                                GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty, (string)ComboBoxCostCenter.SelectedValue);
                            }
                            else
                            {
                                GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty);
                            }
                            await Task.Run(() => _inventoryManager.ReleaseCheck(inv));
                        }
                        else
                        {
                            inv.Quantity += pickQty;
                            _repoInventory.Update(inv);
                            if (orderDetail != null) orderDetail.PickedQuantity += pickQty;
                            GlobalVar.HistoryManager.SaveHistory(actionCode, inv, pickQty, _pickList);
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString("Message5")}{Environment.NewLine}" +
                                    $"{ex.Message} {Environment.NewLine}" +
                                    $" {ex.InnerException}");
                }
            }

            if (_pickList == null)
            {
                if (inv != null) TextBoxFindItem.Text = inv.ItemDefinition.Item;
                await LoadItemDefinitions();
                await LoadCurrentAndNew();
                LabelFormTitle.Text = $"{_resourceManager.GetString("HotSearch")}";
                LabelFormTitle.BackColor = Color.Red;
                Cursor.Current = Cursors.Default;
                tabControl1.SelectedTab = HotPick;
            }
            else
            {
                if (orderDetail != null && orderDetail.PickedQuantity >= _pickList.Ordered.ParseInt())
                {
                    _quantityToPick = 0;
                    orderDetail.LineStatusId = (int)LineStatus.Complete;
                    _repoReplenOrderDetails.Update(orderDetail);

                    //close the form
                    CloseButtonPressed = true;
                    Close();
                }
                else
                {
                    _quantityToPick = _pickList.Ordered.ParseInt() - orderDetail.PickedQuantity;
                    _repoReplenOrderDetails.Update(orderDetail);
                    TextBoxHotPickQuantity.Text = _quantityToPick.ToString();
                    if (inv != null) TextBoxFindItem.Text = inv.ItemDefinition.Item;
                    await LoadItemDefinitions();
                    await LoadCurrentAndNew();
                    LabelFormTitle.Text = $"{_resourceManager.GetString("HotSearch")}";
                    LabelFormTitle.BackColor = Color.Green;
                    Cursor.Current = Cursors.Default;
                    tabControl1.SelectedTab = HotPick;
                }
            }
            Task.Run(() => _logger.Log("Hot Accept Button Press END"));
        }

        private bool CheckForOverPick(int pickQty)
        {
            Task.Run(() => _logger.Log("Check For Over Pick"));
            if (!_hotPickButtonPressed) return false;
            if (pickQty <= _currentInventoryView.Quantity) return false;
            MessageBox.Show($"The quantity to pick exceeds the quantity at this location.");
            TextBoxHotPickQuantity.FocusAndHighlightText();

            return true;
        }

        private ActionCode GetHotActionCode()
        {
            var result = ActionCode.PickHot;
            var radioButtons = new List<RadioButton> { RadioButtonPick, RadioButtonWarranty, RadioButtonScrap, RadioButtonOther, RadioButtonCostCenter };
            //if (RadioButtonPick.Text == $"{_resourceManager.GetString("Pick")}")
            if (_hotPickButtonPressed)
            {
                foreach (var item in radioButtons)
                {
                    if (item.Checked)
                    {
                        switch (item.Tag.ToString())
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
            //else if (RadioButtonPick.Text == $"{_resourceManager.GetString("Store")}")
            else if (_hotStoreButtonPressed)
            {
                result = ActionCode.StoreHot;
                //foreach (var item in radioButtons)
                //{
                //    if (item.Checked)
                //    {
                //        switch (item.Tag.ToString())
                //        {
                //            case "Store":
                //                result = ActionCode.StoreHot;
                //                break;
                //            case "Warranty":
                //                result = ActionCode.WarrantyHotStore;
                //                break;
                //            case "Scrap":
                //                result = ActionCode.ScrapHotStore;
                //                break;
                //            case "Other":
                //                result = ActionCode.OtherHotStore;
                //                break;
                //            case "Cost Center":
                //                result = ActionCode.CostCenterHotStore;
                //                break;
                //            default:
                //                result = ActionCode.StoreHot;
                //                break;
                //        }
                //    }
                //}
            }
            return result;
        }
        private void MBHotActionClose_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("Hot Action Close Button Pressed"));
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
            Task.Run(() => _logger.Log($"Hot Action Key Down Key Pressed: {e.KeyCode} START"));

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    {
                        if (TextBoxFindItem.Focused)
                        {
                            FindItem();
                        }
                        else if (TextBoxScanLocation.Focused)
                        {
                            var slot = TextBoxScanLocation.Text;
                            var searchSlot = GlobalVar.SlotNameFactory.CreateSearchString(slot);
                            TextBoxScanLocation.Text = searchSlot;
                            LoadNewLocationsBySlot(searchSlot);
                        }
                        else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && MBHotAccept.Focused)
                        {
                            await Accept();
                        }
                        else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && TextBoxHotPickQuantity.Focused)
                        {
                            MBHotAccept.Focus();
                        }
                        break;
                    }
                case Keys.Escape:
                    {
                        TextBoxFindItem.Text = "";
                        TextBoxFindItem.Focus();
                        Back();
                        break;
                    }
                case Keys.Space:
                    {
                        await Accept();
                        break;
                    }
                case Keys.F2:
                    {
                        PrintLabel();
                        break;
                    }
                case Keys.F12:
                    {
                        using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
                        {
                            var result = frm.ShowDialog();
                            Show();
                        }
                        break;
                    }
            }






            //if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            //{
            //    if (TextBoxFindItem.Focused)
            //    {
            //        FindItem();
            //    }
            //    else if (TextBoxScanLocation.Focused)
            //    {
            //        var slot = TextBoxScanLocation.Text;
            //        var searchSlot = GlobalVar.SlotNameFactory.CreateSearchString(slot);
            //        TextBoxScanLocation.Text = searchSlot;
            //        LoadNewLocationsBySlot(searchSlot);
            //    }
            //    else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && MBHotAccept.Focused)
            //    {
            //        await Accept();
            //    }
            //    else if (TextBoxHotPickQuantity.Text.ParseInt() > 0 && TextBoxHotPickQuantity.Focused)
            //    {
            //        MBHotAccept.Focus();
            //    }
            //}
            //if (e.KeyCode == Keys.Escape)
            //{
            //    TextBoxFindItem.Text = "";
            //    TextBoxFindItem.Focus();
            //}
            //if (e.KeyCode == Keys.F12)
            //{
            //    using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
            //    {
            //        var result = frm.ShowDialog();
            //        Show();
            //    }
            //}
            Task.Run(() => _logger.Log("Hot Action Key Down END"));
        }

        private void MBCurrentLocations_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("Current Locations Pressed START"));
            MBHotPick.Enabled = true;
            MBHotStore.Enabled = true;
            SetupGridCurrent();
            DataGridViewHot.DataSource = _bindingSourceCurrent;
            var recordCount = GetRecordCount(_bindingSourceCurrent);
            Task.Run(() => _logger.Log("Current Locations Pressed END"));
        }
        private void MBNewLocations_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log("New Locations Pressed START"));
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = true;
            SetupGridNew();
            DataGridViewHot.DataSource = _bindingSourceNewLocations;
            var recordCount = GetRecordCount(_bindingSourceNewLocations);
            Task.Run(() => _logger.Log("New Locations Pressed END"));
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
        private void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            FindItem();
            //await LoadNewLocations(_currentItemDefinition);
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
            _currentInventoryView.Quantity = qty;
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
            //_repoLocationCount.Insert(cnt);
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
            if (!_useCostCenter) return;
            var tb = TextBoxHotPickQuantity.Text;
            ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3, _currentInventoryView.Loc4.ToString(), tb);
        }
        private void TextBoxFindCostCenter_Leave(object sender, EventArgs e)
        {
            if (!_useCostCenter) return;
            var search = TextBoxFindCostCenter.Text;
            var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
            ComboBoxCostCenter.DroppedDown = true;
        }
        private void ComboBoxCostCenter_TextChanged(object sender, EventArgs e)
        {
            if (!_useCostCenter) return;
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
            if (!_useCostCenter) return;
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
            if (!_useCostCenter) return;
            TextBoxFindCostCenter.Focus();
        }


        private void UpdateCurrentDeviceIndicator()
        {
            Task.Run(() => _logger.Log("Update Current Device Indicator START"));
            ClearActiveDeviceIndicators();
            var loc1 = _currentInventoryView.Loc1;
            _deviceIndicators[loc1].BlinkOn();
            _deviceIndicators[loc1].Active = true;
            Task.Run(() => _logger.Log("Update Current Device Indicator END"));
        }

        private void ClearActiveDeviceIndicators()
        {
            Task.Run(() => _logger.Log("Clear Active Device Indicators START"));
            var devices = _deviceIndicators.Where(x => x.Value.Active == true).ToList();
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in devices)
            {
                Task.Run(() => _logger.Log($"Clear Active Device Indicator: {deviceIndicator.Value.DeviceNumber}"));
                deviceIndicator.Value.BlinkOff();
                deviceIndicator.Value.Active = false;
            }
            Task.Run(() => _logger.Log("Clear Active Device Indicators END"));
        }

        private void ClearAllDeviceIndicators()
        {
            Task.Run(() => _logger.Log("Clear ALL Active Device Indicators START"));
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in _deviceIndicators)
            {
                Task.Run(() => _logger.Log($"Clear ALL Active Device Indicator: {deviceIndicator.Value.DeviceNumber}"));
                deviceIndicator.Value.Active = false;
                deviceIndicator.Value.BlinkOff();
            }
            Task.Run(() => _logger.Log("Clear ALL Active Device Indicators END"));
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmHotAction",
               resourceDir: languageDirectory, usingResourceSet: null);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);

                HotPick.Text = _resourceManager.GetString("HotPick");
                CheckBoxAll.Text = _resourceManager.GetString("All");
                MBHotStore.Text = _resourceManager.GetString("HotStore");
                MBHotPick.Text = _resourceManager.GetString("HotPick");
                LabelSearch.Text = _resourceManager.GetString("SearchforanypartofIt");
                MBHotActionClose.Text = _resourceManager.GetString("Close");
                MBNewLocations.Text = _resourceManager.GetString("NewLocations");
                MBCurrentLocations.Text = _resourceManager.GetString("CurrentLocations");
                MBFindItem.Text = _resourceManager.GetString("Search");
                HotAction.Text = _resourceManager.GetString("HotAction");
                LabelMainLocation.Text = _resourceManager.GetString("Location");
                ButtonEditItemDefinition.Text = _resourceManager.GetString("Edit");
                LabelMainHeight.Text = _resourceManager.GetString("Height");
                LabelMainVelocity.Text = _resourceManager.GetString("Velocity");
                LabelMainSize.Text = _resourceManager.GetString("Size");
                LabelMainUnitOfIssue.Text = _resourceManager.GetString("UnitofIssue");
                GroupBoxHotActions.Text = _resourceManager.GetString("TransactionType");
                RadioButtonCostCenter.Text = _resourceManager.GetString("CostCenter");
                RadioButtonOther.Text = _resourceManager.GetString("Other");
                RadioButtonScrap.Text = _resourceManager.GetString("Scrap");
                RadioButtonWarranty.Text = _resourceManager.GetString("Warranty");
                RadioButtonPick.Text = _resourceManager.GetString("Pick");
                LabelMainQuantity.Text = _resourceManager.GetString("Qty");
                LabelMainItem.Text = _resourceManager.GetString("Item");
                LabelMainDescription.Text = _resourceManager.GetString("Desc");
                GroupBoxHotPickLocation.Text = _resourceManager.GetString("Location");
                LabelLocation.Text = _resourceManager.GetString("Location");
                ButtonEditLocationDefinition.Text = _resourceManager.GetString("Edit");
                LabelHeight.Text = _resourceManager.GetString("Height");
                LabelVelocity.Text = _resourceManager.GetString("Velocity");
                LabelSize.Text = _resourceManager.GetString("Size");
                LabelReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                LabelStaticRelease.Text = _resourceManager.GetString("StaticLocation");
                LabelPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                LabelLocationQuantity.Text = _resourceManager.GetString("LocationQuantity");
                LabelBack.Text = _resourceManager.GetString("Back");
                LabelOver.Text = _resourceManager.GetString("Over");
                LabelTray.Text = _resourceManager.GetString("Tray");
                LabelDevice.Text = _resourceManager.GetString("Device");
                MBHotActionCount.Text = _resourceManager.GetString("LocationCount");
                MBHotAccept.Text = _resourceManager.GetString("Accept");
                MBHotActionBack.Text = _resourceManager.GetString("Back");
                LabelFormTitle.Text = _resourceManager.GetString("Jobs");
                mlUserInfo.Text = _resourceManager.GetString("Login?");
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  { ex.Message} { Environment.NewLine} { ex.InnerException} ");
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel1.ClientRectangle,
                Color.Black, 10, ButtonBorderStyle.Dashed, // left
                Color.Black, 10, ButtonBorderStyle.Dashed, // top
                Color.Black, 10, ButtonBorderStyle.Dashed, // right
                Color.Black, 10, ButtonBorderStyle.Dashed);// bottom
        }

        private void TextBoxScanLocation_Enter(object sender, EventArgs e)
        {
            TextBoxScanLocation.Text = string.Empty;
            TextBoxScanLocation.Focus();
        }

        private void TextBoxScanLocation_Click(object sender, EventArgs e)
        {
            TextBoxScanLocation.Text = string.Empty;
            TextBoxScanLocation.Focus();
        }
    }
}
