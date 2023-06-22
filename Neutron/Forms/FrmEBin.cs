using AlliedLogger;
using DeviceIndicatorService;
using Equin.ApplicationFramework;
using JsonManager;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static Neutron.Forms.FrmHotAction;

namespace Neutron.Forms
{
    public partial class FrmEBin : Form
    {
        private readonly IJsonData _jsonData;
        private readonly IImageManager _imageManager;
        private readonly IWorkstationRepository _workstationRepository;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly WorkstationView _workstationView;
        private readonly HistoryManager _historyManager;
        private readonly string _item;
        private readonly int _quantity;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private ResourceManager _enumResourceManager;
        private ResourceManager _gridResourceManager;
        private string _imagesDirectory;
        private LocationsRepository _locationsRepository;
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private InventoryManager _inventoryManager;
        private HeaderTextManager _headerTextManager;
        private DynamicLogger _logger;
        private BindingSource _bindingSourceCurrent = new BindingSource();
        private BindingSource _bindingSourceItemDefinitions = new BindingSource();
        private BindingSource _bindingSourceNewLocations = new BindingSource();
        private ItemDefinitionView _currentItemDefinition;
        public bool CloseButtonPressed { get; set; }
        public FrmEBin(IJsonData jsonData, IImageManager imageManager
            , IWorkstationRepository workstationRepository, IItemDefinitionsRepository itemDefinitionsRepository
            , NeutronVariables neutronVariables, NeutronLicense neutronLicense
            , WorkstationView workstationView, HistoryManager historyManager
            , string item = "", int quantity = 1)
        {
            _jsonData = jsonData;
            _imageManager = imageManager;
            _workstationRepository = workstationRepository;
            _itemDefinitionsRepository = itemDefinitionsRepository;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _historyManager = historyManager;
            _item = item;
            _quantity = quantity;
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            SetupLogger();
        }

