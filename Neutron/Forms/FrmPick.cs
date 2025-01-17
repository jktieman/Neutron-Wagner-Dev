using AlliedLogger;
using AsyncAwaitBestPractices;
using DeviceIndicatorService;
using EnumsNET;
using Equin.ApplicationFramework;
using IPTI.Models;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Classes;
using Neutron.Enums;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using Neutron.Ninject;
using Neutron.UserControls;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronDllu;
using NeutronEvents;
using NeutronLoader;
using ReplenService;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore.StaticClasses;
using NeutronData.UnitOfWorks;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using Panel = System.Windows.Forms.Panel;
using ScrollBars = System.Windows.Forms.ScrollBars;
using StorageType = NeutronData.Models.Lookups.StorageType;
using TextBox = System.Windows.Forms.TextBox;


namespace Neutron.Forms
{
    public partial class FrmPick : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        private readonly IInventoryRepository _inventoryRepository;
        private IInventoryUnitOfWork _inventoryUnitOfWork;

        private ResourceManager _gridResourceManager;

        private readonly NeutronDb _context = new NeutronDb();

        private readonly GenericRepository<Order> _repoOrders;
        private readonly GenericRepository<OrderDetail> _repoOrderDetails;
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly GenericRepository<Location> _repoLocation;
        private readonly GenericRepository<StorageType> _repoStorageTypes;
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder;
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail;
        private readonly GenericRepository<PrintJob> _repoPrintJob;

        private readonly AkaRepository _repoAka = new AkaRepository();
        private readonly OrderDetailsRepository _orderDetailsRepository = new OrderDetailsRepository();
        private readonly IWorkstationRepository _workstationRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IAreaRepository _areaRepository;

        private BindingListView<AvailableOrdersView> _bindingListViewAvailableOrdersViews;

        private readonly BindingSource _bindingSourceCompleted;
        private readonly BindingSource _bindingSourceOrderView;
        private readonly BindingSource _bindingSourceAvailableOrders;
        private readonly BindingSource _bindingSourceAvailableOrdersRack;
        private readonly BindingSource _bindingSourcePickViews;
        private readonly BindingSource _bindingSourcePickStops;
        private readonly BindingSource _bindingSourceHot;
        private readonly BindingSource _bindingSourceOrderDetailsView;
        private readonly BindingSource _bindingSourceSkipView;
        private readonly BindingSource _bindingSourceItems;
        private BindingSource _bindingSourceNewItems;
        private BindingSource _bindingSourceReplenishments;

        public bool CloseButtonPressed { get; set; }
        public OrderView CurrentItem;
        public RackOrderView CurrentRackItem;
        private AvailableOrdersView _currentAvailableOrdersView;
        private TextBox _currentTextBoxPos;
        private List<BatchPosition> _ordersToPick;
        private PickStop _currentPickStop;

        // private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        private bool _openHotPickFromPickScreen;
        private bool _showSkipped;
        private bool _shortPick;

        private TabPage _previousTab;

        readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;
        private readonly IHistoryManager _historyManager;

        private PickDeviceManager _deviceManager;
        private DocumentToPrint _documentToPrint;
        private DocumentPrinterPreferences _documentPrinterPreferences;
        private LabelPrinterPreferences _labelPrinterPreferences;

        private readonly IJsonData _jsonData;

        private readonly IAkaRepository _akaRepository;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;
        private IDynamicLogger _logger;

        private CurrentDataSet _currentDataSet;
        private int _currentJobDetailsOrderId;

        private DeviceIndicatorManager _deviceIndicatorManager;

        //GRIDS
        private bool _orderGridReady;
        private bool _availableOrdersGridReady;
        private bool _pickViewGridReady;
        private bool _orderDetailsGridReady;
        private bool _newOrderGridReady;
        private bool _newItemsGridReady;
        private bool _availableOrdersRackGridReady;
        private bool _adjustGridReady;
        private bool _skipGridReady;
        private bool _skipInventoryGridReady;

        //private readonly int[] _moveableDeviceTypes;

        //private readonly List<Workstation> _pickStations;
        private readonly WorkstationView _workstationView;
        private const int AreaEight = AreaNumber.Eight;
        private readonly int[] _areaIdsForThisWorkstation;

        private List<Inventory> _currentInventory;
        private bool _multiLocationStop;
        private bool _spaceBarDisabled;
        private ReplenProcessor _replenProcessor;
        private StorageType _defaultStorageType;

        private const int BatchDisplayFontSize = 26;
        private const int Reprint = 2;
        private const int OrderDetailPartsCount = 5;


        // private Timer _spaceBarDelayTimer;
        private bool _useCostCenter = false;

        // Hardware Flags to indicate if hardware device/devices are attached...
        //private bool _blastzone = false;
        //private bool _batchTable = false;
        //private bool _prolite = false;
        //private bool _hanel = false;
        private bool _isClientConnected = false;

        private readonly IIptiDisplayFunctions _iptiDisplayFunctions;
        private readonly IDialogService _dialogService;
        private IptiConfig _iptiConfig;
        private SynchronizationContext _synchronizationContext;

        private System.Windows.Forms.Timer _debounceTimer;
        private TextBox _textBoxPos;
        private const int DebounceInterval = 300; // milliseconds

        public delegate void UpdatePickAcceptDelegate(bool b);
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private DateTime _lastKeystrokeTime;
        private TextBox _previousTextBoxPos;
        private int _currentPickStopSequence;
        private List<BatchPosition> _currentBatchPositions;
        private const int ScannerThreshold = 50; // milliseconds

        public FrmPick(IJsonData jsonData, WorkstationView workstationView
            , IAkaRepository akaRepository, NeutronVariables neutronVariables
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IWorkstationRepository workstationRepository
            , IOrdersRepository ordersRepository, NeutronLicense neutronLicense
            , IItemDefinitionsRepository itemDefinitionsRepository, IHistoryManager historyManager
            , ILocationsRepository locationsRepository
            , IAreaRepository areaRepository, IInventoryRepository inventoryRepository, IInventoryUnitOfWork inventoryUnitOfWork
            , IIptiDisplayFunctions iptiDisplayFunctions
            , IDialogService dialogService)
        {

            InitializeComponent();

            _cultureInfo = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
            if (_cultureInfo == null || string.IsNullOrEmpty(_cultureInfo.Name))
            {
                throw new InvalidOperationException("CultureInfo or its Name property is invalid.");
            }
            try
            {
                SetCulture(_cultureInfo.Name);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Failed to set culture.", ex);
            }



            _repoOrders = new GenericRepository<Order>(_context);
            _repoOrderDetails = new GenericRepository<OrderDetail>(_context);
            _repoInventory = new GenericRepository<Inventory>(_context);
            _repoLocation = new GenericRepository<Location>(_context);
            _repoStorageTypes = new GenericRepository<StorageType>(_context);
            _repoItemDefinition = new GenericRepository<ItemDefinition>(_context);
            _repoReplenOrder = new GenericRepository<ReplenOrder>(_context);
            _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(_context);
            _repoPrintJob = new GenericRepository<PrintJob>(_context);

            _bindingSourceCompleted = new BindingSource();
            _bindingSourceOrderView = new BindingSource();
            _bindingSourceAvailableOrders = new BindingSource();
            _bindingSourceAvailableOrdersRack = new BindingSource();
            _bindingSourcePickViews = new BindingSource();
            _bindingSourcePickStops = new BindingSource();
            _bindingSourceHot = new BindingSource();
            _bindingSourceOrderDetailsView = new BindingSource();
            _bindingSourceSkipView = new BindingSource();
            _bindingSourceItems = new BindingSource();
            _bindingSourceNewItems = new BindingSource();
            _bindingSourceReplenishments = new BindingSource();

            _ordersToPick = new List<BatchPosition>();
            _currentPickStop = new PickStop();

            _jsonData = jsonData;
            _workstationView = workstationView;
            _akaRepository = akaRepository;
            _neutronVariables = neutronVariables;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _workstationRepository = workstationRepository;
            _ordersRepository = ordersRepository;
            _neutronLicense = neutronLicense;
            _itemDefinitionsRepository = itemDefinitionsRepository;
            _historyManager = historyManager;
            _locationsRepository = locationsRepository;
            _areaRepository = areaRepository;
            _inventoryRepository = inventoryRepository;
            _inventoryUnitOfWork = inventoryUnitOfWork;
            _iptiDisplayFunctions = iptiDisplayFunctions;
            _dialogService = dialogService;
            _iptiConfig = _jsonData.LoadFile<IptiConfig>();
            _areaIdsForThisWorkstation = new[] { _workstationView.AreaId };
            InitForm();
        }

        private void InitForm()
        {
            KeyPreview = true;
            this.KeyDown += FrmPick_KeyDownHandler;
            _logger = NeutronCore.Global.Logger.SetupLogger("Pick");
            _logger.LogDetailAsync($"Form Pick Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();

            SetupPrinters();
            _synchronizationContext = SynchronizationContext.Current;
            InitGrids();

            _replenProcessor = new ReplenProcessor();

            _useCostCenter = _neutronVariables.UseCostCenter;
            SetupPickPositions(_neutronVariables.PickBatchSize, _neutronVariables.PickBatchRows);
            _ordersToPick = InitOrdersToPick(_neutronVariables.PickBatchSize);
            HideTabControlTabs();
            ShowButtons();
            SetLoaderButtonText();
            SetUploadButtonText();
            FillComboBoxAreaNumbers();

            mlUserInfo.Text = GetCurrentUserString();



            CloseButtonPressed = false;
            _currentTextBoxPos = (TextBox)Controls.Find($"TextBoxPos1", true).First();
            _documentToPrint = new DocumentToPrint();
            MBPrint.Visible = _neutronVariables.PrintPackingListManual;
            MBPickScreenHotPick.Enabled = _securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions];
            if (_workstationView.StationType.Id == (int)StationType.Supervisor) MBMainAvailableOrders.Visible = false;
            _defaultStorageType = _repoStorageTypes.FindByKey(_neutronVariables.DefaultStorageTypeId);
            MBMainAvailableOrders.Text = $"{_resourceManager.GetString($"AvailableOrders")} - {_workstationView.Area.Name}";
            if (_workstationView.Hanels.Any())
            {
                InitDeviceIndicators();
            }

            _debounceTimer = new System.Windows.Forms.Timer();
            _debounceTimer.Interval = DebounceInterval;
            _debounceTimer.Tick += DebounceTimer_Tick;
            // _ipticonfig = _jsonData.LoadFile<IptiConfig>();

            //_tcpIptiCommandCenter = new TcpIptiCommandCenter(_jsonData, _logger, _workstationView);

            Mediator.GetInstance().IptiButtonPressed += async (s, e) => await IptiButtonPickAccept(e.ResponseInfo);
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopUploadAction(e.StartStop);
            Mediator.GetInstance().OrderComplete += (s, e) => ShowOrderComplete(e.Order);
            Mediator.GetInstance().TrayInPosition += (s, e) => UpdateTraysInPosition(e.InPositionInfo);
            // Mediator.GetInstance().TransmitStateChanged += (s, e) => UpdateClientConnected(e.State);
            Mediator.GetInstance().IsClientConnected += FrmPick_IsClientConnected;
            Mediator.GetInstance().GeneralError += FrmPick_GeneralError;
        }

        private string GetCurrentUserString()
        {
            if (_resourceManager == null)
            {
                throw new InvalidOperationException("ResourceManager is not initialized.");
            }
            var currentUserString = _resourceManager.GetString("CurrentUser");
            if (string.IsNullOrEmpty(currentUserString))
            {
                throw new InvalidOperationException("The resource key 'CurrentUser' is missing or empty.");
            }
            var userInfo = GlobalVar.User?.UserInfo ?? "Unknown User";
            return $"{currentUserString}: {userInfo}";

        }
        private void FrmPick_KeyDownHandler(object sender, KeyEventArgs e)
        {
            OnFrmPickKeyDown(sender, e).SafeFireAndForget();
        }

