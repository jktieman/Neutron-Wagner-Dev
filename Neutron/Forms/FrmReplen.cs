using LabelDetail = NeutronData.PrintModels.LabelDetail;
using IPTI.Models;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Classes;
using Neutron.Global;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.PrintModels;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronCore.Enums;
using Neutron.Interfaces;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using AsyncAwaitBestPractices;
using DeviceIndicatorService;
using Neutron.Enums;
using Neutron.Ninject;
using Neutron.UserControls;
using NeutronCore.Extensions;
using NeutronEvents;
using NeutronDllu;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using LineStatus = NeutronCore.Enums.OrderStatus;
using Panel = System.Windows.Forms.Panel;
using ScrollBars = System.Windows.Forms.ScrollBars;
using StorageType = NeutronData.Models.Lookups.StorageType;
using TextBox = System.Windows.Forms.TextBox;
using NeutronCore.StaticClasses;
using NeutronData.UnitOfWorks;


namespace Neutron.Forms
{
    public partial class FrmReplen : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;

        private ResourceManager _gridResourceManager;

        private readonly AkaRepository _repoAka = new AkaRepository();
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        private readonly GenericRepository<StorageType> _repoStorageTypes = new GenericRepository<StorageType>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        // private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocationRepository = new GenericRepository<Location>(new NeutronDb());



        private readonly GenericRepository<SizeCode> _repoSizeCodes = new GenericRepository<SizeCode>(new NeutronDb());

        private readonly GenericRepository<VelocityCode> _repoVelocityCodes = new GenericRepository<VelocityCode>(new NeutronDb());


        private readonly IReplenOrdersRepository _replenOrdersRepository;
        private ReplenOrderDetailsRepository _orderDetailsRepository;
        //private readonly GenericRepository<Workstation> _repoStation = new GenericRepository<Workstation>(new NeutronDb());
        private readonly IWorkstationRepository _workstationRepository;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;
        private readonly GenericRepository<PrintJob> _repoPrintJob = new GenericRepository<PrintJob>(new NeutronDb());
        private readonly BindingSource _bindingSourceOrderView = new BindingSource();
        private readonly BindingSource _bindingSourceCompleted = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrders = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrdersRack = new BindingSource();
        private readonly BindingSource _bindingSourcePickViews = new BindingSource();

        private readonly BindingSource _bindingSourcePickStops = new BindingSource();

        //private readonly BindingSource _bindingSourceHot = new BindingSource();
        private readonly BindingSource _bindingSourceOrderDetailsView = new BindingSource();
        private readonly BindingSource _bindingSourceItems = new BindingSource();
        private readonly BindingSource _bindingSourceNewItems = new BindingSource();
        private readonly BindingSource _bindingSourceNewLocations = new BindingSource();

        private BindingListView<AvailableReplenOrdersView> _bindingListViewAvailableOrdersViews;

        //Assigned but never used
        public bool CloseButtonPressed { get; set; }
        public ReplenOrderView CurrentItem;
        public RackReplenOrderView CurrentRackItem;
        private AvailableReplenOrdersView _currentAvailableOrdersView;
        private bool _gridClickedAvailableOrders;
        private bool ManualOverrideCurrentTextBoxPos;
        private TextBox _currentTextBoxPos;
        private bool _manualOverrideCurrentTextBoxPos;
        private List<BatchPosition> _ordersToPick = new List<BatchPosition>();
        private ReplenPickStop _currentPickStop = new ReplenPickStop();

        //private bool openHotPickFromPickScreen = false;
        //private bool openHotStoreFromPickScreen = false;
        //private InterfaceProcessorTmg _interfaceProcessorTmg;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IHistoryManager _historyManager;
        private readonly IAreaRepository _areaRepository;
        private readonly ILocationsRepository _locationsRepository;
        private IDynamicLogger _logger;
        private ReplenDeviceManager _deviceManager;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;
        private DocumentToPrint _documentToPrint;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private readonly IAkaRepository _akaRepository;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;
        private StorageType _defaultStorageType;

        private CurrentDataSet _currentDataSet;
        private List<Inventory> _currentInventory;
        private int _numberOfInventoryLocations = 5;
        private List<Location> _tempAllocatedLocations;
        private DeviceIndicatorManager _deviceIndicatorManager;

        // private Dictionary<int, DeviceIndicator> _deviceIndicators;
        private string _activeGrid = "Available";
        private readonly int[] _moveableDeviceTypes;

        //GRIDS
        private bool _orderGridReady;
        private bool _availableOrdersGridReady;
        private bool _pickViewGridReady;
        private bool _orderDetailsGridReady;
        private bool _newOrderGridReady;
        private bool _newItemsGridReady;
        private bool _availableOrdersRackGridReady;
        private bool _adjustGridReady;

        private SynchronizationContext _synchronizationContext;
        private IIptiDisplayFunctions _iptiDisplayFunctions;
        private readonly IptiConfig _iptiConfig;
        private readonly IDialogService _dialogService;

        private List<HardwareDevice> _blastzones;
        private bool _blastzone;
        private HardwareDevice _batchTables;
        private bool _batchTable;
        private bool _prolite;
        private bool _hanel;
        private int _currentJobDetailsOrderId;
        private TabPage _previousTab;
        private bool _spaceBarDisabled;
        private bool _multiLocationStop;
        private List<SizeCode> _sizeCodes;
        private List<VelocityCode> _velocityCodes;
        private List<HeightCode> _heightCodes;
        private bool _isClientConnected;
        private const int AreaEight = AreaNumber.Eight;

        private System.Windows.Forms.Timer _debounceTimer;
        private TextBox _textBoxPos;
        private const int DebounceInterval = 300; // milliseconds

        public FrmReplen(IJsonData jsonData, WorkstationView workstationView
            , IAkaRepository akaRepository, NeutronVariables neutronVariables
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IWorkstationRepository workstationRepository
            , IReplenOrdersRepository replenOrdersRepository, NeutronLicense neutronLicense
            , IItemDefinitionsRepository itemDefinitionsRepository, IHistoryManager historyManager
            , ILocationsRepository locationsRepository
            , IAreaRepository areaRepository, IInventoryRepository inventoryRepository, IInventoryUnitOfWork inventoryUnitOfWork
            , IIptiDisplayFunctions iptiDisplayFunctions
            , IptiConfig iptiConfig
            , IDialogService dialogService)

        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            _jsonData = jsonData;
            _workstationView = workstationView;
            _akaRepository = akaRepository;
            _neutronVariables = neutronVariables;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _workstationRepository = workstationRepository;
            _replenOrdersRepository = replenOrdersRepository;
            _neutronLicense = neutronLicense;
            _itemDefinitionsRepository = itemDefinitionsRepository;
            _historyManager = historyManager;
            _locationsRepository = locationsRepository;
            _areaRepository = areaRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryUnitOfWork = inventoryUnitOfWork;
            _iptiDisplayFunctions = iptiDisplayFunctions;
            _iptiConfig = iptiConfig;
            _dialogService = dialogService;

            _currentInventory = new List<Inventory>();

            InitForm();


            //
            //_moveableDeviceTypes = _workstationRepository.GetMoveableDeviceTypeIds();

            //LoadInventory();
            //_documentToPrint = new DocumentToPrint();
            //ShowButtons();
            //HideTabControlTabs();
            //mlUserInfo.Text = GlobalVar.User?.UserInfo;
            //CloseButtonPressed = false;
            //_currentTextBoxPos = TextBoxPos1;