        private void MBHotActionClose_Click(object sender, EventArgs e)
        {
            //Task.Run(() => _logger.LogDetailAsync("Hot Action Close Button Pressed"));
            CloseButtonPressed = true;
        }
        private void SetupLogger()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = $"EBin-{_workstationView.Name}";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }
        private void InitForm()
        {
            KeyPreview = true;

            SetupGridItemDefinition();

            //LabelFormTitle.Text = _resourceManager.GetString("HotActions");
            //LabelFormTitle.BackColor = Color.Red;
            //mlUserInfo.Text = GlobalVar.User?.UserInfo;

            CloseButtonPressed = false;
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
            _locationsRepository = new LocationsRepository();
            //FillComboBoxes();
            _inventoryManager = new InventoryManager(_repoInventory, _locationsRepository, _historyManager);
            //InitialSearch(_item);
            //LabelStationName.Text = _workstationView.Name;
            //LabelStationName2.Text = _workstationView.Name;
            //if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Carousel
            //    || _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Vertical
            //    || _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.EBin)
            //{
            //    TextBoxScanLocation.Visible = true;
            //}
            //else
            //{
            //    TextBoxScanLocation.Visible = false;
            //    _newLocationButtonText = _resourceManager.GetString("AllLocations");
            //    MBNewLocations.Text = _newLocationButtonText;
            //}

                        // Display the Device Indicator
                        // And the Tray Layout
                       // InitDeviceIndicators();
                        //BuildTrayLayout();
                        //LabelFormTitle.Visible = false;
                        //LabelFormHeaderText.Text = "Neutron EBin";
                        MBHotPick.Text = "Pick";
                        MBHotStore.Text = "Store";
                        TextBoxFindItem.Focus();

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
            Task.Run(() => _logger.LogDetailAsync($"Find Hot Record: {findWhat} START"));
            try
            {
                //if (!string.IsNullOrEmpty(findWhat))
                //{
                //    var akaFind = _akaRepository.Get(findWhat);
                //    TextBoxFindItem.Text = akaFind;
                //}
                //else
                //{
                //    TextBoxFindItem.Text = findWhat;
                //}
                LoadItemDefinitions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("Message3") + ex.Message);
            }
            Task.Run(() => _logger.LogDetailAsync("Find Hot Record: {findWhat} End"));
        }
        private void SetupGridItemDefinition()
        {
            _logger.LogDetailAsync($"SetupGridItemDefinition 1");
            //if (_currentGridDataType == GridDataType.Item) return;
            DataGridViewEBin.Columns.Clear();
            //_currentGridDataType = GridDataType.Item;
            DataGridViewEBin.AutoGenerateColumns = false;
            DataGridViewEBin.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _logger.LogDetailAsync($"SetupGridItemDefinition 2");
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationName",
                HeaderText = _gridResourceManager.GetString("StationName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "StationName"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 3");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString("Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 4");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString("Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Description"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 5");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitOfIssueName",
                HeaderText = _gridResourceManager.GetString("UnitOfIssueName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "UnitOfIssueName"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 6");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString("LocationMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMax"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 7");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString("SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 8");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString("VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 9");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString("HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "HeightCodeName"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 10");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMin",
                HeaderText = _gridResourceManager.GetString("LocationMin"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMin"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 11");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = _gridResourceManager.GetString("SystemMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMax"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 12");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = _gridResourceManager.GetString("SystemMin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMin"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition 13");
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString("Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewEBin.Columns.Add(col);

            _logger.LogDetailAsync($"SetupGridItemDefinition Start Column Formatting");

            DataGridViewEBin.EnableHeadersVisualStyles = false;
            DataGridViewEBin.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewEBin.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //foreach (DataGridViewColumn column in DataGridViewEBin.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            _logger.LogDetailAsync($"SetupGridItemDefinition End Column Formatting");
        }
        private void FrmHotAction_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }
        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmEBin",
               resourceDir: languageDirectory, usingResourceSet: null);
                _enumResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "EnumDescriptions",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);

                MBHotStore.Text = _resourceManager.GetString("HotStore");
                MBHotPick.Text = _resourceManager.GetString("HotPick");
                MBHotActionClose.Text = _resourceManager.GetString("Close");
                MBFindItem.Text = _resourceManager.GetString("Search");
              
                
                //HotPick.Text = _resourceManager.GetString("HotPick");
                //CheckBoxAll.Text = _resourceManager.GetString("All");
                //LabelSearch.Text = _resourceManager.GetString("SearchforanypartofIt");
                //MBNewLocations.Text = _resourceManager.GetString("NewLocations");
                //MBCurrentLocations.Text = _resourceManager.GetString("CurrentLocations");
                //HotAction.Text = _resourceManager.GetString("HotAction");
                //ButtonEditItemDefinition.Text = _resourceManager.GetString("Edit");
                //LabelMainHeight.Text = _resourceManager.GetString("Height");
                //LabelMainVelocity.Text = _resourceManager.GetString("Velocity");
                //LabelMainSize.Text = _resourceManager.GetString("Size");
                //LabelMainUnitOfIssue.Text = _resourceManager.GetString("UnitofIssue");
                //GroupBoxHotActions.Text = _resourceManager.GetString("TransactionType");
                //RadioButtonCostCenter.Text = _resourceManager.GetString("CostCenter");
                //RadioButtonOther.Text = _resourceManager.GetString("Other");
                //RadioButtonScrap.Text = _resourceManager.GetString("Scrap");
                //RadioButtonWarranty.Text = _resourceManager.GetString("Warranty");
                //RadioButtonPick.Text = _resourceManager.GetString("Pick");
                //LabelMainQuantity.Text = _resourceManager.GetString("Qty");
                //LabelMainItem.Text = _resourceManager.GetString("Item");
                //LabelMainDescription.Text = _resourceManager.GetString("Desc");
                //GroupBoxHotPickLocation.Text = _resourceManager.GetString("Location");
                //LabelLocationCode.Text = _resourceManager.GetString("LocationCode");
                //ButtonEditLocationDefinition.Text = _resourceManager.GetString("Edit");
                //LabelHeight.Text = _resourceManager.GetString("Height");
                //LabelVelocity.Text = _resourceManager.GetString("Velocity");
                //LabelSize.Text = _resourceManager.GetString("Size");
                //LabelReceivedDate.Text = _resourceManager.GetString("ReceivedDate");
                //LabelStaticRelease.Text = _resourceManager.GetString("StaticLocation");
                //LabelPrimeBin.Text = _resourceManager.GetString("PrimeBin");
                //LabelLocationQuantity.Text = _resourceManager.GetString("LocationQuantity");
                //LabelBack.Text = _resourceManager.GetString("Back");
                //LabelOver.Text = _resourceManager.GetString("Over");
                //LabelTray.Text = _resourceManager.GetString("Tray");
                //LabelDevice.Text = _resourceManager.GetString("Device");
                //MBHotActionCount.Text = _resourceManager.GetString("LocationCount");
                //MBHotAccept.Text = _resourceManager.GetString("Accept");
                //MBHotActionBack.Text = _resourceManager.GetString("Back");
                //LabelFormTitle.Text = _resourceManager.GetString("Jobs");
                //mlUserInfo.Text = _resourceManager.GetString("Login?");
                //LabelFormHeaderText.Text = _resourceManager.GetString("NeutronWarehouseMana");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        private void ButtonHotPickClear_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.LogDetailAsync("Hot Pick Clear START"));
            Cursor.Current = Cursors.WaitCursor;
            _bindingSourceCurrent.DataSource = null;
            //MBCurrentLocations.Text = $"{_resourceManager.GetString("CurrentLocations")} ({_bindingSourceCurrent.Count})";
            _bindingSourceNewLocations.DataSource = null;
           // MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            TextBoxFindItem.Text = string.Empty;
            if (_workstationView.StationType.Name != "EBin")
            {
                LoadItemDefinitions();
            }


            TextBoxFindItem.Focus();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync("Hot Pick Clear END"));
        }
        private async Task LoadItemDefinitions(string find = @"", int recId = 0)
        {
            Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Find: {find}  START"));
            BindingListView<ItemDefinitionView> blv = null;
            Cursor.Current = Cursors.WaitCursor;
            _logger.LogDetailAsync($"Load Item Definitions SetupGridItemDefinition Start ");
            SetupGridItemDefinition();
            _logger.LogDetailAsync($"Load Item Definitions SetupGridItemDefinition End");
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            var idx = 0;
            var findWhat = string.IsNullOrEmpty(find) ? TextBoxFindItem.Text.ToLower().Trim() : find;

            IEnumerable<ItemDefinitionView> views;
            _logger.LogDetailAsync($"Load Item Definitions Check Workstation Type ");
          _logger.LogDetailAsync($"Load Item Definitions NOT a Supervisor Workstation ");
                _logger.LogDetailAsync($"Load Item Definitions Call FindItemDefinitionViewsByWorkstation  ");
                _logger.LogDetailAsync($"Load Item Definitions Passing in findWhat: {findWhat}  and Workstation: {_workstationView.WorkstationId} ");
                views = _itemDefinitionsRepository.FindItemDefinitionViewsByWorkstation(findWhat, _workstationView.WorkstationId).ToList();
                _logger.LogDetailAsync($"Load Item Definitions Back with Views.  Setting them to BindingListView ");

                //if (!views.Any() && !string.IsNullOrEmpty(findWhat))
                //{
                //    var akaFind = _akaRepository.Get(findWhat);
                //    TextBoxFindItem.Text = akaFind;
                //    views = _itemDefinitionsRepository.FindItemDefinitionViewsByWorkstation(akaFind, _workstationView.WorkstationId).ToList();
                //    _logger.LogDetailAsync($"Load Item Definitions Back with Views Using AKA Find.  Setting them to BindingListView ");
                //}

                blv = new BindingListView<ItemDefinitionView>(views.ToList());
                _logger.LogDetailAsync($"Load Item Definitions BLV created ");
            

            _logger.LogDetailAsync($"Load Item Definitions Create a new BindingSource using BLV as DataSource ");
            _logger.LogDetailAsync($"Load Item Definitions called _bindingSourceItemDefinitions");
            _bindingSourceItemDefinitions = new BindingSource { DataSource = blv };
            _logger.LogDetailAsync($"Load Item Definitions New BindingSource has been created");
            _logger.LogDetailAsync($"Load Item Definitions Now set the DataGridViewHot.DataSource = to the new Bindingsource, _bindingSourceItemDefinitions ");
            DataGridViewEBin.DataSource = _bindingSourceItemDefinitions;
            DataGridViewEBin.Update();
            _logger.LogDetailAsync($"Load Item Definitions Update the Grid ");
            //UpdateDataGrid(_bindingSourceItemDefinitions);
            _logger.LogDetailAsync($"Load Item Definitions Get the Record Count");
            var recordCount = GetRecordCount(_bindingSourceItemDefinitions);
            if (recordCount > 0)
            {
                _logger.LogDetailAsync($"Load Item Definitions Record count is greater that zero ");
                //if (recId != 0)
                //{
                //    _logger.LogDetailAsync($"Load Item Definitions if passed in recId is not zero ");
                //    _logger.LogDetailAsync($"Load Item Definitions Set the bindingSource to the recId ");
                //    idx = IndexOf(_bindingSourceItemDefinitions, recId);
                //}
                try
                {
                    _logger.LogDetailAsync($"Load Item Definitions Set the row index to {idx} ");
                    DataGridViewEBin.FirstDisplayedScrollingRowIndex = idx;
                    DataGridViewEBin.Update();
                    DataGridViewEBin.CurrentCell = DataGridViewEBin.Rows[idx].Cells[1];
                    DataGridViewEBin.Rows[idx].Selected = true;
                    _logger.LogDetailAsync($"Load Item Definitions Row set and highlight complete ");
                    _logger.LogDetailAsync($"Load Item Definitions Set the Current Item Definition ");
                    _currentItemDefinition =
                         ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    _logger.LogDetailAsync($"Load Item Definitions Current Item: {_currentItemDefinition.Item} ");
                    _logger.LogDetailAsync($"Load Item Definitions If record count = 1 then call LoadCurrentAndNew ");
                   // if (recordCount == 1) await LoadCurrentAndNew();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}");
                   // _currentGridDataType = GridDataType.None;
                    SetupGridItemDefinition();
                }
            }
            else
            {
                var item = _repoItemDefinition.FindBy(r => r.Item == findWhat).FirstOrDefault();
                if (item != null)
                {
                    MessageBox.Show($"{item.Item} {_resourceManager.GetString("Message0")} {item.AreaId}");
                }
               // ClearCurrentAndNew();
            }
            DataGridViewEBin.ClearSelection();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Find: {find}  END"));
        }
        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
           // LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count.ToString()}";
            return count;
        }
        private int GetRecordCount(IReadOnlyCollection<object> bs)
        {
            var count = bs.Count;
           // LabelRecordCount.Text = $"{_resourceManager.GetString("Records")}: {count.ToString()}";
            return count;
        }

        private void ButtonClearFindItem_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.LogDetailAsync("Hot Pick Clear START"));
            Cursor.Current = Cursors.WaitCursor;
            _bindingSourceCurrent.DataSource = null;
           // MBCurrentLocations.Text = $"{_resourceManager.GetString("CurrentLocations")} ({_bindingSourceCurrent.Count})";
            _bindingSourceNewLocations.DataSource = null;
          //  MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            TextBoxFindItem.Text = string.Empty;
            if (_workstationView.StationType.Name != "EBin")
            {
                LoadItemDefinitions();
            }


            TextBoxFindItem.Focus();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync("Hot Pick Clear END"));
        }
    }
}