        /// <summary>
        /// Handles the Tick event of the debounce timer.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// This method is triggered when the debounce timer elapses. It stops the timer, processes the input asynchronously,
        /// and handles any exceptions that occur during the processing. The timer is restarted at the end of the method.
        /// </remarks>
        private async void DebounceTimer_Tick(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            try
            {
                await _logger.LogDetailAsync($"Start DebounceTimer_Tick").ConfigureAwait(false);
                await _semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    MessageBox.Show("Debounce Timer");
                    await ProcessInput(_textBoxPos).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _logger.LogDetailAsync($"Exception in DebounceTimer_Tick: {ex.Message}").ConfigureAwait(false);
                    // Handle the exception as needed, e.g., show a message to the user
                    // MessageBox.Show("An error occurred while processing input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            finally
            {
                _debounceTimer.Start(); // Restart the timer if needed
            }
        }


        //private async Task ProcessInput(TextBox input)
        //{
        //    await _logger.LogDetailAsync($"Start ProcessInput");
        //    Order order;
        //    if (string.IsNullOrEmpty(input.Text))
        //    {
        //        //clear the position
        //        var position = int.Parse(input.Tag.ToString());
        //        ClearItemFromBatchByPosition(position);
        //    }
        //    else
        //    {
        //        using (var context = new NeutronDb())
        //        {
        //            // get the Order based on the input.Text
        //            order = await context.Orders.FirstOrDefaultAsync(r => r.Ord1 == input.Text);
        //            if (order == null)
        //            {
        //                var position = int.Parse(input.Tag.ToString());
        //                ClearItemFromBatchByPosition(position);
        //            }
        //            else
        //            {
        //                var arrayPosition = AddItemToBatch(order.Id, order.Ord1, order.Ord2);
        //                SetFocusNextTextBoxPos();
        //            }
        //        }
        //    }

        //    // Handle the debounced input here
        //    // MessageBox.Show($"Processed input: {input.Text}  Tag: {input.Tag}");
        //}

        private async Task ProcessInput(TextBox textBox)
        {
            _logger.LogDetailAsync($"Start Scanner ProcessInput").SafeFireAndForget();
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            if (textBox.Tag == null || !int.TryParse(textBox.Tag.ToString(), out var position))
            {
                throw new InvalidOperationException("Invalid textBox tag.");
            }
            _logger.LogDetailAsync($"1 Position: {position}").SafeFireAndForget();
            if (string.IsNullOrEmpty(textBox.Text))
            {
                _logger.LogDetailAsync($"2 textBox.Text is null or empty.").SafeFireAndForget();
                await ClearItemFromBatchByPosition(position);
                _logger.LogDetailAsync($"3 Back From ClearItemFromBatchByPosition Position: {position}").SafeFireAndForget();
                _logger.LogDetailAsync($"4 Returning from null or empty TextBox.Text").SafeFireAndForget();
                return;
            }
            try
            {
                _logger.LogDetailAsync($"5 Starting Try").SafeFireAndForget();
                var rows = GetMatchingRows(textBox.Text);
                _logger.LogDetailAsync($"6 Got Matching Rows {rows.Count}").SafeFireAndForget();
                if (rows.Any())
                {

                    _logger.LogDetailAsync($"7 Got Rows > 1 Call GetRowByTaskNumber.").SafeFireAndForget();
                    var row = rows.Count > 1 ? GetRowByTaskNumber(rows) : rows.FirstOrDefault();

                    if (row != null)
                    {
                        _logger.LogDetailAsync($"8 Got Row By Task Number: {row.Cells["Ord1"].Value}").SafeFireAndForget();

                        var id = Convert.ToInt32(row.Cells["Id"].Value);
                        //               // var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                        //               // var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                        _logger.LogDetailAsync($"9 Add Remove Order From Induction Screen: ID:{id} Index: {row.Index}").SafeFireAndForget();
                        var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
                        _logger.LogDetailAsync($"10 Back From AddRemoveOrderFromInductionScreen").SafeFireAndForget();
                    }
                }
                _logger.LogDetailAsync($"11 Call SetFocusNextTextBoxPos").SafeFireAndForget();
                SetFocusNextTextBoxPos();
                _logger.LogDetailAsync($"12 Back From SetFocusNextTextBoxPos").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in ProcessInput: {ex.Message}").ConfigureAwait(false);
                throw;
            }

            //else
            //{
            //    try
            //    {

            //        // get the row in DataGridViewAvailableOrders where Ord1 = _ordersToPick[index].Ord1 and Ord2 = _ordersToPick[index].Ord2
            //        var rows = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
            //            .Where(r =>
            //                Convert.ToString(r.Cells["Ord1"].Value) == textBox.Text).ToList();
            //        if (rows.Any())
            //        {
            //            // if there is more than one order (Ord1), 
            //            // prompt for Task number.  (Ord2)
            //            //var orders = await context.Orders.Where(r => r.Ord1 == textBox.Text).ToListAsync();
            //            DataGridViewRow row;
            //            if (rows.Count >= 1)
            //            {
            //                // get a list of Ord2 values from orders
            //                var taskNumbers = rows.Select(r => Convert.ToString(r.Cells["Ord2"].Value)).ToList();

            //                // prompt for Task number
            //                var taskNumber = PromptForTaskNumber(taskNumbers);
            //                if (taskNumber == null)
            //                {
            //                    await ClearItemFromBatchByPosition(position);
            //                    return;
            //                }

            //                row = rows
            //                    .FirstOrDefault(r =>
            //                        Convert.ToString(r.Cells["Ord2"].Value) == taskNumber);

            //            }
            //            else
            //            {
            //                row = rows.FirstOrDefault();

            //            }
            //            if (row != null)
            //            {
            //                var id = Convert.ToInt32(row.Cells["Id"].Value);
            //               // var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
            //               // var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
            //                var idx = await AddRemoveOrderFromInductionScreen(id, row.Index);
            //            }
            //            else
            //            {
            //                await ClearItemFromBatchByPosition(position);
            //            }
            //        }
            //        SetFocusNextTextBoxPos();
            //        //if (row == null)
            //        //{

            //        //}
            //        //else
            //        //{
            //        //    var arrayPosition = AddItemToBatch(row.Id, row.Ord1, row.Ord2);

            //        //}

            //    }
            //    catch (Exception ex)
            //    {
            //        await _logger.LogDetailAsync($"Error in ProcessInput: {ex.Message}").ConfigureAwait(false);
            //        throw;
            //    }
            //}
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
        //private void AddItemToBatch(DataGridViewRow row)
        //{
        //    var arrayPosition = AddItemToBatch(row.Id, row.Ord1, row.Ord2);
        //}


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

        private void FrmPick_GeneralError(object sender, LoaderErrorEventArgs e)
        {
            MessageBox.Show(e.Message);
        }

        private void FrmPick_IsClientConnected(object sender, IsClientConnectedEventArgs e)
        {
            if (_isClientConnected.Equals(e.IsClientConnected)) return;
            _isClientConnected = e.IsClientConnected;
            UpdateClientConnected(_isClientConnected);
        }

        private void UpdateClientConnected(bool state)
        {
            _logger.LogDetailAsync("Client connection state updated: " + state).SafeFireAndForget();
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
                MBPickAccept.ForeColor = Color.DarkGreen;
                MBPickAccept.Refresh();
            }
            if (inPositionInfo.TwoInPosition)
            {
                MBPickAccept.ForeColor = Color.Blue;
            }
            if (inPositionInfo.ThreeInPosition)
            {
                MBPickAccept.ForeColor = Color.Red;
            }
        }
        private void InitGrids()
        {

            var id = Thread.CurrentThread.ManagedThreadId;
            Trace.WriteLine("FrmPick thread: " + id);

            _logger.LogDetailAsync("Start Load Grids").SafeFireAndForget();

            if (!_availableOrdersGridReady)
            {
                var callback = new SendOrPostCallback(SetupAvailableOrdersGrid);
                var thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_pickViewGridReady)
            {
                var callback = new SendOrPostCallback(SetupPickViewGrid);
                var thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_orderGridReady)
            {
                var callback = new SendOrPostCallback(SetupOrderGrid);
                var thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_newOrderGridReady)
            {
                var callback = new SendOrPostCallback(SetupNewOrderGrid);
                var thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_orderDetailsGridReady)
            {
                var callback = new SendOrPostCallback(SetupOrderDetailsGrid);
                var thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_newItemsGridReady)
            {
                var callback = new SendOrPostCallback(SetupNewItemsGrid);
                var thread = new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_availableOrdersRackGridReady)
            {
                var callback = new SendOrPostCallback(SetupAvailableOrdersRackGrid);
                var thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_adjustGridReady)
            {
                var callback = new SendOrPostCallback(SetupAdjustGrid);
                var thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }

            if (!_skipGridReady)
            {
                var callback = new SendOrPostCallback(SetupSkipGrid);
                var thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }
            if (!_skipInventoryGridReady)
            {
                var callback = new SendOrPostCallback(SetupSkipInventoryGrid);
                var thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }
        }
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

        private void RunGrid(object state, SendOrPostCallback setupGrid)
        {
            var id = Thread.CurrentThread.ManagedThreadId;
            Trace.WriteLine($"{setupGrid.ToString()}  Thread: " + id);
            if (state is SynchronizationContext uiContext)
            {
                uiContext.Post(setupGrid, null);
            }

        }

        /// <summary>
        /// Initializes the device indicators on the PickScreen.
        /// </summary>
        /// <remarks>
        /// This method checks if the PickScreen is null or if it already contains the "PanelDeviceIndicators" control.
        /// If the PickScreen is valid and does not contain the control, it creates a new <see cref="DeviceIndicatorManager"/>
        /// and adds its <see cref="DeviceIndicatorManager.DeviceIndicatorPanel"/> to the PickScreen controls.
        /// </remarks>
        private void InitDeviceIndicators()
        {
            if (PickScreen == null)
            {
                _logger.LogDetailAsync("PickScreen is null, cannot initialize device indicators").SafeFireAndForget();
                return;
            }
            if (PickScreen.Controls.ContainsKey("PanelDeviceIndicators")) return;
            _logger.LogDetailAsync("Initialize Device Indicators - Create New DeviceIndicatorManager").SafeFireAndForget();
            _deviceIndicatorManager = new DeviceIndicatorManager(_workstationView, new Point(189, 0),
                new Size(769, 127), _neutronVariables);
            PickScreen.Controls.Add(_deviceIndicatorManager.DeviceIndicatorPanel);
        }

        //private Point GetLocation(int sizeWidth, int numDevices, int deviceNumber)
        //{
        //    Point point;
        //    var eachBlock = sizeWidth / numDevices;
        //    var centerBlock = eachBlock / 2;
        //    var positionInBlock = centerBlock - 60;
        //    if (deviceNumber == 1)
        //    {
        //        point = new Point(positionInBlock, 5);
        //    }
        //    else
        //    {
        //        var pos = positionInBlock + (deviceNumber - 1) * eachBlock;
        //        point = new Point(pos, 5);
        //    }

        //    return point;

        //}

        /// <summary>
        /// Populates the ComboBox with area numbers retrieved from the area repository.
        /// </summary>
        /// <remarks>
        /// This method fetches all pickable area numbers from the <see cref="IAreaRepository"/> and sets them as the data source for the ComboBox.
        /// If there are any area numbers, it inserts a default "ALL" option at the beginning.
        /// The ComboBox is then set to the area number of the current workstation.
        /// </remarks>
        private void FillComboBoxAreaNumbers()
        {
            if (_areaRepository == null || _resourceManager == null || _workstationView == null)
            {
                throw new InvalidOperationException("Dependencies are not initialized.");
            }
            var areaNumbers = _areaRepository.GetAllPickableAreaNumbersAsString();
            if (areaNumbers.Any())
            {
                var allText = _resourceManager.GetString($"ALL") ?? "ALL"; // Fallback to "ALL" if resource is missing
                areaNumbers.Insert(0, allText);
            }
            ComboBoxAreaNumber.DataSource = areaNumbers;
            var selectedIndex = areaNumbers.IndexOf(_workstationView.Area.AreaNumber.ToString());
            ComboBoxAreaNumber.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0; // Default to the first item if not found
            //ComboBoxAreaNumber.SelectedIndex = areaNumbers.IndexOf(_workstationView.Area.AreaNumber.ToString());
        }

        private void ShowOrderComplete(Order order)
        {
            _logger.LogDetailAsync($"Show Order Complete Event: Order Number _ {order.Ord1} -- {order.Ord2}").SafeFireAndForget();
            var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == order.Id);
            if (bp == null) return;
            var pos = bp.PositionNumber.ToString();
            try
            {
                if (Controls.Count > 0)
                {
                    var c = Controls.Find("Pos" + pos + "Display", true).First();
                    if (c != null)
                    {
                        var panel = ((Panel)c);
                        panel.BackColor = Color.Green;
                        panel.Visible = true;
                        panel.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Control not Found {Environment.NewLine}{ex.Message}"
                    , "Control Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            bp.OrderComplete = true;
        }

        /// <summary>
        /// Configures the positions for picking orders based on the specified batch size and number of rows.
        /// </summary>
        /// <param name="pickBatchSize">The size of the batch for picking orders.</param>
        /// <param name="pickBatchRows">The number of rows for picking orders. Default is 1.</param>
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
            if (e.KeyCode != Keys.Delete) return;
            // convert _currentTextBoxPos.Tag to an int
            var batchPosition = int.Parse((string)_currentTextBoxPos.Tag);
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
        }

        //private void SetupPickPositions(int pickBatchSize, int pickBatchRows = 1)
        //{
        //    var rows = pickBatchRows;
        //    var positions = pickBatchSize;

        //    var panelManager = new PanelManager(PanelOrderPositions, PanelType.OrderSelection);
        //    PanelOrderPositions = panelManager.AddPositions(rows, positions);

        //    var inductionPanelManager = new PanelManager(PanelOrderInduction, PanelType.OrderInduction);
        //    PanelOrderInduction = inductionPanelManager.AddPositions(rows, positions);

        //    // Events

        //    foreach (var userControl in PanelOrderInduction.Controls.OfType<UserControlPickPosition>())
        //    {
        //        foreach (var panel in userControl.Controls.OfType<Panel>())
        //        {
        //            var textBox = panel.Controls.OfType<TextBox>().FirstOrDefault();
        //            if (textBox == null) continue;

        //            textBox.Click += TextBoxPos_Click;
        //            textBox.TextChanged += TextBoxPos_TextChanged;
        //            textBox.KeyPress += CheckKeyPress;
        //            textBox.Enter += (sender, e) => SetCurrentTextBoxPos(textBox);
        //        }
        //    }
        //}

        private void SetCurrentTextBoxPos(TextBox textBox)
        {
            _currentTextBoxPos = textBox;
            _currentTextBoxPos.Focus();
            _currentTextBoxPos.Select();
        }

        private async void TextBoxPos_TextChanged(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            var timeDifference = (currentTime - _lastKeystrokeTime).TotalMilliseconds;
            _lastKeystrokeTime = currentTime;

            if (timeDifference < ScannerThreshold)
            {
                // Handle scanner input
                //if (((TextBox)sender).Text.Length == 8)
                //{

                await HandleScannerInput((TextBox)sender);
                //}
            }
            else
            {
                // Handle keyboard input
                if (((TextBox)sender).Text.Length == 8)
                {



                    await HandleKeyboardInput((TextBox)sender);
                }

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
                    //case (char)Keys.Delete:
                    //    if (string.IsNullOrEmpty(textBox.Text))
                    //    {
                    //        if (int.TryParse(textBox.Tag?.ToString(), out var positionNumber))
                    //        {
                    //            if (_ordersToPick != null)
                    //            {
                    //                var batchPosition = _ordersToPick.FirstOrDefault(o => o.PositionNumber == positionNumber);
                    //                await ResetBatchPosition(batchPosition);
                    //                textBox.Focus();
                    //                _currentTextBoxPos = textBox;
                    //            }
                    //        }
                    //    }
                    //    break;
            }
        }
        private void SetLoaderButtonText()
        {
            if (GlobalVar.LoaderRunning)
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StopLoader");
                MBRunLoader.Enabled = false;
            }
            else
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"RunLoaderContinuously");
                MBRunLoader.Enabled = true;
            }
        }

        /// <summary>
        /// Sets the text of the upload button based on the current upload state.
        /// </summary>
        /// <remarks>
        /// If the upload is currently running, the button text will be set to indicate that the upload can be stopped.
        /// Otherwise, the button text will be set to indicate that the upload can be started continuously.
        /// </remarks>
        private void SetUploadButtonText()
        {
            if (GlobalVar.UploadRunning)
            {
                MBMainUpload.Text = _resourceManager.GetString($"StopUpload");
                MBRunUploadOnce.Enabled = false;
            }
            else
            {
                MBMainUpload.Text = _resourceManager.GetString($"RunUploadContinuously");
                MBRunUploadOnce.Enabled = true;
            }
        }

        //IptiButtonPressed Event Handler

        public async Task IptiButtonPickAccept(ResponseInfo responseInfo)
        {
            _logger.LogDetailAsync($"IptiButtonPickAccept Display Number:  {responseInfo.DisplayNumber}").SafeFireAndForget();
            await PickAccept();
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StopLoader");
                MBRunLoader.Enabled = false;
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"RunLoaderContinuously");
                MBRunLoader.Enabled = true;
                GlobalVar.LoaderRunning = false;
            }
        }

        private void StartStopUploadAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBMainUpload.Text = _resourceManager.GetString($"StopUpload");
                MBRunUploadOnce.Enabled = false;
                GlobalVar.UploadRunning = true;
            }
            else
            {
                MBMainUpload.Text = _resourceManager.GetString($"RunUploadContinuously");
                MBRunUploadOnce.Enabled = true;
                GlobalVar.UploadRunning = false;
            }
        }

        private void MBMainLoadOrders_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.LoaderRunning)
            {
                GlobalVar.LoaderRunning = true;
                Mediator.GetInstance().OnStartStopLoader(this, "Start");
            }
            else
            {
                GlobalVar.LoaderRunning = false;
                Mediator.GetInstance().OnStartStopLoader(this, "Stop");
            }
        }

        private void MBMainUpload_Click(object sender, EventArgs e)
        {
            if (!GlobalVar.UploadRunning)
            {
                GlobalVar.UploadRunning = true;
                Mediator.GetInstance().OnStartStopUpload(this, "Start");
            }
            else
            {
                GlobalVar.UploadRunning = false;
                Mediator.GetInstance().OnStartStopUpload(this, "Stop");
            }
        }

        private void MBRunLoader_Click(object sender, EventArgs e)
        {
            RunLoaderOnce();
        }

        private void RunLoaderOnce()
        {
            if (GlobalVar.LoaderRunning)
            {
                MessageBox.Show("Loader is already running.", "Loader Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                GlobalVar.LoaderRunning = true;
                Mediator.GetInstance().OnRunLoaderOnce(this);
                GlobalVar.LoaderRunning = false;
            }
        }

        private void MBRunUploadOnce_Click(object sender, EventArgs e)
        {
            RunUploadOnce();
        }

        private void RunUploadOnce()
        {
            if (GlobalVar.UploadRunning)
            {
                MessageBox.Show("Upload is already running.", "Upload Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                GlobalVar.UploadRunning = true;
                Mediator.GetInstance().OnRunUploadOnce(this);
                GlobalVar.UploadRunning = false;
            }
        }
        private void SetupPrinters()
        {
            _documentPrinterPreferences = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinterPreferences = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void FrmPick_Load(object sender, EventArgs e)
        {

            //Communication Monitoring Form use for TEsting

            // var frmCommunication = new FrmCommunication();
            // frmCommunication.Show();

            //if (GlobalVar.LoaderRunning)
            //{
            //    MBMainLoadOrders.Text = _resourceManager.GetString($"StopLoader");
            //}
            //if (GlobalVar.UploadRunning)
            //{
            //    MBMainUpload.Text = _resourceManager.GetString($"StopUpload");
            //}
        }

        // Set the focus to the passed in recId if it's passed in
        private int ShowAllOrders(int recId = 0)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAllOrders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,6,7,8,9";

            var views = _ordersRepository.GetOrderViews(orderStatus, findWhat);
            var bindingListView = new BindingListView<OrderView>(views.ToList());
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
                CurrentItem = ((ObjectView<OrderView>)_bindingSourceOrderView[recId]).Object;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void ShowAvailableStagingOrders(int recId = 0)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAllOrders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            _currentDataSet = CurrentDataSet.Available;
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "1,2,3,4,5,7,8,9";

            var views = _ordersRepository.GetAvailableOrderViews(orderStatus, findWhat);
            var bindingListView = new BindingListView<OrderView>(views.ToList());
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
                CurrentItem = ((ObjectView<OrderView>)_bindingSourceOrderView[recId]).Object;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private int ShowRackOrders(int recId = 0)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Show Rack Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            var views = _ordersRepository.GetRackOrders(findWhat);
            var bindingListView = new BindingListView<OrderView>(views.ToList());
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
                CurrentItem = ((ObjectView<OrderView>)_bindingSourceOrderView.Current).Object;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

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
            var views = _ordersRepository.GetReplenPickOrderViews(orderStatus, findWhat);



            var bindingListView = new BindingListView<OrderView>(views.ToList());
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
                CurrentItem = ((ObjectView<OrderView>)_bindingSourceOrderView.Current).Object;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowAvailableOrdersRack(int recId = 0, string findWhat = "")
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAvailableOrdersRack: [{DateTime.Now.ToLongTimeString()}]"));
            var idx = 0;

            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrdersRack.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetRackOrdersView(_workstationView.AreaId, findWhat);

                var rackOrderViews = views.ToList();
                foreach (var rackOrderView in rackOrderViews)
                {
                    if (rackOrderView.OrderDetails.First().LineStatusId == (int)LineStatus.Picking)
                    {
                        rackOrderView.StatusName = _resourceManager.GetString($"OnFloor");
                    }
                }
                var bindingListView = new BindingListView<RackOrderView>(rackOrderViews.ToList());

                _bindingSourceAvailableOrdersRack.DataSource = bindingListView;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"ShowAvailableOrdersRack Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }


            DataGridViewAvailableOrdersRack.DataSource = _bindingSourceAvailableOrdersRack;
            if (GetRecordCount(_bindingSourceAvailableOrdersRack) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrdersRack, recId);
                    DataGridViewAvailableOrdersRack.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrdersRack.Rows[idx].Index;
                    DataGridViewAvailableOrdersRack.CurrentCell = DataGridViewAvailableOrdersRack.Rows[idx].Cells[1];
                    DataGridViewAvailableOrdersRack.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrdersRack.ClearSelection();
                    DataGridViewAvailableOrdersRack.Update();
                }
                DataGridViewAvailableOrdersRack.Refresh();

                CurrentRackItem = ((ObjectView<RackOrderView>)_bindingSourceAvailableOrdersRack.Current).Object;

            }

            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAvailableOrdersRack End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void ShowAvailableOrders(int recId = 0, string searchField = "")
        {
            _logger.LogDetailAsync("ShowAvailableOrders START").SafeFireAndForget();
            var idx = 0;
            var serialPicking = _neutronVariables.SerialPicking;
            if (string.IsNullOrEmpty(searchField))
            {
                searchField = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetAvailableOrdersForInductionScreen(_workstationView.AreaId, searchField, serialPicking);

                _bindingListViewAvailableOrdersViews = new BindingListView<AvailableOrdersView>(views.ToList());
                _bindingSourceAvailableOrders.DataSource = _bindingListViewAvailableOrdersViews;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }

            DataGridViewAvailableOrders.DataSource = _bindingSourceAvailableOrders;

            if (GetRecordCount(_bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrders, recId);
                    if (idx >= 0 && idx < DataGridViewAvailableOrders.Rows.Count)
                    {
                        DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex =
                            DataGridViewAvailableOrders.Rows[idx].Index;
                        DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                        DataGridViewAvailableOrders.Rows[idx].Selected = true;
                    }
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                    DataGridViewAvailableOrders.Update();
                }

                //     CheckMarkSelectedAvailableOrders();
                //ClearTextBoxPosBackColor();
                //SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();
                if (_bindingSourceAvailableOrders.Current != null)
                {
                    _currentAvailableOrdersView =
                        ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                }
            }
            _logger.LogDetailAsync($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        }

        private int ShowAllAvailableOrders(int recId = 0, string findWhat = "")
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAllAvailableOrders: [{DateTime.Now.ToLongTimeString()}]"));
            var idx = 0;
            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetAvailableOrders(findWhat);

                var bindingListView = new BindingListView<OrderView>(views.ToList());

                _bindingSourceAvailableOrders.DataSource = bindingListView;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            }

            DataGridViewAvailableOrders.DataSource = _bindingSourceAvailableOrders;
            if (GetRecordCount(_bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrders, recId);
                    DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrders.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                    DataGridViewAvailableOrders.Update();
                }

                //   CheckMarkSelectedAvailableOrders();

                var index = SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();

                // _currentAvailableOrdersView = ((ObjectView<OrderView>)_bindingSourceAvailableOrders.Current).Object;

            }

            // ---         Task.Run(() => _logger.LogDetailAsync($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void CheckMarkSelectedAvailableOrders()
        {
            var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();

            foreach (var bp in batchPositions)
            {
                var row = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => Convert.ToInt32(r.Cells["Id"].Value) == bp.OrderId);
                if (row != null)
                {
                    row.Selected = true;
                    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                    chk.Value = chk.TrueValue;
                    break;
                }
            }
            //foreach (var bp in _ordersToPick)
            //{
            //    if (bp.OrderId == 0) continue;
            //    foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            //    {
            //        var id = Convert.ToInt32(row.Cells["Id"].Value);
            //        if (bp.OrderId != id) continue;
            //        var chk = (DataGridViewCheckBoxCell)row.Cells[0];
            //        chk.Value = chk.TrueValue;
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// Set the TextBox Position to the first empty position
        /// </summary>
        /// <returns>Current Position</returns>
        private int SetBatchPositionToFirstEmpty()
        {
            // Return -1 if there are no positions available
            var result = -1;
            // Loop over Orders to Pick to set the first on available

            // first empty using Linq
            var batchPosition = _ordersToPick.OrderBy(o => o.PositionNumber).FirstOrDefault(r => r.OrderId == 0);
            if (batchPosition != null)
            {
                SetCurrentTextBoxPos(batchPosition.PositionNumber);
                result = batchPosition.PositionNumber - 1;
            }
            return result;
        }

        public int IndexOf(BindingSource bindingSource, int value)
        {
            var count = bindingSource.Count;
            var itemIndex = 0;
            for (var i = 0; i < count; i++)
            {
                var rec = ((ObjectView<OrderView>)bindingSource[i]).Object.Id;
                if (rec != value) continue;
                itemIndex = i;
                break;
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
            if (bs == null) return 0;

            var count = bs.Count;
            var records = _resourceManager.GetString($"Records");
            LabelRecordCount.Text = $"{records}: {count}";
            return count;
        }

        public int IndexOf(BindingListView<OrderView> bs, int value)
        {
            var count = bs.Count;
            var itemIndex = -1;
            for (var i = 0; i < count; i++)
            {
                var rec = ((OrderView)bs[i]).Id;
                if (rec == value)
                {
                    itemIndex = i;
                    break;
                }
            }
            return itemIndex;
        }

        private int GetRecordCount(BindingListView<OrderView> bs)
        {
            var count = bs.Count;
            var records = _resourceManager.GetString($"Records");
            LabelRecordCount.Text = string.Format(format: "{0}: {1}", arg0: records, arg1: count.ToString());
            return count;
        }

        private void MButtonClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = Main;
        }

        //private void MButtonViewEdit_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = AvailableOrders;
        //}

        //private void MButtonNew_Click(object sender, EventArgs e)
        //{
        //    tabControl1.SelectedTab = PickScreen;
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
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _gridResourceManager.GetString($"Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _gridResourceManager.GetString($"Status"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "OrderStatusName",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            ////foreach (var pickStation in _pickStations)
            ////{
            //    var dataPropertyName = $"OrderStatus{_pickStations.First().StationNumber.ToString()}";
            //    col = new DataGridViewTextBoxColumn
            //    {
            //        DataPropertyName = "OrderStatus1",
            //        HeaderText = _pickStations.First().StationNumber.ToString(),
            //        DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
            //        Name = dataPropertyName,
            //        AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            //    };
            //    DataGridView1.Columns.Add(col);
            ////}


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_1_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_1_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_2_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_2_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_3_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-3"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_3_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_4_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-4"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_4_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_5_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-5"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_5_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_6_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-6"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_6_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_7_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-7"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_7_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Station_8_HasPicks",
                HeaderText = _gridResourceManager.GetString($"Area-8"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_8_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _gridResourceManager.GetString($"Lines"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _gridResourceManager.GetString($"Pieces"),
                Name = "Pieces",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _gridResourceManager.GetString($"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ShipMethodName",
                HeaderText = _gridResourceManager.GetString($"ShipMethod"),
                Visible = false,
                Name = "ShipMethodName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
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
        private void SetupSkipGrid(object state)
        {
            DataGridViewSkip.AutoGenerateColumns = false;
            DataGridViewSkip.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewSkip.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewSkip.DefaultCellStyle.BackColor = Color.White;
            DataGridViewSkip.ScrollBars = ScrollBars.Both;

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
            DataGridViewSkip.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString($"Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "AreaId"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _gridResourceManager.GetString($"Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item"
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _gridResourceManager.GetString($"Status"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "OrderStatusName",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _gridResourceManager.GetString($"Picked"),
                Name = "Picked",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _gridResourceManager.GetString($"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _gridResourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _gridResourceManager.GetString($"OrderDetailId"),
                Visible = false,
                Name = "OrderDetailId"
            };
            DataGridViewSkip.Columns.Add(col);

            DataGridViewSkip.EnableHeadersVisualStyles = false;
            DataGridViewSkip.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewSkip.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewSkip.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}

            _skipGridReady = true;
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
                HeaderText = _gridResourceManager.GetString($"Sequence"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Visible = false,
                Name = "Sequence"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickPosition",
                HeaderText = _gridResourceManager.GetString($"PickPosition"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PickPosition",
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Slot"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalQuantityInInventory",
                HeaderText = _gridResourceManager.GetString($"Total"),
                Name = "TotalQuantityInInventory",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _gridResourceManager.GetString($"ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            //col.DefaultCellStyle.Format = "{0:dd.MM.yyyy}";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _gridResourceManager.GetString($"OrderId"),
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
            DataGridViewAvailableOrders.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
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
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
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
                    HeaderText = _gridResourceManager.GetString($"Starter"),
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
                HeaderText = _gridResourceManager.GetString($"Priority"),
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
                HeaderText = _gridResourceManager.GetString($"Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _gridResourceManager.GetString($"Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Available",
                HeaderText = _gridResourceManager.GetString($"Available"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Available"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                HeaderText = _gridResourceManager.GetString($"Picked"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Picked"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Skipped",
                HeaderText = _gridResourceManager.GetString($"Skipped"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Skipped"
            };
            DataGridViewAvailableOrders.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OcLines",
                HeaderText = _gridResourceManager.GetString($"OcLines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "OcLines"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OcPieces",
                HeaderText = _gridResourceManager.GetString($"OcPieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "OcPieces"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Route",
                HeaderText = _gridResourceManager.GetString($"Route"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Route"
            };
            DataGridViewAvailableOrders.Columns.Add(col);



            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _gridResourceManager.GetString($"LoadDate"),
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
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
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StatusName",
                HeaderText = _gridResourceManager.GetString($"Status"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StatusName",
                Visible = true
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _gridResourceManager.GetString($"Priority"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Priority"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _gridResourceManager.GetString($"Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _gridResourceManager.GetString($"Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _gridResourceManager.GetString($"LoadDate"),
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
                HeaderText = _gridResourceManager.GetString($"Id"),
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
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                Name = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemDefinitionId",
                HeaderText = _gridResourceManager.GetString($"ItemDefinitionId"),
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
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _gridResourceManager.GetString($"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _gridResourceManager.GetString($"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _gridResourceManager.GetString($"Picked"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _gridResourceManager.GetString($"Status"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LineStatusName"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _gridResourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _gridResourceManager.GetString($"OrderDetailId"),
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
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId",
                ReadOnly = true

            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartNum",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PartNum",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartDesc",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "PartDesc",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _gridResourceManager.GetString($"Picked"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = false
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _gridResourceManager.GetString($"Status"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LineStatusName",
                Visible = false,
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _gridResourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _gridResourceManager.GetString($"OrderDetailId"),
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
                HeaderText = _gridResourceManager.GetString($"Area"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "AreaId"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _gridResourceManager.GetString($"Id"),
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
        private void SetupSkipInventoryGrid(object state)
        {
            DataGridViewSkipInventory.AutoGenerateColumns = false;
            DataGridViewSkipInventory.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewSkipInventory.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewSkipInventory.DefaultCellStyle.BackColor = Color.White;
            DataGridViewSkipInventory.ScrollBars = ScrollBars.Both;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AreaId",
                HeaderText = _gridResourceManager.GetString($"Area"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "AreaId"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageType",
                HeaderText = _gridResourceManager.GetString($"StorageType"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "StorageType"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _gridResourceManager.GetString($"Slot"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Slot"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _gridResourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Item"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _gridResourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _gridResourceManager.GetString($"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _gridResourceManager.GetString($"Picked"),
                Name = "Picked",
                ReadOnly = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Required",
                HeaderText = _gridResourceManager.GetString($"Required"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Required",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InventoryId",
                HeaderText = _gridResourceManager.GetString($"InventoryId"),
                Visible = false,
                Name = "InventoryId"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            DataGridViewSkipInventory.EnableHeadersVisualStyles = false;
            DataGridViewSkipInventory.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            DataGridViewSkipInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);

            //foreach (DataGridViewColumn column in DataGridViewSkipInventory.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            //}

            _skipInventoryGridReady = true;
        }


        #endregion

        private void FrmPick_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }


        /// <summary>
        /// Hides all tabs in the <see cref="TabControl"/> controls contained within the current form.
        /// </summary>
        /// <remarks>
        /// This method iterates through all controls of type <see cref="TabControl"/> within the form
        /// and hides their tabs.
        /// </remarks>
        private void HideTabControlTabs()
        {
            var tabControls = GetTabControls(this, typeof(TabControl));
            foreach (var control in tabControls)
            {
                var tabControl = (TabControl)control;
                HideTabs(tabControl);
            }
        }
        /// <summary>
        /// Hides the tabs of the specified <see cref="TabControl"/> by setting their appearance to flat buttons,
        /// adjusting their item size to zero width and one height, and clearing their text.
        /// </summary>
        /// <param name="tabControl">The <see cref="TabControl"/> whose tabs are to be hidden.</param>
        private void HideTabs(TabControl tabControl)
        {
            var zeroWidth = 0;
            var oneHeight = 1;
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(zeroWidth, oneHeight);
            tabControl.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                tabPage.Text = string.Empty;
            }
        }


        private IEnumerable<Control> GetTabControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();
            var enumerable = controls.ToList();
            return enumerable.SelectMany(c => GetTabControls(c, type)).Concat(enumerable).Where(c => c.GetType() == type);
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //var dgv = (DataGridView)sender;
            //if (e.RowIndex < 0) return;
            //var chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[0];
            //dgv.Rows[e.RowIndex].Cells[0].Value = chk.Value == chk.TrueValue ? chk.FalseValue : chk.TrueValue;


            //var dgv = sender as DataGridView;
            //if (e.RowIndex >= 0)
            //{
            //    var chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[0];

            //    if (chk.Value == chk.TrueValue)
            //    {
            //        dgv.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
            //    }
            //    else
            //    {
            //        dgv.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
            //    }
            //}
        }


        private void DataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            //if (vm.mySelectedItem != null && e.KeyCode == Key.Space)
            //{
            //    vm.MySelectedItem.IsChecked = !vm.MySelectedItem.IsChecked;
            //    e.Handled = true; //this is necessary because otherwise when the checkbox cell is selected, it will apply this keyup and also apply the default behavior for the checkbox
            // }
        }

        private void MBHold_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.OrderStatusId != (int)OrderStatus.Available) continue;
                    order.OrderStatusId = (int)OrderStatus.Hold;
                    _repoOrders.Update(order);
                    _historyManager.SaveHistory(ActionCode.HoldOrder, order);
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        if (orderDetail.LineStatusId != (int)LineStatus.Available) continue;
                        orderDetail.LineStatusId = (int)LineStatus.Hold;
                        _repoOrderDetails.Update(orderDetail);
                        _historyManager.SaveHistory(ActionCode.HoldLine, orderDetail);
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
        }

        private void MBRelease_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order.OrderStatusId != (int)OrderStatus.Hold) continue;
                    order.OrderStatusId = (int)OrderStatus.Available;
                    _repoOrders.Update(order);
                    _historyManager.SaveHistory(ActionCode.ReleaseOrder, order);
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        if (orderDetail.LineStatusId != (int)LineStatus.Hold) continue;
                        orderDetail.LineStatusId = (int)LineStatus.Available;
                        _repoOrderDetails.Update(orderDetail);
                        _historyManager.SaveHistory(ActionCode.ReleaseLine, orderDetail);
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
        }
        private List<Order> GetSelectedOrders(DataGridView dataGridView)
        {
            var orders = new List<Order>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var orderId = (int)row.Cells["Id"].Value;
                var order = _ordersRepository.GetOrder(orderId);
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

        private List<SkipView> GetSelectedPickViews(DataGridView dataGridView)
        {
            var recs = new List<SkipView>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var rec = ((ObjectView<SkipView>)row.DataBoundItem).Object;
                recs.Add(rec);
            }
            if (!recs.Any())
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
            }
            return recs;
        }

        //private List<OrderDetail> GetSelectedOrderDetails(DataGridView dataGridView)
        //{
        //    var orderDetails = new List<OrderDetail>();
        //    foreach (DataGridViewRow row in dataGridView.SelectedRows)
        //    {
        //        var orderDetailId = (int)row.Cells["Id"].Value;
        //        var orderDetail = _orderDetailsRepository.GetOrderDetail(orderId);
        //        if (orderDetail != null)
        //        {
        //            orderDetails.Add(orderDetail);
        //        }
        //    }
        //    if (!orderDetails.Any())
        //    {
        //        MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
        //    }
        //    return orderDetails;
        //}

        private async void MBPickListBack_Click(object sender, EventArgs e)
        {
           // await ExecutePickBackProcess();
            await PickListBack();
        }

        private Task PickListBack()
        {
            ShowAvailableStagingOrders();
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            // NextButtonEnabled();
            tabControl1.SelectedTab = AvailableOrders;
            return Task.CompletedTask;
        }

        private void MBBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = OrderListing;
        }

        private async void MBAvailableOrdersBack_Click(object sender, EventArgs e)
        {
            // if there are any orders not complete change them back to Available
            await AvailableOrdersBack();
        }

        private async Task AvailableOrdersBack()
        {
            await ClearIptiDisplayFunctions();
            ResetHanelDeviceStatus();
            ClearProLites();
            ClearActiveDeviceIndicators();

            LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = Main;
        }

        private async void MBGo_Click(object sender, EventArgs e)
        {

            MBPickAccept.Visible = false;
            // if (await CheckForValidOrders() == false) return;

            //ClearOrderPositions();

            _spaceBarDisabled = true;
            //DisableNextButtons();
            MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
            Cursor.Current = Cursors.WaitCursor;
            await Go();
            Cursor.Current = Cursors.Default;
            _spaceBarDisabled = false;
            //EnableNextButtons();
        }

        //private void DisableNextButtons()
        //{
        //    MBGo.Enabled = false;
        //    MBGo2.Enabled = false;
        //}

        //private void EnableNextButtons()
        //{
        //    MBGo.Enabled = true;
        //    MBGo2.Enabled = true;
        //}

        private async Task Go()
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Go Batch START Before FinalCheckOfOrdersToPick"));

            //if (FinalCheckOfOrdersToPick() == 0) return;


            // ---         Task.Run(() => _logger.LogDetailAsync($"Go Batch START after FinalCheckOfOrdersToPick"));
            _currentBatchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();

            TextBoxFindAvailableOrders.Text = string.Empty;
            try
            {

                var numOrders = await FinalCheckOfOrdersToPick(); // _ordersToPick.Count(o => o.OrderId != 0);
                if (numOrders > 0)
                {
                    LabelFormTitle.Text = _resourceManager.GetString($"PickList");
                    LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);

                    var pickableViews = await PickListLoad();

                    if (pickableViews.Count > 0)
                    {
                        _bindingSourcePickViews.DataSource = pickableViews;
                        DataGridPickView.DataSource = _bindingSourcePickViews;

                        // ---         Task.Run(() => _logger.LogDetailAsync($"bindingSourcePickViews Count:{_bindingSourcePickViews.Count.ToString()}"));
                        GetRecordCount(_bindingSourcePickViews);
                        // Check for items with insufficient inventory
                        var shortItems = GetShortItems(pickableViews);
                        // If there are any short items then show the PickListShortItemReport
                        if (shortItems.Any())
                        {
                            tabControl1.SelectedTab = PickList;
                            MBStart.Focus();
                            // ---         Task.Run(() => _logger.LogDetailAsync($"Opening Pick List  Short Item Report"));
                        }
                        else
                        {
                            // At this point we have a list of PickViews that have sufficient inventory
                            // and we have checked for short items
                            // so we can go ahead and start the batch
                            await Start(pickableViews);
                        }
                    }
                    else
                    {
                        MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
                        ClearAllSelectOrdersToPick();
                        await ClearBatchPositions(_currentBatchPositions);
                        LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                        LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
                        await AvailableOrdersScreen();
                    }
                }
                else
                {
                    // ---         Task.Run(() => _logger.LogDetailAsync($"No Orders Selected"));
                    MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
                    ClearAllSelectOrdersToPick();
                    await ClearBatchPositions(_currentBatchPositions);
                    LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                    LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
                    await AvailableOrdersScreen();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            // ---         Task.Run(() => _logger.LogDetailAsync($"Go Batch START Complete"));
        }

        private TextBox GetTextBoxPosWithFocus()
        {
            if (InvokeRequired)
            {
                return (TextBox)Invoke(new Func<TextBox>(GetTextBoxPosWithFocus));
            }

            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var controls = Controls.Find($"TextBoxPos{i + 1}", true);
                if (!controls.Any()) continue;
                var control = controls.First();
                var textBox = (TextBox)control;
                if (textBox == null) continue;
                if (textBox.Focused)
                {
                    return textBox;
                }
            }

            return _currentTextBoxPos;
        }


        /// <summary>
        /// Check to see if TextBoxPosx.Text doesn't match OrdersToPick.Order
        /// Clear invalid OrdersToPick
        /// </summary>
        private async Task<int> FinalCheckOfOrdersToPick()
        {
            // loop over all TextBoxPosx
            // and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            // Clear invalid OrdersToPick
            var validOrdersToPick = 0;
            _logger.LogDetailAsync($"[{DateTime.Now}]  Start FinalCheckOfOrdersToPick").SafeFireAndForget();
            try
            {
                //ClearBatchPositions();

                foreach (var bp in _ordersToPick)
                {
                    var pos = bp.PositionNumber.ToString();

                    var controls = Controls.Find($"TextBoxPos{pos}", true);
                    if (!controls.Any()) continue;
                    var control = controls.First();
                    var textBox = (TextBox)control;
                    if (textBox == null) continue;
                    var order = textBox.Text.Trim();

                    //Is this a real order
                    if (string.IsNullOrEmpty(order) || bp.OrderId == 0) continue;
                    var isRealOrder = _repoOrders.FindByKey(bp.OrderId);
                    if (isRealOrder != null)
                    {
                        // compare isRealOrder.Ord1 and isRealOrder.Ord2 to bp.Ord1 and bp.Ord2
                        var isMatch = isRealOrder.Ord1 == bp.Ord1 && isRealOrder.Ord2 == bp.Ord2;
                        if (!isMatch)
                        {
                            MessageBox.Show(
                                $"The Delivery {order} in Position {pos} does not match the expected order.");
                            Mediator.GetInstance().OnDisplayMessage(this, $"The Delivery {order} in Position {pos} does not exist.");

                            await ClearItemFromBatchByPosition(bp.PositionNumber);
                        }
                        else
                        {
                            // This is a valid order
                            validOrdersToPick++;
                            // create the Batch Position
                            bp.OrderId = isRealOrder.Id;
                            bp.Ord1 = isRealOrder.Ord1;
                            bp.Ord2 = isRealOrder.Ord2;
                            bp.OrderComplete = false;
                        }
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
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error: {ex.Message}").SafeFireAndForget();
                Mediator.GetInstance().OnDisplayMessage(this, $"The Final Check of Orders to Pick has failed. {Environment.NewLine}{ex.Message}");

                return 0;
            }
            _logger.LogDetailAsync($"[{DateTime.Now}]  End FinalCheckOfOrdersToPick  {validOrdersToPick} valid orders.").SafeFireAndForget();
            return validOrdersToPick;
        }

        /// <summary>
        /// Checks for valid orders in the _ordersToPick list.
        /// </summary>
        /// <remarks>
        /// This method loops over all TextBoxPosx and checks for TextBoxPosx.Text that doesn't match OrdersToPick.Order.
        /// If an invalid order is found, it is cleared from the OrdersToPick list.
        /// </remarks>
        /// <returns>
        /// Returns true if a valid order is found, otherwise false.
        /// </returns>
        /// <exception cref="Exception">Thrown when the check for valid orders fails.</exception>
        private async Task<bool> CheckForValidOrders()
        {
            // loop over all TextBoxPosx
            // and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            // Clear invalid OrdersToPick
            // _logger.LogDetailAsync($"Start").SafeFireAndForget();
            var validOrders = 0;
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
                        if (string.IsNullOrEmpty(order) || bp.OrderId == 0) continue;
                        var isRealOrder = _repoOrders.FindByKey(bp.OrderId);
                        if (isRealOrder != null)
                        {
                            // compare isRealOrder.Ord1 and isRealOrder.Ord2 to bp.Ord1 and bp.Ord2
                            var isMatch = isRealOrder.Ord1 == bp.Ord1 && isRealOrder.Ord2 == bp.Ord2;
                            if (!isMatch)
                            {
                                MessageBox.Show(
                                    $"The Delivery {order} in Position {pos} does not match the expected order.");
                                await ClearItemFromBatchByPosition(bp.PositionNumber);
                            }

                            validOrders++;
                            continue;
                        }

                        MessageBox.Show($"The Delivery {order} in Position {pos} does not exist.");
                        await ClearItemFromBatchByPosition(bp.PositionNumber);
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

            _logger.LogDetailAsync($"End: {validOrders} Valid Orders.").SafeFireAndForget();
            return validOrders > 0;

        }

        //private void DumpOrdersToPick(int lineNumber)
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine("DUMP ORDERS TO PICK");
        //    foreach (var bp in _ordersToPick)
        //    {
        //        sb.AppendLine(
        //            $"Pos:{bp.PositionNumber} ID:{bp.OrderId} Order:{bp.Ord1} Invoice:{bp.Ord2} Complete:{bp.OrderComplete}");
        //    }
        //    _logger.LogDetailAsync($"[{DateTime.Now}]  Line Number: {lineNumber} {Environment.NewLine}{sb}").SafeFireAndForget();
        //}

        //private void UpdateOrdersToPickingStatus(List<BatchPosition> ordersToPick)
        //{
        //    var orderIds = ordersToPick.Where(r => r.OrderId != 0).ToList();

        //    foreach (var batchPosition in orderIds)
        //    {
        //        var orderDetails = _repoOrderDetails.FindBy(r =>
        //            r.OrderId == batchPosition.OrderId && r.StationNumber == _workstationView.StationNumber &&
        //            r.LineStatusId == (int) LineStatus.Available);
        //        foreach (var orderDetail in orderDetails)
        //        {
        //            orderDetail.LineStatusId = (int) LineStatus.Picking;
        //            _repoOrderDetails.Update(orderDetail);
        //        }
        //    }
        //}

        /// <summary>
        /// Creates a list of PickViews that will be short
        /// It doesn't mean that some of the items can't be picked
        /// It just means theres' not enough inventory for all to be picked
        /// </summary>
        /// <param name="pickableViews"></param>
        /// <returns></returns>
        private List<PickView> GetShortItems(List<PickView> pickableViews)
        {
            var shortItems = new List<PickView>();

            foreach (var item in pickableViews)
            {
                if (item.QuantityToBePicked > item.TotalQuantityInInventory)
                {
                    shortItems.Add(item);
                }
            }
            return shortItems;
        }

        /// <summary>
        /// Loads the Inventory for just the current PickViews into the CurrentInventory variable
        /// </summary>
        /// <param name="pickViews"></param>
        private void LoadInventoryForPickViews(List<PickView> pickViews)
        {
            // get the areaId from the WorkstationView
            var areaId = _workstationView.AreaId;

            _logger.LogDetailAsync($"{DateTime.Now}  Load Inventory For PickViews START").SafeFireAndForget();
            // get a distinct list of ItemIds from the pickviews
            var itemIds = pickViews.Select(r => r.ItemId).Distinct().ToList();

            // get the current inventory in this area for all the distinct items in the pickviews
            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => itemIds.Contains(f.ItemDefinitionId) && f.AreaId == areaId).ToList();

            _logger.LogDetailAsync($"{DateTime.Now}  Load Inventory For PickViews END").SafeFireAndForget();
        }

        /// <summary>
        /// Load current Inventory based on Pickable Locations (StorageTypes)
        /// </summary>
        private void LoadInventory()
        {
            _logger.LogDetailAsync($"{DateTime.Now}  Load Inventory START").SafeFireAndForget();
            var pickableLocations = _repoStorageTypes.FindBy(r => r.Pickable == true).Select(r => r.Id).ToList();  // new int[] { 1, 2 };  // 4 is an EBin
                                                                                                                   // var pickableLocations = new int[] { 1, 2 };  // 4 is an EBin
            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => pickableLocations.Contains(f.StorageTypeId)).ToList();
            _logger.LogDetailAsync($"{DateTime.Now}  Load Inventory END").SafeFireAndForget();
        }

        private async Task<List<PickView>> PickListLoad()
        {
            _logger.LogDetailAsync($"{DateTime.Now}  Pick List Load Start").SafeFireAndForget();
            // Get the PickViews for the OrdersToPick with added ItemKey
            var pickViews = GetPickViews();
            // ---         Task.Run(() => _logger.LogDetailAsync($"Pickviews Count: {pickViews.Count}"));

            var pickableViews = new List<PickView>();
            var skipPickableViews = new List<PickView>();
            var zeroPickableViews = new List<PickView>();

            // Loads the currentInventory for just the working PickViews
            LoadInventoryForPickViews(pickViews);
            _logger.LogDetailAsync($"{DateTime.Now}  Load Inventory for PickViews Complete").SafeFireAndForget();
            foreach (var item in pickViews)
            {
                var orderDetail = item.OrderDetail;

                await SetOrderDetailStatusToPicking(orderDetail);

                //orderDetail.LineStatusId = (int)LineStatus.Picking;
                //_repoOrderDetails.Update(orderDetail);

                List<Inventory> exactInventorySequence;
                if (item.Ord1.Equals("TRANSFER", StringComparison.CurrentCultureIgnoreCase))
                {
                    exactInventorySequence = Transfer(item);
                }
                else
                {
                    switch (_neutronVariables.PickMethod)
                    {
                        case "RadioButtonPrimeBinFirst":
                            exactInventorySequence = PrimeBinFirst(item);
                            break;
                        case "RadioButtonPrimeBinLast":
                            exactInventorySequence = PrimeBinLast(item);
                            break;
                        case "RadioButtonFifo":
                            exactInventorySequence = Fifo(item);
                            break;
                        case "RadioButtonLifo":
                            exactInventorySequence = Lifo(item);
                            break;
                        default:
                            exactInventorySequence = Fifo(item);
                            break;
                    }
                }

                if (exactInventorySequence.Count > 0)
                {
                    item.CurrentInventoryLocation = exactInventorySequence.First();
                    item.Inventory = exactInventorySequence;
                    item.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    item.Slot = item.CurrentInventoryLocation.Location.Slot;
                    item.SlotQty = item.TotalQuantityInInventory;
                    item.InventoryIndex = 0;
                    item.ReceivedDate = item.CurrentInventoryLocation.ReceivedDate;
                    pickableViews.Add(item);
                }
                else // no Inventory open Skip Option Form
                {
                    using (var form = new FrmSkipOption(item))

                    {
                        var result = form.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            var pushed = form.Pushed;
                            if (pushed == "Skip")
                            {
                                //item.OrderDetail.LineStatusId = (int)LineStatus.Skipped;

                                item.OrderDetail.LineStatusId = (int)LineStatus.Hold;
                                skipPickableViews.Add(item);
                                await _repoOrderDetails.UpdateAsync(item.OrderDetail);
                                //_historyManager.SaveHistory(ActionCode.Skip, item.OrderDetail, item.OrderDetail.AreaId);
                                _historyManager.SaveHistory(ActionCode.HoldLine, item.OrderDetail, item.OrderDetail.AreaId);
                            }
                            else if (pushed == "Pick Zero")
                            {
                                zeroPickableViews.Add(item);
                                item.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                                item.OrderDetail.PickedQuantity = 0;
                                await _repoOrderDetails.UpdateAsync(item.OrderDetail);
                                // ---         Task.Run(() => _logger.LogDetailAsync($"PickListLoad Pick Zero Option Ord:{item.Ord1}   Res:{item.Ord2}   Item:{item.Item}"));
                                // ---         Task.Run(() => _logger.LogDetailAsync($"PickListLoad SaveHistory Ord:{item.Ord1}   Res:{item.Ord2}   Item:{item.Item}"));
                                _historyManager.SaveHistory(ActionCode.PickOrder, item.OrderDetail, item.OrderDetail.AreaId);
                                if (item.OrderDetail.Order != null)
                                {
                                    await CheckForOrderComplete(item.OrderDetail.Order);
                                }
                            }
                        }
                    }
                }
            }

            CreateSkipZeroSummary(skipPickableViews, zeroPickableViews);
            _logger.LogDetailAsync($"{DateTime.Now}  PickListLoad End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();

            return pickableViews;
        }

        private async Task SetOrderDetailStatusToAvailable(OrderDetail orderDetail)
        {
            orderDetail.LineStatusId = (int)LineStatus.Available;
            await _repoOrderDetails.UpdateAsync(orderDetail);
            var order = _repoOrders.FindByKey(orderDetail.OrderId);
            order.OrderStatusId = (int)OrderStatus.Available;
            await _repoOrders.UpdateAsync(order);
        }

        private async Task SetOrderDetailStatusToPicking(OrderDetail orderDetail)
        {
            orderDetail.LineStatusId = (int)LineStatus.Picking;
            await _repoOrderDetails.UpdateAsync(orderDetail);
            var order = _repoOrders.FindByKey(orderDetail.OrderId);
            order.OrderStatusId = (int)OrderStatus.Picking;
            await _repoOrders.UpdateAsync(order);
        }

        private void CreateSkipZeroSummary(List<PickView> skipPickableViews, List<PickView> zeroPickableViews)
        {
            if (skipPickableViews.Count <= 0 && zeroPickableViews.Count <= 0) return;
            var frm = new FrmSkipZeroSummary(skipPickableViews, zeroPickableViews);
            frm.ShowDialog();
        }

        private List<Inventory> Transfer(PickView item)
        {
            var inventorySequence = new List<Inventory>();
            var recs = GetTransferInventory(item.ItemId);
            // ---         Task.Run(() => _logger.LogDetailAsync($"Transfer Inventory Rec Count:  {recs.Count}"));
            if (recs.Count <= 0) return inventorySequence;

            //sequence the inventory Recs by Received Date
            var sortedRecs = recs.OrderBy(o => o.ReceivedDate);
            inventorySequence.AddRange(sortedRecs);
            return inventorySequence;
        }

        private List<Inventory> PrimeBinFirst(PickView item)
        {
            _logger.LogDetailAsync($"{DateTime.Now} Prime Bin First Item:  {item.Item}").SafeFireAndForget();
            List<Inventory> sortedRecs;
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            _logger.LogDetailAsync($"{DateTime.Now} Prime Bin First Inventory Rec Count:  {recs.Count}").SafeFireAndForget();

            if (recs.Count <= 0) return inventorySequence;
            if (recs.Count == 1) return recs;

            //if there is a prime bin make it first, remove it from the list of inventory locations
            // var prime = recs.FirstOrDefault(r => r.Location.Slot == item.OrderDetail.PrimeBin);
            var prime = recs.FirstOrDefault(r => r.PrimeBin);  // .Location.Slot == item.OrderDetail.PrimeBin);
            if (prime != null)
            {
                // ---         Task.Run(() => _logger.LogDetailAsync($"recs Add Prime [{DateTime.Now.ToLongTimeString()}] "));
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
            _logger.LogDetailAsync($"{DateTime.Now} Prime Bin First Inventory Extra Processing Item:  {item.Item}").SafeFireAndForget();
            return inventorySequence;
        }

        private List<Inventory> PrimeBinLast(PickView item)
        {

            List<Inventory> sortedRecs;
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            // ---         Task.Run(() => _logger.LogDetailAsync($"1512 Prime Bin Last Inventory Rec Count:  {recs.Count}"));
            if (recs.Any())
            {
                //if there is a prime bin make it first, remove it from the list of inventory locations
                var prime = recs.FirstOrDefault(r => r.Location.Slot == item.OrderDetail.PrimeBin);
                if (prime != null)
                {
                    recs.Remove(prime);
                }


                if (_workstationView.AreaId == AreaEight)
                {
                    // sort the recs by the Location.PickSequence
                    sortedRecs = recs.OrderBy(r => r.Location.PickSequence).ToList();
                }
                else
                {
                    //sequence the inventory Recs by Received Date Decending
                    sortedRecs = recs.OrderBy(o => o.ReceivedDate).ToList();
                }

                inventorySequence.AddRange(sortedRecs);

                if (prime != null)
                {
                    inventorySequence.Add(prime);
                }
            }
            return inventorySequence;
        }

        private List<Inventory> Fifo(PickView item)
        {

            List<Inventory> sortedRecs;
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            // ---         Task.Run(() => _logger.LogDetailAsync($"1536 FIFO Inventory Rec Count:  {recs.Count}"));
            if (recs.Count > 0)
            {
                //sequence the inventory Recs by Received Date


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
            }
            return inventorySequence;
        }

        private List<Inventory> Lifo(PickView item)
        {

            List<Inventory> sortedRecs;

            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            // ---         Task.Run(() => _logger.LogDetailAsync($"1553 LIFO Inventory Rec Count:  {recs.Count}"));
            if (recs.Count > 0)
            {
                if (_workstationView.AreaId == AreaEight)
                {
                    // sort the recs by the Location.PickSequence
                    sortedRecs = recs.OrderBy(r => r.Location.PickSequence).ToList();
                }
                else
                {
                    //sequence the inventory Recs by Received Date Descending
                    sortedRecs = recs.OrderByDescending(o => o.ReceivedDate).ToList();
                }

                inventorySequence.AddRange(sortedRecs);
            }
            return inventorySequence;
        }

        private List<PickView> GetPickViews()
        {
            _logger.LogDetailAsync($"{DateTime.Now}  Get Pick Views START").SafeFireAndForget();
            var prevPartNum = "";
            //    Order orderAndDetails = new Order();
            var pickViews = new List<PickView>();

            try
            {
                var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();

                foreach (var bp in batchPositions)
                {
                    if (bp.OrderId == 0) continue;
                    // find the Index of the Order in the AvailableOrders BindingSource
                    var orderIndex = _bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                    // set the Position of the BindingSource to the Index of the Order
                    if (orderIndex > -1) _bindingSourceAvailableOrders.Position = orderIndex;
                    // get the current Order
                    var currentOrder = ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                    // set the firstTime flag to true to indicate this is the first time through the loop
                    // for this Order
                    var firstTime = true;
                    // counter is used to make each line of OrderDetails unique so that an order with the same item
                    // will be picked separately
                    var counter = 0;
                    // get the Order and OrderDetails for the current Order and Area
                    var orderAndDetails = _ordersRepository.GetOrderWithOrderDetails(bp.OrderId, _workstationView.AreaId);

                    //var order = _ordersRepository.GetOrder(bp.OrderId);
                    //var orderDetails = _ordersRepository.GetOrderDetailsByOrderAndArea(bp.OrderId, _workstationView.AreaId);
                    //foreach (var orderDetail in orderDetails)
                    //{
                    //    orderDetail.Order = orderAndDetails;
                    //}
                    //orderAndDetails.OrderDetails = orderDetails;

                    // set the Order property of the currentOrder to the Order and OrderDetails
                    currentOrder.Order = orderAndDetails;
                    // get the OrderDetails for the current Order
                    //var details = currentOrder.Order.OrderDetails.OrderBy(o => o.PartNum).ToList();


                    // loop over the OrderDetails
                    foreach (var detail in orderAndDetails.OrderDetails)
                    {
                        //if (detail.LineStatusId != (int)LineStatus.Available &&
                        //    detail.LineStatusId != (int)LineStatus.Skipped) continue;
                        //key builder makes each line of orderdetails unique so that an order with the same item
                        // will be picked separately
                        // PickStops will be grouped by key, not item number
                        detail.Order = orderAndDetails;
                        var key = "";
                        if (firstTime)
                        {
                            // if this is the first time through the loop for this Order
                            // set the prevPartNum to the current OrderDetail.PartNum
                            prevPartNum = detail.PartNum.Trim();
                            // set the key to the current OrderDetail.PartNum
                            key = detail.PartNum.Trim();
                            // set the firstTime flag to false to indicate this is not the first time through the loop
                            firstTime = false;
                        }
                        // if the prevPartNum is the same as the current OrderDetail.PartNum
                        else if (prevPartNum == detail.PartNum.Trim())
                        {
                            // increment the counter
                            counter++;
                            // set the key to the current OrderDetail.PartNum + the counter
                            key = $"{detail.PartNum.Trim()}{counter}";
                        }
                        // else prevPartNum != detail.PartNum
                        else //prevPartNum != detail.PartNum
                        {
                            prevPartNum = detail.PartNum.Trim();
                            // set the key to the current OrderDetail.PartNum
                            key = detail.PartNum.Trim();
                            // reset the counter to 0
                            counter = 0;
                        }


                        var unitOfIssue = _repoItemDefinition.FindBy(f => f.Id == detail.ItemDefinitionId).FirstOrDefault()?.UnitOfIssue.Name;


                        // create a new PickView and populate it with the OrderDetail
                        // and other information
                        // the ItemKey is used to make each line of OrderDetails unique so that an order with the same item
                        // will be picked separately
                        var pickView = new PickView()
                        {
                            PickPosition = bp.PositionNumber,
                            OrderId = detail.OrderId,
                            Ord1 = detail.Order.Ord1,
                            Ord2 = detail.Order.Ord2,
                            ItemId = detail.ItemDefinitionId,
                            Item = detail.PartNum.Trim(),
                            Description = detail.PartDesc.Trim(),
                            UnitOfIssue = unitOfIssue,
                            Quantity = detail.Quantity,
                            QuantityToBePicked = detail.Quantity,
                            PickedQty = detail.PickedQuantity,
                            Slot = string.Empty,
                            SlotQty = 0,
                            OrderDetail = detail,
                            AreaId = detail.AreaId,
                            PreviousLineStatusId = detail.LineStatusId,
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

            _logger.LogDetailAsync($"{DateTime.Now}  GetPickViews End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return pickViews;
        }

        private int GetBatchPosition(int orderId)
        {
            var result = -1;
            //foreach (var bp in _ordersToPick)
            //{
            var bp = _ordersToPick.FirstOrDefault(o => o.OrderId == orderId);
            //if (bp.OrderId != orderId) continue;
            if (bp != null)
            {
                result = bp.PositionNumber;
            }
            //break;
            // }
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

        private PickView CreatePickView(int pos, OrderDetail detail)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"CreatePickView Start: [{DateTime.Now.ToLongTimeString()}]"));
            var pickView = new PickView()
            {
                PickPosition = pos,
                OrderId = detail.Order.Id,
                Ord1 = detail.Order.Ord1,
                Ord2 = detail.Order.Ord2,
                ItemId = detail.ItemDefinitionId,
                Item = string.Empty,
                Description = string.Empty,
                Quantity = detail.Quantity,
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

            List<Inventory> exactInventorySequence;
            switch (_neutronVariables.PickMethod)
            {
                case "RadioButtonPrimeBinFirst":
                    exactInventorySequence = PrimeBinFirst(pickView);
                    break;
                case "RadioButtonPrimeBinLast":
                    exactInventorySequence = PrimeBinLast(pickView);
                    break;
                case "RadioButtonFifo":
                    exactInventorySequence = Fifo(pickView);
                    break;
                case "RadioButtonLifo":
                    exactInventorySequence = Lifo(pickView);
                    break;
                default:
                    exactInventorySequence = Fifo(pickView);
                    break;
            }

            pickView.CurrentInventoryLocation = exactInventorySequence.First();
            pickView.Inventory = exactInventorySequence;
            pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
            pickView.Slot = pickView.CurrentInventoryLocation.Location.Slot;
            pickView.SlotQty = pickView.TotalQuantityInInventory;
            pickView.InventoryIndex = 0;
            pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;

            _logger.LogDetailAsync($"CreatePickView End: [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            return pickView;
        }

        private List<Inventory> GetTransferInventory(int itemId)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Get Transfer Inventory Item: {itemId} START"));
            var recs = _currentInventory.FindAll(r => r.ItemDefinitionId == itemId && r.PrimeBin == false && r.AreaId == AreaEight);
            // ---         Task.Run(() => _logger.LogDetailAsync($"Get Transfer Inventory Item: {itemId} END"));
            return recs;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            _logger.LogDetailAsync($"{DateTime.Now}  Get Inventory Item: {itemId} START").SafeFireAndForget();
            var recs = _currentInventory.FindAll(r => r.ItemDefinitionId == itemId);
            //var pickableLocations = new[] { 1, 2 };
            //recs = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
            //    .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            _logger.LogDetailAsync($"{DateTime.Now}  Get Inventory Item: {itemId} END").SafeFireAndForget();
            return recs;
        }

        //private void FrmPick_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (!CloseButtonPressed)
        //    {
        //        e.Cancel = true;
        //        return;
        //    }
        //}

        private void RefreshDataGridRows()
        {
            foreach (var batchPosition in _ordersToPick)
            {
                if (batchPosition.OrderId != 0)
                {
                    // how do I find a row in a DataGridView when I know a value in one of the columns
                    var row = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.Cells["Ord1"].Value.ToString() == batchPosition.Ord1
                        && r.Cells["Ord2"].Value.ToString() == batchPosition.Ord2);
                    if (row != null)
                    {
                        DataGridViewAvailableOrders.Rows[row.Index].DefaultCellStyle.BackColor = Color.LawnGreen;
                    }

                }
                //else // OrderId does equal 0, so set the backcolor to white
                //{
                //    var row = DataGridViewAvailableOrders.Rows.Cast<DataGridViewRow>()
                //        .FirstOrDefault(r => r.Cells["OrderId"].Value.ToString() == "0");
                //    if (row != null)
                //    {
                //        DataGridViewAvailableOrders.Rows[row.Index].DefaultCellStyle.BackColor = Color.White;
                //    }
                //}
            }
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
        /// Handles the <see cref="DataGridView.CellClick"/> event of the <see cref="DataGridViewAvailableOrders"/> control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the <see cref="DataGridView"/>.</param>
        /// <param name="e">A <see cref="DataGridViewCellEventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// This method is responsible for handling cell click events in the DataGridView.
        /// It performs actions such as retrieving the order ID and order value based on the clicked row,
        /// and asynchronously adding or removing the order from the induction screen.
        /// </remarks>
        private async void DataGridViewAvailableOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            var ord1 = GetOrd1(e.RowIndex);
            _currentTextBoxPos.Text = ord1;
            await ProcessInput(_currentTextBoxPos);
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

        /// <summary>
        /// Retrieves the value of a specified column from the DataGridViewAvailableOrders at a given row index.
        /// </summary>
        /// <param name="rowIndex">The index of the row from which to retrieve the value.</param>
        /// <param name="columnName">The name of the column from which to retrieve the value.</param>
        /// <returns>The value of the specified column at the given row index as a string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the rowIndex is out of the valid range.</exception>
        /// <exception cref="ArgumentException">Thrown when the columnName does not exist in the DataGridViewAvailableOrders.</exception>
        private string GetOrderValue(int rowIndex, string columnName)
        {
            return DataGridViewAvailableOrders.Rows[rowIndex].Cells[columnName].Value.ToString();
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
            _logger.LogDetailAsync($"2-1 Start AddRemoveOrderFromInductionScreen.").SafeFireAndForget();
            if (orderId == 0) return -1;
            _logger.LogDetailAsync($"2-2 OrderId does not equal zero.").SafeFireAndForget();
            var batchPosition = _ordersToPick.FirstOrDefault(r => r.OrderId == orderId);
            if (batchPosition != null)
            {
                _logger.LogDetailAsync($"2-3 Got BatchPosition Ord1: {batchPosition.Ord1} ").SafeFireAndForget();

                DataGridViewAvailableOrders.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
                _logger.LogDetailAsync($"2-4 Set Backcolor to White.").SafeFireAndForget();
                await RemoveOrderFromInductionScreen(batchPosition);
                _logger.LogDetailAsync($"Back From RemoveOrderFromInductionScreen").SafeFireAndForget();
                return -1;
            }
            _logger.LogDetailAsync($"2-5 BatchPosition is NULL").SafeFireAndForget();
            var order = _repoOrders.FindByKey(orderId);
            if (order == null)
            {
                _logger.LogDetailAsync($"2-6 Back from getting Order is null.").SafeFireAndForget();
                return -1;
            }
            _logger.LogDetailAsync($"2-7 Back from getting Order: {order.Ord1}").SafeFireAndForget();

            DataGridViewAvailableOrders.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LawnGreen;
            _logger.LogDetailAsync($"2-8 Set Backcolor to Green.").SafeFireAndForget();
            var idx = await AddOrderToInductionScreen(order);
            _logger.LogDetailAsync($"2-9 Back from getting Index: {idx}").SafeFireAndForget();
            return idx;
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
        private async Task<int> AddOrderToInductionScreen(Order order)
        {
            var arrayPosition = AddItemToBatch(order.Id, order.Ord1, order.Ord2);
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

        //    var arrayPosition = -1;
        //    if (orderId == 0) return arrayPosition;
        //    var orderToProcess = _ordersToPick.FirstOrDefault(r => r.OrderId == orderId);
        //    if (orderToProcess != null)
        //    {
        //        // return orderToProcess.PositionNumber;
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
        //        var order =  _repoOrders.FindByKey(orderId);
        //        if (order == null) return arrayPosition;

        //        var ord1 = order.Ord1;
        //        var ord2 = order.Ord2;

        //        arrayPosition = AddItemToBatch(orderId, ord1, ord2);

        //        if (arrayPosition == -1) return arrayPosition;
        //        if (_workstationView.BatchTable == null) return arrayPosition;
        //        var ord = ord1.Substring(ord1.Length - 4);
        //        var position = _ordersToPick[arrayPosition].PositionNumber;
        //        if (_iptiDisplayFunctions != null)
        //        {
        //            await _iptiDisplayFunctions.TurnOnBatchDisplay(position, ord);
        //        }
        //    }

        //    return arrayPosition;
        //}

        private async Task NextButtonEnabled()
        {

            var enabled = await CheckForValidOrders();


            if (!enabled)
            {
                // ---         Task.Run(() => _logger.LogDetailAsync($"Next Button Enabled: false"));
                MBGo.Enabled = false;
                MBGo2.Enabled = false;
            }
            else
            {
                // ---         Task.Run(() => _logger.LogDetailAsync($"Next Button Enabled: true"));
                MBGo.Enabled = true;
                MBGo2.Enabled = true;
            }

        }

        /// <summary>
        /// Adds an order to the batch for processing.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order.</param>
        /// <param name="ord1">The first order string, typically used for display or identification.</param>
        /// <param name="ord2">The second order string, typically used for additional identification or processing.</param>
        /// <returns>The position in the batch array where the order was added, or -1 if the order could not be added.</returns>
        /// <remarks>
        /// This method updates the UI to reflect the new order in the batch and ensures the order is valid and not already in the batch.
        /// </remarks>
        private int AddItemToBatch(int orderId, string ord1, string ord2)
        {
            //SetFocusNextTextBoxPos();
            _currentTextBoxPos = GetTextBoxPosWithFocus();

            if (_currentTextBoxPos.InvokeRequired)
            {
                _currentTextBoxPos.Invoke(new Action(() =>
                {
                    _currentTextBoxPos.Text = ord1;
                }));
            }
            else
            {
                _currentTextBoxPos.Text = ord1;
            }
            var position = int.Parse(_currentTextBoxPos.Tag.ToString());
            var arrayPosition = position - 1;
            if (IsValidArrayPosition(arrayPosition) && !OrderInBatch(orderId))
            {
                UpdateOrderInBatch(arrayPosition, orderId, ord1, ord2);
            }
            else
            {
                FocusOnCurrentTextBox();
            }
            return arrayPosition;
        }

        /// <summary>
        /// Determines whether the specified array position is valid within the bounds of the pick batch size.
        /// </summary>
        /// <param name="arrayPosition">The position in the array to validate.</param>
        /// <returns>
        /// <c>true</c> if the specified array position is within the valid range; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValidArrayPosition(int arrayPosition)
        {
            return arrayPosition >= 0 && arrayPosition < _neutronVariables.PickBatchSize;
        }

        /// <summary>
        /// Updates the order details in the batch at the specified array position.
        /// </summary>
        /// <param name="arrayPosition">The position in the batch array where the order details will be updated.</param>
        /// <param name="orderId">The ID of the order to be updated.</param>
        /// <param name="ord1">The first order detail to be updated.</param>
        /// <param name="ord2">The second order detail to be updated.</param>
        private void UpdateOrderInBatch(int arrayPosition, int orderId, string ord1, string ord2)
        {
            _ordersToPick[arrayPosition].OrderId = orderId;
            _ordersToPick[arrayPosition].Ord1 = ord1;
            _ordersToPick[arrayPosition].Ord2 = ord2;
            _ordersToPick[arrayPosition].OrderComplete = false;
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



        //private int AddItemToBatch(int orderId, string ord1, string ord2)
        //{
        //    SetFocusNextTextBoxPos();
        //    _currentTextBoxPos = GetTextBoxPosWithFocus();
        //    _currentTextBoxPos.Text = ord1;
        //    var position = int.Parse(_currentTextBoxPos.Tag.ToString());
        //    var arrayPosition = position - 1;
        //    if (arrayPosition >= 0 && arrayPosition < _neutronVariables.PickBatchSize)
        //    {
        //        if (!OrderInBatch(orderId))
        //        {
        //            _ordersToPick[arrayPosition].OrderId = orderId;
        //            _ordersToPick[arrayPosition].Ord1 = ord1;
        //            _ordersToPick[arrayPosition].Ord2 = ord2;
        //            _ordersToPick[arrayPosition].OrderComplete = false;
        //        }
        //        else
        //        {
        //            _currentTextBoxPos.SelectAll();
        //            _currentTextBoxPos.Focus();
        //        }
        //    }
        //    return arrayPosition;
        //}

        /// <summary>
        /// Determines whether a specified order is already present in the batch.
        /// </summary>
        /// <param name="orderId">The unique identifier of the order to check.</param>
        /// <returns>
        /// <c>true</c> if the order with the specified <paramref name="orderId"/> is present in the batch; otherwise, <c>false</c>.
        /// </returns>
        private bool OrderInBatch(int orderId)
        {
            return _ordersToPick.Any(r => r.OrderId == orderId);
        }

        private int SetBatchPositionToManualOverride()
        {
            var result = -1;
            result = int.Parse(_currentTextBoxPos.Tag.ToString());
            return result - 1;
        }

        /// <summary>
        /// Clears the selection of all orders in the available orders DataGridView.
        /// </summary>
        private void ClearAllSelectOrdersToPick()
        {
            DataGridViewAvailableOrders.ClearSelection();
        }

        /// <summary>
        /// Clears the batch positions asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method resets the batch positions for all orders to pick.
        /// </remarks>
        private Task ClearBatchPositions(List<BatchPosition> batchPositions)
        {
            foreach (var batchPosition in batchPositions)
            {
                ClearBatchPosition(batchPosition);
            }

            return Task.CompletedTask;
            //var resetTasks = batchPositions.Select(ResetBatchPosition);
            //await Task.WhenAll(resetTasks);
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
            
            var pos = batchPosition.PositionNumber.ToString();
            var controlName = $"TextBoxPos{pos}";
            var control = Controls.Find(controlName, true).FirstOrDefault();
            if (control != null)
            {
                var textBox = ((TextBox)control);
                textBox.Text = string.Empty;
            }
            controlName = $"TextBoxPickPos{pos}";
            control = Controls.Find(controlName, true).FirstOrDefault();
            if (control != null)
            {
                var textBox = ((TextBox)control);
                textBox.Text = string.Empty;
            }

            controlName = $"Pos{pos}Display";
            control = Controls.Find(controlName, true).First();
            if (control != null)
            {
                var panel = (Panel)control;
                // if the order is complete, turn the panel back color green
                panel.BackColor = Color.Transparent;
            }

            controlName = $"Pos{pos}Display";
            control = Controls.Find(controlName, true).First();
            if (control != null)
            {
                var panel = (Panel)control;
                // if the order is complete, turn the panel back color green
                panel.BackColor = Color.Transparent;
            }
        }


        //private async Task ResetBatchPosition(BatchPosition batchPosition)
        //{
        //    batchPosition.OrderId = 0;
        //    batchPosition.Ord1 = string.Empty;
        //    batchPosition.Ord2 = string.Empty;
        //    batchPosition.OrderComplete = false;
        //    await UpdateTextBoxPosition(batchPosition);
        //}


        /// <summary>
        /// Sets the current text box position based on the specified batch position number.
        /// </summary>
        /// <param name="batchPositionNumber">The batch position number to locate the corresponding text box.</param>
        /// <remarks>
        /// This method finds the <see cref="TextBox"/> control with the specified batch position number,
        /// sets it as the current text box, and focuses it.
        /// </remarks>
        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            // Find the TextBox control with the specified batch position number
            var controlName = $"TextBoxPos{batchPositionNumber}";
            var control = Controls.Find(controlName, true).SingleOrDefault() as TextBox;
            // If the control is found, set the _currentTextBoxPos and focus it
            if (control != null)
            {
                _currentTextBoxPos = control;
                _currentTextBoxPos.Focus();
                _currentTextBoxPos.Select();
            }
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
        private Task UpdateTextBoxPosition(BatchPosition batchPosition)
        {
            var orderNumber = batchPosition.Ord1;
            var position = batchPosition.PositionNumber;
            // Find the TextBox control based on the position number
            var textBox = FindTextBoxByPosition(position);
            // Update the TextBox text with the order number if the TextBox is found
            if (textBox != null)
            {
                textBox.Text = orderNumber;
            }

            return Task.CompletedTask;
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
        //private async Task<TextBox> FindTextBoxByPositionAsync(int position)
        //{
        //    return await Task.Run(() => Controls
        //        .Find($"TextBoxPos{position}", true)
        //        .OfType<TextBox>()
        //        .SingleOrDefault());
        //}

        private TextBox FindTextBoxByPosition(int position)
        {
            return Controls
                .Find($"TextBoxPos{position}", true)
                .OfType<TextBox>()
                .SingleOrDefault();
        }

        //private static List<BatchPosition> InitOrdersToPick(int pickBatchSize)
        //{
        //    var ordersToPick = new List<BatchPosition>();
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
        //        ordersToPick.Add(batchPosition);
        //    }

        //    return ordersToPick;
        //}

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

        //private void ShowPosition(int position)
        //{
        //    // var font = new Font("Microsoft Sans Serif", 20);
        //    var pos = position.ToString();

        //    Control c = Controls.Find("LabelPickPos" + pos, true).Single() as Label;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("LabelPos" + pos, true).Single() as Label;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
        //    if (c != null)
        //    {
        //        // c.Font = font;
        //        c.Visible = true;
        //    }

        //    c = Controls.Find("TextBoxPos" + pos, true).Single() as TextBox;
        //    if (c != null) c.Visible = true;

        //    c = Controls.Find("Pos" + pos + "Display", true).Single();
        //    if (c != null) ((Panel)c).Visible = true;

        //    c = Controls.Find("AvailablePos" + pos + "Display", true).Single();
        //    if (c != null) ((Panel)c).Visible = true;
        //}

        //------------------------ ShowOrderOnBatchDisplay Start-------------------------------------

        private async Task ShowOrderOnBatchDisplay()
        {
            var font = new Font(Font.FontFamily, BatchDisplayFontSize);
            var batchPositions = _ordersToPick.Where(r => !string.IsNullOrEmpty(r.Ord1)).ToList();

            foreach (var batchPosition in batchPositions)
            {
                var textBox = FindTextBox($"TextBoxPickPos{batchPosition.PositionNumber}");
                if (textBox == null) continue;
                textBox.Font = font;
                var text = batchPosition.Ord1.Trim().Substring(batchPosition.Ord1.Trim().Length - 4);
                textBox.Text = text;
                _logger.LogDetailAsync($" Position: {batchPosition.PositionNumber} TextBox Text: {text}  DELAY: {_iptiConfig.TransmitDelay}").SafeFireAndForget();
                if (_iptiDisplayFunctions == null) continue;
                await _iptiDisplayFunctions.TurnOnBatchDisplay(batchPosition.PositionNumber, text);

                await Task.Delay(_iptiConfig.TransmitDelay);

            }
        }



        //private async Task ShowOrderOnBatchDisplay()
        //{
        //    var fontSize = 26;
        //    var font = new Font(Font.FontFamily, fontSize);
        //    var batchPositions = _ordersToPick.Where(r => !string.IsNullOrEmpty(r.Ord1)).ToList();
        //    foreach (var batchPosition in batchPositions)
        //    {
        //        var textBox = Controls.Find($"TextBoxPickPos{batchPosition.PositionNumber}", true).FirstOrDefault() as TextBox;
        //        if (textBox == null) continue;
        //        textBox.Font = font;
        //        var text = batchPosition.Ord1.Trim().Substring(batchPosition.Ord1.Trim().Length - 4);
        //        textBox.Text = text;
        //        if (_iptiDisplayFunctions != null)
        //        {
        //            await _iptiDisplayFunctions.TurnOnBatchDisplay(batchPosition.PositionNumber, text);
        //        }
        //    }
        //}

        //---------------------------- ShowOrderOnBatchDisplay End------------------------


        //---------------------------- ShowQuantityOnBatchDisplay Start-----------------------------------------

        private async Task ShowQuantityOnBatchDisplay()
        {
            await ClearBatchTableIfPossible();
            var batchPositions = _ordersToPick.Where(r => !string.IsNullOrEmpty(r.Ord1)).ToList();
            await UpdateBatchPositionsDisplay(batchPositions);
            await UpdatePickViewsDisplay(_currentPickStop.PickViews);
        }
        private async Task ClearBatchTableIfPossible()
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
            }
        }
        private async Task UpdateBatchPositionsDisplay(List<BatchPosition> batchPositions)
        {
            foreach (var batchPosition in batchPositions)
            {
                var textBox = FindTextBox($"TextBoxPickPos{batchPosition.PositionNumber}");
                if (textBox == null) continue;
                textBox.Text = batchPosition.OrderComplete ? "END" : string.Empty;
                if (batchPosition.OrderComplete)
                {
                    await TurnOnBatchDisplayEndIfPossible(batchPosition.PositionNumber);
                }
                Task.Delay(_iptiConfig.TransmitDelay).Wait();
            }
        }
        private TextBox FindTextBox(string name)
        {
            return Controls.Find(name, true).FirstOrDefault() as TextBox;
        }
        private async Task TurnOnBatchDisplayEndIfPossible(int positionNumber)
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.TurnOnBatchDisplayEnd(positionNumber);
            }
        }
        private async Task UpdatePickViewsDisplay(List<PickView> pickViews)
        {
            foreach (var pickView in pickViews)
            {
                var pos = pickView.PickPosition;
                var textBox = FindTextBox($"TextBoxPickPos{pos}");
                if (textBox != null && string.IsNullOrEmpty(textBox.Text))
                {
                    UpdateTextBoxForPickView(textBox, pickView);
                    await TurnOnBatchDisplayIfPossible(pos, pickView.QuantityToBePicked.ToString());
                }
                UpdatePanelColor($"Pos{pos}Display", Color.Red);
            }
        }
        private void UpdateTextBoxForPickView(TextBox textBox, PickView pickView)
        {
            textBox.Font = new Font(textBox.Font.FontFamily, 26);
            textBox.Text = pickView.QuantityToBePicked.ToString();
        }
        private async Task TurnOnBatchDisplayIfPossible(int pos, string quantityToBePicked)
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.TurnOnBatchDisplay(pos, quantityToBePicked);
            }
        }
        private void UpdatePanelColor(string name, Color color)
        {
            var panel = Controls.Find(name, true).FirstOrDefault() as Panel;
            if (panel != null)
            {
                panel.BackColor = color;
            }
        }





        //private async Task ShowQuantityOnBatchDisplay()
        //{
        //    if (_iptiDisplayFunctions != null)
        //    {
        //        await _iptiDisplayFunctions.ClearBatchTable();
        //    }

        //    var batchPositions = _ordersToPick.Where(r => !string.IsNullOrEmpty(r.Ord1)).ToList();
        //    foreach (var batchPosition in batchPositions)
        //    {
        //        var textBox = Controls.Find($"TextBoxPickPos{batchPosition.PositionNumber}", true).FirstOrDefault() as TextBox;
        //        if (textBox == null) continue;
        //        textBox.Text = batchPosition.OrderComplete ? "END" : string.Empty;
        //        if (batchPosition.OrderComplete)
        //        {
        //            if (_iptiDisplayFunctions != null)
        //            {
        //                await _iptiDisplayFunctions.TurnOnBatchDisplayEnd(batchPosition.PositionNumber);
        //            }

        //        }
        //    }
        //    foreach (var pickView in _currentPickStop.PickViews)
        //    {
        //        var pos = pickView.PickPosition;
        //        var textBox = Controls.Find($"TextBoxPickPos{pos}", true).FirstOrDefault() as TextBox;
        //        if (textBox != null && string.IsNullOrEmpty(textBox.Text))
        //        {
        //            textBox.Font = new Font(textBox.Font.FontFamily, 26);
        //            textBox.Text = pickView.QuantityToBePicked.ToString();
        //            if (_iptiDisplayFunctions != null)
        //            {
        //                await _iptiDisplayFunctions.TurnOnBatchDisplay(pos, pickView.QuantityToBePicked.ToString());
        //            }
        //        }
        //        var panel = Controls.Find($"Pos{pos}Display", true).FirstOrDefault() as Panel;
        //        if (panel != null)
        //        {
        //            panel.BackColor = Color.Red;
        //        }
        //    }
        //}

        //----------------------------- ShowQuantityOnBatchDisplay End------------------------------------


        private async void MBShowOrderOrQuantityToggle_Click(object sender, EventArgs e)
        {
            await ShowOrderOrQuantityToggle();
        }

        private async Task ShowOrderOrQuantityToggle()
        {
            if (MBShowOrderOrQuantityToggle.Text == _resourceManager.GetString($"ShowJobs"))
            {
                await ShowOrderOnBatchDisplay();
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowQuantity");
            }
            else
            {
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
                await ShowQuantityOnBatchDisplay();
            }
        }

        private async void MBPickBack_Click(object sender, EventArgs e)
        {
            await ExecutePickBackProcess();
        }

        private async Task ExecutePickBackProcess()
        {
            await ClearIptiDisplayFunctions();
            ResetHanelDeviceStatus();
            ClearProLites();
            ClearActiveDeviceIndicators();
            await UpdateOrdersStatus();
            await ClearBatchPositions(_currentBatchPositions);
            //ClearOrderPositions(_currentBatchPositions);
            //ClearPickPositions(_currentBatchPositions);
            //ClearTextBoxPickPositions(_currentBatchPositions);
            //ClearPickDisplays(_currentBatchPositions);
            await ClearInductionScreen(_currentBatchPositions);
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
        private void EnableNextButton()
        {
            //  NextButtonEnabled();
        }
        private async Task UpdateOrdersStatus()
        {
            await UpdateOrdersToAvailableStatus(_ordersToPick);
        }
        private void UpdateFormTitleAndTab()
        {
            LabelFormTitle.Text = _resourceManager.GetString($"PickList");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = PickList;
        }

        private async Task UpdateOrdersToAvailableStatus(List<BatchPosition> ordersToPick)
        {
            foreach (var batchPosition in ordersToPick)
            {
                if (batchPosition.OrderId == 0) continue;
                await UpdateOrderDetailsStatus(batchPosition);
            }
        }
        private async Task UpdateOrderDetailsStatus(BatchPosition batchPosition)
        {
            var orderDetails = GetPickingOrderDetails(batchPosition);

            if (orderDetails == null)
            {
                return;
            }
            foreach (var orderDetail in orderDetails)
            {
                await SetOrderDetailStatusToAvailable(orderDetail);
            }
        }
        private IEnumerable<OrderDetail> GetPickingOrderDetails(BatchPosition batchPosition)
        {
            var orderDetails = _repoOrderDetails.FindBy(r => r.OrderId == batchPosition.OrderId && r.AreaId == _workstationView.AreaId && r.LineStatusId == (int)LineStatus.Picking).ToList();
            return orderDetails;
        }

        //private void UpdateOrdersToAvailableStatus(List<BatchPosition> ordersToPick)
        //{
        //    var batchPositions = ordersToPick.Where(r => r.OrderId != 0 && r.OrderComplete == false).ToList();
        //    foreach (var batchPosition in batchPositions)
        //    {
        //        var orderDetails = _repoOrderDetails.FindBy(r => r.OrderId == batchPosition.OrderId && r.AreaId == _workstationView.AreaId && r.LineStatusId == (int)LineStatus.Picking);
        //        foreach (var orderDetail in orderDetails)
        //        {
        //            orderDetail.LineStatusId = (int)LineStatus.Available;
        //            _repoOrderDetails.Update(orderDetail);
        //        }
        //    }
        //}

        private async void MBStart_Click(object sender, EventArgs e)
        {
            MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
            var pickViews = (IList<PickView>)_bindingSourcePickViews.DataSource;
            if (pickViews == null) return;
            await Start(pickViews.ToList());
        }
        private async Task Start(List<PickView> pickableViews)
        {
            MBPickAccept.Visible = false;

            _logger.LogDetailAsync($"{DateTime.Now} START").SafeFireAndForget();
            ResetHanelDeviceStatus();
            _spaceBarDisabled = true;
            PrintAllDocuments();
            _logger.LogDetailAsync($"{DateTime.Now} Printing complete").SafeFireAndForget();

            // a final check to make sure we have PickViews to pick
            //if (_bindingSourcePickViews.Count == 0) return;
            var pickViews = SortPickViews(pickableViews);
            if (pickViews == null) return;
            _bindingSourcePickViews.Clear();
            DataGridPickView.DataSource = _bindingSourcePickViews;

            _bindingSourcePickViews.DataSource = pickViews;
            DataGridPickView.DataSource = _bindingSourcePickViews;


            // time to create the PickStops
            var pickStops = CreatePickStops(pickViews);
            LogPickStops(pickStops);

            // FinalPickSequence sorts the Pickstops to be picked 1,2,3,4 across all devices
            // If this is not a pick workstation with devices
            // Sort the Picks in PickSequence order
            // Blastzone is a single device

            var finalPickSequence = SortFinalPickSequence(pickStops);

            // set the BindingSourcePickStops to the finalPickSequence
            _bindingSourcePickStops.DataSource = null;
            _bindingSourcePickStops.DataSource = finalPickSequence;

            _logger.LogDetailAsync($"{DateTime.Now} PickStops Order Set Across Devices").SafeFireAndForget();

            // ---         Task.Run(() => _logger.LogDetailAsync($"Start_Click 3 Run GetFirstStop?: [{DateTime.Now.ToLongTimeString()}]"));
            // The PickStop BindingSource is now set
            // GetFirstStop();
            MoveToFirstPickStop();

            // PickStops are built from PickViews
            // it's time to print the labels for the first PickStop
            // If the workstation is a Rack station then ALL the labels will be printed
            // TODO  This is a hack to get the labels to print for the Rack Station
            // TODO  Need to figure out how to get the labels to print for the Rack Station
            // TODO  by adding a variable to NeutronVariables like PrintAllLabels.
            PrintLabelsForPickStops(finalPickSequence);
            _logger.LogDetailAsync($"{DateTime.Now} Label/Labels on OC have Printed").SafeFireAndForget();
            // To here
            // document printing is done
            // labels are printed
            // PickStops are built and the BindingSourcePickStops is set
            // the currentPickStop is set to the first PickStop in the BindingSourcePickStops
            // the currentPickStop is the PickStop that will be picked
            // the currentPickStop is the PickStop that will be displayed on the Pick Screen

            await UpdatePickScreen();
            _logger.LogDetailAsync($"{DateTime.Now} Update Pick Screen Complete").SafeFireAndForget();

            if (_workstationView.AreaId != AreaEight)
            {
                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
            }

            _logger.LogDetailAsync($"{DateTime.Now} Device Indicator Update Complete").SafeFireAndForget();

            await UpdatePickPosition();
            _logger.LogDetailAsync($"{DateTime.Now} Update Pick Position Complete").SafeFireAndForget();

            UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
            _logger.LogDetailAsync($"{DateTime.Now} Update GroupBox Location Complete").SafeFireAndForget();
            // UpdateTowerDisplay();

            tabControl1.SelectedTab = PickScreen;

            await Task.Delay(1000);
            MBPickAccept.Enabled = true;
            MBPickAccept.Visible = true;

            var focused = MBPickAccept.Focus();

            if (!focused)
            {
                _logger.LogDetailAsync($"START--Focus Failed: Enabled:{MBPickAccept.Enabled} Visible:{MBPickAccept.Visible} Focused:{focused}").SafeFireAndForget();
            }

            //feels good to here
            // ---         Task.Run(() => _logger.LogDetailAsync($"Start_Click End: [{DateTime.Now.ToLongTimeString()}]"));
            // StartSpaceBarEnableTimer();
            _spaceBarDisabled = false;
            _logger.LogDetailAsync($"{DateTime.Now} START module Complete").SafeFireAndForget();
        }

        private void PrintLabelsForPickStops(IEnumerable<PickStop> finalPickSequence)
        {

            if (_neutronVariables.EnableLabelPrinter)
            {
                if (_workstationView.AreaId == AreaEight)
                {
                    foreach (var pickStop in finalPickSequence)
                    {
                        PrintLabels(pickStop);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    PrintLabels(_currentPickStop);
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        /// Moves to the first PickStop in the BindingSource and sets the currentPickStop variable to it.
        /// </summary>
        private void MoveToFirstPickStop()
        {
            _bindingSourcePickStops.MoveFirst();
            _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
        }
        /// <summary>
        /// Sorts the final pick sequence based on the area ID of the workstation view.
        /// </summary>
        /// <param name="pickStops">A list of pick stops to be sorted.</param>
        /// <returns>Returns a sorted sequence of pick stops.</returns>
        private IEnumerable<PickStop> SortFinalPickSequence(List<PickStop> pickStops)
        {
            _logger.LogDetailAsync($"Sorting by Area");
            return _workstationView.AreaId == AreaEight
                ? FinalPickSequenceAreaEight(pickStops)
                : FinalPickSequence(pickStops);
        }

        private void LogPickableViews(List<PickView> pickViews)
        {
            foreach (var pick in pickViews)
            {
                _logger.LogDetailAsync($"Pick Position: {pick.PickPosition} " +
                                       $" SKU: {pick.CurrentInventoryLocation.ItemDefinition.Item}" +
                                       $" AREA: {pick.CurrentInventoryLocation.ItemDefinition.AreaId}" +
                                       $" Location: {pick.CurrentInventoryLocation.Location.Loc1}" +
                                       $"-{pick.CurrentInventoryLocation.Location.Loc2}" +
                                       $"-{pick.CurrentInventoryLocation.Location.Loc3}" +
                                       $"-{pick.CurrentInventoryLocation.Location.Loc4}" +
                                       $"  Slot:  {pick.CurrentInventoryLocation.Location.Slot}" +
                                       $"  Sequence:  {pick.CurrentInventoryLocation.Location.PickSequence}").SafeFireAndForget();

            }

            _logger.LogDetailAsync($"{DateTime.Now} Creating PickViews Complete").SafeFireAndForget();
        }

        private void LogPickStops(List<PickStop> pickStops)
        {
            try
            {
                foreach (var stop in pickStops)
                {
                    _logger.LogDetailAsync($"Order: {stop.Ord1} " +
                                           $" SKU: {stop.CurrentInventoryLocation.ItemDefinition.Item}" +
                                           $" AREA: {stop.CurrentInventoryLocation.ItemDefinition.AreaId}" +
                                           $" Location: {stop.CurrentInventoryLocation.Location.Loc1}" +
                                           $"-{stop.CurrentInventoryLocation.Location.Loc2}" +
                                           $"-{stop.CurrentInventoryLocation.Location.Loc3}" +
                                           $"-{stop.CurrentInventoryLocation.Location.Loc4}" +
                                           $"  Slot:  {stop.CurrentInventoryLocation.Location.Slot}" +
                                           $"  Sequence:  {stop.CurrentInventoryLocation.Location.PickSequence}").SafeFireAndForget();

                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"{DateTime.Now} Creating PickStops Exception: {ex.Message}").SafeFireAndForget();
            }


            _logger.LogDetailAsync($"{DateTime.Now} Creating PickStops Complete").SafeFireAndForget();
        }
        /// <summary>
        /// Creates a list of PickStop objects from the provided collection of PickView objects.
        /// </summary>
        /// <param name="pickViews">The collection of PickView objects to be processed.</param>
        /// <returns>A list of PickStop objects created from the provided PickView objects.</returns>
        private List<PickStop> CreatePickStops(IList<PickView> pickViews)
        {
            _logger.LogDetailAsync($"PickViews Count: {pickViews.Count()}");
            var pickStops = new List<PickStop>();
            var pickViewGroups = GroupPickViewsByItemKey(pickViews);
            var sequence = 0;
            foreach (var pickViewGroup in pickViewGroups)
            {
                var pickStop = CreatePickStopFromFirstPickView(pickViewGroup, ref sequence);
                if (pickStop != null)
                {
                    AddPickViewsToPickStop(pickViewGroup, pickStop);
                    pickStops.Add(pickStop);
                }

            }

            _logger.LogDetailAsync($"Return PickStops");
            return pickStops;
        }
        /// <summary>
        /// Groups the provided collection of PickView instances by their ItemKey property.
        /// </summary>
        /// <param name="pickViews">The collection of PickView instances to be grouped.</param>
        /// <returns>A collection of groups where each group represents a set of PickView instances that share the same ItemKey.</returns>
        private IEnumerable<IGrouping<string, PickView>> GroupPickViewsByItemKey(IList<PickView> pickViews)
        {
            _logger.LogDetailAsync($"Grouping PickViews");
            return pickViews.GroupBy(r => r.ItemKey).ToList();
        }
        /// <summary>
        /// Creates a new instance of the <see cref="PickStop"/> class from the first <see cref="PickView"/> in the provided group.
        /// </summary>
        /// <param name="pickViewGroup">The group of <see cref="PickView"/> instances from which to create the <see cref="PickStop"/>.</param>
        /// <param name="sequence">A reference to the sequence number, which is incremented in this method.</param>
        /// <returns>A new instance of the <see cref="PickStop"/> class.</returns>
        private PickStop CreatePickStopFromFirstPickView(IGrouping<string, PickView> pickViewGroup, ref int sequence)
        {
            _logger.LogDetailAsync($"Create PickStop");
            PickStop pickStop = null;
            try
            {
                var firstPickView = pickViewGroup.First();
                pickStop = new PickStop
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
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Create PickStop Exception: {ex.Message}");
            }

            return pickStop;
        }
        /// <summary>
        /// Adds the PickViews from the specified group to the provided PickStop.
        /// </summary>
        /// <param name="pickViewGroup">The group of PickViews to be added to the PickStop.</param>
        /// <param name="pickStop">The PickStop to which the PickViews will be added.</param>
        /// <remarks>
        /// This method also updates the total quantity and the quantity to be picked for the PickStop based on the quantities of the PickViews.
        /// </remarks>
        private void AddPickViewsToPickStop(IGrouping<string, PickView> pickViewGroup, PickStop pickStop)
        {
            var total = 0;
            foreach (var pickView in pickViewGroup)
            {
                pickStop.PickViews.Add(pickView);
                total += pickView.Quantity;
            }
            pickStop.Quantity = total;
            pickStop.QuantityToBePicked = total;
        }



        //private List<PickStop> CreatePickStops(IEnumerable<PickView> pickViews)
        //{
        //    var pickStops = new List<PickStop>();
        //    IEnumerable<IGrouping<string, PickView>> pickViewGroups = pickViews.GroupBy(r => r.ItemKey).ToList();
        //    var sequence = 0;
        //    foreach (var pickViewGroup in pickViewGroups) //for each Item in the group of Items
        //    {
        //        var total = 0;
        //        // a PickStop is of One Item that may be on One to All Pick Positions
        //        // a PickView is an individual pick at a single Pick Position
        //        // so a PickStop is has One or Multiple PickViews that are concerned 
        //        // with picking One Item.
        //        // a PickStop is a summary of all the PickViews 
        //        // and some of the information in a PickStop is the same as in a PickView
        //        // that is why the First PickView is used to provide most of the data to the PickStop
        //        var firstPickView = pickViewGroup.First();

        //        var pickStop = new PickStop
        //        {
        //            Sequence = sequence += 1,
        //            OrderId = firstPickView.OrderId,
        //            Ord1 = firstPickView.Ord1,
        //            Ord2 = firstPickView.Ord2,
        //            ItemId = firstPickView.ItemId,
        //            Item = firstPickView.Item,
        //            Description = firstPickView.Description,
        //            UnitOfIssue = firstPickView.UnitOfIssue,
        //            PickedQty = firstPickView.PickedQty,
        //            Images = firstPickView.Images,
        //            Inventory = firstPickView.Inventory,
        //            InventoryIndex = firstPickView.InventoryIndex,
        //            Slot = firstPickView.Slot,
        //            SlotQty = firstPickView.SlotQty,
        //            CurrentInventoryLocation = firstPickView.CurrentInventoryLocation,
        //            TotalQuantityInInventory = firstPickView.TotalQuantityInInventory,
        //            ItemKey = firstPickView.ItemKey
        //        };

        //        // now that we have a PickStop, we need to add the PickViews to it
        //        // and add up the total quantity to be picked
        //        foreach (var pickView in pickViewGroup)
        //        {
        //            pickStop.PickViews.Add(pickView);
        //            total += pickView.Quantity;
        //        }
        //        // set the total quantity to be picked
        //        // and the quantity to be picked
        //        // to the PickStops List
        //        pickStop.Quantity = total;
        //        pickStop.QuantityToBePicked = total;
        //        pickStops.Add(pickStop);
        //    }
        //    return pickStops;
        //}

        /// <summary>
        /// Sorts the PickViews based on the workstation area and location.
        /// </summary>
        /// <param name="pickableViews"></param>
        /// <returns>
        /// A sorted list of PickViews. If the data source is not a list of PickViews, it logs an error and returns null.
        /// </returns>
        /// <remarks>
        /// If the workstation area is Area Eight, the PickViews are sorted by PickSequence.
        /// Otherwise, they are sorted by location (Loc1, Loc2, Loc3, Loc4).
        /// </remarks>
        private IList<PickView> SortPickViews(List<PickView> pickableViews)
        {
            IList<PickView> pickViews = null;

            //var list = (IList<PickView>)_bindingSourcePickViews.DataSource;
            //if (list != null)
            //{
            //    pickViews = list;
            //}
            //else
            //{
            //    // Handle the error appropriately, e.g., log it, throw an exception, etc.
            //    _logger.LogDetailAsync($"{DateTime.Now} Invalid data source").SafeFireAndForget();
            //    return null;
            //}
            try
            {
                // set the sort order based on Location Type
                // LocationType 3 is a Rack location and 
                // should be sorted using the PickSequence
                //var locationType = _workstationView.Area.LocationTypeId;
                if (_workstationView.AreaId == AreaEight)
                {
                    // Sort by PickSequence in Area Eight
                    pickViews = pickableViews.OrderBy(p => p.CurrentInventoryLocation?.Location?.PickSequence).ToList();
                }
                else
                {
                    // Sort by Location
                    pickViews = pickableViews.OrderBy(p => p.CurrentInventoryLocation?.Location?.Loc1)
                        .ThenBy(p => p.CurrentInventoryLocation?.Location?.Loc2)
                        .ThenBy(p => p.CurrentInventoryLocation?.Location?.Loc3)
                        .ThenBy(p => p.CurrentInventoryLocation?.Location?.Loc4).ToList();
                }
                _logger.LogDetailAsync($"{DateTime.Now} PickView Ordering Complete").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogDetailAsync($"{DateTime.Now} Error while sorting PickViews: {ex.Message}").SafeFireAndForget();
            }

            //if (pickViews != null)
            //{
            //    LogPickableViews(pickViews.ToList());
            //}
            return pickViews;
        }


        /// <summary>
        /// Sorts the PickStops based on the Location's Pick Sequence
        /// </summary>
        /// <param name="pickStops">The List of PickStops to be sorted</param>
        /// <returns></returns>
        private List<PickStop> FinalPickSequenceAreaEight(List<PickStop> pickStops)
        {
            //foreach (var pickStop in pickStops)
            //{
            //    pickStop.Inventory.OrderBy(r => r.Location.PickSequence).ToList();
            //    pickStop.CurrentInventoryLocation = pickStop.Inventory.FirstOrDefault();
            //}

            //return pickStops.OrderBy(r => r.CurrentInventoryLocation.Location.PickSequence).ToList();
            return pickStops;
        }

        //------------------------------------------
        //private List<PickStop> FinalPickSequence(List<PickStop> pickStops)
        //{
        //    var pickableDevices = GetPickableDevices();
        //    var pickStopGroups = GroupPickStopsByDevice(pickStops, pickableDevices);
        //    var totalPickStops = pickStops.Count;
        //    for (var i = 0; i < totalPickStops; i++)
        //    {
        //        ProcessPickStopGroups(pickStopGroups);
        //    }
        //    _deviceManager = new PickDeviceManager(pickStopGroups, _neutronVariables.ShuttleEnabled);
        //    MoveDevicesToNextPickStop();
        //    return new List<PickStop>();
        //}
        //private List<HardwareDevice> GetPickableDevices()
        //{
        //    return _workstationView.HardwareDevices.Where(r => r.DeviceType.Pickable).ToList();
        //}
        //private List<List<PickStop>> GroupPickStopsByDevice(List<PickStop> pickStops, List<HardwareDevice> hardwareDevices)
        //{
        //    var pickStopGroups = new List<List<PickStop>>();
        //    if (!pickStops.Any() || !hardwareDevices.Any()) return pickStopGroups;

        //    for (var i = 0; i < hardwareDevices.Count; i++)
        //    {
        //        var pickStopGroup = pickStops
        //            .Where(p => p.CurrentInventoryLocation.Location.Loc1 == i + 1)
        //            .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
        //            .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
        //            .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
        //            .ToList();
        //        pickStopGroups.Add(pickStopGroup);
        //    }
        //    return pickStopGroups;
        //}
        //private void ProcessPickStopGroups(List<List<PickStop>> pickStopGroups)
        //{
        //    var seq = 1;

        //    for (var i = 0; i < pickStopGroups.Count; i++)
        //    {
        //        // Process each pick stop group
        //        for (var j = 0; j < newCarList.Count; j++)
        //        {
        //            if (newCarList[j].Count >= i + 1)
        //            {
        //                newCarList[j][i].Sequence = seq;
        //                seq += 1;
        //                newList.Add(newCarList[j][i]);
        //            }
        //        }
        //    }
        //}
        //private void MoveDevicesToNextPickStop()
        //{
        //    for (var i = 1; i <= _workstationView.Hanels.Count; i++)
        //    {
        //        _deviceManager.MoveNext(i);
        //    }
        //}




        //------------------------------------------


        /// <summary>
        /// FinalPickSequence sorts the Pickstops to be picked 1,2,3,4 across all devices
        /// </summary>
        /// <param name="pickStops"></param>
        /// <returns>A list of PickStops in Machine Pick Order</returns>
        private List<PickStop> FinalPickSequence(List<PickStop> pickStops)
        {
            _logger.LogDetailAsync($"FinalPickSequence Start PickStop Count: {pickStops.Count}").SafeFireAndForget();
            var newCarList = new List<List<PickStop>>();
            var newList = new List<PickStop>();

            //if (_workstationView.HardwareDevices.Count > 0)
            //{ ONLY Pickable locations are in the PickStops
            // Carousel, Shuttle and Blastzone are all Hardware Devices
            // in the WorkstationView.HardwareDevices List
            // a Blastzone will only have 1 device

            var pickableDevices = _workstationView.HardwareDevices.Where(r => r.DeviceType.Pickable).ToList();

            for (var i = 0; i < pickableDevices.Count; i++)
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
            _logger.LogDetailAsync($"Total Pick Stops: {totalPickStops}");
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

            //}
            //else  //OC Workstation so picks are in PickSequence order
            //{
            //    var carList = pickStops.OrderBy(r => r.Sequence).ToList();
            //    newCarList.Add(carList);
            //}

            //if (_deviceManager == null)
            //{
            _logger.LogDetailAsync($"FinalPickSequence Start Carousel Move").SafeFireAndForget();

            _deviceManager = new PickDeviceManager(newCarList, _neutronVariables.ShuttleEnabled);

            for (var i = 0; i < _workstationView.Hanels.Count; i++)
            {
                //check to see if the device is enabled
                if (_workstationView.Hanels[i].Enabled)
                {
                    _deviceManager.MoveNext(i + 1);
                }
            }

            // ---         Task.Run(() => _logger.LogDetailAsync($"FinalPickSequence End Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            // ---         Task.Run(() => _logger.LogDetailAsync($"FinalPickSequence End: [{DateTime.Now.ToLongTimeString()}]"));
            // }
            _logger.LogDetailAsync($"FinalPickSequence End: ").SafeFireAndForget();
            return newList;
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            if (!_neutronVariables.ShuttleEnabled) return;
            if (GlobalVar.Shuttle != null)
            {
                _logger.LogDetailAsync($"PositionDevice Loc1:{loc1} Loc2:{loc2} Loc3:{loc3} Loc4:{loc4}").SafeFireAndForget();
                GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4);
            }

            if (GlobalVar.Hanel == null) return;
            _logger.LogDetailAsync($"PositionDevice Loc1:{loc1} Loc2:{loc2} Loc3:{loc3} Loc4:{loc4}").SafeFireAndForget();
            GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
        }

        private void InitializeCurrentPickStopSequence()
        {
            if (_currentPickStopSequence == 0)
            {
                _currentPickStopSequence = ((PickStop)_bindingSourcePickStops.Current).Sequence;
            }
        }
        private void EnablePickAcceptIfSequenceMatches()
        {
            if (_currentPickStopSequence == _currentPickStop.Sequence)
            {
                MBPickAccept.Enabled = true;
                _currentPickStopSequence = 0;
            }
            else
            {
                MBPickAccept.Enabled = false;
            }
        }

        private async Task GetFirstStop(bool moveDevice = false)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"GetFirstStop"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                InitializeCurrentPickStopSequence();

                _bindingSourcePickStops.MoveFirst();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;

                EnablePickAcceptIfSequenceMatches();

                await UpdatePickScreen();
                await UpdatePickPosition();

                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;


                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);


                // ---         Task.Run(() => _logger.LogDetailAsync($"GetFirstStop Location : {loc1}-{loc2}-{loc3}-{loc4}"));

                if (moveDevice)
                {
                    PositionDevice(loc1, loc2, loc3, loc4, true);
                }
            }
        }

        private async Task GetNextStop(bool moveDevice = false)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"GetNextStop"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence < numberOfStops)
            {

                InitializeCurrentPickStopSequence();

                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;

                EnablePickAcceptIfSequenceMatches();

                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();

                await UpdatePickPosition();

                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

                // ---         Task.Run(() => _logger.LogDetailAsync($"GetNextStop Location : {loc1}-{loc2}-{loc3}-{loc4}"));

                if (moveDevice)
                {
                    PositionDevice(loc1, loc2, loc3, loc4, true);
                }
            }
        }

        private async Task GetPreviousStop(bool moveDevice = false)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"GetPreviousStop"));

            if (_currentPickStop.Sequence > 0)
            {
                InitializeCurrentPickStopSequence();

                _bindingSourcePickStops.MovePrevious();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;

                EnablePickAcceptIfSequenceMatches();

                await UpdatePickScreen();
                // UpdateCurrentDeviceIndicator();
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

                // ---         Task.Run(() => _logger.LogDetailAsync($"GetPreviousStop Location : {loc1}-{loc2}-{loc3}-{loc4}"));

                if (moveDevice)
                {
                    PositionDevice(loc1, loc2, loc3, loc4, true);
                }
            }
        }

        private async Task GetLastStop(bool moveDevice = false)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"GetLastStop"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                InitializeCurrentPickStopSequence();

                _bindingSourcePickStops.MoveLast();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;

                EnablePickAcceptIfSequenceMatches();

                await UpdatePickScreen();
                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                // UpdateTowerDisplay();
                if (_workstationView.AreaId != AreaEight)
                {
                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);
                }

                // ---         Task.Run(() => _logger.LogDetailAsync($"GetLastStop Location : {loc1}-{loc2}-{loc3}-{loc4}"));
                if (moveDevice)
                {
                    PositionDevice(loc1, loc2, loc3, loc4, true);
                }

            }
        }

        private async void ButtonStopMoveFirst_Click(object sender, EventArgs e)
        {
            await GetFirstStop(moveDevice: false);
        }

        private async void ButtonStopMovePrevious_Click(object sender, EventArgs e)
        {
            await GetPreviousStop(moveDevice: false);
        }

        private async void ButtonStopMoveNext_Click(object sender, EventArgs e)
        {
            await GetNextStop(moveDevice: false);
        }

        private async void ButtonStopMoveLast_Click(object sender, EventArgs e)
        {
            await GetLastStop(moveDevice: false);
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
            MBPickNewItem.Visible = _neutronLicense.CompanyCode == "TOP" ? true : false;

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
        private void MultipleLocationsManager(PickStop currentPickStop)
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

        //private void UpdateTowerDisplay()
        //{
        //    string text = string.Empty;
        //    var item = _currentPickStop.Item;
        //    var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
        //    var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
        //    var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
        //    var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
        //    if (_neutronLicense.CompanyCode == @"MET")
        //    {
        //        text = GetDisplayText();
        //    }
        //    else
        //    {
        //        text = _currentPickStop.QuantityToBePicked.ToString();
        //    }

        //    ShowShi(loc1, loc2, loc3, loc4, text);
        //}

        // Creates the text string that will show up on the tower displays
        // It uses the quantity and item number to create it
        private string GetDisplayText()
        {
            // Get the quantity
            var quantity = _currentPickStop.QuantityToBePicked.ToString();
            if (quantity.Length > 3) return quantity;
            // Get the first 2 chars of the Item number
            var start = _currentPickStop.Item.Length - 2;
            var twoChars = _currentPickStop.Item.Substring(start, 2);
            // Pad the Quantity with spaces to the right
            var paddedQuantity = quantity.PadRight(4, ' ');
            // Return the 6 character result
            var result = $"{paddedQuantity}{twoChars}";
            return result;
        }

        private void UpdateGroupBoxLocation(Inventory inventory)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]"));
            if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Rack)
            {
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
                TextBoxPickLoc1.Text = inventory.Location.Slot;
                TextBoxPickSlot.Visible = true;
                TextBoxPickSlot.Text = inventory.Location.Slot;
            }
            if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Blastzone)
            {
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
                TextBoxPickSlot.Visible = false;

                LabelDevice.Visible = false;
                LabelTray.Visible = true;
                LabelOver.Visible = false;
                LabelBack.Visible = true;
                TextBoxPickLoc1.Visible = false;
                TextBoxPickLoc2.Visible = true;
                TextBoxPickLoc3.Visible = false;
                TextBoxPickLoc4.Visible = true;
                TextBoxPickLoc5.Visible = false;

                TextBoxPickLoc1.Text = inventory.Location.Loc1.ToString();
                TextBoxPickLoc2.Text = inventory.Location.Loc2.ToString();
                TextBoxPickLoc3.Text = inventory.Location.Loc3.ToString();
                TextBoxPickLoc4.Text = inventory.Location.Loc4.ToString();
                TextBoxPickLoc5.Text = inventory.Location.Loc5.ToString();
            }
            if (inventory.Location.Area.LocationTypeId == (int)LocationTypeEnum.Vertical)
            {
                LabelDevice.Visible = true;
                LabelTray.Visible = true;
                LabelOver.Visible = true;
                LabelBack.Visible = true;
                TextBoxPickLoc1.Visible = true;
                TextBoxPickLoc2.Visible = true;
                TextBoxPickLoc3.Visible = true;
                TextBoxPickLoc4.Visible = true;
                TextBoxPickLoc5.Visible = true;
                TextBoxPickSlot.Visible = false;


                TextBoxPickLoc1.Text = inventory.Location.Loc1.ToString();
                TextBoxPickLoc2.Text = inventory.Location.Loc2.ToString();
                TextBoxPickLoc3.Text = inventory.Location.Loc3.ToString();
                TextBoxPickLoc4.Text = inventory.Location.Loc4.ToString();
                TextBoxPickLoc5.Text = inventory.Location.Loc5.ToString();
            }

            LabelLocationNumber.Text = $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = inventory.PrimeBin;
            LabelStaticRelease.Text = inventory.StorageType.Name;
            // ---         Task.Run(() => _logger.LogDetailAsync($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        //------------------------------------------
        private async Task UpdatePickPosition()
        {
            LogStartOfUpdate();
            SetOrderCompleteThisArea();
            UpdatePickPositions();
            UpdatePickDisplays();
            await ClearIptiDisplayFunctions();
            ClearProLites();
            await UpdatePickViews();
            await UpdateBlastzoneDisplay();
            TurnOnProlites();
            LogEndOfUpdate();
        }

        private void TurnOnProlites()
        {
            if (_workstationView.ProLiteManager == null) return;
            var currentLocation = _currentPickStop.CurrentInventoryLocation.Location;
            var device = currentLocation.Loc1;
            var bayController = currentLocation.Loc3;
            var display = currentLocation.Loc4;
            var totalQuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
            _workstationView.ProLiteManager?.TurnOn(device, bayController, display, totalQuantityToBePicked);
        }


        private void LogStartOfUpdate()
        {
            _logger.LogDetailAsync($"UpdatePickPosition START").SafeFireAndForget();
        }
        private void LogEndOfUpdate()
        {
            _logger.LogDetailAsync($"UpdatePickPosition END").SafeFireAndForget();
        }
        private async Task ClearIptiDisplayFunctions()
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.ClearBlastzone();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                //await _iptiDisplayFunctions.TurnOnBatchOrderControl(_currentPickStop.Item.Trim());
            }

        }

        private async Task UpdatePickViews()
        {
            foreach (var pickView in _currentPickStop.PickViews)
            {
                await UpdatePickView(pickView);
            }
        }
        private async Task UpdatePickView(PickView pickView)
        {
            var pos = pickView.PickPosition;
            var textBox = (TextBox)Controls.Find($"TextBoxPickPos{pos}", true).First();
            if (textBox != null)
            {
                textBox.Font = new Font(textBox.Font.FontFamily, 26);
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
        private async Task UpdateBlastzoneDisplay()
        {
            if (_iptiDisplayFunctions != null)
            {
                var device = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var bayController = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var display = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                await _iptiDisplayFunctions.TurnOnBlastzoneDisplay(bayController, display,
                    _currentPickStop.GetTotalQuantityToBePicked().ToString());
                await _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(bayController, _currentPickStop.Item.Trim());
            }

        }




        //------------------------------------------

        /// <summary>
        /// Updates the Pick Positions on the screen
        /// </summary>
        //private async Task UpdatePickPositionOld()
        //{
        //    _logger.LogDetailAsync($"UpdatePickPosition START").SafeFireAndForget();
        //    // Sets the OrderComplete flag in the OrderToPick batch file
        //    SetOrderCompleteThisArea();
        //    _logger.LogDetailAsync($"{DateTime.Now}  Sets the OrderComplete flag in the OrderToPick batch file").SafeFireAndForget();
        //    // Clear the Pick Positions
        //    ClearPickPositions();
        //    _logger.LogDetailAsync($"{DateTime.Now}   Clear the Pick Positions").SafeFireAndForget();
        //    // Clear the Pick Displays
        //    ClearPickDisplays();
        //    _logger.LogDetailAsync($"{DateTime.Now}  Clear the Pick Displays").SafeFireAndForget();

        //    if (_iptiDisplayFunctions != null)
        //    {
        //        await _iptiDisplayFunctions.ClearBatchTable();
        //        await _iptiDisplayFunctions.ClearBlastzone();
        //        await _iptiDisplayFunctions.TurnOnBatchOrderControl(_currentPickStop.Item.Trim());
        //    }

        //    _workstationView.ProLiteManager?.ClearAllProlites();

        //    foreach (var pickView in _currentPickStop.PickViews)
        //    {
        //        var pos = pickView.PickPosition;
        //        var textBox = (TextBox)Controls.Find($"TextBoxPickPos{pos}", true).First();
        //        if (textBox != null)
        //        {
        //            textBox.Font = new Font(textBox.Font.FontFamily, 26);
        //            //textBox.SizeTextBoxFont(1);
        //            textBox.Text = pickView.QuantityToBePicked.ToString();
        //        }
        //        var panel = (Panel)Controls.Find($"Pos{pos}Display", true).First();
        //        if (panel != null)
        //        {
        //            panel.BackColor = Color.Red;
        //        }

        //        if (_iptiDisplayFunctions != null)
        //        {
        //            await _iptiDisplayFunctions.TurnOnBatchDisplay(pos, pickView.QuantityToBePicked.ToString());
        //        }
        //    }

        //    var device = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
        //    var bayController = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
        //    var display = _currentPickStop.CurrentInventoryLocation.Location.Loc4;


        //    if (_iptiDisplayFunctions != null)
        //    {
        //        await _iptiDisplayFunctions.TurnOnBlastzoneDisplay(bayController, display,
        //            _currentPickStop.GetTotalQuantityToBePicked().ToString());
        //        await _iptiDisplayFunctions.TurnOnBlastzoneOrderControl(bayController, _currentPickStop.Item.Trim());
        //    }

        //    _workstationView.ProLiteManager?.TurnOn(device, bayController, display, _currentPickStop.GetTotalQuantityToBePicked());

        //    _logger.LogDetailAsync($"UpdatePickPosition END").SafeFireAndForget();
        //}


        /// <summary>
        /// Clear and Reset all Order Positions on Induction screen
        /// </summary>
        private void ClearOrderPositions(List<BatchPosition> batchPositions)
        {
            if (batchPositions == null) return;
            foreach (var batchPosition in batchPositions)
            {
                var pos = batchPosition.PositionNumber.ToString();
                var control = Controls.Find($"TextBoxPos{pos}", true).FirstOrDefault();
                if (control != null)
                {
                    var textBox = ((TextBox)control);
                    textBox.Text = string.Empty;
                }
            }
        }

        //Clear the pickbox Text on the OrderSelection Screen
        private void ClearTextBoxPickPositions(List<BatchPosition> batchPositions)
        {
            if (batchPositions == null) return;
            foreach (var batchPosition in batchPositions)
            {
                var pos = batchPosition.PositionNumber.ToString();
                var control = Controls.Find($"TextBoxPickPos{pos}", true).FirstOrDefault();
                if (control != null)
                {
                    var textBox = ((TextBox)control);
                    textBox.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// When all lines have been picked for an order in this area
        /// The OrderComplete flag in the OrderToPick batch file is set to true
        /// </summary>
        private void SetOrderCompleteThisArea()
        {
            var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();
            foreach (var bp in from bp in ordersToPick
                               where bp.OrderId != 0
                               let linesNotComplete = _repoOrderDetails
                                   .FindBy(r => r.OrderId == bp.OrderId && r.AreaId == _workstationView.AreaId)
                                   .Where(r => r.LineStatusId != (int)LineStatus.Complete).ToList()
                               where !linesNotComplete.Any()
                               select bp)
            {
                bp.OrderComplete = true;
            }
        }
        /// <summary>
        /// Clears the text of the TextBox control associated with the specified pick position.
        /// </summary>
        /// <param name="position">The position of the pick to be cleared. This is used to find the TextBox control.</param>
        private void ClearPickPosition(string position)
        {
            var control = Controls.Find($"TextBoxPickPos{position}", true).FirstOrDefault();
            if (control != null)
            {
                var textBox = control as TextBox;
                if (textBox != null)
                {
                    textBox.Text = string.Empty;
                }
            }
        }
        /// <summary>
        /// Updates the positions of the orders to be picked by filtering and processing the list of orders.
        /// </summary>
        /// <remarks>
        /// This method filters the list of orders to pick, excluding those with an <see cref="BatchPosition.OrderId"/> of 0.
        /// It then iterates through the filtered list and updates the display for each order.
        /// </remarks>
        private void UpdatePickPositions()
        {
            var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();
            foreach (var order in ordersToPick)
            {
                UpdateOrderDisplay(order);
            }
        }

        private void ClearPickPositions(List<BatchPosition> batchPositions)
        {
            //var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();
            foreach (var order in batchPositions)
            {
                ClearOrderDisplay(order);
            }
        }
        private void UpdateOrderDisplay(BatchPosition order)
        {
            var control = FindOrderControl(order);
            if (control is TextBox textBox)
            {
                textBox.Text = order.OrderComplete ? "END" : string.Empty;
            }
        }

        private void ClearOrderDisplay(BatchPosition order)
        {
            var control = FindOrderControl(order);
            if (control is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
        }
        private Control FindOrderControl(BatchPosition order)
        {
            var position = order.PositionNumber.ToString();
            var controlName = $"TextBoxPickPos{position}";
            return Controls.Find(controlName, true).FirstOrDefault();
        }

        /// <summary>
        /// Clears all the Pick Positions
        /// </summary>
        //private void ClearPickPositions()
        //{
        //    var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();
        //    // var font = new Font("Microsoft San Serif", 24);
        //    foreach (var bp in ordersToPick)
        //    {
        //        var pos = bp.PositionNumber.ToString();
        //        var c = Controls.Find($"TextBoxPickPos{pos}", true).First();
        //        if (c != null)
        //        {
        //            var textBox = ((TextBox)c);
        //            // if the order is complete, show END in the position box
        //            // else show nothing
        //            textBox.Text = bp.OrderComplete ? "END" : string.Empty;
        //        }
        //    }
        //}
        /// <summary>
        /// Clear the Pick Displays
        /// </summary>
        private void ClearPickDisplays(List<BatchPosition> batchPositions)
        {
           // var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();

            foreach (var bp in batchPositions)
            {
                var pos = bp.PositionNumber.ToString();
                var c = Controls.Find($"Pos{pos}Display", true).First();
                if (c != null)
                {
                    var panel = (Panel)c;
                    // if the order is complete, turn the panel back color green
                    panel.BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// Updates the display panels associated with the pick positions.
        /// </summary>
        /// <remarks>
        /// This method iterates through the list of orders to pick, identifies the corresponding display panel
        /// for each position, and updates its background color based on the completion status of the order.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a control corresponding to a position cannot be found or is not of the expected type.
        /// </exception>
        private void UpdatePickDisplays()
        {
            var ordersToPick = _ordersToPick.Where(r => r.OrderId != 0).ToList();

            foreach (var bp in ordersToPick)
            {
                var pos = bp.PositionNumber.ToString();
                var c = Controls.Find($"Pos{pos}Display", true).First();
                if (c != null)
                {
                    var panel = (Panel)c;
                    // if the order is complete, turn the panel back color green
                    panel.BackColor = bp.OrderComplete ? Color.Green : Color.Transparent;
                }
            }
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
                // ---         Task.Run(() => _logger.LogDetailAsync($"SkipPick_Click Start : [{DateTime.Now.ToLongTimeString()}]"));

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
                    // SetOrderDetailLineStatus(pickView.OrderDetail, (int)LineStatus.Skipped, ActionCode.Skip);
                    // Wagner wanted lines to go on HOLD where Skipped
                    SetOrderDetailLineStatus(pickView.OrderDetail, (int)LineStatus.Hold, ActionCode.HoldLine);
                }

                //----------
                // ---         Task.Run(() => _logger.LogDetailAsync($"History Done"));

                var numberOfStops = _bindingSourcePickStops.Count;
                if (_currentPickStop.Sequence < numberOfStops)
                {
                    // ---         Task.Run(() => _logger.LogDetailAsync("Clear Active Device Indicator - SkipPick"));

                    _bindingSourcePickStops.MoveNext();
                    _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                    await UpdatePickScreen();
                    // UpdateCurrentDeviceIndicator();

                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

                    await UpdatePickPosition();

                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    // UpdateTowerDisplay();
                }
                else
                {
                    // ---         Task.Run(() => _logger.LogDetailAsync($"Close Batch With Skip"));
                    var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();
                    await CloseBatchWithSkip(batchPositions);
                }

                // ---         Task.Run(() => _logger.LogDetailAsync($"SkipPick_Click End : [{DateTime.Now.ToLongTimeString()}]"));
                Cursor.Current = Cursors.Default;
                MBSkipPick.Enabled = true;

            }
        }

        private async Task CloseBatchWithSkip(List<BatchPosition> batchPositions)
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
            }


            //ClearOrderPositions();

            await ClearBatchPositions(batchPositions);

            _deviceIndicatorManager?.ClearAllDeviceIndicators();



            //if (_neutronLicense.CompanyCode == "SFH")
            //{
            //    var shortReportProcessor = new ShortReportProcessor(_bindingSourcePickStops, _labelPrinterPreferences);
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
            // ---  _ =        Task.Run(() => _logger.LogDetailAsync($"CloseBatchWithSkip END"));
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

        private async void MBPickAccept_Click(object sender, EventArgs e)
        {
            await PickAccept();
        }

        private async Task PickAccept()
        {
            // NeutronDllu exit to do something with the selection
            // before processing the pick. 
            // For example: Ask operator to scan a lot or serial number
            // Verify something else
            // Returns true is process is to continue
            // Return false if the process is canceled
            if (!SelectAction.Accept(_currentPickStop)) return;



            _spaceBarDisabled = true;

            _logger.LogDetailAsync($"Pick Accept Button Pressed").SafeFireAndForget();

            //if (InvokeRequired)
            //{
            //    var method = new MethodInvoker(PickAccept);
            //    Invoke(method);
            //    return;
            //}

            Cursor.Current = Cursors.WaitCursor;

            //var thisPick = IntegerExtensions.ParseInt(LabelPickQty.Text);

            // var pick = false;

            //var zeroInventory = _currentPickStop.CurrentInventoryLocation.Quantity <= _currentPickStop.QuantityToBePicked;

            //if (zeroInventory)
            //{
            //    // in case a hot action or Location Count changes the current inventory
            //    // let's refresh the currentInventory
            //    var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            //    _currentPickStop.CurrentInventoryLocation.Quantity = GetCurrentInventoryLocationQuantity(inventoryId);

            //    zeroInventory = _currentPickStop.CurrentInventoryLocation.Quantity <= _currentPickStop.QuantityToBePicked;

            //    if (zeroInventory)
            //    {
            //        var prompt = new StringBuilder();
            //        prompt.AppendLine($"Release Inventory Location{Environment.NewLine}Item: {_currentPickStop.Item}{Environment.NewLine}Location: {GetInventoryLocation(_currentPickStop.CurrentInventoryLocation.Location)} ?");

            //        var result = await _dialogService.Show2Async("Release Inventory Location",
            //            $"{prompt}", "Yes", "No");
            //        if (result == true)
            //        {
            //            //await _historyManager.SaveHistoryAsync(ActionCode.InventoryDelete, _currentPickStop.CurrentInventoryLocation);
            //            //await _repoInventory.DeleteAsync(inventory.Id);
            //        }
            //        else
            //        {
            //            var qty = await LocationCount(_currentPickStop.CurrentInventoryLocation.Id);
            //            if (qty == -1)
            //            {
            //                MBPickAccept.Visible = true;
            //                return;
            //            }
            //        }

            //    }
            //}

            MBPickAccept.Visible = false;
            var batchComplete = false;


            var enoughInventory = _currentPickStop.CurrentInventoryLocation.Quantity >= _currentPickStop.QuantityToBePicked;

            _logger.LogDetailAsync($"Check Inventory End.  Enough Inventory: {enoughInventory}    ").SafeFireAndForget();


            if (enoughInventory) //there is enough inventory at this location
            {
                _logger.LogDetailAsync($"Update Pick Views - 1").SafeFireAndForget();

                _currentPickStop.UpdatePickViews(GlobalVar.User); //good

                _logger.LogDetailAsync($"Update Pick Views - 2").SafeFireAndForget();

                _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);

                _logger.LogDetailAsync($"Update Pick Views - 3").SafeFireAndForget();

                _currentPickStop.QuantityToBePicked =
                    GetTotalQuantityToBePicked(_currentPickStop.PickViews); // QuantityToBePicked on ALL PickViews

                _logger.LogDetailAsync($"Update Pick Views - 4").SafeFireAndForget();

                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);

                _logger.LogDetailAsync($"Update Pick Views - 5").SafeFireAndForget();

                _currentPickStop.TotalQuantityInInventory = total;

                _logger.LogDetailAsync($"Update Pick Views - 6").SafeFireAndForget();

                TextBoxTotalQuantity.Text = total.ToString();


                _logger.LogDetailAsync($"Update PickViews End").SafeFireAndForget();
                // ---         Task.Run(() => _logger.LogDetailAsync($"PickAccept 1 : [{DateTime.Now.ToLongTimeString()}]"));

                bool stopComplete;
                // if there's only one inventory location and there is still quantity to pick
                // you must short pick the item
                if (_shortPick || _currentPickStop.Inventory.Count == 1) // set when the Short Pick button is pressed.
                {
                    stopComplete = true;
                    _shortPick = false;
                }
                else
                {
                    stopComplete = StopComplete();
                }


                if (stopComplete)
                {
                    _logger.LogDetailAsync($"PickAccept 2 Stop Complete Start ").SafeFireAndForget();

                    //Getting next location on the current device/ the one that was just picked from.
                    // ---         Task.Run(() => _logger.LogDetailAsync($"PickAccept 2 Stop Complete Start "));

                    await UpdateInventoryQuantity(_currentPickStop);

                    _logger.LogDetailAsync($"Update Inventory Quantity End").SafeFireAndForget();

                    // ---         Task.Run(() => _logger.LogDetailAsync($"UpdateInventoryQuantity"));

                    // ---         Task.Run(() => _logger.LogDetailAsync($"History Done"));

                    _currentPickStop.SetPickViewsComplete(GlobalVar.User, _logger);
                    _logger.LogDetailAsync($" Set Pick Views Complete End").SafeFireAndForget();

                    foreach (var pickView in _currentPickStop.PickViews)
                    {
                        if (await CheckForOrderCompleteOnAllStations(pickView.OrderDetail.Order))
                        {
                            if (_neutronVariables.PrintPackingListEnd)
                            {
                                PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
                            }
                        }
                    }
                    _logger.LogDetailAsync($"Check for Order Complete End").SafeFireAndForget();
                    // ---         Task.Run(() => _logger.LogDetailAsync($"PickAccept Stop Complete End "));

                    var numberOfStops = _bindingSourcePickStops.Count;

                    //var position = _bindingSourcePickStops.Position;
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
                            if (_deviceManager != null)
                            {
                                _deviceManager.ResetMoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                            }



                            _multiLocationStop = false;
                        }
                        else
                        {
                            //Use the first carousel location for the movenext in case multiple picks are required for stop
                            if (_deviceManager != null)
                            {
                                _logger.LogDetailAsync($"MoveNext Start").SafeFireAndForget();
                                _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                                _logger.LogDetailAsync($"MoveNext End").SafeFireAndForget();
                            }
                        }

                        _bindingSourcePickStops.MoveNext();
                        _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                        if (_workstationView.Area.LocationTypeId != (int)LocationTypeEnum.Rack)
                        {
                            PrintLabels(_currentPickStop);
                        }
                        _logger.LogDetailAsync($"Print Labels End").SafeFireAndForget();

                        await UpdatePickScreen();
                        // UpdateCurrentDeviceIndicator();
                        _logger.LogDetailAsync($"Update Pick Screen End").SafeFireAndForget();
                        await UpdatePickPosition();

                        UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                        // UpdateTowerDisplay();
                        _logger.LogDetailAsync($"Update Group Box End").SafeFireAndForget();

                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
                        _logger.LogDetailAsync($"Update Current Device Indicator End").SafeFireAndForget();
                    }
                    else
                    {
                        // ---         Task.Run(() => _logger.LogDetailAsync($"CloseBatch"));
                        await CloseBatchAsync();
                        batchComplete = true;
                    }
                }
                else //PickStop is NOT complete, why?
                {
                    _multiLocationStop = true;
                    // ---         Task.Run(() => _logger.LogDetailAsync($"Pick Stop NOT Complete.  Next Location"));
                    var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                    var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                    var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                    var text = _currentPickStop.QuantityToBePicked.ToString();
                    // ---         Task.Run(() => _logger.LogDetailAsync($"Multi-Location Position Device: {loc1} - {loc2} - {loc3} - {loc4} - {text}     "));

                    PositionDevice(loc1, loc2, loc3, loc4, true);

                    await UpdatePickScreen();
                    //UpdateCurrentDeviceIndicator();

                    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

                    await UpdatePickPosition();
                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    // UpdateTowerDisplay();
                }
            }
            else // Not enough inventory at this location
            {
                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

                MessageBox.Show(text: _resourceManager.GetString($"PickExceedsInventory"),
                    caption: _resourceManager.GetString($"Inventory"), buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Stop);
            }

            _logger.LogDetailAsync($"PickAccept End : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
            Cursor.Current = Cursors.Default;

            if (!batchComplete)
            {
                //EnablePickAccept(true);

                // wait 3 seconds before enabling MBPickAccept button
                await Task.Delay(2000);

                MBPickAccept.Enabled = true;
                MBPickAccept.Visible = true;

                var focused = MBPickAccept.Focus();

                if (!focused)
                {
                    _logger.LogDetailAsync($"Focus Failed: Enabled:{MBPickAccept.Enabled} Visible:{MBPickAccept.Visible} Focused:{focused}").SafeFireAndForget();
                }
                // StartSpaceBarEnableTimer();
                _spaceBarDisabled = false;
            }
        }

        // SpecialBackorderProcess Form
        private async Task<bool> PickStopAdjustmentForm(PickView pickView)
        {
            //assume it will succeed
            var success = true;
            // set values of local variables in case we need to roll back
            var status = pickView.OrderDetail.LineStatusId;
            var quantity = pickView.QuantityToBePicked;

            using (var form = new FrmPickViewAdjustment(pickView))
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
                    await _repoOrderDetails.UpdateAsync(pickView.OrderDetail);
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

        private async Task SpecialPickAccept()
        {
            _logger.LogDetailAsync($"Special Pick Accept Run").SafeFireAndForget();

            if (InvokeRequired)
            {
                var method = new MethodInvoker(() => SpecialPickAccept().Wait());
                Invoke(method);
                return;
            }

            foreach (var pickView in _currentPickStop.PickViews)
            {
                //open a form to update the pickview
                if (await PickStopAdjustmentForm(pickView) != true) return;
            }
        }


        private async Task CompleteSpecialPick()
        {
            _logger.LogDetailAsync("Clear Active Device Indicator - Pick Accept").SafeFireAndForget();

            _deviceIndicatorManager?.ClearAllDeviceIndicators();

            foreach (var pickView in _currentPickStop.PickViews)
            {
                if (await CheckForOrderCompleteOnAllStations(pickView.OrderDetail.Order))
                {
                    if (_neutronVariables.PrintPackingListEnd)
                    {
                        PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
                    }
                }
            }

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
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
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
        /// Print all the labels for the current pickstop
        /// That means if there are multiple PickViews in this PickStop
        /// It will print a label for each order/PickView
        /// </summary>
        /// <param name="currentPickStop"></param>
        /// <param name="reqFunc"></param>
        /// <param name="pos"></param>
        private void PrintLabels(PickStop currentPickStop, int reqFunc = 1, int pos = 0)
        {
            _logger.LogDetailAsync("Print Labels").SafeFireAndForget();
            if (_neutronVariables.EnableLabelPrinter == false) return;

            foreach (var pickView in currentPickStop.PickViews)
            {
                PrintLabel(1, pickView.PickPosition, pickView);
                Thread.Sleep(50);
            }
        }
        //-----------------------------

        private void PrintLabel(int reqFunc, int pos, PickView pickView)
        {
            _logger.LogDetailAsync($"Print Label Start").SafeFireAndForget();
            try
            {

                var orderDetailInfo = GetOrderDetail(pickView);
                if (_neutronLicense.CompanyCode == "WAG")
                {
                    PrintLoftwareLabel(pickView, orderDetailInfo);
                }
                else
                {
                    PrintLabelWithPreferences(reqFunc, pos, pickView);
                }
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnDisplayMessage(this, $"Print Label Error: {ex.Message}");
            }
        }
        private string[] GetOrderDetail(PickView pickView)
        {
            var orderDetail = new string[OrderDetailPartsCount];
            try
            {
                var orderDetailInfo = pickView.OrderDetail.OrderDetailInfo;
                if (!string.IsNullOrEmpty(orderDetailInfo))
                {
                    var splitOrderDetailInfo = orderDetailInfo.Split('|');
                    if (splitOrderDetailInfo.Length == OrderDetailPartsCount)
                    {
                        orderDetail = splitOrderDetailInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnDisplayMessage(this, $"Error while getting order detail: {ex.Message}");
                // Consider rethrowing the exception if it can't be handled here
            }
            return orderDetail;
        }
        private void PrintLoftwareLabel(PickView pickView, string[] orderDetailInfo)
        {
            // join orderDetailInfo into a string
            var orderDetail = orderDetailInfo.Select(x => x.Trim()).ToArray();
            // join orderDetail into a string
            var orderDetailString = string.Join(" ", orderDetail);

            _logger.LogDetailAsync($"OrderDetail: {orderDetailString}").SafeFireAndForget();
            try
            {
                var loftwarePrinterPreferences = _jsonData.LoadFile<LoftwarePrinterPreferences>();
                if (string.IsNullOrEmpty(loftwarePrinterPreferences.LoftwareFilePath)) return;
                var upc = orderDetail[4].Trim();
                var item = pickView.Item.Trim();
                var quantity = pickView.QuantityToBePicked.ToString();
                var desc = pickView.Description.Trim();
                var division = orderDetail[1];
                LogLabelDetails(pickView.Ord1, pickView.Ord2, pickView.PickPosition, upc, desc, item, quantity);
                PrintLabel(loftwarePrinterPreferences, upc, item, quantity, desc, division);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnDisplayMessage(this, $"Print Loftware Label Error: {ex.Message}");
            }
        }
        private void LogLabelDetails(string ord1, string ord2, int pickPosition, string upc, string desc, string item, string quantity)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Loftware Label:");
            sb.AppendLine($"Time: {DateTime.Now.ToLongTimeString()}");
            sb.AppendLine($"Pick Position: {pickPosition}");
            sb.AppendLine($"Delivery: {ord1}  TO: {ord2}");
            sb.AppendLine($"UPC: {upc}  Desc: {desc}  Item: {item} Quantity: {quantity}");

            _logger.LogDetailAsync($"{sb}").SafeFireAndForget();
        }
        private void PrintLabel(LoftwarePrinterPreferences loftwarePrinterPreferences, string upc, string item, string quantity, string desc, string division)
        {
            _logger.LogDetailAsync($"Sending Label Data to ToteToPrint Function.");
            ToteToPrint.PrintLoftwareLabel(loftwarePrinterPreferences.LoftwareFilePath
                , loftwarePrinterPreferences.LoftwarePrinter
                , upc
                , item
                , item
                , quantity
                , desc
                , division);
        }

        /// <summary>
        /// Labels for NON WAG companies
        /// </summary>
        /// <param name="reqFunc"></param>
        /// <param name="pos"></param>
        /// <param name="pickView"></param>
        private void PrintLabelWithPreferences(int reqFunc, int pos, PickView pickView)
        {
            if (string.IsNullOrEmpty(_labelPrinterPreferences.PrinterName)) return;
            var upc = _repoAka.GetUpc(pickView.Item);
            var labelDetail = GetLabelDetail(pickView.OrderDetail);
            ToteToPrint.Print(reqFunc, pos, labelDetail, upc, _labelPrinterPreferences);
        }
        private LabelDetail GetLabelDetail(OrderDetail orderDetail)
        {
            return new LabelDetail
            {
                Item = orderDetail.ItemDefinition.Item,
                Description = orderDetail.ItemDefinition.Description,
                Quantity = orderDetail.Quantity,
                EmpId = GlobalVar.User.EmpId,
                Invoice = orderDetail.Order.Ord2,
                Order = orderDetail.Order.Ord1,
                LoadDate = orderDetail.Order.LoadDate,
                Origin = orderDetail.OrderDetailInfo == null ? string.Empty : orderDetail.OrderDetailInfo.Trim(),
                UnitOfIssue = orderDetail.ItemDefinition.UnitOfIssue.Name
            };
        }

        private int GetCurrentInventoryLocationQuantity(int inventoryLocationId)
        {
            var result = 0;
            var inv = _repoInventory.FindByKey(inventoryLocationId);
            if (inv != null)
            {
                result = inv.Quantity;
            }
            return result;
        }

        public int GetTotalQuantityToBePicked(IList<PickView> pickViews)
        {
            var total = 0;
            foreach (var pickview in pickViews)
            {
                total += pickview.QuantityToBePicked;
            }
            return total;
        }

        private int GetPickedSoFar(IList<PickView> pickViews)
        {
            var total = 0;
            foreach (var pickview in pickViews)
            {
                total += GetPickViewTotal(pickview);
            }
            return total;
        }

        private int GetPickViewTotal(PickView pickview)
        {
            var total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }
            return total;
        }

        private bool StopComplete()
        {
            var result = false;
            if (_currentPickStop.QuantityToBePicked == 0)
            {
                result = true;
            }
            else if (_currentPickStop.QuantityToBePicked < 0)
            {
                var response = MessageBox.Show(_resourceManager.GetString($"OverPickItem"), _resourceManager.GetString($"OverPickCaption")
                    , MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                result = response == DialogResult.Yes;
            }
            else if (_currentPickStop.QuantityToBePicked > 0)
            {
                //does the stop have more than one inventory lcoation
                //var b = GetNextInventoryLocation();
                var b = NextPickingLocation();
                if (b)
                {
                    result = false;
                }
                else  // no more locations
                {
                    var response = MessageBox.Show(_resourceManager.GetString($"ShortPickQuestion"), _resourceManager.GetString($"ShortPickCaption"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    result = response == DialogResult.Yes;
                }
            }

            return result;
        }

        private async Task UpdateInventoryQuantity(PickStop pickStop)
        {
            // check pickStop for null
            if (pickStop == null) return;

            foreach (var pickView in pickStop.PickViews)
            {
                await UpdatePickViewInventoryQuantity(pickView);
            }
        }

        /// <summary>
        /// Updates the inventory quantity for the specified <see cref="PickView"/>.
        /// </summary>
        /// <param name="pickView">The <see cref="PickView"/> object containing the details of the pick operation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pickView"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pickView"/> has null <c>PickLocations</c> or <c>Inventory</c>.</exception>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task UpdatePickViewInventoryQuantity(PickView pickView)
        {
            if (pickView == null)
                throw new ArgumentNullException(nameof(pickView));
            if (pickView.PickLocations == null || pickView.Inventory == null)
                throw new ArgumentException("PickLocations or Inventory is null");
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
        private async Task ProcessPickLocations(PickView pickView, StringBuilder logMessage)
        {
            foreach (var pickLocation in pickView.PickLocations)
            {
                logMessage.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
                var inventory = await _repoInventory.FindByKeyIncludeAsync(r => r.Id == pickLocation.Inventory.Id, r => r.Location);
                if (inventory != null)
                {
                    logMessage.AppendLine($"Inventory Qty: {inventory.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
                    if (inventory.Quantity >= pickLocation.Quantity)
                    {
                        inventory.Quantity -= pickLocation.Quantity;
                        await _repoInventory.UpdateAsync(inventory);
                        logMessage.AppendLine("Update Inventory");
                        await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, pickLocation.Quantity, pickView);
                        logMessage.AppendLine("Write Pick Order to History");
                        //if (inventory.Quantity == 0)
                        //{
                        //    await _historyManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
                        //    await _repoInventory.DeleteAsync(inventory.Id);
                        //}
                    }
                }
                else
                {
                    logMessage.AppendLine("Inventory is NULL.");
                }
            }
        }

        private string GetInventoryLocation(Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (location.AreaId == AreaEight)
            {
                return location.Slot;
            }

            var slot =
                $"{location.Loc1.ToString().PadLeft(2, '0')}-{location.Loc2.ToString().PadLeft(2, '0')}-{location.Loc3.ToString().PadLeft(2, '0')}-{location.Loc4.ToString().PadLeft(2, '0')}";
            return slot;
        }

        private async Task ProcessInventory(PickView pickView, StringBuilder logMessage)
        {
            var inventory = pickView.Inventory.FirstOrDefault();
            if (inventory != null)
            {
                await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, 0, pickView);
                logMessage.AppendLine("Write Pick Order of zero quantity to History");
            }
        }


        //private async Task zUpdatePickViewInventoryQuantity(PickView pickView)
        //{
        //    if (pickView == null) throw new ArgumentNullException(nameof(pickView));
        //    if (pickView.PickLocations == null || pickView.Inventory == null) throw new ArgumentException("PickLocations or Inventory is null");
        //    var sb = new StringBuilder();
        //    sb.AppendLine($"Update Inventory Quantity for PICKVIEW Item: {pickView.Item} - Start");
        //    sb.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");
        //    try
        //    {
        //        if (pickView.PickLocations.Any())
        //        {
        //            foreach (var pickLocation in pickView.PickLocations)
        //            {
        //                sb.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
        //                var inventory = await _repoInventory.FindByKeyIncludeAsync(r => r.Id == pickLocation.Inventory.Id, r => r.Location);
        //                if (inventory != null)
        //                {
        //                    sb.AppendLine($"Inventory Qty: {inventory.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
        //                    if (inventory.Quantity >= pickLocation.Quantity)
        //                    {
        //                        inventory.Quantity -= pickLocation.Quantity;
        //                        await _repoInventory.UpdateAsync(inventory);
        //                        sb.AppendLine("Update Inventory");
        //                        await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, pickLocation.Quantity, pickView);
        //                        sb.AppendLine("Write Pick Order to History");
        //                        if (inventory.Quantity == 0)
        //                        {
        //                            var result = await _dialogService.ShowAsync("Release Inventory Location",
        //                                $"Release Inventory Location for {pickView.Item} at {inventory.Location.Slot}?", "Yes", "No");
        //                            if (result)
        //                            {
        //                                await _historyManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
        //                                await _repoInventory.DeleteAsync(inventory.Id);
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    sb.AppendLine($"Inventory is NULL.");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var inventory = pickView.Inventory.FirstOrDefault();
        //            if (inventory != null)
        //            {
        //                await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, 0, pickView);
        //                sb.AppendLine("Write Pick Order of zero quantity to History");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.LogDetailAsync($"{ex.Message}");
        //    }
        //    await _logger.LogDetailAsync($"{sb}");
        //}

        //private async Task UpdatePickViewInventoryQuantity(PickView pickView)
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine($"Update Inventory Quantity for PICKVIEW Item: {pickView.Item} - Start");
        //    sb.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");

        //    try
        //    {
        //        if (pickView.PickLocations.Any())
        //        {
        //            foreach (var pickLocation in pickView.PickLocations)
        //            {
        //                sb.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
        //                //  pickLocation.Inventory.Quantity -= pickLocation.Quantity;

        //                //var inventory = _repoInventory.FindByKey(pickLocation.Inventory.Id);
        //                var inventory = await _repoInventory.FindByKeyIncludeAsync(r => r.Id == pickLocation.Inventory.Id,r => r.Location );
        //                if (inventory != null)
        //                {
        //                    sb.AppendLine($"Inventory Qty: {inventory.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
        //                    if (inventory.Quantity > pickLocation.Quantity)
        //                    {
        //                        inventory.Quantity -= pickLocation.Quantity;
        //                        await _repoInventory.UpdateAsync(inventory);
        //                        sb.AppendLine("Update Inventory");
        //                        await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, pickLocation.Quantity, pickView);
        //                        sb.AppendLine("Write Pick Order to History");
        //                    }
        //                    if (inventory.Quantity == pickLocation.Quantity)
        //                    {
        //                        inventory.Quantity -= pickLocation.Quantity;
        //                        await _repoInventory.UpdateAsync(inventory);
        //                        sb.AppendLine("Update Inventory");
        //                        await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, pickLocation.Quantity, pickView);
        //                        sb.AppendLine("Write Pick Order to History");
        //                        // prompt for Release Inventory Location

        //                        var result = await _dialogService.ShowAsync("Release Inventory Location",
        //                            $"Release Inventory Location for {pickView.Item} at {inventory.Location.Slot}?", "Yes", "No");
        //                        if (result)
        //                        {
        //                            await _historyManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
        //                            await _repoInventory.DeleteAsync(inventory.Id);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    sb.AppendLine($"Inventory is NULL.");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // no PickLocations so no inventory
        //            // use the PickViews first Inventory Location by default
        //            var inventory = pickView.Inventory[0];
        //            await _historyManager.SaveHistoryAsync(ActionCode.PickOrder, inventory, 0, pickView);
        //            sb.AppendLine("Write Pick Order of zero quantity to History");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"{ex.Message}").SafeFireAndForget();
        //    }

        //    _logger.LogDetailAsync($"{sb}").SafeFireAndForget();
        //}

        private async Task CloseBatchAsync()
        {
            var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();
            
            LogCloseBatchStart();
            ResetHanelDeviceStatus();
            await UpdateOrderStatusAsync();
           // await ClearBatchRelatedData(batchPositions);
            ClearAllDeviceIndicators();
            ClearProLites();
            ParkPositionAfterBatch();
            await ClearBatchPositions(batchPositions);
            //ClearPickPositions(batchPositions);
            //ClearOrderPositions();
            //ClearPickDisplays(batchPositions);
            await ClearInductionScreen(batchPositions);
            await HandlePostBatchActions();
        }

        private async Task UpdateOrderStatusAsync()
        {
            foreach (var bp in _ordersToPick)
            {
                var order = _repoOrders.FindByKey(bp.OrderId);
                if (order == null) continue;
                await CheckForOrderComplete(order);
            }
        }

        private void LogCloseBatchStart()
        {
            _logger.LogDetailAsync("CloseBatch START").SafeFireAndForget();
        }

        private async Task ClearBatchRelatedData(List<BatchPosition> batchPositions)
        {
            if (_iptiDisplayFunctions != null)
            {
                await _iptiDisplayFunctions.ClearBatchTable();
                await _iptiDisplayFunctions.TurnOffBatchOrderControl();
                await _iptiDisplayFunctions.ClearBlastzone();
            }
            //ClearOrderPositions();
            await ClearBatchPositions(batchPositions);
            LogClearAllDeviceIndicators();
        }
        private void LogClearAllDeviceIndicators()
        {
            _logger.LogDetailAsync("Clear All Device Indicators - Close Batch").SafeFireAndForget();
        }
        private void ClearAllDeviceIndicators()
        {
            _deviceIndicatorManager?.ClearAllDeviceIndicators();
        }
        private async Task HandlePostBatchActions()
        {
            if (_neutronVariables.AutoLogOff)
            {
                CloseButtonPressed = true;
                Close();
            }
            else
            {
                await AvailableOrdersScreen();
            }
        }

        //private async Task CloseBatchAsync()
        //{
        //    Task.Run(() => _logger.LogDetailAsync($"CloseBatch START")).SafeFireAndForget();

        //    GlobalVar.Hanel?.ResetHanelDeviceStatus();
        //    if (_iptiDisplayFunctions != null)
        //    {
        //        await _iptiDisplayFunctions.ClearBatchTable();
        //        await _iptiDisplayFunctions.TurnOffBatchOrderControl();
        //        await _iptiDisplayFunctions.ClearBlastzone();
        //    }

        //    ClearOrderPositions();
        //    await ClearBatchPositions();
        //    _logger.LogDetailAsync($"Clear All Device Indicators - Close Batch").SafeFireAndForget();

        //    _deviceIndicatorManager?.ClearAllDeviceIndicators();

        //    ClearProLites();

        //    ParkPositionAfterBatch();

        //    if (_neutronVariables.AutoLogOff)
        //    {
        //        CloseButtonPressed = true;
        //        Close();
        //    }
        //    else
        //    {
        //        await AvailableOrdersScreen();
        //    }
        //}

        //private async Task DeleteRelease()
        //{
        //    _logger.LogDetailAsync($"DeleteRelease Start").SafeFireAndForget();
        //    // check for Inventory locations that need to be Released
        //    //var locationIds = new List<int>();
        //    var inventoryManager = new InventoryManager(_repoInventory, _locationsRepository);

        //    try
        //    {
        //        var invs = await _inventoryRepository.GetInventoryWithReleaseStorageAndZeroQuantityByArea(_workstationView.AreaId);

        //        if (invs.Count > 0)
        //        {
        //            var sb = new StringBuilder();
        //            foreach (var inv in invs)
        //            {
        //                var canDelete = await inventoryManager.QuickReleaseCheckAsync(inv);
        //                if (!canDelete) continue;
        //                var item = inv.ItemDefinition != null ? inv.ItemDefinition.Item : string.Empty;
        //                var storageType = inv.StorageType != null ? inv.StorageType.Name : string.Empty;
        //                sb.AppendLine($"Are you sure you want to DELETE the selected Inventory Item? {Environment.NewLine}" +
        //                              $"ItemId: {inv.ItemDefinitionId} Item: {item} LocationId: {inv.LocationId}" +
        //                              $"Quantity: {inv.Quantity} AreaId: {inv.AreaId} Storage: {storageType} ");
        //                // locationIds.Add(inv.LocationId);
        //                var result = MessageBox.Show($"{sb.ToString()}", "Delete Inventory Item",
        //                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        //                if (result == DialogResult.No)
        //                {
        //                    _logger.LogDetailAsync($"{sb.ToString()}").SafeFireAndForget();
        //                    continue;
        //                }

        //                await inventoryManager.ReleaseCheckAsync(inv);
        //            }



        //            //foreach (var locationId in locationIds)
        //            //{
        //            //    // Look for other items in inventory where the location is the same.
        //            //    // Don't want to change InUse to False is there are other items using this location.
        //            //    _logger.LogDetailAsync($"Check the locations for other items.  Don't set InUse flag to false if other items in location.").SafeFireAndForget();
        //            //    var items = await _repoInventory.FindByAsync(r => r.LocationId == locationId);
        //            //    if (!items.Any())
        //            //    {
        //            //        var location = _repoLocation.FindByKey(locationId);
        //            //        if (location == null) continue;
        //            //        _logger.LogDetailAsync($"Set Location InUse to False. Location ID: {location.Id}").SafeFireAndForget();
        //            //        location.InUse = false;
        //            //        await _repoLocation.UpdateAsync(location);
        //            //    }
        //            //    else
        //            //    {
        //            //        _logger.LogDetailAsync($"Location still has items associated with it.  InUse remains True.").SafeFireAndForget();
        //            //    }
        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogDetailAsync($"DeleteRelease Error: {ex.Message}").SafeFireAndForget();
        //    }
        //    _logger.LogDetailAsync($"DeleteRelease End").SafeFireAndForget();

        //}
        /// <summary>
        /// Print all the documents for the current batch of Orders to Pick
        /// </summary>
        private void PrintAllDocuments()
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            if (!_neutronVariables.PrintPackingListStart) return;
            // loop through all the orders in the batch/ OrdersToPick
            foreach (var bp in _ordersToPick)
            {
                // if the batch position is zero, it's not a real order, so skip it
                if (bp.OrderId == 0) continue;
                // get the orderId
                var id = bp.OrderId;

                //Check to see if it has already been printed, if it has continue without printing.
                // There is a PrintJob table that keeps track of what has been printed.
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == id && r.PickDocument == true).FirstOrDefault();
                // if it's not null, it's already been printed, so skip it
                if (printJob != null) continue;
                // print the document
                PrintPackingList(id, bp.PositionNumber.ToString());
                var order = _repoOrders.FindByKey(id);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDocument(int batchPosition)
        {
            var bp = _ordersToPick.FirstOrDefault(o => o.PositionNumber == batchPosition);
            if (bp == null) return;
            if (bp.OrderId == 0) return;
            var id = bp.OrderId;
            var order = _repoOrders.FindByKey(id);
            var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
            if (printJob == null)
            {
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private List<AnticipatedOut> GetAnticipatedOuts()
        {
            var anticipatedOuts = new List<AnticipatedOut>();
            using (var context = new NeutronDb())
            {
                anticipatedOuts = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOuts").ToList();
            }
            return anticipatedOuts;
        }

        private List<AnticipatedOut> GetAnticipatedOutsByWorkstation(int workstationId)
        {
            var anticipatedOuts = new List<AnticipatedOut>();
            using (var context = new NeutronDb())
            {
                var paramStation = new SqlParameter("@WorkstationId", workstationId);

                anticipatedOuts = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOutsByWorkstation @WorkstationId", paramStation).ToList();
            }
            return anticipatedOuts;
        }

        private List<AnticipatedOut> GetAnticipatedOutsByArea(int areaId)
        {
            List<AnticipatedOut> anticipatedOuts;
            using (var context = new NeutronDb())
            {
                var paramStation = new SqlParameter("@AREAID", areaId);

                anticipatedOuts = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOutsByArea @AREAID", paramStation).ToList();
            } // SQL Tested
            return anticipatedOuts;
        }

        private List<AnticipatedOut> GetAnticipatedOutsByOrder(int orderId)
        {
            var anticipatedOuts = new List<AnticipatedOut>();
            using (var context = new NeutronDb())
            {
                var paramStation = new SqlParameter("@ORDERID", orderId);

                anticipatedOuts = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOutsByOrder @ORDERID", paramStation).ToList();
            }
            return anticipatedOuts;
        }

        //private void PrintAnticipatedOuts()
        //{
        //    if (!_neutronVariables.EnableDocumentPrinter) return;
        //    List<AnticipatedOut> outs = new List<AnticipatedOut>();
        //    var comboBoxValue = ComboBoxStationNumber.Text;
        //    var anticipatedOuts = GetAnticipatedOuts();

        //    if (anticipatedOuts.Count <= 0) return;

        //    if (comboBoxValue != _resourceManager.GetString($"ALL"))
        //    {
        //        var workstationId = IntegerExtensions.ParseInt(comboBoxValue);
        //        anticipatedOuts = anticipatedOuts.Where(r => r.Workstation == workstationId).ToList();
        //    }
        //    _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        //}

        private void MBPrintPick_Click(object sender, EventArgs e)
        {
            var comboBoxValue = ComboBoxAreaNumber.Text;
            var orders = GetSelectedOrders(DataGridView1);
            if (!orders.Any()) return;

            if (comboBoxValue == _resourceManager.GetString($"ALL"))
            {
                foreach (var order in orders)
                {
                    PrintPickListAll(order.Id);
                }
            }
            else
            {
                var areaId = _areaRepository.GetAreaId(comboBoxValue.ParseInt());
                //var workstationId = IntegerExtensions.ParseInt(comboBoxValue);
                foreach (var order in orders)
                {
                    PrintPickListByArea(order.Id, areaId);
                }
            }
        }
        /// <summary>
        /// Prints the Packing List for the selected order and batch Position
        /// The batch position is optional
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="batchPosition"></param>
        private void PrintPackingList(int orderId, string batchPosition = "")
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            // get the packing list data for the order
            var pickSlips = GetPickSlipData(orderId);
            // if there is no data, exit
            if (!pickSlips.Any()) return;
            // if the batch position is not empty, set the batch position on the packing list
            //foreach (var pack in pickSlips)
            //{
            //    pack.BatchPosition = batchPosition;
            //}
            _documentToPrint.PrintPickSlipData(pickSlips, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        }
        /// <summary>
        /// Uses a stored Procedure to get the Packing list data from the database
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private List<PackingList> GetPackingList(int orderId)
        {
            List<PackingList> outs;
            using (var context = new NeutronDb())
            {
                var paramOrderId = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PackingList>("usp_GetPackingList @ORDERID", new object[] { paramOrderId }).ToList();
            }
            return outs;
        }
        /// <summary>
        /// Uses a stored Procedure to get the Packing list data from the database
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private List<PickSlip> GetPickSlipData(int orderId)
        {
            List<PickSlip> outs;
            using (var context = new NeutronDb())
            {
                var paramOrderId = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PickSlip>("usp_GetPickSlipData @ORDERID", new object[] { paramOrderId }).ToList();
            }
            return outs;
        }


        // Rack print - Pick List
        private void MBPrintDocument_Click(object sender, EventArgs e)
        {
            PrintPickList(_workstationView);
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void PrintPickList(WorkstationView workstationView)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Count <= 0) return;
            foreach (var order in orders)
            {
                //TODO  commented out because I'm not handling something correctly
                // and items are getting stuck in Pick status

                var orderDetails = order.OrderDetails.Where(r => _areaIdsForThisWorkstation.Contains(r.AreaId)
                                                                 && (r.LineStatusId == (int)LineStatus.Available ||
                                                                     r.LineStatusId == (int)LineStatus.Picking)).ToList();
                foreach (var orderDetail in orderDetails)
                {
                    orderDetail.LineStatusId = (int)LineStatus.Picking;
                    _repoOrderDetails.Update(orderDetail);
                    //var recToUpdate = _repoOrderDetails.FindByKey(orderDetail.Id);
                    //if (recToUpdate != null)
                    //{
                    //    recToUpdate.LineStatusId = (int)LineStatus.Picking;

                    //    _repoOrderDetails.Update(recToUpdate);
                    //}
                }

                PrintPickListByArea(order.Id, workstationView.AreaId);
            }
        }


        private void PrintPickListAll(int orderId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickList(orderId);
            _documentToPrint.PrintPickList(pickList, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickList(int orderId)
        {
            var outs = new List<PickList>();
            using (var context = new NeutronDb())
            {
                var paramOrder = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PickList>("usp_GetPickList @ORDERID", new object[] { paramOrder }).ToList(); //SQL Tested
            }
            return outs;
        }

        //private void PrintPickListByStation(int orderId, int workstationId)
        //{
        //    if (!_neutronVariables.EnableDocumentPrinter) return;
        //    var pickList = GetPickListByWorkstation(orderId, workstationId);
        //    _documentToPrint.PrintPickList(pickList, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        //}

        //private List<PickList> GetPickListByWorkstation(int orderId, int workstationId)
        //{
        //    var outs = new List<PickList>();
        //    using (var context = new NeutronDb())
        //    {
        //        var paramOrder = new SqlParameter(parameterName: "@ORDERID", value: orderId);
        //        var paramStation = new SqlParameter(parameterName: "@STATIONID", value: workstationId);
        //        outs = context.Database.SqlQuery<PickList>("usp_GetPickListByStation @ORDERID, @STATIONID", paramOrder, paramStation).ToList();
        //    }
        //    return outs;
        //}



        private void PrintPickListByArea(int orderId, int areaId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickListByArea(orderId, areaId);
            _documentToPrint.PrintPickList(pickList, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickListByArea(int orderId, int areaId)
        {
            var outs = new List<PickList>();
            using (var context = new NeutronDb())
            {
                var paramOrder = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                var paramStation = new SqlParameter(parameterName: "@AREAID", value: areaId);
                outs = context.Database.SqlQuery<PickList>("usp_GetPickListByArea @ORDERID, @AREAID", paramOrder, paramStation).ToList();
            }
            return outs;
        }


        private void PrintDoc(int positionNumber, Order order)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Printing Document. {order.Ord1}"));
            if (_neutronVariables.EnableDocumentPrinter)
            {
                Task.Run(() => _documentToPrint.Print(positionNumber, order.Ord1, _documentPrinterPreferences, order.Ord2));
            }
        }

        private void PrintAllToteLabels()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == 0) continue;
                var id = bp.OrderId;
                var order = _repoOrders.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintToteLabel(int batchPosition)
        {
            //foreach (var bp in _ordersToPick)
            //{
            //    if (bp.PositionNumber != batchPosition) continue;
            //    if (bp.OrderId == 0) continue;

            var bp = _ordersToPick.FirstOrDefault(o => o.PositionNumber == batchPosition);
            if (bp == null) return;
            if (bp.OrderId == 0) return;

            var id = bp.OrderId;
            var order = _repoOrders.FindByKey(id);
            var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
            if (printJob == null)
            {
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                _repoPrintJob.Insert(printJob);
            }

            // }
        }

        private void PrintTote(int positionNumber, Order order)
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"Printing Tote Label. {order.Ord1}"));
            if (_neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(positionNumber, order, _labelPrinterPreferences));
            }
        }



        private async Task MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (var bp in ordersToPick)
            {
                if (!bp.OrderComplete)
                {
                    if (bp.OrderId != 0)
                    {
                        var id = bp.OrderId;
                        var order = _repoOrders.FindByKey(id);
                        if (order != null)
                        {
                            await CheckForOrderComplete(order);
                        }
                    }
                }
            }
        }

        private async void ButtonPreviousInventoryLocation_Click(object sender, EventArgs e)
        {
            await GetPreviousInventoryLocation();
        }

        private Task GetPreviousInventoryLocation()
        {
            if (_currentPickStop.GroupBoxLocationInventoryIndex - 1 >= 0)
            {
                var idx = _currentPickStop.GroupBoxLocationInventoryIndex - 1;
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                _currentPickStop.CurrentInventoryLocation = inventory;
                UpdateGroupBoxLocation(inventory);
                if (_workstationView.AreaId == AreaEight)
                {
                    MoveToPickLocation();
                }
            }

            return Task.CompletedTask;
        }

        private async void ButtonNextInventoryLocation_Click(object sender, EventArgs e)
        {
            await GetNextInventoryLocation();
        }

        private Task GetNextInventoryLocation()
        {
            //TODO:  set the proper InventoryItem

            if (_currentPickStop.GroupBoxLocationInventoryIndex + 1 < _currentPickStop.Inventory.Count)
            {
                var idx = _currentPickStop.GroupBoxLocationInventoryIndex + 1;
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                _currentPickStop.CurrentInventoryLocation = inventory;
                UpdateGroupBoxLocation(inventory);
                if (_workstationView.AreaId == AreaEight)
                {
                    MoveToPickLocation();
                }
            }

            return Task.CompletedTask;
        }

        // this is called when there are multiple locations on the same PickStop
        private bool NextPickingLocation()
        {
            var result = false;

            var idx = _currentPickStop.InventoryIndex + 1 < _currentPickStop.Inventory.Count
                ? _currentPickStop.InventoryIndex + 1
                : 0;
            if (idx == 0)
            {
                // back to first location so loop thru all locations to see
                // if there is any inventory left
                for (var i = 0; i < _currentPickStop.Inventory.Count; i++)
                {
                    if (_currentPickStop.Inventory[i].Quantity > 0)
                    {
                        _currentPickStop.GroupBoxLocationInventoryIndex = i;
                        var inventory = _currentPickStop.Inventory[i];
                        _currentPickStop.InventoryIndex = i;
                        _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[i];
                        var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                        // ---         Task.Run(() => _logger.LogDetailAsync($"Get Next Picking Location: {loc1}-{loc2}"));
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                // Next Inventory location
                _currentPickStop.GroupBoxLocationInventoryIndex = idx;
                var inventory = _currentPickStop.Inventory[idx];
                _currentPickStop.InventoryIndex = idx;
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[idx];
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                // ---         Task.Run(() => _logger.LogDetailAsync($"Get Next Picking Location: {loc1}-{loc2}"));
                result = true;
            }
            return result;
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
            var inv = _currentPickStop.CurrentInventoryLocation.Quantity;
            if (pickView == null) return;

            if (newQty <= _currentPickStop.CurrentInventoryLocation.Quantity)
            {
                if (newQty <= pickView.GetQuantityToBePicked())
                {
                    pickView.QuantityToBePicked = newQty;
                    _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
                    LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
                    await UpdatePickScreenAfterChangeQuantity();
                    ReprintToteLabel(pos);
                }
                else
                {
                    var result = MessageBox.Show(_resourceManager.GetString($"OverPickItem"), "Change Quantity", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.Yes)
                    {
                        pickView.QuantityToBePicked = newQty;
                        _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
                        LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
                        await UpdatePickScreenAfterChangeQuantity();
                        ReprintToteLabel(pos);
                    }
                }
            }
            else
            {
                // return message saying there is not enough inventory
                MessageBox.Show("There is not enough inventory to pick the requested quantity.");
            }
        }

        private void ButtonMove_Click(object sender, EventArgs e)
        {
            MoveToPickLocation();
        }

        private void MoveToPickLocation()
        {
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

            //var loc1 = TextBoxPickLoc1.Text.ParseInt();
            //var loc2 = TextBoxPickLoc2.Text.ParseInt();
            //var loc3 = TextBoxPickLoc3.Text.ParseInt();
            //var loc4 = TextBoxPickLoc4.Text.ParseInt();

            //var location = await _repoLocation.FindByFirstOrDefaultAsync(r => r.Loc1 == loc1 && r.Loc2 == loc2 && r.Loc3 == loc3 && r.Loc4 == loc4);
            //if (location == null)
            //{
            //    MessageBox.Show($"Location does not exist.");
            //    return;
            //}

            //var itemDefinition = _currentPickStop.CurrentInventoryLocation.ItemDefinition;
            //var inventory = await
            //    _repoInventory.FindByFirstOrDefaultAsync(r => r.LocationId == location.Id);
            //if (inventory == null)
            //{
            //    // we have a location with nothing in it.
            //    inventory = new Inventory
            //    {
            //        LocationId = location.Id,
            //        ItemDefinitionId = _currentPickStop.CurrentInventoryLocation.ItemDefinitionId,
            //        Quantity = 0,
            //        ReceivedDate = DateTime.Now,
            //        PrimeBin = false,
            //        AreaId = location.AreaId,
            //        RFID = string.Empty,
            //        StorageTypeId = 2
            //    };
            //    await _repoInventory.InsertAsync(inventory);
            //    _currentPickStop.CurrentInventoryLocation = inventory;
            //    _currentPickStop.CurrentInventoryLocation.ItemDefinition = itemDefinition;
            //    _currentPickStop.CurrentInventoryLocation.Location = location;
            //    _currentPickStop.CurrentInventoryLocation.StorageType = _repoStorageTypes.FindByKey(2);
            //}
            //else
            //{
            //    if (inventory.ItemDefinitionId == _currentPickStop.CurrentInventoryLocation.ItemDefinitionId)
            //    {
            //        _currentPickStop.CurrentInventoryLocation = inventory;
            //        _currentPickStop.CurrentInventoryLocation.ItemDefinition = itemDefinition;
            //        _currentPickStop.CurrentInventoryLocation.Location = location;
            //        _currentPickStop.CurrentInventoryLocation.StorageType =  _repoStorageTypes.FindByKey(2);
            //    }
            //    else
            //    {
            //        //MessageBox.Show($"Location is in use by another item.");
            //        return;
            //    }
            //}

            //await UpdatePickScreen();
            //// UpdateCurrentDeviceIndicator();

            //_deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

            //await UpdatePickPosition();

            //UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);

            // UpdateTowerDisplay();
            if (_workstationView.AreaId == AreaEight) return;

            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
            // UpdateCurrentDeviceIndicator();
            // _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);
        }

        //private async Task MoveToPickLocation()
        //{
        //    _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[_currentPickStop.GroupBoxLocationInventoryIndex];
        //    var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
        //    var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
        //    var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
        //    var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

        //    await UpdatePickScreen();
        //    // UpdateCurrentDeviceIndicator();

        //    _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(loc1);

        //    await UpdatePickPosition();

        //    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);

        //    // UpdateTowerDisplay();
        //    if (_workstationView.AreaId == AreaEight) return;

        //    _logger.LogDetailAsync($"5331 PositionDevice Button MOVE").SafeFireAndForget();

        //    PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
        //}

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
                    _repoOrders.Update(order);
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
        }

        private void MBChangeOrderStatus_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);

            if (orders.Any())
            {
                var ord = orders.FirstOrDefault();
                using (var form = new FrmChangeOrderStatus(ord, _historyManager))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {

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
                using (var form = new FrmChangeLineStatus(line, _historyManager))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {

                    }
                }
                ShowOrderDetailsByOrder(line.OrderId);
            }
        }

        private void MBReturnToStock_Click(object sender, EventArgs e)
        {
            var uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger
                , _workstationView);
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        if (_neutronVariables.UseReturnToStock)
                        {
                            //int rtsCode = (int)OrderStatus.Returned;

                            foreach (var orderDetail in order.OrderDetails)
                            {
                                SetOrderDetailLineStatus(orderDetail, (int)OrderStatus.Returned, ActionCode.OrderDetailRts);
                            }

                            uploadProcessor.ReturnOrderToStock(order);  //sets the RTS code to each OrderDetail line
                            order.OrderStatusId = (int)OrderStatus.Returned;  //Returned
                            _repoOrders.Update(order);
                            _historyManager.SaveHistory(ActionCode.OrderRts, order);
                        }
                        if (_neutronVariables.CreateStoreOrderWithRts)
                        {
                            CreateStoreOrderFromOrderDetailComplete(order);
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
        }

        //private void SetOrderStatus(Order order, int status, ActionCode actionCode)
        //{
        //    try
        //    {
        //        order.OrderStatusId = status;
        //        _repoOrders.Update(order);
        //       _historyManager .SaveHistory(actionCode, order);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error setting Order Detail Status Code. " + ex.Message);
        //    }
        //}

        // updates the OrderDetail record in the database
        // and records the action in History
        private void SetOrderDetailLineStatus(OrderDetail detail, int lineStatusId, ActionCode actionCode)
        {
            try
            {
                detail.LineStatusId = lineStatusId;
                _repoOrderDetails.Update(detail);
                _historyManager.SaveHistory(actionCode, detail, detail.AreaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString($"ErrorSettingOrderDetailStatusCode")}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}");
            }
        }

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
            MBKillLine.Enabled = false;
            //MBKillLine.Enabled = ((OrderDetailsView)_bindingSourceOrderDetailsView.Current).LineStatusId !=
            //                     (int)LineStatus.Complete;
        }

        private void ShowOrderDetailsByOrderAndWorkstation(Order order, WorkstationView workstationView)
        {
            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndWorkstation(order.Id, workstationView);
            if (!details.Any()) return;
            foreach (var detail in details)
            {
                detail.PickedQuantity = detail.Quantity;
            }
            // _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewAdjust.DataSource = details;  // _bindingSourceOrderDetailsView;
            LabelFormTitle.Text = _resourceManager.GetString($"JobDetails");
            tabControl1.SelectedTab = AdjustOrder;
        }


        private void MBCreateOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = _resourceManager.GetString($"NewJob");
            tabControl1.SelectedTab = NewOrder;
        }


        private void MBMainOrderManager_Click(object sender, EventArgs e)
        {
            LoadOrderManagerScreen();
        }

        private void LoadOrderManagerScreen()
        {
            ShowAvailableStagingOrders();
            Cursor.Current = Cursors.WaitCursor;
            // ---         Task.Run(() => _logger.LogDetailAsync($"Job Manager Main Screen Start"));
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            //MBOffCarousel.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor ||
            //                        _workstationView.StationTypeId == (int)StationType.Rack;

            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)StationType.Supervisor;
            MBCompress.Visible = false;
            tabControl1.SelectedTab = OrderListing;
            MBShowAvailable.Focus();
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Handles the click event for the MBMainAvailableOrders button.
        /// Initializes the orders to pick, displays the available orders screen,
        /// sets the focus to the next text box position, and manages the cursor state.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private async void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _previousTab = null;
            _ordersToPick = InitOrdersToPick(_neutronVariables.PickBatchSize);
            await AvailableOrdersScreen();
            SetFocusNextTextBoxPos();
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// Displays the available orders screen asynchronously.
        /// </summary>
        /// <remarks>
        /// This method updates the UI to show the available orders, sets the cursor to a wait state,
        /// hides the compress button, updates the form title, clears batch positions, and brings the 
        /// "AvailableOrders" tab to the front. It also logs the start and end of the operation and 
        /// handles any exceptions that occur during the process.
        /// </remarks>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Logs any exceptions that occur during the execution.</exception>
        public Task AvailableOrdersScreen()
        {
            try
            {
                //var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();
                _logger.LogDetailAsync("Available Orders Screen START").SafeFireAndForget();
                ShowAvailableOrders();
                Cursor.Current = Cursors.WaitCursor;
                MBCompress.Visible = false;
                LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
               // await ClearBatchPositions(batchPositions);
                //ClearOrderPositions();
                _showSkipped = true;
                tabControl1.TabPages["AvailableOrders"].BringToFront();
                tabControl1.SelectedTab = AvailableOrders;
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

            return Task.CompletedTask;
        }

        private void MBAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            AllOrdersRack();
        }

        public void AllOrdersRack()
        {
            Cursor.Current = Cursors.WaitCursor;
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            //ClearOrderPositions();
            _ordersToPick = InitOrdersToPick(_neutronVariables.PickBatchSize);
            _showSkipped = true;
            ShowRackOrders();
            // tabControl1.SelectedTab = AvailableRack;
            Cursor.Current = Cursors.Default;
        }

        //Ready
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            InitDataGridViewNewItems();
            LabelFormTitle.Text = _resourceManager.GetString($"NewJob");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            ClearNewOrderForm();
            tabControl1.SelectedTab = NewOrder;
        }

        private void MBMainClose_Click(object sender, EventArgs e)
        {
            CloseButtonPressed = true;
        }
        private void MBBackHotPick_Click(object sender, EventArgs e)
        {
            if (_openHotPickFromPickScreen)
            {
                LabelFormTitle.Text = _resourceManager.GetString($"Selection");
                LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
                tabControl1.SelectedTab = PickScreen;
                _openHotPickFromPickScreen = false;
            }
            else
            {
                LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
                LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
                tabControl1.SelectedTab = Main;
            }
        }
        private void MBNewOrderClose_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = Main;
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            if (_previousTab == null)
            {
                if (_currentDataSet == CurrentDataSet.Available)
                {
                    ShowAvailableStagingOrders();
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
                LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
                LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            }
            tabControl1.SelectedTab = _previousTab ?? OrderListing;
        }

        private async void MBLocationCount_Click(object sender, EventArgs e)
        {
            var qty = await LocationCount();
            if (qty == -1)
            {
                // Do Nothing
            }
        }

        private async Task<int> LocationCount()
        {
            var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            var qty = await OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                var invQty = GetCurrentInventoryLocationQuantity(inventoryId);
                TextBoxLocationQuantity.Text = invQty.ToString();
                _currentPickStop.CurrentInventoryLocation.Quantity = invQty;
                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                _currentPickStop.TotalQuantityInInventory = total;
                TextBoxTotalQuantity.Text = total.ToString();
                LoadInventory();
            }

            return qty;
        }

        private async Task<int> LocationCount(int inventoryId)
        {
            var qty = await OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                var invQty = GetCurrentInventoryLocationQuantity(inventoryId);
                TextBoxLocationQuantity.Text = invQty.ToString();
                _currentPickStop.CurrentInventoryLocation.Quantity = invQty;
                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                _currentPickStop.TotalQuantityInInventory = total;
                TextBoxTotalQuantity.Text = total.ToString();
                LoadInventory();
            }

            return qty;
        }

        private async Task<int> OpenLocationCountForm(int inventoryId)
        {
            var qty = -1;
            using (var form = new FrmLocationCount())
            {
                var result = form.ShowDialog();
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

            var locationCount = new LocationCount()
            {
                InventoryId = inv.Id,
                ItemDefinitionId = inv.ItemDefinitionId,
                LocationId = inv.LocationId,
                UserId = GlobalVar.User.Id,
                PreviousQty = prevQty,
                NewQty = qty,
                CountDate = DateTime.Now,
            };
            // _repoLocationCount.Insert(locationCount);
            await _historyManager.SaveHistoryAsync(ActionCode.LocationCount, locationCount);
        }

        //private void SetCurrentInventoryView(int inventoryId)
        //{
        //    var rec = _bindingSourceHot.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
        //    var pos = _bindingSourceHot.IndexOf(rec);
        //    _bindingSourceHot.Position = pos;
        //    _currentInventoryView = (SqlInventoryView)_bindingSourceHot.Current;
        //}

        private void MBShowAvailable_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Available;
            ShowAvailableStagingOrders();
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)StationType.Supervisor;
            MBKillOrder.Visible = false;  // _workstationView.StationType.Id == (int)StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }

        private void CompressOrders()
        {
            var orders = GetSelectedOrders(DataGridView1);
            const string orderType = "PICK";
            var firstTime = true;
            var sb = new StringBuilder();
            foreach (var order in orders)
            {
                // Make sure the order is Complete before Compress
                if (order.OrderStatusId == (int)OrderStatus.Complete)
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

        /// <summary>
        /// Configures the visibility of various buttons on the form based on the current settings and workstation type.
        /// </summary>
        /// <remarks>
        /// This method sets the visibility of buttons such as MBPriority, MBHold, MBRelease, MBReturnToStock, MBReturnToStockOrderDetail, 
        /// MBDeleteOrder, MBCompress, MBKillLine, MBKillLineSkip, MBKillOrder, and MBKillOrderRack. The visibility of these buttons 
        /// depends on the values of <see cref="NeutronVariables.UseReturnToStock"/> and <see cref="WorkstationView.StationTypeId"/>.
        /// </remarks>
        private void ShowButtons()
        {
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBReturnToStock.Visible = _neutronVariables.UseReturnToStock;
            MBReturnToStockOrderDetail.Visible = _neutronVariables.UseReturnToStock;
            MBDeleteOrder.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
            MBCompress.Visible = false; // _workstationView.StationTypeId == (int)StationType.Supervisor;
            MBKillLine.Visible = false;  // _workstationView.StationTypeId == (int)StationType.Supervisor;
            MBKillLineSkip.Visible = false;  // _workstationView.StationTypeId == (int)StationType.Supervisor;
            MBKillOrder.Visible = false;  // _workstationView.StationTypeId == (int)StationType.Supervisor;
            MBKillOrderRack.Visible = false;  // _workstationView.StationTypeId == (int)StationType.Supervisor;
        }

        //private void HideButtons()
        //{
        //    MBPriority.Visible = false;
        //    MBHold.Visible = false;
        //    MBRelease.Visible = false;
        //    MBReturnToStock.Visible = false;
        //    MBReturnToStockOrderDetail.Visible = false;
        //    MBDeleteOrder.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //    MBCompress.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //    MBKillLine.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //    MBKillLineSkip.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //    MBKillOrder.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //    MBKillOrderRack.Visible = _workstationView.StationTypeId == (int)StationType.Supervisor;
        //}

        //Ready
        private void MBNewOrderSearch_Click(object sender, EventArgs e)
        {
            FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());
        }
        //Ready
        private void FindItemRecord(string s)
        {
            try
            {
                _bindingSourceItems.DataSource = GetItemsList(s);
                DataGridViewNewOrder.DataSource = _bindingSourceItems;
                DataGridViewNewOrder.ClearSelection();
                DataGridViewNewOrder.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        private void ButtonRemoveLine_Click(object sender, EventArgs e)
        {
            _bindingSourceNewItems.RemoveCurrent();
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0
                                       && ((NewItemView)_bindingSourceNewItems.Current).Item != null;
            CreateJobButtonEnable();
        }
        //Ready
        private void TextBoxNewOrderFind_KeyDown(object sender, KeyEventArgs e)
        {
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
        private void TextBoxNewOrderQuantity_TextChanged(object sender, EventArgs e)
        {
            ButtonAddDetail.Enabled = TextBoxNewOrderQuantity.Text.ParseInt() > 0;
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
        private void ButtonNewOrderClear_Click(object sender, EventArgs e)
        {
            TextBoxNewOrderFind.Text = string.Empty;
            TextBoxNewOrderFind.Focus();
            FindItemRecord(string.Empty);
        }
        //Ready
        private void DataGridViewNewOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
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
        private void ButtonAddDetail_Click(object sender, EventArgs e)
        {
            AddDetail();
        }
        //Ready
        private void AddDetail()
        {
            var rec = new NewItemView()
            {
                ItemDefinitionId = LabelNewOrderItemId.Text.ParseInt()
                ,
                AreaId = LabelNewOrderStationNumber.Text.ParseInt()
                ,
                Item = TextBoxNewOrderItem.Text
                ,
                Description = TextBoxNewOrderDescription.Text
                ,
                Quantity = TextBoxNewOrderQuantity.Text.ParseInt()
                ,
                CostCenter = GetCostCenter()

            };
            _bindingSourceNewItems.Add(rec);
            ClearNewOrderDetail();
            ButtonAddDetail.Enabled = false;
            TextBoxNewOrderFind.Focus();
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0 && ((NewItemView)_bindingSourceNewItems.Current).Item != null;
        }

        private string GetCostCenter()
        {
            if (_useCostCenter) return ComboBoxCostCenter.SelectedValue.ToString();
            return string.Empty;
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
            ComboBoxCostCenter.Visible = _useCostCenter;
            LabelCostCenter.Visible = _useCostCenter;

            _ = Task.Run(FillCostCenterComboBox);

        }
        private void FillCostCenterComboBox()
        {
            if (!_useCostCenter) return;
            var costCenterPath = LoaderSettings.GetCostCenterPath();
            var costCenterManager = new CostCenterManager(costCenterPath);
            var costCenterList = costCenterManager.GetCostCenterListAsync();
            ComboBoxCostCenter.DataSource = costCenterList;
            ComboBoxCostCenter.DisplayMember = "Name";
            ComboBoxCostCenter.ValueMember = "Code";
        }

        //Ready
        private void InitDataGridViewNewItems()
        {
            DataGridViewNewItems.DataSource = _bindingSourceNewItems;
        }
        //Ready
        private void MBNewOrderSave_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBoxNewOrderOrd1.Text) && _bindingSourceNewItems.Count > 0)
            {
                var orderDetails = new List<OrderDetail>();

                var order = new Order()
                {
                    Ord1 = TextBoxNewOrderOrd1.Text,
                    Ord2 = TextBoxNewOrderOrd2.Text,
                    Priority = IntegerExtensions.ParseInt(TextBoxNewOrderPriority.Text),
                    LoadDate = DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = (int)OrderStatus.Available
                };
                _repoOrders.Insert(order);

                foreach (DataGridViewRow row in DataGridViewNewItems.Rows)
                {
                    var view = row.DataBoundItem as NewItemView;
                    if (view == null) continue;
                    var itemDefinitionId = view.ItemDefinitionId;
                    var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
                    if (itemDefinition == null) continue;
                    var rec = new OrderDetail()
                    {
                        ItemDefinitionId = itemDefinitionId,
                        OrderId = order.Id,
                        Quantity = view.Quantity,
                        AreaId = view.AreaId,
                        LineStatusId = (int)LineStatus.Available,
                        DateTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                        EmpId = GlobalVar.User.EmpId,
                        JobNum = TextBoxNewOrderOrd1.Text,
                        PartNum = itemDefinition.Item,
                        PartDesc = itemDefinition.Description,
                        Qty = view.Quantity.ToString(),
                        OrderDetailInfo = view.CostCenter,
                        TroubleBit = "0"
                    };

                    _repoOrderDetails.Insert(rec);
                }
                TextBoxNewOrderOrd1.Text = string.Empty;
                TextBoxNewOrderOrd2.Text = string.Empty;
                _bindingSourceNewItems.Clear();
                DataGridViewNewItems.Update();
                ClearNewOrderForm();
            }
        }
        //Ready
        private List<NewItemView> GetItemsList(string s)
        {
            var recs = new List<NewItemView>();
            using (var db = new NeutronDb())
            {
                recs = db.Inventory.Include("ItemDefinition")
                    .Where(d => d.ItemDefinition.Item.ToLower().Contains(s) || d.ItemDefinition.Description.ToLower().Contains(s))
                .GroupBy(g => new
                {
                    g.ItemDefinitionId
                    ,
                    g.ItemDefinition.AreaId
                    ,
                    g.ItemDefinition.Item
                    ,
                    g.ItemDefinition.Description
                })
                        .Select(r => new NewItemView()
                        {
                            ItemDefinitionId = r.Key.ItemDefinitionId
                            ,
                            AreaId = r.Key.AreaId
                            ,
                            Item = r.Key.Item
                            ,
                            Description = r.Key.Description
                            ,
                            Quantity = r.Sum(t => t.Quantity)
                        }).OrderBy(r => r.Item).ToList();
            }
            return recs;
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

        private async void MBPickNewItem_Click(object sender, EventArgs e)
        {
            using (var form = new FrmNewItemAuth())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.AuthCode == "topura")
                    {
                        UpdatePickViewsTopura();
                        await UpdatePickScreen();
                        //UpdateCurrentDeviceIndicator();

                        _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);


                        await UpdatePickPosition();

                        UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                        // UpdateTowerDisplay();
                    }
                }
            }
        }

        private void UpdatePickViewsTopura()
        {
            var success = false;
            foreach (var pickView in _currentPickStop.PickViews)
            {
                var newItem = pickView.OrderDetail.PartNum;
                var newItemDefinition = _repoItemDefinition.FindBy(f => f.Item == newItem).FirstOrDefault();
                if (newItemDefinition != null)
                {
                    pickView.OrderDetail.PartDesc = newItemDefinition.Description;
                    pickView.Description = pickView.OrderDetail.PartDesc;
                    pickView.OrderDetail.PartNum = newItemDefinition.Item;
                    pickView.Item = pickView.OrderDetail.PartNum;
                    pickView.ItemId = newItemDefinition.Id;

                    List<Inventory> exactInventorySequence;
                    switch (_neutronVariables.PickMethod)
                    {
                        case "RadioButtonPrimeBinFirst":
                            exactInventorySequence = PrimeBinFirst(pickView);
                            break;
                        case "RadioButtonPrimeBinLast":
                            exactInventorySequence = PrimeBinLast(pickView);
                            break;
                        case "RadioButtonFifo":
                            exactInventorySequence = Fifo(pickView);
                            break;
                        case "RadioButtonLifo":
                            exactInventorySequence = Lifo(pickView);
                            break;
                        default:
                            exactInventorySequence = Fifo(pickView);
                            break;
                    }

                    pickView.CurrentInventoryLocation = exactInventorySequence.First();
                    pickView.Inventory = exactInventorySequence;
                    pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    pickView.Slot = pickView.CurrentInventoryLocation.Location.Slot;
                    pickView.SlotQty = pickView.TotalQuantityInInventory;
                    pickView.InventoryIndex = 0;
                    pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
                    success = true;
                }
            }
            if (success)
            {
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.PickViews.First().CurrentInventoryLocation;
                _currentPickStop.Description = _currentPickStop.PickViews.First().Description;
                _currentPickStop.Images = _currentPickStop.PickViews.First().Images;
                _currentPickStop.Inventory = _currentPickStop.PickViews.First().Inventory;
                _currentPickStop.InventoryIndex = _currentPickStop.PickViews.First().InventoryIndex;
                _currentPickStop.Item = _currentPickStop.PickViews.First().Item;
                _currentPickStop.ItemId = _currentPickStop.PickViews.First().ItemId;
                _currentPickStop.Ord1 = _currentPickStop.PickViews.First().Ord1;
                _currentPickStop.Ord2 = _currentPickStop.PickViews.First().Ord2;
                _currentPickStop.OrderId = _currentPickStop.PickViews.First().OrderId;
                _currentPickStop.PickedQty = _currentPickStop.PickViews.First().PickedQty;
                _currentPickStop.Quantity = _currentPickStop.PickViews.First().Quantity;
                // _currentPickStop.QuantityToBePicked = _currentPickStop.PickViews.First().QuantityToBePicked;
                _currentPickStop.Slot = _currentPickStop.PickViews.First().Slot;
                _currentPickStop.SlotQty = _currentPickStop.PickViews.First().SlotQty;
                _currentPickStop.TotalQuantityInInventory = _currentPickStop.PickViews.First().TotalQuantityInInventory;

            }
        }

        private void UpdatePickViewsAfterHotAction()
        {

            var success = false;
            foreach (var pickView in _currentPickStop.PickViews)
            {
                var newItem = pickView.OrderDetail.PartNum;
                var newItemDefinition = _repoItemDefinition.FindBy(f => f.Item == newItem).FirstOrDefault();
                if (newItemDefinition != null)
                {
                    pickView.OrderDetail.PartDesc = newItemDefinition.Description;
                    pickView.Description = pickView.OrderDetail.PartDesc;
                    pickView.OrderDetail.PartNum = newItemDefinition.Item;
                    pickView.Item = pickView.OrderDetail.PartNum;
                    pickView.ItemId = newItemDefinition.Id;

                    List<Inventory> exactInventorySequence;
                    switch (_neutronVariables.PickMethod)
                    {
                        case "RadioButtonPrimeBinFirst":
                            exactInventorySequence = PrimeBinFirst(pickView);
                            break;
                        case "RadioButtonPrimeBinLast":
                            exactInventorySequence = PrimeBinLast(pickView);
                            break;
                        case "RadioButtonFifo":
                            exactInventorySequence = Fifo(pickView);
                            break;
                        case "RadioButtonLifo":
                            exactInventorySequence = Lifo(pickView);
                            break;
                        default:
                            exactInventorySequence = Fifo(pickView);
                            break;
                    }

                    pickView.CurrentInventoryLocation = exactInventorySequence.First();
                    pickView.Inventory = exactInventorySequence;
                    pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    pickView.Slot = pickView.CurrentInventoryLocation.Location.Slot;
                    pickView.SlotQty = pickView.TotalQuantityInInventory;
                    pickView.InventoryIndex = 0;
                    pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
                    success = true;
                }
            }
            if (success)
            {
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.PickViews.First().CurrentInventoryLocation;
                _currentPickStop.Description = _currentPickStop.PickViews.First().Description;
                _currentPickStop.Images = _currentPickStop.PickViews.First().Images;
                _currentPickStop.Inventory = _currentPickStop.PickViews.First().Inventory;
                _currentPickStop.InventoryIndex = _currentPickStop.PickViews.First().InventoryIndex;
                _currentPickStop.Item = _currentPickStop.PickViews.First().Item;
                _currentPickStop.ItemId = _currentPickStop.PickViews.First().ItemId;
                _currentPickStop.Ord1 = _currentPickStop.PickViews.First().Ord1;
                _currentPickStop.Ord2 = _currentPickStop.PickViews.First().Ord2;
                _currentPickStop.OrderId = _currentPickStop.PickViews.First().OrderId;
                _currentPickStop.PickedQty = _currentPickStop.GetPickedSoFar();
                _currentPickStop.Quantity = _currentPickStop.GetTotalQuantityToBePicked();
                // _currentPickStop.QuantityToBePicked = _currentPickStop.GetQuantityToBePicked();
                _currentPickStop.Slot = _currentPickStop.PickViews.First().Slot;
                _currentPickStop.SlotQty = _currentPickStop.PickViews.First().SlotQty;
                _currentPickStop.TotalQuantityInInventory = _currentPickStop.PickViews.First().TotalQuantityInInventory;

            }
        }

        private void UpdateInventoryAfterHotAction()
        {
            var exactInventorySequence = new List<Inventory>();
            var currentPickView = _currentPickStop.PickViews.FirstOrDefault();
            if (currentPickView != null)
            {
                switch (_neutronVariables.PickMethod)
                {
                    case "RadioButtonPrimeBinFirst":
                        exactInventorySequence = PrimeBinFirst(currentPickView);
                        break;
                    case "RadioButtonPrimeBinLast":
                        exactInventorySequence = PrimeBinLast(currentPickView);
                        break;
                    case "RadioButtonFifo":
                        exactInventorySequence = Fifo(currentPickView);
                        break;
                    case "RadioButtonLifo":
                        exactInventorySequence = Lifo(currentPickView);
                        break;
                    default:
                        exactInventorySequence = Fifo(currentPickView);
                        break;
                }
            }

            if (exactInventorySequence.Count > 0)
            {
                foreach (var pickView in _currentPickStop.PickViews)
                {
                    pickView.CurrentInventoryLocation = exactInventorySequence.First();
                    pickView.Inventory = exactInventorySequence;
                    pickView.TotalQuantityInInventory = exactInventorySequence.Sum(r => r.Quantity);
                    pickView.Slot = exactInventorySequence.First().Location.Slot;
                    pickView.SlotQty = pickView.TotalQuantityInInventory;
                    pickView.InventoryIndex = 0;
                    pickView.ReceivedDate = pickView.CurrentInventoryLocation.ReceivedDate;
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
            }
        }

        private void MBPickRefresh_Click(object sender, EventArgs e)
        {
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
        }

        private void ButtonImageNext_Click(object sender, EventArgs e)
        {

        }

        private void ButtonImagePrevious_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxPos1_TextChanged(object sender, EventArgs e)
        {


        }

        /// <summary>
        /// Get a list of orders from the AvailableOrders BindingSource
        /// </summary>
        /// <param name="orderNumber">Could represent an order number or an invoice number</param>
        /// <returns>Return a List of <see cref="OrderView"/> records.</returns>
        private List<AvailableOrdersView> GetValidOrdersFromBindingSource(string orderNumber)
        {
            var serialPicking = _neutronVariables.SerialPicking;
            var list = _ordersRepository.GetAvailableOrdersForInductionScreen(_workstationView.AreaId, string.Empty, serialPicking);
            // Convert the AvailableOrders BindingSource to a List 
            //var list =  _bindingSourceAvailableOrders.List.OfType<AvailableOrdersView>();

            //var list = ((IList<AvailableOrdersView>)_bindingSourceAvailableOrders.List);

            //var list = ((List<ObjectView<AvailableOrdersView>>)_bindingSourceAvailableOrders.List);
            // Return all the orders where the order number or invoice number
            // equals the passed in orderNumber value
            var recs = list.Where(s => s.Ord1 == orderNumber || s.Ord2 == orderNumber).ToList();
            return recs;
        }

        private bool CheckForMultipleOrders(string orderNumber)
        {
            var ordersWithThisOrderNumber = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var ord1 = (row.Cells["Ord1"].Value).ToString();
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
            _currentTextBoxPos.BackColor = Color.White;
            var textBox = (TextBox)sender;
            _currentTextBoxPos = textBox;
            textBox.BackColor = Color.Yellow;
            textBox.SelectAll();
            textBox.Focus();
        }

        /// <summary>
        /// If you hit the Enter key it will act like the Tab key was pressed
        /// </summary>
        /// <param name="sender"> <see cref="TextBox"/> TextBoxPosx where x is the number of the position </param>
        /// <param name="e">KeyEventArgs</param>
        private void TextBoxPosKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send(keys: "{Tab}");
            }
            e.Handled = true;
        }

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

        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                Cursor.Current = Cursors.WaitCursor;
                //ShowAvailableOrders();
                var find = TextBoxFindAvailableOrders.Text.Trim().ToLower();
                FilterAvailableOrders(find);
                Cursor.Current = Cursors.Default;
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindAvailableOrders.Text = "";
            }
        }

        private void MBSearchAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowAvailableOrders();
            Cursor.Current = Cursors.Default;
        }

        private void FilterAvailableOrders(string find)
        {
            if (string.IsNullOrWhiteSpace(find))
            {
                _bindingListViewAvailableOrdersViews.RemoveFilter();
            }
            else
            {
                _bindingListViewAvailableOrdersViews.ApplyFilter(r => r.Ord1.ToLower().Contains(find) || r.Ord2.ToLower().Contains(find));
            }
        }



        private void ButtonClearFindAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            TextBoxFindAvailableOrders.Focus();
            RefreshDataGridRows();
            Cursor.Current = Cursors.Default;
        }

        private void MButtonSearch_Click(object sender, EventArgs e)
        {
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
                            ShowAvailableStagingOrders();
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
                _logger.LogDetailAsync($"An error occurred while searching the dataset.{Environment.NewLine}{ex.Message}").SafeFireAndForget();
                //MessageBox.Show("", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SearchDataSet();
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFind.Text = "";
            }
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            TextBoxFind.Text = string.Empty;
            SearchDataSet();
            TextBoxFind.Focus();
        }

        private void MBAvailableOrdersRefresh_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void MBRefresh_Click(object sender, EventArgs e)
        {
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
            MBCompleted.Text = "MBCompleted";
        }

        private void MBClearSelectionDetail_Click(object sender, EventArgs e)
        {
            DataGridViewOrderDetails.ClearSelection();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                chk.Value = chk.FalseValue;
            }
            FocusOnCurrentTextBox();
        }

        private void MBReturnToStockOrderDetail_Click(object sender, EventArgs e)
        {
            var orderId = 0;
            var uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger
                , _workstationView);
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (orderDetails.Any())
            {
                orderId = orderDetails.First().OrderId;
                foreach (var orderDetail in orderDetails)
                {
                    if (orderDetail != null)
                    {
                        if (_neutronVariables.UseReturnToStock)
                        {
                            SetOrderDetailLineStatus(orderDetail, (int)LineStatus.Returned, ActionCode.OrderDetailRts);
                            uploadProcessor.ReturnToStock(orderDetail);
                            orderDetail.LineStatusId = (int)LineStatus.Returned;
                            _repoOrderDetails.Update(orderDetail);
                            _historyManager.SaveHistory(ActionCode.OrderRts, orderDetail);
                        }
                        if (_neutronVariables.CreateStoreOrderWithRts)
                        {
                            CreateStoreOrderFromOrderDetailLine(orderDetail);
                        }
                    }
                }
            }
            ShowOrderDetails(orderId);
        }

        private void ShowOrderDetails(int orderId)
        {
            var details = _orderDetailsRepository.GetOrderDetailsViewByOrder(orderId).ToList();

            _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = _bindingSourceOrderDetailsView;
            DataGridViewOrderDetails.ClearSelection();
            DataGridViewOrderDetails.Update();
        }

        private void CreateStoreOrderFromOrderDetailLine(OrderDetail orderDetail)
        {
            var detailLine = _repoOrderDetails.FindBy(r => r.Id == orderDetail.Id).FirstOrDefault();
            if (detailLine != null)
            {
                StoreOrderDetails(detailLine);
            }
        }

        private void StoreOrderDetails(OrderDetail detailLine)
        {
            var firstRec = detailLine;
            if (firstRec != null)
            {
                var replenOrder = new ReplenOrder()
                {
                    Ord1 = firstRec.JobNum,
                    Ord2 = firstRec.EmpId,
                    Priority = 99,
                    LoadDate = DateTime.Now,
                    //  ShipperId = 1,
                    //  ShipMethodId = 1,
                    OrderStatusId = (int)OrderStatus.Available
                };
                try
                {
                    _repoReplenOrder.Insert(replenOrder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_resourceManager.GetString($"InsertStoreOrderError")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }

                var storeItemDefinition = _repoItemDefinition.FindBy(r => r.Item == detailLine.PartNum).FirstOrDefault();
                if (storeItemDefinition != null)
                {
                    var rec = _repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == detailLine.PrimeBin).FirstOrDefault();
                    if (rec != null)
                    {
                        var replenOrderDetail = new ReplenOrderDetail()
                        {
                            ReplenOrderId = replenOrder.Id,
                            ItemDefinitionId = rec.ItemDefinitionId,
                            Quantity = (detailLine.Qty).ParseInt(),
                            DateTime = detailLine.DateTime,
                            EmpId = detailLine.EmpId,
                            JobNum = detailLine.JobNum,
                            NewBin = detailLine.NewBin,
                            PartDesc = detailLine.PartDesc,
                            PartNum = detailLine.PartNum,
                            PrimeBin = detailLine.PrimeBin,
                            Qty = detailLine.Qty,
                            TroubleBit = detailLine.TroubleBit,
                            TypeCode = detailLine.TypeCode,
                            LineStatusId = (int)LineStatus.Available,
                            AreaId = rec.Location.AreaId
                        };
                        try
                        {
                            _repoReplenOrderDetail.Insert(replenOrderDetail);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{_resourceManager.GetString($"InsertStoreItemError")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"{_resourceManager.GetString($"BuildStoreOrderNoItemDefintion")}");
                    }
                }
                else
                {
                    MessageBox.Show($"{_resourceManager.GetString($"BuildStoreOrderNoInventoryItem")}");
                }
            }
        }

        private void MBSelectAllDetail_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                chk.Value = chk.TrueValue;
                if (row.IsNewRow)
                {
                    chk.Value = chk.FalseValue;
                }
            }
        }

        private void CreateStoreOrderFromOrderDetailComplete(Order order)
        {
            var detailLines = _repoOrderDetails.FindBy(r => r.OrderId == order.Id && r.LineStatusId == (int)LineStatus.Complete).ToList();
            if (detailLines.Any())
            {
                StoreOrderDetails(detailLines);
            }

        }

        // Internal order builder for items that have been picked and then
        // the order is returned to stock
        private void StoreOrderDetails(IReadOnlyCollection<OrderDetail> hostOrderLines)
        {
            if (hostOrderLines.Any())
            {
                var firstRec = hostOrderLines.FirstOrDefault();
                if (firstRec != null)
                {
                    var replenOrder = new ReplenOrder()
                    {
                        Ord1 = firstRec.JobNum,
                        Ord2 = firstRec.EmpId,
                        Priority = 99,
                        LoadDate = DateTime.Now,
                        // ShipperId = 1,
                        //  ShipMethodId = 1,
                        OrderStatusId = (int)OrderStatus.Available
                    };
                    try
                    {
                        _repoReplenOrder.Insert(replenOrder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{_resourceManager.GetString($"InsertStoreOrderError")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    }

                    foreach (var hostOrder in hostOrderLines)
                    {
                        var storeItemDefinition = _repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                        if (storeItemDefinition != null)
                        {
                            var rec = _repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                            if (rec != null)
                            {
                                var replenOrderDetail = new ReplenOrderDetail()
                                {
                                    ReplenOrderId = replenOrder.Id,
                                    ItemDefinitionId = rec.ItemDefinitionId,
                                    Quantity = hostOrder.Qty.ParseInt(),
                                    DateTime = hostOrder.DateTime,
                                    EmpId = hostOrder.EmpId,
                                    JobNum = hostOrder.JobNum,
                                    NewBin = hostOrder.NewBin,
                                    PartDesc = hostOrder.PartDesc,
                                    PartNum = hostOrder.PartNum,
                                    PrimeBin = hostOrder.PrimeBin,
                                    Qty = hostOrder.Qty,
                                    TroubleBit = hostOrder.TroubleBit,
                                    TypeCode = hostOrder.TypeCode,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = rec.Location.AreaId
                                };
                                try
                                {
                                    _repoReplenOrderDetail.Insert(replenOrderDetail);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"{_resourceManager.GetString($"InsertStoreItemError")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                                }
                            }
                            else
                            {
                                MessageBox.Show($"{_resourceManager.GetString($"BuildStoreOrderNoItemDefintion")}");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"{_resourceManager.GetString($"BuildStoreOrderNoInventoryItem")}");
                        }
                    }
                }
            }
        }

        private void MBReleaseDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (!orderDetails.Any()) return;
            var orderId = orderDetails.First().OrderId;
            foreach (var item in orderDetails)
            {
                if (item.LineStatusId == (int)LineStatus.Hold)
                {
                    item.LineStatusId = (int)LineStatus.Available;
                    _repoOrderDetails.Update(item);
                    item.Order = _repoOrders.FindByKey(item.OrderId);
                    _historyManager.SaveHistory(ActionCode.ReleaseLine, item);
                }
            }
            ShowOrderDetails(orderId);
        }

        private void MBHoldDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (orderDetails.Any())
            {
                var orderId = orderDetails.First().OrderId;
                foreach (var item in orderDetails)
                {
                    if (item.LineStatusId == (int)LineStatus.Available)
                    {
                        item.LineStatusId = (int)LineStatus.Hold;
                        _repoOrderDetails.Update(item);

                        item.Order = _repoOrders.FindByKey(item.OrderId);

                        _historyManager.SaveHistory(ActionCode.HoldLine, item);
                    }
                }
                ShowOrderDetails(orderId);
            }
        }

        private void DataGridPickView_FormatRows()
        {
            var grid = DataGridPickView;
            if (grid.RowCount > 0)
            {
                grid.SuspendLayout();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (IntegerExtensions.ParseInt(row.Cells["Quantity"].Value.ToString()) > IntegerExtensions.ParseInt(row.Cells["TotalQuantityInInventory"].Value.ToString()))
                    {
                        row.DefaultCellStyle.BackColor = Color.Gold;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }


                grid.ResumeLayout();
            }

        }


        private async void MBPickScreenHotPick_Click(object sender, EventArgs e)
        {
            await HotAction();
        }

        private async Task HotAction()
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                var item = LabelPickItemNumber.Text;
                Hide();
                //item = "10006886";
                //var quantity = 0;


                //using (var frm = DI.Create<FrmHotAction>(
                //          _neutronVariables
                //          , _neutronLicense
                //          , _workstationView
                //          , _historyManager
                //          , item))
                //          //, quantity))
                //          //, null))

                using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                                          , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                                          , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _inventoryUnitOfWork, _iptiDisplayFunctions, _inventoryRepository, item))
                {
                    //frm.Item = item;
                    var result = frm.ShowDialog();
                    Show();
                    _deviceManager?.Reset();
                    // ---         Task.Run(() => _logger.LogDetailAsync($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
                }
                // UpdatePickViews();
                //  UpdatePickViewsAfterHotAction();
                LoadInventory();
                UpdateInventoryAfterHotAction();

                await UpdatePickScreen();

                _deviceIndicatorManager?.UpdateCurrentDeviceIndicator(_currentPickStop.CurrentInventoryLocation.Location.Loc1);


                await UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                // UpdateTowerDisplay();


                //var location = _currentPickStop.CurrentInventoryLocation.Location;

                // _logger.LogDetailAsync($"After Reset get _currentPickStop.CurrentInventoryLocation.Location: {location.Slot}");

                // PositionDevice(location.Loc1, location.Loc2, location.Loc3, location.Loc4, true);
            }
        }

        private int ShowCompleted(int recId = 0)
        {

            _logger.LogDetailAsync($"Show Completed Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]").SafeFireAndForget();
            var idx = 0;
            var searchField = TextBoxFind.Text.Trim().ToLower();
            var orderStatus = "6";

            var views = _ordersRepository.GetCompletedOrderViews(orderStatus, searchField);
            var bindingListView = new BindingListView<OrderView>(views.ToList());
            _bindingSourceCompleted.DataSource = bindingListView;
            DataGridView1.DataSource = _bindingSourceCompleted;


            if (GetRecordCount(_bindingSourceCompleted) > 0)
            {
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
                CurrentItem = ((ObjectView<OrderView>)_bindingSourceCompleted.Current).Object;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"Show Completed Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;


        }

        private void ShowAvailable()
        {
            var views = _ordersRepository.GetAvailableOrders();
            var bindingListView = new BindingListView<OrderView>(views.ToList());
            _bindingSourceCompleted.DataSource = bindingListView;
            GetRecordCount(_bindingSourceCompleted);
            DataGridView1.DataSource = _bindingSourceCompleted;
            DataGridView1.ClearSelection();
            DataGridView1.Update();
        }

        //private void ShowRack()
        //{
        //    var views = _ordersRepository.GetRackOrders();
        //    var bindingListView = new BindingListView<OrderView>(views.ToList());
        //    _bindingSourceCompleted.DataSource = bindingListView;
        //    DataGridView1.DataSource = _bindingSourceCompleted;
        //    DataGridView1.ClearSelection();
        //    DataGridView1.Update();
        //}

        private void MBPrintOrderListing_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridView1);
        }

        private void MBPrintPickList_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridPickView);
        }

        private void MBPrintOrderDetails_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewOrderDetails);
        }

        /// <summary>
        /// Handles the Click event of a <see cref="TextBox"/> control.
        /// </summary>
        /// <param name="sender">The source of the event, typically the <see cref="TextBox"/> that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// This method sets the clicked <see cref="TextBox"/> as the current text box, selects all text within it,
        /// and focuses the control.
        /// </remarks>
        private void TextBoxPos_Click(object sender, EventArgs e)
        {
            // Set the TextBox that was just clicked to the Current TextBox  
            _currentTextBoxPos = sender as TextBox;
            if (_currentTextBoxPos == null) return;
            _currentTextBoxPos.SelectAll();
            _currentTextBoxPos.FocusAndHighlightText();
        }

        /// <summary>
        /// Clear the BackColor of all the TextBoxPosx TextBoxes
        /// </summary>
        private void ClearTextBoxPosBackColor()
        {
            // ---         Task.Run(() => _logger.LogDetailAsync($"START ClearTextBoxPosBackColor "));
            foreach (var bp in _ordersToPick)
            {

                var pos = bp.PositionNumber.ToString();
                // ---         Task.Run(() => _logger.LogDetailAsync($"Each Position: {pos}"));
                var c = Controls.Find($"TextBoxPos{pos}", true).First();
                if (c != null) c.BackColor = Color.White;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"END ClearTextBoxPosBackColor "));
        }

        private async void MBFillStarters_Click(object sender, EventArgs e)
        {
            var emptyBatchPositions = _ordersToPick.Where(r => r.OrderId == 0).ToList();
            var count = 0;
            var totalRows = DataGridViewAvailableOrders.Rows.Count;
            if (totalRows <= 0) return;

            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                // ManualOverrideCurrentTextBoxPos = false;
                var starterValue = Convert.ToBoolean(row.Cells["Starter"].Value);
                if (starterValue)
                {
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
                // ManualOverrideCurrentTextBoxPos = false;
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
        }

        private void MBRackBack_Click(object sender, EventArgs e)
        {
            //LoadOrderManagerScreen();
            tabControl1.SelectedTab = Main;
        }

        private void MBPrintToteLabel_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    PrintTote(positionNumber: 1, order: order);
                    var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                    if (printJob == null)
                    {
                        printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                        _repoPrintJob.Insert(printJob);
                    }
                }
            }
        }
        /// <summary>
        /// Checks if the order is complete on all stations.
        /// </summary>
        /// <param name="order">The order to check.</param>
        /// <returns>Returns true if the order is complete, otherwise false.</returns>
        /// <remarks>
        /// This method checks if all the lines of the order are complete. If any line is not complete, it returns false.
        /// If all lines are complete, it updates the order status to complete, saves the history, and raises the OrderComplete event.
        /// </remarks>

        private async Task<bool> CheckForOrderCompleteOnAllStations(Order order)
        {
            var isOrderComplete = await IsOrderComplete(order);
            if (!isOrderComplete) return false;
            await SetCompleteOrder(order);
            return true;
        }
        private async Task<bool> IsOrderComplete(Order order)
        {
            var linesNotComplete = await _repoOrderDetails
                .FindByAsync(r => r.OrderId == order.Id && r.LineStatusId != (int)LineStatus.Complete);
            return !linesNotComplete.Any();
        }
        private async Task SetCompleteOrder(Order order)
        {
            order.OrderStatusId = (int)OrderStatus.Complete;
            await _historyManager.SaveHistoryAsync(ActionCode.OrderComplete, order);
            await _repoOrders.UpdateAsync(order);
            Mediator.GetInstance().OnOrderComplete(this, order);
        }

        private async Task SetOrderStatusToAvailable(Order order)
        {
            order.OrderStatusId = (int)OrderStatus.Available;
            await _historyManager.SaveHistoryAsync(ActionCode.ChangeOrderStatus, order);
            await _repoOrders.UpdateAsync(order);
        }



        private void MBRefreshRack_Click(object sender, EventArgs e)
        {
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MbPrintAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewAvailableOrdersRack);
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrint_Click(object sender, EventArgs e)
        {
            var position = _currentPickStop.PickViews.First().PickPosition;

            try
            {
                using (var form = new FrmReprint(_neutronVariables, position))
                {
                    var result = form.ShowDialog();
                    if (result != DialogResult.OK) return;
                    if (form.printData.PrintDocument)
                    {
                        ReprintDocument(form.printData.Position);
                    }

                    if (form.printData.PrintToteLabel)
                    {
                        ReprintToteLabel(form.printData.Position);
                    }
                }
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnDisplayMessage(this, $"Print Error: {ex.Message}");
            }
        }

        private void ReprintToteLabel(int batchPosition)
        {
            try
            {
                if (_currentPickStop?.PickViews != null)
                {
                    var view = _currentPickStop.PickViews.FirstOrDefault(r => r.PickPosition == batchPosition);
                    if (view == null) return;
                    _logger.LogDetailAsync("Reprint Label").SafeFireAndForget();
                    PrintLabel(Reprint, batchPosition, view);
                }
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnDisplayMessage(this, $"Reprint Tote Label Error: {ex.Message}");

            }

            //foreach (var bp in _ordersToPick)
            //{
            //    if (bp.PositionNumber != batchPosition) continue;
            //    if (bp.OrderId == 0) continue;

            //var bp = _ordersToPick.Where(o => o.PositionNumber == batchPosition).FirstOrDefault();
            //if (bp == null) return;
            //if (bp.OrderId == 0) return;

            //var id = bp.OrderId;
            //var order = _repoOrders.FindByKey(id);
            //var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
            //if (printJob == null)
            //{
            //    PrintTote(bp.PositionNumber, order);
            //    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
            //    _repoPrintJob.Insert(printJob);
            //}
            //else
            //{
            //    PrintTote(bp.PositionNumber, order);
            //    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
            //    _repoPrintJob.Update(printJob);
            //}
            // }
        }

        private void ReprintDocument(int batchPosition)
        {
            var bp = _ordersToPick.FirstOrDefault(o => o.PositionNumber == batchPosition);
            if (bp == null) return;
            if (bp.OrderId == 0) return;
            var id = bp.OrderId;
            var order = _repoOrders.FindByKey(id);
            if (order == null) return;
            var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
            if (printJob == null)
            {
                //PrintDoc(bp.PositionNumber, order);
                PrintPackingList(id);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
            else
            {
                //PrintDoc(bp.PositionNumber, order);
                PrintPackingList(id);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Update(printJob);
            }
            //}
        }

        private void PictureBoxItemImage_MouseEnter(object sender, EventArgs e)
        {
            if (!_neutronVariables.AutoEnlargeImage) return;
            PictureBoxItemImage.Location = new Point(128, 161);
            PictureBoxItemImage.Size = new Size(512, 512);
            PictureBoxItemImage.BringToFront();
        }

        private void PictureBoxItemImage_MouseLeave(object sender, EventArgs e)
        {
            if (!_neutronVariables.AutoEnlargeImage) return;
            PictureBoxItemImage.Location = new Point(440, 476);
            PictureBoxItemImage.Size = new Size(200, 200);
            PictureBoxItemImage.BringToFront();
        }

        private void MBSearchAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowAvailableOrdersRack(0, TextBoxFindAvailableOrdersRack.Text.Trim().ToLower());
            Cursor.Current = Cursors.Default;
        }

        private void TextBoxFindAvailableOrdersRack_KeyDown(object sender, KeyEventArgs e)
        {
            // MessageBox.Show($" TextBoxFindAvailableOrdersRack Key code: {e.KeyCode}");
            if (e.KeyCode == Keys.Return)
            {
                ShowAvailableOrdersRack(0, TextBoxFindAvailableOrdersRack.Text.Trim().ToLower());
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxFindAvailableOrdersRack.Text = "";
            }
        }

        private void ButtonClearFindAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrdersRack.Text = string.Empty;
            ShowAvailableOrders(0, TextBoxFindAvailableOrdersRack.Text.Trim().ToLower());
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void Main_Enter(object sender, EventArgs e)
        {
            MBMainAvailableOrders.Focus();
        }

        private void MBCompleted_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            ShowCompletedOrders();
            Cursor.Current = Cursors.Default;
            MBCompleted.Focus();
        }

        private void ShowCompletedOrders()
        {

            _currentDataSet = CurrentDataSet.Complete;
            ShowCompleted();
            MBDeleteOrder.Visible = false;
            MBKillOrder.Visible = false;
            MBCompress.Visible = false;  // _workstationView.StationType.Id == (int)StationType.Supervisor;

        }

        private void MBDeleteOrder_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to DELETE the selected orders?", "Delete Orders",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No) return;
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    //var ord = _repoOrders.AllInclude(s => s.OrderDetails).FirstOrDefault(r => r.Id == order.Id);

                    if (order == null) continue;
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        _repoOrderDetails.Delete(orderDetail.Id);
                        _historyManager.SaveHistory(ActionCode.OrderDetailDelete, orderDetail);
                    }
                    _repoOrders.Delete(order.Id);
                    _historyManager.SaveHistory(ActionCode.OrderDelete, order);
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
        }

        //public void ProcessDataReceived(object sender, MyDataReceivedEventArgs args)
        //{
        //    // ---         Task.Run(() => _logger.LogDetailAsync($"ProcessDataReceived:  {args.FormText}"));
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
        //        // ---         Task.Run(() => _logger.LogDetailAsync($"Pick Accept in Data Received: {t}"));
        //        // ---         Task.Run(() => _logger.LogDetailAsync("Hitting the PickAccept button from ProcessDataReceived."));
        //        PickAccept();
        //    }


        //}

        //private void zTextBoxFindAvailableOrdersRack_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
        //        {
        //            if (row.Cells["Ord1"].Value.ToString().Trim() == TextBoxFindAvailableOrdersRack.Text)
        //            {_pickStations
        //                var chk = (DataGridViewCheckBoxCell) row.Cells[0];
        //                row.Cells[0].Value = chk.TrueValue;
        //                TextBoxFindAvailableOrdersRack.Text = string.Empty;
        //                TextBoxFindAvailableOrdersRack.Select();
        //                break;
        //            }
        //        }
        //    }
        //}

        //private void ButtonPrintAO_Click(object sender, EventArgs e)
        //{
        //    var anticipatedOuts = new List<AnticipatedOut>();
        //    //if (_workstationView.StationType.Id == (int)StationType.Supervisor)
        //    if (ComboBoxStationNumber.SelectedText == "ALL")
        //    {
        //        // print all workstation if Supervisor Workstation
        //        foreach (var pickStation in _pickStations)
        //        {
        //            anticipatedOuts = GetAnticipatedOutsByWorkstation(pickStation.Id);
        //            _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        //        }
        //    }
        //    else
        //    {
        //        var workstationId =  ((Workstation)ComboBoxStationNumber.SelectedItem).Id;
        //        anticipatedOuts = GetAnticipatedOutsByWorkstation(workstationId);
        //        _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinterPreferences, _neutronVariables.PrintPreview);
        //    }
        //}
        /// <summary>
        /// Print Anticipated Outs by Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrintAO_Click(object sender, EventArgs e)
        {
            List<AnticipatedOut> anticipatedOuts;

            if ((string)ComboBoxAreaNumber.SelectedValue == "ALL")
            {
                // print all areas  
                foreach (var area in _areaRepository.GetAllAreas())
                {
                    anticipatedOuts = GetAnticipatedOutsByArea(area.Id);
                    if (anticipatedOuts.Any())
                    {
                        _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinterPreferences, _neutronVariables.PrintPreview);
                    }
                    else
                    {
                        MessageBox.Show($"No Anticipated Outs to print in Area: {area.AreaNumber}.");
                    }
                }
            }
            else
            {
                var areaNumber = int.Parse((string)ComboBoxAreaNumber.SelectedValue);
                var areaId = _areaRepository.GetAreaId(areaNumber);
                anticipatedOuts = GetAnticipatedOutsByArea(areaId);
                if (anticipatedOuts.Any())
                {
                    _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinterPreferences,
                        _neutronVariables.PrintPreview);
                }
                else
                {
                    MessageBox.Show($"No Anticipated Outs to print in Area: {areaNumber}.");
                }
            }
        }

        private void ButtonPrintPacking_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (!orders.Any()) return;
            foreach (var order in orders)
            {
                PrintPackingList(order.Id);
            }
        }

        private void MBAdjustOrder_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Any())
            {
                ShowOrderDetailsByOrderAndWorkstation(orders.First(), _workstationView);
            }
        }

        private async void MBAdjustOrderSave_Click(object sender, EventArgs e)
        {
            Order order = null;
            var detailLinesThisStation = (List<OrderDetail>)DataGridViewAdjust.DataSource;
            if (detailLinesThisStation != null && detailLinesThisStation.Count > 0)
            {
                foreach (var detail in detailLinesThisStation)
                {
                    order = detail.Order;
                    detail.LineStatusId = (int)LineStatus.Complete;
                    detail.EmpId = GlobalVar.User.EmpId;
                    await _historyManager.SaveHistoryAsync(ActionCode.StoreRack, detail);
                    await _repoOrderDetails.UpdateAsync(detail);
                }
                //Mediator.GetInstance().OnBatchComplete(this);
            }

            if (order != null)
            {
                await CheckForOrderComplete(order);
            }

            ShowAvailableOrdersRack();
            DataGridViewAvailableOrdersRack.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = AvailableRack;
        }

        private void MBAdjustOrderBack_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrdersRack.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = AvailableRack;
        }

        private async void MBRackOrderComplete_Click(object sender, EventArgs e)
        {

            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    var detailLinesThisStation = _repoOrderDetails.FindBy(r => r.OrderId == order.Id && _areaIdsForThisWorkstation.Contains(r.AreaId)).ToList();

                    if (detailLinesThisStation.Count > 0)
                    {
                        foreach (var detail in detailLinesThisStation)
                        {
                            detail.LineStatusId = (int)LineStatus.Complete;
                            detail.PickedQuantity = detail.Quantity;
                            detail.EmpId = GlobalVar.User.EmpId;
                            _historyManager.SaveHistory(ActionCode.PickRack, detail, _workstationView.WorkstationId);
                            await _repoOrderDetails.UpdateAsync(detail);
                        }
                        // Mediator.GetInstance().OnBatchComplete(this);
                    }

                    if (order != null)
                    {
                        if (await CheckForOrderComplete(order))
                        {
                            if (_neutronVariables.PrintPackingListEnd)
                            {
                                PrintPackingList(order.Id);
                            }
                        }
                    }
                }

                //var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                //uploadProcessor.CreateHostFileRack(orders);

                ShowAvailableOrdersRack();
                TextBoxFindAvailableOrdersRack.Focus();
            }

        }

        private void MBSkipped_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = false;
            MBKillOrder.Visible = false;
            ShowSkipped();
            Cursor.Current = Cursors.Default;
        }

        private void ShowSkipped()
        {
            var skipViews = _ordersRepository.GetSkippedOrders().ToList();

            var blv = new BindingListView<SkipView>(skipViews);
            _bindingSourceSkipView.DataSource = blv;
            DataGridViewSkip.DataSource = _bindingSourceSkipView;
            GetRecordCount(_bindingSourceSkipView);
            DataGridViewSkip.ClearSelection();
            LabelFormTitle.Text = _resourceManager.GetString($"SkipManager");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = Skip;
        }

        private void MBBackSkip_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrders.Refresh();
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Available;
            ShowAvailableStagingOrders();
            MBCompress.Visible = false;
            MBKillOrder.Visible = false;  // _workstationView.StationType.Id == (int)StationType.Supervisor;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)StationType.Supervisor;


            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = OrderListing;
            Cursor.Current = Cursors.Default;
            MBShowAvailable.Focus();
        }

        //private void MBSelectAllSkip_Click(object sender, EventArgs e)
        //{
        //    SelectAll(DataGridViewSkip);
        //}

        //private void MBClearSelectionSkip_Click(object sender, EventArgs e)
        //{
        //    ClearSelection(DataGridViewSkip);
        //}

        private void MBInventorySkip_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }

            if (_bindingSourceSkipView.Current != null)
            {
                var cur = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
                var skipInventoryViews = new List<SkipInventoryView>();

                var inventoryViews = _inventoryRepository.GetInventoryViewByItem(cur.Item).ToList();

                foreach (var invView in inventoryViews)
                {
                    var skipInventory = new SkipInventoryView();
                    skipInventory.InventoryId = invView.Id;
                    skipInventory.StorageType = invView.StorageTypeName;
                    skipInventory.AreaId = invView.AreaId;
                    skipInventory.Quantity = invView.Quantity;
                    skipInventory.Slot = invView.Slot;
                    skipInventory.PickSequence = invView.PickSequence;
                    skipInventory.Item = invView.Item;
                    skipInventory.Description = invView.Description;

                    skipInventoryViews.Add(skipInventory);
                }

                var blv = new BindingListView<SkipInventoryView>(skipInventoryViews);
                _bindingSourceSkipView.DataSource = blv;
                DataGridViewSkipInventory.DataSource = _bindingSourceSkipView;
                GetRecordCount(_bindingSourceSkipView);
                DataGridViewSkipInventory.ClearSelection();



                LabelFormTitle.Text = $"Inventory for Skipped Item.";
                LabelItemNumber.Text = cur.Item;
                LabelRequiredQuantity.Text = cur.Quantity.ToString();
                LabelPickedQuantity.Text = "0";
                //LabelFormTitle.Text = _resourceManager.GetString($"Inventory for {cur.Item}.");
                LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
                tabControl1.SelectedTab = SkipInventory;

            }
        }

        private void MBSkipInventoryBack_Click(object sender, EventArgs e)
        {
            ShowSkipped();
        }



        private async void MBPickComplete_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }

            var recs = GetSelectedPickViews(DataGridViewSkip);

            foreach (var currentSkip in recs)
            {

                //var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
                currentSkip.Picked = currentSkip.Quantity;
                _historyManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

                currentSkip.Picked = currentSkip.Quantity;
                _historyManager.SaveHistory(ActionCode.SkipPick, currentSkip);

                var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

                detail.LineStatusId = (int)LineStatus.Complete;
                detail.PickedQuantity = detail.Quantity;
                detail.EmpId = GlobalVar.User.EmpId;
                await _repoOrderDetails.UpdateAsync(detail);
                if (detail.Order != null)
                {
                    if (await CheckForOrderComplete(detail.Order))
                    {
                        if (_neutronVariables.PrintPackingListEnd)
                        {
                            PrintPackingList(detail.Order.Id);
                        }
                    }
                }
            }

            ShowSkipped();
        }

        private async void MBPickZero_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }

            var recs = GetSelectedPickViews(DataGridViewSkip);

            foreach (var currentSkip in recs)
            {

                //    var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
                currentSkip.Picked = 0;
                _historyManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

                //currentSkip.Picked = 0;
                _historyManager.SaveHistory(ActionCode.SkipPick, currentSkip);

                var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

                detail.LineStatusId = (int)LineStatus.Complete;
                detail.PickedQuantity = 0;
                detail.EmpId = GlobalVar.User.EmpId;
                await _repoOrderDetails.UpdateAsync(detail);
                if (detail.Order != null)
                {
                    if (await CheckForOrderComplete(detail.Order))
                    {
                        if (_neutronVariables.PrintPackingListEnd)
                        {
                            PrintPackingList(detail.Order.Id);
                        }
                    }
                }
            }

            ShowSkipped();
        }


        private async void MBAdjustQuantity_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }

            var recs = GetSelectedPickViews(DataGridViewSkip);

            foreach (var currentSkip in recs)
            {

                //  var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
                using (var form = new FrmChangeQuantityOnly(currentSkip))
                {
                    form.NewQty = currentSkip.Quantity;
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        var newQty = form.NewQty;
                        currentSkip.Picked = newQty;
                        _historyManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

                        //currentSkip.Picked = newQty;
                        _historyManager.SaveHistory(ActionCode.SkipPick, currentSkip);

                        // Mediator.GetInstance().OnBatchComplete(this);

                        var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

                        detail.LineStatusId = (int)LineStatus.Complete;
                        detail.PickedQuantity = newQty;
                        detail.EmpId = GlobalVar.User.EmpId;
                        await _repoOrderDetails.UpdateAsync(detail);
                        if (detail.Order != null)
                        {
                            if (await CheckForOrderComplete(detail.Order))
                            {
                                if (_neutronVariables.PrintPackingListEnd)
                                {
                                    PrintPackingList(detail.Order.Id);
                                }
                            }
                        }
                    }
                }
            }

            ShowSkipped();
        }

        private void MBCompress_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CompressOrders();
            Cursor.Current = Cursors.Default;
        }

        private void MBShowReplenOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Replen;
            ShowReplenOrders();
            MBCompress.Visible = false;
            MBKillOrder.Visible = false; // _workstationView.StationType.Id == (int)StationType.Supervisor;
            MBDeleteOrder.Visible = _workstationView.StationType.Id == (int)StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }

        private async void MBShortPick_Click(object sender, EventArgs e)
        {
            await ShortPick();
        }

        //private void ShortPick()
        //{
        //    if (_currentPickStop.PickViews.Count > 1 && _neutronVariables.SpecialBackOrder)
        //    {
        //        EnablePickAccept(false);
        //        SpecialPickAccept();
        //        EnablePickAccept(true);
        //    }
        //    else
        //    {
        //        var pickView = _currentPickStop.PickViews.FirstOrDefault();

        //        // get the total of all picklocations before clearing picklocations
        //        // use that total for the OrderDetail Update
        //        var pickViewTotal = _currentPickStop.GetPickViewTotal(pickView);

        //        ////write any picklocations to history
        //        UpdatePickViewInventoryQuantity(pickView);

        //        pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
        //        pickView.OrderDetail.PickedQuantity = pickViewTotal;
        //        _repoOrderDetails.Update(pickView.OrderDetail);

        //        // function to finish the move and update the lights for the next pick
        //        CompleteSpecialPick();
        //    }
        //}

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
                    await _repoOrderDetails.UpdateAsync(pickView.OrderDetail);
                }
            }

            // function to finish the move and update the lights for the next pick
            await CompleteSpecialPick();
            //EnablePickAccept(true);
        }

        private void MBOffCarousel_Click(object sender, EventArgs e)
        {
            ShowAvailableRackScreen();
        }

        private void ShowAvailableRackScreen()
        {
            ShowAvailableOrdersRack();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.FromArgb(0, 120, 215);
            tabControl1.SelectedTab = AvailableRack;
        }

        private void MBRackHotAction_Click(object sender, EventArgs e)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions]) return;
            var workstationView = _workstationRepository.GetStationView(AreaEight);
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
                       , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _inventoryUnitOfWork, _iptiDisplayFunctions, _inventoryRepository))

            {
                var result = frm.ShowDialog();
                Show();
                //   Task.Run(() => _deviceManager.Reset());
                //    // ---         Task.Run(() => _logger.LogDetailAsync($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
            }

            // var location = _currentPickStop.CurrentInventoryLocation.Location;
            // PositionDevice(location.Loc1, location.Loc2, location.Loc3, location.Loc4, true);
        }


        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();
                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmPick", resourceDir: languageDirectory, usingResourceSet: null);
                _gridResourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "GridHeaders", resourceDir: languageDirectory, usingResourceSet: null);

                //Main Panel
                LabelFormHeaderText.Text = _resourceManager.GetString($"LabelFormHeaderText");
                LabelFormTitle.Text = _resourceManager.GetString($"LabelFormTitle");
                MBMainClose.Text = _resourceManager.GetString($"MBMainClose");
                MBMainAvailableOrders.Text = _resourceManager.GetString($"MBMainAvailableOrders");
                MBMainOrderManager.Text = _resourceManager.GetString($"MBMainOrderManager");
                MBMainNewOrder.Text = _resourceManager.GetString($"MBMainNewOrder");
                // Order Loading and Uploading
                MBMainLoadOrders.Text = _resourceManager.GetString($"RunLoaderContinuously");
                MBMainUpload.Text = _resourceManager.GetString($"RunUploadContinuously");
                MBRunLoader.Text = _resourceManager.GetString($"RunLoaderOnce");
                MBRunUploadOnce.Text = _resourceManager.GetString($"RunUploadOnce");
                //Order Listing Panel
                MBSkipped.Text = _resourceManager.GetString($"MBSkipped");
                MBShowAvailable.Text = _resourceManager.GetString($"MBShowAvailable");
                MBCompleted.Text = _resourceManager.GetString($"MBCompleted");
                MBShowReplenOrders.Text = _resourceManager.GetString($"MBShowReplenOrders");
                LabelFindDescription.Text = _resourceManager.GetString($"LabelFindDescription");
                MButtonSearch.Text = _resourceManager.GetString($"MButtonSearch");
                MButtonClose.Text = _resourceManager.GetString($"MButtonClose");
                ButtonPrintAO.Text = _resourceManager.GetString($"ButtonPrintAO");
                ButtonPrintPacking.Text = _resourceManager.GetString($"ButtonPrintPacking");
                MBPrintPick.Text = _resourceManager.GetString($"MBPrintPick");
                MBOffCarousel.Text = _resourceManager.GetString($"MBOffCarousel");
                MBHold.Text = _resourceManager.GetString($"MBHold");
                MBRelease.Text = _resourceManager.GetString($"MBRelease");
                MBPriority.Text = _resourceManager.GetString($"MBPriority");
                MBCompress.Text = _resourceManager.GetString($"MBCompress");
                MBReturnToStock.Text = _resourceManager.GetString($"MBReturnToStock");
                MBDeleteOrder.Text = _resourceManager.GetString($"MBDeleteOrder");
                MBJobDetails.Text = _resourceManager.GetString($"MBJobDetails");
                MBPrintOrderListing.Text = _resourceManager.GetString($"MBPrintOrderListing");
                MBFillOptimized.Text = _resourceManager.GetString($"MBFillOptimized");

                //Available Orders
                //MbPrintAvailableOrders.Text = _resourceManager.GetString($"MbPrintAvailableOrders");
                MBAvailableOrdersRefresh.Text = _resourceManager.GetString($"MBAvailableOrdersRefresh");
                MBGo.Text = _resourceManager.GetString($"MBGo");
                LabelAvailableOrdersSearchFor.Text = _resourceManager.GetString($"LabelAvailableOrdersSearchFor");
                MBSearchAvailableOrders.Text = _resourceManager.GetString($"MBSearchAvailableOrders");
                MBAvailableOrdersBack.Text = _resourceManager.GetString($"MBAvailableOrdersBack");
                MBFill.Text = _resourceManager.GetString($"MBFill");
                MBFillStarters.Text = _resourceManager.GetString($"MBFillStarters");
                //MBShowSkipped.Text = _resourceManager.GetString($"MBShowSkipped");
                MBGo2.Text = _resourceManager.GetString($"MBGo2");

                //Pick List
                MBPrintPickList.Text = _resourceManager.GetString($"MBPrintPickList");
                MBStart.Text = _resourceManager.GetString($"MBStart");
                MBPickListBack.Text = _resourceManager.GetString($"MBPickListBack");

                //Pick Screen
                MBLocationCount.Text = _resourceManager.GetString($"MBLocationCount");
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"MBShowOrderOrQuantityToggle");
                MBPickScreenHotPick.Text = _resourceManager.GetString($"MBPickScreenHotPick");
                MBResetCarousels.Text = _resourceManager.GetString($"MBResetCarousels");
                MBPrint.Text = _resourceManager.GetString($"MBPrint");
                MBPickNewItem.Text = _resourceManager.GetString($"MBPickNewItem");
                MBPickBack.Text = _resourceManager.GetString($"MBPickBack");
                LabelItem.Text = _resourceManager.GetString($"LabelItem");
                LabelUOI.Text = _resourceManager.GetString($"LabelUOI");
                LabelQty.Text = _resourceManager.GetString($"LabelQty");
                GroupBoxLocation.Text = _resourceManager.GetString($"GroupBoxLocation");
                LabelDevice.Text = _resourceManager.GetString($"LabelDevice");
                LabelTray.Text = _resourceManager.GetString($"LabelTray");
                LabelOver.Text = _resourceManager.GetString($"LabelOver");
                LabelBack.Text = _resourceManager.GetString($"LabelBack");
                LabelReceivedDate.Text = _resourceManager.GetString($"LabelReceivedDate");
                LabelLocationQty.Text = _resourceManager.GetString($"LabelLocationQty");
                LabelTotalQty.Text = _resourceManager.GetString($"LabelTotalQty");
                LabelReqQty.Text = _resourceManager.GetString($"LabelReqQty");
                LabelPickedSoFar.Text = _resourceManager.GetString($"LabelPickedSoFar");
                MBPickChangeQuantity.Text = _resourceManager.GetString($"MBPickChangeQuantity");
                MBSkipPick.Text = _resourceManager.GetString($"MBSkipPick");
                MBShortPick.Text = _resourceManager.GetString($"MBShortPick");
                MBPickAccept.Text = _resourceManager.GetString($"MBPickAccept");

                //Order Details
                MBReturnToStockOrderDetail.Text = _resourceManager.GetString($"MBReturnToStockOrderDetail");
                MBHoldDetail.Text = _resourceManager.GetString($"MBHoldDetail");
                MBReleaseDetail.Text = _resourceManager.GetString($"MBReleaseDetail");
                MBPrintOrderDetails.Text = _resourceManager.GetString($"MBPrintOrderDetails");
                MBOrderDetailsBack.Text = _resourceManager.GetString($"MBOrderDetailsBack");

                //New Order
                GroupBoxOrderInformation.Text = _resourceManager.GetString($"GroupBoxOrderInformation");
                LabelJob.Text = _resourceManager.GetString($"LabelJob");
                LabelInvoice.Text = _resourceManager.GetString($"LabelInvoice");
                LabelPriority.Text = _resourceManager.GetString($"LabelPriority");
                MBNewOrderSave.Text = _resourceManager.GetString($"MBNewOrderSave");
                MBNewOrderClose.Text = _resourceManager.GetString($"MBNewOrderClose");
                GroupBoxDetailInformation.Text = _resourceManager.GetString($"GroupBoxDetailInformation");
                LabelNewOrderItem.Text = _resourceManager.GetString($"LabelNewOrderItem");
                LabelNewOrderDescription.Text = _resourceManager.GetString($"LabelNewOrderDescription");
                LabelNewOrderQuantity.Text = _resourceManager.GetString($"LabelNewOrderQuantity");
                LabelCostCenter.Text = _resourceManager.GetString($"LabelCostCenter");
                ButtonAddDetail.Text = _resourceManager.GetString($"ButtonAddDetail");
                LabelSearchForItem.Text = _resourceManager.GetString($"LabelSearchForItem");
                MBNewOrderSearch.Text = _resourceManager.GetString($"MBNewOrderSearch");
                ButtonRemoveLine.Text = _resourceManager.GetString($"ButtonRemoveLine");

                //Available Rack
                MbPrintAvailableOrdersRack.Text = _resourceManager.GetString($"MbPrintAvailableOrdersRack");
                MBRefreshRack.Text = _resourceManager.GetString($"MBRefreshRack");
                LabelSearchForRack.Text = _resourceManager.GetString($"LabelSearchForRack");
                MBSearchAvailableOrdersRack.Text = _resourceManager.GetString($"MBSearchAvailableOrdersRack");
                MBRackHotAction.Text = _resourceManager.GetString($"MBRackHotAction");
                MBRackBack.Text = _resourceManager.GetString($"MBRackBack");
                MBPrintDocument.Text = _resourceManager.GetString($"MBPrintDocument");
                MBPrintToteLabel.Text = _resourceManager.GetString($"MBPrintToteLabel");
                MBRackOrderComplete.Text = _resourceManager.GetString($"MBRackOrderComplete");
                MBAdjustOrder.Text = _resourceManager.GetString($"MBAdjustOrder");

                //Adjust Order
                MBAdjustOrderSave.Text = _resourceManager.GetString($"MBAdjustOrderSave");
                MBAdjustOrderBack.Text = _resourceManager.GetString($"MBAdjustOrderBack");

                //Skip
                MBInventorySkip.Text = _resourceManager.GetString($"MBInventorySkip");
                MBPrintSkip.Text = _resourceManager.GetString($"MBPrintSkip");
                MBPickComplete.Text = _resourceManager.GetString($"MBPickComplete");
                MBAdjustQuantity.Text = _resourceManager.GetString($"MBAdjustQuantity");
                MBPickZero.Text = _resourceManager.GetString($"MBPickZero");
                MBBackSkip.Text = _resourceManager.GetString($"MBBackSkip");

                //Skip Inventory




            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString($"ErrorLoadingLanguages")} {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void MBResetCarousels_Click(object sender, EventArgs e)
        {
            _deviceManager.Reset();
            _logger.LogDetailAsync($"Reset After Reset Carousel Button Pushed : [{DateTime.Now.ToLongTimeString()}]").SafeFireAndForget();
        }

        public void StartSpaceBarEnableTimer()
        {
            //TODO: Fix or Remove Spacebar and Accept Button Delay
            //var startTimeSpan = TimeSpan.FromMilliseconds(0);
            //var periodTimeSpan = TimeSpan.FromMilliseconds(500);
            //_spaceBarDelayTimer = new Timer(t => { SetSpaceBarDisabledFalse(); }, null, startTimeSpan, periodTimeSpan);
        }

        private void SetSpaceBarDisabledFalse()
        {
            //TODO: Fix or Remove Spacebar and Accept Button Delay
            //_spaceBarDisabled = false;
            //EnablePickAccept(true);
            //this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmPick_KeyDown);
            //_spaceBarDelayTimer.Dispose();
            //// ---         Task.Run(() => _logger.LogDetailAsync("*********  You can press the space bar now.  ***********"));
        }

        //TODO: Fix or Remove Spacebar and Accept Button Delay
        //TODO: All references to EnablePickAccept have been commented out

        private void EnablePickAccept(bool b)
        {
            if (MBPickAccept.InvokeRequired)
            {
                var d = new UpdatePickAcceptDelegate(EnablePickAccept);
                BeginInvoke(d, new object[] { b });
            }
            else
            {
                MBPickAccept.Enabled = b;
            }
        }

        private async Task OnFrmPickKeyDown(object sender, KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown");
            }
            if (tabControl1?.SelectedTab?.Name == null)
            {
                return;
            }
            switch (tabControl1.SelectedTab.Name)
            {
                case "PickScreen":
                    await HandlePickScreenKeyDown(e);
                    break;
                case "AvailableOrders":
                    await HandleAvailableOrdersKeyDown(e);
                    break;
            }
        }
        private async Task HandlePickScreenKeyDown(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync($"FrmPick_KeyDown: TabControl Name: PickScreen");
            }
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    await HandleEnterKey(e);
                    break;
                case Keys.Space:
                    await HandleSpaceKey(e);
                    break;
                case Keys.L:
                    await HandleLKey(e);
                    break;
                case Keys.A:
                    await HandleAKey(e);
                    break;
                case Keys.S:
                    await HandleSKey(e);
                    break;
                case Keys.Q:
                    await HandleQKey(e);
                    break;
                case Keys.B:
                    await HandleBKey(e);
                    break;
                case Keys.H:
                    await HandleHKey(e);
                    break;
                case Keys.F2:
                    await HandleF2Key(e);
                    break;
                case Keys.OemQuestion:
                    await HandleOemQuestionKey(e);
                    break;
                case Keys.F12:
                    await HandleF12Key(e);
                    break;
            }
        }
        private async Task HandleEnterKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: ENTER Key");
            }
            e.Handled = true;
            //PickAccept();
        }
        private async Task HandleSpaceKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: SPACE Key");
            }
            e.Handled = true;
            if (!_spaceBarDisabled)
            {
                if (_logger != null)
                {
                    await _logger.LogDetailAsync("FrmPick_KeyDown: SPACE Key ENABLED");
                }
                await PickAccept();
            }
            else
            {
                if (_logger != null)
                {
                    await _logger.LogDetailAsync("FrmPick_KeyDown: SPACE Key DISABLED");
                }
            }
        }
        private async Task HandleLKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: L Key");
            }
            await LocationCount();
            e.Handled = true;
        }
        private async Task HandleAKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: A Key");
            }
            await HotAction();
            e.Handled = true;
        }
        private async Task HandleSKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: S Key");
            }
            await ShowOrderOrQuantityToggle();
            e.Handled = true;
        }
        private async Task HandleQKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: Q Key");
            }
            await ChangeQuantity();
            e.Handled = true;
        }
        private async Task HandleBKey(KeyEventArgs e)
        {
            e.Handled = true;
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: B Key");
            }
            await ShortPick();
        }
        private async Task HandleHKey(KeyEventArgs e)
        {
            e.Handled = true;
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: H Key");
            }
            await SkipPick();
        }
        private async Task HandleF2Key(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: F2 Key");
            }
            PrintLabels(_currentPickStop, 2);
            e.Handled = true;
        }
        private async Task HandleOemQuestionKey(KeyEventArgs e)
        {
            if (_logger != null)
            {
                await _logger.LogDetailAsync("FrmPick_KeyDown: OEM QUESTION Key");
            }
            ShowShortCutForm();
            e.Handled = true;
        }
        private async Task HandleF12Key(KeyEventArgs e)
        {
            using (var frm = DI.Create<FrmInventory>())
            {
                var result = frm.ShowDialog();
                Show();
            }
        }
        private async Task HandleAvailableOrdersKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    // _logger.LogDetailAsync("FrmPick_KeyDown: ENTER Key");
                    // e.Handled = true;
                    // Go();
                    break;
            }
        }

        private async void FrmPick_KeyDown(object sender, KeyEventArgs e)
        {
            _logger.LogDetailAsync($"FrmPick_KeyDown").SafeFireAndForget();



            switch (tabControl1.SelectedTab.Name)
            {
                case "PickScreen":
                    {
                        _logger.LogDetailAsync($"FrmPick_KeyDown: TabControl Name: PickScreen").SafeFireAndForget();
                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: ENTER Key").SafeFireAndForget();
                                    e.Handled = true;
                                    //PickAccept();
                                    break;
                                }
                            case Keys.Space:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: SPACE Key").SafeFireAndForget();
                                    e.Handled = true;
                                    if (!_spaceBarDisabled)
                                    {
                                        _logger.LogDetailAsync($"FrmPick_KeyDown: SPACE Key ENABLED").SafeFireAndForget();
                                        await PickAccept();
                                    }
                                    else
                                    {
                                        _logger.LogDetailAsync($"FrmPick_KeyDown: SPACE Key DISABLED").SafeFireAndForget();
                                    }

                                    break;
                                }
                            case Keys.L:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: L  Key").SafeFireAndForget();
                                    await LocationCount();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.A:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: A  Key").SafeFireAndForget();
                                    await HotAction();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.S:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: S  Key").SafeFireAndForget();
                                    await ShowOrderOrQuantityToggle();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.Q:
                                {

                                    _logger.LogDetailAsync($"FrmPick_KeyDown: Q  Key").SafeFireAndForget();
                                    await ChangeQuantity();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.B:
                                {
                                    e.Handled = true;
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: B  Key").SafeFireAndForget();
                                    await ShortPick();

                                    break;
                                }
                            case Keys.H:
                                {
                                    e.Handled = true;
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: H  Key").SafeFireAndForget();
                                    await SkipPick();

                                    break;
                                }
                            case Keys.F2:
                                {
                                    _logger.LogDetailAsync($"FrmPick_KeyDown: F2  Key").SafeFireAndForget();
                                    PrintLabels(_currentPickStop, 2);
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.OemQuestion:
                                _logger.LogDetailAsync($"FrmPick_KeyDown: OEM QUESTION Key").SafeFireAndForget();
                                ShowShortCutForm();
                                e.Handled = true;
                                break;

                            case Keys.F12:
                                {
                                    using (var frm = DI.Create<FrmInventory>())
                                    {
                                        var result = frm.ShowDialog();
                                        Show();
                                        break;
                                    }
                                }
                        }

                        break;
                    }
                case "AvailableOrders":
                    {

                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                {
                                    //_logger.LogDetailAsync($"FrmPick_KeyDown: ENTER Key");
                                    //e.Handled = true;
                                    //Go();
                                    break;
                                }
                        }

                        break;
                    }
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

        private void MBSkipInventoryAdjustQuantity_Click(object sender, EventArgs e)
        {

        }

        private void MBPrintSkip_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }
        }

        private async void MBFillOptimized_Click(object sender, EventArgs e)
        {
            // GetCommonParts();

            var emptyBatchPositions = _ordersToPick.Where(r => r.OrderId == 0).ToList();
            var count = 0;
            var totalRows = DataGridViewAvailableOrders.Rows.Count;
            if (totalRows <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
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
        }

        private void GetCommonParts()
        {
            var orders = new List<Order>();

            using (var context = new NeutronDb())
            {
                orders = context.Orders.Include("OrderDetails").ToList();
            }

            var commonParts = orders
                .SelectMany(order => order.OrderDetails, (order, orderDetail) => new { order.Id, orderDetail.PartNum })
                .GroupBy(x => x.PartNum)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    PartId = g.Key,
                    Orders = g.Select(x => x.Id).ToList()
                });
            // Display the results
            foreach (var part in commonParts)
            {
                Console.WriteLine($"PartId: {part.PartId} is common in Orders: {string.Join(", ", part.Orders)}");
            }
        }

        private void DataGridPickView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridPickView.ClearSelection();
            DataGridPickView_FormatRows();
        }

        private void MBKillOrder_Click(object sender, EventArgs e)
        {
            //var orders = GetSelectedOrders(DataGridView1);
            //if (orders.Any()) KillOrder(orders);
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

        private async Task KillOrder(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        if (order.OrderStatusId == (int)OrderStatus.Available)
                        {
                            await KillLine(order.OrderDetails);
                            await _historyManager.SaveHistoryAsync(ActionCode.KillOrder, order);

                            await CheckForOrderComplete(order);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kill Order Error {Environment.NewLine}{ex.Message}", "Kill Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
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
        private async Task KillLine(ICollection<OrderDetail> orderDetails)
        {
            try
            {
                foreach (var orderDetail in orderDetails)
                {
                    var order = _repoOrders.FindByKey(orderDetail.OrderId);
                    if ((orderDetail.LineStatusId == (int)LineStatus.Available || orderDetail.LineStatusId == (int)LineStatus.Skipped)
                        && order.OrderStatusId == (int)OrderStatus.Available)
                    {
                        var areaId = orderDetail.AreaId;
                        orderDetail.PickedQuantity = 0;
                        orderDetail.LineStatusId = (int)LineStatus.Complete;
                        orderDetail.Order = order;
                        await _repoOrderDetails.UpdateAsync(orderDetail);
                        _historyManager.SaveHistory(ActionCode.KillLine, orderDetail, areaId);
                        await CheckForOrderComplete(order);
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
            //    var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            //    if (orderDetails.Any()) KillLine(orderDetails);
            //    ShowOrderDetailsByOrder(_currentJobDetailsOrderId);
        }
        /// <summary>
        /// Retrieves the details of the selected orders from a given DataGridView.
        /// </summary>
        /// <param name="dataGridView">The DataGridView from which to retrieve the selected order details.</param>
        /// <returns>A list of ReplenOrderDetail objects representing the details of the selected orders.
        /// If no orders are selected, a message box is displayed and an empty list is returned.</returns>
        private List<OrderDetail> GetSelectedOrderDetails(DataGridView dataGridView)
        {
            var orderDetails = new List<OrderDetail>();
            foreach (DataGridViewRow row in dataGridView.SelectedRows)
            {
                var orderDetailId = (int)row.Cells["OrderDetailId"].Value;
                var orderDetail = _repoOrderDetails.FindByKey(orderDetailId);
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

        private void MBKillOrderRack_Click(object sender, EventArgs e)
        {
            //var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            //if (orders.Any()) KillOrder(orders);
            //ShowAvailableOrdersRack();
        }

        private void MBKillLineSkip_Click(object sender, EventArgs e)
        {
            //var orderDetails = GetSelectedOrderDetails(DataGridViewSkip);
            //if (orderDetails.Any()) KillLine(orderDetails);
            //ShowSkipped();
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
        private async Task<bool> CheckForOrderComplete(Order order)
        {
            if (order == null) return false;

            // ---         Task.Run(() => _logger.LogDetailAsync($"CheckForOrderComplete   Ord:{order.Ord1}   Res:{order.Ord2} "));

            var lines = await _repoOrderDetails.FindByAsync(r => r.OrderId == order.Id);
            var linesNotComplete = lines.Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();

            if (linesNotComplete.Any())
            {
                await SetOrderStatusToAvailable(order);
                return false;
            }
            // ---         Task.Run(() => _logger.LogDetailAsync($"CheckForOrderComplete Order is Complete.  Ord:{order.Ord1}   Res:{order.Ord2} "));
            order.OrderStatusId = (int)OrderStatus.Complete;
            await _historyManager.SaveHistoryAsync(ActionCode.OrderComplete, order);
            await _repoOrders.UpdateAsync(order);
            return true;
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

        private void AvailableOrders_Enter(object sender, EventArgs e)
        {
            var textBoxPos1 = (TextBox)Controls.Find($"TextBoxPos1", true).First();
            textBoxPos1.Focus();
        }

        private async Task ClearItemFromBatchByPosition(int position)
        {
            await _logger.LogDetailAsync($" Start ClearItemFromBatchByPosition: {position}");
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

        private void DataGridViewOrderDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            MBKillLine.Enabled = false;
            //MBKillLine.Enabled = ((OrderDetailsView)_bindingSourceOrderDetailsView.Current).LineStatusId !=
            //                     (int)LineStatus.Complete;
        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ShowJobDetails();
        }

        private void TextBoxPickSlot_Enter(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.SelectAll();
            textbox.SelectedText = "";
            textbox.Focus();
        }

        private void TextBoxPickSlot_Click(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.SelectAll();
            textbox.SelectedText = "";
            textbox.Focus();
        }

        private async void MBClear_Click(object sender, EventArgs e)
        {
            
            var batchPositions = _ordersToPick.Where(r => r.OrderId != 0).ToList();
await ClearInductionScreen(batchPositions);
        }
        private async Task ClearInductionScreen(List<BatchPosition> batchPositions)
        {
            ClearAllSelectOrdersToPick();
            await ClearBatchPositions(batchPositions);
            //ClearOrderPositions(batchPositions);
            ClearDataGridViewBackColor();
            if (_iptiDisplayFunctions != null) await ClearIptiDisplayFunctions();

            SetFocusNextTextBoxPos();
            FocusOnCurrentTextBox();
        }
    }
}
