using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using AsyncAwaitBestPractices;
using CurrentDeviceIndicator;
using DeviceIndicatorService;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Enums;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using Neutron.Ninject;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using NeutronDllu;
using NeutronTrayLayout;
using DeviceTypeEnum = NeutronCore.Enums.DeviceTypeEnum;
using StorageType = Neutron.Enums.StorageType;
using StationType = NeutronCore.Enums;
using NeutronEvents;
using IPTI.Models;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;
using System.Text;
using NeutronData.UnitOfWorks;

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

        private readonly GenericRepository<VelocityCode> _repoVelocityCode =
            new GenericRepository<VelocityCode>(new NeutronDb());

        private readonly GenericRepository<HeightCode> _repoHeightCode =
            new GenericRepository<HeightCode>(new NeutronDb());

        private readonly GenericRepository<Inventory>
            _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails =
            new GenericRepository<ReplenOrderDetail>(new NeutronDb());

        //private readonly LocationsRepository _repoLocation = new LocationsRepository();
        //private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        // private readonly IWorkstationRepository _workstationRepository;
        private readonly IHistoryManager _historyManager;

        private ILocationsRepository _locationsRepository;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;
        private readonly IIptiDisplayFunctions _iptiDisplayFunctions;
        private readonly IInventoryRepository _inventoryRepository;

        private InventoryRepository _repoInv;

        //private readonly ItemDefinitionsRepository _itemDefinitionsRepository = new ItemDefinitionsRepository();
        private BindingSource _bindingSourceCurrent = new BindingSource();
        private BindingSource _bindingSourceItemDefinitions = new BindingSource();
        private BindingSource _bindingSourceNewLocations = new BindingSource();
        public bool CloseButtonPressed { get; set; }

        private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;

        private IDynamicLogger _logger;
        readonly WorkstationView _workstationView;
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
        private string _costCenterCode = string.Empty;
        private ActionCode _costCenterActionCode = ActionCode.PickHot;

        private InventoryManager _inventoryManager;
        private string _newLocationButtonText = "Locations";
        private Dictionary<int, DeviceIndicator> _deviceIndicators;
        private DeviceIndicatorManager _deviceIndicatorManager;

        private NeutronTrayManager _neutronTrayManager;

        //private readonly int[] _moveableDeviceTypes;
        private string _item;
        private int _quantity;
        private int _quantityToPick;
        private readonly PickList _pickList;
        private LabelPrinterPreferences _labelPrinter;
        private HeaderTextManager _headerTextManager;
        private TcpIptiCommandCenter _tcpIptiCommandCenter;

        private int _currentPage;

        public string Item
        {
            get { return _item; }
            set { _item = value; }
        }

        public enum GridDataType
        {
            None,
            Item,
            Current,
            New
        }

        public delegate void UpdateDataGridDelegate(BindingSource bindingSource);

        public FrmHotAction(IJsonData jsonData, IAkaRepository akaRepository
            , ILacProcessor lacProcessor, IImageManager imageManager
            , IItemDefinitionsRepository itemDefinitionsRepository
            , NeutronVariables neutronVariables, NeutronLicense neutronLicense
            , WorkstationView workstationView, IHistoryManager historyManager, ILocationsRepository locationsRepository
            , IInventoryUnitOfWork inventoryUnitOfWork
            , IIptiDisplayFunctions iptiDisplayFunctions, IInventoryRepository inventoryRepository
            , string item = "", int quantity = 1, PickList pickList = null)
        {

            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            //_workstationRepository = workstationRepository;

            _workstationView = workstationView;
            _historyManager = historyManager;
            _locationsRepository = locationsRepository;
            _inventoryUnitOfWork = inventoryUnitOfWork;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _itemDefinitionsRepository = itemDefinitionsRepository;
            _akaRepository = akaRepository;

            _iptiDisplayFunctions = iptiDisplayFunctions;
            _inventoryRepository = inventoryRepository;


            _currentPage = 1;
            _pickList = pickList;
            _item = item;
            _quantity = quantity;
            _quantityToPick = quantity;
            InitForm();

            if (_tcpIptiCommandCenter == null)
            {
                _tcpIptiCommandCenter = new TcpIptiCommandCenter(_jsonData, _logger, _workstationView);
            }

            Mediator.GetInstance().WorkItMessageChng += FrmHotAction_WorkItMessageChng;
        }

        private void InitForm()
        {
            // _pickList is a value passed in from another process like Picking
            if (_pickList == null)
            {
                KeyPreview = true;
                _headerTextManager = new HeaderTextManager();
                _logger = NeutronCore.Global.Logger.SetupLogger("HotAction");
                //SetupLogger();
                _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();

                SetupGridItemDefinition();
                _repoInv = new InventoryRepository(_logger);
                _useCostCenter = _neutronVariables.UseCostCenter;
                LabelFormTitle.Text = _resourceManager.GetString($"HotActions");
                LabelFormTitle.BackColor = Color.Red;
                HideTabControlTabs();
                mlUserInfo.Text = GlobalVar.User?.UserInfo;
                CloseButtonPressed = false;
                _imagesDirectory = LoaderSettings.GetImagesDirectory();
                FillComboBoxes();
                _inventoryManager = new InventoryManager(_inventoryUnitOfWork, _locationsRepository);
                InitialSearch(_item);
                LabelStationName.Text = _workstationView.ToString();
                LabelStationName2.Text = _workstationView.ToString();
                TextBoxGoTo.Text = _currentPage.ToString();
                //_blastzones = _workstationView.HardwareDevices
                //    .Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.Blastzone).ToList();
                //_blastzone = _blastzones.Any();

                //_batchTables =
                //    _workstationView.HardwareDevices.FirstOrDefault(r =>
                //        r.DeviceTypeId == (int)DeviceTypeEnum.IptiDisplays);
                //_batchTable = _batchTables != null;

                //var prolites = _workstationView.HardwareDevices
                //    .Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.ProLite).ToList();
                //_prolite = prolites.Any();

                //var hanels = _workstationView.HardwareDevices.Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.Hanel12D)
                //    .ToList();
                //_hanel = hanels.Any();


                if (_workstationView.AreaId == 8)
                {
                    // TextBoxScanLocation.Visible = true;
                    //  LabelScanLocation.Visible = true;
                }
                else
                {
                    TextBoxScanLocation.Visible = false;
                    LabelScanLocation.Visible = false;
                    _newLocationButtonText = _resourceManager.GetString($"AllLocations");
                    MBNewLocations.Text = _newLocationButtonText;
                }



                //if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Carousel
                //    || _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Vertical)
                //{
                //    TextBoxScanLocation.Visible = true;
                //}
                //else
                //{
                //    TextBoxScanLocation.Visible = false;
                //    _newLocationButtonText = _resourceManager.GetString($"AllLocations");
                //    MBNewLocations.Text = _newLocationButtonText;
                //}


                // Check for the type of Workstation in order to determine
                // the proper location indicator
                // Is it a Carousel, Vertical, or Rack
                switch (_workstationView.StationType.Name)
                {
                    case "Carousel":
                        {
                            // Display the Device Indicator
                            // And the Tray Layout
                            InitDeviceIndicators();
                            //BuildTrayLayout();
                            break;
                        }
                    case "Vertical":
                        {
                            // Display the Device Indicator
                            // And the Tray Layout
                            InitDeviceIndicators();
                            //BuildTrayLayout();
                            break;
                        }
                    case "Rack-Tablet":
                        {
                            InitDeviceIndicators();
                            // Just the Tray Layout
                            break;
                        }
                    case "Blastzone":
                        {
                            InitDeviceIndicators();
                            // Just the Tray Layout
                            break;
                        }
                        //case "EBin":
                        //{
                        //    // Display the Device Indicator
                        //    // And the Tray Layout
                        //    InitDeviceIndicators();
                        //    //BuildTrayLayout();
                        //    LabelFormTitle.Visible = false;
                        //    LabelFormHeaderText.Text = "Neutron EBin";
                        //    MBHotPick.Text = "Pick";
                        //    MBHotStore.Text = "Store";
                        //    TextBoxFindItem.Focus();
                        //    break;
                        //}

                }
            }
            else
            {
                _item = _pickList.Item;
                _quantity = _pickList.Ordered.ParseInt();
                _quantityToPick = _pickList.Ordered.ParseInt();
                KeyPreview = true;
                SetupLogger();
                SetupGridItemDefinition();
                InitDeviceIndicators();
                _useCostCenter = _neutronVariables.UseCostCenter;
                LabelFormTitle.Text = _resourceManager.GetString($"HotActions");
                LabelFormTitle.BackColor = Color.Green;
                HideTabControlTabs();
                mlUserInfo.Text = GlobalVar.User?.UserInfo;
                CloseButtonPressed = false;
                _imagesDirectory = LoaderSettings.GetImagesDirectory();
                FillComboBoxes();
                _inventoryManager = new InventoryManager(_inventoryUnitOfWork, _locationsRepository);
                InitialSearch(_item);
                LabelStationName.Text = _workstationView.ToString();
                LabelStationName2.Text = _workstationView.ToString();
                if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Carousel
                    || _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Vertical) return;

                _newLocationButtonText = _resourceManager.GetString($"AllLocations");
                MBNewLocations.Text = _newLocationButtonText;
                MBHotPick.Visible = false;
                TextBoxFindItem.ReadOnly = true;
                ButtonClearFindItem.Visible = false;
                MBFindItem.Visible = false;
                MBHotActionCount.Visible = false;

            }
        }

        private void BuildTrayLayout(int maxColumns, int maxRows, int loc3, int loc4, int columnSpan = 1,
            int rowSpan = 1)
        {

            if (HotAction.Controls.ContainsKey("TrayLayout"))
            {
                HotAction.Controls.RemoveByKey("TrayLayout");
            }

            _logger.LogDetail("Build TrayLayout");
            var panel = new Panel();
            panel.Size = new Size(525, 235);
            panel.Location = new Point(470, 10);
            panel.Name = "TrayLayout";
            _neutronTrayManager = new NeutronTrayManager(panel, 0, 0, maxColumns, maxRows, loc3, loc4, 1, 1);

            HotAction.Controls.Add(_neutronTrayManager.TrayLayout);

        }

        private void FrmHotAction_WorkItMessageChng(object sender, WorkItEventArgs e)
        {
            var msg = e.Message;
            UpdateWorkItMessage(msg);
        }

        public void UpdateWorkItMessage(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateWorkItMessage), message);
            }
            else
            {
                MessageBox.Show(message);
                //TextBoxCurrentTray.Text = message;
            }
        }
        /// <summary>
        /// Gets the creation parameters for the current window.
        /// </summary>
        /// <remarks>
        /// This property overrides the base class property to modify the extended style of the window.
        /// The WS_EX_COMPOSITED style is added to enable double buffering in the window to reduce flicker.
        /// </remarks>
        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }
        /// <summary>
        /// Initializes the device indicators for the current workstation view.
        /// </summary>
        /// <remarks>
        /// This method creates a new instance of the DeviceIndicatorManager and adds the device indicator panel to the HotAction controls.
        /// If the panel already exists in the HotAction controls, the method will return without making any changes.
        /// </remarks>
        private void InitDeviceIndicators()
        {
            if (HotAction.Controls.ContainsKey("PanelDeviceIndicators")) return;

            _logger.LogDetailAsync("Initialize Device Indicators - InitDeviceIndicators").SafeFireAndForget();

            _deviceIndicatorManager = new DeviceIndicatorManager(_workstationView, new Point(10, 10),
                new Size(450, 235), _neutronVariables);
            if (_deviceIndicatorManager?.DeviceIndicatorPanel == null)
            {
                // Log an error or throw an exception
                _logger.LogDetailAsync("DeviceIndicatorPanel is null").SafeFireAndForget();
                return;
            }
            HotAction.Controls.Add(_deviceIndicatorManager.DeviceIndicatorPanel);
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

        //private void FillCostCenterComboBox()
        //{
        //    if (!_useCostCenter) return;
        //    var costCenterPath = LoaderSettings.GetCostCenterPath();
        //    _costCenterManager = new CostCenterManager(costCenterPath);
        //    var costCenterList = _costCenterManager.GetCostCenterListAsync();
        //    ComboBoxCostCenter.DataSource = costCenterList;
        //    ComboBoxCostCenter.DisplayMember = "Name";
        //    ComboBoxCostCenter.ValueMember = "Code";
        //}

        private void FillComboBoxes()
        {
            var sizeCodes = _repoSizeCode.All();
            var velocityCodes = _repoVelocityCode.All();
            var heightCodes = _repoHeightCode.All();

            ComboBoxSizeCodeItem.DataSource = sizeCodes; // _repoSizeCode.All();
            ComboBoxSizeCodeItem.DisplayMember = "Name";
            ComboBoxSizeCodeItem.ValueMember = "Id";
            ComboBoxVelocityCodeItem.DataSource = velocityCodes; // _repoVelocityCode.All();
            ComboBoxVelocityCodeItem.DisplayMember = "Name";
            ComboBoxVelocityCodeItem.ValueMember = "Id";
            ComboBoxHeightCodeItem.DataSource = heightCodes; // _repoHeightCode.All();
            ComboBoxHeightCodeItem.DisplayMember = "Name";
            ComboBoxHeightCodeItem.ValueMember = "Id";
            ComboBoxSizeCodeLocation.DataSource = sizeCodes; // _repoSizeCode.All();
            ComboBoxSizeCodeLocation.DisplayMember = "Name";
            ComboBoxSizeCodeLocation.ValueMember = "Id";
            ComboBoxVelocityCodeLocation.DataSource = velocityCodes; //  _repoVelocityCode.All();
            ComboBoxVelocityCodeLocation.DisplayMember = "Name";
            ComboBoxVelocityCodeLocation.ValueMember = "Id";
            ComboBoxHeightCodeLocation.DataSource = heightCodes; //  _repoHeightCode.All();
            ComboBoxHeightCodeLocation.DisplayMember = "Name";
            ComboBoxHeightCodeLocation.ValueMember = "Id";
            //Tray
            ComboBoxSizeCodeLocationTray.DataSource = sizeCodes; // _repoSizeCode.All();
            ComboBoxSizeCodeLocationTray.DisplayMember = "Name";
            ComboBoxSizeCodeLocationTray.ValueMember = "Id";
            ComboBoxVelocityCodeLocationTray.DataSource = velocityCodes; //  _repoVelocityCode.All();
            ComboBoxVelocityCodeLocationTray.DisplayMember = "Name";
            ComboBoxVelocityCodeLocationTray.ValueMember = "Id";
            ComboBoxHeightCodeLocationTray.DataSource = heightCodes; //  _repoHeightCode.All();
            ComboBoxHeightCodeLocationTray.DisplayMember = "Name";
            ComboBoxHeightCodeLocationTray.ValueMember = "Id";

        }

        private void SetupLogger()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = $"HotAction";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        /// <summary>
        /// Gets all the Inventory Locations for the current Item Definition
        /// and puts them into the _bindingSourceCurrent
        /// Uses the Form level variable, _currentItemDefinition
        /// </summary>
        private void LoadCurrent()
        {
            _logger.LogDetailAsync($"Load Current By Item: {_currentItemDefinition.Item}  START").SafeFireAndForget();
            var recs = _repoInv.GetAllInventoryViewsByItemDefinitionId(_currentItemDefinition.Id).ToList();
            var blv = new BindingListView<SqlInventoryView>(recs);
            _bindingSourceCurrent.DataSource = blv;
            GetRecordCount(recs);
            MBCurrentLocations.Text =
                $"{_resourceManager.GetString($"CurrentLocations")} ({_bindingSourceCurrent.Count})";

            _logger.LogDetailAsync($"Load Current By Item: {_currentItemDefinition.Item}  END").SafeFireAndForget();
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
                Origin = $"  ",
                UnitOfIssue = LabelHotPickUOI.Text
            };

            ToteToPrint.Print(1, 1, labelDetail, upc, _labelPrinter);
        }

        private void LoadNewLocations()
        {
            Task.Run(() => Task.Run(() =>
                _logger.LogDetailAsync($"Load New Locations By Item: {_currentItemDefinition.Item}  START")));

            if (_workstationView.StationType.Id == (int)StationType.StationType.Supervisor)
            {
                CheckBoxAll.Checked = true;
                CheckBoxAll.Visible = false;
            }
            else
            {
                CheckBoxAll.Visible = true;
            }

            var areaId = _workstationView.AreaId;
            var sizeCodeId = _currentItemDefinition.SizeCodeId;
            var velocityCodeId = _currentItemDefinition.VelocityCodeId;
            var heightCodeId = _currentItemDefinition.HeightCodeId;
            var inUse = 0;

            if (CheckBoxAll.Checked)
            {
                var views = _locationsRepository.FindLocationViewsByArea(areaId, _currentPage);
                var locationViews = views.ToList();
                var blvAll = new BindingListView<LocationView>(locationViews.ToList());
                _bindingSourceNewLocations.DataSource = blvAll;
                GetRecordCount(locationViews.ToList());
            }
            else
            {
                var views = _locationsRepository.GetAllLocationViewsExact(areaId,
                    sizeCodeId, velocityCodeId, heightCodeId, inUse, _currentPage);

                var locationViews = views.ToList();
                var blv = new BindingListView<LocationView>(locationViews.ToList());
                _bindingSourceNewLocations.DataSource = blv;
                GetRecordCount(locationViews.ToList());
            }

            //var blv = new BindingListView<LocationView>(views.ToList());
            //_bindingSourceNewLocations.DataSource = blv;
            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";

            Task.Run(() => Task.Run(() =>
                _logger.LogDetailAsync($"Load New Locations By Item: {_currentItemDefinition.Item}  END")));
        }

        private void LoadNewLocationsBySlot(string slot)
        {
            Task.Run(() => Task.Run(() => _logger.LogDetailAsync($"Load New Locations By Slot: {slot}  START")));
            IEnumerable<LocationView> views = null;

            // Sending an areaId of -1 returns ALL LocationViews
            var areaId = -1;
            if (_workstationView.StationType.Id == (int)StationType.StationType.Supervisor)
            {
                areaId = -1;
                CheckBoxAll.Checked = true;
                CheckBoxAll.Visible = false;
            }
            else
            {
                CheckBoxAll.Visible = true;
                areaId = _workstationView.AreaId;
            }

            if (CheckBoxAll.Checked)
            {

                views = _locationsRepository.FindLocationViewsByAreaAndSlot(areaId, slot).ToList();
            }
            else
            {
                views = _locationsRepository.FindLocationViewsByAreaAndSlot(areaId, slot).ToList();
                //views = views.Where(r => r.InUse == false);
            }

            var locationViews = views.ToList();
            var blvAll = new BindingListView<LocationView>(locationViews.ToList());
            _bindingSourceNewLocations.DataSource = blvAll;
            SetupGridNew();
            DataGridViewHot.DataSource = _bindingSourceNewLocations;
            GetRecordCount(locationViews.ToList());

            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            _logger.LogDetailAsync($"Load New Locations By Slot: {slot}  END").SafeFireAndForget();
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
                GetRecordCount(bindingSource);
            }
        }

        private void LoadItemDefinitions(string find = @"", int recId = 0)
        {
            //_currentPage = 1;
            TextBoxGoTo.Text = _currentPage.ToString();
            _logger.LogDetailAsync($"Load Item Definitions Find: {find}  START").SafeFireAndForget();
            BindingListView<ItemDefinitionView> blv = null;
            Cursor.Current = Cursors.WaitCursor;
            _logger.LogDetailAsync($"Load Item Definitions SetupGridItemDefinition Start ").SafeFireAndForget();
            SetupGridItemDefinition();
            _logger.LogDetailAsync($"Load Item Definitions SetupGridItemDefinition End").SafeFireAndForget();
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = false;
            var idx = 0;
            var findWhat = string.IsNullOrEmpty(find) ? TextBoxFindItem.Text.ToLower().Trim() : find;

            IEnumerable<ItemDefinitionView> views;
            _logger.LogDetailAsync($"Load Item Definitions Check Workstation Type ").SafeFireAndForget();
            // if its a Supervisor workstation, load the Rack items
            if (_workstationView.StationType.Id == (int)StationType.StationType.Supervisor)
            {
                _logger.LogDetailAsync($"Load Item Definitions Workstation Type Is Supervisor ").SafeFireAndForget();

                _logger.LogDetailAsync($"Load Item Definitions Rack Workstation is NOT null ").SafeFireAndForget();
                _logger.LogDetailAsync($"Load Item Definitions Call FindItemDefinitionViewsByArea  ").SafeFireAndForget();
                _logger.LogDetailAsync(
                    $"Load Item Definitions Passing in findWhat: {findWhat}  and AreaId: {_workstationView.AreaId} ").SafeFireAndForget();

                var count = _itemDefinitionsRepository.TotalItemDefinitionViewsByArea(findWhat,
                    _workstationView.AreaId);
                var pages = count % 15 == 0 ? count / 15 : count / 15 + 1;

                TextBoxTotalPages.Text = pages.ToString();


                views = _itemDefinitionsRepository.FindItemDefinitionViewsByArea(findWhat, _workstationView.AreaId, _currentPage);

                _logger.LogDetailAsync($"Load Item Definitions Back with Views.  Setting them to BindingListView ").SafeFireAndForget();

                blv = new BindingListView<ItemDefinitionView>(views.ToList());
                _logger.LogDetailAsync($"Load Item Definitions BLV created ").SafeFireAndForget();

            }
            else
            {
                _logger.LogDetailAsync($"Load Item Definitions NOT a Supervisor Workstation ").SafeFireAndForget();
                _logger.LogDetailAsync($"Load Item Definitions Call FindItemDefinitionViewsByArea  ").SafeFireAndForget();
                _logger.LogDetailAsync(
                    $"Load Item Definitions Passing in findWhat: {findWhat}  and Area: {_workstationView.AreaId} ").SafeFireAndForget();

                var count = _itemDefinitionsRepository.TotalItemDefinitionViewsByArea(findWhat,
                    _workstationView.AreaId);
                var pages = count % 15 == 0 ? count / 15 : count / 15 + 1;

                TextBoxTotalPages.Text = pages.ToString();

                views = _itemDefinitionsRepository.FindItemDefinitionViewsByArea(findWhat, _workstationView.AreaId, _currentPage)
                    .ToList();
                _logger.LogDetailAsync(
                    $"Load Item Definitions Back with Views.  Setting them to BindingListView ").SafeFireAndForget();

                if (!views.Any() && !string.IsNullOrEmpty(findWhat))
                {
                    var akaFind = _akaRepository.Get(findWhat);
                    TextBoxFindItem.Text = akaFind;
                    views = _itemDefinitionsRepository.FindItemDefinitionViewsByArea(akaFind, _workstationView.AreaId)
                        .ToList();
                    _logger.LogDetailAsync(
                        $"Load Item Definitions Back with Views Using AKA Find.  Setting them to BindingListView ").SafeFireAndForget();
                }

                blv = new BindingListView<ItemDefinitionView>(views.ToList());
                _logger.LogDetailAsync($"Load Item Definitions BLV created ").SafeFireAndForget();
            }




            _logger.LogDetailAsync($"Load Item Definitions Create a new BindingSource using BLV as DataSource ").SafeFireAndForget();
            _logger.LogDetailAsync($"Load Item Definitions called _bindingSourceItemDefinitions").SafeFireAndForget();
            _bindingSourceItemDefinitions = new BindingSource { DataSource = blv };
            _logger.LogDetailAsync($"Load Item Definitions New BindingSource has been created").SafeFireAndForget();
            _logger.LogDetailAsync($"Load Item Definitions Now set the DataGridViewHot.DataSource = to the new Bindingsource, _bindingSourceItemDefinitions ").SafeFireAndForget();
            DataGridViewHot.DataSource = _bindingSourceItemDefinitions;
            DataGridViewHot.Update();
            _logger.LogDetailAsync($"Load Item Definitions Update the Grid ").SafeFireAndForget();
            //UpdateDataGrid(_bindingSourceItemDefinitions);
            _logger.LogDetailAsync($"Load Item Definitions Get the Record Count").SafeFireAndForget();
            GetRecordCount(_bindingSourceItemDefinitions);
            if (_bindingSourceItemDefinitions.Count > 0)
            {
                _logger.LogDetailAsync($"Load Item Definitions Record count is greater that zero ").SafeFireAndForget();
                if (recId != 0)
                {
                    _logger.LogDetailAsync($"Load Item Definitions if passed in recId is not zero ").SafeFireAndForget();
                    _logger.LogDetailAsync($"Load Item Definitions Set the bindingSource to the recId ").SafeFireAndForget();
                    IndexOf(_bindingSourceItemDefinitions, recId);
                }

                try
                {
                    Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Set the row index to {idx} "));
                    DataGridViewHot.FirstDisplayedScrollingRowIndex = idx;
                    DataGridViewHot.Update();
                    if (DataGridViewHot.RowCount > 0)
                    {
                        DataGridViewHot.CurrentCell = DataGridViewHot.Rows[idx].Cells[1];
                        DataGridViewHot.Rows[idx].Selected = true;
                    }

                    Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Row set and highlight complete "));
                    Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Set the Current Item Definition "));
                    _currentItemDefinition =
                        ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Current Item: {_currentItemDefinition.Item} "));
                    Task.Run(() => _logger.LogDetailAsync(
                        $"Load Item Definitions If record count = 1 then call LoadCurrentAndNew "));
                    if (_bindingSourceItemDefinitions.Count == 1) LoadCurrentAndNew();
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
                    MessageBox.Show($"{item.Item} {_resourceManager.GetString($"Message0")} {item.AreaId}");
                }

                ClearCurrentAndNew();
            }

            DataGridViewHot.ClearSelection();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync($"Load Item Definitions Find: {find}  END"));
        }

        private void ClearCurrentAndNew()
        {
            //_bindingSourceCurrent.  .Clear();
            MBCurrentLocations.Text = $"{_resourceManager.GetString($"CurrentLocations")} (0)";
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

        private void GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString($"Records")}: {count.ToString()}";
        }

        private void GetRecordCount(IReadOnlyCollection<object> bs)
        {
            var count = bs.Count;
            LabelRecordCount.Text = $"{_resourceManager.GetString($"Records")}: {count.ToString()}";
        }

        private void SetupGridItemDefinition()
        {
            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 1"));
            if (_currentGridDataType == GridDataType.Item) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.Item;
            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewHot.ScrollBars = ScrollBars.Both;

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 2"));
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = $"Area"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 3"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = $"Item"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 4"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 5"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitOfIssueName",
                HeaderText = _gridResourceManager.GetString($"UnitOfIssueName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "UnitOfIssueName"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 6"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString($"LocationMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMax"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 7"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString($"SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 8"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString($"VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 9"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString($"HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 10"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMin",
                HeaderText = _gridResourceManager.GetString($"LocationMin"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LocationMin"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 11"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMax",
                HeaderText = _gridResourceManager.GetString($"SystemMax"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMax"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 12"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SystemMin",
                HeaderText = _gridResourceManager.GetString($"SystemMin"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                // DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "SystemMin"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition 13"));
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewHot.Columns.Add(col);

            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition Start Column Formatting"));

            DataGridViewHot.EnableHeadersVisualStyles = false;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
            Task.Run(() => _logger.LogDetailAsync($"SetupGridItemDefinition End Column Formatting"));
        }

        private void SetupGridNew()
        {
            if (_currentGridDataType == GridDataType.New) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.New;
            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewHot.ScrollBars = ScrollBars.Both;
            
            var xcol = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "InUse",
                HeaderText = _gridResourceManager.GetString($"InUse"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "InUse"
            };
            DataGridViewHot.Columns.Add(xcol);
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = $"AreaName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Slot"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc1",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc1", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc1"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc2",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc2", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc2"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc3",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc3", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc3"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc4",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc4", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc4"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Loc5",
                HeaderText = _headerTextManager.GetHeaderText(_workstationView, "Loc5", _gridResourceManager),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Loc5"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SizeCodeName",
                HeaderText = _gridResourceManager.GetString($"SizeCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "SizeCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VelocityCodeName",
                HeaderText = _gridResourceManager.GetString($"VelocityCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "VelocityCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "HeightCodeName",
                HeaderText = _gridResourceManager.GetString($"HeightCodeName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "HeightCodeName"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationCode",
                HeaderText = _gridResourceManager.GetString($"LocationCode"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Name = "LocationCode",
                Visible = false
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewHot.Columns.Add(col);

            DataGridViewHot.EnableHeadersVisualStyles = false;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
        }

        private void SetupGridCurrent()
        {
            if (_currentGridDataType == GridDataType.Current) return;
            DataGridViewHot.Columns.Clear();
            _currentGridDataType = GridDataType.Current;
            DataGridViewHot.AutoGenerateColumns = false;
            DataGridViewHot.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewHot.ScrollBars = ScrollBars.Both;
            DataGridViewHot.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewHot.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"AreaName",
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = $"AreaName"
            };
            DataGridViewHot.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = $"Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = $"Item"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                Name = "Slot",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LocationMax",
                HeaderText = _gridResourceManager.GetString($"LocationMax"),
                Name = "LocationMax",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString($"ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            col.DefaultCellStyle.Format = "MM-dd-yyyy hh:mm:ss";
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageTypeName",
                HeaderText = _gridResourceManager.GetString($"StorageTypeName"),
                Name = "StorageTypeName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewHot.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewHot.Columns.Add(col);

            DataGridViewHot.EnableHeadersVisualStyles = false;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewHot.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //foreach (DataGridViewColumn column in DataGridViewHot.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}
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
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable)
                .Where(c => c.GetType() == type);
        }

        private void ShowShi(int loc1, int loc2, int loc3, string loc4, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_iptiDisplayFunctions != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        // ClearAllShi();
                        Task.Run(() => _logger.LogDetailAsync($"ShowShi HotAction {loc2} {loc3}"));
                        // _iptiDisplayFunctions?.ShowShi(loc1, loc2, loc3, loc4, text);
                        _workstationView.ProLiteManager.TurnOn(loc1, loc3, loc4.ParseInt(), text.ParseInt());
                    }
                }
            }
        }

        private void ClearAllShi()
        {
            Task.Run(() => _logger.LogDetailAsync($"Clear All Shi  START"));
            if (!_neutronVariables.DisplaysEnabled) return;
            if (_iptiDisplayFunctions == null) return;
            if (!_neutronVariables.ShiEnabled) return;
            // _iptiDisplayFunctions?.ClearAllShi();
            Task.Run(() => _logger.LogDetailAsync($"Clear All Shi  END"));
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            Task.Run(() => _logger.LogDetailAsync($"Position Device: {loc1} {loc2} {loc3} {loc4}  START"));
            if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2))
            {
                if (_neutronVariables.ShuttleEnabled)
                {
                    if (GlobalVar.Shuttle != null)
                    {
                        if (moveDevice)
                        {
                            Task.Run(() =>
                                _logger.LogDetailAsync(
                                    $"905 HOT Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}"));
                            var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4));
                            Task.Run(() =>
                                _logger.LogDetailAsync(
                                    $"956 HOT Position Device Tray Response:{response.Result.AsString(EnumFormat.Description)}"));
                            if (response.Result != DeviceResponse.Success)
                            {
                                var resp = _enumResourceManager.GetString(response.Result.ToString());
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                    caption: _resourceManager.GetString($"DeviceInformation")
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }
                    }

                    if (GlobalVar.Hanel != null)
                    {
                        if (moveDevice)
                        {
                            Task.Run(() =>
                                _logger.LogDetailAsync(
                                    $"905 HOT Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}"));
                            var response = Task.Run(() => GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4));
                            Task.Run(() =>
                                _logger.LogDetailAsync(
                                    $"956 HOT Position Device Tray Response:{response.Result.AsString(EnumFormat.Description)}"));
                            if (response.Result != DeviceResponse.Success)
                            {
                                var resp = _enumResourceManager.GetString(response.Result.ToString());
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                    caption: _resourceManager.GetString($"DeviceInformation")
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

            Task.Run(() => _logger.LogDetailAsync($"Position Device END"));
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
                MessageBox.Show(_resourceManager.GetString($"Message3") + ex.Message);
            }

            Task.Run(() => _logger.LogDetailAsync("Find Hot Record: {findWhat} End"));
        }

        private void ButtonHotPickClear_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.LogDetailAsync($"Hot Pick Clear START"));
            Cursor.Current = Cursors.WaitCursor;
            _bindingSourceCurrent.DataSource = null;
            MBCurrentLocations.Text =
                $"{_resourceManager.GetString($"CurrentLocations")} ({_bindingSourceCurrent.Count})";
            _bindingSourceNewLocations.DataSource = null;
            MBNewLocations.Text = $"{_newLocationButtonText} ({_bindingSourceNewLocations.Count})";
            TextBoxFindItem.Text = string.Empty;
            //if (_workstationView.StationType.Name != "EBin")
            //{
            //    LoadItemDefinitions();
            //}
            LoadItemDefinitions();

            TextBoxFindItem.Focus();
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync($"Hot Pick Clear END"));
        }

        private void MBHotPick_Click(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"Hot Pick Button Pressed START").SafeFireAndForget();
            _hotPickButtonPressed = true;
            _hotStoreButtonPressed = false;
            CloseButtonPressed = false;

            // open a form to get the quantity to pick
            _quantity = GetQuantity();

            if (_quantity == 0) return;

            //Cost Center

            if (_useCostCenter)
            {
                using (var form = new FrmCostCenter())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        _costCenterCode = form.CostCenterCode;
                        _costCenterActionCode = form.CostCenterActionCode;
                        TextBoxCostCenter.Text = form.CostCenterName;
                    }
                }
            }

            //GroupBoxHotActions.Visible = _useCostCenter;
            //if (_useCostCenter)
            //{
            //    FillCostCenterComboBox();
            //    RadioButtonCostCenter.Checked = true;
            //    ComboBoxCostCenter.Visible = true;
            //    TextBoxFindCostCenter.Visible = true;
            //    RadioButtonCostCenter.Visible = true;
            //    RadioButtonPick.Text = $"{_resourceManager.GetString($"Pick")}";
            //    RadioButtonPick.Tag = "Pick";
            //}

            _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;
            var loc1 = _currentInventoryView.Loc1;
            var loc2 = _currentInventoryView.Loc2;
            var loc3 = _currentInventoryView.Loc3;
            var loc4 = _currentInventoryView.Loc4;

            var maxColumns = _locationsRepository.GetMaxColumns(_workstationView.AreaId, loc1, loc2);
            var maxRows = _locationsRepository.GetMaxRows(_workstationView.AreaId, loc1, loc2);


            if (!_workstationView.HardwareDevices.Any())
            {
                HotAction.BackColor = Color.Red;
                LabelFormTitle.BackColor = Color.Red;
                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                UpdateHotPickScreen(_currentInventoryView);
                // UpdateCurrentDeviceIndicator();
                //_deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                tabControl1.SelectedTab = HotAction;
                return;
            } //No Hardware devices

            foreach (var device in _workstationView.HardwareDevices)
            {
                switch (device.DeviceTypeId)
                {
                    case (int)DeviceTypeEnum.Shuttle
                        when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        //HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.Red;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  //Shuttle
                        //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                        //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        UpdateHotPickScreenTray(_currentInventoryView);
                        // tabControl1.SelectedTab = HotActionTray;
                        tabControl1.SelectedTab = HotAction;
                        break;
                    case (int)DeviceTypeEnum.Shuttle:
                        //Access Denied
                        MessageBox.Show($"Location Access Denied");
                        break;
                    case (int)DeviceTypeEnum.Carousel
                        when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        HotAction.BackColor = Color.Red;
                        LabelFormTitle.BackColor = Color.Red;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  // Carousel
                        ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                            , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                        break;
                    case (int)DeviceTypeEnum.Carousel:
                        // Access Denied
                        MessageBox.Show($"Location Access Denied");
                        break;
                    case (int)DeviceTypeEnum.Hanel12D
                        when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        //HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.LightGray;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  //Hanel12D
                        _workstationView.ProLiteManager?.TurnOn(loc1, loc3, loc4, _quantity);
                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                        break;
                    case (int)DeviceTypeEnum.Hanel12D:
                        //Access Denied
                        MessageBox.Show($"Location Access Denied");
                        break;
                    case (int)DeviceTypeEnum.Hanel12N
                        when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        // HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.LightGray;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  //Hanel12N
                        _workstationView.ProLiteManager?.TurnOn(loc1, loc3, loc4, _quantity);
                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                        break;
                    case (int)DeviceTypeEnum.Hanel12N:
                        //Access Denied
                        MessageBox.Show($"Location Access Denied");
                        break;
                    case (int)DeviceTypeEnum.Blastzone:
                        // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                        //  BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        // HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.LightGray;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        //TextBoxHotPickQuantity.Text = _quantity.ToString();
                        //PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

                        // await _iptiDisplayFunctions?.ShowBli(loc2, loc4, 2, _quantity.ToString());

                        if (_neutronVariables.IptiDisplays)
                        {
                            if (_iptiDisplayFunctions != null)
                            {
                                _iptiDisplayFunctions.TurnOnBlastzoneDisplay(loc2, loc4, _quantity.ToString());
                                _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(loc2, _currentInventoryView.Item.Trim());
                            }
                        }

                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                        break;
                    case (int)DeviceTypeEnum.ProLite:
                        {
                            _workstationView.ProLiteManager?.TurnOn(loc1, loc3, loc4, _quantity);
                            break;
                        }
                    case (int)DeviceTypeEnum.IptiDisplays:
                        {
                            if (_neutronVariables.IptiDisplays)
                            {
                                var bliController = _neutronVariables.BliController;
                                if (_iptiDisplayFunctions != null)
                                {
                                    _iptiDisplayFunctions?.TurnOnBlastzoneDisplay(bliController, loc4, _quantity.ToString());
                                    _iptiDisplayFunctions?.TurnOnBlastzoneOrderControl(bliController, _currentInventoryView.Item.Trim());
                                }
                            }

                            break;
                        }
                    case (int)DeviceTypeEnum.Rack:
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        //HotAction.BackColor = Color.Red;
                        LabelFormTitle.BackColor = Color.Red;
                        LabelFormTitle.Text = "Rack Selection"; // $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                        //UpdateCurrentDeviceIndicator();
                        //PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                        //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                        //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        //await UpdateHotPickScreen(_currentInventoryView);
                        // tabControl1.SelectedTab = HotRackTray;
                        tabControl1.SelectedTab = HotAction;
                        break;
                    // Rack or Supervisor
                    default:
                        HotAction.BackColor = Color.Red;
                        LabelFormTitle.BackColor = Color.Red;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                        MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                        UpdateHotPickScreen(_currentInventoryView);
                        // UpdateCurrentDeviceIndicator();
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                        tabControl1.SelectedTab = HotAction;
                        break;
                }
            }

            _logger.LogDetailAsync($"Hot Pick Button Press END").SafeFireAndForget();
        }

        /// <summary>
        /// Open a form to get the quantity to pick
        /// </summary>
        /// <returns>Quantity</returns>
        private int GetQuantity()
        {
            var quantity = 0;
            var frm = new FrmQuantity();
            frm.ShowDialog();
            if (frm.DialogResult == DialogResult.OK)
            {
                quantity = frm.Quantity.ParseInt();
            }

            return quantity;
        }

        private void MBHotStore_Click(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"Hot Store Button Pressed START").SafeFireAndForget();
            TextBoxHotPickQuantity.Text = _quantityToPick.ToString();
            _hotPickButtonPressed = false;
            _hotStoreButtonPressed = true;
            CloseButtonPressed = false;

            // get the quantity to store
            _quantity = GetQuantity();
            if (_quantity == 0) return;

            if (_currentGridDataType == GridDataType.Current)
            {
                _currentInventoryView = ((ObjectView<SqlInventoryView>)_bindingSourceCurrent.Current).Object;
                var loc1 = _currentInventoryView.Loc1;
                var loc2 = _currentInventoryView.Loc2;
                var loc3 = _currentInventoryView.Loc3;
                var loc4 = _currentInventoryView.Loc4;

                var maxColumns = _locationsRepository.GetMaxColumns(_workstationView.Area.Id, loc1, loc2);
                var maxRows = _locationsRepository.GetMaxRows(_workstationView.AreaId, loc1, loc2);

                //var device = _workstationView.HardwareDevices.FirstOrDefault(d => d.DeviceNumber == loc1
                // && d.DeviceType.Pickable);
                //if (device == null) return;
                UpdateHotPickScreen(_currentInventoryView);
                HotAction.BackColor = Color.Green;
                LabelFormTitle.BackColor = Color.Green;
                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                MBHotAccept.Text = _resourceManager.GetString($"Accept");
                MBHotAccept.Enabled = true;
                tabControl1.SelectedTab = HotAction;

                if (_workstationView.HardwareDevices.Any()) // Hardware devices
                {

                    foreach (var device in _workstationView.HardwareDevices)
                    {
                        switch (device.DeviceTypeId)
                        {
                            //if (device == null) //No Hardware devices
                            //{
                            //    BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                            //    await UpdateHotPickScreen(_currentInventoryView);
                            //    tabControl1.SelectedTab = HotAction;
                            //}
                            //else 
                            case (int)DeviceTypeEnum.Shuttle
                                when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                                // HotActionTray.BackColor = Color.LightGray;
                                HotAction.BackColor = Color.LightGray;
                                LabelFormTitle.BackColor = Color.Red;
                                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                                MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                                PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  // Shuttle
                                                                                           //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                                                                                           //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                                UpdateHotPickScreenTray(_currentInventoryView);
                                BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

                                tabControl1.SelectedTab = HotAction;
                                break;
                            case (int)DeviceTypeEnum.Shuttle:
                                //Access Denied
                                MessageBox.Show($"Location Access Denied");
                                break;
                            case (int)DeviceTypeEnum.Carousel
                                when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                                BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);

                                //  UpdateCurrentDeviceIndicator();
                                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                                PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  // Carousel
                                ShowShi(loc1, loc2, loc3, loc4.ToString(), 1.ToString());
                                UpdateHotPickScreen(_currentInventoryView);
                                tabControl1.SelectedTab = HotAction;
                                break;
                            case (int)DeviceTypeEnum.Carousel:
                                // Access Denied
                                MessageBox.Show($"Location Access Denied");
                                break;
                            case (int)DeviceTypeEnum.Hanel12D
                                when _lacProcessor.MovePermitted(_workstationView.WorkstationId, loc1, loc2):
                                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                                BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                                // HotActionTray.BackColor = Color.LightGray;
                                HotAction.BackColor = Color.LightGray;
                                LabelFormTitle.BackColor = Color.LightGray;
                                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                                MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                                PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);  //Hanel12D
                                                                                           //ProLite(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                                                                                           //    , _currentInventoryView.Loc4.ToString(), 1.ToString(), quantity);
                                UpdateHotPickScreen(_currentInventoryView);
                                tabControl1.SelectedTab = HotAction;
                                break;
                            case (int)DeviceTypeEnum.Hanel12D:
                                //Access Denied
                                MessageBox.Show($"Location Access Denied");
                                break;
                            case (int)DeviceTypeEnum.Blastzone:
                                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                                //  BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                                // HotActionTray.BackColor = Color.LightGray;
                                HotAction.BackColor = Color.LightGray;
                                LabelFormTitle.BackColor = Color.LightGray;
                                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotPick")}";
                                MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                                //TextBoxHotPickQuantity.Text = _quantity.ToString();
                                //PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

                                // await _iptiDisplayFunctions?.ShowBli(loc2, loc4, 2, _quantity.ToString());

                                if (_neutronVariables.IptiDisplays)
                                {
                                    if (_iptiDisplayFunctions != null)
                                    {
                                        _iptiDisplayFunctions.TurnOnBlastzoneDisplay(loc2, loc4, _quantity.ToString());
                                        _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(loc2, _currentInventoryView.Item.Trim());
                                    }

                                }
                                UpdateHotPickScreen(_currentInventoryView);
                                tabControl1.SelectedTab = HotAction;
                                break;
                            case (int)DeviceTypeEnum.ProLite:
                                {
                                    _workstationView.ProLiteManager?.TurnOn(loc1, loc3, loc4, _quantity);
                                    break;
                                }
                            case (int)DeviceTypeEnum.IptiDisplays:  //Batch Table
                                {
                                    if (_neutronVariables.IptiDisplays)
                                    {
                                        if (_iptiDisplayFunctions != null)
                                        {
                                            _iptiDisplayFunctions.TurnOnBatchDisplay(1, _quantity.ToString());
                                            _iptiDisplayFunctions.TurnOnBatchOrderControl(_currentInventoryView.Item.Trim());
                                        }
                                    }
                                    break;
                                }
                            case (int)DeviceTypeEnum.Rack:
                                BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                                //HotAction.BackColor = Color.Red;
                                LabelFormTitle.BackColor = Color.Red;
                                LabelFormTitle.Text = "Rack Selection"; // $"{_resourceManager.GetString($"HotPick")}";
                                MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                                //UpdateCurrentDeviceIndicator();
                                //PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                                //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                                //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                                //await UpdateHotPickScreen(_currentInventoryView);
                                // tabControl1.SelectedTab = HotRackTray;
                                tabControl1.SelectedTab = HotAction;
                                break;

                            //case (int)DeviceTypeEnum.Rack:
                            //    {
                            //        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                            //        //HotAction.BackColor = Color.Red;
                            //        LabelFormTitle.BackColor = Color.Red;
                            //        LabelFormTitle.Text = "Rack Selection"; // $"{_resourceManager.GetString($"HotPick")}";
                            //        MBHotAccept.Text = $"{_resourceManager.GetString($"Accept")}";
                            //        //UpdateCurrentDeviceIndicator();
                            //        //PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
                            //        //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                            //        //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                            //        //await UpdateHotPickScreen(_currentInventoryView);
                            //        var manager = new NeutronTrayManager(PanelTableLayoutRack, 4, 1, 4, 2, 2, 2, 1, 1);
                            //        tabControl1.SelectedTab = HotAction;
                            //        break;
                            //    }
                            // Rack or Supervisor
                            default:
                                MessageBox.Show("Invalid Hardware Setting.");
                                //await UpdateHotPickScreen(_currentInventoryView);
                                ////UpdateCurrentDeviceIndicator();
                                //_deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                                //tabControl1.SelectedTab = HotAction;
                                break;
                        }

                        _logger.LogDetailAsync($"Hot Pick Button Press END").SafeFireAndForget();

                        //---------------------------------------------------------------------


                    }
                }
            }
            else // the current grid is New Items, Not Current Items
            {
                var location = ((ObjectView<LocationView>)_bindingSourceNewLocations.Current).Object;

                var itemDef = ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                // Is this location and itemDef already in inventory
                var inventoryView =
                    _repoInv.GetInventoryViewByItemDefinitionIdAndLocationId(itemDef.Id, location.Id);
                if (inventoryView == null)
                {
                    _currentInventoryView = new SqlInventoryView
                    {
                        ItemDefinitionId = itemDef.Id,
                        Item = itemDef.Item,
                        Description = itemDef.Description,
                        HeightCodeId = itemDef.HeightCodeId,
                        HeightCodeName = itemDef.HeightCodeName,
                        ReceivedDate = DateTime.Now,
                        SizeCodeId = itemDef.SizeCodeId,
                        SizeCodeName = itemDef.SizeCodeName,
                        VelocityCodeId = itemDef.VelocityCodeId,
                        VelocityCodeName = itemDef.VelocityCodeName,
                        StorageTypeId = itemDef.StorageTypeId,
                        StorageTypeName = itemDef.StorageTypeName,
                        AreaId = itemDef.AreaId,
                        Quantity = 0,
                        LocationId = location.Id,
                        Loc1 = location.Loc1,
                        Loc2 = location.Loc2,
                        Loc3 = location.Loc3,
                        Loc4 = location.Loc4,
                        Loc5 = location.Loc5,
                        LocationCode = location.LocationCode,
                        InUse = location.InUse,
                        UnitOfIssueId = itemDef.UnitOfIssueId,
                        UnitOfIssueName = itemDef.UnitOfIssueName
                    };
                }
                else
                {
                    _currentInventoryView = inventoryView;
                }

                // var loc1 = location.Loc1;
                var loc2 = location.Loc2;
                var loc3 = location.Loc3;
                var loc4 = location.Loc4;

                var maxColumns = _locationsRepository.GetMaxColumns(_workstationView.AreaId, location.Loc1, loc2);
                var maxRows = _locationsRepository.GetMaxRows(_workstationView.AreaId, location.Loc1, loc2);

                var device = _workstationView.HardwareDevices.FirstOrDefault(d => d.DeviceNumber == location.Loc1);


                HotAction.BackColor = Color.Green;
                LabelFormTitle.BackColor = Color.Green;
                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                MBHotAccept.Text = _resourceManager.GetString($"Accept");
                MBHotAccept.Enabled = true;


                if (device == null) //No Hardware devices
                {
                    // BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                    // HotActionTray.BackColor = Color.LightGray;
                    HotAction.BackColor = Color.LightGray;
                    UpdateHotPickScreen(_currentInventoryView);
                    tabControl1.SelectedTab = HotAction;
                }
                else if (device.DeviceTypeId == (int)DeviceTypeEnum.Shuttle)
                {
                    if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, location.Loc1, loc2))
                    {
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(location.Loc1);
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        //HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.Red;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        PositionDevice(location.Loc1, loc2, loc3, loc4, moveDevice: true);  //Shuttle
                        //ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                        //    , _currentInventoryView.Loc4.ToString(), 1.ToString());
                        UpdateHotPickScreenTray(_currentInventoryView);
                        // tabControl1.SelectedTab = HotActionTray;
                        tabControl1.SelectedTab = HotAction;
                    }
                    else
                    {
                        //Access Denied
                        MessageBox.Show($"Location Access Denied");
                    }
                }
                else if (device.DeviceTypeId == (int)DeviceTypeEnum.Carousel)
                {
                    if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, location.Loc1, loc2))
                    {
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        // UpdateCurrentDeviceIndicator();
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(location.Loc1);
                        PositionDevice(location.Loc1, loc2, loc3, loc4, moveDevice: true);  //Carousel
                        ShowShi(location.Loc1, loc2, loc3, loc4.ToString(), 1.ToString());
                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                    }
                    else
                    {
                        // Access Denied
                        MessageBox.Show($"Location Access Denied");
                    }
                }
                else if (device.DeviceTypeId == (int)DeviceTypeEnum.Hanel12D)
                {
                    if (_lacProcessor.MovePermitted(_workstationView.WorkstationId, location.Loc1, loc2))
                    {
                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(location.Loc1);
                        BuildTrayLayout(maxColumns, maxRows, loc3, loc4, 1, 1);
                        // HotActionTray.BackColor = Color.LightGray;
                        HotAction.BackColor = Color.LightGray;
                        LabelFormTitle.BackColor = Color.LightGray;
                        LabelFormTitle.Text = $"{_resourceManager.GetString($"HotStore")}";
                        MBHotAcceptTray.Text = $"{_resourceManager.GetString($"Accept")}";
                        PositionDevice(location.Loc1, loc2, loc3, loc4, moveDevice: true);  //Hanel12D
                        //ProLite(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3
                        //    , _currentInventoryView.Loc4.ToString(), 1.ToString(), quantity);
                        UpdateHotPickScreen(_currentInventoryView);
                        tabControl1.SelectedTab = HotAction;
                    }
                    else
                    {
                        //Access Denied
                        MessageBox.Show($"Location Access Denied");
                    }
                }
                else // Rack or Supervisor
                {
                    UpdateHotPickScreen(_currentInventoryView);
                    // UpdateCurrentDeviceIndicator();
                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(location.Loc1);
                    tabControl1.SelectedTab = HotAction;
                }

                _logger.LogDetailAsync($"Hot Pick Button Press END").SafeFireAndForget();

            }


            _logger.LogDetailAsync($"Hot Store Button Press END").SafeFireAndForget();
        }

        private void UpdateHotPickScreen(SqlInventoryView invItem)
        {
            Task.Run(() => _logger.LogDetailAsync("Update Hot Pick Screen START"));
            try
            {
                using (var db = new NeutronDb())
                {
                    //_quantity = _quantityToPick;
                    var itemDefinition = db.ItemDefinitions.FirstOrDefault(r => r.Id == invItem.ItemDefinitionId);
                    if (itemDefinition == null) throw new ArgumentNullException(nameof(itemDefinition));
                    var location = db.Locations.FirstOrDefault(r => r.Id == invItem.LocationId);
                    if (location == null) throw new ArgumentNullException(nameof(location));
                    LabelHotPickDescription.Text = itemDefinition.Description;
                    LabelHotPickItem.Text = itemDefinition.Item;
                    LabelHotPickUOI.Text = itemDefinition.UnitOfIssue.Name;

                    //TextBoxHotPickLoc1.Text = location.Loc1.ToString();
                    //TextBoxHotPickLoc2.Text = location.Loc2.ToString();
                    //TextBoxHotPickLoc3.Text = location.Loc3.ToString();
                    //TextBoxHotPickLoc4.Text = location.Loc4.ToString();
                    //TextBoxHotPickLoc5.Text = location.Loc5.ToString();
                    //LabelSlot.Text = location.Slot;
                    //----------------------                 
                    UpdateGroupBoxLocation(location);


                    //----------------------

                    //if (location.Area.LocationTypeId == (int)LocationTypeEnum.Rack)
                    //{
                    //    //show slot
                    //    LabelDevice.Visible = false;
                    //    LabelTray.Visible = false;
                    //    LabelOver.Visible = false;
                    //    LabelBack.Visible = false;

                    //    TextBoxHotPickLoc1.Visible = true;
                    //    TextBoxHotPickLoc2.Visible = false;
                    //    TextBoxHotPickLoc3.Visible = false;
                    //    TextBoxHotPickLoc4.Visible = false;
                    //    TextBoxHotPickLoc5.Visible = false;

                    //    LabelDevice.Text = "Slot";
                    //    LabelDevice.Visible = true;
                    //    TextBoxHotPickLoc1.Location = new Point(10, 57);
                    //    TextBoxHotPickLoc1.Size = new Size(380, 57);
                    //    TextBoxHotPickLoc1.Text = location.Slot;
                    //}
                    //else
                    //{
                    //    LabelDevice.Visible = true;
                    //    LabelTray.Visible = true;
                    //    LabelOver.Visible = true;
                    //    LabelBack.Visible = true;
                    //    TextBoxHotPickLoc1.Visible = true;
                    //    TextBoxHotPickLoc2.Visible = true;
                    //    TextBoxHotPickLoc3.Visible = true;
                    //    TextBoxHotPickLoc4.Visible = true;
                    //    TextBoxHotPickLoc5.Visible = true;

                    //    TextBoxHotPickLoc1.Text = location.Loc1.ToString();
                    //    TextBoxHotPickLoc2.Text = location.Loc2.ToString();
                    //    TextBoxHotPickLoc3.Text = location.Loc3.ToString();
                    //    TextBoxHotPickLoc4.Text = location.Loc4.ToString();
                    //    TextBoxHotPickLoc5.Text = location.Loc5.ToString();
                    //}





                    //----------------------

                    ComboBoxSizeCodeItem.SelectedIndex = ComboBoxSizeCodeItem.FindStringExact(itemDefinition.SizeCode.Name);
                    ComboBoxVelocityCodeItem.SelectedIndex = ComboBoxVelocityCodeItem.FindStringExact(itemDefinition.VelocityCode.Name);
                    ComboBoxHeightCodeItem.SelectedIndex = ComboBoxHeightCodeItem.FindStringExact(itemDefinition.HeightCode.Name);
                    TextBoxLocationCode.Text = location.LocationCode;
                    ComboBoxSizeCodeLocation.SelectedIndex = ComboBoxSizeCodeLocation.FindStringExact(location.SizeCode.Name);
                    ComboBoxVelocityCodeLocation.SelectedIndex = ComboBoxVelocityCodeLocation.FindStringExact(location.VelocityCode.Name);
                    ComboBoxHeightCodeLocation.SelectedIndex = ComboBoxHeightCodeLocation.FindStringExact(location.HeightCode.Name);
                    TextBoxHotPickQuantity.Text = _quantity.ToString();
                    TextBoxHotPickQuantity.Focus();
                    TextBoxHotPickLocationQuantity.Text = invItem.Quantity.ToString();
                    TextBoxHotPickReceivedDate.Text = string.IsNullOrEmpty(invItem.ReceivedDate.ToString("d"))
                        ? ""
                        : invItem.ReceivedDate.ToShortDateString();
                    LabelPrimeBin.Visible = invItem.PrimeBin;
                    LabelStaticRelease.Text = invItem.StorageTypeName;
                    if (_neutronVariables.UseImages) PictureBoxItemHotImage?.LoadAsync(_imageManager.GetImageFile(invItem.Item));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Task.Run(() => _logger.LogDetailAsync("Update Hot Pick Screen END"));
        }

        private void UpdateGroupBoxLocation(Location location)
        {
            _logger.LogDetailAsync($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            if (location.Area.LocationTypeId == (int)LocationTypeEnum.Rack)
            {
                //show slot
                LabelDevice.Visible = true;
                LabelTray.Visible = false;
                LabelOver.Visible = false;
                LabelBack.Visible = false;

                TextBoxHotPickLoc1.Visible = true;
                TextBoxHotPickLoc2.Visible = false;
                TextBoxHotPickLoc3.Visible = false;
                TextBoxHotPickLoc4.Visible = false;
                TextBoxHotPickLoc5.Visible = false;

                LabelDevice.Text = @"Slot";
                LabelDevice.Location = new Point(150, 30);
                LabelDevice.Size = new Size(100, 22);

                TextBoxHotPickLoc1.Location = new Point(50, 57);
                TextBoxHotPickLoc1.Size = new Size(300, 62);
                TextBoxHotPickLoc1.Text = location.Slot;
            }
            if (location.Area.LocationTypeId == (int)LocationTypeEnum.Blastzone)
            {
                LabelTray.Text = @"Unit";
                LabelBack.Text = @"Display";
                LabelTray.Location = new Point(70, 30);
                LabelTray.Size = new Size(100, 22);
                LabelBack.Location = new Point(230, 30);
                LabelBack.Size = new Size(100, 22);
                TextBoxHotPickLoc2.Location = new Point(70, 57);
                TextBoxHotPickLoc2.Size = new Size(100, 62);
                TextBoxHotPickLoc4.Location = new Point(230, 57);
                TextBoxHotPickLoc4.Size = new Size(100, 62);

                LabelDevice.Visible = false;
                LabelTray.Visible = true;
                LabelOver.Visible = false;
                LabelBack.Visible = true;
                TextBoxHotPickLoc1.Visible = false;
                TextBoxHotPickLoc2.Visible = true;
                TextBoxHotPickLoc3.Visible = false;
                TextBoxHotPickLoc4.Visible = true;
                TextBoxHotPickLoc5.Visible = false;

                TextBoxHotPickLoc1.Text = location.Loc1.ToString();
                TextBoxHotPickLoc2.Text = location.Loc2.ToString();
                TextBoxHotPickLoc3.Text = location.Loc3.ToString();
                TextBoxHotPickLoc4.Text = location.Loc4.ToString();
                TextBoxHotPickLoc5.Text = location.Loc5.ToString();
            }
            if (location.Area.LocationTypeId == (int)LocationTypeEnum.Vertical)
            {
                LabelDevice.Visible = true;
                LabelTray.Visible = true;
                LabelOver.Visible = true;
                LabelBack.Visible = true;
                TextBoxHotPickLoc1.Visible = true;
                TextBoxHotPickLoc2.Visible = true;
                TextBoxHotPickLoc3.Visible = true;
                TextBoxHotPickLoc4.Visible = true;
                TextBoxHotPickLoc5.Visible = true;

                TextBoxHotPickLoc1.Text = location.Loc1.ToString();
                TextBoxHotPickLoc2.Text = location.Loc2.ToString();
                TextBoxHotPickLoc3.Text = location.Loc3.ToString();
                TextBoxHotPickLoc4.Text = location.Loc4.ToString();
                TextBoxHotPickLoc5.Text = location.Loc5.ToString();
            }

            //LabelLocationNumber.Text = $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            //TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            //TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            //TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G"));
            //LabelPrimeBin.Visible = inventory.PrimeBin;
            //LabelStaticRelease.Text = inventory.StorageType.Name;
            //Task.Run(() => _logger.LogDetailAsync($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void UpdateHotPickScreenTray(SqlInventoryView invItem)
        {
            Task.Run(() => _logger.LogDetailAsync("Update Hot Pick Screen Tray START"));
            try
            {
                using (var db = new NeutronDb())
                {
                    var itemDefinition = db.ItemDefinitions.FirstOrDefault(r => r.Id == invItem.ItemDefinitionId);
                    if (itemDefinition == null) throw new ArgumentNullException(nameof(itemDefinition));
                    var location = db.Locations.FirstOrDefault(r => r.Id == invItem.LocationId);
                    if (location == null) throw new ArgumentNullException(nameof(location));
                    LabelHotPickDescriptionTray.Text = itemDefinition.Description;
                    LabelHotPickItemTray.Text = itemDefinition.Item;
                    LabelHotPickUOITray.Text = itemDefinition.UnitOfIssue.Name;
                    TextBoxHotPickLoc1Tray.Text = location.Loc1.ToString();
                    TextBoxHotPickLoc2Tray.Text = location.Loc2.ToString();
                    TextBoxHotPickLoc3Tray.Text = location.Loc3.ToString();
                    TextBoxHotPickLoc4Tray.Text = location.Loc4.ToString();
                    TextBoxHotPickLoc5Tray.Text = location.Loc5.ToString();

                    //Control c = Controls.Find($"LabelWidth{location.Loc3.ToString()}", true).First();
                    //if (c != null)
                    //{
                    //    var label = ((Label)c);
                    //    label.BackColor = Color.Red;
                    //    //label.Visible = true;
                    //    label.Refresh();
                    //}

                    //c = Controls.Find($"LabelDepth{location.Loc4.ToString()}", true).First();
                    //if (c != null)
                    //{
                    //    var label = ((Label)c);
                    //    label.BackColor = Color.Red;
                    //    //label.Visible = true;
                    //    label.Refresh();
                    //}

                    LabelSlotTray.Text = location.Slot;
                    // ComboBoxSizeCodeItem.SelectedIndex = ComboBoxSizeCodeItem.FindStringExact(itemDefinition.SizeCode.Name);
                    // ComboBoxVelocityCodeItem.SelectedIndex = ComboBoxVelocityCodeItem.FindStringExact(itemDefinition.VelocityCode.Name);
                    // ComboBoxHeightCodeItem.SelectedIndex = ComboBoxHeightCodeItem.FindStringExact(itemDefinition.HeightCode.Name);
                    //  ComboBoxLocationCodeItem.SelectedIndex = ComboBoxLocationCodeItem.FindStringExact(itemDefinition.LocationCode.Name);
                    ComboBoxSizeCodeLocationTray.SelectedIndex = ComboBoxSizeCodeLocationTray.FindStringExact(location.SizeCode.Name);
                    ComboBoxVelocityCodeLocationTray.SelectedIndex = ComboBoxVelocityCodeLocationTray.FindStringExact(location.VelocityCode.Name);
                    ComboBoxHeightCodeLocationTray.SelectedIndex = ComboBoxHeightCodeLocationTray.FindStringExact(location.HeightCode.Name);
                    TextBoxHotPickLocationCodeTray.Text = location.LocationCode;
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
            Task.Run(() => _logger.LogDetailAsync("Update Hot Pick Screen Tray END"));
        }

        //private void RadioButtonHotAction(object sender, EventArgs e)
        //{
        //    if (RadioButtonPick.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonPick.Text;
        //    }
        //    else if (RadioButtonWarranty.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonWarranty.Text;
        //    }
        //    else if (RadioButtonScrap.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonScrap.Text;
        //    }
        //    else if (RadioButtonOther.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonOther.Text;
        //    }
        //    else if (RadioButtonCostCenter.Checked)
        //    {
        //        MBHotAccept.Text = RadioButtonCostCenter.Text;
        //    }
        //}
        private void DataGridViewHot_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Task.Run(() => _logger.LogDetailAsync("DataGrid View Hot Cell Click START"));
            Cursor.Current = Cursors.WaitCursor;
            switch (_currentGridDataType)
            {
                case GridDataType.Item:
                    DataGridViewHot.Columns.Clear();
                    _currentPage = 1;
                    TextBoxGoTo.Text = _currentPage.ToString();
                    TextBoxTotalPages.Text = string.Empty;
                    LoadCurrentAndNew();
                    break;
                case GridDataType.New:
                    break;
                case GridDataType.Current:
                    break;
            }
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.LogDetailAsync("DataGrid View Hot Cell Click END"));
        }

        private void LoadCurrentAndNew()
        {
            Task.Run(() => _logger.LogDetailAsync("Load Current And New START"));
            if (_currentGridDataType == GridDataType.Item)
            {
                if (_bindingSourceItemDefinitions.Count > 0)
                {
                    _currentItemDefinition = ((ObjectView<ItemDefinitionView>)_bindingSourceItemDefinitions.Current).Object;
                    TextBoxFindItem.Text = _currentItemDefinition.Item;

                    LoadCurrent();
                    LoadNewLocations();
                    if (_bindingSourceCurrent.Count > 0)
                    {
                        SetupGridCurrent();
                        DataGridViewHot.DataSource = _bindingSourceCurrent;
                        GetRecordCount(_bindingSourceCurrent);
                        //if(DataGridViewHot.RowCount > 0)
                        //{
                        //    var selected = DataGridViewHot.SelectedRows[0].Selected; // = true;
                        //}
                        //DataGridViewHot.ClearSelection();
                    }
                    else if (_bindingSourceNewLocations.Count > 0)
                    {
                        SetupGridNew();
                        DataGridViewHot.DataSource = _bindingSourceNewLocations;
                        GetRecordCount(_bindingSourceNewLocations);
                        //if (DataGridViewHot.RowCount > 0)
                        //{
                        //    var selected = DataGridViewHot.SelectedRows[0].Selected; // = true;
                        //}
                        //DataGridViewHot.ClearSelection();
                    }

                    SetHotButtonStatus();
                }
            }
            Task.Run(() => _logger.LogDetailAsync("Load Current And New END"));
        }

        private void SetHotButtonStatus()
        {
            MBHotPick.Enabled = true;
            MBHotStore.Enabled = true;
            MBCurrentLocations.Enabled = true;
            MBNewLocations.Enabled = true;
            // TextBoxScanLocation.Text = String.Empty;
            // TextBoxScanLocation.Enabled = true;

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

        private void MBHotActionBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void Back()
        {
            LogBackButtonPressStart();
            CloseButtonPressed = false;
            if (_pickList == null)
            {
                HandleBackActionWithoutPickList();
            }
            else
            {
                HandleBackActionWithPickList();
            }
            LogBackButtonPressEnd();
        }
        private void LogBackButtonPressStart()
        {
            _logger.LogDetailAsync($"Hot Action Back Button Pressed START").SafeFireAndForget();
        }
        private void LogBackButtonPressEnd()
        {
            _logger.LogDetailAsync($"Hot Action Back Button Pressed END").SafeFireAndForget();
        }
        private void HandleBackActionWithoutPickList()
        {
            ClearIptiDisplayFunctions();
            ClearWorkstationView();
            FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
            UpdateFormTitle(Color.Red);
            tabControl1.SelectedTab = HotPick;
        }
        private void HandleBackActionWithPickList()
        {
            UpdateFormTitle(Color.Green);
            tabControl1.SelectedTab = HotPick;
        }
        private void ClearIptiDisplayFunctions()
        {
            if (_iptiDisplayFunctions != null)
            {
                _iptiDisplayFunctions.ClearBatchTable();
                _iptiDisplayFunctions.TurnOffBatchOrderControl();
                _iptiDisplayFunctions.ClearBlastzone();
            }
        }
        private void ClearWorkstationView()
        {
            _workstationView.ProLiteManager?.ClearAllProlites();
            _deviceIndicatorManager?.ClearAllDeviceIndicators();
        }
        private void UpdateFormTitle(Color color)
        {
            LabelFormTitle.Text = _resourceManager.GetString($"HotActions");
            LabelFormTitle.BackColor = color;
        }


        //private void Back()
        //{
        //    Task.Run(() => _logger.LogDetailAsync($"Hot Action Back Button Pressed START"));
        //    if (_pickList == null)
        //    {
        //        CloseButtonPressed = false;
        //        // ClearAllShi();
        //        _iptiDisplayFunctions?.ClearBatchTable();
        //        _iptiDisplayFunctions?.TurnOffBatchOrderControl();
        //        _iptiDisplayFunctions?.ClearBlastzone();

        //        _workstationView.ProLiteManager?.ClearAllProlites();
        //        _deviceIndicatorManager?.ClearAllDeviceIndicators();

        //        FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //        LabelFormTitle.Text = _resourceManager.GetString($"HotActions");
        //        LabelFormTitle.BackColor = Color.Red;
        //        tabControl1.SelectedTab = HotPick;
        //        TextBoxFindCostCenter.Text = string.Empty;
        //    }
        //    else
        //    {
        //        CloseButtonPressed = false;

        //        LabelFormTitle.Text = _resourceManager.GetString($"HotActions");
        //        LabelFormTitle.BackColor = Color.Green;
        //        tabControl1.SelectedTab = HotPick;
        //    }

        //    Task.Run(() => _logger.LogDetailAsync($"Hot Action Back Button Pressed END"));
        //}

        private async void MBHotAccept_Click(object sender, EventArgs e)
        {
           await Accept();
        }

        private async Task Accept()
        {
            _logger.LogDetailAsync($"Hot Accept Button Pressed START").SafeFireAndForget();
            ReplenOrderDetail orderDetail = null;
            var pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
            if (CheckForOverPick(pickQty)) return;

            Cursor.Current = Cursors.WaitCursor;
            Inventory inv = null;
            var actionCode = ActionCode.PickHot;
            if (_pickList == null)
            {
                actionCode = ActionCode.PickHot;

                _deviceIndicatorManager?.ClearAllDeviceIndicators();

                _workstationView.ProLiteManager?.ClearAllProlites();
                if (_iptiDisplayFunctions != null)
                {
                    await _iptiDisplayFunctions.ClearBatchTable();
                    await _iptiDisplayFunctions.ClearBlastzone();
                    await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                }

                if (_useCostCenter && _hotPickButtonPressed)
                {
                    actionCode = _costCenterActionCode;  // GetHotActionCode();
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
                        AreaId = _currentInventoryView.AreaId,
                        RFID = string.Empty
                    };
                    if (inventory.Quantity > 0 || inventory.StorageTypeId == (int)StorageType.Static)
                    {
                        await _repoInventory.UpdateAsync(inventory);

                        await _locationsRepository.SetLocationInUse(inventory.LocationId, true);
                        inv = _repoInventory.FindByKey(inventory.Id);

                        if (_pickList == null)
                        {
                            _historyManager.SaveHistory(actionCode, inv, pickQty);
                        }
                        else
                        {
                            if (orderDetail != null) orderDetail.PickedQuantity += pickQty;
                            _historyManager.SaveHistory(actionCode, inv, pickQty, _pickList);
                        }

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString($"Message4")}{Environment.NewLine}" +
                                    $"{ex.Message} {Environment.NewLine}" +
                                    $"{ex.InnerException}");
                }
            }
            else  // Existing Inventory Record
            {
                try
                {
                    //inv = _repoInventory.FindByKeyInclude(r => r.Id == _currentInventoryView.Id, inventory =>  new [] {"ItemDefinition", "Location"});
                    inv = _inventoryRepository.GetInventoryById(_currentInventoryView.Id);
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

                            await _repoInventory.UpdateAsync(inv);

                            if (_useCostCenter && _hotPickButtonPressed)
                            {
                                _historyManager.SaveHistory(actionCode, inv, pickQty, _costCenterCode);
                            }
                            else
                            {
                                _historyManager.SaveHistory(actionCode, inv, pickQty);
                            }
                            var inventoryManager = new InventoryManager(_inventoryUnitOfWork, _locationsRepository);
                            var canDelete = inventoryManager.QuickReleaseCheckAsync(inv);
                            if (canDelete)
                            {
                                var sb = new StringBuilder();
                                var item = inv.ItemDefinition != null ? inv.ItemDefinition.Item : string.Empty;
                                var storageType = inv.StorageType != null ? inv.StorageType.Name : string.Empty;
                                sb.AppendLine(
                                    $"Are you sure you want to DELETE the selected Inventory Item? {Environment.NewLine}" +
                                    $"ItemId: {inv.ItemDefinitionId} Item: {item} LocationId: {inv.LocationId}" +
                                    $"Quantity: {inv.Quantity} AreaId: {inv.AreaId} Storage: {storageType} ");

                                var result = MessageBox.Show($"{sb.ToString()}", "Delete Inventory Item",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                                if (result == DialogResult.Yes)
                                {
                                    _logger.LogDetailAsync($"{sb.ToString()}").SafeFireAndForget();
                                    var deleted = await inventoryManager.ReleaseCheckAsync(inv);
                                }
                            }
                        }
                        else
                        {
                            inv.Quantity += pickQty;
                            await _repoInventory.UpdateAsync(inv);
                            if (orderDetail != null) orderDetail.PickedQuantity += pickQty;
                            _historyManager.SaveHistory(actionCode, inv, pickQty, _pickList);
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString($"Message5")}{Environment.NewLine}" +
                                    $"{ex.Message} {Environment.NewLine}" +
                                    $" {ex.InnerException}");
                }
            }



            if (_pickList == null)
            {
                if (inv != null)
                {
                    var invItem = _repoInv.GetInventoryViewById(inv.Id);
                    TextBoxFindItem.Text = invItem.ItemDefinition.Item;
                }

                LoadItemDefinitions();
                LoadCurrentAndNew();
                LabelFormTitle.Text = $"{_resourceManager.GetString($"HotSearch")}";
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
                    await _repoReplenOrderDetails.UpdateAsync(orderDetail);

                    //close the form
                    CloseButtonPressed = true;
                    Close();
                }
                else
                {
                    _quantityToPick = _pickList.Ordered.ParseInt() - orderDetail.PickedQuantity;
                    await _repoReplenOrderDetails.UpdateAsync(orderDetail);
                    TextBoxHotPickQuantity.Text = _quantityToPick.ToString();
                    if (inv != null)
                    {
                        var invItem = _repoInv.GetInventoryViewById(inv.Id);
                        TextBoxFindItem.Text = invItem.ItemDefinition.Item;
                    }
                    LoadItemDefinitions();
                    LoadCurrentAndNew();
                    LabelFormTitle.Text = $"{_resourceManager.GetString($"HotSearch")}";
                    LabelFormTitle.BackColor = Color.Green;
                    Cursor.Current = Cursors.Default;
                    tabControl1.SelectedTab = HotPick;
                }
            }
            _logger.LogDetailAsync($"Hot Accept Button Press END").SafeFireAndForget();
        }

        private bool CheckForOverPick(int pickQty)
        {
            Task.Run(() => _logger.LogDetailAsync("Check For Over Pick"));
            if (!_hotPickButtonPressed) return false;
            if (pickQty <= _currentInventoryView.Quantity) return false;
            MessageBox.Show($"The quantity to pick exceeds the quantity at this location.");
            TextBoxHotPickQuantity.FocusAndHighlightText();

            return true;
        }

        //private ActionCode GetHotActionCode()
        //{
        //    var result = ActionCode.PickHot;
        //    var radioButtons = new List<RadioButton> { RadioButtonPick, RadioButtonWarranty, RadioButtonScrap, RadioButtonOther, RadioButtonCostCenter };
        //    //if (RadioButtonPick.Text == $"{_resourceManager.GetString("Pick")}")
        //    if (_hotPickButtonPressed)
        //    {
        //        foreach (var item in radioButtons)
        //        {
        //            if (item.Checked)
        //            {
        //                switch (item.Tag.ToString())
        //                {
        //                    case "Pick":
        //                        result = ActionCode.PickHot;
        //                        break;
        //                    case "Warranty":
        //                        result = ActionCode.WarrantyHotPick;
        //                        break;
        //                    case "Scrap":
        //                        result = ActionCode.ScrapHotPick;
        //                        break;
        //                    case "Other":
        //                        result = ActionCode.OtherHotPick;
        //                        break;
        //                    case "Cost Center":
        //                        result = ActionCode.CostCenterHotPick;
        //                        break;
        //                    default:
        //                        result = ActionCode.PickHot;
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    //else if (RadioButtonPick.Text == $"{_resourceManager.GetString($"Store")}")
        //    else if (_hotStoreButtonPressed)
        //    {
        //        result = ActionCode.StoreHot;
        //        //foreach (var item in radioButtons)
        //        //{
        //        //    if (item.Checked)
        //        //    {
        //        //        switch (item.Tag.ToString())
        //        //        {
        //        //            case "Store":
        //        //                result = ActionCode.StoreHot;
        //        //                break;
        //        //            case "Warranty":
        //        //                result = ActionCode.WarrantyHotStore;
        //        //                break;
        //        //            case "Scrap":
        //        //                result = ActionCode.ScrapHotStore;
        //        //                break;
        //        //            case "Other":
        //        //                result = ActionCode.OtherHotStore;
        //        //                break;
        //        //            case "Cost Center":
        //        //                result = ActionCode.CostCenterHotStore;
        //        //                break;
        //        //            default:
        //        //                result = ActionCode.StoreHot;
        //        //                break;
        //        //        }
        //        //    }
        //        //}
        //    }
        //    return result;
        //}
        private void MBHotActionClose_Click(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"Hot Action Close Button Pressed").SafeFireAndForget();
            CloseButtonPressed = true;
        }
        private void PictureBoxItemHotImage_MouseEnter(object sender, EventArgs e)
        {
            if (_neutronVariables.AutoEnlargeImage)
            {
                PictureBoxItemHotImage.Location = new Point(170, 162);
                PictureBoxItemHotImage.Size = new Size(512, 512);
                PictureBoxItemHotImage.BringToFront();
            }
        }
        private void PictureBoxItemHotImage_MouseLeave(object sender, EventArgs e)
        {
            if (_neutronVariables.AutoEnlargeImage)
            {
                PictureBoxItemHotImage.Location = new Point(426, 418);
                PictureBoxItemHotImage.Size = new Size(256, 256);
                PictureBoxItemHotImage.BringToFront();
            }
        }
        private async void FrmHotAction_KeyDown(object sender, KeyEventArgs e)
        {
            _logger.LogDetailAsync($"Hot Action Key Down Key Pressed: {e.KeyCode} START").SafeFireAndForget();

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
                            // var searchSlot = GlobalVar.SlotNameFactory.CreateSearchString(slot);
                            // TextBoxScanLocation.Text = searchSlot;
                            LoadNewLocationsBySlot(slot);
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
                case Keys.Tab:
                    {
                        if (TextBoxFindItem.Focused)
                        {
                            FindItem();
                        }
                        else if (TextBoxScanLocation.Focused)
                        {
                            var slot = TextBoxScanLocation.Text;
                            // var searchSlot = GlobalVar.SlotNameFactory.CreateSearchString(slot);
                            // TextBoxScanLocation.Text = searchSlot;
                            LoadNewLocationsBySlot(slot);
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
                        //await Accept();
                        break;
                    }
                case Keys.F2:
                    {
                        PrintLabel();
                        break;
                    }
                case Keys.F12:
                    {
                        Hide();
                        using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables, _iptiDisplayFunctions))
                        {
                            frm.ShowDialog();
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
            //    using (MetroForm frm = new FrmInventory(_jsonData, _workstationView, _akaRepository, _lacProcessor))
            //    {
            //        var result = frm.ShowDialog();
            //        Show();
            //    }
            //}
            _logger.LogDetailAsync($"Hot Action Key Down END").SafeFireAndForget();
        }

        private void MBCurrentLocations_Click(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"Current Locations Pressed START").SafeFireAndForget();
            MBHotPick.Enabled = true;
            MBHotStore.Enabled = true;
            SetupGridCurrent();
            DataGridViewHot.DataSource = _bindingSourceCurrent;
            GetRecordCount(_bindingSourceCurrent);
            _logger.LogDetailAsync($"Current Locations Pressed END").SafeFireAndForget();
        }
        private void MBNewLocations_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.LogDetailAsync("New Locations Pressed START"));
            MBHotPick.Enabled = false;
            MBHotStore.Enabled = true;
            SetupGridNew();
            DataGridViewHot.DataSource = _bindingSourceNewLocations;
            GetRecordCount(_bindingSourceNewLocations);
            Task.Run(() => _logger.LogDetailAsync("New Locations Pressed END"));
        }

        private void LabelHotPickItem_Click(object sender, EventArgs e)
        {
            EditItemDefinition(_currentInventoryView.ItemDefinitionId);
        }
        private void ButtonEditItemDefinition_Click(object sender, EventArgs e)
        {
            EditItemDefinition(_currentInventoryView.ItemDefinitionId);
        }
        private void EditItemDefinition(int id)
        {
            Hide();
            using (var frm = new FrmEditItemDefinition(id, _historyManager))
            {
                var result = frm.ShowDialog();
                Show();
            }

        }
        private void ButtonEditLocationDefinition_Click(object sender, EventArgs e)
        {
            EditLocationDefinition(_currentInventoryView.LocationId);
        }
        private void EditLocationDefinition(int id)
        {
            Hide();
            using (var frm = new FrmEditLocationDefinition(id))
            {
                var result = frm.ShowDialog();
                Show();
            }
            UpdateHotPickScreen(_currentInventoryView);
        }
        private async void ComboBoxSizeCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.ItemDefinitions.FirstOrDefault(r => r.Id == _currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                rec.SizeCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
            }
        }
        private async void ComboBoxVelocityCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.ItemDefinitions.FirstOrDefault(r => r.Id == _currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);

                rec.VelocityCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
            }
        }
        private async void ComboBoxHeightCodeItem_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.ItemDefinitions.FirstOrDefault(r => r.Id == _currentInventoryView.ItemDefinitionId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                rec.HeightCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
            }
        }

        private async void ComboBoxSizeCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.Locations.FirstOrDefault(r => r.Id == _currentInventoryView.LocationId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                rec.SizeCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
            }
        }
        private async void ComboBoxVelocityCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.Locations.FirstOrDefault(r => r.Id == _currentInventoryView.LocationId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                rec.VelocityCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
            }
        }
        private async void ComboBoxHeightCodeLocation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var box = (ComboBox)sender;
            using (var db = new NeutronDb())
            {
                var rec = db.Locations.FirstOrDefault(r => r.Id == _currentInventoryView.LocationId);
                if (rec == null) return;
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                rec.HeightCodeId = (int)box.SelectedValue;
                await db.SaveChangesAsync();
                await _historyManager.SaveHistoryAsync(ActionCode.ItemModify, rec);
                UpdateHotPickScreen(_currentInventoryView);
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
            _historyManager.SaveHistory(ActionCode.InventoryModify, inv, prevQty, true);
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
            _historyManager.SaveHistory(ActionCode.LocationCount, cnt);
        }
        private void MBHotActionCount_Click(object sender, EventArgs e)
        {
            var inventoryId = _currentInventoryView.Id;
            var qty = OpenLocationCountForm(inventoryId);
            if (qty < 0) return;
            TextBoxHotPickLocationQuantity.Text = qty.ToString();
        }

        private void TextBoxHotPickQuantity_Leave(object sender, EventArgs e)
        {
            if (!_useCostCenter) return;
            var tb = TextBoxHotPickQuantity.Text;
            ShowShi(_currentInventoryView.Loc1, _currentInventoryView.Loc2, _currentInventoryView.Loc3, _currentInventoryView.Loc4.ToString(), tb);
        }
        //private void TextBoxFindCostCenter_Leave(object sender, EventArgs e)
        //{
        //    if (!_useCostCenter) return;
        //    var search = TextBoxFindCostCenter.Text;
        //    var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
        //    MBHotAccept.Enabled = costCenterList.Count > 0;

        //    ComboBoxCostCenter.DataSource = costCenterList;
        //    ComboBoxCostCenter.DisplayMember = "Name";
        //    ComboBoxCostCenter.ValueMember = "Code";
        //    //ComboBoxCostCenter.DroppedDown = true;
        //}
        //private void ComboBoxCostCenter_TextChanged(object sender, EventArgs e)
        //{
        //    if (!_useCostCenter) return;
        //    MBHotAccept.Enabled = false;
        //    try
        //    {
        //        var t = ((ComboBox)sender).SelectedItem;
        //        if (t != null)
        //        {
        //            var cost = (CostCenter)t;
        //            var n = cost.Name;
        //            if (!string.IsNullOrEmpty(n))
        //            {
        //                MBHotAccept.Enabled = true;
        //            }
        //        }
        //        else
        //        {
        //            MBHotAccept.Enabled = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Task.Run(() => _logger.LogDetailAsync($"Error reading Cost Center Text Changed: {ex.Message} {Environment.NewLine} {ex.InnerException}"));
        //        MBHotAccept.Enabled = true;
        //    }
        //}
        //private void TextBoxFindCostCenter_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //}
        //private void TextBoxFindCostCenter_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (!_useCostCenter) return;
        //    if (e.KeyData == Keys.Enter)
        //    {
        //        //file CostCenter Combo Box
        //        var search = TextBoxFindCostCenter.Text;
        //        var costCenterList = _costCenterManager.GetCostCenterList(search.ToLower());
        //        MBHotAccept.Enabled = costCenterList.Count > 0;
        //        ComboBoxCostCenter.DataSource = costCenterList;
        //        ComboBoxCostCenter.DisplayMember = "Name";
        //        ComboBoxCostCenter.ValueMember = "Code";
        //        ComboBoxCostCenter.DroppedDown = true;
        //    }
        //}
        //private void HotAction_Enter(object sender, EventArgs e)
        //{
        //    if (!_useCostCenter) return;
        //    TextBoxFindCostCenter.Focus();
        //}


        private void UpdateCurrentDeviceIndicator()
        {
            Task.Run(() => _logger.LogDetail("Update Current Device Indicator START"));
            // Task.Run(() => _logger.LogDetailAsync("Update Current Device Indicator START"));
            ClearActiveDeviceIndicators();
            var loc1 = _currentInventoryView.Loc1;
            _deviceIndicators[loc1].BlinkOn();
            _deviceIndicators[loc1].Active = true;
            //Task.Run(() => _logger.LogDetailAsync("Update Current Device Indicator END"));
            _logger.LogDetail("Update Current Device Indicator END");
        }

        private void ClearActiveDeviceIndicators()
        {
            _logger.LogDetail("Clear Active Device Indicators START");
            //Task.Run(() => _logger.LogDetailAsync("Clear Active Device Indicators START"));
            var devices = _deviceIndicators.Where(x => x.Value.Active == true).ToList();
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in devices)
            {
                _logger.LogDetail($"Clear Active Device Indicator: {deviceIndicator.Value.DeviceNumber}");
                // Task.Run(() => _logger.LogDetailAsync($"Clear Active Device Indicator: {deviceIndicator.Value.DeviceNumber}"));
                deviceIndicator.Value.BlinkOff();
                deviceIndicator.Value.Active = false;
            }
            _logger.LogDetail("Clear Active Device Indicators END");
            //Task.Run(() => _logger.LogDetailAsync("Clear Active Device Indicators END"));
        }

        private void ClearAllDeviceIndicators()
        {
            _logger.LogDetail("Clear ALL Active Device Indicators START");
            //Task.Run(() => _logger.LogDetailAsync("Clear ALL Active Device Indicators START"));
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in _deviceIndicators)
            {
                _logger.LogDetail($"Clear ALL Active Device Indicator: {deviceIndicator.Value.DeviceNumber}");
                // Task.Run(() => _logger.LogDetailAsync($"Clear ALL Active Device Indicator: {deviceIndicator.Value.DeviceNumber}"));
                deviceIndicator.Value.Active = false;
                deviceIndicator.Value.BlinkOff();
            }
            _logger.LogDetail("Clear ALL Active Device Indicators END");
            //Task.Run(() => _logger.LogDetailAsync("Clear ALL Active Device Indicators END"));
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

                HotPick.Text = _resourceManager.GetString($"HotPick");
                CheckBoxAll.Text = _resourceManager.GetString($"All");
                MBHotStore.Text = _resourceManager.GetString($"HotStore");
                MBHotPick.Text = _resourceManager.GetString($"HotPick");
                LabelSearch.Text = _resourceManager.GetString($"SearchforanypartofIt");
                MBHotActionClose.Text = _resourceManager.GetString($"Close");
                MBNewLocations.Text = _resourceManager.GetString($"NewLocations");
                MBCurrentLocations.Text = _resourceManager.GetString($"CurrentLocations");
                MBFindItem.Text = _resourceManager.GetString($"Search");
                HotAction.Text = _resourceManager.GetString($"HotAction");
                ButtonEditItemDefinition.Text = _resourceManager.GetString($"Edit");
                LabelMainHeight.Text = _resourceManager.GetString($"Height");
                LabelMainVelocity.Text = _resourceManager.GetString($"Velocity");
                LabelMainSize.Text = _resourceManager.GetString($"Size");
                LabelMainUnitOfIssue.Text = _resourceManager.GetString($"UnitofIssue");
                LabelMainQuantity.Text = _resourceManager.GetString($"Qty");
                LabelMainItem.Text = _resourceManager.GetString($"Item");
                LabelMainDescription.Text = _resourceManager.GetString($"Desc");
                GroupBoxHotPickLocation.Text = _resourceManager.GetString($"Location");
                LabelLocationCode.Text = _resourceManager.GetString($"LocationCode");
                ButtonEditLocationDefinition.Text = _resourceManager.GetString($"Edit");
                LabelHeight.Text = _resourceManager.GetString($"Height");
                LabelVelocity.Text = _resourceManager.GetString($"Velocity");
                LabelSize.Text = _resourceManager.GetString($"Size");
                LabelReceivedDate.Text = _resourceManager.GetString($"ReceivedDate");
                LabelStaticRelease.Text = _resourceManager.GetString($"StaticLocation");
                LabelPrimeBin.Text = _resourceManager.GetString($"PrimeBin");
                LabelLocationQuantity.Text = _resourceManager.GetString($"LocationQuantity");
                LabelBack.Text = _resourceManager.GetString($"Back");
                LabelOver.Text = _resourceManager.GetString($"Over");
                LabelTray.Text = _resourceManager.GetString($"Tray");
                LabelDevice.Text = _resourceManager.GetString($"Device");
                MBHotActionCount.Text = _resourceManager.GetString($"LocationCount");
                MBHotAccept.Text = _resourceManager.GetString($"Accept");
                MBHotActionBack.Text = _resourceManager.GetString($"Back");
                LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
                mlUserInfo.Text = _resourceManager.GetString($"Login?");
                LabelFormHeaderText.Text = _resourceManager.GetString($"NeutronWarehouseMana");
                LabelCostCenter.Text = _resourceManager.GetString($"CostCenter");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file.  {ex.Message} {Environment.NewLine} {ex.InnerException} ");
            }
        }

        //private void panel2_Paint(object sender, PaintEventArgs e)
        //{
        //    ControlPaint.DrawBorder(e.Graphics, panel1.ClientRectangle,
        //        Color.Black, 10, ButtonBorderStyle.Dashed, // left
        //        Color.Black, 10, ButtonBorderStyle.Dashed, // top
        //        Color.Black, 10, ButtonBorderStyle.Dashed, // right
        //        Color.Black, 10, ButtonBorderStyle.Dashed);// bottom
        //}

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

        private void TextBoxScanLocation_TextChanged(object sender, EventArgs e)
        {

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            Back();
        }

        /// <summary>
        /// Done
        /// </summary>
        /// <param name="bayController"></param>
        /// <param name="position"></param>
        /// <param name="text"></param>
        private void TurnOnIptiDisplay(int bayController, int position, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.IptiDisplays)
                {
                    if (_iptiDisplayFunctions == null) return;
                    // var command = _tcpIptiCommandCenter.TurnOnDisplay(bayController.ToString(), position, text);
                    _iptiDisplayFunctions?.TurnOnBlastzoneDisplay(bayController, position, text);
                }
            }
        }

        private void TurnOnIptiOrderControl(int bayController, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.IptiDisplays)
                {
                    if (GlobalVar.Displays == null) return;

                    // var bayId = bayController.ToString().PadLeft(2, '0');
                    // var command = _tcpIptiCommandCenter.GetBayController(bayId)
                    //     .TurnOnOrderControlModule(text);
                    // GlobalVar.Displays.SendText(command);
                    if (_iptiDisplayFunctions != null)
                    {
                        _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(bayController, text);
                    }
                }
            }
        }

        //private void ClearBatchTableAsync()
        //{
        //    Task.Run(() =>  _logger.LogDetailAsync($"Clear Batch Table Function - START"));

        //    if (_neutronVariables.IptiDisplays)
        //    {
        //        if (GlobalVar.Displays == null) return;
        //        var bayId = _neutronVariables.BliController.ToString();

        //        var command = _tcpIptiCommandCenter.ClearBayController(bayId);
        //        GlobalVar.Displays.SendText(command);

        //        command = _tcpIptiCommandCenter.GetBayController(bayId).TurnOffOrderControlModule();
        //        GlobalVar.Displays.SendText(command);

        //    }

        //    Task.Run(() =>  _logger.LogDetailAsync($"Clear Batch Table Function - END"));
        //}


        /// <summary>
        /// Done
        /// </summary>
        /// <param name="bayController"></param>
        /// <param name="position"></param>
        /// <param name="beacon"></param>
        /// <param name="text"></param>
        private void TurnOnBlastzone(int bayController, int position, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.IptiDisplays)
                {
                    if (_iptiDisplayFunctions != null)
                    {
                        _iptiDisplayFunctions?.TurnOnBlastzoneDisplay(bayController, position, text);
                    }
                    //if (GlobalVar.Displays == null) return;

                    // var command = _tcpIptiCommandCenter.TurnOnDisplay(bayController.ToString(), position, text);
                    //GlobalVar.Displays.SendText(command);
                    // GlobalVar.Displays.ShowBlastzone(bayController, position, beacon, text);
                }
            }
        }

        private void LabelStationName_Click(object sender, EventArgs e)
        {

        }

        private void LabelStationName2_Click(object sender, EventArgs e)
        {

        }

        private void ButtonPreviousPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                TextBoxGoTo.Text = _currentPage.ToString();
                LoadDataGrid();
            }
        }

        private void ButtonNextPage_Click(object sender, EventArgs e)
        {
            _currentPage++;
            TextBoxGoTo.Text = _currentPage.ToString();
            LoadDataGrid();
        }

        private void ButtonGoTo_Click(object sender, EventArgs e)
        {
            _currentPage = TextBoxGoTo.Text.ParseInt();
            LoadDataGrid();
        }

        private void GetTotalPages(string findWhat)
        {
            var count = _itemDefinitionsRepository.TotalItemDefinitionViewsByArea(findWhat,
                _workstationView.AreaId);
            var countmod15 = count % 15 == 0 ? count / 15 : count / 15 + 1;

            TextBoxTotalPages.Text = countmod15.ToString();
        }

        private void LoadDataGrid()
        {
            switch (_currentGridDataType)
            {
                case GridDataType.Item:
                    LoadItemDefinitions();
                    break;
                case GridDataType.New:
                    //LoadCurrentLocations();
                    break;
                case GridDataType.Current:
                    //LoadNewLocations();
                    break;
                case GridDataType.None:
                    //LoadHotPick();
                    break;
            }
        }
    }
}
