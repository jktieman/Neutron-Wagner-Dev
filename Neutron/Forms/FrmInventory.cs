using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using Neutron.Global;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using Neutron.Classes;
using Equin.ApplicationFramework;
using EnumsNET;
using NeutronCore.Extensions;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore.Global;
using NeutronCore;
using NeutronData.Interfaces;
using NeutronCore.Enums;
using IntegerExtensions = NeutronCore.Extensions.IntegerExtensions;
using StationType = NeutronCore.Enums.StationType;
using StorageType = Neutron.Enums.StorageType;
namespace Neutron.Forms
{
    public partial class FrmInventory : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _gridResourceManager;

        private readonly BindingSource _bindingSource = new BindingSource();
        private readonly BindingSource _locationBindingSource = new BindingSource();
        private readonly BindingSource _newLocationBindingSource = new BindingSource();
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());
        private readonly GenericRepository<SizeCode> _repoSizeCode = new GenericRepository<SizeCode>(new NeutronDb());
        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());
        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.StorageType> _repoStorageType =
            new GenericRepository<NeutronData.Models.Lookups.StorageType>(new NeutronDb());
        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue =
            new GenericRepository<UnitOfIssue>(new NeutronDb());

        private readonly IWorkstationRepository _workstationRepository;

        private readonly ILocationsRepository _locationsRepository;

        private readonly InventoryRepository _inventoryRepository = new InventoryRepository();
        private readonly IJsonData _jsonData;
        private readonly NeutronVariables _neutronVariables;
        private readonly HistoryManager _historyManager;
        private readonly IAreaRepository _areaRepository;
        private readonly IRFIDManager _rfidManager;
        private readonly WorkstationView _workstationView;
        private readonly IAkaRepository _akaRepository;
        private readonly ILacProcessor _lacProcessor;
        private bool _allAvailable;
        public ItemDefinition CurrentItem { get; set; }
        public Location CurrentLocation { get; set; }
        public Inventory CurrentInventoryItem { get; set; }
        public bool CloseButtonPressed { get; set; }
        private bool _firstTime = true;

        private DynamicLogger _logger;
        private HeaderTextManager _headerTextManager;

        public FrmInventory(IJsonData jsonData, IAkaRepository akaRepository,
            ILacProcessor lacProcessor, IWorkstationRepository workstationRepository
            , WorkstationView workstationView, NeutronVariables neutronVariables
            , HistoryManager historyManager, IAreaRepository areaRepository
            , IRFIDManager rfidManager, ILocationsRepository locationsRepository)
        {
            InitializeComponent();

            _workstationRepository = workstationRepository;
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            KeyPreview = true;
            CloseButtonPressed = false;
            _workstationView = workstationView;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _historyManager = historyManager;
            _areaRepository = areaRepository;
            _rfidManager = rfidManager;
            _akaRepository = akaRepository;
            _lacProcessor = lacProcessor;
            _locationsRepository = locationsRepository;

            Init();
        }

        private void Init()
        {
            LabelStationName.Text = _workstationView.ToString();
            _headerTextManager = new HeaderTextManager();
            SetupGrids();
            HideTabControlTabs();
            SetupNewForm();
            SetupViewEditForm();
            SetupAddDetailForm();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            _logger = CreateLog();

            if (_workstationView.StationType.Id == (int)StationType.Supervisor)
            {
                CheckBoxAllStations.Checked = true;
            }

            ComboBoxAreaNumber.DataSource = _areaRepository.GetAllPickableAreas();
            ComboBoxAreaNumber.ValueMember = "Id";
            ComboBoxAreaNumber.DisplayMember = "Name";
            _firstTime = false;
            //Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
            //    , "Inventory File", MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            LoadInventory();
        }
        /// <summary>
        /// Creates a new Dynamic Logger
        /// </summary>
        /// <returns></returns>
        private DynamicLogger CreateLog()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = $"Inventory_{_workstationView.WorkstationNumber.ToString()}";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
            return _logger;
        }
        /// <summary>
        /// CreateParams
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        /// <summary>
        /// Loads the inventory by AreaId or All if it is a Supervisor Station
        /// If the Find textbox has any value, it will search for it
        /// The process looks in the AKA file first
        /// If a Record id is passed is the focus is on that record
        /// </summary>
        /// <param name="recId">Record Id to set the focus</param>
        private void LoadInventory(int recId = 0)
        {
            Cursor.Current = Cursors.WaitCursor;

            var findWhat = TextBoxFind.Text.ToLower().Trim();
            var find = _akaRepository.Get(findWhat);
            TextBoxFind.Text = find;

            if (_workstationView != null)
            {
                if (_workstationView.StationType.Id == (int)StationType.Supervisor)
                {
                    CheckBoxAllStations.Checked = true;
                }

                IEnumerable<SqlInventoryView> views = CheckBoxAllStations.Checked
                    ? _inventoryRepository.FindInventoryViews(find)
                    : _inventoryRepository.FindInventoryViewsByArea(find, _workstationView.AreaId);

                var blv = new BindingListView<SqlInventoryView>(views.ToList());
                _bindingSource.DataSource = blv;
                DataGridView1.DataSource = _bindingSource;
                if (GetRecordCount(_bindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        var idx = IndexOf(_bindingSource, recId);
                        DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                        DataGridView1.CurrentCell = DataGridView1.Rows[idx].Cells[1];
                        DataGridView1.Rows[idx].Selected = true;
                    }
                    SetCurrentInventoryItem();
                    DataGridView1.Refresh();
                    DataGridView1.ClearSelection();
                    if (DataGridView1.RowCount > 0) DataGridView1.FastAutoSizeColumns();
                }
                else
                {
                    CurrentItem = null;
                    CurrentLocation = null;
                    CurrentInventoryItem = null;
                }
            }
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Sets the Current Inventory Item, Location and Item Definition
        /// </summary>
        private void SetCurrentInventoryItem()
        {
            if (_bindingSource.Current != null)
            {
                var inventory = ((ObjectView<SqlInventoryView>)_bindingSource.Current).Object;
                CurrentInventoryItem = _repoInventory.FindByKey(inventory.Id);
                SetCurrentLocation(inventory.LocationId);
                SetCurrentItemDefinition(inventory.ItemDefinitionId);
            }
        }
        private void SetCurrentInventoryItem(Inventory inventory)
        {
            CurrentInventoryItem = inventory;
            SetCurrentLocation(inventory.LocationId);
            SetCurrentItemDefinition(inventory.ItemDefinitionId);
        }

        /// <summary>
        /// Sets the Current Item Definition
        /// </summary>
        /// <param name="itemDefinitionId">Item Definition Id</param>
        private void SetCurrentItemDefinition(int itemDefinitionId)
        {
            var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
            if (itemDefinition == null) return;
            CurrentItem = itemDefinition;
        }

        /// <summary>
        /// Sets the Current Location
        /// </summary>
        /// <param name="locationId">Location Id</param>
        private void SetCurrentLocation(int locationId)
        {
            var location = _repoLocation.FindByKey(locationId);
            if (location == null) return;
            CurrentLocation = location;
        }


        public int IndexOf(BindingSource bs, int id)
        {
            var count = bs.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((SqlInventoryView)bs[i]).Id;
                if (rec == id)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        /// <summary>
        /// Display the record count
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count}";
            LabelAvailableLocations.Text = $"{_resourceManager.GetString("Records")}: {count}";
            return count;
        }

        #region Find Functions
        private void MButtonFind_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
        }
        private void FindRecord()
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
        }
        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                Cursor.Current = Cursors.WaitCursor;
                LoadInventory();
                Cursor.Current = Cursors.Default;
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }
        #endregion
        #region Button Clicks
        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            TextBoxFind.Focus();
        }
        private void MButtonClose_Click(object sender, EventArgs e)
        {
            ClearAllShi();
            CloseButtonPressed = true;
            Close();
        }
        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            if (DataGridView1.RowCount <= 0) return;
            LoadViewEdit();
        }
        private void LoadViewEdit()
        {
            if (CurrentItem != null)
            {
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
                var views = GetInventoryViewListByItem(CurrentItem.Id);
                var blv = new BindingListView<SqlInventoryView>(views.ToList());
                _locationBindingSource.DataSource = blv;
                DataGridViewInventoryLocations.DataSource = _locationBindingSource;
                tabControl1.SelectedTab = tabPage2;
                DataGridViewInventoryLocations.ClearSelection();
            }
        }
        private void MButtonNew_Click(object sender, EventArgs e)
        {
            ClearNewFields();
            var inventoryViews = GetSelectedInventoryViews(DataGridView1);
            if (inventoryViews != null && inventoryViews.Count > 0)
            {
                TextBoxNewItem.Text = inventoryViews.First().Item;
                NewItemFind();
            }

            tabControl1.SelectedTab = tabPage3;
        }
        private void MbViewEditListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbViewEditNew_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage3;
        }
        private void MbNewFind_Click(object sender, EventArgs e)
        {
            NewItemFind();
        }
        private void MbViewEditClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbViewEditSave_Click(object sender, EventArgs e)
        {
            UpdateViewEdit();
        }
        private void MbViewEditEdit_Click(object sender, EventArgs e)
        {
            var inventoryView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
            TextBoxAddDetailItem.Text = inventoryView.Item;
            TextBoxAddDetailDescription.Text = inventoryView.Description;
            DateTimePickerAddDetailReceivedDate.Value = inventoryView.ReceivedDate;
            TextBoxAddDetailInventoryId.Text = inventoryView.Id.ToString();
            TextBoxAddDetailLocationId.Text = inventoryView.LocationId.ToString();
            TextBoxAddDetailQuantity.Text = inventoryView.Quantity.ToString();
            TextBoxAddDetailItemDefinitionId.Text = inventoryView.ItemDefinitionId.ToString();
            ComboBoxAddDetailArea.SelectedValue = inventoryView.AreaId;
            CheckBoxAddDetailPrimeBin.Checked = inventoryView.PrimeBin;
            TextBoxAddDetailLoc1.Text = inventoryView.Loc1.ToString();
            TextBoxAddDetailLoc2.Text = inventoryView.Loc2.ToString();
            TextBoxAddDetailLoc3.Text = inventoryView.Loc3.ToString();
            TextBoxAddDetailLoc4.Text = inventoryView.Loc4.ToString();
            TextBoxAddDetailLoc5.Text = inventoryView.Loc5.ToString();
            TextBoxAddDetailSlot.Text = inventoryView.Slot.ToString();
            TextBoxPickSequence.Text = inventoryView.PickSequence.ToString();
            ComboBoxAddDetailStorageType.SelectedValue = inventoryView.StorageTypeId;
            ComboBoxAddDetailSizeCode.SelectedValue = inventoryView.SizeCodeId;
            ComboBoxAddDetailVelocityCode.SelectedValue = inventoryView.VelocityCodeId;
            ComboBoxAddDetailHeightCode.SelectedValue = inventoryView.HeightCodeId;
            TextBoxAddDetailLocationCode.Text = inventoryView.LocationCode;
            CheckBoxInUse.Checked = inventoryView.InUse;
            LabelActionAddDetail.Text = _resourceManager.GetString("EditDetail");
            TextBoxAddDetailQuantity.Focus();
            ButtonPositionDevice.Visible = inventoryView.AreaId != 8;
            tabControl1.SelectedTab = tabPage5;
        }
        private void MbNewListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbNewViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
        }
        private void MbNewSave_Click(object sender, EventArgs e)
        {
            SaveNew();
        }
        private void MbNewClose_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        #endregion
        private void SaveNew()
        {
            try
            {
                var rec = new Inventory
                {
                    ItemDefinitionId =
                        (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).ItemDefinitionId,
                };
                if (!ValidateFields(rec))
                {
                    MessageBox.Show(_resourceManager.GetString("Message0"));
                    return;
                }
                if (IsDuplicate(rec))
                {
                    MessageBox.Show(_resourceManager.GetString("Message1"));
                    return;
                }
                _repoInventory.Insert(rec);
                _historyManager.SaveHistory(ActionCode.InventoryAdd, rec);
                LoadInventory(rec.Id);
                tabControl1.SelectedTab = tabPage1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message3") + ex.Message + ex.InnerException + ex.InnerException.InnerException);
            }
        }
        private void UpdateViewEdit()
        {
            var id = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).Id;
            var rec = new Inventory
            {
                Id = id,
                LocationId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).LocationId,
                ItemDefinitionId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).ItemDefinitionId,
            };
            if (!ValidateFields(rec))
            {
                MessageBox.Show(_resourceManager.GetString("Message4"));
                return;
            }
            _repoInventory.Update(rec);
            _historyManager.SaveHistory(ActionCode.InventoryModify, rec);
            LoadInventory(rec.Id);
            tabControl1.SelectedTab = tabPage1;
        }
        private bool ValidateFields(Inventory rec)
        {
            //if (!IntegerValidator(rec.Loc1))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc2))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc3))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc4))
            //{
            //    return false;
            //}
            //if (!IntegerValidator(rec.Loc5))
            //{
            //    return false;
            //}
            return true;
        }
        private bool StringValidator(string input)
        {
            var pattern = "[^a-zA-Z]";
            if (Regex.IsMatch(input, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //validate integer 
        private bool IntegerValidator(int input)
        {
            var pattern = "^[0-9]+$";
            if (Regex.IsMatch(input.ToString(), pattern))
            {
                if (input <= 0)
                {
                    MessageBox.Show(_resourceManager.GetString("Message4"));
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsDuplicate(Inventory recIn)
        {
            var rec = _repoInventory
                .FindBy(f => f.LocationId == recIn.LocationId && f.ItemDefinitionId == recIn.ItemDefinitionId)
                .FirstOrDefault();
            if (rec != null)
            {
                MessageBox.Show(_resourceManager.GetString("Message5"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            return false;
        }

        #region Form Setup Grids
        private void SetupGrids()
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.AllowUserToAddRows = false;
            var position = _gridResourceManager.GetString("Position");
            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = _gridResourceManager.GetString(""),
                Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridView1.Columns.Add(bCol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Item"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString("Quantity"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridView1.Columns.Add(col);
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString("PrimeBin"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString("ReceivedDate"),
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridView1.Columns.Add(col);

            DataGridView1.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);

            DataGridView1.EnableHeadersVisualStyles = false;
            DataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridView1.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            //Location Grid
            DataGridViewInventoryLocations.AutoGenerateColumns = false;
            DataGridViewInventoryLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryLocations.AllowUserToAddRows = false;
            bCol = new DataGridViewButtonColumn
            {
                //HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryLocations.Columns.Add(bCol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "AreaName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString("Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString("ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "ReceivedDate"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString("StorageTypeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StorageTypeName"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridViewInventoryLocations.Columns.Add(col);
            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "PrimeBin",
                HeaderText = _gridResourceManager.GetString("PrimeBin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PrimeBin"
            };
            DataGridViewInventoryLocations.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryLocations.Columns.Add(col);

            DataGridViewInventoryLocations.EnableHeadersVisualStyles = false;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryLocations.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewInventoryLocations.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            //New Location Grid
            DataGridViewInventoryNewLocations.AutoGenerateColumns = false;
            DataGridViewInventoryNewLocations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewInventoryNewLocations.AllowUserToAddRows = false;
            bCol = new DataGridViewButtonColumn
            {
                // HeaderText = _gridResourceManager.GetString("Id"),
                Visible = _workstationView.StationTypeId != (int)StationType.RackTablet && _workstationView.StationTypeId != (int)StationType.Supervisor,
                Name = "Position",
                Text = position,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(bCol);
            xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString("InUse"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "InUse"
            };
            DataGridViewInventoryNewLocations.Columns.Add(xcol);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaName",
                HeaderText = _gridResourceManager.GetString("Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "StationName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Slot"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickSequence",
                HeaderText = $"Pick Sequence",  // _gridResourceManager.GetString("Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PickSequence"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "SizeCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc1",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc2",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc3",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc4",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Loc5",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "VelocityCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "HeightCodeName",
                ReadOnly = true
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString("LocationCode"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LocationCode"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewInventoryNewLocations.Columns.Add(col);

            DataGridViewInventoryNewLocations.EnableHeadersVisualStyles = false;
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewInventoryNewLocations.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewInventoryNewLocations.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
        }
        private void DataGridViewInventoryLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }
        private void DataGridViewInventoryAddDetailLocations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
        }
        private void SetupNewForm()
        {
            //LabelFindDescription.Text = "Search any part of the Item or Description field";
            ComboBoxNewSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxNewSizeCode.DisplayMember = "Name";
            ComboBoxNewSizeCode.ValueMember = "Id";
            ComboBoxNewVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxNewVelocityCode.DisplayMember = "Name";
            ComboBoxNewVelocityCode.ValueMember = "Id";
            ComboBoxNewHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxNewHeightCode.DisplayMember = "Name";
            ComboBoxNewHeightCode.ValueMember = "Id";
            ComboBoxNewArea.DataSource = _workstationRepository.Lookup();
            ComboBoxNewArea.DisplayMember = "Name";
            ComboBoxNewArea.ValueMember = "Id";
            ComboBoxNewStorageType.DataSource = _repoStorageType.All();
            ComboBoxNewStorageType.DisplayMember = "Name";
            ComboBoxNewStorageType.ValueMember = "Id";
            ComboBoxInventoryNewLocationsStorageType.DataSource = _repoStorageType.All();
            ComboBoxInventoryNewLocationsStorageType.DisplayMember = "Name";
            ComboBoxInventoryNewLocationsStorageType.ValueMember = "Id";
            ComboBoxNewUnitOfIssue.DataSource = _repoUnitOfIssue.All();
            ComboBoxNewUnitOfIssue.DisplayMember = "Name";
            ComboBoxNewUnitOfIssue.ValueMember = "Id";
        }
        private void SetupAddDetailForm()
        {
            ComboBoxAddDetailSizeCode.DataSource = _repoSizeCode.All();
            ComboBoxAddDetailSizeCode.DisplayMember = "Name";
            ComboBoxAddDetailSizeCode.ValueMember = "Id";
            ComboBoxAddDetailVelocityCode.DataSource = _repoVelocityCode.All();
            ComboBoxAddDetailVelocityCode.DisplayMember = "Name";
            ComboBoxAddDetailVelocityCode.ValueMember = "Id";
            ComboBoxAddDetailHeightCode.DataSource = _repoHeightCode.All();
            ComboBoxAddDetailHeightCode.DisplayMember = "Name";
            ComboBoxAddDetailHeightCode.ValueMember = "Id";
            ComboBoxAddDetailArea.DataSource = _areaRepository.Lookup();
            ComboBoxAddDetailArea.DisplayMember = "Name";
            ComboBoxAddDetailArea.ValueMember = "Id";
            ComboBoxAddDetailStorageType.DataSource = _repoStorageType.All();
            ComboBoxAddDetailStorageType.DisplayMember = "Name";
            ComboBoxAddDetailStorageType.ValueMember = "Id";
        }
        private void SetupViewEditForm()
        {
        }
        #endregion
        #region Return Key Functions
        //private void TextBoxNewLoc1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc2.Focus();
        //    }
        //}
        //private void TextBoxNewLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc3.Focus();
        //    }
        //}
        //private void TextBoxNewLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc4.Focus();
        //    }
        //}
        //private void TextBoxNewLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc5.Focus();
        //    }
        //}
        //private void TextBoxNewLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewSizeCode.Focus();
        //    }
        //}
        //private void ComboBoxNewSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewVelocityCode.Focus();
        //    }
        //}
        //private void ComboBoxNewVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxNewHeightCode.Focus();
        //    }
        //}
        //private void ComboBoxNewHeightCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxNewLoc2.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc1_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc2.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc2_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc3.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc3_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc4.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc4_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc5.Focus();
        //    }
        //}
        //private void TextBoxViewEditLoc5_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditSizeCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditSizeCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditVelocityCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditVelocityCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        ComboBoxViewEditHeightCode.Focus();
        //    }
        //}
        //private void ComboBoxViewEditHeightCode_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        TextBoxViewEditLoc1.Focus();
        //    }
        //}
        //private void tabControl1_Enter(object sender, EventArgs e)
        //{
        //    //if (tabControl1.SelectedIndex == 1)
        //    //{
        //    //    TextBoxViewEditLoc1.Focus();
        //    //}
        //    //if (tabControl1.SelectedIndex == 2)
        //    //{
        //    //    TextBoxNewLoc1.Focus();
        //    //}
        //}
        #endregion
        private void MbViewEditDelete_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (_bindingSource.Current != null)
            {
                var locView = ((ObjectView<SqlInventoryView>)_locationBindingSource.Current).Object;
                if (locView != null)
                {
                    // var invId = (((ObjectView<SqlInventoryView>)_bindingSource.Current).Object).Id;
                    DeleteInventoryItem(locView.Id, false);
                    // LoadInventory();
                    LoadViewEdit();
                    tabControl1.SelectedTab = tabPage2;
                    //LoadInventory();
                    //tabControl1.SelectedTab = tabPage1; 
                }
            }
            else
            {
                MessageBox.Show(_resourceManager.GetString("Message6"));
            }
            Cursor.Current = Cursors.Default;
        }
        public void DeleteInventoryItem(int invId, bool releaseOnly)
        {
            var inventoryManager = new InventoryManager(_repoInventory, _locationsRepository, _historyManager);
            inventoryManager.DeleteInventoryRecord(invId, releaseOnly: releaseOnly);
        }
        //private void DeleteInventoryRecord(int invId, bool releaseOnly = false)
        //{
        //    var inventory = _repoInventory.FindByKey(invId);
        //    if (inventory == null) return;
        //    if (releaseOnly)
        //    {
        //        if (inventory.StorageTypeId == (int)StorageType.Release)
        //        {
        //            _historyManager.SaveHistory(ActionCode.InventoryDelete, inventory);
        //            _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
        //            _repoInventory.Delete(inventory.Id);
        //        }
        //    }
        //    else
        //    {
        //        _historyManager.SaveHistory(ActionCode.InventoryDelete, inventory);
        //        _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
        //        _repoInventory.Delete(inventory.Id);
        //    }
        //}
        private void MbViewEditAddLocation_Click(object sender, EventArgs e)
        {
            AddLocation();
        }
        private void AddLocation()
        {
            if (CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                TextBoxNewLocationsItemDefinitionId.Text = CurrentItem.Id.ToString();
                TextBoxInventoryNewLocationsQuantity.Text = "0";
                ComboBoxInventoryNewLocationsStorageType.SelectedIndex = 1;
                GetAvailableLocations(CurrentItem);
            }
            tabControl1.SelectedTab = tabPage4;
        }
        public async void GetAvailableLocations(ItemDefinition itemDefinition, int recId = 0)
        {
            var idx = recId;
            var areaId = itemDefinition.AreaId;
            var inUse = 0;
            var area = _areaRepository.GetArea(itemDefinition.AreaId);
            if (area != null)
            {
                var views = await _locationsRepository.GetAllLocationViewsExact(areaId, itemDefinition.SizeCodeId,
                    itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse);

                var blv = new BindingListView<LocationView>(views.ToList());
                _newLocationBindingSource.DataSource = blv;
                DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
                if (GetRecordCount(_newLocationBindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_newLocationBindingSource, recId);
                    }

                    DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                        DataGridViewInventoryNewLocations.Rows[idx].Index;
                    DataGridViewInventoryNewLocations.Refresh();
                    DataGridViewInventoryNewLocations.CurrentCell =
                        DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                    DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
                }
            }
        }

        private List<InventoryView> GetSelectedInventoryViews(DataGridView dataGridView)
        {
            var inventoryViews = new List<InventoryView>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var inventoryId = (int)row.Cells["Id"].Value;
                var inventoryView = _inventoryRepository.GetInventoryViewById(inventoryId);
                if (inventoryView != null)
                {
                    inventoryViews.Add(inventoryView);
                }
            }
            //if (!inventoryViews.Any())
            //{
            //    MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
            //}
            return inventoryViews;
        }

        /// <summary>
        /// Revised edition that uses PickStations
        /// A Workstation can have multiple PickStations
        /// The WorkstationView has a variable, Stations with the Stations that this Workstation Controls
        /// </summary>
        /// <param name="itemDefinition"></param>
        /// <param name="recId"></param>
        public async void GetAvailableLocationsByStation(ItemDefinition itemDefinition, int recId = 0)
        {
            var areas = _areaRepository.GetAllPickableAreas();
            //string areas = string.Empty;
            var idx = recId;
            var areaIds = _areaRepository.GetAllPickableAreaIds();
            //var areaIds  = _workstationView.Areas.Select(r => new List<string> {r.Id.ToString()}).ToList();
            //if (areaIds.Any())
            //{
            //    areas = string.Join(",", areaIds);
            //}
            var areasAsString = string.Join(",", areaIds);
            var inUse = 0;
            var area = _areaRepository.GetArea(itemDefinition.AreaId);
            if (area != null)
            {
                var views = await _locationsRepository.GetAllLocationViewsExactByAreas(areasAsString, itemDefinition.SizeCodeId,
                    itemDefinition.VelocityCodeId, itemDefinition.HeightCodeId, inUse);

                var blv = new BindingListView<LocationView>(views.ToList());
                _newLocationBindingSource.DataSource = blv;
                DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
                if (GetRecordCount(_newLocationBindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_newLocationBindingSource, recId);
                    }

                    DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                        DataGridViewInventoryNewLocations.Rows[idx].Index;
                    DataGridViewInventoryNewLocations.Refresh();
                    DataGridViewInventoryNewLocations.CurrentCell =
                        DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                    DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
                }
            }
        }

        public async void GetAllAvailableLocations(ItemDefinition itemDefinition, int recId = 0)
        {
            var idx = recId;

            if (_workstationView != null)
            {
                var areaId = _workstationView.AreaId;
                IEnumerable<LocationView> views = new List<LocationView>();
                IEnumerable<LocationView> recs = new List<LocationView>();
                views = await Task.Run(() => _locationsRepository.FindLocationViewsByArea(areaId));
                var blv = new BindingListView<LocationView>(views.ToList());
                _newLocationBindingSource.DataSource = blv;
                DataGridViewInventoryNewLocations.DataSource = _newLocationBindingSource;
                if (GetRecordCount(_newLocationBindingSource) > 0)
                {
                    if (recId != 0)
                    {
                        idx = IndexOf(_newLocationBindingSource, recId);
                    }

                    DataGridViewInventoryNewLocations.FirstDisplayedScrollingRowIndex =
                        DataGridViewInventoryNewLocations.Rows[idx].Index;
                    DataGridViewInventoryNewLocations.Refresh();
                    DataGridViewInventoryNewLocations.CurrentCell =
                        DataGridViewInventoryNewLocations.Rows[idx].Cells[1];
                    DataGridViewInventoryNewLocations.Rows[idx].Selected = true;
                }
            }
        }
        private void FrmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
        private void TextBoxNewItem_Leave(object sender, EventArgs e)
        {
            // get the item and fill in the form
            NewItemFind();
        }
        private void NewItemFind()
        {
            var item = TextBoxNewItem.Text;

            try
            {
                var rec = _repoItemDefinition.FindBy(f => f.Item == item).FirstOrDefault();
                if (rec != null)
                {
                    TextBoxNewId.Text = rec.Id.ToString();
                    TextBoxNewItem.Text = rec.Item;
                    TextBoxNewDescription.Text = rec.Description;
                    CheckBoxNewScale.Checked = rec.Scale;
                    ComboBoxNewArea.SelectedValue = rec.AreaId;
                    TextBoxNewLocationMax.Text = rec.LocationMax.ToString();
                    TextBoxNewLocationMin.Text = rec.LocationMin.ToString();
                    TextBoxNewSystemMax.Text = rec.SystemMax.ToString();
                    TextBoxNewSystemMin.Text = rec.SystemMin.ToString();
                    TextBoxNewWeight.Text = rec.Weight.ToString("F4");
                    ComboBoxNewStorageType.SelectedValue = rec.StorageTypeId;
                    ComboBoxNewUnitOfIssue.SelectedValue = rec.UnitOfIssueId;
                    ComboBoxNewSizeCode.SelectedValue = rec.SizeCodeId;
                    ComboBoxNewVelocityCode.SelectedValue = rec.VelocityCodeId;
                    ComboBoxNewHeightCode.SelectedValue = rec.HeightCodeId;
                    MbNewAddLocation.Enabled = true;
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message7"));
                    MbNewAddLocation.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message8") + ex.Message + ex.InnerException.Message);
                TextBoxNewItem.Focus();
            }
        }
        private void MbNewAddLocation_Click(object sender, EventArgs e)
        {
            var id = IntegerExtensions.ParseInt((TextBoxNewId.Text));
            // CurrentItem = _repoItemDefinition.FindByKey(id);
            SetCurrentItemDefinition(id);
            if (CurrentItem != null)
            {
                TextBoxInventoryNewLocationsItem.Text = CurrentItem.Item;
                TextBoxInventoryNewLocationsDescription.Text = CurrentItem.Description;
                GetAvailableLocations(CurrentItem);
            }
            tabControl1.SelectedTab = tabPage4;
        }
        private void MbNewLocationsClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage2;
            // SetCurrentItemDefinition();
            LoadViewEdit();
        }
        private void MbNewLocationsListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void MbNewLocationsSave_Click(object sender, EventArgs e)
        {
            if (TextBoxInventoryNewLocationsQuantity.Text.ParseInt() == 0 &&
                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt() ==
                (int)StorageType.Release)
            {
                MessageBox.Show(_resourceManager.GetString("Message9"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                TextBoxInventoryNewLocationsQuantity.Focus();
            }
            else
            {
                if (_newLocationBindingSource.Count > 0)
                {
                    var locView = ((ObjectView<LocationView>)_newLocationBindingSource.Current).Object;
                    if (locView != null)
                    {
                        //var inv = _repoInventory
                        //    .FindBy(r => r.ItemDefinitionId == CurrentItem.Id && r.LocationId == locView.Id)
                        //    .FirstOrDefault();
                        //if (inv == null)
                        //{
                        var inventory = new Inventory()
                        {
                            ItemDefinitionId = CurrentItem.Id,
                            LocationId = locView.Id,
                            Quantity = TextBoxInventoryNewLocationsQuantity.Text.ParseInt(),
                            ReceivedDate = DateTimePickerInventoryNewLocationsReceivedDate.Value,
                            StorageTypeId =
                                ComboBoxInventoryNewLocationsStorageType.SelectedValue.ToString().ParseInt(),
                            PrimeBin = CheckBoxInventoryNewLocationsPrimeBin.Checked,
                            AreaId = CurrentItem.AreaId,
                            RFID = TextBoxInventoryNewLocationsRfid.Text
                        };
                        _repoInventory.Insert(inventory);
                        _historyManager.SaveHistory(ActionCode.InventoryAdd, inventory);
                        _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);

                        //TODO  fixed locationCode 

                        var locationCode = @"1234567890123456";


                        _locationsRepository.SetLocationCode(inventory.LocationId, locationCode);

                        SetCurrentInventoryItem(inventory);
                        LoadViewEdit();
                        tabControl1.SelectedTab = tabPage2;
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString("Message10"));
                }
            }
        }
        private IEnumerable<SqlInventoryView> GetInventoryViewListByItem(int itemId)
        {
            IEnumerable<SqlInventoryView> projection;
            using (var context = new NeutronDb())
            {
                var findItemId = new SqlParameter("@ItemId", itemId);
                projection = context.Database
                    .SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItem @ItemId", findItemId).ToList();
            }
            return projection;
        }
        private IEnumerable<SqlInventoryView> GetInventoryViewList()
        {
            IEnumerable<SqlInventoryView> projection;
            using (var context = new NeutronDb())
            {
                projection = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryView").ToList();
            }
            return projection;
        }
        private void TextBoxNewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SendKeys.Send("{Tab}");
            }
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
        private void MbAddDetailSave_Click(object sender, EventArgs e)
        {
            var inventory = new Inventory();
            var inventoryId = TextBoxAddDetailInventoryId.Text;
            if (!string.IsNullOrEmpty(inventoryId))
            {
                //update
                inventory = _repoInventory.FindByKey(IntegerExtensions.ParseInt(inventoryId));
                inventory.Quantity = IntegerExtensions.ParseInt((TextBoxAddDetailQuantity.Text));
                inventory.StorageTypeId = ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                _repoInventory.Update(inventory);
                _historyManager.SaveHistory(ActionCode.InventoryModify, inventory);
            }
            else
            {
                //new
                inventory.ItemDefinitionId = IntegerExtensions.ParseInt((TextBoxAddDetailItemDefinitionId.Text));
                inventory.LocationId = IntegerExtensions.ParseInt((TextBoxAddDetailLocationId.Text));
                inventory.Quantity = IntegerExtensions.ParseInt((TextBoxAddDetailQuantity.Text));
                inventory.StorageTypeId = ((NeutronData.Models.Lookups.StorageType)ComboBoxAddDetailStorageType.SelectedItem).Id;
                inventory.ReceivedDate = DateTimePickerAddDetailReceivedDate.Value;
                inventory.PrimeBin = CheckBoxAddDetailPrimeBin.Checked;
                inventory.AreaId = (int)ComboBoxAddDetailArea.SelectedValue;
                inventory.RFID = TextBoxAddDetailLocationCode.Text;
                _repoInventory.Insert(inventory);
                _historyManager.SaveHistory(ActionCode.InventoryAdd, inventory);
            }
            if (inventory.Quantity == 0 && inventory.StorageTypeId == (int)StorageType.Release)
            {
                DeleteInventoryItem(inventory.Id, releaseOnly: true);
            }
            else
            {
                _locationsRepository.SetLocationInUse(inventory.LocationId, b: true);
                TextBoxViewEditId.Text = CurrentItem.Id.ToString();
                TextBoxViewEditItem.Text = CurrentItem.Item;
                TextBoxViewEditDescription.Text = CurrentItem.Description;
            }
            var views = GetInventoryViewListByItem(CurrentItem.Id);
            var blv = new BindingListView<SqlInventoryView>(views.ToList());
            _locationBindingSource.DataSource = blv;
            DataGridViewInventoryLocations.DataSource = _locationBindingSource;
            tabControl1.SelectedTab = tabPage2;
        }
        private void MbAddDetailClose_Click(object sender, EventArgs e)
        {
            //LoadInventory();
            //SetCurrentItemDefinition();
            LoadViewEdit();
            tabControl1.SelectedTab = tabPage2;
        }
        private void MbAddDetailListing_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            LoadInventory();
            Cursor.Current = Cursors.Default;
            tabControl1.SelectedTab = tabPage1;
        }
        private void ClearAddDetailFields()
        {
            TextBoxAddDetailInventoryId.Text = "";
            TextBoxAddDetailLocationId.Text = "";
            TextBoxAddDetailQuantity.Text = "";
            TextBoxAddDetailItemDefinitionId.Text = "";
            ComboBoxAddDetailArea.SelectedValue = -1;
            TextBoxAddDetailLoc1.Text = "";
            TextBoxAddDetailLoc2.Text = "";
            TextBoxAddDetailLoc3.Text = "";
            TextBoxAddDetailLoc4.Text = "";
            TextBoxAddDetailLoc5.Text = "";
            TextBoxAddDetailSlot.Text = "";
            CheckBoxInUse.Checked = false;
            ComboBoxAddDetailStorageType.SelectedValue = -1;
            ComboBoxAddDetailSizeCode.SelectedValue = -1;
            ComboBoxAddDetailVelocityCode.SelectedValue = -1;
            ComboBoxAddDetailHeightCode.SelectedValue = -1;
        }
        private void ClearNewFields()
        {
            TextBoxNewId.Text = "";
            TextBoxNewItem.Text = "";
            TextBoxNewDescription.Text = "";
            CheckBoxNewScale.Checked = false;
            CheckBoxInUse.Checked = false;
            ComboBoxNewArea.SelectedValue = -1;
            TextBoxNewLocationMax.Text = "";
            TextBoxNewLocationMin.Text = "";
            TextBoxNewSystemMax.Text = "";
            TextBoxNewSystemMin.Text = "";
            TextBoxNewWeight.Text = "";
            ComboBoxNewStorageType.SelectedValue = -1;
            ComboBoxNewUnitOfIssue.SelectedValue = -1;
            ComboBoxNewSizeCode.SelectedValue = -1;
            ComboBoxNewVelocityCode.SelectedValue = -1;
            ComboBoxNewHeightCode.SelectedValue = -1;
        }
        public bool AllAvailable
        {
            get { return _allAvailable; }
            set
            {
                _allAvailable = value;
                MbNewAvailableLocations.Text = _allAvailable ? _resourceManager.GetString("Preferred") : _resourceManager.GetString("ShowAll");
            }
        }
        private void MbNewAvailableLocations_Click(object sender, EventArgs e)
        {
            AllAvailable = !_allAvailable;
            if (_allAvailable)
            {
                GetAllAvailableLocations(CurrentItem);
            }
            else
            {
                GetAvailableLocations(CurrentItem);
            }
        }
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ////if button cell position device, otherwise open View/Edit
            //var grid = (DataGridView) sender;
            //if (e.RowIndex <= 0) return;
            //else
            //{
            //    SetCurrentItemDefinition();
            //    TextBoxViewEditId.Text = CurrentItem.Id.ToString();
            //    TextBoxViewEditItem.Text = CurrentItem.Item;
            //    TextBoxViewEditDescription.Text = CurrentItem.Description;
            //    var views = (IEnumerable<SqlInventoryView>) GetInventoryViewListByItem(CurrentItem.Id);
            //    locationBindingSourceEquin = new BindingListView<SqlInventoryView>(views.ToList());
            //    locationBindingSource.DataSource = locationBindingSourceEquin;
            //    DataGridViewInventoryLocations.DataSource = locationBindingSource;
            //    GetRecordCount(locationBindingSource);
            //    tabControl1.SelectedTab = tabPage2;
            //}
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewPosition((DataGridView)sender, e.RowIndex);
            SetCurrentInventoryItem();
        }

        private void DataGridViewPosition(DataGridView grid, int rowIndex)
        {
            var qty = 0;
            var display = string.Empty;

            try
            {
                if (rowIndex < 0) return;
                if (!grid.Columns.Contains("Position")) return;
                if (grid.CurrentCell.ColumnIndex != grid.Columns["Position"].Index) return;
                if (!grid.Columns.Contains(columnName: "Loc1")) return;
                var deviceNumber = grid["Loc1", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc2")) return;
                var trayNumber = grid["Loc2", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc3")) return;
                var level = grid["Loc3", rowIndex].Value.ToString().ParseInt();
                if (!grid.Columns.Contains(columnName: "Loc4")) return;
                var part = grid["Loc4", rowIndex].Value.ToString().ParseInt();
                if (grid.Columns.Contains(columnName: "Quantity"))
                {
                    qty = grid["Quantity", rowIndex].Value.ToString().ParseInt();
                }

                //if (_workstationView.StationType.Id == (int)StationType.EBin)
                //{
                //    var eBinDisplayManager = new EBinDisplayManager();
                //    var response = eBinDisplayManager.TurnOnDisplay(deviceNumber, trayNumber, level, part, qty, display);
                //    MessageBox.Show($"{response}", "EBin Display Command", MessageBoxButtons.OK);
                //}
                MoveDevice(deviceNumber, trayNumber, level, part, qty, display);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to move device. {Environment.NewLine} {ex.Message}");
            }
        }

        private void MoveDevice(int deviceNumber, int trayNumber, int level, int part, int quantity = 0, string display = "")
        {
            if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, deviceNumber, trayNumber))
            {
                if (_neutronVariables.ShuttleEnabled)
                {
                    var hardwareDevice =
                        _workstationView.HardwareDevices.FirstOrDefault(s => s.DeviceNumber == deviceNumber);
                    if (hardwareDevice != null)
                    {
                        if (hardwareDevice.Enabled)
                        {
                            if (GlobalVar.Shuttle != null)
                            {
                                var response = Task.Run(() =>
                                    GlobalVar.Shuttle.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                        display));
                                if (response.Result != DeviceResponse.Success)
                                {
                                    MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                        caption: string.Empty, buttons: MessageBoxButtons.OK,
                                        icon: MessageBoxIcon.Error);
                                }
                            }
                            else if (GlobalVar.Hanel != null)
                            {
                                var response = Task.Run(() =>
                                    GlobalVar.Hanel.PositionDevice(deviceNumber, trayNumber, level, part, quantity,
                                        display));
                                if (response.Result != DeviceResponse.Success)
                                {
                                    MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                        caption: string.Empty, buttons: MessageBoxButtons.OK,
                                        icon: MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show(_resourceManager.GetString("Message11"));
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                $"{_resourceManager.GetString("Message12")} - {hardwareDevice.Name}");
                        }
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString("Message13"));
                    }
                }

                if (_neutronVariables.DisplaysEnabled)
                {
                    if (GlobalVar.Displays != null)
                    {
                        ClearAllShi();

                        var text = ($"{quantity.ToString().PadLeft(6, paddingChar: ' ')}");
                        GlobalVar.Displays.ShowShi(deviceNumber, trayNumber, level, part.ToString(), text);
                    }
                }

            }
            else
            {
                MessageBox.Show($"Location Access Denied");
            }
        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
                if (GlobalVar.Displays != null)
                    GlobalVar.Displays.ClearAllShi();
        }
        private void MBPrintInventory_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }
        private void DataGridViewInventoryLocations_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Click");
        }
        private void DataGridView1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Click");
        }
        private void CheckBoxAllStations_CheckedChanged(object sender, EventArgs e)
        {
            if (!_firstTime) FindRecord();
        }
        private void MBCreateInventoryFile_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MBCreateInventoryFile.Enabled = false;
            var areaId = ((Area)ComboBoxAreaNumber.SelectedItem).Id;
            CreateInventoryFileByArea(areaId);
            Cursor.Current = Cursors.Default;
            MBCreateInventoryFile.Enabled = true;
        }
        private void CreateInventoryFileByArea(int areaId)
        {
            var fileName = GetFileName();
            //Task.Run(() => CsvUtility.SaveToCsv(fileName, stationNumber));
            CsvUtility.SaveToCsv(fileName, areaId);
        }
        private string GetFileName()
        {
            var rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\CSV\");
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
            var sfd = new SaveFileDialog
            {
                InitialDirectory = rootDirectory,
                Filter = "CSV (*.csv)|*.csv",
                FileName = "Output.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return sfd.FileName;
            MessageBox.Show(_resourceManager.GetString("Message14"));
            if (!File.Exists(sfd.FileName)) return sfd.FileName;
            try
            {
                File.Delete(sfd.FileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message15") + ex.Message);
            }
            return sfd.FileName;
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmInventory",
               resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
                LabelFormTitle.Text = _resourceManager.GetString("Inventory");
                MBCreateInventoryFile.Text = _resourceManager.GetString("InventoryFile");
                CheckBoxAllStations.Text = _resourceManager.GetString("AllAreas");
                MBPrintInventory.Text = _resourceManager.GetString("SaveToFile");
                LabelFindDescription.Text = _resourceManager.GetString("SearchFor");
                MButtonNew.Text = _resourceManager.GetString("New");
                MButtonViewEdit.Text = _resourceManager.GetString("View/Edit");
                MButtonClose.Text = _resourceManager.GetString("Home");
                MButtonSearch.Text = _resourceManager.GetString("Search");
                MbViewEditEdit.Text = _resourceManager.GetString("EditLocation");
                MbViewEditAddLocation.Text = _resourceManager.GetString("AddLocation");
                MbViewEditListing.Text = _resourceManager.GetString("Listing");
                MbViewEditDelete.Text = _resourceManager.GetString("DeleteLocation");
                MbViewEditClose.Text = _resourceManager.GetString("Back");
                LabelViewEditDescription.Text = _resourceManager.GetString("Description");
                LabelViewEditItem.Text = _resourceManager.GetString("Item");
                MbNewAddLocation.Text = _resourceManager.GetString("AddLocation");
                LabelActionNew.Text = _resourceManager.GetString("NewInventoryItem");
                MbNewListing.Text = _resourceManager.GetString("Listing");
                MbNewClose.Text = _resourceManager.GetString("Back");
                MbNewFind.Text = _resourceManager.GetString("Find");
                LabelNewItem.Text = _resourceManager.GetString("EnterItemNumber");
                LabelNewUnitOfIssue.Text = _resourceManager.GetString("UnitOfIssue");
                LabelNewStorageType.Text = _resourceManager.GetString("StorageType");
                LabelNewWeight.Text = _resourceManager.GetString("Weight");
                CheckBoxNewScale.Text = _resourceManager.GetString("UseScale");
                LabelNewSystemMin.Text = _resourceManager.GetString("SystemMin");
                LabelNewLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelNewHeight.Text = _resourceManager.GetString("Height");
                LabelNewVelocity.Text = _resourceManager.GetString("Velocity");
                LabelNewSize.Text = _resourceManager.GetString("Size");
                LabelNewSystemMax.Text = _resourceManager.GetString("SystemMax");
                LabelNewLocationMin.Text = _resourceManager.GetString("LocationMin");
                LabelNewLocationMax.Text = _resourceManager.GetString("LocationMax");
                LabelNewDescriiption.Text = _resourceManager.GetString("Description");
                LabelNewArea.Text = _resourceManager.GetString("Area");
                LabelActionNewLocations.Text = _resourceManager.GetString("NewLocations");
                MbNewLocationsListing.Text = _resourceManager.GetString("Listing");
                MbNewAvailableLocations.Text = _resourceManager.GetString("ShowAll");
                MbNewLocationsClose.Text = _resourceManager.GetString("Back");
                MbNewLocationsSave.Text = _resourceManager.GetString("Save");
                CheckBoxInventoryNewLocationsPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                LabelNewLocationsStorageType.Text = _resourceManager.GetString("StorageType");
                LabelNewLocationsReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                LabelNewLocationsQuantity.Text = _resourceManager.GetString("Qty");
                LabelNewLocationsDescription.Text = _resourceManager.GetString("Description");
                LabelNewLocationsItem.Text = _resourceManager.GetString("Item");
                LabelActionAddDetail.Text = _resourceManager.GetString("AddDetail");
                MbAddDetailListing.Text = _resourceManager.GetString("Listing");
                MbAddDetailClose.Text = _resourceManager.GetString("Back");
                MbAddDetailSave.Text = _resourceManager.GetString("Save");
                CheckBoxAddDetailPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                LabelAddDetailReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                LabelAddDetailDescription.Text = _resourceManager.GetString("Description");
                LabelAddDetailItem.Text = _resourceManager.GetString("Item");
                LabelAddDetailStorageType.Text = _resourceManager.GetString("StorageType");
                LabelAddDetailQuantity.Text = _resourceManager.GetString("Qty");
                GroupBoxAddDetailLocation.Text = _resourceManager.GetString("Location");
                CheckBoxInUse.Text = _resourceManager.GetString("InUse");
                LabelLocationCode.Text = _resourceManager.GetString("LocationCode");
                LabelAddDetailHeight.Text = _resourceManager.GetString("Height");
                LabelAddDetailVelocity.Text = _resourceManager.GetString("Velocity");
                LabelAddDetailSlot.Text = _resourceManager.GetString("Slot");
                LabelAddDetailSize.Text = _resourceManager.GetString("Size");
                LabelAddDetailTag.Text = _resourceManager.GetString("Tag");
                LabelAddDetailBack.Text = _resourceManager.GetString("Back");
                LabelAddDetailOver.Text = _resourceManager.GetString("Over");
                LabelAddDetailTray.Text = _resourceManager.GetString("Tray");
                LabelAddDetailDevice.Text = _resourceManager.GetString("Device");
                LabelAddDetailArea.Text = _resourceManager.GetString("Area");
                LabelSlotInformation.Text = _resourceManager.GetString("EnterSlotDescription");
                ButtonPositionDevice.Text = _resourceManager.GetString("PositionDevice");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        private void ButtonPositionDevice_Click(object sender, EventArgs e)
        {
            var deviceNumber = TextBoxAddDetailLoc1.Text.ParseInt();
            var trayNumber = TextBoxAddDetailLoc2.Text.ParseInt();
            var level = TextBoxAddDetailLoc3.Text.ParseInt();
            var part = TextBoxAddDetailLoc4.Text.ParseInt();
            var qty = TextBoxAddDetailQuantity.Text.ParseInt();
            Task.Run(() =>
                _logger.LogDetailAsync(
                    $"Device: {deviceNumber} Tray: {trayNumber} Level: {level} Part: {part}"));
            MoveDevice(deviceNumber, trayNumber, level, part, qty);
        }

        private void DataGridView1_DoubleClick(object sender, EventArgs e)
        {
            LoadViewEdit();
        }
    }
}