            //InitDataGridViewNewItems();
            //_defaultStorageType = new NeutronData.Models.Lookups.StorageType { Id = 2, Name = "Release", Sequence = 20 };
        }

        private void InitForm()
        {
            KeyPreview = true;
            _logger = NeutronCore.Global.Logger.SetupLogger("Store");
            _logger.LogDetailAsync($"Form Replen Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();

            //_iptiDisplayFunctions =
            //    new IptiDisplayFunctions(_jsonData, _neutronVariables, _workstationView, _tcpIptiController);

            SetupPrinters();
            _synchronizationContext = SynchronizationContext.Current;
            InitGrids();

            SetupPickPositions(_neutronVariables.StoreBatchSize, _neutronVariables.StoreBatchRows);
            InitOrdersToPick(_neutronVariables.StoreBatchSize);
            HideTabControlTabs();
            ShowButtons();

            mlUserInfo.Text = $"{_resourceManager.GetString($"CurrentUser")}{GlobalVar.User?.UserInfo}";
            CloseButtonPressed = false;
            _currentTextBoxPos = (TextBox)Controls.Find($"TextBoxPos1", true).First();
            //ToolTipPickScreen.SetToolTip(ButtonMove, _resourceManager.GetString($"GetBin"));


            _orderDetailsRepository = new ReplenOrderDetailsRepository();

            _documentToPrint = new DocumentToPrint();
            MBPrint.Visible = _neutronVariables.PrintPackingListManual;

            //InitDataGridViewNewItems();

            MBPickScreenHotPick.Enabled = _securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions];

            //if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor ||
            //    _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.RackTablet)
            //{
            //    MBMainAvailableOrders.Text = _resourceManager.GetString($"OffCarousel");
            //}
            if (_workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor) MBMainAvailableOrders.Visible = false;


            _defaultStorageType = _repoStorageTypes.FindByKey(_neutronVariables.DefaultStorageTypeId);
            //_tempAllocatedLocations = new List<Location>();

            MBMainAvailableOrders.Text = $"{_resourceManager.GetString($"AvailableOrders")} - {_workstationView.Area.Name}";

            if (_workstationView.Hanels.Any())
            {
                InitDeviceIndicators();
            }

            _sizeCodes = LoadSizeCodesByArea(_workstationView.AreaId);
            _velocityCodes = LoadVelocityCodesByArea(_workstationView.AreaId);
            _heightCodes = LoadHeightCodesByArea(_workstationView.AreaId);


            _debounceTimer = new System.Windows.Forms.Timer();
            _debounceTimer.Interval = DebounceInterval;
            _debounceTimer.Tick += DebounceTimer_Tick;

            Mediator.GetInstance().IptiButtonPressed += (s, e) => IptiButtonPickAccept(e.ResponseInfo);
            Mediator.GetInstance().OrderComplete += (s, e) => ShowOrderComplete(e.Order);
            Mediator.GetInstance().TrayInPosition += (s, e) => UpdateTraysInPosition(e.InPositionInfo);
            Mediator.GetInstance().TransmitStateChanged += (s, e) => UpdateClientConnected(e.State);
            Mediator.GetInstance().IsClientConnected += FrmReplen_IsClientConnected;
        }

        private async void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();

            await ProcessInput(_textBoxPos);

        }

        /// <summary>
        /// Attaches event handlers to the <see cref="TextBox"/> controls within the <see cref="PanelOrderInduction"/> panel.
        /// </summary>
        /// <remarks>
        /// This method iterates through all <see cref="UserControlPickPosition"/> controls within the <see cref="PanelOrderInduction"/> panel,
        /// and attaches event handlers to the <see cref="TextBox"/> controls found within these user controls.
        /// </remarks>
        private void AttachTextBoxEvents()
        {
            foreach (var userControl in PanelOrderInduction.Controls.OfType<UserControlPickPosition>())
            {
                foreach (var panel in userControl.Controls.OfType<Panel>())
                {
                    var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
                    if (textBox == null) continue;
                    textBox.Click += TextBoxPos_Click;
                    //textBox.TextChanged += TextBoxPos_TextChanged;
                    textBox.KeyPress += CheckKeyPress;
                    textBox.KeyDown += TextBox_KeyDown;
                    //textBox.Enter += (sender, e) => SetCurrentTextBoxPos(textBox);
                    textBox.Enter += TextBoxEnter;
                }
            }
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                // convert _currentTextBoxPos.Tag to an int
                int batchPosition = int.Parse((string)_currentTextBoxPos.Tag);
                var index = batchPosition - 1;

                if (_ordersToPick[index].OrderId != 0)
                {
                    // get the row in DataGridViewAvailableOrders where Ord1 = _ordersToPick[index].Ord1 and Ord2 = _ordersToPick[index].Ord2
                    var row = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(r =>
                            Convert.ToInt32(r.Cells["Id"].Value) == _ordersToPick[index].OrderId);
                    if (row != null)
                    {
                        var id = Convert.ToInt32(row.Cells["Id"].Value);
                        var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                        var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                        var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
                    }
                }
                else
                {
                    _currentTextBoxPos.Text = string.Empty;
                }

                e.Handled = true;

                // Handle the Delete key press
                //MessageBox.Show("Delete key pressed");
                // Optionally, you can suppress the default behavior
                //e.SuppressKeyPress = true;
            }
        }

        private async Task HandleScannerInput(TextBox textBox)
        {
            await ProcessInput(textBox);
        }

        private async Task HandleKeyboardInput(TextBox textBox)
        {
            await ProcessInput(textBox);
        }

        private async void CheckKeyPress(object sender, KeyPressEventArgs e)
        {
            var textBox = (TextBox)sender;
            switch (e.KeyChar)
            {
                case (char)Keys.Return:
                    e.Handled = true;
                    await ProcessInput(textBox);
                    break;
                case (char)Keys.Tab:
                    textBox.Parent.SelectNextControl(textBox, true, true, true, true);
                    break;
                case (char)Keys.Delete:
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        if (int.TryParse(textBox.Tag?.ToString(), out var positionNumber))
                        {
                            if (_ordersToPick != null)
                            {
                                var batchPosition = _ordersToPick.FirstOrDefault(o => o.PositionNumber == positionNumber);
                                await ResetBatchPosition(batchPosition);
                                textBox.Focus();
                                _currentTextBoxPos = textBox;
                            }
                        }
                    }
                    break;
            }
        }
        private async Task ProcessInput(TextBox input)
        {
            if (string.IsNullOrEmpty(input.Text))
            {
                //clear the position
                var position = int.Parse(input.Tag.ToString());
                await ClearItemFromBatchByPosition(position);
            }
            else
            {
                // get the Order based on the input.Text
                var order = await _repoReplenOrder.FindByFirstOrDefaultAsync(r => r.Ord1 == input.Text);
                if (order == null)
                {
                    var position = int.Parse(input.Tag.ToString());
                    await ClearItemFromBatchByPosition(position);
                    return;
                }

                var arrayPosition = AddItemToBatch(order.Id, order.Ord1, order.Ord2);
                SetFocusNextTextBoxPos();
            }

            // Handle the debounced input here
            // MessageBox.Show($"Processed input: {input.Text}  Tag: {input.Tag}");
        }

        private void SetFocusNextTextBoxPos()
        {
            //_previousTextBoxPos = _currentTextBoxPos;
            if (_currentTextBoxPos != null)
            {
                _currentTextBoxPos.BackColor = Color.White;
            }
            if (InvokeRequired)
            {
                Invoke(new Action(SetFocusNextTextBoxPos));
                return;
            }

            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                if (_ordersToPick[i].OrderId == 0)
                {
                    _currentTextBoxPos = FindTextBox($"TextBoxPos{_ordersToPick[i].PositionNumber}");
                    _currentTextBoxPos.Text = string.Empty;
                    _currentTextBoxPos.BackColor = Color.Yellow;
                    _currentTextBoxPos.Focus();
                    _currentTextBoxPos.Select();
                    _currentTextBoxPos.Refresh();
                    //exit the for loop
                    break;
                }
            }
        }

        private TextBox FindTextBox(string name)
        {
            return Controls.Find(name, true).FirstOrDefault() as TextBox;
        }
        private void FrmReplen_IsClientConnected(object sender, IsClientConnectedEventArgs e)
        {
            if (_isClientConnected.Equals(e.IsClientConnected)) return;
            _isClientConnected = e.IsClientConnected;
            UpdateClientConnected(_isClientConnected);
        }

        private void UpdateClientConnected(bool state)
        {
            _logger.LogDetailAsync($"Client connection state updated: {state}").SafeFireAndForget();
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(UpdateClientConnected), state);
                return;
            }

            CheckBoxClientConnected.Checked = state;
        }
        private void UpdateTraysInPosition(InPositionInfo inPositionInfo)
        {
            if (inPositionInfo.OneInPosition)
            {
                MBStoreAccept.ForeColor = Color.DarkGreen;
                MBStoreAccept.Refresh();
            }
            if (inPositionInfo.TwoInPosition)
            {
                MBStoreAccept.ForeColor = Color.Blue;
            }
            if (inPositionInfo.ThreeInPosition)
            {
                MBStoreAccept.ForeColor = Color.Red;
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parms = base.CreateParams;
                parms.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                //parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return parms;
            }
        }

        private void ShowOrderComplete(Order order)
        {
            _logger.LogDetailAsync($"Show Order Complete Event: Order Number _ {order.Ord1} -- {order.Ord2}").SafeFireAndForget();
            var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == order.Id);
            if (bp == null) return;
            var pos = bp.PositionNumber.ToString();
            Control c = Controls.Find("Pos" + pos + "Display", true).First();
            if (c != null)
            {
                var panel = ((Panel)c);
                panel.BackColor = Color.Green;
                panel.Visible = true;
                panel.Refresh();
            }

            bp.OrderComplete = true;
        }

        public Task IptiButtonPickAccept(ResponseInfo responseInfo)
        {
            _logger.LogDetailAsync($"IptiButtonPickAccept Display Number:  {responseInfo.DisplayNumber}").SafeFireAndForget();
            return StoreAccept();
        }

        private void SetupPickPositions(int pickBatchSize, int pickBatchRows = 1)
        {
            InitializePanelPositions(PanelOrderPositions, PanelType.OrderSelection, pickBatchRows, pickBatchSize);
            InitializePanelPositions(PanelOrderInduction, PanelType.OrderInduction, pickBatchRows, pickBatchSize);
            AttachTextBoxEvents();
        }

        /// <summary>
        /// Initializes the positions of the panels based on the specified parameters.
        /// </summary>
        /// <param name="panel">The panel to be initialized.</param>
        /// <param name="panelType">The type of the panel being initialized.</param>
        /// <param name="rows">The number of rows to be created in the panel.</param>
        /// <param name="positions">The number of positions to be created in the panel.</param>
        private void InitializePanelPositions(Panel panel, PanelType panelType, int rows, int positions)
        {
            var panelManager = new PanelManager(panel, panelType);
            panel = panelManager.AddPositions(rows, positions);
        }

        //private void SetupPickPositions(int pickBatchSize, int pickBatchRows)
        //{
        //    var rows = pickBatchRows;
        //    var positions = pickBatchSize;

        //    var panelManager = new PanelManager(PanelOrderPositions, PanelType.OrderSelection);
        //    PanelOrderPositions = panelManager.AddPositions(rows, positions);

        //    var inductionPanelManager = new PanelManager(PanelOrderInduction, PanelType.OrderInduction);
        //    PanelOrderPositions = inductionPanelManager.AddPositions(rows, positions);

        //    // Events

        //    foreach (var userControl in PanelOrderInduction.Controls.OfType<UserControlPickPosition>())
        //    {
        //        foreach (var panel in userControl.Controls.OfType<Panel>())
        //        {
        //            var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
        //            if (textBox == null) continue;
        //            textBox.KeyPress += new KeyPressEventHandler(CheckEnterKeyPress);
        //        }

        //    }
        //}

        private async Task CheckEnterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                SendKeys.Send(keys: "{Tab}");
                await NextButtonEnabled();
                e.Handled = true;
            }
        }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void FrmReplen_Load(object sender, EventArgs e)
        {
            _logger.LogDetailAsync($"Not Loading ShowAllOrders on INIT FrmReplen").SafeFireAndForget();
        }

        private void InitGrids()
        {

            int id = Thread.CurrentThread.ManagedThreadId;
            Trace.WriteLine("FrmPick thread: " + id);

            Console.WriteLine("Start Load Grids");


            if (!_availableOrdersGridReady)
            {
                var callback = new SendOrPostCallback(SetupAvailableOrdersGrid);
                Thread thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_pickViewGridReady)
            {
                var callback = new SendOrPostCallback(SetupPickViewGrid);
                Thread thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_orderGridReady)
            {
                var callback = new SendOrPostCallback(SetupOrderGrid);
                Thread thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_newOrderGridReady)
            {
                var callback = new SendOrPostCallback(SetupNewOrderGrid);
                Thread thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_orderDetailsGridReady)
            {
                var callback = new SendOrPostCallback(SetupOrderDetailsGrid);
                Thread thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_newItemsGridReady)
            {
                var callback = new SendOrPostCallback(SetupNewItemsGrid);
                Thread thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_availableOrdersRackGridReady)
            {
                var callback = new SendOrPostCallback(SetupAvailableOrdersRackGrid);
                Thread thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_adjustGridReady)
            {
                var callback = new SendOrPostCallback(SetupAdjustGrid);
                Thread thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }
        }

        private void RunGrid(object state, SendOrPostCallback setupGrid)
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            Trace.WriteLine($"{setupGrid.ToString()}  Thread: " + id);
            var uiContext = state as SynchronizationContext;
            if (uiContext != null) uiContext.Post(setupGrid, null);
        }

        private void InitDeviceIndicators()
        {
            _logger.LogDetailAsync($"Init Device Indicators START").SafeFireAndForget();
            if (PickScreen.Controls.ContainsKey("PanelDeviceIndicators")) return;

            _logger.LogDetailAsync("Initialize Device Indicators - InitDeviceIndicators").SafeFireAndForget();
            _deviceIndicatorManager = new DeviceIndicatorManager(_workstationView, new Point(189, 0),
               new Size(769, 127), _neutronVariables);



            //_deviceIndicators = new Dictionary<int, DeviceIndicator>();

            //var hardwareDevices = _workstationView.HardwareDevices.Where(x => _moveableDeviceTypes.Contains(x.DeviceTypeId))
            //    .ToList();
            //var numDevices = hardwareDevices.Count;
            //var panel = new Panel();
            //panel.Location = new Point(189, 0);
            //panel.Size = new Size(769, 127);
            //panel.BackColor = Color.Transparent;
            //panel.Name = "PanelDeviceIndicators";
            //var flashRate = _neutronVariables.DeviceFlashRate;
            //foreach (var hardwareDevice in hardwareDevices)
            //{
            //    var device = new DeviceIndicator(hardwareDevice.DeviceNumber, flashRate, Color.Yellow
            //        , Color.Transparent);
            //    device.Name = $"DeviceIndicator{hardwareDevice.DeviceNumber}";
            //    device.DeviceNumber = hardwareDevice.DeviceNumber;
            //    device.Location = GetLocation(panel.Size.Width, numDevices, hardwareDevice.DeviceNumber);
            //    _deviceIndicators.Add(hardwareDevice.DeviceNumber, device);
            //    panel.Controls.Add(device);
            //}

            if (_deviceIndicatorManager != null)
            {
                if (_deviceIndicatorManager.DeviceIndicatorPanel != null)
                {
                    PickScreen.Controls.Add(_deviceIndicatorManager.DeviceIndicatorPanel);
                }
            }
            _logger.LogDetailAsync($"Init Device Indicators END").SafeFireAndForget();
        }


        //public void ProcessDataReceived(object sender, MyDataReceivedEventArgs args)
        //{
        //     _logger.LogDetailAsync($"Process Data Received:  {args.FormText} START"));
        //    var t = args.FormText;
        //    var response = new IptiButtonResponse();
        //    if (t.Length > 14)
        //    {
        //        var cmd = t.Substring(1, 14);
        //        response = new IptiButtonResponse()
        //        {
        //            BayId = cmd.Substring(0, 2),
        //            Command = cmd.Substring(2, 2),
        //            DisplayId = cmd.Substring(4, 2),
        //            Quantity = cmd.Substring(6, 4),
        //            Text = cmd.Substring(10, 4)
        //        };
        //         _logger.LogDetailAsync($"Store Accept in Data Received: {t}"));
        //         _logger.LogDetailAsync("Hitting the Store Accept button from ProcessDataReceived."));
        //        StoreAccept();
        //    }
        //     _logger.LogDetailAsync($"Process Data Received:  {args.FormText} END"));
        //}

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

        #region All Grid Setups

        private void SetupOrderGrid(object state)
        {
            DataGridView1.AutoGenerateColumns = false;
            DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            DataGridView1.DefaultCellStyle.BackColor = Color.White;
            DataGridView1.ScrollBars = ScrollBars.Both;

            var colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            };
            DataGridView1.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString($"Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _resourceManager.GetString($"OrderStatusName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "OrderStatusName",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_1_HasPicks",
                HeaderText = @"1",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_1_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_2_HasPicks",
                HeaderText = @"2",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_2_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_3_HasPicks",
                HeaderText = @"3",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_3_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_4_HasPicks",
                HeaderText = @"4",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_4_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_5_HasPicks",
                HeaderText = @"5",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_5_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_6_HasPicks",
                HeaderText = @"6",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_6_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_7_HasPicks",
                HeaderText = @"7",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_7_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_8_HasPicks",
                HeaderText = _resourceManager.GetString($"Off"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_8_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _resourceManager.GetString($"Lines"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _resourceManager.GetString($"Pieces"),
                Name = "Pieces",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString($"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ShipMethodName",
                HeaderText = _resourceManager.GetString($"ShipMethodName"),
                Visible = false,
                Name = "ShipMethodName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _resourceManager.GetString($"Id"),
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

            _orderGridReady = true;
        }

        private void SetupPickViewGrid(object state)
        {

            DataGridPickView.AutoGenerateColumns = false;
            DataGridPickView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridPickView.DefaultCellStyle.ForeColor = Color.Black;
            DataGridPickView.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Sequence",
                HeaderText = _resourceManager.GetString($"Sequence"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Visible = false,
                Name = "Sequence"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickPosition",
                HeaderText = _resourceManager.GetString($"PickPosition"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PickPosition",
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _resourceManager.GetString($"Slot"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Slot"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalQuantityInInventory",
                HeaderText = _resourceManager.GetString($"TotalQuantityInInventory"),
                Name = "TotalQuantityInInventory",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _resourceManager.GetString($"ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            //col.DefaultCellStyle.Format = "{0:dd.MM.yyyy}";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _resourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridPickView.Columns.Add(col);

            DataGridPickView.EnableHeadersVisualStyles = false;
            DataGridPickView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridPickView.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridPickView.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _pickViewGridReady = true;
        }

        private void SetupAvailableOrdersGrid(object state)
        {

            //*****************************************************************************
            //DataGridViewAvailableOrders

            DataGridViewAvailableOrders.AutoGenerateColumns = false;
            DataGridViewAvailableOrders.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewAvailableOrders.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrders.DefaultCellStyle.BackColor = Color.White;

            var colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                Visible = false
            };
            DataGridViewAvailableOrders.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewAvailableOrders.Columns.Add(col);


            if (_neutronVariables.SerialPicking) //Show the Starter column
            {
                colx = new DataGridViewCheckBoxColumn
                {
                    DataPropertyName = "Starter",
                    HeaderText = _resourceManager.GetString($"Starter"),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                    Name = "Starter",
                    Visible = true,
                    TrueValue = 1,
                    FalseValue = 0,
                };
                DataGridViewAvailableOrders.Columns.Add(colx);

            }

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString($"Priority"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Priority"
            };

            DataGridViewAvailableOrders.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "OrderStatusName",
            //    HeaderText = _resourceManager.GetString("OrderStatusName"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
            //    Name = "OrderStatusName",
            //    Visible = false
            //};
            //DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _resourceManager.GetString($"Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Available",
            //    HeaderText = _resourceManager.GetString($"Available"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Available"
            //};
            //DataGridViewAvailableOrders.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Picked",
            //    HeaderText = _resourceManager.GetString($"Picked"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Picked"
            //};
            //DataGridViewAvailableOrders.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Skipped",
            //    HeaderText = _resourceManager.GetString($"Skipped"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Skipped"
            //};
            //DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _resourceManager.GetString($"Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString($"LoadDate"),
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "ShipMethodName",
            //    HeaderText = _resourceManager.GetString("ShipMethod"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
            //    Visible = false,
            //    Name = "ShipMethodName"
            //};

            // DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _resourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TransId",
                HeaderText = $"TransId",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Visible = true,
                Name = "TransId"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            DataGridViewAvailableOrders.EnableHeadersVisualStyles = false;
            DataGridViewAvailableOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewAvailableOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewAvailableOrders.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _availableOrdersGridReady = true;

        }

        private void SetupAvailableOrdersRackGrid(object state)
        {
            //*****************************************************************************
            //DataGridViewAvailableOrdersRack

            DataGridViewAvailableOrdersRack.AutoGenerateColumns = false;
            DataGridViewAvailableOrdersRack.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.BackColor = Color.White;

            var colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                Visible = false
            };
            DataGridViewAvailableOrdersRack.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StatusName",
                HeaderText = _resourceManager.GetString($"StatusName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StatusName",
                Visible = true
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString($"Priority"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Priority"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _resourceManager.GetString($"Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _resourceManager.GetString($"Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString($"LoadDate"),
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            DataGridViewAvailableOrdersRack.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "ShipMethodName",
            //    HeaderText = _resourceManager.GetString(@"ShipMethod"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
            //    Visible = false,
            //    Name = "ShipMethodName"
            //};

            // DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _resourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            DataGridViewAvailableOrdersRack.EnableHeadersVisualStyles = false;
            DataGridViewAvailableOrdersRack.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewAvailableOrdersRack.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewAvailableOrdersRack.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _availableOrdersRackGridReady = true;
        }

        private void SetupNewOrderGrid(object state)
        {
            //********************************************************************
            // DataGridViewNewOrder

            DataGridViewNewOrder.AutoGenerateColumns = false;
            DataGridViewNewOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewOrder.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewOrder.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _resourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                Name = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemDefinitionId",
                HeaderText = _resourceManager.GetString($"ItemDefinitionId"),
                Visible = false,
                Name = "ItemDefinitionId"
            };
            DataGridViewNewOrder.Columns.Add(col);

            DataGridViewNewOrder.EnableHeadersVisualStyles = false;
            DataGridViewNewOrder.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewNewOrder.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewNewOrder.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _newOrderGridReady = true;
        }

        private void SetupOrderDetailsGrid(object state)
        {
            //********************************************************************
            //DataGridViewOrderDetails

            DataGridViewOrderDetails.AutoGenerateColumns = false;
            DataGridViewOrderDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewOrderDetails.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewOrderDetails.DefaultCellStyle.BackColor = Color.White;
            DataGridViewOrderDetails.ScrollBars = ScrollBars.Both;

            var colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                Visible = false
            };
            DataGridViewOrderDetails.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _resourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _resourceManager.GetString($"PickedQuantity"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _resourceManager.GetString($"LineStatusName"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LineStatusName"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _resourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _resourceManager.GetString($"OrderDetailId"),
                Visible = false,
                Name = "OrderDetailId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            DataGridViewOrderDetails.EnableHeadersVisualStyles = false;
            DataGridViewOrderDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewOrderDetails.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewOrderDetails.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _orderDetailsGridReady = true;
        }

        private void SetupAdjustGrid(object state)
        {
            //********************************************************************
            //        //DataGridViewAdjust
            //
            DataGridViewAdjust.AutoGenerateColumns = false;
            // DataGridViewAdjust.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewAdjust.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAdjust.DefaultCellStyle.BackColor = Color.White;
            DataGridViewAdjust.ScrollBars = ScrollBars.Both;

            var adjust = _gridResourceManager.GetString($"Adjust");
            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = _gridResourceManager.GetString($""),
                Visible = true,
                Name = "Adjust",
                Text = adjust,
                // AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                UseColumnTextForButtonValue = true
            };
            DataGridViewAdjust.Columns.Add(bCol);

            var colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                Visible = false
            };
            DataGridViewAdjust.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Workstation",
                HeaderText = _resourceManager.GetString($"Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Workstation",
                ReadOnly = true

            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _resourceManager.GetString($"Slot"),
                Name = "Slot",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ordered",
                HeaderText = _resourceManager.GetString($"Quantity"),
                Name = "Ordered",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _resourceManager.GetString($"OrderDetailId"),
                Visible = false,
                Name = "OrderDetailId"
            };
            DataGridViewAdjust.Columns.Add(col);

            DataGridViewAdjust.Columns[5].DefaultCellStyle.Padding = new Padding(0, 0, 20, 0);

            DataGridViewAdjust.EnableHeadersVisualStyles = false;
            DataGridViewAdjust.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewAdjust.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewAdjust.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _adjustGridReady = true;
        }

        private void SetupNewItemsGrid(object state)
        {
            //********************************************************************
            //DataGridViewNewItems

            DataGridViewNewItems.AutoGenerateColumns = false;
            DataGridViewNewItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewItems.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewItems.DefaultCellStyle.BackColor = Color.White;
            DataGridViewNewItems.ScrollBars = ScrollBars.Both;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _resourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _resourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewNewItems.Columns.Add(col);

            DataGridViewNewItems.EnableHeadersVisualStyles = false;
            DataGridViewNewItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewNewItems.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewNewItems.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}

            _newItemsGridReady = true;
        }

        #endregion

        private int ShowAvailableStagingOrders(int recId = 0)
        {
            // ---         _logger.LogDetailAsync($"ShowAllOrders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            _currentDataSet = CurrentDataSet.Available;
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,7,8,9";

            var views = _replenOrdersRepository.GetAvailableReplenOrderViews(orderStatus, findWhat);
            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceOrderView.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceOrderView;

            if (GetRecordCount(_bindingSourceOrderView) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceOrderView, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }
                DataGridView1.Refresh();
                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceOrderView[recId]).Object;
            }
            // ---         _logger.LogDetailAsync($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        // Set the focus to the passed in recId if it's passed in
        private int ShowAllOrders(int recId = 0)
        {
            _logger.LogDetailAsync($"ShowAllOrders Replen Start: [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]").SafeFireAndForget();
            var idx = 0;
            var searchField = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,6,7,8,9";
            var views = _replenOrdersRepository.GetReplenOrderViews(orderStatus, searchField);
            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceOrderView.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceOrderView;

            if (GetRecordCount(_bindingSourceOrderView) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceOrderView, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }

                DataGridView1.Refresh();

                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceOrderView[recId]).Object;

            }

            _logger.LogDetailAsync($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return idx;
        }

        private void FilterAvailableOrders(string find)
        {
            if (string.IsNullOrWhiteSpace(find))
            {
                _bindingListViewAvailableOrdersViews.RemoveFilter();
            }
            else
            {
                _bindingListViewAvailableOrdersViews.ApplyFilter(r => r.Ord1.Contains(find));
            }
        }

        private int ShowAvailableOrders(int recId = 0, string searchField = "")
        {
            _logger.LogDetailAsync("ShowAvailableOrders START").SafeFireAndForget();
            var idx = 0;

            if (string.IsNullOrEmpty(searchField))
            {
                searchField = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = _replenOrdersRepository.GetAvailableReplenOrdersForInductionScreen(_workstationView.AreaId, searchField);
                _bindingListViewAvailableOrdersViews = new BindingListView<AvailableReplenOrdersView>(views.ToList());
                _bindingSourceAvailableOrders.DataSource = _bindingListViewAvailableOrdersViews;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                        $"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }

            DataGridViewAvailableOrders.DataSource = _bindingSourceAvailableOrders;

            if (GetRecordCount(_bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrders, recId);
                    DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex =
                        DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrders.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                    DataGridViewAvailableOrders.Update();
                }

                //SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();
                if (_bindingSourceAvailableOrders.Current != null)
                {
                    _currentAvailableOrdersView =
                        ((ObjectView<AvailableReplenOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                }
            }

            _logger.LogDetailAsync($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return idx;
        }

        //private int ShowAvailableOrders(int recId = 0)
        //{
        //    _logger.LogDetailAsync($"ShowAvailableOrders Replen: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    var idx = 0;

        //    var findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();

        //    try
        //    {
        //        var views = replenOrdersRepository.GetAvailableOrders(_workstationView, findWhat, _neutronVariables.SerialPicking);

        //        var bindingListView = new BindingListView<AvailableReplenOrdersView>(views.ToList());

        //        bindingSourceAvailableOrders.DataSource = bindingListView;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"ShowAvailableOrders Replen Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]"));
        //    }

        //    DataGridViewAvailableOrders.DataSource = bindingSourceAvailableOrders;
        //    DataGridViewAvailableOrdersRack.DataSource = bindingSourceAvailableOrders;
        //    if (GetRecordCount(bindingSourceAvailableOrders) > 0)
        //    {
        //        if (recId != 0)
        //        {
        //            idx = IndexOf(bindingSourceAvailableOrders, recId);
        //            DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
        //            DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
        //            DataGridViewAvailableOrders.Rows[idx].Selected = true;
        //            DataGridViewAvailableOrdersRack.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
        //            DataGridViewAvailableOrdersRack.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
        //            DataGridViewAvailableOrdersRack.Rows[idx].Selected = true;
        //        }
        //        else
        //        {
        //            DataGridViewAvailableOrders.ClearSelection();
        //            DataGridViewAvailableOrdersRack.ClearSelection();
        //        }
        //        CheckMarkSelectedAvailableOrders();
        //        SetBatchPositionToFirstEmpty();
        //        DataGridViewAvailableOrders.Refresh();
        //        DataGridViewAvailableOrdersRack.Refresh();

        //        _currentAvailableOrdersView = ((ObjectView<AvailableReplenOrdersView>)bindingSourceAvailableOrders.Current).Object;
        //    }

        //    _logger.LogDetailAsync($"ShowAvailableOrders Replen End: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    return idx;
        //}

        //private void CheckMarkSelectedAvailableOrders()
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.OrderId == 0) continue;
        //        foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
        //        {
        //            var id = Convert.ToInt32(row.Cells["Id"].Value);
        //            if (bp.OrderId != id) continue;
        //            var chk = (DataGridViewCheckBoxCell)row.Cells[0];
        //            chk.Value = chk.TrueValue;
        //            break;
        //        }
        //    }
        //}

        private int SetBatchPositionToFirstEmpty()
        {
            var result = -1;

            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var bp = _ordersToPick[i];
                if (bp.OrderId != 0) continue;
                SetCurrentTextBoxPos(bp.PositionNumber);
                result = i;
                break;
            }

            return result;
        }

        public int IndexOf(BindingSource bs, int value)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((ReplenOrderView)bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }
        /// <summary>
        /// Gets the count of records in the provided BindingSource.
        /// </summary>
        /// <param name="bs">The BindingSource whose records count is to be retrieved.</param>
        /// <returns>The count of records in the provided BindingSource.</returns>
        private int GetRecordCount(BindingSource bs)
        {
            int count = bs.Count;
            var records = _resourceManager.GetString($"Records");
            LabelRecordCount.Text = $"{records}: {count}";
            return count;
        }

        public int IndexOf(BindingListView<ReplenOrderView> bs, int value)
        {
            int count = bs.Count;
            int itemIndex = -1;
            for (int i = 0; i < count; i++)
            {
                int rec = ((ReplenOrderView)bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }

            return itemIndex;
        }

        private int GetRecordCount(BindingListView<ReplenOrderView> bs)
        {
            int count = bs.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
            return count;
        }

        #region Button Clicks


        private void MButtonClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MButtonViewEdit_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = AvailableOrders;
        }

        //private void MButtonNew_Click(object sender, EventArgs e)
        //{
        //    this.tabControl1.SelectedTab = Replen     Screen;
        //}

        //private void MbNewListing_Click(object sender, EventArgs e)
        //{
        //    ShowAllOrders();
        //    tabControl1.SelectedTab = OrderListing;
        //}

        //private void MbNewViewEdit_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = AvailableOrders;
        //}

        #endregion

        private void FrmReplen_FormClosing(object sender, FormClosingEventArgs e)
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

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (e.RowIndex < 0) return;
            var chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[0];
            dgv.Rows[e.RowIndex].Cells[0].Value = chk.Value == chk.TrueValue ? chk.FalseValue : chk.TrueValue;

        }

        private void MBHold_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.OrderStatusId == (int)OrderStatus.Available)
                    {
                        order.OrderStatusId = (int)OrderStatus.Hold;
                        _repoReplenOrder.Update(order);
                        _historyManager.SaveHistory(ActionCode.HoldOrder, order);
                        foreach (var orderDetail in order.ReplenOrderDetails)
                        {
                            if (orderDetail.LineStatusId == (int)LineStatus.Available)
                            {
                                orderDetail.LineStatusId = (int)LineStatus.Hold;
                                _repoReplenOrderDetails.Update(orderDetail);
                                _historyManager.SaveHistory(ActionCode.HoldLine, orderDetail);
                            }
                        }
                    }
                }
            }

            if (_currentDataSet == CurrentDataSet.Available)
            {
                ShowAvailableStagingOrders();
            }
            if (_currentDataSet == CurrentDataSet.Rack)
            {
                ShowRackOrders();
            }
            if (_currentDataSet == CurrentDataSet.Replen)
            {
                ShowReplenOrders();
            }
            if (_currentDataSet == CurrentDataSet.Putaway)
            {
                ShowPutawayOrders();
            }
            //ShowAllOrders();
        }

        private void MBRelease_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.OrderStatusId == (int)OrderStatus.Hold)
                    {
                        order.OrderStatusId = (int)OrderStatus.Available;
                        _repoReplenOrder.Update(order);
                        _historyManager.SaveHistory(ActionCode.ReleaseOrder, order);
                        foreach (var orderDetail in order.ReplenOrderDetails)
                        {
                            if (orderDetail.LineStatusId == (int)LineStatus.Hold)
                            {
                                orderDetail.LineStatusId = (int)LineStatus.Available;
                                _repoReplenOrderDetails.Update(orderDetail);
                                _historyManager.SaveHistory(ActionCode.ReleaseLine, orderDetail);
                            }
                        }
                    }
                }
            }


            if (_currentDataSet == CurrentDataSet.Available)
            {
                ShowAvailableStagingOrders();
            }
            if (_currentDataSet == CurrentDataSet.Rack)
            {
                ShowRackOrders();
            }
            if (_currentDataSet == CurrentDataSet.Replen)
            {
                ShowReplenOrders();
            }
            if (_currentDataSet == CurrentDataSet.Putaway)
            {
                ShowPutawayOrders();
            }
            //ShowAllOrders();
        }

        //private List<int> GetCheckedOrderDetailIds(DataGridView grid)
        //{
        //    var orderIds = new List<int>();

        //    var selectedRows = grid.SelectedRows
        //        .OfType<DataGridViewRow>()
        //        .Where(row => !row.IsNewRow)
        //        .ToArray();
        //    foreach (var row in selectedRows)
        //    {
        //        orderIds.Add((int)row.Cells["OrderDetailId"].Value);
        //    }

        //    if (!orderIds.Any())
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderIds;
        //}

        //private List<int> GetCheckedOrderIds(DataGridView grid)
        //{
        //    var orderIds = new List<int>();

        //    var selectedRows = grid.SelectedRows
        //        .OfType<DataGridViewRow>()
        //        .Where(row => !row.IsNewRow)
        //        .ToArray();
        //    foreach (var row in selectedRows)
        //    {
        //        orderIds.Add((int)row.Cells["Id"].Value);
        //    }

        //    if (!orderIds.Any())
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderIds;
        //}

        private List<ReplenOrder> GetSelectedOrders(DataGridView dataGridView)
        {
            var orders = new List<ReplenOrder>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var orderId = (int)row.Cells["Id"].Value;
                var order = _replenOrdersRepository.GetOrder(orderId);
                if (order != null)
                {
                    orders.Add(order);
                }
            }
            if (!orders.Any())
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
            }
            return orders;
        }

        /// <summary>
        /// Retrieves the details of the selected orders from a given DataGridView.
        /// </summary>
        /// <param name="dataGridView">The DataGridView from which to retrieve the selected order details.</param>
        /// <returns>A list of ReplenOrderDetail objects representing the details of the selected orders.
        /// If no orders are selected, a message box is displayed and an empty list is returned.</returns>
        private List<ReplenOrderDetail> GetSelectedOrderDetails(DataGridView dataGridView)
        {
            var orderDetails = new List<ReplenOrderDetail>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var orderDetailId = (int)row.Cells["OrderDetailId"].Value;
                var orderDetail = _repoReplenOrderDetails.FindByKey(orderDetailId);
                if (orderDetail != null)
                {
                    orderDetails.Add(orderDetail);
                }
            }
            if (!orderDetails.Any())
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
            }
            return orderDetails;
        }


        //private List<int> GetCheckedOrderIds()
        //{

        //    var orderIds = new List<int>();

        //    var selectedRows = DataGridView1.SelectedRows
        //        .OfType<DataGridViewRow>()
        //        .Where(row => !row.IsNewRow)
        //        .ToArray();
        //    foreach (var row in selectedRows)
        //    {
        //        orderIds.Add((int)row.Cells["Id"].Value);
        //    }


        //    //foreach (DataGridViewRow row in DataGridView1.Rows)
        //    //{
        //    //    if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
        //    //    {
        //    //        orderIds.Add((int)row.Cells["Id"].Value);
        //    //    }
        //    //}
        //    if (!orderIds.Any())
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderIds;
        //}

        //private List<ReplenOrderView> GetCheckedOrders()
        //{
        //    var ordViews = new List<ReplenOrderView>();
        //    var orderIds = new List<int>();
        //    foreach (DataGridViewRow row in DataGridView1.Rows)
        //    {
        //        if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
        //        {
        //            var ordId = (int)row.Cells["Id"].Value;
        //            ReplenOrderView view = _replenOrdersRepository.GetOrderView().Where(r => r.Id == ordId)
        //                .FirstOrDefault();
        //            if (view != null)
        //            {
        //                ordViews.Add(view);
        //            }
        //        }
        //    }

        //    if (ordViews.Count() == 0)
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return ordViews;
        //}

        //private List<int> GetCheckedAvailableOrderIds()
        //{
        //    var orderIds = new List<int>();

        //    foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
        //    {
        //        if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
        //        {
        //            orderIds.Add((int)row.Cells["Id"].Value);
        //        }
        //    }

        //    if (orderIds.Count() == 0)
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderIds;
        //}

        private void MBPickListBack_Click(object sender, EventArgs e)
        {
            PickListBack();
        }

        private void PickListBack()
        {
            ShowAvailableStagingOrders();
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.Green;
            Task.Run(NextButtonEnabled);
            tabControl1.SelectedTab = AvailableOrders;
        }

        /// <summary>
        /// Asynchronously updates the text of a <see cref="TextBox"/> control based on the specified <see cref="BatchPosition"/>.
        /// </summary>
        /// <param name="batchPosition">The <see cref="BatchPosition"/> containing the position number and order number to update the <see cref="TextBox"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method finds the <see cref="TextBox"/> control corresponding to the position number in the <paramref name="batchPosition"/> 
        /// and updates its text with the order number. If the <see cref="TextBox"/> is not found, no update is performed.
        /// </remarks>
        private async Task UpdateTextBoxPosition(BatchPosition batchPosition)
        {
            var orderNumber = batchPosition.Ord1;
            var position = batchPosition.PositionNumber;
            // Find the TextBox control based on the position number
            var textBox = await FindTextBoxByPositionAsync(position);
            // Update the TextBox text with the order number if the TextBox is found
            if (textBox != null)
            {
                textBox.Text = orderNumber;
            }
        }

        /// <summary>
        /// Asynchronously finds a <see cref="TextBox"/> control based on the specified position number.
        /// </summary>
        /// <param name="position">The position number used to identify the <see cref="TextBox"/> control.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the <see cref="TextBox"/> control
        /// if found; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method searches for a <see cref="TextBox"/> control within the current form's controls collection
        /// using the specified position number. The search is performed asynchronously to avoid blocking the UI thread.
        /// </remarks>
        private async Task<TextBox> FindTextBoxByPositionAsync(int position)
        {
            return await Task.Run(() => Controls
                .Find($"TextBoxPos{position}", true)
                .OfType<TextBox>()
                .SingleOrDefault());
        }


        //private void UpdateTextBoxPosition(BatchPosition bp)
        //{
        //    _logger.LogDetailAsync($"BP: {bp.PositionNumber} Start: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        //    var orderNumber = bp.Ord1;
        //    var pos = bp.PositionNumber;

        //    Control c = Controls.Find($"TextBoxPos{pos}", true).Single() as TextBox;
        //    if (c != null) c.Text = orderNumber;
        //    _logger.LogDetailAsync($"UpdateTextBoxPosition End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        //}

        private void MBBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = OrderListing;
        }

        private void MbNewClose_Click_1(object sender, EventArgs e)
        {

        }

        private async void MBAvailableOrdersBack_Click(object sender, EventArgs e)
        {
            await AvailableOrdersBack();
            ////ShowAllOrders();
            //if (_currentDataSet == CurrentDataSet.Available)
            //{
            //    ShowAvailableStagingOrders();
            //}
            //if (_currentDataSet == CurrentDataSet.Rack)
            //{
            //    ShowRackOrders();
            //}
            //if (_currentDataSet == CurrentDataSet.Replen)
            //{
            //    ShowReplenOrders();
            //}
            //if (_currentDataSet == CurrentDataSet.Putaway)
            //{
            //    ShowPutawayOrders();
            //}

            //ClearBatchPositions();
            //LabelFormTitle.Text = "Jobs";
            //LabelFormTitle.BackColor = Color.Green;
            //tabControl1.SelectedTab = Main;
        }

        private async Task AvailableOrdersBack()
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                await _iptiDisplayFunctions.ClearBlastzone();
            }

            GlobalVar.Hanel?.ResetHanelDeviceStatus();

            // Clear all the ProLites using the ProLiteManager

            _workstationView.ProLiteManager?.ClearAllProlites();

            _deviceIndicatorManager?.ClearActiveDeviceIndicators();

            LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }


        private ReplenOrder ScanTransId(string order)
        {
            ReplenOrder replenOrder = null;
            using (var form = new FrmScanTransId(order))
            {

                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    replenOrder = form.ReplenOrder;
                    var TransId = form.TransId;
                }
            }

            return replenOrder;
        }

        /// <summary>
        /// Check to see if TextBoxPosx.Text doesn't match OrdersToPick.Order
        /// Clear invalid OrdersToPick
        /// </summary>
        private async Task<bool> FinalCheckOfOrdersToPick()
        {
            // loop over all TextBoxPosx
            // and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            // Clear invalid OrdersToPick
            _logger.LogDetailAsync($"[{DateTime.Now}]  Start FinalCheckOfOrdersToPick").SafeFireAndForget();
            try
            {
                //ClearBatchPositions();

                foreach (var bp in _ordersToPick)
                {
                    var pos = bp.PositionNumber.ToString();
                    var c = Controls.Find($"TextBoxPos{pos}", true).First();
                    if (c != null)
                    {
                        var textBox = ((TextBox)c);
                        var order = textBox.Text.Trim();
                        //Is this a real order
                        if (!string.IsNullOrEmpty(order))
                        {
                            var isRealOrder = _repoReplenOrder.FindBy(r => r.Id == bp.OrderId && r.OrderStatusId == 1).FirstOrDefault();
                            if (isRealOrder == null)
                            {
                                MessageBox.Show($"The Delivery {order} in Position {pos} does not exist.");
                                await ClearItemFromBatchByPosition(bp.PositionNumber);
                                return false;
                            }
                            else
                            {
                                // create the Batch Position
                                bp.OrderId = isRealOrder.Id;
                                bp.Ord1 = isRealOrder.Ord1;
                                bp.Ord2 = isRealOrder.Ord2;
                                bp.OrderComplete = false;
                            }
                            //var realOrders = _repoReplenOrder.FindBy(r => r.Ord1 == order && r.OrderStatusId == 1).ToList();

                            //if (realOrders.Count > 1)
                            //{
                            //    // Prompt to scan a value into an inputbox
                            //    var replenOrder = ScanTransId(order);
                            //    if (replenOrder != null)
                            //    {
                            //        // create the Batch Position
                            //        bp.OrderId = replenOrder.Id;
                            //        bp.Ord1 = replenOrder.Ord1;
                            //        bp.Ord2 = replenOrder.Ord2;
                            //        bp.OrderComplete = false;
                            //    }
                            //    else
                            //    {
                            //        MessageBox.Show($"Cannot locate the Delivery {order} in Position {pos}.  Removing from batch.");
                            //        ClearItemFromBatchByPosition(bp.PositionNumber);
                            //        return false;
                            //    }

                            //}
                            //else if (realOrders.Count == 1)
                            //{
                            //    var ord = realOrders.First();
                            //    // create the Batch Position
                            //    bp.OrderId = ord.Id;
                            //    bp.Ord1 = ord.Ord1;
                            //    bp.Ord2 = ord.Ord2;
                            //    bp.OrderComplete = false;
                            //}
                            //else  // no order found
                            //{
                            //    MessageBox.Show($"Cannot locate the Delivery {order} in Position {pos}.  Removing from batch.");
                            //    ClearItemFromBatchByPosition(bp.PositionNumber);
                            //    return false;
                            //}
                        }
                        else
                        {
                            bp.OrderId = 0;
                            bp.Ord1 = string.Empty;
                            bp.Ord2 = string.Empty;
                            bp.OrderComplete = false;
                            ClearPickPosition(pos);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
                MessageBox.Show($"The Final Check of Orders to Pick has failed. {Environment.NewLine}" +
                                $"{ex.Message}");
                return false;
            }
            DumpOrdersToPick(2848);
            _logger.LogDetailAsync($"[{DateTime.Now}]  End FinalCheckOfOrdersToPick").SafeFireAndForget();
            return true;




            //// loop over all TextBoxPosx
            //// and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            //// Clear invalid OrdersToPick

            //_logger.LogDetailAsync($" Start FinalCheckOfOrdersToPick"));
            //try
            //{
            //    //var batchPositions = _ordersToPick.Where(r => r.Ord1 != "").ToList();
            //    foreach (var bp in _ordersToPick)
            //    {
            //        var pos = bp.PositionNumber.ToString();
            //        var c = Controls.Find($"TextBoxPos{pos}", true).FirstOrDefault();
            //        if (c != null)
            //        {

            //            var textBox = ((TextBox)c);
            //            var order = textBox.Text.Trim();
            //            if (!bp.Ord1.Equals(order, StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                // ---         _logger.LogDetailAsync($"TextBox is Null or Empty: {order}"));
            //                MessageBox.Show($"There's something wrong with the order in position {bp.PositionNumber}.");
            //                // _logger.LogDetailAsync($"TextBox must be Null or Empty: {order}"));
            //                // clear the Batch Position
            //                ClearItemFromBatchByPosition(bp.PositionNumber);
            //                return false;
            //            }
            //        }

            //        DumpOrdersToPick(2227);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogDetailAsync($"Error: {ex.Message}"));
            //    MessageBox.Show($"The Final Check of Orders to Pick has failed. {Environment.NewLine}" +
            //                    $"{ex.Message}");
            //    return false;
            //}
            //_logger.LogDetailAsync($"[{DateTime.Now}]  End FinalCheckOfOrdersToPick"));
            //return true;
        }

        /// <summary>
        /// Clears the pick position in the user interface.
        /// </summary>
        /// <param name="pos">The position to be cleared, represented as an integer.</param>
        private void ClearPickPosition(string pos)
        {
            var c = Controls.Find($"TextBoxPickPos{pos}", true).First();
            if (c != null)
            {
                var textBox = ((TextBox)c);
                // if the order is complete, show END in the position box
                // else show nothing
                textBox.Text = string.Empty;
            }
        }
        private void DumpOrdersToPick(int lineNumber)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DUMP REPLEN ORDERS TO PICK");
            foreach (var bp in _ordersToPick)
            {
                sb.AppendLine(
                    $"Pos:{bp.PositionNumber} ID:{bp.OrderId} Order:{bp.Ord1} Invoice:{bp.Ord2} Complete:{bp.OrderComplete}");
            }
            _logger.LogDetailAsync($"[{DateTime.Now}]  Line Number: {lineNumber} {Environment.NewLine}" +
                                                  $"{sb.ToString()}").SafeFireAndForget();
        }

        private async Task ClearItemFromBatchByPosition(int position)
        {
            _logger.LogDetailAsync($" Start ClearItemFromBatchByPosition: {position}").SafeFireAndForget();
            try
            {
                var bp = _ordersToPick.FirstOrDefault(o => o.PositionNumber == position);
                if (bp == null) return;
                bp.OrderId = 0;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                await UpdateTextBoxPosition(bp);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }
        }

        private async void MBGo_Click(object sender, EventArgs e)
        {
            _spaceBarDisabled = true;
            DisableNextButtons();
            MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
            Cursor.Current = Cursors.WaitCursor;
            await Go();
            Cursor.Current = Cursors.Default;
            _spaceBarDisabled = false;
            EnableNextButtons();
        }

        private void DisableNextButtons()
        {
            MBGo.Enabled = false;
            MBGo2.Enabled = false;
        }

        private void EnableNextButtons()
        {
            MBGo.Enabled = true;
            MBGo2.Enabled = true;
        }

        private async Task Go()
        {
            _logger.LogDetailAsync($"Go Replen Batch START").SafeFireAndForget();

            if (await FinalCheckOfOrdersToPick() == false)
            {
                return;
            }

            TextBoxFindAvailableOrders.Text = string.Empty;
            try
            {

                Cursor.Current = Cursors.WaitCursor;
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");

                var numOrders = _ordersToPick.Count(o => o.OrderId != 0);
                if (numOrders > 0)
                {
                    LabelFormTitle.Text = _resourceManager.GetString($"PickList");
                    LabelFormTitle.BackColor = Color.Green;

                    var pickableViews = await PickListLoad();

                    if (pickableViews.Count > 0)
                    {
                        _bindingSourcePickViews.DataSource = pickableViews;
                        DataGridPickView.DataSource = _bindingSourcePickViews;
                        _logger.LogDetailAsync(
                                $"Binding Source Pick Views Count:{_bindingSourcePickViews.Count.ToString()}")
                            .SafeFireAndForget();
                        GetRecordCount(_bindingSourcePickViews);

                        await Start();
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
                        ClearAllSelectOrdersToPick();
                        LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                        LabelFormTitle.BackColor = Color.Green;
                        await AvailableOrdersScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();

                Cursor.Current = Cursors.Default;
                _logger.LogDetailAsync($"Go Batch END").SafeFireAndForget();
            }
        }

        private async Task Start()
        {
            _logger.LogDetailAsync($"Start START").SafeFireAndForget();

            if (GlobalVar.Hanel != null)
            {
                GlobalVar.Hanel?.ResetHanelDeviceStatus();
                _logger.LogDetailAsync($"{DateTime.Now} Finished Resetting Hanel Device Status").SafeFireAndForget();
            }

            _spaceBarDisabled = true;

            // InitDeviceIndicators();
            //-------------------------------------
            // _logger.LogDetailAsync($"Call Printing Start: [{DateTime.Now.ToLongTimeString()}]"));

            ////  PrintAllToteLabels();
            //if (_neutronVariables.EnableDocumentPrinter)
            //{
            //    if (_neutronVariables.PrintPackingListStart)
            //    {
            //        PrintAllDocuments();
            //    }
            //}

            // _logger.LogDetailAsync($"Call Printing End: [{DateTime.Now.ToLongTimeString()}]"));
            try
            {
                // a final check to make sure we have PickViews to pick
                if (_bindingSourcePickViews.Count == 0) return;
                var pickViews = (IList<ReplenPickView>)_bindingSourcePickViews.DataSource;
                if (pickViews == null) return;
                // set the sort order based on Location Type
                // LocationType 3 is a Rack location and 
                // should be sorted using the PickSequence
                //var locationType = _workstationView.Area.LocationTypeId;
                if (_workstationView.AreaId == AreaEight)
                {
                    // Sort by PickSequence
                    pickViews = pickViews.OrderBy(p => p.CurrentInventoryLocation?.Location.PickSequence).ToList();
                }
                else
                {
                    // Sort by Location
                    pickViews = pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
                        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
                        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();
                }

                //TODO SetOrderStatusToPicking(pickViews);
                _logger.LogDetailAsync($"Start_Click 1: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                var pickStops = new List<ReplenPickStop>();
                IEnumerable<IGrouping<string, ReplenPickView>> pickViewGroups = pickViews.GroupBy(r => r.ItemKey).ToList();
                var sequence = 0;
                foreach (var pickViewGroup in pickViewGroups) //for each Item in the group of Items
                {
                    var total = 0;
                    // a ReplenPickStop is of One Item that may be on One to All Pick Positions
                    // a ReplenPickView is an individual pick at a single Pick Position
                    // so a ReplenPickStop is has One or Multiple PickViews that are concerned 
                    // with picking One Item.
                    // a ReplenPickStop is a summary of all the PickViews 
                    // and some of the information in a ReplenPickStop is the same as in a ReplenPickView
                    // that is why the First ReplenPickView is used to provide most of the data to the ReplenPickStop
                    var firstPickView = pickViewGroup.First();

                    var pickStop = new ReplenPickStop
                    {
                        Sequence = sequence += 1,
                        OrderId = firstPickView.OrderId,
                        Ord1 = firstPickView.Ord1,
                        Ord2 = firstPickView.Ord2,
                        ItemId = firstPickView.ItemId,
                        Item = firstPickView.Item,
                        Description = firstPickView.Description,
                        UnitOfIssue = firstPickView.UnitOfIssue,
                        PickedQty = firstPickView.PickedQty,
                        Images = firstPickView.Images,
                        Inventory = firstPickView.Inventory,
                        InventoryIndex = firstPickView.InventoryIndex,
                        Slot = firstPickView.Slot,
                        SlotQty = firstPickView.SlotQty,
                        CurrentInventoryLocation = firstPickView.CurrentInventoryLocation,
                        TotalQuantityInInventory = firstPickView.TotalQuantityInInventory,
                        ItemKey = firstPickView.ItemKey
                    };

                    foreach (var pickView in pickViewGroup)
                    {
                        pickStop.PickViews.Add(pickView);
                        total += pickView.Quantity;
                    }

                    pickStop.Quantity = total;
                    pickStop.QuantityToBePicked = total;

                    pickStops.Add(pickStop);
                }
                foreach (var stop in pickStops)
                {
                    _logger.LogDetailAsync($"STOP SEQUENCE: {stop.Sequence} " +
                                            $" SKU: {stop.CurrentInventoryLocation.ItemDefinition.Item}" +
                                            $" Location: {stop.CurrentInventoryLocation.Location.Loc1}" +
                                            $"-{stop.CurrentInventoryLocation.Location.Loc2}" +
                                            $"-{stop.CurrentInventoryLocation.Location.Loc3}" +
                                            $"-{stop.CurrentInventoryLocation.Location.Loc2}" +
                                            $"  Slot:  {stop.CurrentInventoryLocation.Location.Slot}" +
                                            $"  Sequence:  {stop.CurrentInventoryLocation.Location.PickSequence}").SafeFireAndForget();

                }




                _logger.LogDetailAsync($"Start_Click 2: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                //var finalPickSequence = FinalPickSequence(pickStops);
                var finalPickSequence =
            _workstationView.AreaId == AreaEight ? FinalPickSequenceAreaEight(pickStops) : FinalPickSequence(pickStops);
                _bindingSourcePickStops.DataSource = null;
                _bindingSourcePickStops.DataSource = finalPickSequence;


                _logger.LogDetailAsync($"Start_Click 3 Run GetFirstStop?: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                // GetFirstStop();
                _bindingSourcePickStops.MoveFirst();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;

                await UpdatePickScreen();

                if (_workstationView.AreaId == AreaEight)
                {
                    TextBoxSlot.Visible = true;
                    TextBoxSlot.Text = string.Empty;
                    TextBoxSlot.Focus();
                }
                else
                {
                    TextBoxSlot.Visible = false;
                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                }


                await UpdatePickPosition();

                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);

                // UpdateTowerDisplay();

                tabControl1.SelectedTab = PickScreen;
                MBStoreAccept.Focus();
                _spaceBarDisabled = false;

                //feels good to here
                _logger.LogDetailAsync($"Start_Click End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No Current Inventory Locations.  {ex.Message}");
            }
        }
        /// <summary>
        /// Sorts the PickStops based on the Location's Pick Sequence
        /// </summary>
        /// <param name="pickStops">The List of PickStops to be sorted</param>
        /// <returns></returns>
        private List<ReplenPickStop> FinalPickSequenceAreaEight(List<ReplenPickStop> pickStops)
        {
            // return pickStops.OrderBy(r => r.CurrentInventoryLocation.Location.PickSequence).ToList();

            return pickStops;
        }


        private async Task<List<ReplenPickView>> PickListLoad()
        {
            // _logger.LogDetailAsync($"Pick List Load START"));
            var pickViews = GetPickViews();
            // _logger.LogDetailAsync($"Pickviews Count: {pickViews.Count}"));

            var pickableViews = new List<ReplenPickView>();

            try
            {

                LoadInventoryForPickViews(pickViews);

                foreach (var pickView in pickViews)
                {
                    var orderDetail = pickView.OrderDetail;

                    await SetOrderDetailStatusToPicking(orderDetail);

                    //orderDetail.LineStatusId = (int)LineStatus.Picking;
                    //await _repoReplenOrderDetails.UpdateAsync(orderDetail);

                    List<Inventory> exactInventorySequence;

                    if (_workstationView.AreaId == AreaEight)
                    {
                        exactInventorySequence = await PrimeBinFirst(pickView);
                    }
                    else
                    {
                        exactInventorySequence = Lifo(pickView);
                    }

                    // if not Area 8, get 3 locations including existing locations
                    // to store into
                    if (_workstationView.AreaId != AreaEight)
                    {
                        var neededLocations = 3 - exactInventorySequence.Count;
                        if (neededLocations > 0)
                        {
                            var itemDefinition = _repoItemDefinition.FindByKey(pickView.ItemId);
                            if (itemDefinition == null) return pickableViews;

                            var additionInventoryLocations = await
                                GetNewInventoryLocations(itemDefinition, neededLocations);
                            exactInventorySequence.AddRange(additionInventoryLocations);
                        }
                    }


                    if (exactInventorySequence.Count > 0)
                    {
                        pickView.CurrentInventoryLocation = exactInventorySequence.First();
                        pickView.Inventory = exactInventorySequence;
                        pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                        pickView.Slot = exactInventorySequence.First().Location.Slot;
                        pickView.SlotQty = exactInventorySequence.First().Quantity;
                        pickView.InventoryIndex = 0;
                        pickView.ReceivedDate = exactInventorySequence.First().ReceivedDate == default ? DateTime.Now : exactInventorySequence.First().ReceivedDate;
                        pickableViews.Add(pickView);
                    }
                    else
                    {
                        MessageBox.Show($"There are no locations to store this item: {pickView.Item}.{Environment.NewLine}Description: {pickView.Description}{Environment.NewLine}This item will not be included in this batch.");
                        await RemoveItemFromBatch(pickView.OrderId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            //_logger.LogDetailAsync($"PickListLoad End: [{DateTime.Now.ToLongTimeString()}]"));
            return pickableViews;
        }

        /// <summary>
        /// Loads the Inventory for just the current PickViews into the CurrentInventory variable
        /// </summary>
        /// <param name="pickViews"></param>
        /// <param name="pickView"></param>
        //private void LoadInventoryForPickViews(List<ReplenPickView> pickViews)
        //{
        //    // get the areaId from the WorkstationView
        //    var areaId = _workstationView.AreaId;

        //    _logger.LogDetailAsync($"Load Inventory For PickViews START"));
        //    // get a distinct list of ItemIds from the pickviews
        //    var itemIds = pickViews.Select(r => r.ItemId).Distinct().ToList();

        //    // get the current inventory in this area for all the distinct items in the pickviews
        //    _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
        //        .Where(f => itemIds.Contains(f.ItemDefinitionId) && f.AreaId == areaId).ToList();

        //    _logger.LogDetailAsync($"Load Inventory For PickViews END"));
        //}

        private async Task<List<Inventory>> PrimeBinFirst(ReplenPickView pickView)
        {
            _logger.LogDetailAsync($"Prime Bin First Inventory START").SafeFireAndForget();
            var inventorySequence = new List<Inventory>();
            List<Inventory> sortedRecs;

            var recs = GetInventory(pickView.ItemId);

            if (recs.Count <= 0) return await GetDefaultInventory(pickView.ItemId, _workstationView.AreaId);
            if (recs.Count == 1) return recs;

            //if there is a prime bin make it first, remove it from the list of inventory locations
            // var prime = recs.FirstOrDefault(r => r.Location.Slot == item.OrderDetail.PrimeBin);
            var prime = recs.FirstOrDefault(r => r.PrimeBin);  // .Location.Slot == item.OrderDetail.PrimeBin);
            if (prime != null)
            {
                // ---         _logger.LogDetailAsync($"recs Add Prime [{DateTime.Now.ToLongTimeString()}] "));
                inventorySequence.Add(prime);
                recs.Remove(prime);
            }

            if (_workstationView.AreaId == AreaEight)
            {
                // sort the recs by the Location.PickSequence
                sortedRecs = recs.OrderBy(r => r.Location.PickSequence).ToList();
            }
            else
            {
                //sequence the inventory Recs by Received Date
                sortedRecs = recs.OrderBy(o => o.ReceivedDate).ToList();
            }
            inventorySequence.AddRange(sortedRecs);

            //if (!recs.Any()) return inventorySequence;
            //var rec = recs.First();
            //var itemDef = _repoItemDefinition.FindByKey(rec.Id);
            //var locationMax = itemDef.LocationMax;


            ////if there is a prime bin make it first, remove it from the list of inventory locations
            //if (recs.Count == 1)
            //{
            //    inventorySequence = new List<Inventory> { recs.First() };
            //    //if (rec.Quantity < locationMax)
            //    //{
            //    //    inventorySequence = recs;
            //    // }

            //}
            //else if (recs.Count > 1)
            //{
            //    var prime = recs.FirstOrDefault(r => r.Location.Slot == pickView.OrderDetail.PrimeBin);
            //    if (prime != null)
            //    {
            //        //if (prime.Quantity < locationMax)
            //        //{
            //        inventorySequence.Add(prime);
            //        recs.Remove(prime);
            //        //}
            //    }

            //    //sequence the inventory Recs by Received Date
            //    var sortedRecs = recs.OrderBy(o => o.ReceivedDate);
            //    foreach (var inventory in sortedRecs)
            //    {
            //        if (inventory.Quantity < locationMax)
            //        {
            //            inventorySequence.Add(inventory);
            //        }
            //    }
            //}
            _logger.LogDetailAsync($"Prime Bin First Inventory END").SafeFireAndForget();
            return inventorySequence;
        }

        private async Task<List<Inventory>> GetDefaultInventory(int itemDefinitionId, int areaId)
        {
            var inventoryList = new List<Inventory>();

            if (areaId == AreaEight)
            {
                var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
                if (itemDefinition == null) return inventoryList;
                var location = _repoLocationRepository.FindByKey(10954);
                if (location == null) return inventoryList;


                // no inventory locations for this item
                // create a new empty Inventory object
                var inventory = new Inventory
                {
                    AreaId = areaId,
                    ItemDefinition = itemDefinition,
                    ItemDefinitionId = itemDefinitionId,
                    RFID = string.Empty,
                    ReceivedDate = DateTime.Now,
                    Quantity = 0,
                    Location = location,
                    LocationId = 10954
                };
                inventoryList.Add(inventory);
                return inventoryList;
            }
            else
            {
                var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
                if (itemDefinition == null) return inventoryList;
                var location = await _repoLocationRepository.FindByFirstOrDefaultAsync(r => r.SizeCodeId == itemDefinition.SizeCodeId && r.VelocityCodeId == itemDefinition.VelocityCodeId && r.HeightCodeId == itemDefinition.HeightCodeId && r.InUse == false);
                if (location != null)
                {
                    // no inventory locations for this item
                    // create a new empty Inventory object
                    var inventory = new Inventory
                    {
                        AreaId = areaId,
                        ItemDefinition = itemDefinition,
                        ItemDefinitionId = itemDefinitionId,
                        RFID = string.Empty,
                        ReceivedDate = DateTime.Now,
                        Quantity = 0,
                        Location = location,
                        LocationId = location.Id
                    };
                    inventoryList.Add(inventory);
                    return inventoryList;
                }
            }
            return inventoryList;
        }

        private List<Inventory> Fifo(ReplenPickView pickView)
        {
            _logger.LogDetailAsync($"1536 START FIFO Inventory ").SafeFireAndForget();
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(pickView.ItemId);
            if (!recs.Any()) return inventorySequence;
            if (recs.Count == 1)
                inventorySequence = recs;
            else if (recs.Count > 1)
                inventorySequence = recs.OrderByDescending(o => o.ReceivedDate).ToList();
            //else
            //    inventorySequence = inventorySequence;

            return inventorySequence;
        }
        private List<Inventory> Lifo(ReplenPickView item)
        {

            List<Inventory> sortedRecs;

            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            // ---         _logger.LogDetailAsync($"1553 LIFO Inventory Rec Count:  {recs.Count}"));
            if (recs.Count > 0)
            {
                //sequence the inventory Recs by Received Date Descending
                sortedRecs = recs.OrderBy(o => o.ReceivedDate).ToList();

                inventorySequence.AddRange(sortedRecs);
            }
            return inventorySequence;
        }

        private List<ReplenPickView> GetPickViews()
        {
            // _logger.LogDetailAsync($"Get Pick Views START"));
            var prevPartNum = "";

            var pickViews = new List<ReplenPickView>();

            try
            {
                var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();
                foreach (var bp in batchPositions)
                {
                    if (bp.OrderId == 0) continue;
                    var itemFound = _bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                    if (itemFound > -1) _bindingSourceAvailableOrders.Position = itemFound;
                    var currentOrder = ((ObjectView<AvailableReplenOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                    var firstTime = true;
                    var counter = 0;
                    var orderAndDetails = _replenOrdersRepository.GetOrderAndOrderDetails(bp.OrderId, _workstationView.AreaId);
                    currentOrder.Order = orderAndDetails;

                    //var details = currentOrder.Order.ReplenOrderDetails.OrderBy(o => o.PartNum);

                    foreach (var detail in orderAndDetails.ReplenOrderDetails)
                    {
                        //already checked the line status in the Repository
                        //if (detail.LineStatusId != (int)LineStatus.Available &&
                        //    detail.LineStatusId != (int)LineStatus.Skipped) continue;
                        //key builder makes each line of orderdetails unique so that an order with the same item
                        // will be picked separately
                        // PickStops will be grouped by key, not item number

                        detail.ReplenOrder = (ReplenOrder)orderAndDetails;

                        var key = "";
                        if (firstTime)
                        {
                            prevPartNum = detail.PartNum;
                            key = detail.PartNum;
                            firstTime = false;
                        }
                        else if (prevPartNum == detail.PartNum)
                        {
                            counter++;
                            key = $"{detail.PartNum}{counter}";
                        }
                        else //prevPartNum != detail.PartNum
                        {
                            prevPartNum = detail.PartNum;
                            key = detail.PartNum;
                            counter = 0;
                        }

                        var unitOfIssue = _repoItemDefinition.FindBy(f => f.Id == detail.ItemDefinitionId).FirstOrDefault()?.UnitOfIssue.Name;

                        var pickView = new ReplenPickView()
                        {
                            PickPosition = bp.PositionNumber,
                            OrderId = detail.ReplenOrder.Id,  // currentOrder.Id,
                            Ord1 = detail.ReplenOrder.Ord1,
                            Ord2 = detail.ReplenOrder.Ord2,
                            ItemId = detail.ItemDefinitionId,
                            Item = detail.PartNum,
                            Description = detail.PartDesc,
                            UnitOfIssue = unitOfIssue,
                            Quantity = detail.Quantity,
                            QuantityToBePicked = detail.Quantity,
                            PickedQty = detail.PickedQuantity,
                            Slot = string.Empty,
                            SlotQty = 0,
                            OrderDetail = detail,
                            AreaId = detail.AreaId,
                            ItemKey = key
                        };
                        pickViews.Add(pickView);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            _logger.LogDetailAsync($"Get Pick Views END").SafeFireAndForget();
            return pickViews;
        }

        private int GetBatchPosition(int orderId)
        {
            var result = -1;

            var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == orderId);
            if (bp != null)
            {
                result = bp.PositionNumber;

            }
            return result;
        }

        private int[] GetOrderIdArray(List<BatchPosition> ordersToPick)
        {
            var orderIds = new List<int>();
            foreach (var bp in ordersToPick)
            {
                if (bp.OrderId != 0)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }

            return orderIds.ToArray();
        }

        private ReplenPickView CreatePickView(int pos, ReplenOrderDetail detail)
        {
            _logger.LogDetailAsync($"CreatePickView Start: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var pickView = new ReplenPickView()
            {
                PickPosition = pos,
                OrderId = detail.ReplenOrder.Id,
                Ord1 = detail.ReplenOrder.Ord1,
                Ord2 = detail.ReplenOrder.Ord2,
                ItemId = detail.ItemDefinitionId,
                Item = string.Empty,
                Description = string.Empty,
                Quantity = detail.Quantity,
                // QuantityToBePicked = detail.Quantity,
                PickedQty = 0,
                Slot = string.Empty,
                SlotQty = 0,
                OrderDetail = detail
            };

            var def = _repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
            if (def != null)
            {
                pickView.Item = def.Item;
                pickView.Description = def.Description;
            }

            //List<Inventory> recs = GetInventory(pickView.ItemId);
            var recs = _currentInventory.FindAll(r => r.ItemDefinitionId == pickView.ItemId);
            var exactInventorySequence = new List<Inventory>();
            if (recs.Count != 0)
            {
                if (recs.Count == 1)
                {
                    exactInventorySequence.Add(recs.First());
                }

                if (recs.Count > 1)
                {
                    if (_neutronVariables.UsePrimeBin)
                    {
                        //if there is a prime bin make it first, remove it from the list of inventory locations
                        var prime = recs.FirstOrDefault(r => r.Location.Slot == pickView.OrderDetail.PrimeBin);
                        if (prime != null)
                        {
                            exactInventorySequence.Add(prime);
                            recs.Remove(prime);
                        }
                    }

                    //if you removed the prime bin and recs are still > 1, you must sort
                    if (recs.Count > 1)
                    {
                        //sequence the inventory Recs by Received Date Descending
                        var orderByDescending = recs.OrderByDescending(o => o.ReceivedDate);
                        foreach (var inv in orderByDescending)
                        {
                            exactInventorySequence.Add(inv);
                        }
                    }
                }

                pickView.CurrentInventoryLocation = exactInventorySequence.First();
                pickView.Inventory = exactInventorySequence;
                pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                pickView.Slot = pickView.CurrentInventoryLocation.Location.Slot;
                pickView.SlotQty = pickView.CurrentInventoryLocation.Quantity;
                pickView.InventoryIndex = 0;
                pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
            }
            else
            {
                MessageBox.Show("No Inventory for Item: " + pickView.Item);
            }

            _logger.LogDetailAsync($"CreatePickView End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return pickView;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            _logger.LogDetailAsync($"Get Inventory Item: {itemId} START").SafeFireAndForget();

            //var pickableLocations = new int[] { 1, 2 };
            //recs = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
            //    .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            var recs = _currentInventory.FindAll(r => r.ItemDefinitionId == itemId);

            _logger.LogDetailAsync($"Get Inventory END").SafeFireAndForget();
            return recs;
        }

        /// <summary>
        /// Loads the Inventory for just the current PickViews into the CurrentInventory variable
        /// </summary>
        /// <param name="pickViews"></param>
        private void LoadInventoryForPickViews(List<ReplenPickView> pickViews)
        {
            // get the areaId from the WorkstationView
            var areaId = _workstationView.AreaId;

            _logger.LogDetailAsync($"Load Inventory For PickViews START").SafeFireAndForget();
            // get a distinct list of ItemIds from the pickviews
            var itemIds = pickViews.Select(r => r.ItemId).Distinct().ToList();

            // get the current inventory in this area for all the distinct items in the pickviews
            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => itemIds.Contains(f.ItemDefinitionId) && f.AreaId == areaId).ToList();

            _logger.LogDetailAsync($"Load Inventory For PickViews END").SafeFireAndForget();
        }

        private void LoadInventory()
        {

            _logger.LogDetailAsync($"Load Inventory START").SafeFireAndForget();
            var pickableLocations = _repoStorageTypes.FindBy(r => r.Pickable == true).Select(r => r.Id).ToList();  // new int[] { 1, 2 };  // 4 is an EBin
                                                                                                                   // var pickableLocations = new int[] { 1, 2 };  // 4 is an EBin
            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => pickableLocations.Contains(f.StorageTypeId)).ToList();
            _logger.LogDetailAsync($"Load Inventory END").SafeFireAndForget();
        }

        private async void DataGridViewAvailableOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            ManualOverrideCurrentTextBoxPos = false;
            if (e.RowIndex < 0) return;

            var ord1 = GetOrd1(e.RowIndex);
            try
            {
                var rows = GetMatchingRows(ord1);
                if (rows.Any())
                {
                    var row = rows.FirstOrDefault();
                    if (row != null)
                    {
                        var id = Convert.ToInt32(row.Cells["Id"].Value);
                        var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in Cell Click: {ex.Message}").ConfigureAwait(false);
                throw;
            }
            SetFocusNextTextBoxPos();

        }

        /// <summary>
        /// Retrieves the order ID from the specified row index in the DataGridView.
        /// </summary>
        /// <param name="rowIndex">The index of the row from which to retrieve the order ID.</param>
        /// <returns>The order ID as an integer.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified row index is out of range of the DataGridView rows.
        /// </exception>
        private int GetOrderId(int rowIndex)
        {
            return Convert.ToInt32(DataGridViewAvailableOrders.Rows[rowIndex].Cells["Id"].Value);
        }

        private string GetOrd1(int rowIndex)
        {
            return Convert.ToString(DataGridViewAvailableOrders.Rows[rowIndex].Cells["Ord1"].Value);
        }

        private List<DataGridViewRow> GetMatchingRows(string orderNumber)
        {
            return DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                .Where(r => Convert.ToString(r.Cells["Ord1"].Value) == orderNumber)
                .ToList();
        }
        private DataGridViewRow GetRowByTaskNumber(List<DataGridViewRow> rows)
        {
            var taskNumbers = rows.Select(r => Convert.ToString(r.Cells["Ord2"].Value)).ToList();
            var taskNumber = PromptForTaskNumber(taskNumbers);
            return taskNumber == null ? null : rows.FirstOrDefault(r => Convert.ToString(r.Cells["Ord2"].Value) == taskNumber);
        }

        private string PromptForTaskNumber(List<string> taskNumbers)
        {
            try
            {
                //create a dialog box to get the value of Ord2 in the order
                if (!taskNumbers.Any())
                {
                    throw new ArgumentNullException(nameof(taskNumbers), "No Task Numbers were passed in.");
                }
                var input = this.ShowInputDialogAsync("Enter Task Number", "Task Number", taskNumbers);
                return input;
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogDetailAsync($"{ex.Message}").SafeFireAndForget();
                return string.Empty;
            }
        }

        private string ShowInputDialogAsync(string title, string labelText, List<string> taskNumbers)
        {
            try
            {
                var form = new FrmTaskNumber(taskNumbers);
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    return form.TaskNumber;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogDetailAsync($"{ex.Message}").SafeFireAndForget();
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Adds or removes an order from the induction screen based on its presence.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to be processed.</param>
        /// <param name="rowIndex"></param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the status of the operation:
        /// -1 if the order was removed or not found, otherwise the result of adding the order.
        /// </returns>
        /// <remarks>
        /// If the order is already present in the induction screen, it will be removed. 
        /// If the order is not present, it will be added.
        /// </remarks>
        private async Task<int> AddRemoveOrderFromInductionScreen(int orderId, int rowIndex)
        {
            if (orderId == 0) return -1;
            var orderToProcess = _ordersToPick.FirstOrDefault(r => r.OrderId == orderId);
            if (orderToProcess != null)
            {
                DataGridViewAvailableOrders.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
                await RemoveOrderFromInductionScreen(orderToProcess);
                return -1;
            }
            var order = _repoReplenOrder.FindByKey(orderId);
            if (order == null) return -1;
            DataGridViewAvailableOrders.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LawnGreen;
            return await AddOrderToInductionScreen(order);
        }

        /// <summary>
        /// Asynchronously removes an order from the induction screen.
        /// </summary>
        /// <param name="orderToProcess">The order to be removed, represented by a <see cref="BatchPosition"/> object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method interacts with the <see cref="WorkstationView"/> and <see cref="IIptiDisplayFunctions"/> to turn off the batch display
        /// for the specified order and remove the order from the batch.
        /// </remarks>
        private async Task RemoveOrderFromInductionScreen(BatchPosition orderToProcess)
        {
            if (_workstationView.BatchTable != null && _iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.TurnOffBatchDisplay(orderToProcess.PositionNumber);
            }
            await RemoveItemFromBatch(orderToProcess.OrderId);
        }

        /// <summary>
        /// Adds an order to the induction screen.
        /// </summary>
        /// <param name="order">The order to be added.</param>
        /// <returns>
        /// The position in the batch array where the order was added, or -1 if the order could not be added.
        /// </returns>
        /// <remarks>
        /// This method updates the batch display with the order information if the order is successfully added.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="order"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the batch table is not initialized.</exception>
        private async Task<int> AddOrderToInductionScreen(ReplenOrder order)
        {
            var arrayPosition = await AddItemToBatch(order.Id, order.Ord1, order.Ord2);
            if (arrayPosition == -1 || _workstationView.BatchTable == null) return arrayPosition;
            var ord = order.Ord1.Substring(order.Ord1.Length - 4);
            var position = _ordersToPick[arrayPosition].PositionNumber;
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.TurnOnBatchDisplay(position, ord);
            }
            return arrayPosition;
        }

        //private async Task<int> AddRemoveOrderFromInductionScreen(int orderId)
        //{

        //    var idx = -1;
        //    if (orderId == 0) return idx;
        //    var orderToProcess = _ordersToPick.FirstOrDefault(r => r.OrderId == orderId);
        //    if (orderToProcess != null)
        //    {
        //        // already inducted, so remove the order
        //        if (_workstationView.BatchTable != null)
        //        {
        //            if (_iptiDisplayFunctions != null)
        //            {
        //                await _iptiDisplayFunctions.TurnOffBatchDisplay(orderToProcess.PositionNumber);
        //            }
        //        }

        //        RemoveItemFromBatch(orderToProcess.OrderId);
        //    }
        //    else //must be an add order
        //    {
        //        var order = _repoReplenOrder.FindByKey(orderId);
        //        var ord1 = order.Ord1;
        //        var ord2 = order.Ord2;

        //        idx = AddItemToBatch(orderId, ord1, ord2);
        //        // ---         _logger.LogDetailAsync($"AddItemToBatch RETURN"));
        //        if (idx != -1)
        //        {
        //            if (_workstationView.BatchTable != null)
        //            {
        //                var ord = ord1.Substring(ord1.Length - 4);
        //                var position = _ordersToPick[idx].PositionNumber;
        //                if (_iptiDisplayFunctions != null)
        //                {
        //                    await _iptiDisplayFunctions.TurnOnBatchDisplay(position, ord);
        //                }
        //            }
        //        }
        //    }
        //    return idx;
        //}




        //Back button on Pick Screen
        //private void PickBack()
        //{
        //    // ---          _logger.LogDetailAsync($"PickBack START"));
        //    //LabelFormTitle.Text = _resourceManager.GetString($"PickList");
        //    //LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
        //    //tabControl1.SelectedTab = PickList;


        //    // ClearAllShi();
        //    //ClearAllBli();
        //    //ClearOc();


        //    _iptiDisplayFunctions?.ClearBatchTable();

        //    _iptiDisplayFunctions?.TurnOffBatchOrderControl();

        //    GlobalVar.Hanel?.ResetHanelDeviceStatus();

        //    _iptiDisplayFunctions?.ClearBlastzone();

        //    // Clear all the ProLites using the ProLiteManager

        //    _workstationView.ProLiteManager?.ClearAllProlites();


        //    _deviceIndicatorManager?.ClearActiveDeviceIndicators();


        //    NextButtonEnabled();
        //    //TODO  commented out because I'm not handling something correctly
        //    // and items are getting stuck in Pick status
        //    UpdateOrdersToAvailableStatus(_ordersToPick);

        //    LabelFormTitle.Text = _resourceManager.GetString($"PickList");
        //    LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
        //    tabControl1.SelectedTab = PickList;
        //    // ---         _logger.LogDetailAsync($"PickBack END"));
        //}

        //private void RemoveItemFromBatch(int orderId)
        //{
        //    var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == orderId);
        //    if (bp != null)
        //    {
        //        bp.OrderId = 0;
        //        bp.Ord1 = string.Empty;
        //        bp.Ord2 = string.Empty;
        //        bp.OrderComplete = false;
        //        UpdateTextBoxPosition(bp);
        //    }
        //    NextButtonEnabled();
        //}

        private async Task RemoveItemFromBatch(int orderId)
        {
            var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == orderId);
            if (bp != null)
            {
                bp.OrderId = 0;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                await UpdateTextBoxPosition(bp);
            }
        }


        private async Task<int> AddItemToBatch(int orderId, string ord1, string ord2)
        {
            var idx = ManualOverrideCurrentTextBoxPos
                ? SetBatchPositionToManualOverride()
                : SetBatchPositionToFirstEmpty();

            if (idx >= 0 && idx < _neutronVariables.StoreBatchSize)
            {
                if (!OrderInBatch(orderId))
                {
                    _ordersToPick[idx].OrderId = orderId;
                    _ordersToPick[idx].Ord1 = ord1;
                    _ordersToPick[idx].Ord2 = ord2;
                    _ordersToPick[idx].OrderComplete = false;
                    _currentTextBoxPos.Text = $"{ord1}";  //{Environment.NewLine}{ord2}";

                    // Now we can turn off the Manual override
                    ManualOverrideCurrentTextBoxPos = false;

                    await NextButtonEnabled();
                }
                else
                {
                    ManualOverrideCurrentTextBoxPos = true;
                    _currentTextBoxPos.SelectAll();
                    _currentTextBoxPos.Focus();
                }
            }

            return idx;
        }

        private bool OrderInBatch(int orderId)
        {
            var result = _ordersToPick.FirstOrDefault(r => r.OrderId == orderId);
            return result != null;
        }

        private int SetBatchPositionToManualOverride()
        {
            var result = -1;
            result = int.Parse(_currentTextBoxPos.Tag.ToString());
            return result;
        }

        private void ClearAllSelectOrdersToPick()
        {
            DataGridViewAvailableOrders.ClearSelection();
            //foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            //{
            //    bool checkedBox = Convert.ToBoolean(row.Cells[0].Value);

            //    if (checkedBox)
            //    {
            //        int orderId = Convert.ToInt32(row.Cells["Id"].Value);
            //        row.Cells[0].Value = false;
            //        await RemoveItemFromBatch(orderId);
            //    }
            //}
        }


        /// <summary>
        /// Clears the batch positions asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method resets the batch positions for all orders to pick.
        /// </remarks>
        private async Task ClearBatchPositions()
        {
            var resetTasks = _ordersToPick.Select(ResetBatchPosition);
            await Task.WhenAll(resetTasks);
        }

        /// <summary>
        /// Resets the properties of the specified <see cref="BatchPosition"/> object and updates the corresponding TextBox control.
        /// </summary>
        /// <param name="batchPosition">The <see cref="BatchPosition"/> object to be reset.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method clears the properties of the <see cref="BatchPosition"/> object using <see cref="ClearBatchPosition(BatchPosition)"/> 
        /// and then updates the corresponding TextBox control with the new position using <see cref="UpdateTextBoxPosition(BatchPosition)"/>.
        /// </remarks>
        private async Task ResetBatchPosition(BatchPosition batchPosition)
        {
            ClearBatchPosition(batchPosition);
            await UpdateTextBoxPosition(batchPosition);
        }

        /// <summary>
        /// Clears the properties of the specified <see cref="BatchPosition"/> object.
        /// </summary>
        /// <param name="batchPosition">The <see cref="BatchPosition"/> object to be cleared.</param>
        /// <remarks>
        /// This method resets the <see cref="BatchPosition.OrderId"/> to 0, 
        /// <see cref="BatchPosition.Ord1"/> and <see cref="BatchPosition.Ord2"/> to empty strings, 
        /// and <see cref="BatchPosition.OrderComplete"/> to false.
        /// </remarks>
        private void ClearBatchPosition(BatchPosition batchPosition)
        {
            batchPosition.OrderId = 0;
            batchPosition.Ord1 = string.Empty;
            batchPosition.Ord2 = string.Empty;
            batchPosition.OrderComplete = false;
        }



        //private void ClearBatchPositions()
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        bp.OrderId = 0;
        //        bp.Ord1 = string.Empty;
        //        bp.Ord2 = string.Empty;
        //        bp.OrderComplete = false;
        //        UpdateTextBoxPosition(bp);
        //    }
        //}

        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            Control c = Controls.Find($"TextBoxPos{batchPositionNumber}", true).Single() as TextBox;
            if (c != null) _currentTextBoxPos = (TextBox)c;
            // _currentTextBoxPos.BackColor = Color.Yellow;
            _currentTextBoxPos.Focus();
        }

        /// <summary>
        /// Initializes a list of <see cref="BatchPosition"/> objects to be picked.
        /// </summary>
        /// <param name="pickBatchSize">The number of batch positions to initialize.</param>
        /// <returns>A list of initialized <see cref="BatchPosition"/> objects.</returns>
        private List<BatchPosition> InitOrdersToPick(int pickBatchSize)
        {
            var ordersToPick = new List<BatchPosition>(pickBatchSize);
            for (var index = 0; index < pickBatchSize; index++)
            {
                ordersToPick.Add(CreateBatchPosition(index));
            }
            return ordersToPick;
        }
        /// <summary>
        /// Creates a new instance of the <see cref="BatchPosition"/> class with the specified index.
        /// </summary>
        /// <param name="index">The index to be used for the <see cref="BatchPosition.PositionNumber"/>.</param>
        /// <returns>A new instance of the <see cref="BatchPosition"/> class.</returns>
        private BatchPosition CreateBatchPosition(int index)
        {
            return new BatchPosition
            {
                PositionNumber = index + 1,
                OrderId = 0,
                Ord1 = string.Empty,
                Ord2 = string.Empty,
                OrderComplete = false
            };
        }




        /// <summary>
        /// Initializes the list of orders to be picked.
        /// </summary>
        /// <param name="pickBatchSize">The size of the batch to be picked.</param>
        //private void InitOrdersToPick(int pickBatchSize)
        //{
        //    _ordersToPick = new List<BatchPosition>();
        //    for (var index = 0; index < pickBatchSize; index++)
        //    {
        //        var batchPosition = new BatchPosition
        //        {
        //            PositionNumber = index + 1,
        //            OrderId = 0,
        //            Ord1 = string.Empty,
        //            Ord2 = string.Empty,
        //            OrderComplete = false
        //        };
        //        _ordersToPick.Add(batchPosition);
        //    }
        //}

        //private void ShowPosition(int position)
        //{
        //    //var font = new Font("Microsoft Sans Serif", 20);
        //    var pos = position.ToString();

        //    Control c = Controls.Find("LabelPickPos" + pos, true).Single() as Label;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("LabelPos" + pos, true).Single() as Label;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
        //    if (c != null)
        //    {
        //        c.Visible = true;
        //        // c.Font = font;
        //    }

        //    c = Controls.Find("TextBoxPos" + pos, true).Single() as TextBox;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("Pos" + pos + "Display", true).Single();
        //    ((Panel)c).Visible = true;

        //    c = Controls.Find("AvailablePos" + pos + "Display", true).Single();
        //    ((Panel)c).Visible = true;
        //}

        //private void ShowOrdersToPick()
        //{
        //    _logger.LogDetailAsync($"ShowOrdersToPick Start: [{DateTime.Now.ToLongTimeString()}]"));
        //    // var font = new Font("Microsoft Sans Serif", 10);
        //    for (var i = 0; i < _ordersToPick.Count; i++)
        //    {
        //        var pos = (i + 1).ToString();
        //        var textBox = Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
        //        if (textBox == null) continue;
        //        textBox.SizeTextBoxFont(2);
        //        textBox.Text = $"{_ordersToPick[i].Ord1}";  //{Environment.NewLine}{_ordersToPick[i].Ord2}";
        //    }

        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.BliEnabled)
        //            {
        //                _logger.LogDetailAsync($"Show Orders To Pick On Displays Clear ALL Bli"));
        //                //GlobalVar.Displays.ClearAllBli();
        //                foreach (var item in _ordersToPick)
        //                {
        //                    if (string.IsNullOrEmpty(item.Ord1)) continue;
        //                    _logger.LogDetailAsync($"Show Each Order To Pick {item.Ord1}"));
        //                    GlobalVar.Displays.ShowBli(item.PositionNumber, beacon: 2, text: item.Ord1);
        //                }
        //            }
        //        }
        //    }

        //    _logger.LogDetailAsync($"ShowOrdersToPick End: [{DateTime.Now.ToLongTimeString()}]"));
        //}

        private async Task ShowOrderOnBatchDisplay()
        {
            var font = new Font(Font.FontFamily, 26);
            if (_workstationView.AreaId == AreaEight)
            {
                font = new Font(Font.FontFamily, 26);
            }

            var batchPositions = _ordersToPick.Where(r => r.Ord1 != string.Empty).ToList();
            foreach (var batchPosition in batchPositions)
            {
                TextBox textBox = Controls.Find($"TextBoxPickPos{batchPosition.PositionNumber}", true).First() as TextBox;
                if (textBox == null) continue;
                textBox.Font = font;  //textBox.SizeTextBoxFont(2);

                var len = batchPosition.Ord1.Trim().Length;
                var text = batchPosition.Ord1.Substring(len - 4);

                //textBox.Text = $"{batchPosition.Ord1.Trim()}";  //{Environment.NewLine}{batchPosition.Ord2.Trim()}";
                textBox.Text = text;
                if (_iptiDisplayFunctions != null)
                {
                    await _iptiDisplayFunctions.TurnOnBatchDisplay(batchPosition.PositionNumber, text);

                }
                Task.Delay(_iptiConfig.TransmitDelay).Wait();
                // TurnOnIptiDisplay(_neutronVariables.BliController, batchPosition.PositionNumber, text);
            }
        }

        private async Task ShowQuantityOnBatchDisplay()
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();

            }

            var batchPositions = _ordersToPick.Where(r => r.Ord1 != string.Empty).ToList();
            foreach (var batchPosition in batchPositions)
            {
                TextBox textBox = Controls.Find($"TextBoxPickPos{batchPosition.PositionNumber}", true).First() as TextBox;
                if (textBox == null) continue;
                textBox.Text = string.Empty;
                if (batchPosition.OrderComplete)
                {
                    textBox.Text = "END";
                    if (_iptiDisplayFunctions != null)
                    {
                        await _iptiDisplayFunctions.TurnOnBatchDisplayEnd(batchPosition.PositionNumber);
                    }
                }
                Task.Delay(_iptiConfig.TransmitDelay).Wait();
            }

            //ClearBatchTable();
            foreach (var pickView in _currentPickStop.PickViews)
            {
                var pos = pickView.PickPosition;
                var textBox = (TextBox)Controls.Find($"TextBoxPickPos{pos}", true).First();
                if (textBox != null)
                {
                    //textBox.SizeTextBoxFont(1);
                    textBox.Font = new Font(textBox.Font.FontFamily, 26);

                    //textBox.Text = pickView.QuantityToBePicked.ToString();
                    if (textBox.Text == string.Empty)
                    {
                        textBox.Text = pickView.QuantityToBePicked.ToString();
                        if (_iptiDisplayFunctions != null)
                        {
                            await _iptiDisplayFunctions.TurnOnBatchDisplay(pos, pickView.QuantityToBePicked.ToString());
                        }
                    }
                }

                var panel = (Panel)Controls.Find($"Pos{pos}Display", true).First();
                if (panel != null)
                {
                    panel.BackColor = Color.Red;
                }

                // TurnOnIptiDisplay(_neutronVariables.BliController, pos, pickView.QuantityToBePicked.ToString());

            }
        }

        private async void MBShowOrderOrQuantityToggle_Click(object sender, EventArgs e)
        {
            await ShowOrderOrQuantityToggle();
        }

        private async Task ShowOrderOrQuantityToggle()
        {
            if (MBShowOrderOrQuantityToggle.Text == _resourceManager.GetString($"ShowJobs"))
            {
                // Task.Run(ClearBatchTable);

                // ClearPickPositions();
                await ShowOrderOnBatchDisplay();
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowQuantity");
            }
            else
            {
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
                //UpdatePickPosition();
                await ShowQuantityOnBatchDisplay();
            }





            //if (MBShowOrderOrQuantityToggle.Text == _resourceManager.GetString($"ShowJobs"))
            //{
            //    ClearAllBli();
            //    ClearPickPositions();
            //    ShowOrdersToPick();
            //    MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowQuantity");
            //}
            //else
            //{
            //    MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
            //    UpdatePickPosition();
            //}
        }

        //private void TurnOnBlastzoneDisplay(int bayController, int position, string text)
        //{
        //    _logger.LogDetailAsync($"Turn On Blastzone Display - START"));
        //    try
        //    {
        //        if (_workstationView.Blastzones.Any())
        //        {
        //            var bayId = bayController.ToString().PadLeft(2, '0');
        //            var controller = _tcpIptiCommandCenter.BlastBayControllers.FirstOrDefault(r => r.BayId == bayId);
        //            if (controller == null) return;
        //            var command = controller.TurnOnDisplay(position, text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"Turn On Blastzone Display Function Failed:{Environment.NewLine}{ex.Message}"));

        //    }
        //    _logger.LogDetailAsync($"Turn On Blastzone Function - END"));
        //}

        //private void TurnOnBlastzoneOrderControl(int bayController, string text)
        //{
        //    _logger.LogDetailAsync($"ClearBlastzone Function - START"));
        //    try
        //    {
        //        if (_workstationView.Blastzones.Any())
        //        {
        //            var bayId = bayController.ToString().PadLeft(2, '0');
        //            var command = _tcpIptiCommandCenter.GetBayController(bayId)
        //                .TurnOnOrderControlModule(text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"ClearBlastzone Function Failed:{Environment.NewLine}{ex.Message}"));

        //    }
        //    _logger.LogDetailAsync($"ClearBlastzone Function - END"));

        //    //var bayId = bayController.ToString().PadLeft(2, '0');
        //    //        var command = _tcpIptiCommandCenter.GetBayController(bayId)
        //    //            .TurnOnOrderControlModule(text);
        //    //        GlobalVar.Displays.SendText(command);
        //    //    }
        //    //}
        //}

        //private void ClearBlastzone()
        //{
        //    _logger.LogDetailAsync($"ClearBlastzone Function - START"));
        //    try
        //    {
        //        if (_workstationView.Blastzones.Any())
        //        {
        //            foreach (var bayController in _tcpIptiCommandCenter.BlastBayControllers)
        //            {
        //                var text = bayController.ClearDisplays();
        //                GlobalVar.Displays.SendText(text);

        //                text = bayController.TurnOffOrderControlModule();
        //                GlobalVar.Displays.SendText(text);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"ClearBlastzone Function Failed:{Environment.NewLine}{ex.Message}"));

        //    }
        //    _logger.LogDetailAsync($"ClearBlastzone Function - END"));
        //}

        //private void ClearBatchTable()
        //{
        //    try
        //    {
        //        if (_workstationView.BatchTable != null)
        //        {
        //            var command = _tcpIptiCommandCenter.BatchBayController.ClearDisplays();
        //            GlobalVar.Displays.SendText(command);
        //            Thread.Sleep(100);
        //            command = _tcpIptiCommandCenter.BatchBayController.TurnOffOrderControlModule();

        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"ClearBatchTable Function Failed:{Environment.NewLine}{ex.Message}"));
        //    }
        //}

        //private void TurnOnBatchDisplay(int bayControllerId, int position, int beacon, string text)
        //{
        //    try
        //    {
        //        if (_workstationView.BatchTable != null)
        //        {
        //            var command = _tcpIptiCommandCenter.BatchBayController.TurnOnDisplay(position, text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"TurnOn Batch Display Function Failed:{Environment.NewLine}{ex.Message}"));
        //    }
        //}
        //private void TurnOnBatchOrderControl(string text)
        //{
        //    try
        //    {
        //        if (_workstationView.BatchTable != null)
        //        {
        //            var command = _tcpIptiCommandCenter.BatchBayController.TurnOnOrderControlModule(text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"TurnOn Batch Order Control Function Failed:{Environment.NewLine}{ex.Message}"));
        //    }
        //}




        ///// <summary>
        ///// Done
        ///// </summary>
        ///// <param name="bayController"></param>
        ///// <param name="position"></param>
        ///// <param name="text"></param>
        //private void TurnOnIptiDisplay(int bayController, int position, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;
        //            var command = _tcpIptiCommandCenter.TurnOnDisplay(bayController.ToString(), position, text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //}

        //private void TurnOnIptiOrderControl(int bayController, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;

        //            var bayId = bayController.ToString().PadLeft(2, '0');
        //            var command = _tcpIptiCommandCenter.GetBayController(bayId).TurnOnOrderControlModule(text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //}

        //private void ClearBatchTable()
        //{
        //    if (_neutronVariables.IptiDisplays)
        //    {
        //        if (GlobalVar.Displays == null) return;
        //        var bayId = _neutronVariables.BliController.ToString();

        //        var command = _tcpIptiCommandCenter.ClearBayController(bayId);
        //        GlobalVar.Displays.SendText(command);

        //        command = _tcpIptiCommandCenter.GetBayController(bayId).TurnOffOrderControlModule();

        //        GlobalVar.Displays.SendText(command);
        //    }
        //}



        private async Task NextButtonEnabled()
        {

            var enabled = await CheckForValidOrder();


            if (!enabled)
            {
                // ---         _logger.LogDetailAsync($"Next Button Enabled: false"));
                MBGo.Enabled = false;
                MBGo2.Enabled = false;
            }
            else
            {
                // ---         _logger.LogDetailAsync($"Next Button Enabled: true"));
                MBGo.Enabled = true;
                MBGo2.Enabled = true;
            }

        }


        private async Task<bool> CheckForValidOrder()
        {
            // loop over all TextBoxPosx
            // and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            // Clear invalid OrdersToPick
            _logger.LogDetailAsync($"Start").SafeFireAndForget();
            try
            {
                foreach (var bp in _ordersToPick)
                {
                    var pos = bp.PositionNumber.ToString();
                    var c = Controls.Find($"TextBoxPos{pos}", true).First();
                    if (c != null)
                    {
                        var textBox = (TextBox)c;
                        var order = textBox.Text.Trim();
                        //Is this a real order
                        if (!string.IsNullOrEmpty(order))
                        {
                            var isRealOrder = _repoReplenOrder.FindBy(r => r.Ord1 == order).FirstOrDefault();
                            if (isRealOrder != null) return true;

                            MessageBox.Show($"The Delivery {order} in Position {pos} does not exist.");
                            await ClearItemFromBatchByPosition(bp.PositionNumber);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
                MessageBox.Show($"The Check for Valid Orders has failed. {Environment.NewLine}" +
                                $"{ex.Message}");
                return false;
            }

            _logger.LogDetailAsync($"End ").SafeFireAndForget();
            return false;

        }

        private async void MBPickBack_Click(object sender, EventArgs e)
        {
            await ExecutePickBackProcess();
        }

        private async Task ExecutePickBackProcess()
        {
            await ClearDisplayFunctions();
            ResetHanelDeviceStatus();
            ClearProLites();
            ClearActiveDeviceIndicators();
            await EnableNextButton();
            await UpdateOrdersStatus();
            UpdateFormTitleAndTab();
        }
        private async Task ClearDisplayFunctions()
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                await _iptiDisplayFunctions.ClearBlastzone();
            }
        }
        private void ResetHanelDeviceStatus()
        {
            if (GlobalVar.Hanel != null)
            {
                GlobalVar.Hanel.ResetHanelDeviceStatus();
            }
        }
        private void ClearProLites()
        {
            _workstationView.ProLiteManager?.ClearAllProlites();
        }
        private void ClearActiveDeviceIndicators()
        {
            _deviceIndicatorManager?.ClearActiveDeviceIndicators();
        }
        private async Task EnableNextButton()
        {
            await NextButtonEnabled();
        }
        private async Task UpdateOrdersStatus()
        {
            await UpdateOrdersToAvailableStatus(_ordersToPick);
        }
        private void UpdateFormTitleAndTab()
        {
            LabelFormTitle.Text = _resourceManager.GetString($"PickList");
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = PickList;
        }

        private async Task UpdateOrdersToAvailableStatus(List<BatchPosition> ordersToPick)
        {
            var batchPositions = GetIncompleteBatchPositions(ordersToPick);
            foreach (var batchPosition in batchPositions)
            {
                await UpdateOrderDetailsStatus(batchPosition);
            }
        }
        private List<BatchPosition> GetIncompleteBatchPositions(List<BatchPosition> ordersToPick)
        {
            return ordersToPick.Where(r => r.OrderId != 0 && r.OrderComplete == false).ToList();
        }
        private async Task UpdateOrderDetailsStatus(BatchPosition batchPosition)
        {
            var orderDetails = GetPickingOrderDetails(batchPosition);
            foreach (var orderDetail in orderDetails)
            {
                await SetOrderDetailStatusToAvailable(orderDetail);
            }
        }
        private IEnumerable<ReplenOrderDetail> GetPickingOrderDetails(BatchPosition batchPosition)
        {
            return _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == batchPosition.OrderId && r.AreaId == _workstationView.AreaId && r.LineStatusId == (int)LineStatus.Picking);
        }
        private async Task SetOrderDetailStatusToAvailable(ReplenOrderDetail orderDetail)
        {
            orderDetail.LineStatusId = (int)LineStatus.Available;
            await _repoReplenOrderDetails.UpdateAsync(orderDetail);
            var order = _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
            order.OrderStatusId = (int)OrderStatus.Available;
            await _repoReplenOrder.UpdateAsync(order);
        }

        private async Task SetOrderDetailStatusToPicking(ReplenOrderDetail orderDetail)
        {
            orderDetail.LineStatusId = (int)LineStatus.Picking;
            await _repoReplenOrderDetails.UpdateAsync(orderDetail);
            var order =  _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
            order.OrderStatusId = (int)OrderStatus.Picking;
            await _repoReplenOrder.UpdateAsync(order);
        }


        //private void UpdateOrdersToAvailableStatus(List<BatchPosition> ordersToPick)
        //{
        //    var batchPositions = ordersToPick.Where(r => r.OrderId != 0 && r.OrderComplete == false).ToList();
        //    foreach (var batchPosition in batchPositions)
        //    {
        //        var orderDetails = _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == batchPosition.OrderId && r.AreaId == _workstationView.AreaId && r.LineStatusId == (int)LineStatus.Picking);
        //        foreach (var orderDetail in orderDetails)
        //        {
        //            orderDetail.LineStatusId = (int)LineStatus.Available;
        //            _repoReplenOrderDetails.Update(orderDetail);
        //        }
        //    }
        //}

        private async void MBStart_Click(object sender, EventArgs e)
        {
            MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
            await Start();
        }
        //private void Start()
        //{
        //    _logger.LogDetailAsync($"Start_Click Start: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    var pickViews = (IList<ReplenPickView>)bindingSourcePickViews.DataSource;
        //    pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();
        //    //TODO SetOrderStatusToPicking(pickViews);
        //    _logger.LogDetailAsync($"Start_Click 1: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    var pickStops = new List<ReplenPickStop>();
        //    IEnumerable<IGrouping<string, ReplenPickView>> pickViewGroups = pickViews.GroupBy(r => r.Item).ToList();
        //    int sequence = 0;
        //    foreach (var pickViewGroup in pickViewGroups)  //for each Item in the group of Items
        //    {
        //        int total = 0;
        //        // a ReplenPickStop is of One Item that may be on One to All Pick Positions
        //        // a ReplenPickView is an individual pick at a single Pick Position
        //        // so a ReplenPickStop is has One or Multiple PickViews that are concerned 
        //        // with picking One Item.
        //        // a ReplenPickStop is a summary of all the PickViews 
        //        // and some of the information in a ReplenPickStop is the same as in a ReplenPickView
        //        // that is why the First ReplenPickView is used to provide most of the data to the ReplenPickStop
        //        ReplenPickView firstPickView = pickViewGroup.First();

        //        var pickStop = new ReplenPickStop();

        //        pickStop.Sequence = sequence += 1;
        //        pickStop.OrderId = firstPickView.ReplenOrderId;
        //        pickStop.Ord1 = firstPickView.Ord1;
        //        pickStop.Ord2 = firstPickView.Ord2;
        //        pickStop.ItemId = firstPickView.ItemId;
        //        pickStop.Item = firstPickView.Item;
        //        pickStop.Description = firstPickView.Description;
        //        pickStop.UnitOfIssue = firstPickView.UnitOfIssue;
        //        pickStop.PickedQty = 0;
        //        pickStop.Images = firstPickView.Images;
        //        pickStop.Inventory = firstPickView.Inventory;
        //        pickStop.InventoryIndex = firstPickView.InventoryIndex;
        //        pickStop.Slot = firstPickView.Slot;
        //        pickStop.SlotQty = firstPickView.SlotQty;
        //        pickStop.CurrentInventoryLocation = firstPickView.CurrentInventoryLocation;
        //        pickStop.TotalQuantityInInventory = firstPickView.TotalQuantityInInventory;
        //        foreach (var pickView in pickViewGroup)
        //        {
        //            pickStop.PickViews.Add(pickView);
        //            total += pickView.Quantity;
        //        }
        //        pickStop.Quantity = total;
        //        pickStop.QuantityToBePicked = total;

        //        pickStops.Add(pickStop);
        //    }
        //    _logger.LogDetailAsync($"Start_Click 2: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    List<ReplenPickStop> finalPickSequence = FinalPickSequence(pickStops);
        //    bindingSourcePickStops.DataSource = finalPickSequence;
        //    _logger.LogDetailAsync($"Start_Click 3 Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    // GetFirstStop();
        //    bindingSourcePickStops.MoveFirst();
        //    currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
        //    UpdatePickScreen();
        //    _logger.LogDetailAsync($"Start_Click 4  Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    // PrintAllDocuments();
        //    // PrintAllToteLabels();

        //    tabControl1.SelectedTab = PickScreen;
        //    //feels good to here
        //    _logger.LogDetailAsync($"Start_Click End: [{System.DateTime.Now.ToLongTimeString()}]"));
        //}

        private List<ReplenPickStop> FinalPickSequence(List<ReplenPickStop> pickStops)
        {
            _logger.LogDetailAsync($"FinalPickSequence Start: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var newCarList = new List<List<ReplenPickStop>>();
            var newList = new List<ReplenPickStop>();
            for (var i = 0; i < _workstationView.HardwareDevices.Count; i++)
            {
                var carList = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == i + 1)
                    .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
                    .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                    .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
                    .ToList();
                newCarList.Add(carList);
            }
            var seq = 1;
            var totalPickStops = pickStops.Count;

            for (var i = 0; i < totalPickStops; i++)
            {
                for (var j = 0; j < newCarList.Count; j++)
                {
                    if (newCarList[j].Count >= i + 1)
                    {
                        newCarList[j][i].Sequence = seq;
                        seq += 1;
                        newList.Add(newCarList[j][i]);
                    }
                }
            }
            //_logger.LogDetailAsync($"FinalPickSequence Start Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            _deviceManager = new ReplenDeviceManager(newCarList, _neutronVariables.ShuttleEnabled);
            //for (var i = 1; i <= _workstationView.HardwareDevices.Count; i++)
            //{
            //    _deviceManager.MoveNext(i);
            //}
            //_logger.LogDetailAsync($"FinalPickSequence End Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            //_logger.LogDetailAsync($"FinalPickSequence End: [{DateTime.Now.ToLongTimeString()}]"));


            if (_deviceManager != null)
            {
                _logger.LogDetailAsync($"FinalPickSequence Start Carousel Move: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                // _deviceManager = new ReplenDeviceManager(newCarList, _neutronVariables.ShuttleEnabled);
                for (var i = 1; i <= _workstationView.Hanels.Count; i++)
                {
                    _deviceManager.MoveNext(i);
                }

                _logger.LogDetailAsync($"FinalPickSequence End Carousel Move: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                _logger.LogDetailAsync($"FinalPickSequence End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }
            return newList;
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            _logger.LogDetailAsync($"3384 PositionDevice").SafeFireAndForget();
            if (_neutronVariables.ShuttleEnabled)
            {
                _logger.LogDetailAsync($"4056 PositionDevice").SafeFireAndForget();
                if (GlobalVar.Shuttle != null)
                {
                    _logger.LogDetailAsync($"4061 Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}").SafeFireAndForget();

                    _logger.LogDetailAsync($"4063 PositionDevice").SafeFireAndForget();
                    GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4);
                }
                if (GlobalVar.Hanel != null)
                {
                    _logger.LogDetailAsync($"Hanel Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}").SafeFireAndForget();

                    _logger.LogDetailAsync($"Hanel PositionDevice").SafeFireAndForget();
                    GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                }
            }
        }

        private async Task GetFirstStop(bool moveDevice = true)
        {
            _logger.LogDetailAsync($"GetFirstStop: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                Console.WriteLine("Clear Device Indicator - Get First Stop");

                _bindingSourcePickStops.MoveFirst();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;

                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.LogDetailAsync($"3012 GetFirstStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}").SafeFireAndForget();
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            _logger.LogDetailAsync($"GetFirstStop End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        }

        private async Task GetNextStop(bool moveDevice = true)
        {
            _logger.LogDetailAsync($"GetNextStop: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence < numberOfStops)
            {
                Console.WriteLine("Clear Device Indicator - Get Next Stop");

                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                //_deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.LogDetailAsync($"3445 GetNextStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}").SafeFireAndForget();
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);

            }
            _logger.LogDetailAsync($"GetNextStop Return: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        }

        private async Task GetPreviousStop(bool moveDevice = true)
        {
            _logger.LogDetailAsync($"Get Prev Stop START").SafeFireAndForget();

            if (_currentPickStop.Sequence > 0)
            {
                _bindingSourcePickStops.MovePrevious();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.LogDetailAsync($"3470 GetPreviousStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}").SafeFireAndForget();
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            _logger.LogDetailAsync($"Get Prev Stop END ").SafeFireAndForget();
        }

        private async Task GetLastStop(bool moveDevice = true)
        {
            _logger.LogDetailAsync($"GetLastStop: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                _bindingSourcePickStops.MoveLast();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                // UpdateTowerDisplay();
                _logger.LogDetailAsync($"3494 GetLastStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}").SafeFireAndForget();

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            _logger.LogDetailAsync($"GetLastStop Return").SafeFireAndForget();
        }

        private async void ButtonStopMoveFirst_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await GetFirstStop(moveDevice: false);
            Cursor.Current = Cursors.Default;

        }

        private async void ButtonStopMovePrevious_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await GetPreviousStop(moveDevice: false);
            Cursor.Current = Cursors.Default;

        }

        private async void ButtonStopMoveNext_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await GetNextStop(moveDevice: false);
            Cursor.Current = Cursors.Default;

        }

        private async void ButtonStopMoveLast_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            await GetLastStop(moveDevice: false);
            Cursor.Current = Cursors.Default;
        }


        //private void UpdatePickScreen()
        //{
        //    _logger.LogDetailAsync($"UpdatePickScreen Start: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();


        //    if (_neutronVariables.UseImages) PictureBoxItemImage.LoadAsync(_imageManager.GetImageFile(_currentPickStop.Item));
        //    LabelFormTitle.Text = _resourceManager.GetString($"Selection");
        //    LabelPickDescription.Text = _currentPickStop.Description;
        //    LabelPickItemNumber.Text = _currentPickStop.Item;
        //    LabelPickUOI.Text = _currentPickStop.UnitOfIssue;
        //    LabelLineOfLines.Text = string.Format("{0} of {1}"
        //        , _currentPickStop.Sequence.ToString(), _bindingSourcePickStops.Count);
        //    TextBoxRequestedQty.Text = _currentPickStop.Quantity.ToString();

        //    var pickedSoFar = GetPickedSoFar(_currentPickStop.PickViews);
        //    TextBoxPickedSoFar.Text = pickedSoFar.ToString();

        //    var quantityToBePicked = _currentPickStop.QuantityToBePicked;
        //    LabelPickQty.Text = quantityToBePicked.ToString();
        //    _logger.LogDetailAsync($"UpdatePickScreen End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

        //    MBStoreAccept.Enabled = quantityToBePicked > 0;

        //    MBPickChangeQuantity.Enabled = quantityToBePicked > 0;
        //    MBSkipPick.Enabled = quantityToBePicked > 0;
        //    // MBShortPick.Enabled = quantityToBePicked > 0;

        //    // MultipleLocationsManager(_currentPickStop);

        //}
        //private void MultipleLocationsManager(PickStop currentPickStop)
        //{
        //    // GroupBoxMultipleLocations.Visible = false;
        //    var pickLocations = new List<PickLocation>();
        //    if (currentPickStop == null) return;
        //    var pickViews = _currentPickStop.PickViews.Where(r => r.PickLocations.Count > 0).ToList();
        //    if (!pickViews.Any()) return;
        //    foreach (var pickView in pickViews)
        //    {
        //        foreach (var pickLocation in pickView.PickLocations)
        //        {
        //            pickLocations.Add(pickLocation);
        //        }
        //    }

        //    var distinctLocations = pickLocations.Distinct().ToList();
        //    //GroupBoxMultipleLocations.Visible = distinctLocations.Count != 1;
        //}
        //private async Task UpdatePickScreenAfterChangeQuantity()
        //{
        //    _logger.LogDetailAsync($"UpdatePickScreen AfterChangeQuantity Start: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

        //    await UpdatePickPosition();
        //    LabelPickQty.Text = (_currentPickStop.QuantityToBePicked).ToString();

        //    _logger.LogDetailAsync($"UpdatePickScreen AfterChangeQuantity End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        //}

        private void UpdateTowerDisplay()
        {
            //var loc1 = _currentPickStop.CurrentInventoryLocation.Location == null ? string.Empty :  _currentPickStop.CurrentInventoryLocation.Location.Loc1;

            //var loc2 = _currentPickStop.CurrentInventoryLocation == null ? string.Empty :  _currentPickStop.CurrentInventoryLocation.Location.Loc2;

            //var loc3 = _currentPickStop.CurrentInventoryLocation == null ? string.Empty :  _currentPickStop.CurrentInventoryLocation.Location.Loc3;

            //var loc4 = _currentPickStop.CurrentInventoryLocation == null ? string.Empty :  _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();


            //var text =     _currentPickStop.QuantityToBePicked.ToString();
            //ShowShi(loc1, loc2, loc3, loc4, text);

        }


        private void UpdateGroupBoxLocation(Inventory inventory)
        {
            _logger.LogDetailAsync($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

            //TextBoxPickLoc1.Text = inventory.Location.Loc1.ToString();
            //TextBoxPickLoc2.Text = inventory.Location.Loc2.ToString();
            //TextBoxPickLoc3.Text = inventory.Location.Loc3.ToString();
            //TextBoxPickLoc4.Text = inventory.Location.Loc4.ToString();
            //TextBoxPickLoc5.Text = inventory.Location.Loc5.ToString();textboxpickposleave
            //LabelLocationNumber.Text =
            //    $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            //TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            //TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            //TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G");
            //LabelPrimeBin.Visible = inventory.PrimeBin;
            //LabelStaticRelease.Text = inventory.StorageType.Name;
            //_logger.LogDetailAsync($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]"));

            _logger.LogDetailAsync($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            if (_workstationView.StationTypeId == 3)
            {

                //}
                //if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Rack)
                //{
                //show slot
                LabelDevice.Visible = true;
                LabelTray.Visible = false;
                LabelOver.Visible = false;
                LabelBack.Visible = false;

                TextBoxPickLoc1.Visible = true;
                TextBoxPickLoc2.Visible = false;
                TextBoxPickLoc3.Visible = false;
                TextBoxPickLoc4.Visible = false;
                TextBoxPickLoc5.Visible = false;

                LabelDevice.Text = @"Slot";
                LabelDevice.Location = new Point(150, 30);
                LabelDevice.Size = new Size(100, 22);

                TextBoxPickLoc1.Location = new Point(50, 57);
                TextBoxPickLoc1.Size = new Size(300, 62);
                TextBoxPickLoc1.Text = inventory.Location == null ? string.Empty : inventory.Location.Slot;
                TextBoxSlot.Text = inventory.Location == null ? string.Empty : inventory.Location.Slot;
            }

            if (_workstationView.StationType.Id == 7)
            {

                //}
                //if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Blastzone)
                //{
                LabelTray.Text = @"Unit";
                LabelBack.Text = @"Display";
                LabelTray.Location = new Point(70, 30);
                LabelTray.Size = new Size(100, 22);
                LabelBack.Location = new Point(230, 30);
                LabelBack.Size = new Size(100, 22);
                TextBoxPickLoc2.Location = new Point(70, 57);
                TextBoxPickLoc2.Size = new Size(100, 62);
                TextBoxPickLoc4.Location = new Point(230, 57);
                TextBoxPickLoc4.Size = new Size(100, 62);

                LabelDevice.Visible = false;
                LabelTray.Visible = true;
                LabelOver.Visible = false;
                LabelBack.Visible = true;
                TextBoxPickLoc1.Visible = false;
                TextBoxPickLoc2.Visible = true;
                TextBoxPickLoc3.Visible = false;
                TextBoxPickLoc4.Visible = true;
                TextBoxPickLoc5.Visible = false;

                TextBoxPickLoc1.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc1.ToString();
                TextBoxPickLoc2.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc2.ToString();
                TextBoxPickLoc3.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc3.ToString();
                TextBoxPickLoc4.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc4.ToString();
                TextBoxPickLoc5.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc5.ToString();
            }

            if (_workstationView.StationTypeId == 1 || _workstationView.StationTypeId == 2)
            {

                //}
                //if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Vertical)
                //{
                LabelDevice.Visible = true;
                LabelTray.Visible = true;
                LabelOver.Visible = true;
                LabelBack.Visible = true;
                TextBoxPickLoc1.Visible = true;
                TextBoxPickLoc2.Visible = true;
                TextBoxPickLoc3.Visible = true;
                TextBoxPickLoc4.Visible = true;
                TextBoxPickLoc5.Visible = true;

                TextBoxPickLoc1.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc1.ToString();
                TextBoxPickLoc2.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc2.ToString();
                TextBoxPickLoc3.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc3.ToString();
                TextBoxPickLoc4.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc4.ToString();
                TextBoxPickLoc5.Text = inventory.Location == null ? string.Empty : inventory.Location.Loc5.ToString();
            }

            LabelLocationNumber.Text = $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = inventory.PrimeBin;
            LabelStaticRelease.Text = inventory.StorageType == null ? string.Empty : inventory.StorageType.Name;

            TextBoxSlot.FocusAndHighlightText();

            _logger.LogDetailAsync($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

        }


        //private void UpdateInventoryLocation()
        //{
        //    _logger.LogDetailAsync($"UpdateInventoryLocation Start : [{System.DateTime.Now.ToLongTimeString()}]"));

        //    string loc1 = TextBoxPickLoc1.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
        //    string loc2 = TextBoxPickLoc2.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
        //    string loc3 = TextBoxPickLoc3.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
        //    string loc4 = TextBoxPickLoc4.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
        //    string loc5 = TextBoxPickLoc5.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc5.ToString();

        //    ShowShi(loc1.ParseInt(), loc2.ParseInt(), loc3.ParseInt(), loc4, _currentPickStop.QuantityToBePicked.ToString());

        //    LabelLocationNumber.Text = string.Format(format: "{0} of {1}"
        //        , arg0: _currentPickStop.InventoryIndex + 1, arg1: _currentPickStop.Inventory.Count);
        //    TextBoxLocationQuantity.Text = _currentPickStop.CurrentInventoryLocation.Quantity.ToString();
        //    TextBoxTotalQuantity.Text = _currentPickStop.TotalQuantityInInventory.ToString();
        //    TextBoxReceivedDate.Text = _currentPickStop.CurrentInventoryLocation.ReceivedDate.ToString("G");
        //    LabelPrimeBin.Visible = _currentPickStop.CurrentInventoryLocation.PrimeBin;
        //    _logger.LogDetailAsync($"UpdateInventoryLocation End : [{System.DateTime.Now.ToLongTimeString()}]"));
        //}

        //private void ShowShi(int loc1, int loc2, int loc3, string loc4, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.ShiEnabled)
        //            {
        //                // ClearAllShi();

        //                _logger.LogDetailAsync($"Frm Replen Show SHI {loc1}-{loc2}-{loc3}-{loc4}{Environment.NewLine}-{text}"));

        //                GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text);
        //            }
        //        }
        //    }

        //}

        //private void ClearAllShi()
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.ShiEnabled)
        //            {
        //                _logger.LogDetailAsync($"Frm Replen ClearAllShi"));
        //                GlobalVar.Displays.ClearAllShi();
        //            }
        //        }
        //    }
        //}

        //private void ClearAllBli()
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.BliEnabled)
        //            {
        //                _logger.LogDetailAsync($"Frm Replen ClearAllBli"));
        //                GlobalVar.Displays.ClearAllBli();
        //            }
        //        }
        //    }
        //}

        //private void TurnOnOcDisplay(int bayControllerId, int position, int beacon, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.BliEnabled)
        //            {
        //                GlobalVar.Displays.ShowOc(_neutronVariables.BliController, position, beacon, text));
        //            }
        //        }
        //    }
        //}

        private async Task UpdatePickPosition()
        {
            _logger.LogDetailAsync($"UpdatePickPosition Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            SetOrderCompleteThisArea();
            ClearPickPositions();
            ClearPickDisplays();
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                await _iptiDisplayFunctions.ClearBlastzone();
                await _iptiDisplayFunctions.TurnOnBatchOrderControl(_currentPickStop.Item.Trim());
            }


            _workstationView.ProLiteManager?.ClearAllProlites();


            foreach (var pickView in _currentPickStop.PickViews)
            {
                var pos = pickView.PickPosition;

                var textBox = (TextBox)Controls.Find($"TextBoxPickPos{pos}", true).First();
                if (textBox != null)
                {
                    textBox.Font = new Font(textBox.Font.FontFamily, 26);
                    //textBox.SizeTextBoxFont(1);
                    textBox.Text = pickView.QuantityToBePicked.ToString();
                }

                var panel = (Panel)Controls.Find($"Pos{pos}Display", true).First();
                if (panel != null)
                {
                    panel.BackColor = Color.Red;
                }
                if (_iptiDisplayFunctions != null)
                {
                    await _iptiDisplayFunctions.TurnOnBatchDisplay(pos, pickView.QuantityToBePicked.ToString());
                }
            }

            var device = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var bayController = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var display = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.TurnOnBlastzoneDisplay(bayController, display, _currentPickStop.GetTotalQuantityToBePicked().ToString());
                await _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(bayController, _currentPickStop.Item.Trim());
            }

            _workstationView.ProLiteManager?.TurnOn(device, bayController, display, _currentPickStop.GetTotalQuantityToBePicked());

            _logger.LogDetailAsync($"UpdatePickPosition END").SafeFireAndForget();

        }

        ///// <summary>
        ///// Done
        ///// </summary>
        ///// <param name="bayController"></param>
        ///// <param name="position"></param>
        ///// <param name="text"></param>
        //private async Task TurnOnIptiDisplayAsync(int bayController, int position, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;
        //            var command = _tcpIptiCommandCenter.TurnOnDisplay(bayController.ToString(), position, text);
        //            // await GlobalVar.Displays.SendText(command);
        //            await _tcpIptiController.SendText(command);

        //        }
        //    }
        //}
        //private void TurnOnIptiOrderControlAsync(int bayController, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;

        //            var bayId = bayController.ToString().PadLeft(2, '0');
        //            var command = _tcpIptiCommandCenter.GetBayController(bayId)
        //                .TurnOnOrderControlModule(text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //}

        //private void ClearBatchTableAsync()
        //{
        //    _logger.LogDetailAsync($"Clear Batch Table Function - START"));

        //    if (_neutronVariables.IptiDisplays)
        //    {
        //        if (GlobalVar.Displays == null) return;
        //        var bayId = _neutronVariables.BliController.ToString();

        //        var command = _tcpIptiCommandCenter.ClearBayController(bayId);
        //        GlobalVar.Displays.SendText(command);

        //        command = _tcpIptiCommandCenter.GetBayController(bayId).TurnOffOrderControlModule();
        //        GlobalVar.Displays.SendText(command);

        //    }

        //    _logger.LogDetailAsync($"Clear Batch Table Function - END"));
        //}


        ///// <summary>
        ///// Done
        ///// </summary>
        ///// <param name="bayController"></param>
        ///// <param name="position"></param>
        ///// <param name="beacon"></param>
        ///// <param name="text"></param>
        //private void TurnOnBlastzoneAsync(int bayController, int position, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;
        //            var command = _tcpIptiCommandCenter.TurnOnDisplay(bayController.ToString(), position, text);
        //            GlobalVar.Displays.SendText(command);
        //        }
        //    }
        //}
        ///// <summary>
        ///// Done
        ///// </summary>
        //private void ClearBlastzoneAsync()
        //{
        //    _logger.LogDetailAsync($"ClearBlastzone Function - START"));
        //    if (_neutronVariables.DisplaysEnabled && _blastzone)
        //    {
        //        if (_neutronVariables.IptiDisplays)
        //        {
        //            if (GlobalVar.Displays == null) return;

        //            var blastzoneBayControllers = _tcpIptiCommandCenter.BayControllers
        //                .Where(r => r.BayControllerType == "Blast").ToList();
        //            foreach (var blastzoneBayId in blastzoneBayControllers)
        //            {
        //                var command = _tcpIptiCommandCenter.ClearBayController(blastzoneBayId.BayId);
        //                GlobalVar.Displays.SendText(command);
        //                command = _tcpIptiCommandCenter.GetBayController(blastzoneBayId.BayId).TurnOffOrderControlModule();
        //                GlobalVar.Displays.SendText(command);
        //            }
        //        }
        //    }
        //    _logger.LogDetailAsync($"ClearBlastzone Function - END"));
        //}
        private void SetOrderCompleteThisArea()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == 0) continue;
                var linesNotComplete = _repoReplenOrderDetails
                    .FindBy(r => r.ReplenOrderId == bp.OrderId && r.AreaId == _workstationView.AreaId)
                    .Where(r => r.LineStatusId != (int)LineStatus.Complete).ToList();
                if (linesNotComplete.Count != 0) continue;
                bp.OrderComplete = true;
            }
        }

        //private void TurnOnBatchPositionDisplay(int position, int beacon, string text)
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            if (_neutronVariables.BliEnabled)
        //            {
        //                _logger.LogDetailAsync($"Frm Replen TurnOnBatchPositionDisplay"));
        //                GlobalVar.Displays.ShowBli(position, beacon, text);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Clear and Reset all Order Positions
        /// </summary>
        private void ClearOrderPositions()
        {
            if (_ordersToPick == null) return;
            foreach (var bp in _ordersToPick)
            {
                var pos = bp.PositionNumber.ToString();
                var c = Controls.Find($"TextBoxPos{pos}", true).FirstOrDefault();
                if (c != null)
                {
                    var textBox = ((TextBox)c);
                    textBox.Text = string.Empty;
                }
            }
        }

        private void ClearPickPositions()
        {
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"TextBoxPickPos{pos}", true).First();
                if (c != null)
                {
                    var textBox = ((TextBox)c);
                    // textBox.Font = font;
                    textBox.Text = bp.OrderComplete ? "END" : string.Empty;
                }
            }
        }

        private void ClearPickDisplays()
        {
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"Pos{pos}Display", true).First();
                if (c != null)
                {
                    var panel = ((Panel)c);
                    panel.BackColor = bp.OrderComplete ? Color.Black : Color.Transparent;
                }
            }
        }

        //private void MBPickStore_Click(object sender, EventArgs e)
        //{
        //    PickAccept();
        //    MBStoreAccept.Focus();
        //}

        //private void PickAccept()
        //{



        //    Cursor.Current = Cursors.WaitCursor;
        //    _logger.LogDetailAsync($"PickAccept_Click Start : [{DateTime.Now.ToLongTimeString()}]"));
        //    bool pick = false;
        //    pick = _currentPickStop.CurrentInventoryLocation.Quantity < _currentPickStop.QuantityToBePicked ? false : true;

        //    if (pick)
        //    {
        //        _currentPickStop.UpdatePickViews(GlobalVar.User);  //good
        //        _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);
        //        _currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews
        //        _logger.LogDetailAsync($"PickAccept_Click 1 : [{DateTime.Now.ToLongTimeString()}]"));
        //        if (StopComplete())
        //        {
        //            //Getting next location on the current device/ the one that was just picked from.
        //            _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1));
        //            _logger.LogDetailAsync($"PickAccept_Click 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]"));

        //            UpdateInventoryQuantity(_currentPickStop);

        //            _historyManager.SaveHistory(ActionCode.StoreOrder, _currentPickStop);

        //            _currentPickStop.SetPickViewsComplete(GlobalVar.User);

        //            _logger.LogDetailAsync($"PickAccept_Click Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

        //            int numberOfStops = _bindingSourcePickStops.Count;
        //            if (_currentPickStop.Sequence < numberOfStops)
        //            {
        //                _bindingSourcePickStops.MoveNext();
        //                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
        //                UpdatePickScreen();
        //            }
        //            else
        //            {
        //                CloseBatchAsync();
        //            }

        //        }
        //        else  //ReplenPickStop is NOT complete, why?
        //        {
        //            UpdatePickScreen();
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("Pick Exceeds Inventory at this location.  Add Inventory or Change Quantity before continuing.", "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        //    }
        //    _logger.LogDetailAsync($"PickAccept_Click End : [{DateTime.Now.ToLongTimeString()}]"));
        //    Cursor.Current = Cursors.Default;
        //}

        private async void MBStoreAccept_Click(object sender, EventArgs e)
        {
            await StoreAccept();
            MBStoreAccept.Focus();
        }

        private async Task StoreAccept()
        {

            //if (InvokeRequired)
            //{
            //    var method = new MethodInvoker(StoreAccept);
            //    Invoke(method);
            //    return;
            //}

            var slot = TextBoxSlot.Text.Trim().ToUpper();
            // get the new location based on the scanned in slot number
            if (!string.IsNullOrEmpty(slot))
            {
                var location = GetScannedLocation(slot);
                //MessageBox.Show($"You scanned Location: {slot} and found Location: {location.Slot}");

                if (location == null)
                {
                    MessageBox.Show($"Invalid Location: {slot}");
                    return;
                }

                var itemDef = _currentPickStop.ItemId;

                var inventoryRecord = _repoInventory.FindBy(r => r.ItemDefinitionId == _currentPickStop.ItemId
                                                                     && r.LocationId == location.Id).FirstOrDefault();


                if (inventoryRecord == null)
                {
                    var newInventoryRecord = new Inventory()
                    {
                        AreaId = location.AreaId,
                        ItemDefinitionId = _currentPickStop.ItemId,
                        LocationId = location.Id,
                        PrimeBin = false,
                        Quantity = 0,
                        ReceivedDate = DateTime.Now,
                        StorageTypeId = (int)NeutronCore.Enums.StorageType.Release,
                        RFID = string.Empty
                    };

                    await _repoInventory.InsertAsync(newInventoryRecord);

                    var fullInventoryRecord = _repoInventory.FindBy(r => r.ItemDefinitionId == newInventoryRecord.ItemDefinitionId
                    && r.LocationId == newInventoryRecord.LocationId).FirstOrDefault();

                    _currentPickStop.CurrentInventoryLocation = fullInventoryRecord;
                }
                else
                {
                    _currentPickStop.CurrentInventoryLocation = inventoryRecord;
                }



                //_currentPickStop.CurrentInventoryLocation.Location = location;
                //_currentPickStop.CurrentInventoryLocation.LocationId = location.Id;
                //_currentPickStop.CurrentInventoryLocation.ItemDefinitionId = _currentPickStop.ItemId;
                TextBoxSlot.Text = string.Empty;
            }
            //else
            //{
            //    // use the current Inventory Item in the Group Box
            //    // is other words the _currentPickStops Inventory Item
            //}



            // NeutronDllu exit to do something with the selection
            // before processing the replenishment. 
            // For example: Ask operator to scan a lot or serial number
            // Verify something else
            // Returns true is process is to continue
            // Return false if the process is canceled
            if (!SelectAction.StoreAccept(_currentPickStop)) return;


            Cursor.Current = Cursors.WaitCursor;
            _logger.LogDetailAsync($"StoreAccept_Click Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            MBStoreAccept.Enabled = false;
            //ClearActiveDeviceIndicators();
            //ClearAllDeviceIndicators();

            _currentPickStop.UpdatePickViews(GlobalVar.User); //good

            _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);

            // QuantityToBePicked on ALL PickViews
            _currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);

            _logger.LogDetailAsync($"PickAccept_Click 1 : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            if (await StopComplete())
            {
                //Getting next location on the current device/ the one that was just picked from.
                if (_currentPickStop != null)
                {
                    if (_currentPickStop.CurrentInventoryLocation != null)
                    {
                        _deviceManager?.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                    }
                }

                _logger.LogDetailAsync($"PickAccept_Click 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

                UpdateInventoryQuantity(_currentPickStop);

                _historyManager.SaveHistory(ActionCode.StoreOrder, _currentPickStop);

                _currentPickStop.SetPickViewsComplete(GlobalVar.User);

                foreach (var pickView in _currentPickStop.PickViews)
                {
                    CheckForOrderCompleteOnDevice(pickView.OrderDetail.ReplenOrder);
                }

                int numberOfStops = _bindingSourcePickStops.Count;
                var position = _bindingSourcePickStops.Position;



                if (_currentPickStop.Sequence < numberOfStops)
                {
                    //Use the first carousel location for the movenext in case multiple picks are required for stop
                    if (_deviceManager != null)
                    {
                        _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                    }

                    _bindingSourcePickStops.MoveNext();
                    _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                    await UpdatePickScreen();
                    // UpdateCurrentDeviceIndicator();
                    // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                    await UpdatePickPosition();
                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    // UpdateTowerDisplay();
                }
                else
                {
                    await CloseBatchAsync();
                }

            }
            else //ReplenPickStop is NOT complete, why?
            {
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                var text = _currentPickStop.QuantityToBePicked.ToString();

                PositionDevice(loc1, loc2, loc3, loc4, true);

                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
            }


            _logger.LogDetailAsync($"StoreAccept_Click End : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            MBStoreAccept.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private Location GetScannedLocation(string slot)
        {

            return _repoLocationRepository.FindBy(r => r.Slot.Trim() == slot).FirstOrDefault();

        }

        private bool CheckForOrderCompleteOnDevice(ReplenOrder order)
        {
            var linesNotComplete = _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return false;

            order.OrderStatusId = (int)NeutronCore.Enums.OrderStatus.Complete;
            _historyManager.SaveHistory(ActionCode.OrderComplete, order);
            _repoReplenOrder.Update(order);
            return true;
        }

        public int GetTotalQuantityToBePicked(IList<ReplenPickView> pickViews)
        {
            int total = 0;
            foreach (var pickview in pickViews)
            {
                total += pickview.QuantityToBePicked;
            }

            return total;
        }

        private int GetPickedSoFar(IList<ReplenPickView> pickViews)
        {
            int total = 0;
            foreach (var pickview in pickViews)
            {
                total += GetPickViewTotal(pickview);
            }

            return total;
        }

        private int GetPickViewTotal(ReplenPickView pickview)
        {
            int total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }

            return total;
        }

        private async Task<bool> StopComplete()
        {
            var result = false;
            // they could put more away than the Replen order indicated
            if (_currentPickStop.QuantityToBePicked <= 0)
            {
                result = true; //Stop Complete
            }
            else if (_currentPickStop.QuantityToBePicked > 0) // Still have qty to put away
            {
                var movedToNewLocation = await NextPickingLocation(); // moved to next existing location so don't end batch return false
                if (!movedToNewLocation)
                {
                    // no more locations
                    //var locations = LoadNewLocations(_currentPickStop.ItemId, _numberOfInventoryLocations)).Result;
                    //if (locations.Count > 0)
                    //{
                    //    var location = locations.First();
                    //    var storageType = _defaultStorageType;
                    //    var inventory = new Inventory
                    //    {
                    //        ItemDefinition = _currentPickStop.CurrentInventoryLocation.ItemDefinition,
                    //        ItemDefinitionId = _currentPickStop.ItemId,
                    //        LocationId = location.Id,
                    //        Location = location,
                    //        Quantity = 0,
                    //        ReceivedDate = DateTime.Now,
                    //        PrimeBin = false,
                    //        AreaId = _currentPickStop.CurrentInventoryLocation.AreaId,
                    //        Workstation = _currentPickStop.CurrentInventoryLocation.Workstation,
                    //        StorageTypeId = storageType.Id,
                    //        StorageType = storageType,
                    //        RFID = string.Empty
                    //    };
                    //_currentPickStop.Inventory.Add(inventory);
                    //_currentPickStop.InventoryIndex += 1;
                    //_currentPickStop.GroupBoxLocationInventoryIndex = _currentPickStop.InventoryIndex;
                    //_currentPickStop.CurrentInventoryLocation = inventory;
                    //}
                    //else
                    //{
                    //    MessageBox.Show(_resourceManager.GetString($"NoAvailableLocations"), _resourceManager.GetString($"NewLocations"));
                    //}
                }
            }

            return result;
        }

        private async Task<List<Inventory>> GetNewInventoryLocations(ItemDefinition itemDefinition,
            int inventoryLocationsNeeded = 3)
        {
            var inventory = new List<Inventory>();
            var locationViews = LoadNewLocations(itemDefinition, inventoryLocationsNeeded);

            var storageType = _defaultStorageType;
            if (locationViews.Any())
            {
                foreach (var loc in locationViews)
                {
                    var location = _repoLocationRepository.FindByKey(loc.Id);
                    var inv = new Inventory
                    {
                        ItemDefinition = itemDefinition,
                        ItemDefinitionId = itemDefinition.Id,
                        LocationId = loc.Id,
                        Location = location,
                        Quantity = 0,
                        ReceivedDate = DateTime.Now,
                        PrimeBin = false,
                        AreaId = itemDefinition.AreaId,
                        StorageTypeId = storageType.Id,
                        StorageType = storageType,
                        RFID = string.Empty
                    };
                    inventory.Add(inv);
                }
            }
            else
            {
                if (_workstationView.AreaId == AreaEight)
                {
                    // create a default location in Area 8
                    var loc = _repoLocationRepository.FindByKey(10954);
                    if (loc != null)
                    {
                        var inv = new Inventory
                        {
                            ItemDefinition = itemDefinition,
                            ItemDefinitionId = itemDefinition.Id,
                            LocationId = loc.Id,
                            Location = loc,
                            Quantity = 0,
                            ReceivedDate = DateTime.Now,
                            PrimeBin = false,
                            AreaId = itemDefinition.AreaId,
                            StorageTypeId = storageType.Id,
                            StorageType = storageType,
                            RFID = string.Empty
                        };
                        inventory.Add(inv);
                    }
                }
            }

            return inventory;
        }

        private List<LocationView> LoadNewLocations(ItemDefinition itemDefinition,
            int numberOfInventoryLocations)
        {
            // Return top 20 locations
            // First get exact matches
            // Then add the balance needed from the rest in order of size sequence
            var areaId = itemDefinition.AreaId;
            var item = itemDefinition;
            var sizeCodeId = item.SizeCodeId;
            var velocityCodeId = item.VelocityCodeId;
            var heightCodeId = item.HeightCodeId;

            var maxSizeCodeInThisArea = _sizeCodes.Last().Id;
            var maxVelocityCodeInThisArea = _velocityCodes.Last().Id;
            var maxHeightCodeInThisArea = _heightCodes.Last().Id;

            var validSizeCodes = _sizeCodes.Where(r => r.Id >= sizeCodeId).ToList();
            var validVelocityCodes = _velocityCodes.Where(r => r.Id >= velocityCodeId).ToList();
            var validHeightCodes = _heightCodes.Where(r => r.Id >= heightCodeId).ToList();

            var stillNeeded = numberOfInventoryLocations;
            var locations = new List<LocationView>();
            var haveAllLocations = false;
            var sb = new StringBuilder();

            //foreach (var velocityCode in validVelocityCodes)
            //{
            //    var v = velocityCode.Id;
            //    foreach (var sizeCode in validSizeCodes)
            //    {
            //        var s = sizeCode.Id;
            //        sb.AppendLine($"Size Code: {s}  Velocity Code: {v}  Height Code: {heightCodeId}");
            //    }
            //}

            //_logger.LogDetailAsync($"{sb.ToString()}");


            try
            {

                foreach (var velocityCode in validVelocityCodes)
                {
                    var v = velocityCode.Id;
                    foreach (var sizeCode in validSizeCodes)
                    {
                        var s = sizeCode.Id;
                        sb.AppendLine($"Size Code: {s}  Velocity Code: {v}  Height Code: {heightCodeId}");

                        var views = _locationsRepository.GetAllLocationViewsExact(areaId, s, v, heightCodeId, false).ToList();

                        if (views.Any())
                        {
                            // got all we needed
                            if (views.Count >= stillNeeded)
                            {
                                var locs = views.Take(stillNeeded).ToList();
                                locations.AddRange(locs);
                                haveAllLocations = true;
                                // return all we needed
                                break;
                            }
                            if (views.Count < stillNeeded)
                            {
                                var locs = views.ToList();
                                locations.AddRange(locs);
                            }
                        }
                    }

                    if (haveAllLocations)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            return locations;

        }

        private List<VelocityCode> LoadVelocityCodesByArea(int areaId)
        {
            var velocityCodes = new List<VelocityCode>();
            try
            {
                velocityCodes = _replenOrdersRepository.GetVelocityCodesByArea(areaId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }

            return velocityCodes;
        }

        private List<SizeCode> LoadSizeCodesByArea(int areaId)
        {
            var sizeCodes = new List<SizeCode>();
            try
            {
                sizeCodes = _replenOrdersRepository.GetSizeCodesByArea(areaId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }

            return sizeCodes;
        }

        private List<HeightCode> LoadHeightCodesByArea(int areaId)
        {
            var heightCodes = new List<HeightCode>();
            try
            {
                heightCodes = _replenOrdersRepository.GetHeightCodesByArea(areaId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }

            return heightCodes;
        }
        private int GetNextLargerSizeCode(int sizeCodeId)
        {
            try
            {
                SizeCode sizeCode = null;
                foreach (var s in _sizeCodes)
                {
                    if (s.Id > sizeCodeId)
                    {
                        sizeCode = s;
                        break;
                    }
                }

                if (sizeCode != null) return sizeCode.Id;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }
            return 0;
        }

        private int GetNextLargerVelocityCode(int velocityCodeId)
        {
            try
            {
                VelocityCode velocityCode = null;
                foreach (var v in _velocityCodes)
                {
                    if (v.Id > velocityCodeId)
                    {
                        velocityCode = v;
                        break;
                    }
                }

                if (velocityCode != null) return velocityCode.Id;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }
            return 0;
        }

        private int GetNextLargerHeightCode(int heightCodeId)
        {
            try
            {
                HeightCode heightCode = null;
                foreach (var s in _heightCodes)
                {
                    if (s.Id > heightCodeId)
                    {
                        heightCode = s;
                        break;
                    }
                }

                if (heightCode != null) return heightCode.Id;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
            }
            return 0;
        }

        // this is called when there are multiple locations on the same PickStop
        private async Task<bool> NextPickingLocation()
        {
            bool result = false;

            if (_workstationView.AreaId == AreaEight)
            {
                TextBoxSlot.Text = string.Empty;
                TextBoxSlot.Focus();
            }

            // current Inventory Index
            var index = _currentPickStop.InventoryIndex;
            // add one to index and see if there is another inventory location to pick from
            var idx = _currentPickStop.InventoryIndex + 1 < _currentPickStop.Inventory.Count
                ? _currentPickStop.InventoryIndex + 1
                : 0;
            // if there are no more locations pre-defined
            // create a new Default location/Inventory and add it to the 
            // PickStop.Inventory list
            if (idx == 0)
            {

                // create a new Default location/Inventory List
                var inv = await GetDefaultInventory(_currentPickStop.ItemId, _workstationView.AreaId);
                // Next Inventory location
                // add it to the PickStop.Inventory list
                _currentPickStop.Inventory.Add(inv.First());

                var newIndex = _currentPickStop.Inventory.Count - 1;

                _currentPickStop.GroupBoxLocationInventoryIndex = newIndex;
                var inventory = _currentPickStop.Inventory[newIndex];
                _currentPickStop.InventoryIndex = newIndex;
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[newIndex];
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var slot = _currentPickStop.CurrentInventoryLocation.Location.Slot;
                _logger.LogDetailAsync($"Get Next Picking Location: {loc1}-{loc2} Slot: {slot}").SafeFireAndForget();
                result = true;

            }
            if (idx > 0)
            {
                // Next Inventory location
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                _currentPickStop.InventoryIndex = idx;
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[idx];
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var slot = _currentPickStop.CurrentInventoryLocation.Location.Slot;
                _logger.LogDetailAsync($"Get Next Picking Location: {loc1}-{loc2} Slot: {slot}").SafeFireAndForget();
                result = true;
            }
            return result;
        }

        private void UpdateInventoryQuantity(ReplenPickStop pickStop)
        {
            Inventory inventory;

            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    if (pickLocation.Quantity > 0)
                    {
                        pickLocation.Inventory.Quantity += pickLocation.Quantity;

                        if (pickLocation.Inventory.Location.Id == 0)
                        {
                            inventory = _repoInventory.FindBy(r => r.LocationId == pickStop.CurrentInventoryLocation.LocationId).FirstOrDefault();
                        }
                        else
                        {
                            inventory = _repoInventory.FindByKey(pickLocation.Inventory.Id);
                        }

                        if (inventory == null)
                        {
                            // Create an Inventory and get the Id
                            if (pickLocation.Inventory.Id == 0)
                            {
                                var inv = new Inventory
                                {
                                    ItemDefinitionId = pickLocation.Inventory.ItemDefinitionId,
                                    LocationId = pickLocation.Inventory.LocationId,
                                    Quantity = pickLocation.Inventory.Quantity,
                                    ReceivedDate = pickLocation.Inventory.ReceivedDate,
                                    PrimeBin = pickLocation.Inventory.PrimeBin,
                                    AreaId = pickLocation.Inventory.AreaId,
                                    StorageTypeId = 2,  //   pickLocation.Inventory.StorageTypeId,
                                    RFID = string.Empty
                                };
                                _repoInventory.Insert(inv);
                                pickLocation.Inventory.Id = inv.Id;
                            }


                            //  _repoInventory.Insert(pickLocation.Inventory);
                        }
                        else
                        {
                            //pickLocation.Inventory.Quantity += pickLocation.Quantity;
                            _repoInventory.Update(pickLocation.Inventory);
                        }


                        var location = _repoLocationRepository.FindByKey(pickLocation.Inventory.LocationId);
                        location.InUse = true;
                        _repoLocationRepository.Update(location);
                    }
                }
            }
        }

        private Inventory GetNewInventoryLocation(PickLocation pickLocation)
        {
            // Create a new Inventory
            var inventory = new Inventory
            {
                ItemDefinitionId = pickLocation.Inventory.ItemDefinitionId,
                LocationId = pickLocation.Inventory.LocationId,
                Quantity = pickLocation.Inventory.Quantity,
                ReceivedDate = pickLocation.Inventory.ReceivedDate,
                PrimeBin = pickLocation.Inventory.PrimeBin,
                AreaId = pickLocation.Inventory.AreaId,
                StorageTypeId = pickLocation.Inventory.StorageTypeId,
                RFID = string.Empty
            };
            _repoInventory.Insert(inventory);

            return inventory;
        }

        private async Task CloseBatchAsync()
        {

            GlobalVar.Hanel?.ResetHanelDeviceStatus();
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                await _iptiDisplayFunctions.ClearBlastzone();
            }

            ClearOrderPositions();
            await ClearBatchPositions();
            _logger.LogDetailAsync("Clear All Device Indicators - Close Batch").SafeFireAndForget();

            _deviceIndicatorManager?.ClearAllDeviceIndicators();

            _workstationView.ProLiteManager?.ClearAllProlites();

            ParkPositionAfterBatch();

            if (_neutronVariables.AutoLogOff)
            {
                CloseButtonPressed = true;
                Close();
            }
            else
            {
                //ShowAllOrders();
                //ShowAvailableOrders();
                //tabControl1.SelectedTab = AvailableOrders;
                await AvailableOrdersScreen();
            }

        }

        private void ParkPositionAfterBatch()
        {
            if (!_neutronVariables.ParkPositionAfterBatch) return;
            if (!_neutronVariables.ShuttleEnabled) return;
            if (GlobalVar.Shuttle == null) return;
            var response = GlobalVar.Shuttle.Park();
            if (response == DeviceResponse.Success) return;
            if (response != DeviceResponse.TrayDidNotArrive)
            {
                MessageBox.Show(response.AsString(EnumFormat.Description)
                    , caption: _resourceManager.GetString($"DeviceInformation")
                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        //private void PrintAllDocuments()
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true)
        //            .FirstOrDefault();
        //        if (printJob != null) continue;
        //        PrintDoc(bp.PositionNumber, order);
        //        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
        //        _repoPrintJob.Insert(printJob);
        //    }
        //}

        //private void PrintDocument(int batchPosition)
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.PositionNumber != batchPosition) continue;
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true)
        //            .FirstOrDefault();
        //        if (printJob != null) continue;
        //        PrintDoc(bp.PositionNumber, order);
        //        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
        //        _repoPrintJob.Insert(printJob);
        //    }
        //}

        //private void PrintDoc(int positionNumber, ReplenOrder order)
        //{
        //    _logger.LogDetailAsync($"Printing Document. {order.Ord1}"));
        //    if (_neutronVariables.EnableDocumentPrinter)
        //    {
        //        _documentToPrint.Print(positionNumber, order.Ord1, _documentPrinter, order.Ord2));
        //    }
        //}

        //private void PrintAllToteLabels()
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
        //        if (printJob != null) continue;
        //        PrintTote(bp.PositionNumber, order);
        //        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
        //        _repoPrintJob.Insert(printJob);
        //    }
        //}

        //private void PrintToteLabel(int batchPosition)
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.PositionNumber != batchPosition) continue;
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
        //        if (printJob != null) continue;
        //        PrintTote(bp.PositionNumber, order);
        //        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
        //        _repoPrintJob.Insert(printJob);
        //    }
        //}

        //private void PrintTote(int positionNumber, ReplenOrder order)
        //{
        //    _logger.LogDetailAsync($"Printing Tote Label. {order.Ord1}"));
        //    if (_neutronVariables.EnableLabelPrinter)
        //    {
        //        ToteToPrint.Print(positionNumber, order, _labelPrinter));
        //    }
        //}



        private void MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (var bp in ordersToPick)
            {
                if (bp.OrderId == 0) continue;
                var id = bp.OrderId;
                var rec = _repoReplenOrder.FindByKey(id);
                rec.OrderStatusId = (int)NeutronCore.Enums.OrderStatus.Complete;
                _repoReplenOrder.Update(rec);
                _historyManager.SaveHistory(ActionCode.OrderComplete, rec);
            }
        }

        private void ButtonPreviousInventoryLocation_Click(object sender, EventArgs e)
        {
            GetPreviousInventoryLocation();
        }

        private void GetPreviousInventoryLocation()
        {
            if (_currentPickStop.GroupBoxLocationInventoryIndex - 1 >= 0)
            {
                var idx = _currentPickStop.GroupBoxLocationInventoryIndex - 1;
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                UpdateGroupBoxLocation(inventory);
            }
        }

        private void ButtonNextInventoryLocation_Click(object sender, EventArgs e)
        {
            GetNextInventoryLocation();
        }

        private void GetNextInventoryLocation()
        {
            if (_currentPickStop.GroupBoxLocationInventoryIndex + 1 < _currentPickStop.Inventory.Count)
            {
                var idx = _currentPickStop.GroupBoxLocationInventoryIndex + 1;
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                UpdateGroupBoxLocation(inventory);
            }


        }

        private async void MBPickChangeQuantity_Click(object sender, EventArgs e)
        {
            await ChangeQuantity();
        }

        private async Task ChangeQuantity()
        {
            MBPickChangeQuantity.Enabled = false;
            using (var form = new FrmChangeQuantity(_currentPickStop))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var newQty = form.NewQty;
                    var pos = form.Position;
                    await UpdateCurrentPickStopQuantities(pos, newQty);
                }
            }
            MBPickChangeQuantity.Enabled = true;
        }

        private async Task UpdateCurrentPickStopQuantities(int pos, int newQty)
        {
            var pickView = _currentPickStop.PickViews.FirstOrDefault(p => p.PickPosition == pos);
            if (pickView == null) return;
            pickView.QuantityToBePicked = newQty;
            _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
            LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
            await UpdatePickScreenAfterChangeQuantity();
        }



        private async void ButtonMove_Click(object sender, EventArgs e)
        {
            //ClearActiveDeviceIndicators();
            //ClearAllDeviceIndicators();
            //_currentPickStop.CurrentInventoryLocation =
            //    _currentPickStop.Inventory[_currentPickStop.GroupBoxLocationInventoryIndex];
            //var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            //var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            //var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            //var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
            var loc1 = TextBoxPickLoc1.Text.ParseInt();
            var loc2 = TextBoxPickLoc2.Text.ParseInt();
            var loc3 = TextBoxPickLoc3.Text.ParseInt();
            var loc4 = TextBoxPickLoc4.Text.ParseInt();

            var location = await _repoLocationRepository.FindByFirstOrDefaultAsync(r => r.Loc1 == loc1 && r.Loc2 == loc2 && r.Loc3 == loc3 && r.Loc4 == loc4);
            if (location == null)
            {
                MessageBox.Show($"Location does not exist.");
                return;
            }

            var itemDefinition = _currentPickStop.CurrentInventoryLocation.ItemDefinition;
            var inventory = await
                _repoInventory.FindByFirstOrDefaultAsync(r => r.LocationId == location.Id);
            if (inventory == null)
            {
                // we have a location with nothing in it.
                inventory = new Inventory
                {
                    LocationId = location.Id,
                    ItemDefinitionId = _currentPickStop.CurrentInventoryLocation.ItemDefinitionId,
                    Quantity = 0,
                    ReceivedDate = DateTime.Now,
                    PrimeBin = false,
                    AreaId = location.AreaId,
                    RFID = string.Empty
                };
                await _repoInventory.InsertAsync(inventory);
                _currentPickStop.CurrentInventoryLocation = inventory;
                _currentPickStop.CurrentInventoryLocation.ItemDefinition = itemDefinition;
                _currentPickStop.CurrentInventoryLocation.Location = location;
            }
            else
            {
                if (inventory.ItemDefinitionId == _currentPickStop.CurrentInventoryLocation.ItemDefinitionId)
                {
                    _currentPickStop.CurrentInventoryLocation = inventory;
                    _currentPickStop.CurrentInventoryLocation.ItemDefinition = itemDefinition;
                    _currentPickStop.CurrentInventoryLocation.Location = location;
                }
                else
                {
                    MessageBox.Show($"Location is in use by another item.");
                    return;
                }
            }

            await UpdatePickScreen();
            // UpdateCurrentDeviceIndicator();

            _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

            await UpdatePickPosition();

            UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);

            // UpdateTowerDisplay();
            if (_workstationView.AreaId == AreaEight) return;

            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
            // UpdateCurrentDeviceIndicator();
            // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
        }

        private void MBPriority_Click(object sender, EventArgs e)
        {
            var priority = 0;
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                using (var form = new FrmChangePriority())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        priority = (form.NewPriority).ParseInt();
                    }
                }

                foreach (var order in orders)
                {
                    order.Priority = priority;
                    _repoReplenOrder.Update(order);
                    _historyManager.SaveHistory(ActionCode.ChangePriority, order);
                }
            }
            if (_currentDataSet == CurrentDataSet.Available)
            {
                ShowAvailableStagingOrders();
            }
            if (_currentDataSet == CurrentDataSet.Rack)
            {
                ShowRackOrders();
            }
            if (_currentDataSet == CurrentDataSet.Replen)
            {
                ShowReplenOrders();
            }
            if (_currentDataSet == CurrentDataSet.Putaway)
            {
                ShowPutawayOrders();
            }
            //ShowAllOrders();
        }

        private void MBChangeOrderStatus_Click(object sender, EventArgs e)
        {
            //var orders = GetSelectedOrders(DataGridView1);

            //if (orders.Any())
            //{
            //    var ord = orders.FirstOrDefault();
            //    using (var form = new FrmChangeOrderStatus(ord, _historyManager))
            //    {
            //        var result = form.ShowDialog();
            //        if (result == DialogResult.OK)
            //        {

            //        }
            //    }
            //}
            //if (_currentDataSet == CurrentDataSet.Available)
            //{
            //    ShowAvailableStagingOrders();
            //}
            //if (_currentDataSet == CurrentDataSet.Rack)
            //{
            //    ShowRackOrders();
            //}
            //if (_currentDataSet == CurrentDataSet.Replen)
            //{
            //    ShowReplenOrders();
            //}
        }

        private void SetOrderStatus(ReplenOrder order, int status, ActionCode actionCode)
        {
            try
            {
                order.OrderStatusId = status;
                _repoReplenOrder.Update(order);
                _historyManager.SaveHistory(actionCode, order);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
            }
        }

        private void SetOrderDetailLineStatus(ReplenOrderDetail detail, int lineStatusId, ActionCode actionCode)
        {
            try
            {
                detail.LineStatusId = lineStatusId;
                _repoReplenOrderDetails.Update(detail);
                _historyManager.SaveHistory(actionCode, detail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
            }
        }

        //private void SetOrderDetailPickedQuantity(OrderDetail detail, int qty)
        //{
        //    try
        //    {
        //        detail.PickedQuantity = qty;
        //        repoReplenOrderDetail.Update(detail);
        //        _historyManager.SaveHistory((int)ActionCode.PickOrder, detail);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
        //    }
        //}

        private void MBJobDetails_Click(object sender, EventArgs e)
        {
            ShowJobDetails();
        }
        /// <summary>
        /// Shows the details of the job associated with the current row in the DataGridView1.
        /// </summary>
        /// <remarks>
        /// This method retrieves the ID from the "Id" cell of the current row in DataGridView1.
        /// If the ID is greater than 0, it calls the ShowOrderDetailsByOrder method with the ID as an argument.
        /// </remarks>
        private void ShowJobDetails()
        {
            var row = DataGridView1.CurrentRow;
            if (row == null || row.Index < 0) return;
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            if (id > 0)
            {
                ShowOrderDetailsByOrder(id);
            }
        }
        /// <summary>
        /// Displays the details of a specific order in the DataGridViewOrderDetails control.
        /// </summary>
        /// <param name="orderId">The ID of the order whose details are to be displayed.</param>
        /// <remarks>
        /// This method retrieves the order details from the _orderDetailsRepository using the provided orderId. 
        /// The details are then bound to the _bindingSourceOrderDetailsView and displayed in the DataGridViewOrderDetails control.
        /// The method also updates the LabelFormTitle text and the selected tab in the tabControl1 control.
        /// If the LineStatusId of the current order detail is not Complete, the MBKillLine control is enabled.
        /// </remarks>
        private void ShowOrderDetailsByOrder(int orderId)
        {
            _currentJobDetailsOrderId = orderId;
            var details = _orderDetailsRepository.GetOrderDetailsViewByOrder(orderId);
            if (!details.Any()) return;
            _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = _bindingSourceOrderDetailsView;
            GetRecordCount(_bindingSourceOrderDetailsView);
            LabelFormTitle.Text = _resourceManager.GetString($"JobDetails");
            tabControl1.SelectedTab = OrderDetails;
            MBKillLine.Enabled = ((ReplenOrderDetailsView)_bindingSourceOrderDetailsView.Current).LineStatusId !=
                                 (int)LineStatus.Complete;
        }
        /// <summary>
        /// Checks if the specified replenishment order is complete.
        /// </summary>
        /// <param name="order">The replenishment order to check.</param>
        /// <returns>Returns true if the order is complete, otherwise returns false.</returns>
        /// <remarks>
        /// This method checks each line of the order. If any line is not complete, the method returns false.
        /// If all lines are complete, the method logs the completion, updates the order status to complete,
        /// saves the history, updates the order in the repository, and returns true.
        /// </remarks>
        private bool CheckForOrderComplete(ReplenOrder order)
        {
            _logger.LogDetailAsync($"CheckForReplenOrderComplete   Ord:{order.Ord1}   Res:{order.Ord2} ").SafeFireAndForget();

            var linesNotComplete = _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return false;
            _logger.LogDetailAsync($"CheckForReplenOrderComplete Order is Complete.  Ord:{order.Ord1}   Res:{order.Ord2} ").SafeFireAndForget();
            order.OrderStatusId = (int)NeutronCore.Enums.OrderStatus.Complete;
            _historyManager.SaveHistory(ActionCode.OrderComplete, order);
            _repoReplenOrder.Update(order);
            return true;
        }

        /// <summary>
        /// Terminates the processing of the specified collection of replenishment order details.
        /// </summary>
        /// <param name="orderDetails">The collection of replenishment order details to be processed.</param>
        /// <remarks>
        /// This method iterates through each order detail in the provided collection. If the line status of an order detail is either 'Available' or 'Skipped', and the order status is 'Available', 
        /// the method sets the picked quantity to zero, changes the line status to 'Complete', and updates the order detail in the repository. 
        /// It also saves the history of the 'KillLine' action. If the order is complete after this operation, it checks for order completion.
        /// If an exception occurs during the processing, a message box is displayed with the error details.
        /// </remarks>
        /// <exception cref="System.Exception">Thrown when an error occurs during the processing of the order details.</exception>
        private void KillLine(ICollection<ReplenOrderDetail> orderDetails)
        {
            try
            {
                foreach (var orderDetail in orderDetails)
                {
                    var order = _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
                    orderDetail.ReplenOrder = order;
                    if ((orderDetail.LineStatusId == (int)LineStatus.Available || orderDetail.LineStatusId == (int)LineStatus.Skipped)
                        && order.OrderStatusId == (int)NeutronCore.Enums.OrderStatus.Available)
                    {
                        var areaId = orderDetail.AreaId;
                        orderDetail.PickedQuantity = 0;
                        orderDetail.LineStatusId = (int)LineStatus.Complete;
                        _repoReplenOrderDetails.Update(orderDetail);
                        _historyManager.SaveHistory(ActionCode.KillLine, orderDetail, areaId);
                        CheckForOrderComplete(order);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kill Line Error {Environment.NewLine}{ex.Message}", "Kill Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Handles the Click event of the MBKillLine control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method retrieves the selected order details from the DataGridViewOrderDetails control. If any order details are selected, it calls the KillLine method to terminate the processing of the selected order details.
        /// After the KillLine operation, it refreshes the display of order details by calling the ShowOrderDetailsByOrder method with the current job details order ID.
        /// </remarks>
        private void MBKillLine_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (orderDetails.Any()) KillLine(orderDetails);
            ShowOrderDetailsByOrder(_currentJobDetailsOrderId);
        }

        /// <summary>
        /// Handles the Click event of the MBChangeLineStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <remarks>
        /// This method retrieves the selected order details from the DataGridViewOrderDetails control.
        /// If any order details are selected, it opens the FrmChangeLineStatus form for each selected order detail.
        /// After the FrmChangeLineStatus form is closed, it refreshes the order details for the order associated with the selected order detail.
        /// </remarks>
        private void MBChangeLineStatus_Click(object sender, EventArgs e)
        {
            var lines = GetSelectedOrderDetails(DataGridViewOrderDetails);

            if (!lines.Any()) return;
            foreach (var line in lines)
            {
                using (var form = new FrmChangeReplenLineStatus(line))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {

                    }
                }
                ShowOrderDetailsByOrder(line.ReplenOrderId);
            }
        }

        private void MBCreateOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = "New Job";
            tabControl1.SelectedTab = NewOrder;
        }


        private void MBMainOrderManager_Click(object sender, EventArgs e)
        {
            LoadOrderManagerScreen();
        }

        private void LoadOrderManagerScreen()
        {
            Cursor.Current = Cursors.WaitCursor;
            _logger.LogDetailAsync($"Job Manager Main Screen Start").SafeFireAndForget();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.Green;
            ShowAllOrders();
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor;
            MBCompress.Visible = false;
            tabControl1.SelectedTab = OrderListing;
            Cursor.Current = Cursors.Default;
        }

        private void MBShowAvailable_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Available;
            _activeGrid = "Available";
            ShowAvailableStagingOrders();
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }

        private async void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _previousTab = null;
            _ordersToPick = InitOrdersToPick(_neutronVariables.StoreBatchSize);
            await AvailableOrdersScreen();
            SetFocusNextTextBoxPos();
            Cursor.Current = Cursors.Default;
        }

        public async Task AvailableOrdersScreen()
        {
            try
            {
                _logger.LogDetailAsync("Available Orders Screen START").SafeFireAndForget();
                ShowAvailableOrders();
                Cursor.Current = Cursors.WaitCursor;
                MBCompress.Visible = false;
                LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                LabelFormTitle.BackColor = Color.Green;
                await ClearBatchPositions();
                ClearOrderPositions();
                tabControl1.TabPages["AvailableOrders"].BringToFront();
                tabControl1.SelectedTab = AvailableOrders;
                await NextButtonEnabled();
                Cursor.Current = Cursors.Default;
                _logger.LogDetailAsync("Available Orders Screen END").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                _logger.LogDetailAsync($"Exception in AvailableOrdersScreen: {ex.Message}").SafeFireAndForget();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowAvailableRackScreen()
        {
            ShowAvailableOrdersRack();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = AvailableRack;
        }

        private int ShowAvailableOrdersRack(int recId = 0, string findWhat = "")
        {
            _logger.LogDetailAsync($"ShowAvailableOrdersRack Replen: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            var idx = 0;

            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrdersRack.Text.Trim().ToLower();
            }

            try
            {
                var views = _replenOrdersRepository.GetRackOrdersView(_workstationView.WorkstationId, findWhat);

                var rackOrderViews = views.ToList();
                foreach (var rackOrderView in rackOrderViews)
                {
                    rackOrderView.Lines = rackOrderView.OrderDetails.Count();
                    rackOrderView.Pieces = rackOrderView.OrderDetails.Sum(s => s.Quantity);
                    if (rackOrderView.OrderDetails.First().LineStatusId == (int)LineStatus.Picking)
                    {
                        rackOrderView.StatusName = _resourceManager.GetString($"OnFloor");
                    }
                }


                //var filteredViews = views.Where(v => v.Station_8_HasPicks != "C").ToList();

                var bindingListView = new BindingListView<RackReplenOrderView>(rackOrderViews.ToList());

                _bindingSourceAvailableOrdersRack.DataSource = bindingListView;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                       $"ShowAvailableOrdersRack Replen Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }


            DataGridViewAvailableOrdersRack.DataSource = _bindingSourceAvailableOrdersRack;
            if (GetRecordCount(_bindingSourceAvailableOrdersRack) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrdersRack, recId);
                    DataGridViewAvailableOrdersRack.FirstDisplayedScrollingRowIndex =
                        DataGridViewAvailableOrdersRack.Rows[idx].Index;
                    DataGridViewAvailableOrdersRack.CurrentCell = DataGridViewAvailableOrdersRack.Rows[idx].Cells[1];
                    DataGridViewAvailableOrdersRack.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrdersRack.ClearSelection();
                    DataGridViewAvailableOrdersRack.Update();
                }

                DataGridViewAvailableOrdersRack.Refresh();

                CurrentRackItem = ((ObjectView<RackReplenOrderView>)_bindingSourceAvailableOrdersRack.Current).Object;

            }

            _logger.LogDetailAsync($"ShowAvailableOrdersRack Replen End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return idx;
        }

        //Ready
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            InitDataGridViewNewItems();
            LabelFormTitle.Text = _resourceManager.GetString($"NewJob");
            LabelFormTitle.BackColor = Color.Green;
            ClearNewOrderForm();
            tabControl1.SelectedTab = NewOrder;
        }

        //Ready
        private void ClearNewOrderForm()
        {
            ClearNewOrderDetail();
            _bindingSourceNewItems.Clear();
            _bindingSourceItems.Clear();
            TextBoxNewOrderOrd1.Text = "";
            TextBoxNewOrderOrd2.Text = "";
            TextBoxNewOrderPriority.Text = "99";
            TextBoxNewOrderItem.Text = "";
            TextBoxNewOrderDescription.Text = "";
            TextBoxNewOrderQuantity.Text = "";
            TextBoxNewOrderOrd1.Focus();
        }

        //Ready
        private void ClearNewOrderDetail()
        {
            TextBoxNewOrderItem.Text = "";
            TextBoxNewOrderDescription.Text = "";
            TextBoxNewOrderQuantity.Text = "";
            TextBoxNewOrderFind.Text = "";
            LabelNewOrderStationNumber.Text = "";
            LabelNewOrderItemId.Text = "";
        }

        //Ready
        private List<NewItemView> GetItemsList(string s)
        {
            var recs = _itemDefinitionsRepository.GetNewItemViews(s.Trim()).ToList();
            return recs;
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }

        //private void MBBackHotPick_Click(object sender, EventArgs e)
        //{
        //    if (openHotPickFromPickScreen)
        //    {
        //        LabelFormTitle.Text = "Selection";
        //        LabelFormTitle.BackColor = Color.Green;
        //        tabControl1.SelectedTab = PickScreen;
        //        openHotPickFromPickScreen = false;
        //    }
        //    else
        //    {
        //        LabelFormTitle.Text = "Jobs";
        //        LabelFormTitle.BackColor = Color.Green;
        //        tabControl1.SelectedTab = Main;
        //    }
        //}

        private void MBNewOrderClose_Click(object sender, EventArgs e)
        {
            //ShowAllOrders();
            //LabelFormTitle.Text = "Job Listing";
            //LabelFormTitle.BackColor = Color.Green;
            //tabControl1.SelectedTab = OrderListing;
            tabControl1.SelectedTab = Main;
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            if (_previousTab == null)
            {
                if (_currentDataSet == CurrentDataSet.Available)
                {
                    ShowAvailableStagingOrders();
                    // ShowAllOrders();
                }
                if (_currentDataSet == CurrentDataSet.Rack)
                {
                    ShowRackOrders();
                }
                if (_currentDataSet == CurrentDataSet.Complete)
                {
                    ShowCompletedOrders();
                }
                if (_currentDataSet == CurrentDataSet.Replen)
                {
                    ShowReplenOrders();
                }
                if (_currentDataSet == CurrentDataSet.Putaway)
                {
                    ShowPutawayOrders();
                }
                LabelFormTitle.Text = "Job Listing";
                LabelFormTitle.BackColor = Color.Green;
            }

            // DataGridViewAvailableOrders.Refresh();

            tabControl1.SelectedTab = _previousTab == null ? OrderListing : _previousTab;

        }

        private async void MBLocationCount_Click(object sender, EventArgs e)
        {
            var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            var qty = await OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                TextBoxLocationQuantity.Text = qty.ToString();
                _currentPickStop.CurrentInventoryLocation.Quantity = qty;
                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                TextBoxTotalQuantity.Text = total.ToString();
                LoadInventory();
            }
        }


        private async Task<int> OpenLocationCountForm(int inventoryId)
        {
            var qty = -1;
            using (FrmLocationCount form = new FrmLocationCount())
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    qty = (form.NewQty).ParseInt();
                    await LocationCount(inventoryId, qty);
                }
            }

            return qty;
        }

        private async Task LocationCount(int inventoryId, int qty)
        {
            var inv =  _repoInventory.FindByKey(inventoryId);
            if (inv == null) return;
            await _historyManager.SaveHistoryAsync(ActionCode.InventoryModify, inv, inv.Quantity, true);

            var prevQty = inv.Quantity;
            inv.Quantity = qty;
            await _repoInventory.UpdateAsync(inv);
            await _historyManager.SaveHistoryAsync(ActionCode.InventoryModify, inv, prevQty, true);

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
            await _historyManager.SaveHistoryAsync(ActionCode.LocationCount, cnt);
        }

        //private void SetCurrentInventoryView(int inventoryId)
        //{
        //    InventoryView rec = _bindingSourceHot.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
        //    int pos = _bindingSourceHot.IndexOf(rec);
        //    _bindingSourceHot.Position = pos;
        //    currentInventoryView = (SqlInventoryView)_bindingSourceHot.Current;
        //}

        private void MBCompleted_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowCompletedOrders();
            Cursor.Current = Cursors.Default;
        }

        private void ShowCompletedOrders()
        {
            _activeGrid = "Complete";
            MBDeleteOrder.Visible = false;
            _currentDataSet = CurrentDataSet.Complete;
            ShowCompleted();
            MBCompress.Visible = false; // _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor;
        }

        private void CompressOrders()
        {
            var orders = GetSelectedOrders(DataGridView1);
            const string orderType = "REPLEN";
            var firstTime = true;
            var sb = new StringBuilder();
            foreach (var order in orders)
            {
                // Make sure the order is Complete before Compress
                if (order.OrderStatusId == (int)NeutronCore.Enums.OrderStatus.Complete)
                {
                    if (firstTime)
                    {
                        sb.Append(order.Id);
                        firstTime = false;
                    }
                    else
                    {
                        sb.Append("," + order.Id);
                    }
                }
            }
            var orderIds = sb.ToString();

            using (var context = new NeutronDb())
            {
                var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                var paramOrderType = new SqlParameter("@ORDERTYPE", orderType);
                var parameters = new object[] { paramOrderIds, paramOrderType };

                context.Database.ExecuteSqlCommand("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds, paramOrderType);
            }
            ShowCompletedOrders();
        }

        private void SelectAll()
        {
            try
            {
                foreach (DataGridViewRow row in DataGridView1.Rows)
                {
                    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                    chk.Value = chk.TrueValue;
                    if (row.IsNewRow)
                    {
                        chk.Value = chk.FalseValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                       $"Select All Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }
        }


        private void ShowCompleted(int recId = 0)
        {
            _logger.LogDetailAsync(
                   $"ShowCompletedOrders Replen Start: [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]").SafeFireAndForget();
            var idx = 0;

            var searchField = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "6";

            //var views = _replenOrdersRepository.GetReplenOrderViews(orderStatus, searchField);
            var views = _replenOrdersRepository.GetCompletedReplenOrderViews(orderStatus, searchField);

            //var views = _replenOrdersRepository.GetCompletedOrders(findWhat);
            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceCompleted.DataSource = bindingListView;

            //GetRecordCount(_bindingSourceCompleted);
            DataGridView1.DataSource = _bindingSourceCompleted;
            DataGridView1.ClearSelection();

            if (GetRecordCount(_bindingSourceCompleted) > 0)
            {
                MBCompress.Enabled = true;
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceCompleted, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }

                DataGridView1.Refresh();

                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceCompleted[recId]).Object;

            }

            _logger.LogDetailAsync($"ShowCompletedOrders End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return;

        }

        private void ShowButtons()
        {
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBDeleteOrder.Visible = _workstationView.StationTypeId == (int)NeutronCore.Enums.StationType.Supervisor;
            MBCompress.Visible = false; // _workstationView.StationTypeId == (int)NeutronCore.Enums.StationType.Supervisor;
        }

        private void HideButtons()
        {
            MBPriority.Visible = false;
            MBHold.Visible = false;
            MBRelease.Visible = false;
            MBDeleteOrder.Visible = _workstationView.StationTypeId == (int)NeutronCore.Enums.StationType.Supervisor;
            MBCompress.Visible = false; // _workstationView.StationTypeId == (int)NeutronCore.Enums.StationType.Supervisor;
        }

        //Ready
        private void MBNewOrderSearch_Click(object sender, EventArgs e)
        {
            var findWhat = TextBoxNewOrderFind.Text.Trim().ToLower();
            _logger.LogDetailAsync($"MBNewOrderSearch_Click  {findWhat}").SafeFireAndForget();
            FindItemRecord(findWhat);
        }

        //Ready
        private void FindItemRecord(string s)
        {
            _logger.LogDetailAsync($"FindItemRecord  {s}").SafeFireAndForget();
            try
            {
                _bindingSourceItems.DataSource = _itemDefinitionsRepository.GetNewItemViews(s.Trim()).ToList();
                _logger.LogDetailAsync($"Return from Getting Datasource Count:  {_bindingSourceItems.Count}").SafeFireAndForget();
                DataGridViewNewOrder.DataSource = _bindingSourceItems;
                _logger.LogDetailAsync($"Bind Datasource to Grid").SafeFireAndForget();
                DataGridViewNewOrder.ClearSelection();
                _logger.LogDetailAsync($"Clear and Update Grid ").SafeFireAndForget();
                DataGridViewNewOrder.Update();
                _logger.LogDetailAsync($"FindItemRecord  Complete").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        //Ready
        private void TextBoxNewOrderFind_KeyDown(object sender, KeyEventArgs e)
        {
            _logger.LogDetailAsync($"TextBoxNewOrderFind_KeyDown  {e.KeyCode}").SafeFireAndForget();
            if (e.KeyCode == Keys.Return)
            {
                FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());
            }

            if (e.KeyCode == Keys.Escape)
            {
                TextBoxNewOrderFind.Text = "";
            }
        }

        //Ready
        private void ButtonNewOrderClear_Click(object sender, EventArgs e)
        {
            TextBoxNewOrderFind.Text = string.Empty;
            TextBoxNewOrderFind.Focus();
            FindItemRecord(string.Empty);
        }

        //Ready
        private void DataGridViewNewOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            _logger.LogDetailAsync($"DataGridViewNewOrder_CellContentClick Row:  {e.RowIndex}").SafeFireAndForget();
            if (e.RowIndex >= 0)
            {
                var currentItem = (NewItemView)_bindingSourceItems.Current;
                LabelNewOrderItemId.Text = currentItem.ItemDefinitionId.ToString();
                LabelNewOrderStationNumber.Text = currentItem.AreaId.ToString();
                TextBoxNewOrderItem.Text = currentItem.Item;
                TextBoxNewOrderDescription.Text = currentItem.Description;
                TextBoxNewOrderQuantity.Focus();
            }
        }

        //Ready
        private void InitDataGridViewNewItems()
        {
            DataGridViewNewItems.DataSource = _bindingSourceNewItems;
        }

        //Ready
        private void ButtonAddDetail_Click(object sender, EventArgs e)
        {
            AddDetail();
        }

        //Ready
        private void AddDetail()
        {
            var rec = new NewItemView()
            {
                ItemDefinitionId = LabelNewOrderItemId.Text.ParseInt(),
                AreaId = LabelNewOrderStationNumber.Text.ParseInt(),
                Item = TextBoxNewOrderItem.Text,
                Description = TextBoxNewOrderDescription.Text,
                Quantity = TextBoxNewOrderQuantity.Text.ParseInt()
            };
            _bindingSourceNewItems.Add(rec);
            ClearNewOrderDetail();
            ButtonAddDetail.Enabled = false;
            TextBoxNewOrderFind.Focus();
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0;
        }

        //Ready
        private void MBNewOrderSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxNewOrderOrd1.Text) && _bindingSourceNewItems.Count > 0)
            {
                var orderDetails = new List<ReplenOrderDetail>();

                var order = new ReplenOrder()
                {
                    Ord1 = TextBoxNewOrderOrd1.Text,
                    Ord2 = TextBoxNewOrderOrd2.Text,
                    Priority = TextBoxNewOrderPriority.Text.ParseInt(),
                    LoadDate = DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = (int)NeutronCore.Enums.OrderStatus.Available
                };
                _repoReplenOrder.Insert(order);

                foreach (DataGridViewRow row in DataGridViewNewItems.Rows)
                {
                    NewItemView view = row.DataBoundItem as NewItemView;
                    if (view == null) continue;
                    var itemDefinitionId = view.ItemDefinitionId;
                    var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
                    if (itemDefinition == null) continue;
                    var rec = new ReplenOrderDetail()
                    {
                        ItemDefinitionId = itemDefinitionId,
                        ReplenOrderId = order.Id,
                        Quantity = view.Quantity,
                        AreaId = view.AreaId,
                        LineStatusId = (int)NeutronCore.Enums.OrderStatus.Available,
                        DateTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                        EmpId = GlobalVar.User.EmpId,
                        JobNum = TextBoxNewOrderOrd1.Text,
                        PartNum = itemDefinition.Item,
                        PartDesc = itemDefinition.Description,
                        Qty = view.Quantity.ToString(),
                        OrderDetailInfo = string.Empty,
                        TroubleBit = "0"
                    };

                    _repoReplenOrderDetails.Insert(rec);
                }

                TextBoxNewOrderOrd1.Text = string.Empty;
                TextBoxNewOrderOrd2.Text = string.Empty;
                _bindingSourceNewItems.Clear();
                DataGridViewNewItems.Update();
                ClearNewOrderForm();
            }
        }


        private void MBPickRefresh_Click(object sender, EventArgs e)
        {
            //ShowAllOrders();
            if (_currentDataSet == CurrentDataSet.Available)
            {
                ShowAvailableStagingOrders();
            }
            if (_currentDataSet == CurrentDataSet.Rack)
            {
                ShowRackOrders();
            }
            if (_currentDataSet == CurrentDataSet.Replen)
            {
                ShowReplenOrders();
            }
            if (_currentDataSet == CurrentDataSet.Putaway)
            {
                ShowPutawayOrders();
            }
        }


        private void ButtonImageNext_Click(object sender, EventArgs e)
        {

        }

        private void ButtonImagePrevious_Click(object sender, EventArgs e)
        {

        }

        private List<ReplenOrderView> GetValidOrdersFromBindingSource(string orderNumber)
        {
            IEnumerable<ReplenOrderView> list = _bindingSourceAvailableOrders.List.OfType<ReplenOrderView>();
            return list.Where(s => s.Ord1 == orderNumber).ToList();
        }

        private bool CheckForMultipleOrders(string orderNumber)
        {
            var ordersWithThisOrderNumber = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                string ord1 = (row.Cells["Ord1"].Value).ToString();
                if (orderNumber == ord1)
                {
                    ordersWithThisOrderNumber.Add(row);
                }
            }

            if (ordersWithThisOrderNumber.Count > 1)
            {
                return true;
            }

            return false;
        }

        private void TextBoxEnter(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            _currentTextBoxPos = textBox;
            _gridClickedAvailableOrders = false;
            textBox.Focus();
        }

        private void TextBoxPosLeave(object sender, EventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            string orderNumber = textBox.Text;
            int position = textBox.Tag.ToString().ParseInt();
            if (string.IsNullOrEmpty(orderNumber)) return;
            if (ValidateOrderAndPosition(position, orderNumber)) return;
            textBox.SelectAll();
            textBox.Focus();
        }

        private bool ValidateOrderAndPosition(int position, string orderNumber)
        {

            var orders = GetValidOrdersFromBindingSource(orderNumber);
            //ordersToPick 
            if (orders == null) return false;
            var rowsWithThisOrderNumber = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var ord1 = (row.Cells["Ord1"].Value).ToString();
                var ord2 = (row.Cells["Ord2"].Value).ToString();
                if (orderNumber.Trim() == ord1.Trim() || orderNumber.Trim() == ord2.Trim())
                {
                    rowsWithThisOrderNumber.Add(row);
                }
            }

            foreach (var row in rowsWithThisOrderNumber)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                var idValue = row.Cells["Id"].Value.ToString().ParseInt();
                var ord1 = row.Cells["Ord1"].Value.ToString();
                var ord2 = row.Cells["Ord2"].Value.ToString();
                var bp = _ordersToPick.FirstOrDefault(r => r.OrderId == idValue);

                if (bp != null) continue;
                //not in a Batch POsition
                //position is good
                chk.Value = chk.TrueValue;
                var idx = AddItemToBatch(idValue, ord1, ord2);
                return true;
            }

            return false;
        }

        private void TextBoxPosKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send(keys: "{Tab}");
            }
            e.Handled = true;
        }



        #region Find Functions Available Orders Screen


        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ShowAvailableOrders();
            }

            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindAvailableOrders.Text = "";
            }
        }

        private void MBSearchAvailableOrders_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void ButtonClearFindAvailableOrders_Click(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            TextBoxFindAvailableOrders.Focus();
        }

        #endregion

        #region Find Functions Main Orders

        private void MButtonSearch_Click(object sender, EventArgs e)
        {
            //ShowOrders();
            SearchDataSet();
        }

        /// <summary>
        /// Searches the current dataset based on the value of _currentDataSet.
        /// </summary>
        /// <remarks>
        /// This method changes the cursor to a wait cursor, performs the search operation based on the current dataset, 
        /// logs any exceptions that occur during the search, and finally resets the cursor to the default cursor.
        /// The search operation can be one of the following: ShowAvailableStagingOrders, ShowCompleted, ShowRackOrders, ShowReplenOrders.
        /// </remarks>
        private void SearchDataSet()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                switch (_currentDataSet)
                {
                    case CurrentDataSet.Available:
                        {
                            ShowAvailableOrders();
                            break;
                        }
                    case CurrentDataSet.Complete:
                        {
                            ShowCompleted();
                            break;
                        }
                    case CurrentDataSet.Rack:
                        {
                            ShowRackOrders();
                            break;
                        }
                    case CurrentDataSet.Replen:
                        {
                            ShowReplenOrders();
                            break;
                        }
                    default:
                        {
                            ShowAvailableStagingOrders();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                    $"An error occurred while searching the dataset.{Environment.NewLine}{ex.Message}").SafeFireAndForget();
                MessageBox.Show("", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowOrders()
        {
            switch (_activeGrid)
            {
                case "Available":
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _activeGrid = "Available";
                        _currentDataSet = CurrentDataSet.Available;
                        ShowAllOrders();
                        Cursor.Current = Cursors.Default;
                        break;
                    }
                case "Complete":
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _activeGrid = "Complete";
                        _currentDataSet = CurrentDataSet.Complete;
                        ShowCompleted();
                        MBCompress.Enabled = true;
                        Cursor.Current = Cursors.Default;
                        break;
                    }
                    //case "Rack":
                    //{
                    //    Cursor.Current = Cursors.WaitCursor;
                    //    DataGridView1.Columns.Clear();
                    //    _currentDataSet = CurrentDataSet.Rack;
                    //    //  SetupOrderGrid();
                    //    ShowRackOrders();
                    //    Cursor.Current = Cursors.Default;
                    //    break;
                    //}
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                ShowOrders();
            }

            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            ShowOrders();
            TextBoxFind.Focus();
        }


        #endregion

        private void MBAvailableOrdersRefresh_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void MBRefresh_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
        }

        private void ShowOrderDetails(int orderId)
        {
            List<ReplenOrderDetailsView> details = _orderDetailsRepository.GetOrderDetailsViewByOrder(orderId).ToList();

            _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = _bindingSourceOrderDetailsView;
            DataGridViewOrderDetails.ClearSelection();
        }

        //private int GetTrayNumber(string primeBin)
        // {
        //     int result = 99;
        //     if (_neutronLicense.CompanyCode == "TOP")
        //     {
        //         result = int.Parse(primeBin.Substring(2, 2));
        //     }
        //     return result;
        // }

        //private List<int> GetCheckedOrderDetailIds()
        //{

        //    var orderDetailIds = new List<int>();
        //    foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
        //    {
        //        if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
        //        {
        //            orderDetailIds.Add((int)row.Cells["Id"].Value);
        //        }
        //    }

        //    if (orderDetailIds.Count() == 0)
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderDetailIds;
        //}

        //private List<ReplenOrderDetail> GetCheckedOrderDetails()
        //{
        //    var orderDetails = new List<ReplenOrderDetail>();
        //    var ids = GetCheckedOrderDetailIds(DataGridViewOrderDetails);
        //    foreach (var id in ids)
        //    {
        //        var orderDetail = _repoReplenOrderDetails.FindByKey(id);
        //        if (orderDetail != null)
        //        {
        //            orderDetails.Add(orderDetail);
        //        }
        //    }

        //    if (!orderDetails.Any())
        //    {
        //        MessageBox.Show(text: "No Detail Lines Selected.");
        //    }

        //    return orderDetails;
        //}



        private void DataGridViewOrderDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex >= 0)
            //{
            //    var chk = (DataGridViewCheckBoxCell)DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0];
            //    if (chk.Value == chk.TrueValue)
            //    {
            //        DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
            //        //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);
            //    }
            //    else
            //    {
            //        DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
            //        //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);
            //    }
            //}
        }

        private void MBReleaseDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (!orderDetails.Any()) return;
            var orderId = orderDetails.First().ReplenOrderId;
            foreach (var orderDetail in orderDetails)
            {
                if (orderDetail.LineStatusId == (int)NeutronCore.Enums.LineStatus.Hold)
                {
                    orderDetail.LineStatusId = (int)NeutronCore.Enums.LineStatus.Available;
                    _repoReplenOrderDetails.Update(orderDetail);
                    orderDetail.ReplenOrder = _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
                    _historyManager.SaveHistory(ActionCode.ReleaseLine, orderDetail);
                }
            }

            ShowOrderDetails(orderId);
        }

        private void MBHoldDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (!orderDetails.Any()) return;
            var orderId = orderDetails.First().ReplenOrderId;

            foreach (var orderDetail in orderDetails)
            {

                if (orderDetail.LineStatusId == (int)LineStatus.Available)
                {
                    orderDetail.LineStatusId = (int)LineStatus.Hold;
                    _repoReplenOrderDetails.Update(orderDetail);

                    orderDetail.ReplenOrder = _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
                    _historyManager.SaveHistory(ActionCode.HoldLine, orderDetail);
                }
            }
            ShowOrderDetails(orderId);
        }

        private async void MBPickScreenHotPick_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                var item = LabelPickItemNumber.Text;
                Hide();
                //using (var frm = DI.Create<FrmHotAction>(
                //           _neutronVariables
                //           , _neutronLicense
                //           , _workstationView
                //           , _historyManager))
                ////, 1
                ////, null))
                using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                           , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                           , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _inventoryUnitOfWork, _iptiDisplayFunctions, _inventoryRepository, item))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                    _deviceManager.Reset();
                    _logger.LogDetailAsync($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
                }
                LoadInventory();
                await UpdateInventoryAfterHotAction();
                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();

                //_openHotPickFromPickScreen = true;
                //LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
                //LabelFormTitle.BackColor = Color.Green;
                //TextBoxFindItem.Text = LabelPickItemNumber.Text;
                //tabControl1.SelectedTab = HotPick;
            }

            //openHotPickFromPickScreen = true;
            //LabelFormTitle.Text = "Hot Search";
            //LabelFormTitle.BackColor = Color.Green;
            //TextBoxFindItem.Text = LabelPickItemNumber.Text;
            //tabControl1.SelectedTab = HotPick;
        }

        private async Task UpdateInventoryAfterHotAction()
        {
            // var success = false;
            var pickView = _currentPickStop.PickViews.FirstOrDefault();
            if (pickView != null)
            {
                var exactInventorySequence = _neutronVariables.UsePrimeBin ? await PrimeBinFirst(pickView) : Fifo(pickView);

                var neededLocations = 2 - exactInventorySequence.Count;
                if (neededLocations > 0)
                {
                    var additionInventoryLocations = await
                        GetNewInventoryLocations(pickView.OrderDetail.ItemDefinition, neededLocations);
                    exactInventorySequence.AddRange(additionInventoryLocations);
                }

                if (exactInventorySequence.Count > 0)
                {
                    pickView.CurrentInventoryLocation = exactInventorySequence.First();
                    pickView.Inventory = exactInventorySequence;
                    pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    pickView.Slot = exactInventorySequence.First().Location.Slot;
                    pickView.SlotQty = exactInventorySequence.First().Quantity;
                    pickView.InventoryIndex = 0;
                    pickView.ReceivedDate = exactInventorySequence.First().ReceivedDate;
                }
                var firstPickView = _currentPickStop.PickViews.FirstOrDefault();
                if (firstPickView != null)
                {
                    _currentPickStop.CurrentInventoryLocation = firstPickView.CurrentInventoryLocation;
                    _currentPickStop.Description = firstPickView.Description;
                    _currentPickStop.Images = firstPickView.Images;
                    _currentPickStop.Inventory = firstPickView.Inventory;
                    _currentPickStop.InventoryIndex = firstPickView.InventoryIndex;
                    _currentPickStop.Item = firstPickView.Item;
                    _currentPickStop.ItemId = firstPickView.ItemId;
                    _currentPickStop.Ord1 = firstPickView.Ord1;
                    _currentPickStop.Ord2 = firstPickView.Ord2;
                    _currentPickStop.OrderId = firstPickView.OrderId;
                    _currentPickStop.PickedQty = _currentPickStop.GetPickedSoFar();
                    _currentPickStop.Quantity = _currentPickStop.GetTotalQuantityToBePicked();
                    // _currentPickStop.QuantityToBePicked = _currentPickStop.GetQuantityToBePicked();
                    _currentPickStop.Slot = firstPickView.Slot;
                    _currentPickStop.SlotQty = firstPickView.SlotQty;
                    _currentPickStop.TotalQuantityInInventory = firstPickView.TotalQuantityInInventory;

                }


                //    switch (_neutronVariables.PickMethod)
                //    {
                //        case "RadioButtonPrimeBinFirst":
                //            exactInventorySequence = PrimeBinFirst(currentPickView);
                //            break;
                //        case "RadioButtonPrimeBinLast":
                //            exactInventorySequence = PrimeBinLast(currentPickView);
                //            break;
                //        case "RadioButtonFifo":
                //            exactInventorySequence = Fifo(currentPickView);
                //            break;
                //        case "RadioButtonLifo":
                //            exactInventorySequence = Lifo(currentPickView);
                //            break;
                //        default:
                //            exactInventorySequence = Fifo(currentPickView);
                //            break;
                //    }
                //}

                //if (exactInventorySequence.Count > 0)
                //{
                //    foreach (var pickView in _currentPickStop.PickViews)
                //    {
                //        pickView.CurrentInventoryLocation = exactInventorySequence.First();
                //        pickView.Inventory = exactInventorySequence;
                //        pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                //        pickView.Slot = exactInventorySequence.First().Location.Slot;
                //        pickView.SlotQty = pickView.TotalQuantityInInventory;
                //        pickView.InventoryIndex = 0;
                //        pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
                //        success = true;
                //    }

                //    var firstPickView = _currentPickStop.PickViews.FirstOrDefault();
                //    if (firstPickView != null)
                //    {
                //        _currentPickStop.CurrentInventoryLocation = firstPickView.CurrentInventoryLocation;
                //        _currentPickStop.Description = firstPickView.Description;
                //        _currentPickStop.Images = firstPickView.Images;
                //        _currentPickStop.Inventory = firstPickView.Inventory;
                //        _currentPickStop.InventoryIndex = firstPickView.InventoryIndex;
                //        _currentPickStop.Item = firstPickView.Item;
                //        _currentPickStop.ItemId = firstPickView.ItemId;
                //        _currentPickStop.Ord1 = firstPickView.Ord1;
                //        _currentPickStop.Ord2 = firstPickView.Ord2;
                //        _currentPickStop.OrderId = firstPickView.OrderId;
                //        _currentPickStop.PickedQty = _currentPickStop.GetPickedSoFar();
                //        _currentPickStop.Quantity = _currentPickStop.GetTotalQuantityToBePicked();
                //        // _currentPickStop.QuantityToBePicked = _currentPickStop.GetQuantityToBePicked();
                //        _currentPickStop.Slot = firstPickView.Slot;
                //        _currentPickStop.SlotQty = firstPickView.SlotQty;
                //        _currentPickStop.TotalQuantityInInventory = firstPickView.TotalQuantityInInventory;

                //    }
            }
        }

        private void MBArchive_Click(object sender, EventArgs e)
        {

        }

        private void MBPrintOrderListing_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }

        private void MbPrintAvailableOrders_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewAvailableOrders);
        }

        private void MBPrintPickList_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridPickView);
        }

        private void MBPrintOrderDetails_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewOrderDetails);
        }

        //private void MBBackHotStore_Click(object sender, EventArgs e)
        //{
        //    if (openHotStoreFromPickScreen)
        //    {
        //        LabelFormTitle.Text = "Selection";
        //        LabelFormTitle.BackColor = Color.Green;
        //        tabControl1.SelectedTab = PickScreen;
        //        openHotStoreFromPickScreen = false;
        //    }
        //    else
        //    {
        //        LabelFormTitle.Text = "Jobs";
        //        LabelFormTitle.BackColor = Color.Green;
        //        tabControl1.SelectedTab = Main;
        //    }
        //}

        private void TextBoxPos_Click(object sender, EventArgs e)
        {
            //if you click directly in a textboxpos, you override the
            //automatic get of the next empty textbox
            //to let the automatic process know to use the manually
            //clicked textbox, set the flag to true
            // unset the flag after the automatic check runs
            _currentTextBoxPos = sender as TextBox;
            if (_currentTextBoxPos == null) return;
            // ClearTextBoxPosBackColor();
            // _currentTextBoxPos.BackColor = Color.Yellow;
            _currentTextBoxPos.SelectAll();
            // Let the world know that the TextBox was directly clicked in focus
            ManualOverrideCurrentTextBoxPos = true;
            _currentTextBoxPos.FocusAndHighlightText();
        }

        private void ClearTextBoxPosBackColor()
        {
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"TextBoxPos{pos}", true).First();
                if (c != null) c.BackColor = Color.White;
            }
        }

        private async void MBFill_Click(object sender, EventArgs e)
        {
            var emptyBatchPositions = _ordersToPick.Where(r => r.OrderId == 0).ToList();
            var count = 0;
            var totalRows = DataGridViewAvailableOrders.Rows.Count;
            if (totalRows <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                ManualOverrideCurrentTextBoxPos = false;
                var id = Convert.ToInt32(row.Cells["Id"].Value);
                var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
                count++;
                if (count == emptyBatchPositions.Count)
                {
                    break;
                }

                if (idx == -1)
                {
                    //no more locations
                    break;
                }
                SetFocusNextTextBoxPos();

            }

            //var totalRows = DataGridViewAvailableOrders.Rows.Count;
            //if (totalRows <= 0) return;
            //foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            //{


            //    //if (row.Selected) continue;
            //    var id = Convert.ToInt32(row.Cells["Id"].Value);
            //    var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
            //    var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
            //    // var idx = AddItemToBatch(id, ord1, ord2);
            //    var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
            //    if (idx == -1)
            //    {
            //        //no more locations
            //        break;
            //    }
            //}

            //if (DataGridViewAvailableOrders.Rows.Count <= 0) return;
            //foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            //{
            //    var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];

            //    if (Convert.ToBoolean(checkBoxCell.Value) != false) continue;
            //    var id = Convert.ToInt32(row.Cells["Id"].Value);
            //    var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
            //    var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
            //    var idx = AddItemToBatch(id, ord1, ord2);
            //    if (idx == -1)
            //    {
            //        //no more locations
            //        break;
            //    }
            //    else
            //    {
            //        row.Cells[0].Value = checkBoxCell.TrueValue;
            //    }
            //}
        }

        private void FrmReplen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                // MessageBox.Show("Launch Find Box in PICK");
                var rfidManager = DI.Create<IRFIDManager>();
                //using (MetroForm frm = new FrmInventory(_jsonData, _akaRepository, _lacProcessor
                //           , _workstationRepository, _workstationView, _neutronVariables, _historyManager
                //           , _areaRepository, rfidManager, _locationsRepository))
                using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables, _iptiDisplayFunctions))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F2)
            {
                _logger.LogDetailAsync($"FrmReplen_KeyDown: F2 Key").SafeFireAndForget();
                PrintLabels(_currentPickStop, 2);
                e.Handled = true;
            }
        }

        private void PrintLabels(ReplenPickStop currentPickStop, int reqFunc = 1, int pos = 0)
        {
            foreach (var pickView in currentPickStop.PickViews)
            {
                var upc = _repoAka.GetUpc(pickView.Item);
                var labelDetail = GetLabelDetail(pickView.OrderDetail);
                ToteToPrint.Print(reqFunc, pickView.PickPosition, labelDetail, upc, _labelPrinter);
            }
        }

        private void PrintLabel(int reqFunc, int pos, ReplenPickView pickview)
        {
            var upc = _repoAka.GetUpc(pickview.Item);
            var labelDetail = GetLabelDetail(pickview.OrderDetail);
            ToteToPrint.Print(reqFunc, pos, labelDetail, upc, _labelPrinter);
        }

        private LabelDetail GetLabelDetail(ReplenOrderDetail orderDetail)
        {
            return new LabelDetail
            {
                Item = orderDetail.ItemDefinition.Item,
                Description = orderDetail.ItemDefinition.Description,
                Quantity = orderDetail.Quantity,
                EmpId = GlobalVar.User.EmpId,
                Invoice = orderDetail.ReplenOrder.Ord2,
                Order = orderDetail.ReplenOrder.Ord1,
                LoadDate = orderDetail.ReplenOrder.LoadDate,
                Origin = orderDetail.OrderDetailInfo.Trim(),
                UnitOfIssue = orderDetail.ItemDefinition.UnitOfIssue.Name
            };
        }

        private void MBRackBack_Click(object sender, EventArgs e)
        {
            //LabelFormTitle.Text = "Jobs";
            //LabelFormTitle.BackColor = Color.Green;
            //tabControl1.SelectedTab = OrderListing;
            tabControl1.SelectedTab = Main;
        }

        private void MBPrintDocument_Click(object sender, EventArgs e)
        {

        }

        private void MBPrintToteLabel_Click(object sender, EventArgs e)
        {

        }

        private void MBRackOrderComplete_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);

            if (!orders.Any()) return;
            foreach (var order in orders)
            {
                var detailLinesThisStation = _repoReplenOrderDetails
                    .FindBy(r => r.ReplenOrderId == order.Id && r.AreaId == _workstationView.AreaId).ToList();
                if (detailLinesThisStation.Count > 0)
                {
                    foreach (var orderDetail in detailLinesThisStation)
                    {
                        orderDetail.LineStatusId = (int)LineStatus.Complete;
                        orderDetail.PickedQuantity = orderDetail.Quantity;
                        orderDetail.EmpId = GlobalVar.User.EmpId;
                        _historyManager.SaveHistory(ActionCode.PickRack, orderDetail);
                        _repoReplenOrderDetails.Update(orderDetail);
                    }
                }

                CheckForReplenOrderComplete(order);
            }

            //var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
            //uploadProcessor.CreateHostFileRack(orders);

            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();

        }

        private void CheckForReplenOrderComplete(ReplenOrder order)
        {
            var linesNotComplete = _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == order.Id)
                .Where(r => r.LineStatusId != (int)LineStatus.Complete).ToList();
            if (linesNotComplete.Count != 0) return;

            order.OrderStatusId = (int)NeutronCore.Enums.OrderStatus.Complete;
            _historyManager.SaveHistory(ActionCode.OrderComplete, order: order);
            _repoReplenOrder.Update(order);
        }

        private void MBRefreshRack_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        //private void MbPrintAvailableOrdersRack_Click(object sender, EventArgs e)
        //{
        //    CsvUtility.SaveToCsv(DataGridViewAvailableOrdersRack);
        //    TextBoxFindAvailableOrdersRack.Focus();
        //}

        //private void MBPrintDocumentAndToteLabel_Click(object sender, EventArgs e)
        //{

        //}

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrdersRack.Focus();
        }

        //private void MBPrint_Click(object sender, EventArgs e)
        //{
        //    var position = _currentPickStop.PickViews.First().PickPosition;

        //    using (FrmReprint form = new FrmReprint(_neutronVariables, position))
        //    {
        //        DialogResult result = form.ShowDialog();
        //        if (result == DialogResult.OK)
        //        {
        //            if (form.printData.PrintDocument)
        //            {
        //                ReprintDocument(form.printData.Position);
        //            }

        //            if (form.printData.PrintToteLabel)
        //            {
        //                ReprintToteLabel(form.printData.Position);
        //            }
        //        }
        //    }
        //}

        //private void ReprintToteLabel(int batchPosition)
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.PositionNumber != batchPosition) continue;
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
        //        if (printJob == null)
        //        {
        //            PrintTote(bp.PositionNumber, order);
        //            printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
        //            _repoPrintJob.Insert(printJob);
        //        }
        //        else
        //        {
        //            PrintTote(bp.PositionNumber, order);
        //            printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
        //            _repoPrintJob.Update(printJob);
        //        }
        //    }
        //}

        //private void ReprintDocument(int batchPosition)
        //{
        //    foreach (var bp in _ordersToPick)
        //    {
        //        if (bp.PositionNumber != batchPosition) continue;
        //        if (bp.OrderId == 0) continue;
        //        var id = bp.OrderId;
        //        var order = _repoReplenOrder.FindByKey(id);
        //        var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
        //        if (printJob == null)
        //        {
        //            PrintDoc(bp.PositionNumber, order);
        //            printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
        //            _repoPrintJob.Insert(printJob);
        //        }
        //        else
        //        {
        //            PrintDoc(bp.PositionNumber, order);
        //            printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
        //            _repoPrintJob.Update(printJob);
        //        }
        //    }
        //}

        private void TextBoxFindAvailableOrdersRack_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
                {
                    if (row.Cells["Ord1"].Value.ToString().Trim() == TextBoxFindAvailableOrdersRack.Text)
                    {
                        var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                        row.Cells[0].Value = chk.TrueValue;
                        TextBoxFindAvailableOrdersRack.Text = string.Empty;
                        TextBoxFindAvailableOrdersRack.Select();
                        break;
                    }
                }
            }
        }

        private void PictureBoxItemImage_MouseEnter(object sender, EventArgs e)
        {
            if (!_neutronVariables.UseImages) return;
            if (!_neutronVariables.AutoEnlargeImage) return;
            PictureBoxItemImage.Location = new Point(138, 151);
            PictureBoxItemImage.Size = new Size(512, 512);
            PictureBoxItemImage.BringToFront();
        }

        private void PictureBoxItemImage_MouseLeave(object sender, EventArgs e)
        {
            if (!_neutronVariables.UseImages) return;
            if (!_neutronVariables.AutoEnlargeImage) return;
            PictureBoxItemImage.Location = new Point(422, 464);
            PictureBoxItemImage.Size = new Size(228, 199);
            PictureBoxItemImage.BringToFront();
        }

        private void MBDeleteOrder_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                var result = MessageBox.Show("Are you sure you want to delete these records?", "Delete Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                {
                    foreach (var order in orders)
                    {
                        //var ord = _repoReplenOrder.AllInclude(s => s.ReplenOrderDetails)
                        //    .FirstOrDefault(r => r.Id == order);

                        if (order == null) continue;
                        foreach (var orderDetail in order.ReplenOrderDetails)
                        {
                            _repoReplenOrderDetails.Delete(orderDetail.Id);
                            _historyManager.SaveHistory(ActionCode.ReplenDetailDelete, orderDetail);
                        }

                        _repoReplenOrder.Delete(order.Id);
                        _historyManager.SaveHistory(ActionCode.ReplenOrderDelete, order);
                    }
                }
            }

            ShowAllOrders();
        }

        private void ButtonRemoveLine_Click(object sender, EventArgs e)
        {
            _bindingSourceNewItems.RemoveCurrent();
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0 && ((NewItemView)_bindingSourceNewItems.Current).Item != null;
            CreateJobButtonEnable();
        }


        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmReplen",
                    resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders",
                    resourceDir: languageDirectory, usingResourceSet: null);
                //Main Panel
                LabelFormHeaderText.Text = _resourceManager.GetString($"LabelFormHeaderText");
                LabelFormTitle.Text = _resourceManager.GetString($"LabelFormTitle");
                MBMainClose.Text = _resourceManager.GetString($"MBMainClose");
                MBMainAvailableOrders.Text = _resourceManager.GetString($"MBMainAvailableOrders");
                MBMainOrderManager.Text = _resourceManager.GetString($"MBMainOrderManager");
                MBMainNewOrder.Text = _resourceManager.GetString($"MBMainNewOrder");
                //MBMainLoadOrders.Text = _resourceManager.GetString($"MBMainLoadOrders");
                //MBMainUpload.Text = _resourceManager.GetString($"MBMainUpload");
                //Order Listing Panel

                //MBShowAvailable.Text = _resourceManager.GetString($"MBShowAvailable");
                MBCompleted.Text = _resourceManager.GetString($"MBCompleted");
                MBShowReplenOrders.Text = _resourceManager.GetString($"MBShowReplenOrders");
                LabelFindDescription.Text = _resourceManager.GetString($"LabelFindDescription");
                MButtonSearch.Text = _resourceManager.GetString($"MButtonSearch");
                MButtonClose.Text = _resourceManager.GetString($"MButtonClose");
                //ButtonPrintAO.Text = _resourceManager.GetString($"ButtonPrintAO");
                //ButtonPrintPacking.Text = _resourceManager.GetString($"ButtonPrintPacking");
                //MBPrintPick.Text = _resourceManager.GetString($"MBPrintPick");
                //MBOffCarousel.Text = _resourceManager.GetString($"MBOffCarousel");
                MBHold.Text = _resourceManager.GetString($"MBHold");
                MBRelease.Text = _resourceManager.GetString($"MBRelease");
                MBPriority.Text = _resourceManager.GetString($"MBPriority");
                MBCompress.Text = _resourceManager.GetString($"MBCompress");
                //MBReturnToStock.Text = _resourceManager.GetString($"MBReturnToStock");
                MBDeleteOrder.Text = _resourceManager.GetString($"MBDeleteOrder");
                MBJobDetails.Text = _resourceManager.GetString($"MBJobDetails");
                MBPrintOrderListing.Text = _resourceManager.GetString($"MBPrintOrderListing");
                //MBFillOptimized.Text = _resourceManager.GetString("MBFillOptimized");
                //Available Orders
                MBGo2.Text = _resourceManager.GetString($"MBGo");
                MBAvailableOrdersRefresh.Text = _resourceManager.GetString($"MBAvailableOrdersRefresh");
                MBGo.Text = _resourceManager.GetString($"MBGo");
                LabelAvailableOrdersSearchFor.Text = _resourceManager.GetString($"LabelAvailableOrdersSearchFor");
                MBSearchAvailableOrders.Text = _resourceManager.GetString($"MBSearchAvailableOrders");
                MBAvailableOrdersBack.Text = _resourceManager.GetString($"MBAvailableOrdersBack");
                MBFill.Text = _resourceManager.GetString($"MBFill");

                //Pick List
                MBPrintPickList.Text = _resourceManager.GetString($"MBPrintPickList");
                MBStart.Text = _resourceManager.GetString($"MBStart");
                MBPickListBack.Text = _resourceManager.GetString($"MBPickListBack");

                //Pick Screen
                MBLocationCount.Text = _resourceManager.GetString($"MBLocationCount");
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"MBShowOrderOrQuantityToggle");
                MBPickScreenHotPick.Text = _resourceManager.GetString($"MBPickScreenHotPick");
                //MBResetCarousels.Text = _resourceManager.GetString($"MBResetCarousels");
                MBPrint.Text = _resourceManager.GetString($"MBPrint");
                MBPickBack.Text = _resourceManager.GetString($"MBPickBack");
                //LabelItem.Text = _resourceManager.GetString($"LabelItem");
                //LabelUOI.Text = _resourceManager.GetString($"LabelUOI");
                //LabelQty.Text = _resourceManager.GetString($"LabelQty");
                GroupBoxLocation.Text = _resourceManager.GetString($"GroupBoxLocation");
                LabelDevice.Text = _resourceManager.GetString($"LabelDevice");
                LabelTray.Text = _resourceManager.GetString($"LabelTray");
                LabelOver.Text = _resourceManager.GetString($"LabelOver");
                LabelBack.Text = _resourceManager.GetString($"LabelBack");
                LabelReceivedDate.Text = _resourceManager.GetString($"LabelReceivedDate");
                //LabelLocationQty.Text = _resourceManager.GetString($"LabelLocationQty");
                //LabelTotalQty.Text = _resourceManager.GetString($"LabelTotalQty");
                //LabelReqQty.Text = _resourceManager.GetString($"LabelReqQty");
                //LabelPickedSoFar.Text = _resourceManager.GetString($"LabelPickedSoFar");
                MBPickChangeQuantity.Text = _resourceManager.GetString($"MBPickChangeQuantity");
                //MBShortPick.Text = _resourceManager.GetString($"MBShortPick");
                MBStoreAccept.Text = _resourceManager.GetString($"MBPickAccept");

                //Order Details
                //MBSelectAllDetail.Text = _resourceManager.GetString($"MBSelectAllDetail");
                //MBClearSelectionDetail.Text = _resourceManager.GetString($"MBClearSelectionDetail");
                //MBReturnToStockOrderDetail.Text = _resourceManager.GetString($"MBReturnToStockOrderDetail");
                MBHoldDetail.Text = _resourceManager.GetString($"MBHoldDetail");
                MBReleaseDetail.Text = _resourceManager.GetString($"MBReleaseDetail");
                MBPrintOrderDetails.Text = _resourceManager.GetString($"MBPrintOrderDetails");
                MBOrderDetailsBack.Text = _resourceManager.GetString($"MBOrderDetailsBack");

                //New Order
                //GroupBoxOrderInformation.Text = _resourceManager.GetString($"GroupBoxOrderInformation");
                //LabelJob.Text = _resourceManager.GetString($"LabelJob");
                //LabelInvoice.Text = _resourceManager.GetString($"LabelInvoice");
                //LabelPriority.Text = _resourceManager.GetString($"LabelPriority");
                MBNewOrderSave.Text = _resourceManager.GetString($"MBNewOrderSave");
                MBNewOrderClose.Text = _resourceManager.GetString($"MBNewOrderClose");
                //GroupBoxDetailInformation.Text = _resourceManager.GetString($"GroupBoxDetailInformation");
                TextBoxNewOrderItem.Text = _resourceManager.GetString($"TextBoxNewOrderItem");
                TextBoxNewOrderDescription.Text = _resourceManager.GetString($"TextBoxNewOrderDescription");
                //LabelNewOrderQuantity.Text = _resourceManager.GetString($"LabelNewOrderQuantity");
                ButtonAddDetail.Text = _resourceManager.GetString($"ButtonAddDetail");
                //LabelSearchForItem.Text = _resourceManager.GetString($"LabelSearchForItem");
                MBNewOrderSearch.Text = _resourceManager.GetString($"MBNewOrderSearch");
                //ButtonRemoveLine.Text = _resourceManager.GetString($"ButtonRemoveLine");

                //Available Rack
                MbPrintAvailableOrdersRack.Text = _resourceManager.GetString($"MbPrintAvailableOrdersRack");
                MBRefreshRack.Text = _resourceManager.GetString($"MBRefreshRack");
                //LabelSearchForRack.Text = _resourceManager.GetString($"LabelSearchForRack");
                MBSearchAvailableOrdersRack.Text = _resourceManager.GetString($"MBSearchAvailableOrdersRack");
                //MBRackHotAction.Text = _resourceManager.GetString($"MBRackHotAction");
                MBRackBack.Text = _resourceManager.GetString($"MBRackBack");
                //MBPrintDocument.Text = _resourceManager.GetString($"MBPrintDocument");

                //MBRackOrderComplete.Text = _resourceManager.GetString($"MBRackOrderComplete");

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{_resourceManager.GetString($"ErrorLoadingLanguages")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        //private void SetCulture(string lang)
        //{
        //    try
        //    {
        //        var languageDirectory = LoaderSettings.GetLanguageDirectory();
        //        _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
        //        _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmReplen", resourceDir: languageDirectory, usingResourceSet: null);
        //        MBMainOrderManager.Text = _resourceManager.GetString($"JobManager");
        //        MBMainAvailableOrders.Text = _resourceManager.GetString($"AvailableJobs");
        //        LabelFormHeaderText.Text = _resourceManager.GetString($"NeutronWarehouseManagement");
        //        MBMainNewOrder.Text = _resourceManager.GetString($"NewJob");
        //        MBMainLoadOrders.Text = _resourceManager.GetString($"LoadJobs");
        //        MBMainClose.Text = _resourceManager.GetString($"Home");
        //        LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
        //        MBSelectAll.Text = _resourceManager.GetString($"SelectAll");
        //        MButtonClearSelection.Text = _resourceManager.GetString($"ClearSelection");
        //        MBOrderListingAvailable.Text = _resourceManager.GetString($"Available");
        //        MButtonSearch.Text = _resourceManager.GetString($"Search");
        //        MButtonClose.Text = _resourceManager.GetString($"Back");
        //        MBShowAll.Text = _resourceManager.GetString($"ShowAll");
        //        MBRefresh.Text = _resourceManager.GetString($"Refresh");
        //        MBHold.Text = _resourceManager.GetString($"Hold");
        //        MBRelease.Text = _resourceManager.GetString($"Release");
        //        MBPriority.Text = _resourceManager.GetString($"Priority");
        //        MBReturnToStock.Text = _resourceManager.GetString($"ReturnToStock");
        //        MBDeleteOrder.Text = _resourceManager.GetString($"Delete");
        //        MBPrintOrderListing.Text = _resourceManager.GetString($"SaveToFile");
        //        MBJobDetails.Text = _resourceManager.GetString($"JobDetails");
        //        LabelFindDescription.Text = _resourceManager.GetString($"SearchFor");
        //        //Available Orders
        //        MbPrintAvailableOrders.Text = _resourceManager.GetString($"SaveToFile");
        //        MBAvailableOrdersRefresh.Text = _resourceManager.GetString($"Refresh");
        //        MBGo.Text = _resourceManager.GetString($"Next");
        //        LabelAvailableOrdersSearchFor.Text = _resourceManager.GetString($"SearchFor");
        //        MBSearchAvailableOrders.Text = _resourceManager.GetString($"Search");
        //        MBFill.Text = _resourceManager.GetString($"Fill");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error loading languages.  FrmReplen  {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //    }
        //}

        private void MBShowAll_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
            MBCompleted.Text = "Completed";
        }

        private void MBCompress_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CompressOrders();
            TextBoxFind.Text = string.Empty;
            ShowOrders();
            Cursor.Current = Cursors.Default;
        }


        private void MBShowPutawayOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Putaway;
            ShowPutawayOrders();
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }

        private int ShowPutawayOrders(int recId = 0)
        {

            _logger.LogDetailAsync($"Show Putaway Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]").SafeFireAndForget();
            var idx = 0;

            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,6,7,8,9";
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            //var views = _ordersRepository.GetRackOrders(findWhat);
            //var views = _ordersRepository.GetReplenishmentOrders(findWhat);
            var views = _replenOrdersRepository.GetPutawayOrderViews(orderStatus, findWhat);



            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceOrderView.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceOrderView;

            if (GetRecordCount(_bindingSourceOrderView) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceOrderView, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }
                DataGridView1.Refresh();
                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceOrderView.Current).Object;
            }
            // ---         _logger.LogDetailAsync($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void MBShowReplenOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Replen;
            ShowReplenOrders();
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)NeutronCore.Enums.StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }
        //private void MBShowRackOrders_Click(object sender, EventArgs e)
        //{
        //    //Cursor.Current = Cursors.WaitCursor;
        //    //DataGridView1.Columns.Clear();
        //    //_currentDataSet = CurrentDataSet.Rack;
        //    ////  SetupOrderGrid();
        //    //ShowRackOrders();
        //    //Cursor.Current = Cursors.Default;
        //    _activeGrid = "Rack";
        //    ShowAvailableRackScreen();
        //}

        private int ShowReplenOrders(int recId = 0)
        {

            _logger.LogDetailAsync($"Show Replen Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]").SafeFireAndForget();
            var idx = 0;

            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,6,7,8,9";
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            //var views = _ordersRepository.GetRackOrders(findWhat);
            //var views = _ordersRepository.GetReplenishmentOrders(findWhat);
            var views = _replenOrdersRepository.GetReplenStoreOrderViews(orderStatus, findWhat);



            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceOrderView.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceOrderView;

            if (GetRecordCount(_bindingSourceOrderView) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceOrderView, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }
                DataGridView1.Refresh();
                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceOrderView.Current).Object;
            }
            // ---         _logger.LogDetailAsync($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowRackOrders(int recId = 0)
        {
            _logger.LogDetailAsync($"Show Rack Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]").SafeFireAndForget();
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            //if (!string.IsNullOrEmpty(findWhat))
            //{
            var views = _replenOrdersRepository.GetRackOrders(findWhat);
            var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            _bindingSourceOrderView.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceOrderView;
            //}
            //else
            //{
            //    var views = _replenOrdersRepository.GetRackOrders();
            //    var bindingListView = new BindingListView<ReplenOrderView>(views.ToList());
            //    _bindingSourceOrderView.DataSource = bindingListView;
            //    DataGridView1.DataSource = _bindingSourceOrderView;
            //}

            if (GetRecordCount(_bindingSourceOrderView) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceOrderView, recId);
                    DataGridView1.FirstDisplayedScrollingRowIndex = DataGridView1.Rows[idx].Index;
                }
                else
                {
                    DataGridView1.ClearSelection();
                    DataGridView1.Update();
                }

                DataGridView1.Refresh();
                CurrentItem = ((ObjectView<ReplenOrderView>)_bindingSourceOrderView.Current).Object;
            }

            _logger.LogDetailAsync($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return idx;
        }

        //Ready
        private void TextBoxNewOrderQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                AddDetail();
            }
        }

        //Ready
        private void TextBoxNewOrderQuantity_TextChanged(object sender, EventArgs e)
        {
            ButtonAddDetail.Enabled = TextBoxNewOrderQuantity.Text.ParseInt() > 0;
        }

        private void DataGridPickView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridPickView.ClearSelection();

            // we don't need formatting in Replen
            // formatting in Pick shows Inventory that's Short 
            //DataGridPickView_FormatRows();
        }

        private void DataGridPickView_FormatRows()
        {
            var grid = DataGridPickView;
            if (grid.RowCount > 0)
            {
                grid.SuspendLayout();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    row.DefaultCellStyle.BackColor =
                        row.Cells["Quantity"].Value.ToString().ParseInt() >
                        row.Cells["TotalQuantityInInventory"].Value.ToString().ParseInt()
                            ? Color.Gold
                            : Color.White;
                }

                grid.ResumeLayout();
            }
        }

        private void MBShortCut_Click(object sender, EventArgs e)
        {
            ShowShortCutForm();
        }

        private void ShowShortCutForm()
        {
            using (var form = new FrmShortCut())
            {
                var result = form.ShowDialog();
            }
        }

        private void ButtonClearFindAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrdersRack.Text = string.Empty;
            ShowAvailableOrdersRack(0, TextBoxFindAvailableOrdersRack.Text.Trim().ToLower());
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBSearchAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowAvailableOrdersRack(0, TextBoxFindAvailableOrdersRack.Text.Trim().ToLower());
            Cursor.Current = Cursors.Default;
        }

        private void MBRackHotAction_Click(object sender, EventArgs e)
        {
            //OpenHotActionForm();

            // var location = _currentPickStop.CurrentInventoryLocation.Location;
            // PositionDevice(location.Loc1, location.Loc2, location.Loc3, location.Loc4, true);
        }

        private void OpenHotActionForm(PickList pickList)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions]) return;
            Hide();
            var item = pickList.Item;
            var quantity = pickList.Ordered.ParseInt();
            //using (MetroForm frm = new FrmHotAction(workstation, _jsonData, _akaRepository
            //    , _neutronVariables, _lacProcessor, _imageManager, _itemDefinitionsRepository, _workstationRepository, item, quantity, pickList))
            //using (var frm = DI.Create<FrmHotAction>(
            //          _neutronVariables
            //          , _neutronLicense
            //          , _workstationView
            //          , _historyManager))
            ////, quantity
            ////, pickList))
            using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                       , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                       , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _inventoryUnitOfWork, _iptiDisplayFunctions, _inventoryRepository, item, quantity, pickList))
            {
                var result = frm.ShowDialog();
                Show();
                //   _deviceManager.Reset());
                //    _logger.LogDetailAsync($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
            }
        }


        private void MBPrintRackDocument_Click(object sender, EventArgs e)
        {

            PrintPickList(_workstationView);
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void PrintPickList(WorkstationView workstationView)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (!orders.Any()) return;
            foreach (var order in orders)
            {
                var orderDetails = order.ReplenOrderDetails.Where(r =>
                    r.AreaId == _workstationView.AreaId && r.LineStatusId != (int)LineStatus.Complete).ToList();

                foreach (var orderDetail in orderDetails)
                {
                    if (orderDetail == null) continue;
                    orderDetail.LineStatusId = (int)NeutronCore.Enums.LineStatus.Picking;

                    _repoReplenOrderDetails.Update(orderDetail);
                }

                order.OrderStatusId = (int)NeutronCore.Enums.LineStatus.Picking;
                _repoReplenOrder.Update(order);
                PrintPickListByStation(order.Id, workstationView);
            }
        }

        private void PrintPickListByStation(int orderId, WorkstationView workstationView)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickListByWorkstation(orderId, workstationView);
            _documentToPrint.PrintReplenList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickListByWorkstation(int orderId, WorkstationView workstationView)
        {
            var outs = new List<PickList>();

            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndAreaNotCompleted(orderId, workstationView.AreaId);
            if (!details.Any()) return outs;
            foreach (var detail in details)
            {
                //check for Inventory
                var inv = _repoInventory.FindBy(r => r.ItemDefinitionId == detail.ItemDefinitionId).OrderBy(o => o.ReceivedDate).ToList();
                if (inv.Count == 0)
                {
                    // put 1 line in for where they put it.
                    outs.Add(new PickList
                    {
                        Area = detail.Area.AreaNumber.ToString(),
                        Workstation = workstationView.WorkstationId.ToString(),
                        OrderDetailId = detail.Id.ToString(),
                        Order = detail.ReplenOrder.Ord1,
                        Invoice = detail.ReplenOrder.Ord2,
                        CostCenter = string.Empty,
                        Recipient = string.Empty,
                        Date = detail.ReplenOrder.LoadDate.ToString("MM/dd/yyyy"),
                        Time = detail.ReplenOrder.LoadDate.ToString("HH:mm:ss tt"),
                        Item = detail.ItemDefinition.Item,
                        Description = detail.ItemDefinition.Description,
                        Slot = string.Empty,
                        OnHand = string.Empty,
                        Ordered = detail.Quantity.ToString()
                    });
                }
                else  // do have inventory
                {
                    foreach (var inventory in inv)
                    {
                        outs.Add(new PickList
                        {
                            Area = detail.Area.AreaNumber.ToString(),
                            Workstation = workstationView.WorkstationId.ToString(),
                            OrderDetailId = detail.Id.ToString(),
                            Order = detail.ReplenOrder.Ord1,
                            Invoice = detail.ReplenOrder.Ord2,
                            CostCenter = string.Empty,
                            Recipient = string.Empty,
                            Date = detail.ReplenOrder.LoadDate.ToString("MM/dd/yyyy"),
                            Time = detail.ReplenOrder.LoadDate.ToString("HH:mm:ss tt"),
                            Item = detail.ItemDefinition.Item,
                            Description = detail.ItemDefinition.Description,
                            Slot = inventory.Location.Slot,
                            OnHand = inventory.Quantity.ToString(),
                            Ordered = detail.Quantity.ToString()
                        });
                    }
                    //then add a line for putting it somewhere else
                    outs.Add(new PickList
                    {
                        Area = detail.Area.AreaNumber.ToString(),
                        Workstation = workstationView.WorkstationId.ToString(),
                        OrderDetailId = detail.Id.ToString(),
                        Order = detail.ReplenOrder.Ord1,
                        Invoice = detail.ReplenOrder.Ord2,
                        CostCenter = string.Empty,
                        Recipient = string.Empty,
                        Date = detail.ReplenOrder.LoadDate.ToString("MM/dd/yyyy"),
                        Time = detail.ReplenOrder.LoadDate.ToString("HH:mm:ss tt"),
                        Item = detail.ItemDefinition.Item,
                        Description = detail.ItemDefinition.Description,
                        Slot = string.Empty,
                        OnHand = string.Empty,
                        Ordered = detail.Quantity.ToString()
                    });
                }

            }


            return outs;
        }

        private List<PickList> GetPickListByStationAdjust(int orderId, WorkstationView workstationView)
        {
            var outs = new List<PickList>();
            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndAreaNotCompleted(orderId, workstationView.AreaId);
            if (!details.Any()) return outs;
            foreach (var detail in details)
            {
                //check for Inventory
                var inv = _repoInventory.FindBy(r => r.ItemDefinitionId == detail.ItemDefinitionId).OrderBy(o => o.ReceivedDate).ToList();
                if (inv.Count == 0)
                {
                    // put 1 line in for where they put it.
                    outs.Add(new PickList
                    {
                        Area = detail.Area.AreaNumber.ToString(),
                        Workstation = workstationView.WorkstationId.ToString(),
                        OrderDetailId = detail.Id.ToString(),
                        Order = detail.ReplenOrder.Ord1,
                        Invoice = detail.ReplenOrder.Ord2,
                        CostCenter = string.Empty,
                        Recipient = string.Empty,
                        Date = detail.ReplenOrder.LoadDate.ToString("MM/dd/yyyy"),
                        Time = detail.ReplenOrder.LoadDate.ToString("HH:mm:ss tt"),
                        Item = detail.ItemDefinition.Item,
                        Description = detail.ItemDefinition.Description,
                        Slot = string.Empty,
                        OnHand = string.Empty,
                        Ordered = detail.Quantity.ToString()
                    });
                }
                else  // do have inventory use the first
                {
                    var inventory = inv.First();

                    outs.Add(new PickList
                    {
                        Area = detail.Area.AreaNumber.ToString(),
                        Workstation = workstationView.WorkstationId.ToString(),
                        OrderDetailId = detail.Id.ToString(),
                        Order = detail.ReplenOrder.Ord1,
                        Invoice = detail.ReplenOrder.Ord2,
                        CostCenter = string.Empty,
                        Recipient = string.Empty,
                        Date = detail.ReplenOrder.LoadDate.ToString("MM/dd/yyyy"),
                        Time = detail.ReplenOrder.LoadDate.ToString("HH:mm:ss tt"),
                        Item = detail.ItemDefinition.Item,
                        Description = detail.ItemDefinition.Description,
                        Slot = inventory.Location.Slot,
                        OnHand = inventory.Quantity.ToString(),
                        Ordered = detail.Quantity.ToString()
                    });
                }

            }


            return outs;
        }

        private void MBAdjustOrder_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Any())
            {
                ShowOrderDetailsByOrderAndStation(orders.First(), _workstationView);
            }
        }

        private void ShowOrderDetailsByOrderAndStation(ReplenOrder order, WorkstationView workstationView)
        {
            var details = GetPickListByStationAdjust(order.Id, workstationView);
            //var details = _orderDetailsRepository.GetOrderDetailsByOrderAndStation(order.Id, stationNumber);
            if (!details.Any()) return;
            //foreach (var detail in details)
            //{
            //    detail.PickedQuantity = detail.Quantity;
            //}

            // _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewAdjust.DataSource = details; // _bindingSourceOrderDetailsView;
            LabelFormTitle.Text = _resourceManager.GetString($"JobDetails");
            tabControl1.SelectedTab = AdjustOrder;
        }
        private void MBAdjustOrderSave_Click(object sender, EventArgs e)
        {
            //var rows = DataGridViewAdjust.Rows
            //    .OfType<DataGridViewRow>()
            //    .Where(row => !row.IsNewRow)
            //    .ToArray();
            var details = (IList<PickList>)DataGridViewAdjust.DataSource;

            foreach (var row in details)
            {
                var orderDetailId = row.OrderDetailId;
                //var newValue = row.Ordered;
                var orderDetail = _repoReplenOrderDetails.FindByKey(orderDetailId.ParseInt());
                orderDetail.PickedQuantity = row.Ordered.ParseInt();
                orderDetail.LineStatusId = (int)LineStatus.Complete;
                _repoReplenOrderDetails.Update(orderDetail);
                _historyManager.SaveHistory(ActionCode.StoreRack, orderDetail);

                var order = _repoReplenOrder.FindByKey(orderDetail.ReplenOrderId);
                //order.OrderStatusId = (int)OrderStatus.Complete;
                CheckForReplenOrderComplete(order);

                ShowAvailableRackScreen();
            }
        }

        private void MBAdjustOrderBack_Click(object sender, EventArgs e)
        {
            ShowAvailableRackScreen();
        }

        private void DataGridViewAdjust_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var grid = ((DataGridView)sender);
            if (grid?.CurrentRow == null) return;
            var pickList = (PickList)grid.CurrentRow.DataBoundItem;
            AdjustRackStore(pickList);

            var orderId = _repoReplenOrderDetails.FindByKey(pickList.OrderDetailId.ParseInt()).ReplenOrderId;

            var details = GetPickListByStationAdjust(orderId, _workstationView);
            if (!details.Any()) return;
            DataGridViewAdjust.DataSource = details;
            LabelFormTitle.Text = _resourceManager.GetString($"JobDetails");
            tabControl1.SelectedTab = AdjustOrder;
        }

        private void AdjustRackStore(PickList pickList)
        {
            OpenHotActionForm(pickList);
        }

        private void CreateJobButtonEnable()
        {
            if (
                TextBoxNewOrderOrd1.Text.Length > 0
                && TextBoxNewOrderOrd2.Text.Length > 0
                && TextBoxNewOrderPriority.Text.Length > 0
                && _bindingSourceNewItems.Count > 0 && ((NewItemView)_bindingSourceNewItems.Current).Item != null
            )
            {
                MBNewOrderSave.Enabled = true;
            }
            else
            {
                MBNewOrderSave.Enabled = false;
            }
        }

        private void TextBoxNewOrderOrd1_TextChanged(object sender, EventArgs e)
        {
            CreateJobButtonEnable();
        }

        private void TextBoxNewOrderOrd2_TextChanged(object sender, EventArgs e)
        {
            CreateJobButtonEnable();
        }

        private void TextBoxNewOrderPriority_TextChanged(object sender, EventArgs e)
        {
            CreateJobButtonEnable();
        }

        private void DataGridViewNewItems_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            CreateJobButtonEnable();
        }

        private async void MBSkipPick_Click(object sender, EventArgs e)
        {
            await SkipPick();
        }

        private async Task SkipPick()
        {
            if (_currentPickStop.PickViews.Count > 1 && _neutronVariables.SpecialBackOrder)
            {
                //EnablePickAccept(false);
                await SpecialPickAccept();
                await CompleteSpecialPick();
                //EnablePickAccept(true);
            }
            else  // only skipping one pickview
            {

                Cursor.Current = Cursors.WaitCursor;
                MBSkipPick.Enabled = false;
                // ---         _logger.LogDetailAsync($"SkipPick_Click Start : [{DateTime.Now.ToLongTimeString()}]"));

                _currentPickStop.Skipped = true;

                if (_deviceManager != null)
                {
                    _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                }


                // var pickViewCount = _currentPickStop.PickViews.Count;
                foreach (var pickView in _currentPickStop.PickViews)
                {
                    // Don't skip any pickViews that have a pickedQty
                    // this allows for picking some and skipping the rest.
                    pickView.PickedQty = 0;
                    SetOrderDetailLineStatus(pickView.OrderDetail, (int)LineStatus.Hold, ActionCode.HoldLine);
                }

                //----------
                // ---         _logger.LogDetailAsync($"History Done"));

                var numberOfStops = _bindingSourcePickStops.Count;
                if (_currentPickStop.Sequence < numberOfStops)
                {
                    // ---         _logger.LogDetailAsync("Clear Active Device Indicator - SkipPick"));

                    _bindingSourcePickStops.MoveNext();
                    _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                    await UpdatePickScreen();
                    // UpdateCurrentDeviceIndicator();

                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

                    await UpdatePickPosition();

                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    // UpdateTowerDisplay();
                }
                else
                {
                    // ---         _logger.LogDetailAsync($"Close Batch With Skip"));
                    await CloseBatchWithSkip();
                }

                // ---         _logger.LogDetailAsync($"SkipPick_Click End : [{DateTime.Now.ToLongTimeString()}]"));
                Cursor.Current = Cursors.Default;
                MBSkipPick.Enabled = true;

            }
        }

        //private void SpecialPickAccept()
        //{
        //    // ---         _logger.LogDetailAsync($"Special Pick Accept Run"));
        //    if (InvokeRequired)
        //    {
        //        var method = new MethodInvoker(SpecialPickAccept);
        //        Invoke(method);
        //        return;
        //    }

        //    foreach (var pickView in _currentPickStop.PickViews)
        //    {
        //        //open a form to update the pickview
        //        if (PickStopAdjustmentForm(pickView) != true) return;
        //    }
        //}

        // SpecialBackorderProcess Form
        //private bool PickStopAdjustmentForm(ReplenPickView pickView)
        //{
        //    //assume it will succeed
        //    var success = true;
        //    // set values of local variables in case we need to roll back
        //    var status = pickView.OrderDetail.LineStatusId;
        //    var quantity = pickView.QuantityToBePicked;

        //    using (var form = new FrmReplenPickViewAdjustment(pickView))
        //    {
        //        var result = form.ShowDialog();
        //        var buttonPressed = form.ButtonPressed;
        //        if (!form.Success)
        //        {
        //            // form did not exit with OK, so roll back the changes
        //            pickView.OrderDetail.LineStatusId = status;
        //            pickView.QuantityToBePicked = quantity;
        //            success = false;
        //        }
        //        else   // form is successful
        //        {
        //            _repoReplenOrderDetails.Update(pickView.OrderDetail);
        //            switch (buttonPressed)
        //            {
        //                case "Highlight":
        //                    {
        //                        success = true;
        //                        _historyManager.SaveHistory(ActionCode.Skip, pickView.OrderDetail);
        //                        break;
        //                    }
        //                case "Backorder":
        //                    {
        //                        // ---         _logger.LogDetailAsync($"PickStopAdjustmentForm  Backorder Option"));
        //                        UpdatePickViewInventoryQuantity(pickView);
        //                        success = true;
        //                        break;
        //                    }
        //                case "Accept":
        //                    {
        //                        success = true;
        //                        _currentPickStop.UpdatePickView(pickView, GlobalVar.User);
        //                        // ---         _logger.LogDetailAsync($"PickStopAdjustmentForm  Accept Option"));
        //                        UpdatePickViewInventoryQuantity(pickView);
        //                        break;
        //                    }
        //            }
        //        }
        //    }

        //    return success;
        //}


        //private async Task CompleteSpecialPick()
        //{
        //    Console.WriteLine("Clear Active Device Indicator - Pick Accept");

        //    _deviceIndicatorManager?.ClearAllDeviceIndicators();

        //    //foreach (var pickView in _currentPickStop.PickViews)
        //    //{
        //    //    if (CheckForOrderCompleteOnDevice(pickView.OrderDetail.Order))
        //    //    {
        //    //        if (_neutronVariables.PrintPackingListEnd)
        //    //        {
        //    //            PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
        //    //        }
        //    //    }
        //    //}

        //    // ---         _logger.LogDetailAsync($"PickAccept Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

        //    var numberOfStops = _bindingSourcePickStops.Count;
        //    var position = _bindingSourcePickStops.Position;

        //    if (_currentPickStop.Sequence < numberOfStops)
        //    {
        //        // if multiple locations were required to complete this stop
        //        // the devices will not be where they're supposed to be for the
        //        // normal picking process
        //        // The device manager knows, based on the stop, where each device should be
        //        // The Reset command of the DeviceManager will send a command to each
        //        // device to put it in the correct position/location
        //        if (_multiLocationStop)
        //        {
        //            // Instead of resetting all the devices THEN moving the one
        //            // to the Next Stop, the ResetMoveNext combines the action
        //            // and the deviceManager doesn't reset the device that needs to 
        //            // move to the next stop, it calls the MoveNext function instead
        //            if (_deviceManager != null)
        //            {
        //                _deviceManager.ResetMoveNext(_currentPickStop.Inventory[0].Location.Loc1);
        //            }

        //            _multiLocationStop = false;
        //        }
        //        else
        //        {
        //            //Use the first carousel location for the movenext in case multiple picks are required for stop
        //            if (_deviceManager != null)
        //            {
        //                _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
        //            }
        //        }

        //        _bindingSourcePickStops.MoveNext();
        //        _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
        //        if (_workstationView.Area.LocationTypeId != (int)LocationTypeEnum.Rack)
        //        {
        //            PrintLabels(_currentPickStop);
        //        }


        //        UpdatePickScreen();
        //        //UpdateCurrentDeviceIndicator();

        //        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

        //        await UpdatePickPosition();

        //        UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
        //        // UpdateTowerDisplay();
        //    }
        //    else
        //    {
        //        // ---         _logger.LogDetailAsync($"CloseBatch"));
        //        await CloseBatchAsync();
        //    }
        //}

        private async Task CloseBatchWithSkip()
        {
            // ---             _logger.LogDetailAsync($"CloseBatchWithSkip START"));
            // ClearAllShi();
            // ClearAllBli();
            // ClearOc();
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
            }


            ClearOrderPositions();

            await ClearBatchPositions();

            _deviceIndicatorManager?.ClearAllDeviceIndicators();



            //if (_neutronLicense.CompanyCode == "SFH")
            //{
            //    var shortReportProcessor = new ShortReportProcessor(_bindingSourcePickStops, _labelPrinter);
            //}

            ParkPositionAfterBatch();

            if (_neutronVariables.AutoLogOff)
            {
                CloseButtonPressed = true;
                Close();
            }
            else
            {
                await AvailableOrdersScreen();
            }
            // ---         _logger.LogDetailAsync($"CloseBatchWithSkip END"));
        }


        //private void UpdateInventoryQuantity(ReplenPickStop pickStop)
        //{
        //    var sb = new StringBuilder();
        //    //currentPickStop.CurrentInventoryLocation.Quantity -= currentPickStop.PickedQty;
        //    // repoInventory.Update(currentPickStop.CurrentInventoryLocation);
        //    sb.AppendLine($"Update Inventory Quantity for PICKSTOP Item: {pickStop.Item} - Start");

        //    foreach (var pickView in pickStop.PickViews)
        //    {
        //        UpdatePickViewInventoryQuantity(pickView);
        //    }
        //    // ---         _logger.LogDetailAsync($"{sb.ToString()}"));
        //}

        private async Task UpdatePickViewInventoryQuantity(ReplenPickView pickView)
        {
            if (pickView == null)
                throw new ArgumentNullException(nameof(pickView));
            if (pickView.PickLocations == null || pickView.Inventory == null)
                throw new ArgumentException("StoreLocations or Inventory is null");
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Update Inventory Quantity for PICKVIEW Item: {pickView.Item} - Start");
            logMessage.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");
            try
            {
                if (pickView.PickLocations.Any())
                {
                    await ProcessPickLocations(pickView, logMessage);
                }
                else
                {
                    await ProcessInventory(pickView, logMessage);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"{ex.Message}");
            }
            await _logger.LogDetailAsync($"{logMessage}");
        }
        private async Task ProcessPickLocations(ReplenPickView pickView, StringBuilder logMessage)
        {
            foreach (var pickLocation in pickView.PickLocations)
            {
                logMessage.AppendLine($"Store Location: {pickLocation.Inventory.Location.Slot}");
                var inventory = await _repoInventory.FindByKeyIncludeAsync(r => r.Id == pickLocation.Inventory.Id, r => r.Location);
                if (inventory != null)
                {
                    logMessage.AppendLine($"Inventory Qty: {inventory.Quantity}  Store Location Qty: {pickLocation.Quantity}");

                    inventory.Quantity += pickLocation.Quantity;
                    await _repoInventory.UpdateAsync(inventory);
                    logMessage.AppendLine("Update Inventory");
                    await _historyManager.SaveHistoryAsync(ActionCode.StoreOrder, inventory, pickLocation.Quantity, pickView);
                    logMessage.AppendLine("Write Store Order to History");

                    if (inventory.Quantity == 0)
                    {
                        var result = await _dialogService.Show2Async("Release Inventory Location",
                           $"Release Inventory Location for/n {pickView.Item} at {inventory.Location.Slot}?", "Yes", "No");
                        if (result)
                        {
                            await _historyManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
                            await _repoInventory.DeleteAsync(inventory.Id);
                        }
                    }

                }
                else
                {
                    logMessage.AppendLine("Inventory is NULL.");
                }
            }
        }
        private async Task ProcessInventory(ReplenPickView pickView, StringBuilder logMessage)
        {
            var inventory = pickView.Inventory.FirstOrDefault();
            if (inventory != null)
            {
                await _historyManager.SaveHistoryAsync(ActionCode.StoreOrder, inventory, 0, pickView);
                logMessage.AppendLine("Write Pick Order of zero quantity to History");
            }
        }


        //private async Task UpdatePickViewInventoryQuantity(ReplenPickView pickView)
        //{
        //    var sb = new StringBuilder();
        //    //currentPickStop.CurrentInventoryLocation.Quantity -= currentPickStop.PickedQty;
        //    // repoInventory.Update(currentPickStop.CurrentInventoryLocation);
        //    sb.AppendLine($"Update Inventory Quantity for PICKVIEW Item: {pickView.Item} - Start");

        //    sb.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");

        //    if (pickView.PickLocations.Any())
        //    {
        //        foreach (var pickLocation in pickView.PickLocations)
        //        {
        //            sb.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
        //            //  pickLocation.Inventory.Quantity -= pickLocation.Quantity;

        //            var inventory = _repoInventory.FindByKey(pickLocation.Inventory.Id).Result;
        //            if (inventory != null)
        //            {
        //                sb.AppendLine($"Inventory Qty: {inventory.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
        //                inventory.Quantity -= pickLocation.Quantity;

        //                await _repoInventory.UpdateAsync(inventory);
        //                sb.AppendLine("Update Inventory");
        //                await _historyManager.SaveHistoryAsync(ActionCode.StoreOrder, inventory, pickLocation.Quantity, pickView);
        //                sb.AppendLine("Write Pick Order to History");
        //            }
        //            else
        //            {
        //                sb.AppendLine($"Inventory is NULL.");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // no PickLocations so no inventory
        //        // use the PickViews first Inventory Location by default
        //        var inventory = pickView.Inventory[0];
        //        await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, 0, pickView);
        //        sb.AppendLine("Write Pick Order of zero quantity to History");
        //    }

        //    // ---         _logger.LogDetailAsync($"{sb.ToString()}"));
        //}

        private async void MBShortPick_Click(object sender, EventArgs e)
        {
            await ShortPick();
        }
        private async Task ShortPick()
        {
            //EnablePickAccept(false);

            foreach (var pickView in _currentPickStop.PickViews)
            {
                if (_neutronVariables.SpecialBackOrder)
                {
                    await SpecialPickAccept();
                }
                else
                {
                    // get the total of all pickLocations before clearing pickLocations
                    // use that total for the OrderDetail Update
                    var pickViewTotal = _currentPickStop.GetPickViewTotal(pickView);

                    ////write any pickLocations to history
                    await UpdatePickViewInventoryQuantity(pickView);

                    pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                    pickView.OrderDetail.PickedQuantity = pickViewTotal;
                    await _repoReplenOrderDetails.UpdateAsync(pickView.OrderDetail);
                }
            }

            // function to finish the move and update the lights for the next pick
            await CompleteSpecialPick();
            //EnablePickAccept(true);
        }
        private async Task SpecialPickAccept()
        {
            await _logger.LogDetailAsync($"Special Pick Accept Run");

            if (InvokeRequired)
            {
                var method = new MethodInvoker(() => SpecialPickAccept().Wait());
                Invoke(method);
                return;
            }

            foreach (var pickView in _currentPickStop.PickViews)
            {
                //open a form to update the pickview
                //MessageBox.Show($"Fix PickStopAdjustmentForm Function");
                if (await PickStopAdjustmentForm(pickView) != true) return;
            }
        }

        private async Task<bool> PickStopAdjustmentForm(ReplenPickView pickView)
        {
            //assume it will succeed
            var success = true;
            // set values of local variables in case we need to roll back
            var status = pickView.OrderDetail.LineStatusId;
            var quantity = pickView.QuantityToBePicked;

            using (var form = new FrmReplenPickViewAdjustment(pickView))
            {
                var result = form.ShowDialog();
                var buttonPressed = form.ButtonPressed;
                if (!form.Success)
                {
                    // form did not exit with OK, so roll back the changes
                    pickView.OrderDetail.LineStatusId = status;
                    pickView.QuantityToBePicked = quantity;
                    success = false;
                }
                else   // form is successful
                {
                    await _repoReplenOrderDetails.UpdateAsync(pickView.OrderDetail);
                    switch (buttonPressed)
                    {
                        case "Highlight":
                            {
                                await _historyManager.SaveHistoryAsync(ActionCode.Skip, pickView.OrderDetail);
                                break;
                            }
                        case "Backorder":
                            {
                                // ---         Task.Run(() => _logger.LogDetailAsync($"PickStopAdjustmentForm  Backorder Option"));
                                await UpdatePickViewInventoryQuantity(pickView);
                                break;
                            }
                        case "Accept":
                            {
                                _currentPickStop.UpdatePickView(pickView, GlobalVar.User);
                                // ---         Task.Run(() => _logger.LogDetailAsync($"PickStopAdjustmentForm  Accept Option"));
                                await UpdatePickViewInventoryQuantity(pickView);
                                break;
                            }
                    }
                }
            }

            return success;
        }

        private async Task CompleteSpecialPick()
        {
            _logger.LogDetailAsync("Clear Active Device Indicator - Pick Accept").SafeFireAndForget();

            _deviceIndicatorManager?.ClearAllDeviceIndicators();

            //foreach (var pickView in _currentPickStop.PickViews)
            //{
            //    if (await CheckForOrderCompleteOnAllStations(pickView.OrderDetail.Order))
            //    {
            //        if (_neutronVariables.PrintPackingListEnd)
            //        {
            //            PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
            //        }
            //    }
            //}

            // ---         Task.Run(() => _logger.LogDetailAsync($"PickAccept Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

            var numberOfStops = _bindingSourcePickStops.Count;
            var position = _bindingSourcePickStops.Position;

            if (_currentPickStop.Sequence < numberOfStops)
            {
                // if multiple locations were required to complete this stop
                // the devices will not be where they're supposed to be for the
                // normal picking process
                // The device manager knows, based on the stop, where each device should be
                // The Reset command of the DeviceManager will send a command to each
                // device to put it in the correct position/location
                if (_multiLocationStop)
                {
                    // Instead of resetting all the devices THEN moving the one
                    // to the Next Stop, the ResetMoveNext combines the action
                    // and the deviceManager doesn't reset the device that needs to 
                    // move to the next stop, it calls the MoveNext function instead
                    _deviceManager?.ResetMoveNext(_currentPickStop.Inventory[0].Location.Loc1);

                    _multiLocationStop = false;
                }
                else
                {
                    //Use the first carousel location for the movenext in case multiple picks are required for stop
                    _deviceManager?.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                }

                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                if (_workstationView.Area.LocationTypeId != (int)LocationTypeEnum.Rack)
                {
                    PrintLabels(_currentPickStop);
                }


                await UpdatePickScreen();
                //UpdateCurrentDeviceIndicator();

                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

                await UpdatePickPosition();

                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
            }
            else
            {
                // ---         Task.Run(() => _logger.LogDetailAsync($"CloseBatch"));
                await CloseBatchAsync();
            }
        }

        /// <summary>
        /// Updates the screen with the Current PickStop information
        /// Loads the image if the workstation is configured to use images
        /// Calculates the PickedSoFar quantity and the QuantityToBePicked
        /// </summary>
        private async Task UpdatePickScreen()
        {
            await _logger.LogDetailAsync($"UpdatePickScreen Start: [{DateTime.Now.ToLongTimeString()}]");
            // Topura has the option to ask for a new item
            //MBPickNewItem.Visible = _neutronLicense.CompanyCode == "TOP" ? true : false;

            // Set the Pick Screen Labels
            LabelFormTitle.Text = _resourceManager.GetString($"Selection");
            LabelPickDescription.Text = _currentPickStop.Description;
            LabelPickItemNumber.Text = _currentPickStop.Item;
            LabelPickUOI.Text = _currentPickStop.UnitOfIssue;
            LabelLineOfLines.Text = string.Format("{0} of {1}"
                , (_currentPickStop.Sequence).ToString(), _bindingSourcePickStops.Count);
            TextBoxRequestedQty.Text = _currentPickStop.Quantity.ToString();

            var pickedSoFar = GetPickedSoFar(_currentPickStop.PickViews);
            TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            var quantityToBePicked = _currentPickStop.QuantityToBePicked;
            LabelPickQty.Text = quantityToBePicked.ToString();
            // ---         Task.Run(() => _logger.LogDetailAsync($"UpdatePickScreen End: [{DateTime.Now.ToLongTimeString()}]"));

            // If the workstation is configured to use images
            if (_neutronVariables.UseImages)
            {
                PictureBoxItemImage.LoadAsync(_imageManager.GetImageFile(_currentPickStop.Item));
            }

            MultipleLocationsManager(_currentPickStop);

            MBPickChangeQuantity.Enabled = quantityToBePicked > 0;
            MBSkipPick.Enabled = quantityToBePicked > 0;
            MBShortPick.Enabled = quantityToBePicked > 0;

            // wait 3 seconds before enabling MBPickAccept button
            // await Task.Delay(4000);

            // MBPickAccept.Visible = quantityToBePicked > 0;

        }

        //TODO
        private void MultipleLocationsManager(ReplenPickStop currentPickStop)
        {
            // GroupBoxMultipleLocations.Visible = false;
            var pickLocations = new List<PickLocation>();
            if (currentPickStop == null) return;
            var pickViews = _currentPickStop.PickViews.Where(r => r.PickLocations.Count > 0).ToList();
            if (!pickViews.Any()) return;
            foreach (var pickView in pickViews)
            {
                foreach (var pickLocation in pickView.PickLocations)
                {
                    pickLocations.Add(pickLocation);
                }
            }

            var distinctLocations = pickLocations.Distinct().ToList();
            //GroupBoxMultipleLocations.Visible = distinctLocations.Count != 1;
        }

        private async Task UpdatePickScreenAfterChangeQuantity()
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"UpdatePickScreen AfterChangeQuantity Start: [{DateTime.Now.ToLongTimeString()}]"));
            // UpdateTowerDisplay();
            await UpdatePickPosition();
            LabelPickQty.Text = (_currentPickStop.QuantityToBePicked).ToString();


            // ---         Task.Run(() => _logger.LogDetailAsync($"UpdatePickScreen AfterChangeQuantity End: [{DateTime.Now.ToLongTimeString()}]"));
        }
        private async void MBClear_Click(object sender, EventArgs e)
        {
            ClearAllSelectOrdersToPick();
            await ClearBatchPositions();
            ClearDataGridViewBackColor();
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
            }
            SetFocusNextTextBoxPos();
            FocusOnCurrentTextBox();
        }
        private void ClearDataGridViewBackColor()
        {
            var rows = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                .Where(r => r.DefaultCellStyle.BackColor == Color.LawnGreen).ToList();
            if (rows.Any())
            {
                foreach (var row in rows)
                {
                    DataGridViewAvailableOrders.Rows[row.Index].DefaultCellStyle.BackColor = Color.White;
                }
            }
        }

        /// <summary>
        /// Sets the focus on the current text box and selects all text within it.
        /// </summary>
        private void FocusOnCurrentTextBox()
        {
            if (_currentTextBoxPos.InvokeRequired)
            {
                _currentTextBoxPos.Invoke(new Action(() =>
                {
                    _currentTextBoxPos.Focus();
                    _currentTextBoxPos.SelectAll();
                }));
            }
            else
            {
                _currentTextBoxPos.Focus();
                _currentTextBoxPos.SelectAll();

            }
        }
    }
}
