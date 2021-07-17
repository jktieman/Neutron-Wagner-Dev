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
using NeutronData.SqlModelViews;
using NeutronLoader;
using NeutronCore.Enums;
using Neutron.Interfaces;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using CurrentDeviceIndicator;
using Neutron.Controllers;
using Neutron.Enums;
using NeutronCore.Extensions;
using NeutronEvents;
using NeutronDllu;
using OrderStatus = NeutronCore.Enums.OrderStatus;

namespace Neutron.Forms
{
    public partial class FrmPick : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly AkaRepository _repoAka = new AkaRepository();
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<PrintJob> _repoPrintJob = new GenericRepository<PrintJob>(new NeutronDb());

        private readonly InventoryRepository _repoInv = new InventoryRepository();
        private readonly OrderDetailsRepository _orderDetailsRepository = new OrderDetailsRepository();
        private readonly LocationsRepository _locationsRepository = new LocationsRepository();
        private readonly IStationRepository _stationRepository;
        private readonly IOrdersRepository _ordersRepository;

        private BindingListView<AvailableOrdersView> _bindingListViewAvailableOrdersViews;

        private readonly BindingSource _bindingSourceCompleted = new BindingSource();
        private readonly BindingSource _bindingSourceOrderView = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrders = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrdersRack = new BindingSource();
        private readonly BindingSource _bindingSourcePickViews = new BindingSource();
        private readonly BindingSource _bindingSourcePickStops = new BindingSource();
        private readonly BindingSource _bindingSourceHot = new BindingSource();
        private readonly BindingSource _bindingSourceOrderDetailsView = new BindingSource();
        private readonly BindingSource _bindingSourceSkipView = new BindingSource();
        private readonly BindingSource _bindingSourceItems = new BindingSource();
        private BindingSource _bindingSourceNewItems = new BindingSource();

        public bool CloseButtonPressed { get; set; }
        public OrderView CurrentItem;
        public RackOrderView CurrentRackItem;
        private AvailableOrdersView _currentAvailableOrdersView;
        private TextBox _currentTextBoxPos;
        private bool ManualOverrideCurrentTextBoxPos;
        private List<BatchPosition> _ordersToPick = new List<BatchPosition>();
        private PickStop _currentPickStop = new PickStop();

        private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        private bool _openHotPickFromPickScreen;
        private bool _showSkipped;
        private bool _shortPick;

        private TabPage _previousTab;

        readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;

        private string _imagesDirectory;
        private PickDeviceManager _deviceManager;
        private DocumentToPrint _documentToPrint;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;

        private readonly IJsonData _jsonData;
        private readonly StationView _station;
        private readonly IAkaRepository _akaRepository;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;
        private DynamicLogger _logger;

        private CurrentDataSet _currentDataSet;
        private int _currentJobDetailsOrderId;
        private Dictionary<int, DeviceIndicator> _deviceIndicators;
        private DeviceIndicator _currentDeviceIndicator;

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

        private readonly int[] _moveableDeviceTypes;
        private readonly Station _rackStation;
        private readonly List<Station> _pickStations;

        private SynchronizationContext _synchronizationContext;
        private List<Inventory> _currentInventory;
        private bool _multiLocationStop;

        public FrmPick(IJsonData jsonData, StationView station
            , IAkaRepository akaRepository, NeutronVariables neutronVariables
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor,
            IImageManager imageManager, IStationRepository stationRepository
            , IOrdersRepository ordersRepository, NeutronLicense neutronLicense
            , IItemDefinitionsRepository itemDefinitionsRepository)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            _jsonData = jsonData;
            _station = station;
            _akaRepository = akaRepository;
            _neutronVariables = neutronVariables;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _stationRepository = stationRepository;
            _ordersRepository = ordersRepository;
            _neutronLicense = neutronLicense;
            _itemDefinitionsRepository = itemDefinitionsRepository;

            _rackStation = _stationRepository.GetRackStation();
            _moveableDeviceTypes = _stationRepository.GetMoveableDeviceTypeIds();
            _pickStations = _stationRepository.GetPickStations();
            _synchronizationContext = SynchronizationContext.Current;

            InitForm();
        }

        private void InitForm()
        {
            KeyPreview = true;
            SetupLogger();
            _logger.Log($"Form Pick Company Code: {_neutronLicense.CompanyCode}");
            if (_station.StationType.Id == (int)StationType.Supervisor)
            {
                MBMainLoadOrders.Visible = true;
                MBMainUpload.Visible = true;
                MBRunLoader.Visible = true;
                MBRunUpload.Visible = true;
            }
            SetupPrinters();

            InitGrids();

            SetupPickPositions(_neutronVariables.PickBatchSize);
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            HideTabControlTabs();
            ShowButtons();
            SetLoaderButtonText();
            SetUploadButtonText();
            mlUserInfo.Text = $"{_resourceManager.GetString($"CurrentUser")}{GlobalVar.User?.UserInfo}";
            CloseButtonPressed = false;
            _currentTextBoxPos = TextBoxPos1;
            ToolTipPickScreen.SetToolTip(ButtonMove, _resourceManager.GetString($"GetBin"));
            if (_neutronVariables.DisplaysEnabled && _neutronVariables.IptiDisplays)
            {
                GlobalVar.Displays.MySerialDataReceived -= ProcessDataReceived;
                GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
            }
            _documentToPrint = new DocumentToPrint();
            MBPrint.Visible = _neutronVariables.PrintPackingListManual;
            // MBFillStarters.Visible = _neutronVariables.SerialPicking;
            InitDataGridViewNewItems();
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
            MBPickScreenHotPick.Enabled = _securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions];
            if (_station.StationType.Id == (int)StationType.Supervisor || _station.StationType.Id == (int)StationType.Rack)
            {
                MBMainAvailableOrders.Text = _resourceManager.GetString($"OffCarousel");
            }

            Mediator.GetInstance().IptiButtonPressed += (s, e) => IptiButtonPickAccept(e.ResponseInfo);
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopUploadAction(e.StartStop);
            Mediator.GetInstance().OrderComplete += (s, e) => ShowOrderComplete(e.Order);
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

            if (!_skipGridReady)
            {
                var callback = new SendOrPostCallback(SetupSkipGrid);
                Thread thread =
                    new Thread(new ThreadStart(() => RunGrid(_synchronizationContext, callback)));
                thread.Start();
            }
            if (!_skipInventoryGridReady)
            {
                var callback = new SendOrPostCallback(SetupSkipInventoryGrid);
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
            uiContext.Post(setupGrid, null);
        }

        private void InitDeviceIndicators()
        {
            if (PickScreen.Controls.ContainsKey("PanelDeviceIndicators")) return;

            Console.WriteLine("Initialize Device Indicators - InitDeviceIndicators");
            _deviceIndicators = new Dictionary<int, DeviceIndicator>();

            //var hardwareDevices = _station.HardwareDevices.Where(x => _moveableDeviceTypes.Contains(x.DeviceTypeId)).ToList();
            //var numDevices = hardwareDevices.Count;
            var hardwareDevices = _station.HardwareDevices.ToList();
            var numDevices = hardwareDevices.Count;


            var panel = new Panel();
            panel.Location = new Point(189, 0);
            panel.Size = new Size(769, 127);
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

            PickScreen.Controls.Add(panel);

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

        private void FillComboBoxStationNumber()
        {
            List<string> stationNumbers = new List<string>();
            stationNumbers = _stationRepository.GetPickStationNumbers();
            if (stationNumbers.Count > 0)
            {
                stationNumbers.Insert(0, _resourceManager.GetString($"ALL"));
            }
            //else
            //{
            //    stationNumbers = new List<string> { "All", "1", "2", "3", "4", "5", "8" };
            //}
            ComboBoxStationNumber.DataSource = stationNumbers;
            ComboBoxStationNumber.SelectedIndex = 0;
        }

        private void ShowOrderComplete(Order order)
        {
            Task.Run(() => _logger.Log($"Show Order Complete Event: Order Number _ {order.Ord1} -- {order.Ord2}"));
            var bp = _ordersToPick.Where(o => o.OrderId == order.Id).FirstOrDefault();
            if (bp == null) return;
            string pos = bp.PositionNumber.ToString();
            try
            {
                if (Controls.Count > 0)
                {
                    Control c = Controls.Find("Pos" + pos + "Display", true).First();
                    if (c != null)
                    {
                        var panel = ((Panel) c);
                        panel.BackColor = Color.Green;
                        panel.Visible = true;
                        panel.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Control not Found {Environment.NewLine}{ex.Message}", "Control Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            bp.OrderComplete = true;
        }

        private void SetupPickPositions(int pickBatchSize)
        {
            switch (pickBatchSize)
            {
                case 6:
                    {
                        // 
                        // LabelPickPos1
                        // 
                        LabelPickPos1.BackColor = Color.RoyalBlue;
                        LabelPickPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos1.ForeColor = SystemColors.ControlText;
                        LabelPickPos1.Location = new Point(77, 77);
                        LabelPickPos1.Name = "LabelPickPos1";
                        LabelPickPos1.Size = new Size(36, 26);
                        LabelPickPos1.TabIndex = 120;
                        LabelPickPos1.Text = "1";
                        LabelPickPos1.TextAlign = ContentAlignment.MiddleCenter;
                        // 
                        // LabelPickPos2
                        // 
                        LabelPickPos2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos2.Location = new Point(267, 77);
                        LabelPickPos2.Name = "LabelPickPos2";
                        LabelPickPos2.Size = new Size(36, 26);
                        LabelPickPos2.TabIndex = 122;
                        LabelPickPos2.Text = "2";
                        LabelPickPos2.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPickPos2.Visible = false;
                        // 
                        // LabelPickPos3
                        // 
                        LabelPickPos3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos3.Location = new Point(457, 77);
                        LabelPickPos3.Name = "LabelPickPos3";
                        LabelPickPos3.Size = new Size(36, 26);
                        LabelPickPos3.TabIndex = 124;
                        LabelPickPos3.Text = "3";
                        LabelPickPos3.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPickPos3.Visible = false;
                        // 
                        // LabelPickPos4
                        // 
                        LabelPickPos4.BackColor = Color.RoyalBlue;
                        LabelPickPos4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos4.Location = new Point(647, 77);
                        LabelPickPos4.Name = "LabelPickPos4";
                        LabelPickPos4.Size = new Size(36, 26);
                        LabelPickPos4.TabIndex = 126;
                        LabelPickPos4.Text = "4";
                        LabelPickPos4.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPickPos4.Visible = false;
                        // 
                        // LabelPickPos5
                        // 
                        LabelPickPos5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos5.Location = new Point(837, 77);
                        LabelPickPos5.Name = "LabelPickPos5";
                        LabelPickPos5.Size = new Size(36, 26);
                        LabelPickPos5.TabIndex = 128;
                        LabelPickPos5.Text = "5";
                        LabelPickPos5.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPickPos5.Visible = false;
                        // 
                        // LabelPickPos6
                        // 
                        LabelPickPos6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPickPos6.Location = new Point(1027, 77);
                        LabelPickPos6.Name = "LabelPickPos6";
                        LabelPickPos6.Size = new Size(36, 26);
                        LabelPickPos6.TabIndex = 130;
                        LabelPickPos6.Text = "6";
                        LabelPickPos6.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPickPos6.Visible = false;
                        // 
                        // LabelPickPos7
                        // 
                        //this.LabelPickPos7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        //this.LabelPickPos7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.LabelPickPos7.Location = new System.Drawing.Point(907, 12);
                        //this.LabelPickPos7.Name = "LabelPickPos7";
                        //this.LabelPickPos7.Size = new System.Drawing.Size(26, 26);
                        //this.LabelPickPos7.TabIndex = 161;
                        //this.LabelPickPos7.Text = "7";
                        //this.LabelPickPos7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        LabelPickPos7.Visible = false;
                        // 
                        // LabelPickPos8
                        // 
                        //this.LabelPickPos8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        //this.LabelPickPos8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.LabelPickPos8.Location = new System.Drawing.Point(1048, 12);
                        //this.LabelPickPos8.Name = "LabelPickPos8";
                        //this.LabelPickPos8.Size = new System.Drawing.Size(26, 26);
                        //this.LabelPickPos8.TabIndex = 164;
                        //this.LabelPickPos8.Text = "8";
                        //this.LabelPickPos8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        LabelPickPos8.Visible = false;

                        LabelPickPos9.Visible = false;
                        LabelPickPos10.Visible = false;
                        LabelPickPos11.Visible = false;
                        LabelPickPos12.Visible = false;
                        LabelPickPos13.Visible = false;
                        LabelPickPos14.Visible = false;
                        LabelPickPos15.Visible = false;
                        LabelPickPos16.Visible = false;

                        // 
                        // TextBoxPickPos1
                        // 
                        TextBoxPickPos1.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos1.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos1.Location = new Point(28, 109);
                        TextBoxPickPos1.Multiline = true;
                        TextBoxPickPos1.Name = "TextBoxPickPos1";
                        TextBoxPickPos1.ReadOnly = true;
                        TextBoxPickPos1.Size = new Size(134, 44);
                        TextBoxPickPos1.TabIndex = 121;
                        TextBoxPickPos1.Tag = "0";
                        TextBoxPickPos1.Text = "";
                        TextBoxPickPos1.TextAlign = HorizontalAlignment.Center;
                        // 
                        // TextBoxPickPos2
                        // 
                        TextBoxPickPos2.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos2.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos2.Location = new Point(218, 109);
                        TextBoxPickPos2.Multiline = true;
                        TextBoxPickPos2.Name = "TextBoxPickPos2";
                        TextBoxPickPos2.ReadOnly = true;
                        TextBoxPickPos2.Size = new Size(134, 44);
                        TextBoxPickPos2.TabIndex = 123;
                        TextBoxPickPos2.Tag = "1";
                        TextBoxPickPos2.TextAlign = HorizontalAlignment.Center;
                        TextBoxPickPos2.Visible = false;
                        // 
                        // TextBoxPickPos3
                        // 
                        TextBoxPickPos3.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos3.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos3.Location = new Point(408, 109);
                        TextBoxPickPos3.Multiline = true;
                        TextBoxPickPos3.Name = "TextBoxPickPos3";
                        TextBoxPickPos3.ReadOnly = true;
                        TextBoxPickPos3.Size = new Size(134, 44);
                        TextBoxPickPos3.TabIndex = 125;
                        TextBoxPickPos3.Tag = "2";
                        TextBoxPickPos3.TextAlign = HorizontalAlignment.Center;
                        TextBoxPickPos3.Visible = false;
                        // 
                        // TextBoxPickPos4
                        // 
                        TextBoxPickPos4.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos4.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos4.Location = new Point(598, 109);
                        TextBoxPickPos4.Multiline = true;
                        TextBoxPickPos4.Name = "TextBoxPickPos4";
                        TextBoxPickPos4.ReadOnly = true;
                        TextBoxPickPos4.Size = new Size(134, 44);
                        TextBoxPickPos4.TabIndex = 127;
                        TextBoxPickPos4.Tag = "3";
                        TextBoxPickPos4.TextAlign = HorizontalAlignment.Center;
                        TextBoxPickPos4.Visible = false;
                        // 
                        // TextBoxPickPos5
                        // 
                        TextBoxPickPos5.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos5.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos5.Location = new Point(788, 109);
                        TextBoxPickPos5.Multiline = true;
                        TextBoxPickPos5.Name = "TextBoxPickPos5";
                        TextBoxPickPos5.ReadOnly = true;
                        TextBoxPickPos5.Size = new Size(134, 44);
                        TextBoxPickPos5.TabIndex = 129;
                        TextBoxPickPos5.Tag = "4";
                        TextBoxPickPos5.TextAlign = HorizontalAlignment.Center;
                        TextBoxPickPos5.Visible = false;
                        // 
                        // TextBoxPickPos6
                        // 
                        TextBoxPickPos6.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos6.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPickPos6.Location = new Point(978, 109);
                        TextBoxPickPos6.Multiline = true;
                        TextBoxPickPos6.Name = "TextBoxPickPos6";
                        TextBoxPickPos6.ReadOnly = true;
                        TextBoxPickPos6.Size = new Size(134, 44);
                        TextBoxPickPos6.TabIndex = 131;
                        TextBoxPickPos6.Tag = "5";
                        TextBoxPickPos6.TextAlign = HorizontalAlignment.Center;
                        TextBoxPickPos6.Visible = false;
                        // 
                        // TextBoxPickPos7
                        // 
                        //this.TextBoxPickPos7.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        //this.TextBoxPickPos7.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.TextBoxPickPos7.Location = new System.Drawing.Point(864, 44);
                        //this.TextBoxPickPos7.Multiline = true;
                        //this.TextBoxPickPos7.Name = "TextBoxPickPos7";
                        //this.TextBoxPickPos7.ReadOnly = true;
                        //this.TextBoxPickPos7.Size = new System.Drawing.Size(112, 44);
                        //this.TextBoxPickPos7.TabIndex = 160;
                        //this.TextBoxPickPos7.Tag = "6";
                        //this.TextBoxPickPos7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        TextBoxPickPos7.Visible = false;
                        // 
                        // TextBoxPickPos8
                        // 
                        //this.TextBoxPickPos8.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        //this.TextBoxPickPos8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.TextBoxPickPos8.Location = new System.Drawing.Point(1005, 44);
                        //this.TextBoxPickPos8.Multiline = true;
                        //this.TextBoxPickPos8.Name = "TextBoxPickPos8";
                        //this.TextBoxPickPos8.ReadOnly = true;
                        //this.TextBoxPickPos8.Size = new System.Drawing.Size(112, 44);
                        //this.TextBoxPickPos8.TabIndex = 165;
                        //this.TextBoxPickPos8.Tag = "7";
                        //this.TextBoxPickPos8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        TextBoxPickPos8.Visible = false;
                        TextBoxPickPos9.Visible = false;
                        TextBoxPickPos10.Visible = false;
                        TextBoxPickPos11.Visible = false;
                        TextBoxPickPos12.Visible = false;
                        TextBoxPickPos13.Visible = false;
                        TextBoxPickPos14.Visible = false;
                        TextBoxPickPos15.Visible = false;
                        TextBoxPickPos16.Visible = false;

                        // 
                        // Pos1Display
                        // 
                        Pos1Display.BackColor = Color.Transparent;
                        Pos1Display.Location = new Point(20, 109);
                        Pos1Display.Name = "Pos1Display";
                        Pos1Display.Size = new Size(150, 53);
                        Pos1Display.TabIndex = 153;
                        // 
                        // Pos2Display
                        // 
                        Pos2Display.BackColor = Color.Transparent;
                        Pos2Display.Location = new Point(210, 109);
                        Pos2Display.Name = "Pos2Display";
                        Pos2Display.Size = new Size(150, 53);
                        Pos2Display.TabIndex = 154;
                        Pos2Display.Visible = false;
                        // 
                        // Pos3Display
                        // 
                        Pos3Display.BackColor = Color.Transparent;
                        Pos3Display.Location = new Point(400, 109);
                        Pos3Display.Name = "Pos3Display";
                        Pos3Display.Size = new Size(150, 53);
                        Pos3Display.TabIndex = 155;
                        Pos3Display.Visible = false;
                        // 
                        // Pos4Display
                        // 
                        Pos4Display.BackColor = Color.Transparent;
                        Pos4Display.Location = new Point(590, 109);
                        Pos4Display.Name = "Pos4Display";
                        Pos4Display.Size = new Size(150, 53);
                        Pos4Display.TabIndex = 156;
                        Pos4Display.Visible = false;
                        // 
                        // Pos5Display
                        // 
                        Pos5Display.BackColor = Color.Transparent;
                        Pos5Display.Location = new Point(780, 109);
                        Pos5Display.Name = "Pos5Display";
                        Pos5Display.Size = new Size(150, 53);
                        Pos5Display.TabIndex = 157;
                        Pos5Display.Visible = false;
                        // 
                        // Pos6Display
                        // 
                        Pos6Display.BackColor = Color.Transparent;
                        Pos6Display.Location = new Point(970, 109);
                        Pos6Display.Name = "Pos6Display";
                        Pos6Display.Size = new Size(150, 53);
                        Pos6Display.TabIndex = 158;
                        Pos6Display.Visible = false;
                        // 
                        // Pos7Display
                        // 
                        //this.Pos7Display.BackColor = System.Drawing.Color.Transparent;
                        //this.Pos7Display.Location = new System.Drawing.Point(856, 44);
                        //this.Pos7Display.Name = "Pos7Display";
                        //this.Pos7Display.Size = new System.Drawing.Size(128, 53);
                        //this.Pos7Display.TabIndex = 162;
                        Pos7Display.Visible = false;

                        // 
                        // Pos8Display
                        // 
                        //this.Pos8Display.BackColor = System.Drawing.Color.Transparent;
                        //this.Pos8Display.Location = new System.Drawing.Point(997, 44);
                        //this.Pos8Display.Name = "Pos8Display";
                        //this.Pos8Display.Size = new System.Drawing.Size(128, 53);
                        //this.Pos8Display.TabIndex = 165;
                        Pos8Display.Visible = false;
                        Pos9Display.Visible = false;
                        Pos10Display.Visible = false;
                        Pos11Display.Visible = false;
                        Pos12Display.Visible = false;
                        Pos13Display.Visible = false;
                        Pos14Display.Visible = false;
                        Pos15Display.Visible = false;
                        Pos16Display.Visible = false;

                        //-----------------Induction Screen ----------------------
                        // 
                        // LabelPos1
                        // 
                        LabelPos1.BackColor = Color.RoyalBlue;
                        LabelPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos1.ForeColor = SystemColors.ControlText;
                        LabelPos1.Location = new Point(77, 77);
                        LabelPos1.Name = "LabelPos1";
                        LabelPos1.Size = new Size(36, 26);
                        LabelPos1.TabIndex = 151;
                        LabelPos1.Text = "1";
                        LabelPos1.TextAlign = ContentAlignment.MiddleCenter;
                        // 
                        // LabelPos2
                        // 
                        LabelPos2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos2.Location = new Point(267, 77);
                        LabelPos2.Name = "LabelPos2";
                        LabelPos2.Size = new Size(36, 26);
                        LabelPos2.TabIndex = 152;
                        LabelPos2.Text = "2";
                        LabelPos2.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPos2.Visible = false;
                        // 
                        // LabelPos3
                        // 
                        LabelPos3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos3.Location = new Point(457, 77);
                        LabelPos3.Name = "LabelPos3";
                        LabelPos3.Size = new Size(36, 26);
                        LabelPos3.TabIndex = 153;
                        LabelPos3.Text = "3";
                        LabelPos3.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPos3.Visible = false;
                        // 
                        // LabelPos4
                        // 
                        LabelPos4.BackColor = Color.RoyalBlue;
                        LabelPos4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos4.Location = new Point(647, 77);
                        LabelPos4.Name = "LabelPos4";
                        LabelPos4.Size = new Size(36, 26);
                        LabelPos4.TabIndex = 154;
                        LabelPos4.Text = "4";
                        LabelPos4.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPos4.Visible = false;
                        // 
                        // LabelPos5
                        // 
                        LabelPos5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos5.Location = new Point(837, 77);
                        LabelPos5.Name = "LabelPos5";
                        LabelPos5.Size = new Size(36, 26);
                        LabelPos5.TabIndex = 155;
                        LabelPos5.Text = "5";
                        LabelPos5.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPos5.Visible = false;
                        // 
                        // LabelPos6
                        // 
                        LabelPos6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                        LabelPos6.Location = new Point(1027, 77);
                        LabelPos6.Name = "LabelPos6";
                        LabelPos6.Size = new Size(36, 26);
                        LabelPos6.TabIndex = 156;
                        LabelPos6.Text = "6";
                        LabelPos6.TextAlign = ContentAlignment.MiddleCenter;
                        LabelPos6.Visible = false;
                        // 
                        // LabelPos7
                        // 
                        //this.LabelPos7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        //this.LabelPos7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.LabelPos7.Location = new System.Drawing.Point(619, 558);
                        //this.LabelPos7.Name = "LabelPos7";
                        //this.LabelPos7.Size = new System.Drawing.Size(26, 26);
                        //this.LabelPos7.TabIndex = 171;
                        //this.LabelPos7.Text = "7";
                        //this.LabelPos7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        LabelPos7.Visible = false;
                        // 
                        // LabelPos8
                        // 
                        //this.LabelPos8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        //this.LabelPos8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.LabelPos8.Location = new System.Drawing.Point(759, 557);
                        //this.LabelPos8.Name = "LabelPos8";
                        //this.LabelPos8.Size = new System.Drawing.Size(26, 26);
                        //this.LabelPos8.TabIndex = 172;
                        //this.LabelPos8.Text = "8";
                        //this.LabelPos8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        LabelPos8.Visible = false;
                        LabelPos9.Visible = false;
                        LabelPos10.Visible = false;
                        LabelPos11.Visible = false;
                        LabelPos12.Visible = false;
                        LabelPos13.Visible = false;
                        LabelPos14.Visible = false;
                        LabelPos15.Visible = false;
                        LabelPos16.Visible = false;
                        // 
                        // TextBoxPos1
                        // 
                        TextBoxPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos1.Location = new Point(28, 109);
                        TextBoxPos1.Name = "TextBoxPos1";
                        TextBoxPos1.Size = new Size(134, 44);
                        TextBoxPos1.TabIndex = 0;
                        TextBoxPos1.Tag = "0";
                        TextBoxPos1.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos1.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos1.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos1.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos1.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos2
                        // 
                        TextBoxPos2.BackColor = SystemColors.Control;
                        TextBoxPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos2.Location = new Point(218, 109);
                        TextBoxPos2.Name = "TextBoxPos2";
                        TextBoxPos2.Size = new Size(134, 44);
                        TextBoxPos2.TabIndex = 1;
                        TextBoxPos2.Tag = "1";
                        TextBoxPos2.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos2.Visible = false;
                        TextBoxPos2.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos2.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos2.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos2.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos3
                        // 
                        TextBoxPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos3.Location = new Point(408, 109);
                        TextBoxPos3.Name = "TextBoxPos3";
                        TextBoxPos3.Size = new Size(134, 44);
                        TextBoxPos3.TabIndex = 2;
                        TextBoxPos3.Tag = "2";
                        TextBoxPos3.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos3.Visible = false;
                        TextBoxPos3.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos3.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos3.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos3.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos4
                        // 
                        TextBoxPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos4.Location = new Point(598, 109);
                        TextBoxPos4.Name = "TextBoxPos4";
                        TextBoxPos4.Size = new Size(134, 44);
                        TextBoxPos4.TabIndex = 3;
                        TextBoxPos4.Tag = "3";
                        TextBoxPos4.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos4.Visible = false;
                        TextBoxPos4.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos4.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos4.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos4.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos5
                        // 
                        TextBoxPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos5.Location = new Point(788, 109);
                        TextBoxPos5.Name = "TextBoxPos5";
                        TextBoxPos5.Size = new Size(134, 44);
                        TextBoxPos5.TabIndex = 4;
                        TextBoxPos5.Tag = "4";
                        TextBoxPos5.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos5.Visible = false;
                        TextBoxPos5.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos5.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos5.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos5.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos6
                        // 
                        TextBoxPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                        TextBoxPos6.Location = new Point(978, 109);
                        TextBoxPos6.Name = "TextBoxPos6";
                        TextBoxPos6.Size = new Size(134, 44);
                        TextBoxPos6.TabIndex = 5;
                        TextBoxPos6.Tag = "5";
                        TextBoxPos6.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos6.Visible = false;
                        TextBoxPos6.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos6.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos6.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        //TextBoxPos6.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos7
                        // 
                        //this.TextBoxPos7.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.TextBoxPos7.Location = new System.Drawing.Point(577, 590);
                        //this.TextBoxPos7.Name = "TextBoxPos7";
                        //this.TextBoxPos7.Size = new System.Drawing.Size(110, 29);
                        //this.TextBoxPos7.TabIndex = 169;
                        //this.TextBoxPos7.Tag = "6";
                        //this.TextBoxPos7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        TextBoxPos7.Visible = false;
                        // 
                        // TextBoxPos8
                        // 
                        //this.TextBoxPos8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //this.TextBoxPos8.Location = new System.Drawing.Point(717, 590);
                        //this.TextBoxPos8.Name = "TextBoxPos8";
                        //this.TextBoxPos8.Size = new System.Drawing.Size(110, 29);
                        //this.TextBoxPos8.TabIndex = 170;
                        //this.TextBoxPos8.Tag = "7";
                        //this.TextBoxPos8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        TextBoxPos8.Visible = false;
                        TextBoxPos9.Visible = false;
                        TextBoxPos10.Visible = false;
                        TextBoxPos11.Visible = false;
                        TextBoxPos12.Visible = false;
                        TextBoxPos13.Visible = false;
                        TextBoxPos14.Visible = false;
                        TextBoxPos15.Visible = false;
                        TextBoxPos16.Visible = false;


                        // 
                        // AvailablePos1Display
                        // 
                        // this.AvailablePos1Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos1Display.Location = new Point(20, 109);
                        //  this.AvailablePos1Display.Name = "Pos1Display";
                        AvailablePos1Display.Size = new Size(150, 53);
                        //  this.AvailablePos1Display.TabIndex = 161;
                        AvailablePos1Display.Visible = true;
                        // 
                        // AvailablePos2Display
                        // 
                        // this.AvailablePos2Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos2Display.Location = new Point(210, 109);
                        // this.AvailablePos2Display.Name = "Pos2Display";
                        AvailablePos2Display.Size = new Size(150, 53);
                        // this.AvailablePos2Display.TabIndex = 162;
                        AvailablePos2Display.Visible = true;
                        // 
                        // AvailablePos3Display
                        // 
                        //  this.AvailablePos3Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos3Display.Location = new Point(400, 109);
                        //   this.AvailablePos3Display.Name = "Pos3Display";
                        AvailablePos3Display.Size = new Size(150, 53);
                        //  this.AvailablePos3Display.TabIndex = 163;
                        AvailablePos3Display.Visible = true;
                        // 
                        // AvailablePos4Display
                        // 
                        //  this.AvailablePos4Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos4Display.Location = new Point(590, 109);
                        //  this.AvailablePos4Display.Name = "Pos4Display";
                        AvailablePos4Display.Size = new Size(150, 53);
                        //  this.AvailablePos4Display.TabIndex = 164;
                        AvailablePos4Display.Visible = true;
                        // 
                        // AvailablePos5Display
                        // 
                        // this.AvailablePos5Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos5Display.Location = new Point(780, 109);
                        //  this.AvailablePos5Display.Name = "Pos5Display";
                        AvailablePos5Display.Size = new Size(150, 53);
                        // this.AvailablePos5Display.TabIndex = 165;
                        AvailablePos5Display.Visible = true;
                        // 
                        // AvailablePos6Display
                        // 
                        // this.AvailablePos6Display.BackColor = System.Drawing.Color.Transparent;
                        AvailablePos6Display.Location = new Point(970, 109);
                        // this.AvailablePos6Display.Name = "Pos6Display";
                        AvailablePos6Display.Size = new Size(150, 53);
                        //  this.AvailablePos6Display.TabIndex = 166;
                        AvailablePos6Display.Visible = true;
                        // 
                        // AvailablePos7Display
                        // 
                        //this.AvailablePos7Display.BackColor = System.Drawing.Color.Transparent;
                        //this.AvailablePos7Display.Location = new System.Drawing.Point(569, 590);
                        //this.AvailablePos7Display.Name = "Pos7Display";
                        //this.AvailablePos7Display.Size = new System.Drawing.Size(126, 53);
                        //this.AvailablePos7Display.TabIndex = 173;
                        AvailablePos7Display.Visible = false;
                        // 
                        // AvailablePos8Display
                        // 
                        //this.AvailablePos8Display.BackColor = System.Drawing.Color.Transparent;
                        //this.AvailablePos8Display.Location = new System.Drawing.Point(709, 590);
                        //this.AvailablePos8Display.Name = "Pos8Display";
                        //this.AvailablePos8Display.Size = new System.Drawing.Size(126, 53);
                        //this.AvailablePos8Display.TabIndex = 174;
                        AvailablePos8Display.Visible = false;
                        AvailablePos9Display.Visible = false;
                        AvailablePos10Display.Visible = false;
                        AvailablePos11Display.Visible = false;
                        AvailablePos12Display.Visible = false;
                        AvailablePos13Display.Visible = false;
                        AvailablePos14Display.Visible = false;
                        AvailablePos15Display.Visible = false;
                        AvailablePos16Display.Visible = false;

                        break;
                    }
                case 8:
                    {
                        // 
                        // LabelPickPos1
                        // 
                        LabelPickPos9.Visible = false;
                        LabelPickPos10.Visible = false;
                        LabelPickPos11.Visible = false;
                        LabelPickPos12.Visible = false;
                        LabelPickPos13.Visible = false;
                        LabelPickPos14.Visible = false;
                        LabelPickPos15.Visible = false;
                        LabelPickPos16.Visible = false;

                        // 
                        // TextBoxPickPos1
                        // 

                        TextBoxPickPos9.Visible = false;
                        TextBoxPickPos10.Visible = false;
                        TextBoxPickPos11.Visible = false;
                        TextBoxPickPos12.Visible = false;
                        TextBoxPickPos13.Visible = false;
                        TextBoxPickPos14.Visible = false;
                        TextBoxPickPos15.Visible = false;
                        TextBoxPickPos16.Visible = false;

                        // 
                        // Pos1Display
                        // 

                        Pos9Display.Visible = false;
                        Pos10Display.Visible = false;
                        Pos11Display.Visible = false;
                        Pos12Display.Visible = false;
                        Pos13Display.Visible = false;
                        Pos14Display.Visible = false;
                        Pos15Display.Visible = false;
                        Pos16Display.Visible = false;

                        //-----------------Induction Screen ----------------------
                        // 
                        // LabelPos1
                        // 

                        LabelPos9.Visible = false;
                        LabelPos10.Visible = false;
                        LabelPos11.Visible = false;
                        LabelPos12.Visible = false;
                        LabelPos13.Visible = false;
                        LabelPos14.Visible = false;
                        LabelPos15.Visible = false;
                        LabelPos16.Visible = false;

                        // 
                        // TextBoxPos1
                        // 

                        TextBoxPos9.Visible = false;
                        TextBoxPos10.Visible = false;
                        TextBoxPos11.Visible = false;
                        TextBoxPos12.Visible = false;
                        TextBoxPos13.Visible = false;
                        TextBoxPos14.Visible = false;
                        TextBoxPos15.Visible = false;
                        TextBoxPos16.Visible = false;

                        // 
                        // AvailablePos1Display
                        // 

                        AvailablePos9Display.Visible = false;
                        AvailablePos10Display.Visible = false;
                        AvailablePos11Display.Visible = false;
                        AvailablePos12Display.Visible = false;
                        AvailablePos13Display.Visible = false;
                        AvailablePos14Display.Visible = false;
                        AvailablePos15Display.Visible = false;
                        AvailablePos16Display.Visible = false;

                        break;
                    }
            }
        }

        private void SetLoaderButtonText()
        {
            if (GlobalVar.LoaderRunning)
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StopLoader");
            }
            else
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StartLoader");
            }
        }

        private void SetUploadButtonText()
        {
            if (GlobalVar.UploadRunning)
            {
                MBMainUpload.Text = _resourceManager.GetString($"StopUpload");
            }
            else
            {
                MBMainUpload.Text = _resourceManager.GetString($"StartUpload");
            }
        }

        //IptiButtonPressed Event Handler

        public void IptiButtonPickAccept(ResponseInfo responseInfo)
        {
            _logger.Log($"IptiButtonPickAccept Display Number:  {responseInfo.DisplayNumber}");
            PickAccept();
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StopLoader");
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBMainLoadOrders.Text = _resourceManager.GetString($"StartLoader");
                GlobalVar.LoaderRunning = false;
            }
        }

        private void StartStopUploadAction(string startStop)
        {
            if (startStop == "Start")
            {
                MBMainUpload.Text = _resourceManager.GetString($"StopUpload");
                GlobalVar.UploadRunning = true;
            }
            else
            {
                MBMainUpload.Text = _resourceManager.GetString($"StartUpload");
                GlobalVar.UploadRunning = false;
            }
        }

        private void MBMainLoadOrders_Click(object sender, EventArgs e)
        {
            Mediator.GetInstance().OnStartStopLoader(this, !GlobalVar.LoaderRunning ? "Start" : "Stop");
        }

        private void MBMainUpload_Click(object sender, EventArgs e)
        {
            Mediator.GetInstance().OnStartStopUpload(this, !GlobalVar.UploadRunning ? "Start" : "Stop");
        }

        private void MBRunLoader_Click(object sender, EventArgs e)
        {
            RunLoaderOnce();
        }

        private void MBRunUpload_Click(object sender, EventArgs e)
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

        private void SetupLogger()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = @"PickModule";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
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
            Task.Run(() => _logger.Log($"ShowAllOrders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            var views = _ordersRepository.GetOrderViewNotCompleted(findWhat);
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
            Task.Run(() => _logger.Log($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowRackOrders(int recId = 0)
        {
            Task.Run(() => _logger.Log($"Show Rack Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
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
            Task.Run(() => _logger.Log($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowAvailableOrdersRack(int recId = 0, string findWhat = "")
        {
            Task.Run(() => _logger.Log($"ShowAvailableOrdersRack: [{DateTime.Now.ToLongTimeString()}]"));
            var idx = 0;
            // var station = _stationRepository.GetStationView(8);
            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrdersRack.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetRackOrdersView(_rackStation.StationNumber, findWhat);

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
                Task.Run(() => _logger.Log($"ShowAvailableOrdersRack Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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

            Task.Run(() => _logger.Log($"ShowAvailableOrdersRack End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowAvailableOrders(int recId = 0, string findWhat = "")
        {
            Task.Run(() => _logger.Log($"ShowAvailableOrders: [{DateTime.Now.ToLongTimeString()}]"));
            var idx = 0;
            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetAvailableOrders(_station, findWhat, _neutronVariables.SerialPicking);
                // var views = _ordersRepository.GetAvailableOrders(_station);
                //var views = !string.IsNullOrEmpty(findWhat)
                //    ? _ordersRepository.GetAvailableOrders(_station, findWhat, _neutronVariables.SerialPicking)
                //    : _ordersRepository.GetAvailableOrders(_station);

                _bindingListViewAvailableOrdersViews = new BindingListView<AvailableOrdersView>(views.ToList());
                _bindingSourceAvailableOrders.DataSource = _bindingListViewAvailableOrdersViews;
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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
                CheckMarkSelectedAvailableOrders();
                ClearTextBoxPosBackColor();
                SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();

                _currentAvailableOrdersView = ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;

            }

            Task.Run(() => _logger.Log($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private int ShowAllAvailableOrders(int recId = 0, string findWhat = "")
        {
            Task.Run(() => _logger.Log($"ShowAllAvailableOrders: [{DateTime.Now.ToLongTimeString()}]"));
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
                Task.Run(() => _logger.Log($"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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
                CheckMarkSelectedAvailableOrders();
                SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();

                // _currentAvailableOrdersView = ((ObjectView<OrderView>)_bindingSourceAvailableOrders.Current).Object;

            }

            Task.Run(() => _logger.Log($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        private void CheckMarkSelectedAvailableOrders()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
                {
                    var id = Convert.ToInt32(row.Cells["Id"].Value);
                    if (bp.OrderId != id) continue;
                    var chk = (DataGridViewCheckBoxCell)row.Cells[0];
                    chk.Value = chk.TrueValue;
                    break;
                }
            }
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
            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var bp = _ordersToPick[i];
                if (bp.OrderId != null) continue;
                // Position is available, set it
                SetCurrentTextBoxPos(bp.PositionNumber);
                // Set the return variable to the i/position number
                result = i;
                // Break out of the For loop
                break;
            }
            // return the position
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

        private int GetRecordCount(BindingSource bs)
        {
            var count = bs.Count;
            var records = _resourceManager.GetString($"Records");
            LabelRecordCount.Text = string.Format(format: "{0}: {1}", arg0: records, arg1: count.ToString());
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
            LabelFormTitle.BackColor = Color.RoyalBlue;
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
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "StationNumber"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString($"Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString($"Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString($"Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item"
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _resourceManager.GetString($"OrderStatusName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "OrderStatusName",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _resourceManager.GetString($"Picked"),
                Name = "Picked",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString($"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = _resourceManager.GetString($"Id"),
                Visible = false,
                Name = "Id"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = _resourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = _resourceManager.GetString($"OrderDetailId"),
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
            var result = false;

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
            DataGridViewAvailableOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
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

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Available",
                HeaderText = _resourceManager.GetString($"Available"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Available"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                HeaderText = _resourceManager.GetString($"Picked"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Picked"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Skipped",
                HeaderText = _resourceManager.GetString($"Skipped"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Skipped"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

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
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
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
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
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
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber",
                ReadOnly = true

            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartNum",
                HeaderText = _resourceManager.GetString($"PartNum"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PartNum",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartDesc",
                HeaderText = _resourceManager.GetString($"PartDesc"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "PartDesc",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _resourceManager.GetString($"PickedQuantity"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = false
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _resourceManager.GetString($"LineStatusName"),
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
                HeaderText = _resourceManager.GetString($"OrderId"),
                Visible = false,
                Name = "OrderId"
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
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
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
        private void SetupSkipInventoryGrid(object state)
        {
            DataGridViewSkipInventory.AutoGenerateColumns = false;
            DataGridViewSkipInventory.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewSkipInventory.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewSkipInventory.DefaultCellStyle.BackColor = Color.White;
            DataGridViewSkipInventory.ScrollBars = ScrollBars.Both;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString($"StationNumber"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "StationNumber"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageType",
                HeaderText = _resourceManager.GetString($"StorageType"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "StorageType"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _resourceManager.GetString($"Slot"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Slot"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString($"Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Item"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString($"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString($"Quantity"),
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
                HeaderText = _resourceManager.GetString($"Picked"),
                Name = "Picked",
                ReadOnly = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Required",
                HeaderText = _resourceManager.GetString($"Required"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Required",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InventoryId",
                HeaderText = _resourceManager.GetString($"InventoryId"),
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

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            //var dgv = sender as DataGridView;

            //if (dgv.Columns[e.ColumnIndex].Name.Equals("OrderStatusName"))
            //{
            //    if (e.Value != null)
            //    {
            //        switch (e.Value.ToString().Trim())
            //        {
            //            case "Available":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
            //                break;
            //            case "Hold":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Yellow;
            //                break;
            //            case "Picking":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Blue;
            //                break;
            //            case "Partial":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Orange;
            //                break;
            //            case "Deleted":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.Red;
            //                break;
            //            case "Complete":
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.ForestGreen;
            //                break;
            //            default:
            //                dgv.Rows[e.RowIndex].Cells["IsChecked"].Style.BackColor = Color.White;
            //                break;
            //        }
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
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldOrder, order);
                }
            }
            ShowAllOrders();
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
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseOrder, order);
                }
            }
            ShowAllOrders();
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

        private void MBPickListBack_Click(object sender, EventArgs e)
        {
            PickListBack();
        }

        private void PickListBack()
        {
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = AvailableOrders;
        }

        private void MBBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBAvailableOrdersBack_Click(object sender, EventArgs e)
        {
            // if there are any orders not complete change them back to Available
            AvailableOrdersBack();
        }

        private void AvailableOrdersBack()
        {
            // ClearBatchPositions();
            LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private void MBGo_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void Go()
        {
            Task.Run(() => _logger.Log($"Go Batch START Before FinalCheckOfOrdersToPick"));

            FinalCheckOfOrdersToPick();

            Task.Run(() => _logger.Log($"Go Batch START after FinalCheckOfOrdersToPick"));
            Cursor.Current = Cursors.WaitCursor;
            TextBoxFindAvailableOrders.Text = string.Empty;
            // UpdateOrdersToPickingStatus(_ordersToPick);
            var numOrders = _ordersToPick.Where(o => o.OrderId != null).Count();
            if (numOrders > 0)
            {
                LabelFormTitle.Text = _resourceManager.GetString($"PickList");
                LabelFormTitle.BackColor = Color.RoyalBlue;

                var pickableViews = PickListLoad();

                if (pickableViews.Count > 0)
                {
                    _bindingSourcePickViews.DataSource = pickableViews;
                    DataGridPickView.DataSource = _bindingSourcePickViews;
                    Task.Run(() => _logger.Log($"bindingSourcePickViews Count:{_bindingSourcePickViews.Count.ToString()}"));
                    GetRecordCount(_bindingSourcePickViews);

                    var shortItems = GetShortItems(pickableViews);
                    // Check for Short Items
                    if (shortItems.Count > 0)
                    {
                        tabControl1.SelectedTab = PickList;
                        Task.Run(() => _logger.Log($"Opening Pick List  Short Item Report"));
                    }
                    else
                    {
                        Start();
                    }
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
                    ClearAllSelectOrdersToPick();
                    LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                    LabelFormTitle.BackColor = Color.RoyalBlue;
                    AvailableOrdersScreen();
                }
            }

            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log($"Go Batch START"));
        }

        private void FinalCheckOfOrdersToPick()
        {
            // loop over all TextBoxPosx
            // and check for TextBoxPosx.Text that doesn't match OrdersToPick.Order
            // Clear invalid OrdersToPick
            _logger.Log($" Start FinalCheckOfOrdersToPick");
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"TextBoxPos{pos}", true).First();
                if (c != null)
                {
                    var textBox = ((TextBox)c);
                    var order = textBox.Text.Trim();
                    if (string.IsNullOrEmpty(order))
                    {
                        _logger.Log($"TextBox must be Null or Empty: {order}");
                        // clear the Batch Position
                        ClearItemFromBatchByPosition(bp.PositionNumber);
                    }
                    //else
                    //{
                    //    //var itemFound = _bindingSourceAvailableOrders.Find("Ord1", order);
                    //    //if (itemFound > -1) _bindingSourceAvailableOrders.Position = itemFound;
                    //    //var currentOrder = ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                    //   var currentOrder = _repoOrders.FindBy(r => r.Ord1 == order).FirstOrDefault();
                    //    AddItemToBatchByPosition(currentOrder, pos);
                    //}
                }
            }
        }

        private void AddItemToBatchByPosition(Order order, string pos)
        {
            _logger.Log($"AddItemToBatchByPosition: {pos} Order: {order.Ord1}");
            var position = int.Parse(pos);
            _ordersToPick[position].OrderId = order.Id;
            _ordersToPick[position].Ord1 = order.Ord1;
            _ordersToPick[position].Ord2 = order.Ord2;
            _ordersToPick[position].OrderComplete = false;

        }

        //private void UpdateOrdersToPickingStatus(List<BatchPosition> ordersToPick)
        //{
        //    var orderIds = ordersToPick.Where(r => r.OrderId != null).ToList();

        //    foreach (var batchPosition in orderIds)
        //    {
        //        var orderDetails = _repoOrderDetails.FindBy(r =>
        //            r.OrderId == batchPosition.OrderId && r.StationNumber == _station.StationNumber &&
        //            r.LineStatusId == (int) LineStatus.Available);
        //        foreach (var orderDetail in orderDetails)
        //        {
        //            orderDetail.LineStatusId = (int) LineStatus.Picking;
        //            _repoOrderDetails.Update(orderDetail);
        //        }
        //    }
        //}


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

        private void LoadInventory()
        {
            Task.Run(() => _logger.Log($"Load Inventory START"));
            var pickableLocations = new int[] { 1, 2 };
            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => pickableLocations.Contains(f.StorageTypeId)).ToList();
            Task.Run(() => _logger.Log($"Load Inventory END"));
        }

        private List<PickView> PickListLoad()
        {
            Task.Run(() => _logger.Log($"Pick List Load Start"));
            var pickViews = GetPickViews();
            Task.Run(() => _logger.Log($"Pickviews Count: {pickViews.Count}"));

            var pickableViews = new List<PickView>();
            var skipPickableViews = new List<PickView>();
            var zeroPickableViews = new List<PickView>();
            LoadInventory();
            foreach (var item in pickViews)
            {
                var orderDetail = item.OrderDetail;
                //TODO  commented out because I'm not handling something correctly
                // and items are getting stuck in Pick status
                //orderDetail.LineStatusId = (int)LineStatus.Picking;
                _repoOrderDetails.Update(orderDetail);

                List<Inventory> exactInventorySequence;
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
                else
                {
                    using (var form = new FrmSkipOption(item))
                    {
                        var result = form.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            var pushed = form.Pushed;
                            if (pushed == "Skip")
                            {
                                item.OrderDetail.LineStatusId = (int)LineStatus.Skipped;
                                skipPickableViews.Add(item);
                                _repoOrderDetails.Update(item.OrderDetail);
                                GlobalVar.HistoryManager.SaveHistory(ActionCode.Skip, item.OrderDetail, item.OrderDetail.StationNumber);
                            }
                            else if (pushed == "Pick Zero")
                            {
                                zeroPickableViews.Add(item);
                                item.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                                item.OrderDetail.PickedQuantity = 0;
                                _repoOrderDetails.Update(item.OrderDetail);

                                GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, item.OrderDetail, item.OrderDetail.StationNumber);
                                CheckForOrderComplete(item.OrderDetail.Order);
                            }
                        }
                    }
                }
            }

            CreateSkipZeroSummary(skipPickableViews, zeroPickableViews);

            //if (pickableViews.Count > 0)
            //{
            //    _bindingSourcePickViews.DataSource = pickableViews;
            //    DataGridPickView.DataSource = _bindingSourcePickViews;
            //    Task.Run(() => _logger.Log($"bindingSourcePickViews [{DateTime.Now.ToLongTimeString()}] {Environment.NewLine}Count:{_bindingSourcePickViews.Count.ToString()}"));
            //    GetRecordCount(_bindingSourcePickViews);

            //    // DataGridPickView_FormatRows();

            //    //DataGridPickView.ClearSelection();
            //    //DataGridPickView.Update();
            //    //DataGridPickView.ScrollBars = ScrollBars.Both;
            //}
            //else
            //{
            //    MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
            //    ClearAllSelectOrdersToPick();
            //    LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            //    LabelFormTitle.BackColor = Color.RoyalBlue;
            //    ShowAvailableOrders();
            //    tabControl1.SelectedTab = AvailableOrders;
            //}
            //foreach (var item in pickableViews)
            //{
            //    if (item.QuantityToBePicked > item.TotalQuantityInInventory)
            //    {
            //        itemShort = true;
            //        break;
            //    }
            //}
            Task.Run(() => _logger.Log($"PickListLoad End: [{DateTime.Now.ToLongTimeString()}]"));

            return pickableViews;
        }

        private void CreateSkipZeroSummary(List<PickView> skipPickableViews, List<PickView> zeroPickableViews)
        {
            if (skipPickableViews.Count <= 0 && zeroPickableViews.Count <= 0) return;
            var frm = new FrmSkipZeroSummary(skipPickableViews, zeroPickableViews);
            frm.ShowDialog();
        }

        private List<Inventory> PrimeBinFirst(PickView item)
        {
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            Task.Run(() => _logger.Log($"1487 Prime Bin First Inventory Rec Count:  {recs.Count}"));
            if (recs.Count <= 0) return inventorySequence;
            //if there is a prime bin make it first, remove it from the list of inventory locations
            var prime = recs.FirstOrDefault(r => r.Location.Slot == item.OrderDetail.PrimeBin);
            if (prime != null)
            {
                Task.Run(() => _logger.Log($"recs Add Prime [{DateTime.Now.ToLongTimeString()}] "));
                inventorySequence.Add(prime);
                recs.Remove(prime);
            }
            //sequence the inventory Recs by Received Date
            var sortedRecs = recs.OrderBy(o => o.ReceivedDate);
            inventorySequence.AddRange(sortedRecs);
            return inventorySequence;
        }

        private List<Inventory> PrimeBinLast(PickView item)
        {
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            Task.Run(() => _logger.Log($"1512 Prime Bin Last Inventory Rec Count:  {recs.Count}"));
            if (recs.Any())
            {
                //if there is a prime bin make it first, remove it from the list of inventory locations
                var prime = recs.FirstOrDefault(r => r.Location.Slot == item.OrderDetail.PrimeBin);
                if (prime != null)
                {
                    recs.Remove(prime);
                }
                //sequence the inventory Recs by Received Date Decending
                var sortedRecs = recs.OrderBy(o => o.ReceivedDate).ToList();
                foreach (var inv in sortedRecs)
                {
                    inventorySequence.Add(inv);
                }
                if (prime != null)
                {
                    inventorySequence.Add(prime);
                }
            }
            return inventorySequence;
        }

        private List<Inventory> Fifo(PickView item)
        {
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            Task.Run(() => _logger.Log($"1536 FIFO Inventory Rec Count:  {recs.Count}"));
            if (recs.Count > 0)
            {
                //sequence the inventory Recs by Received Date
                inventorySequence = recs.OrderBy(o => o.ReceivedDate).ToList();
            }
            return inventorySequence;
        }

        private List<Inventory> Lifo(PickView item)
        {
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(item.ItemId);
            Task.Run(() => _logger.Log($"1553 LIFO Inventory Rec Count:  {recs.Count}"));
            if (recs.Count > 0)
            {
                //sequence the inventory Recs by Received Date Decending
                var sortedRecs = recs.OrderByDescending(o => o.ReceivedDate).ToList();
                foreach (var inv in sortedRecs)
                {
                    inventorySequence.Add(inv);
                }
            }
            return inventorySequence;
        }

        private List<PickView> GetPickViews()
        {
            Task.Run(() => _logger.Log($"Get Pick Views START"));
            var prevPartNum = "";

            var pickViews = new List<PickView>();
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var itemFound = _bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                if (itemFound > -1) _bindingSourceAvailableOrders.Position = itemFound;
                var currentOrder = ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                var firstTime = true;
                var counter = 0;
                var orderAndDetails = _ordersRepository.GetOrderAndOrderDetails(bp.OrderId, _station.StationNumber);
                currentOrder.Order = orderAndDetails;
                var details = currentOrder.Order.OrderDetails.OrderBy(o => o.PartNum);
                foreach (var detail in details)
                {
                    //if (detail.LineStatusId != (int)LineStatus.Available &&
                    //    detail.LineStatusId != (int)LineStatus.Skipped) continue;
                    //key builder makes each line of orderdetails unique so that an order with the same item
                    // will be picked separately
                    // PickStops will be grouped by key, not item number

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
                        key = $"detail.PartNum{counter}";
                    }
                    else //prevPartNum != detail.PartNum
                    {
                        prevPartNum = detail.PartNum;
                        key = detail.PartNum;
                        counter = 0;
                    }

                    //detail.ItemDefinition = _repoItemDefinition.FindByKey(detail.ItemDefinitionId);

                    var pickView = new PickView()
                    {
                        PickPosition = bp.PositionNumber,
                        OrderId = detail.Order.Id,
                        Ord1 = detail.Order.Ord1,
                        Ord2 = detail.Order.Ord2,
                        ItemId = detail.ItemDefinitionId,
                        Item = detail.PartNum,
                        Description = detail.PartDesc,
                        UnitOfIssue = detail.ItemDefinition.UnitOfIssue.Name,
                        Quantity = detail.Quantity,
                        QuantityToBePicked = detail.Quantity,
                        PickedQty = detail.PickedQuantity,
                        Slot = string.Empty,
                        SlotQty = 0,
                        OrderDetail = detail,
                        StationNumber = detail.StationNumber,
                        PreviousLineStatusId = detail.LineStatusId,
                        ItemKey = key
                    };
                    pickViews.Add(pickView);
                }
            }

            //Add Item definition
            //foreach (var item in pickViews)
            //{
            //    var def = _repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
            //    if (def != null)
            //    {
            //        item.Item = def.Item;
            //        item.Description = def.Description;
            //    }
            //}
            Task.Run(() => _logger.Log($"GetPickViews End: [{DateTime.Now.ToLongTimeString()}]"));
            return pickViews;
        }

        private int GetBatchPosition(int orderId)
        {
            var result = -1;
            //foreach (var bp in _ordersToPick)
            //{
            var bp = _ordersToPick.Where(o => o.OrderId == orderId).FirstOrDefault();
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
                if (bp.OrderId != null)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }
            return orderIds.ToArray();
        }

        private PickView CreatePickView(int pos, OrderDetail detail)
        {
            Task.Run(() => _logger.Log($"CreatePickView Start: [{DateTime.Now.ToLongTimeString()}]"));
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

            Task.Run(() => _logger.Log($"CreatePickView End: [{DateTime.Now.ToLongTimeString()}]"));
            return pickView;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            Task.Run(() => _logger.Log($"Get Inventory Item: {itemId} START"));
            List<Inventory> recs = new List<Inventory>();
            recs = _currentInventory.FindAll(r => r.ItemDefinitionId == itemId);
            //var pickableLocations = new[] { 1, 2 };

            //recs = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
            //    .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            Task.Run(() => _logger.Log($"Get Inventory Item: {itemId} END"));
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



        //select Available orders to Batch Positions
        private void DataGridViewAvailableOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            _logger.Log($"Grid Click Start Row Index: {e.RowIndex}");
            if (e.RowIndex < 0) return;
            var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0];

            if (chk.Value == chk.TrueValue)
            {
                DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                var id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                _logger.Log($"RemoveItemFromBatch ID: {id}");
                RemoveItemFromBatch(id);
            }
            else
            {
                DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                var id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                if (id <= 0) return;
                var ord1 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                var ord2 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord2"].Value.ToString();
                _logger.Log($"AddItemToBatch ID: {id} Ord1: {ord1}  Ord2: {ord2}");
                var idx = AddItemToBatch(id, ord1, ord2);
                _logger.Log($"AddItemToBatch RETURN");
            }
        }

        private void RemoveItemFromBatch(int orderId)
        {
            var bp = _ordersToPick.Where(o => o.OrderId == orderId).FirstOrDefault();
            if (bp != null)
            {
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                UpdateTextBoxPosition(bp);
            }
        }

        /// <summary>
        /// Add an Order to the Orders To Pick List
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="ord1">Order Number</param>
        /// <param name="ord2">Invoice Number</param>
        /// <returns>Pick Position</returns>


        /// <summary>
        /// Add an Order to the Orders To Pick List
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <param name="ord1">Order Number</param>
        /// <param name="ord2">Invoice Number</param>
        /// <returns>Pick Position</returns>
        private int AddItemToBatch(int orderId, string ord1, string ord2)
        {
            _logger.Log($"START   AddItemToBatch ID: {orderId} Ord1: {ord1}  Ord2: {ord2}");
            // If ManualOverrideCurrentTextBoxPos is true.  That means the TextBox
            // was directly clicked into.
            // If true then run function to set the current batch position
            // If false, set the batch position to the next empty position to the right
            // Both functions have the posibility of returning a -1
            // meaning they could not find or set a current batch position
            var idx = ManualOverrideCurrentTextBoxPos ? SetBatchPositionToManualOverride() : SetBatchPositionToFirstEmpty();

            _logger.Log($"TextBox Position Index: {idx}");
            // Now we have to check for a valid position number
            // and -1 is not one of them
            // Also, the idx must be a valid position in the Order to Pick List
            // The number of positions in Orders to Pick is determined by the
            // PickBatchSize variable in NeutronVariables
            // This is the check to see if it falls in that range
            if (idx >= 0 && idx < _neutronVariables.PickBatchSize)
            {
                _logger.Log($"Index in Range: {idx}");
                // Set the initial Orders to Pick item values
                _ordersToPick[idx].OrderId = orderId;
                _ordersToPick[idx].Ord1 = ord1;
                _ordersToPick[idx].Ord2 = ord2;
                _ordersToPick[idx].OrderComplete = false;
                _logger.Log($" Set the Current TextBox Posx Text to the Order Number");
                _currentTextBoxPos.Text = ord1;
            }
            // Now we can turn off the Manual override
            ManualOverrideCurrentTextBoxPos = false;
            _logger.Log($"  Clear the TextBox Posx BackColor");
            ClearTextBoxPosBackColor();
            _logger.Log($" Back with the idx:{idx} it could be -1");
            return idx;
        }

        private int SetBatchPositionToManualOverride()
        {
            var result = -1;
            result = int.Parse(_currentTextBoxPos.Tag.ToString());
            return result;
        }

        private void ClearAllSelectOrdersToPick()
        {
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var checkedBox = Convert.ToBoolean(row.Cells[0].Value);

                if (checkedBox)
                {
                    var orderId = Convert.ToInt32(row.Cells["Id"].Value);
                    row.Cells[0].Value = false;
                    RemoveItemFromBatch(orderId);
                }
            }
        }

        private void ClearBatchPositions()
        {
            foreach (var bp in _ordersToPick)
            {
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                UpdateTextBoxPosition(bp);

            }
        }

        /// <summary>
        /// Gets the Current TextBox Pos and sets its BackColor to Yellow
        /// </summary>
        /// <param name="batchPositionNumber">Position Number</param>
        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            // Finds the exact TextBoxPosx in the Controls collection
            Control c = Controls.Find($"TextBoxPos{batchPositionNumber}", true).Single() as TextBox;
            // If we found it, set the variable _currentTextBoxPos to it
            if (c != null) _currentTextBoxPos = (TextBox)c;
            // Set the BackColor to Yellow
            _currentTextBoxPos.BackColor = Color.Yellow;
        }

        private void UpdateTextBoxPosition(BatchPosition bp)
        {
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition Start: [{DateTime.Now.ToLongTimeString()}]"));
            var orderNumber = bp.Ord1;
            var pos = bp.PositionNumber;

            Control c = Controls.Find($"TextBoxPos{pos}", true).Single() as TextBox;
            if (c != null) c.Text = orderNumber;
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void UpdateTextBoxPositionQuantity(BatchPosition bp, int quantity)
        {
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition Quantity Start: [{DateTime.Now.ToLongTimeString()}]"));
            var orderNumber = bp.Ord1;
            var pos = bp.PositionNumber;

            Control c = Controls.Find($"TextBoxPos{pos}", true).Single() as TextBox;
            if (c != null) c.Text = quantity.ToString();
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition Quantity End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void InitOrdersToPick(int pickBatchSize)
        {
            _ordersToPick = new List<BatchPosition>();
            for (var i = 0; i < pickBatchSize; i++)
            {
                var bp = new BatchPosition() { PositionNumber = i + 1, OrderId = null, Ord1 = string.Empty, Ord2 = string.Empty, OrderComplete = false };
                _ordersToPick.Add(bp);
                ShowPosition(i + 1);
            }
        }

        private void ShowPosition(int position)
        {
            var font = new Font("Microsoft Sans Serif", 20);
            var pos = position.ToString();

            Control c = Controls.Find("LabelPickPos" + pos, true).Single() as Label;
            if (c != null) c.Visible = true;

            c = Controls.Find("LabelPos" + pos, true).Single() as Label;
            if (c != null) c.Visible = true;

            c = Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
            if (c != null)
            {
                c.Font = font;
                c.Visible = true;
            }

            c = Controls.Find("TextBoxPos" + pos, true).Single() as TextBox;
            if (c != null) c.Visible = true;

            c = Controls.Find("Pos" + pos + "Display", true).Single();
            if (c != null) ((Panel)c).Visible = true;

            c = Controls.Find("AvailablePos" + pos + "Display", true).Single();
            if (c != null) ((Panel)c).Visible = true;
        }

        private void ShowOrdersToPick()
        {
            Task.Run(() => _logger.Log($"ShowOrdersToPick Start: [{DateTime.Now.ToLongTimeString()}]"));
            var font = new Font("Microsoft Sans Serif", 10);
            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var pos = (i + 1).ToString();
                Control c = Controls.Find("TextBoxPickPos" + pos, true).First() as TextBox;
                if (c == null) continue;
                c.Font = font;
                c.Height = 43;
                c.Text = _ordersToPick[i].Ord1 + Environment.NewLine + _ordersToPick[i].Ord2;
            }

            Task.Run(() => _logger.Log($"ShowOrdersToPick Clear All Bli"));

            foreach (var item in _ordersToPick)
            {
                Task.Run(() => _logger.Log($"ShowOrdersToPick Display Pos: {item.PositionNumber} Order: {item.Ord2}"));
                if (!string.IsNullOrEmpty(item.Ord2))
                {
                    var d = item.Ord2.PadLeft(4, ' ');
                    var displayText = d.Substring(d.Length - 4, 4);
                    TurnOnBatchPositionDisplay(item.PositionNumber, beacon: 2, text: displayText);
                }
            }
            Task.Run(() => _logger.Log($"ShowOrdersToPick End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void MBShowOrderOrQuantityToggle_Click(object sender, EventArgs e)
        {
            ShowOrderOrQuantityToggle();
        }

        private void ShowOrderOrQuantityToggle()
        {
            if (MBShowOrderOrQuantityToggle.Text == _resourceManager.GetString($"ShowJobs"))
            {
                ClearAllBli();
                ClearPickPositions();
                ShowOrdersToPick();
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowQuantity");
            }
            else
            {
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
                UpdatePickPosition();
            }
        }

        private void MBPickBack_Click(object sender, EventArgs e)
        {
            PickBack();
        }

        //Back button on Pick Screen
        private void PickBack()
        {
            LabelFormTitle.Text = _resourceManager.GetString($"PickList");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = PickList;
            ClearAllShi();
            ClearAllBli();
            Console.WriteLine("Clear Active Device Indicator  PickBack");
            ClearActiveDeviceIndicators();
            //TODO  commented out because I'm not handling something correctly
            // and items are getting stuck in Pick status
            //UpdateOrdersToAvailableStatus(_ordersToPick);

        }

        //TODO  commented out because I'm not handling something correctly
        // and items are getting stuck in Pick status
        //private void UpdateOrdersToAvailableStatus(List<BatchPosition> ordersToPick)
        //{
        //    var orderIds = ordersToPick.Where(r => r.OrderId != null && r.OrderComplete == false).ToList();
        //    foreach (var batchPosition in orderIds)
        //    {
        //        var orderDetails = _repoOrderDetails.FindBy(r => r.OrderId == batchPosition.OrderId && r.StationNumber == _station.StationNumber && r.LineStatusId == (int)LineStatus.Picking);
        //        foreach (var orderDetail in orderDetails)
        //        {
        //            orderDetail.LineStatusId = (int)LineStatus.Available;
        //            _repoOrderDetails.Update(orderDetail);
        //        }
        //    }
        //}

        private void MBStart_Click(object sender, EventArgs e)
        {
            var pickViews = (IList<PickView>)_bindingSourcePickViews.DataSource;
            if (pickViews == null) return;

            //TODO  commented out because I'm not handling something correctly
            // and items are getting stuck in Pick status
            //foreach (var item in pickViews)
            //{
            //    var orderDetail = item.OrderDetail;
            //    orderDetail.LineStatusId = (int)LineStatus.Picking;
            //    _repoOrderDetails.Update(orderDetail);

            //}
            Start();
        }

        private void Start()
        {
            Console.WriteLine("Init Device Indicators  Start");
            InitDeviceIndicators();
            //-------------------------------------
            Task.Run(() => _logger.Log($"Call Printing Start: [{DateTime.Now.ToLongTimeString()}]"));

            //  PrintAllToteLabels();
            if (_neutronVariables.EnableDocumentPrinter)
            {
                if (_neutronVariables.PrintPackingListStart)
                {
                    PrintAllDocuments();
                }
            }

            Task.Run(() => _logger.Log($"Call Printing End: [{DateTime.Now.ToLongTimeString()}]"));

            Task.Run(() => _logger.Log($"Start_Click Start: [{DateTime.Now.ToLongTimeString()}]"));
            var pickViews = (IList<PickView>)_bindingSourcePickViews.DataSource;
            if (pickViews == null) return;

            pickViews = pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
                  .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
                  .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                  .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();
            //TODO SetOrderStatusToPicking(pickViews);
            Task.Run(() => _logger.Log($"Start_Click 1: [{DateTime.Now.ToLongTimeString()}]"));
            var pickStops = new List<PickStop>();
            IEnumerable<IGrouping<string, PickView>> pickViewGroups = pickViews.GroupBy(r => r.ItemKey).ToList();
            var sequence = 0;
            foreach (var pickViewGroup in pickViewGroups) //for each Item in the group of Items
            {
                var total = 0;
                // a PickStop is of One Item that may be on One to All Pick Positions
                // a PickView is an individual pick at a single Pick Position
                // so a PickStop is has One or Multiple PickViews that are concerned 
                // with picking One Item.
                // a PickStop is a summary of all the PickViews 
                // and some of the information in a PickStop is the same as in a PickView
                // that is why the First PickView is used to provide most of the data to the PickStop
                var firstPickView = pickViewGroup.First();

                var pickStop = new PickStop
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

            Task.Run(() => _logger.Log($"Start_Click 2: [{DateTime.Now.ToLongTimeString()}]"));
            var finalPickSequence = FinalPickSequence(pickStops);
            _bindingSourcePickStops.DataSource = finalPickSequence;
            Task.Run(() => _logger.Log($"Start_Click 3 Run GetFirstStop?: [{DateTime.Now.ToLongTimeString()}]"));
            // GetFirstStop();
            _bindingSourcePickStops.MoveFirst();
            _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
            PrintLabels(_currentPickStop);
            UpdatePickScreen();

            UpdateCurrentDeviceIndicator();
            UpdatePickPosition();
            UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
            UpdateTowerDisplay();

            tabControl1.SelectedTab = PickScreen;

            //feels good to here
            Task.Run(() => _logger.Log($"Start_Click End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private List<PickStop> FinalPickSequence(List<PickStop> pickStops)
        {
            Task.Run(() => _logger.Log($"FinalPickSequence Start: [{DateTime.Now.ToLongTimeString()}]"));
            var newCarList = new List<List<PickStop>>();
            var newList = new List<PickStop>();
            for (var i = 0; i < _station.HardwareDevices.Count; i++)
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
            Task.Run(() => _logger.Log($"FinalPickSequence Start Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            _deviceManager = new PickDeviceManager(newCarList, _neutronVariables.ShuttleEnabled);
            for (var i = 1; i <= _station.HardwareDevices.Count; i++)
            {
                _deviceManager.MoveNext(i);
            }
            Task.Run(() => _logger.Log($"FinalPickSequence End Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            Task.Run(() => _logger.Log($"FinalPickSequence End: [{DateTime.Now.ToLongTimeString()}]"));
            return newList;
        }

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            Task.Run(() => _logger.Log($"4051 PositionDevice"));
            if (_neutronVariables.ShuttleEnabled)
            {
                Task.Run(() => _logger.Log($"4056 PositionDevice"));
                if (GlobalVar.Shuttle != null)
                {
                    _logger.Log($"4061 Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}");

                    Task.Run(() => _logger.Log($"4063 PositionDevice"));
                    GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4);
                }
            }
        }

        private void GetFirstStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetFirstStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                Console.WriteLine("Clear Device Indicator - Get First Stop");

                _bindingSourcePickStops.MoveFirst();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;

                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"4110 GetFirstStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            Task.Run(() => _logger.Log($"GetFirstStop End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void GetNextStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetNextStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence < numberOfStops)
            {
                Console.WriteLine("Clear Device Indicator - Get Next Stop");

                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"4136 GetNextStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);

            }
            Task.Run(() => _logger.Log($"GetNextStop Return: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void GetPreviousStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"Get Prev Stop START"));

            if (_currentPickStop.Sequence > 0)
            {
                _bindingSourcePickStops.MovePrevious();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"4161 GetPreviousStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            Task.Run(() => _logger.Log($"Get Prev Stop END "));
        }

        private void GetLastStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetLastStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                _bindingSourcePickStops.MoveLast();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                UpdateTowerDisplay();
                _logger.Log($"4187 GetLastStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            Task.Run(() => _logger.Log($"GetLastStop Return: [{DateTime.Now.ToLongTimeString()}]"));
        }
        
        private void ButtonStopMoveFirst_Click(object sender, EventArgs e)
        {
            GetFirstStop(moveDevice: true);
        }

        private void ButtonStopMovePrevious_Click(object sender, EventArgs e)
        {
            GetPreviousStop(moveDevice: true);
        }

        private void ButtonStopMoveNext_Click(object sender, EventArgs e)
        {
            GetNextStop(moveDevice: true);
        }

        private void ButtonStopMoveLast_Click(object sender, EventArgs e)
        {
            GetLastStop(moveDevice: true);
        }

        private void UpdatePickScreen()
        {
            Task.Run(() => _logger.Log($"UpdatePickScreen Start: [{DateTime.Now.ToLongTimeString()}]"));
            MBPickNewItem.Visible = _neutronLicense.CompanyCode == "TOP" ? true : false;

            if (_neutronVariables.UseImages) PictureBoxItemImage.LoadAsync(_imageManager.GetImageFile(_currentPickStop.Item));
            LabelFormTitle.Text = _resourceManager.GetString($"Selection");
            LabelPickDescription.Text = _currentPickStop.Description;
            LabelPickItemNumber.Text = _currentPickStop.Item;
            LabelPickUOI.Text = _currentPickStop.UnitOfIssue;
            LabelLineOfLines.Text = string.Format("{0} of {1}"
                , (_currentPickStop.Sequence).ToString(), _bindingSourcePickStops.Count);
            TextBoxRequestedQty.Text = _currentPickStop.Quantity.ToString();

            var pickedSoFar = GetPickedSoFar(_currentPickStop.PickViews);
            TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            LabelPickQty.Text = (_currentPickStop.QuantityToBePicked).ToString();
            Task.Run(() => _logger.Log($"UpdatePickScreen End: [{DateTime.Now.ToLongTimeString()}]"));


        }

        private void UpdatePickScreenAfterChangeQuantity()
        {
            Task.Run(() => _logger.Log($"UpdatePickScreen AfterChangeQuantity Start: [{DateTime.Now.ToLongTimeString()}]"));
            UpdateTowerDisplay();
            UpdatePickPosition();
            LabelPickQty.Text = (_currentPickStop.QuantityToBePicked).ToString();
            Task.Run(() => _logger.Log($"UpdatePickScreen AfterChangeQuantity End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void UpdateTowerDisplay()
        {
            string text = string.Empty;
            var item = _currentPickStop.Item;
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            if (_neutronLicense.CompanyCode == @"MET")
            {
                text = GetDisplayText();
            }
            else
            {
                text = _currentPickStop.QuantityToBePicked.ToString();
            }

            ShowShi(loc1, loc2, loc3, loc4, text);
        }

        // Creates the text string that will show up on the tower displays
        // It uses the quantity and item number to create it
        private string GetDisplayText()
        {
            string result = string.Empty;
            // Get the quantity
            var quantity = _currentPickStop.QuantityToBePicked.ToString();
            if (quantity.Length > 3) return quantity;
            // Get the first 2 chars of the Item number
            var start = _currentPickStop.Item.Length - 2;
            var twoChars = _currentPickStop.Item.Substring(start, 2);
            // Pad the Quantity with spaces to the right
            var paddedQuantity = quantity.PadRight(4, ' ');
            // Return the 6 character result
            result = $"{paddedQuantity}{twoChars}";
            return result;
        }

        private void UpdateCurrentDeviceIndicator()
        {
            Task.Run(() => _logger.Log($"Update Current Device Indicator START"));
            ClearActiveDeviceIndicators();
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            _deviceIndicators[loc1].BlinkOn();
            _deviceIndicators[loc1].Active = true;
            Task.Run(() => _logger.Log($"Update Current Device Indicator: {loc1} END"));
        }

        private void ClearActiveDeviceIndicators()
        {
            Task.Run(() => _logger.Log($"Clear Active Device Indicators START"));
            var devices = _deviceIndicators.Where(x => x.Value.Active == true).ToList();
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in devices)
            {
                deviceIndicator.Value.BlinkOff();
                deviceIndicator.Value.Active = false;
                Task.Run(() => _logger.Log($"Blink Off: {deviceIndicator.Value.DeviceNumber}"));
            }
            Task.Run(() => _logger.Log($"Clear Active Device Indicators END"));
        }

        private void ClearAllDeviceIndicators()
        {
            Task.Run(() => _logger.Log($"Clear All Device Indicators START"));
            foreach (KeyValuePair<int, DeviceIndicator> deviceIndicator in _deviceIndicators)
            {
                deviceIndicator.Value.Active = false;
                deviceIndicator.Value.BlinkOff();
            }
            Task.Run(() => _logger.Log($"Clear All Device Indicators END"));
        }

        //private void UpdateInventoryLocation()
        //{
        //    Task.Run(() => _logger.Log($"UpdateInventoryLocation Start : [{DateTime.Now.ToLongTimeString()}]"));

        //    TextBoxPickLoc1.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
        //    TextBoxPickLoc2.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
        //    TextBoxPickLoc3.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
        //    TextBoxPickLoc4.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
        //    TextBoxPickLoc5.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc5.ToString();
        //    LabelLocationNumber.Text = $"{_currentPickStop.InventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
        //    //LabelLocationNumber.Text = string.Format(format: "{0} of {1}"
        //    //    , arg0: _currentPickStop.InventoryIndex + 1, arg1: _currentPickStop.Inventory.Count);
        //    var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
        //    var qty = GetCurrentInventoryLocationQuantity(inventoryId);
        //    TextBoxLocationQuantity.Text = qty.ToString();
        //    _currentPickStop.CurrentInventoryLocation.Quantity = qty;
        //    var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
        //    _currentPickStop.TotalQuantityInInventory = total;
        //    TextBoxTotalQuantity.Text = total.ToString();

        //    //TextBoxLocationQuantity.Text = _currentPickStop.CurrentInventoryLocation.Quantity.ToString();
        //    //TextBoxTotalQuantity.Text = _currentPickStop.TotalQuantityInInventory.ToString();
        //    TextBoxReceivedDate.Text = _currentPickStop.CurrentInventoryLocation.ReceivedDate.ToString("G");
        //    LabelPrimeBin.Visible = _currentPickStop.CurrentInventoryLocation.PrimeBin;
        //    LabelStaticRelease.Text = _currentPickStop.CurrentInventoryLocation.StorageType.Name;

        //    Task.Run(() => _logger.Log($"UpdateInventoryLocation End : [{DateTime.Now.ToLongTimeString()}]"));

        //}

        private void UpdateGroupBoxLocation(Inventory inventory)
        {
            Task.Run(() => _logger.Log($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]"));

            TextBoxPickLoc1.Text = inventory.Location.Loc1.ToString();
            TextBoxPickLoc2.Text = inventory.Location.Loc2.ToString();
            TextBoxPickLoc3.Text = inventory.Location.Loc3.ToString();
            TextBoxPickLoc4.Text = inventory.Location.Loc4.ToString();
            TextBoxPickLoc5.Text = inventory.Location.Loc5.ToString();
            LabelLocationNumber.Text = $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = inventory.PrimeBin;
            LabelStaticRelease.Text = inventory.StorageType.Name;
            Task.Run(() => _logger.Log($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]"));
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
                        Task.Run(() => _logger.Log($"Show SHI function {loc1}-{loc2}-{loc3}-{loc4}{Environment.NewLine}{text}"));

                        GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text);
                    }
                }
            }
        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.ShiEnabled)
                {
                    if (GlobalVar.Displays == null) return;
                    _logger.Log($"ClearAllShi Function");
                    GlobalVar.Displays.ClearAllShi();
                }
            }
        }

        private void ClearAllBli()
        {
            _logger.Log($"Pick Form - Clear All BLI - START");
            if (_neutronVariables.DisplaysEnabled)
            {
                _logger.Log($"Pick Form - Clear All BLI - Displays Enabled");
                if (_neutronVariables.BliEnabled)
                {
                    _logger.Log($"Pick Form - Clear All BLI - BLI Enabled");
                    if (GlobalVar.Displays == null) return;
                    _logger.Log($"Pick Form - Clear All BLI - GlobalVar.Displays NOT NULL");
                    _logger.Log($"Pick Form - Clear All BLI - ClearAllBli Function Call");
                    GlobalVar.Displays.ClearAllBli();
                    _logger.Log($"Pick Form - Clear All BLI - ClearAllBli Function Return");
                    _logger.Log($"Pick Form - Clear All BLI - Clear Order Control Function Call");
                    GlobalVar.Displays.ClearOc(1);
                    _logger.Log($"Pick Form - Clear All BLI - Clear Order Control Function Return");
                }
            }
            _logger.Log($"Pick Form - Clear All BLI - END");
        }

        //private void ClearAllBli() //373
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            Task.Run(() => _logger.Log($"ClearAllBli Function"));
        //            GlobalVar.Displays.ClearAllBli();
        //            GlobalVar.Displays.ClearOc(1);
        //        }
        //    }
        //}

        //private void ClearAllShi() //373
        //{
        //    if (_neutronVariables.DisplaysEnabled)
        //    {
        //        if (GlobalVar.Displays != null)
        //        {
        //            _logger.Log($"ClearAllShi Function");
        //            GlobalVar.Displays.ClearAllShi();
        //        }
        //    }
        //}





        private void UpdatePickPosition()
        {
            Task.Run(() => _logger.Log($"UpdatePickPosition Start : [{DateTime.Now.ToLongTimeString()}]"));
            SetOrderCompleteThisStation();
            ClearPickPositions();
            ClearPickDisplays();
            ClearAllBli();

            if (_neutronVariables.IptiDisplays)
            {
                TurnOnOcDisplay(1, 1, _currentPickStop.Item);
            }

            foreach (var pickView in _currentPickStop.PickViews)
            {
                var pos = pickView.PickPosition;

                Control control = Controls.Find($"TextBoxPickPos{pos}", true).First();
                if (control != null)
                {
                    var textBox = ((TextBox)control);
                    //if (pickView.PickedQty == 0)
                    //{
                    textBox.Text = pickView.QuantityToBePicked.ToString();
                    //}
                    //else
                    //{
                    //    textBox.Text = $"[{pickView.Quantity.ToString()}]";
                    //}

                }
                control = Controls.Find($"LabelPickPos{pos}", true).First();
                if (control != null)
                {
                    var label = ((Label)control);
                    label.BackColor = GetBackColor(pickView.QuantityToBePicked);
                }
                control = Controls.Find($"Pos{pos}Display", true).First();
                if (control != null)
                {
                    var panel = ((Panel)control);
                    panel.BackColor = Color.Red;
                }
                TurnOnBatchPositionDisplay(position: pos, beacon: 2, text: pickView.QuantityToBePicked.ToString());
            }
            Task.Run(() => _logger.Log($"UpdatePickPosition End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void TurnOnBatchPositionDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.BliEnabled)
                {
                    if (GlobalVar.Displays == null) return;
                    GlobalVar.Displays.ShowBli(position, beacon, text);
                }
            }
        }

        private void TurnOnOcDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (_neutronVariables.IptiDisplays)
                {
                    if (GlobalVar.Displays == null) return;
                    GlobalVar.Displays.ShowOc(position, beacon, text);
                }
            }
        }

        private Color GetBackColor(int quantityToBePicked)
        {
            if (quantityToBePicked == 0)
            {
                return Color.Green;
            }
            else
            {
                return Color.RoyalBlue;
            }
        }

        private string GetRemainingToPick(int totalQty, int pickedSoFar)
        {
            var result = totalQty - pickedSoFar;

            if (result == 0)
            {
                return string.Empty;
            }
            else
            {
                return result.ToString();
            }
        }

        private void ClearOrderPositions()
        {
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"TextBoxPos{pos}", true).First();
                if (c != null)
                {
                    var textBox = ((TextBox)c);
                    textBox.Text = string.Empty;
                }
            }
        }

        private void SetOrderCompleteThisStation()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var linesNotComplete = _repoOrderDetails
                    .FindBy(r => r.OrderId == bp.OrderId && r.StationNumber == _station.StationNumber)
                    .Where(r => r.LineStatusId != (int)LineStatus.Complete).ToList();
                if (linesNotComplete.Any()) continue;
                bp.OrderComplete = true;
            }
        }

        private void ClearPickPositions()
        {
            var font = new Font("Microsoft San Serif", 24);
            foreach (var bp in _ordersToPick)
            {
                string pos = bp.PositionNumber.ToString();
                Control c = Controls.Find($"TextBoxPickPos{pos}", true).First();
                if (c != null)
                {
                    var textBox = ((TextBox)c);
                    textBox.Font = font;
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
                    panel.BackColor = bp.OrderComplete ? Color.Green : Color.Transparent;
                }
            }
        }

        private void MBSkipPick_Click(object sender, EventArgs e)
        {
            SkipPick();
        }

        private void SkipPick()
        {
            if (_currentPickStop.PickViews.Count > 1 && _neutronVariables.SpecialBackOrder)
            {
                MBPickAccept.Enabled = false;
                SpecialPickAccept();
                MBPickAccept.Enabled = true;
            }
            else  // only skipping one pickview
            {

                Cursor.Current = Cursors.WaitCursor;
                MBSkipPick.Enabled = false;
                Task.Run(() => _logger.Log($"SkipPick_Click Start : [{DateTime.Now.ToLongTimeString()}]"));

                _currentPickStop.Skipped = true;

                _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1);

                // var pickViewCount = _currentPickStop.PickViews.Count;
                foreach (var pickView in _currentPickStop.PickViews)
                {
                    // Don't skip any pickViews that have a pickedQty
                    // this allows for picking some and skipping the rest.
                    pickView.PickedQty = 0;
                    SetOrderDetailLineStatus(pickView.OrderDetail, (int)LineStatus.Skipped, ActionCode.Skip);
                }

                //----------
                Task.Run(() => _logger.Log($"History Done"));

                var numberOfStops = _bindingSourcePickStops.Count;
                if (_currentPickStop.Sequence < numberOfStops)
                {
                    Console.WriteLine("Clear Active Device Indicator - SkipPick");

                    _bindingSourcePickStops.MoveNext();
                    _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                    UpdatePickScreen();
                    UpdateCurrentDeviceIndicator();
                    UpdatePickPosition();
                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    UpdateTowerDisplay();
                }
                else
                {
                    Task.Run(() => _logger.Log($"Close Batch With Skip"));
                    CloseBatchWithSkip();
                }

                Task.Run(() => _logger.Log($"SkipPick_Click End : [{DateTime.Now.ToLongTimeString()}]"));
                Cursor.Current = Cursors.Default;
                MBSkipPick.Enabled = true;

            }
        }

        private void CloseBatchWithSkip()
        {
            ClearAllShi();
            ClearAllBli();
            ClearOrderPositions();
            ClearBatchPositions();
            ClearAllDeviceIndicators();

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
                AvailableOrdersScreen();
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

        private void MBPickAccept_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log($"PickAccept_Click Start BUTTON"));
            PickAccept();
            MBPickAccept.Focus();
        }

        private void PickAccept()
        {

            _logger.Log($"Pick Accept Run");
            if (InvokeRequired)
            {
                var method = new MethodInvoker(PickAccept);
                Invoke(method);
                return;
            }

            Console.WriteLine("Clear Active Device Indicator - Pick Accept");
            ClearAllDeviceIndicators();
            MBPickAccept.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            //var thisPick = IntegerExtensions.ParseInt(LabelPickQty.Text);

            // var pick = false;
            // in case a hot action or Location Count changes the current inventory
            // let's refresh the currentInventory
            var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            _currentPickStop.CurrentInventoryLocation.Quantity = GetCurrentInventoryLocationQuantity(inventoryId);
            var enoughInventory = _currentPickStop.CurrentInventoryLocation.Quantity >=
                                  _currentPickStop.QuantityToBePicked;

            if (enoughInventory) //there is enough inventory at this location
            {
                _currentPickStop.UpdatePickViews(GlobalVar.User); //good
                _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);
                _currentPickStop.QuantityToBePicked =
                    GetTotalQuantityToBePicked(_currentPickStop.PickViews); // QuantityToBePicked on ALL PickViews

                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                _currentPickStop.TotalQuantityInInventory = total;
                TextBoxTotalQuantity.Text = total.ToString();

                Task.Run(() => _logger.Log($"PickAccept 1 : [{DateTime.Now.ToLongTimeString()}]"));

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
                    

                    //Getting next location on the current device/ the one that was just picked from.
                    Task.Run(() =>
                        _logger.Log($"PickAccept 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]"));

                    UpdateInventoryQuantity(_currentPickStop);

                    Task.Run(() => _logger.Log($"UpdateInventoryQuantity"));

                    Task.Run(() => _logger.Log($"History Done"));

                    _currentPickStop.SetPickViewsComplete(GlobalVar.User, _logger);

                    foreach (var pickView in _currentPickStop.PickViews)
                    {
                        if (CheckForOrderCompleteOnDevice(pickView.OrderDetail.Order))
                        {
                            if (_neutronVariables.PrintPackingListEnd)
                            {
                                PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
                            }
                        }
                    }

                    Task.Run(() =>
                        _logger.Log($"PickAccept Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

                    var numberOfStops = _bindingSourcePickStops.Count;
                    var position = _bindingSourcePickStops.Position;
                    if (_currentPickStop.Sequence < numberOfStops)
                    {

                        //Use the first carousel location for the movenext in case multiple picks are required for stop
                        _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);

                        // if multiple locations were required to complete this stop
                        // the devices will not be where they're supposed to be for the
                        // normal picking process
                        // The device manager knows, based on the stop, where each device should be
                        // The Reset command of the DeviceManager will send a command to each
                        // device to put it in the correct position/location
                        if (_multiLocationStop)
                        {
                            _deviceManager.Reset();
                            _multiLocationStop = false;
                        }
                        _bindingSourcePickStops.MoveNext();
                        _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                        PrintLabels(_currentPickStop);
                        UpdatePickScreen();
                        UpdateCurrentDeviceIndicator();
                        UpdatePickPosition();
                        UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                        UpdateTowerDisplay();
                        
                    }
                    else
                    {
                        Task.Run(() => _logger.Log($"CloseBatch"));
                        CloseBatch();
                    }
                }
                else //PickStop is NOT complete, why?
                {
                    _multiLocationStop = true;
                    _logger.Log($"Pick Stop NOT Complete.  Next Location");
                    var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                    var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                    var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                    var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                    var text = _currentPickStop.QuantityToBePicked.ToString();
                    Task.Run(() => _logger.Log($"Position Device: {loc1} - {loc2} - {loc3} - {loc4} - {text}     "));
                    PositionDevice(loc1, loc2, loc3, loc4, true);

                    UpdatePickScreen();
                    UpdateCurrentDeviceIndicator();
                    UpdatePickPosition();
                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    UpdateTowerDisplay();
                }
            }
            else
            {
                MessageBox.Show(text: _resourceManager.GetString($"PickExceedsInventory"),
                    caption: _resourceManager.GetString($"Inventory"), buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Stop);
            }

            Task.Run(() => _logger.Log($"PickAccept End : [{DateTime.Now.ToLongTimeString()}]"));
            Cursor.Current = Cursors.Default;
            MBPickAccept.Enabled = true;
            MBPickAccept.Focus();

        }

        // SpecialBackorderProcess Form
        private bool PickStopAdjustmentForm(PickView pickView)
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
                    _repoOrderDetails.Update(pickView.OrderDetail);
                    switch (buttonPressed)
                    {
                        case "Highlight":
                            {
                                success = true;
                                GlobalVar.HistoryManager.SaveHistory(ActionCode.Skip, pickView.OrderDetail);
                                break;
                            }
                        case "Backorder":
                            {
                                UpdatePickViewInventoryQuantity(pickView);
                                success = true;
                                break;
                            }
                        case "Accept":
                            {
                                success = true;
                                _currentPickStop.UpdatePickView(pickView, GlobalVar.User);
                                UpdatePickViewInventoryQuantity(pickView);
                                break;
                            }
                    }
                }
            }

            return success;
        }

        private void SpecialPickAccept()
        {
            _logger.Log($"Special Pick Accept Run");
            if (InvokeRequired)
            {
                var method = new MethodInvoker(SpecialPickAccept);
                Invoke(method);
                return;
            }

            foreach (var pickView in _currentPickStop.PickViews)
            {
                //open a form to update the pickview
                if (PickStopAdjustmentForm(pickView) != true) return;
            }

            CompleteSpecialPick();
        }


        private void CompleteSpecialPick()
        {
            Console.WriteLine("Clear Active Device Indicator - Pick Accept");
            ClearAllDeviceIndicators();

            foreach (var pickView in _currentPickStop.PickViews)
            {
                if (CheckForOrderCompleteOnDevice(pickView.OrderDetail.Order))
                {
                    if (_neutronVariables.PrintPackingListEnd)
                    {
                        PrintPackingList(pickView.OrderDetail.Order.Id, pickView.PickPosition.ToString());
                    }
                }
            }

            Task.Run(() => _logger.Log($"PickAccept Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

            var numberOfStops = _bindingSourcePickStops.Count;
            var position = _bindingSourcePickStops.Position;

            if (_currentPickStop.Sequence < numberOfStops)
            {
                //Use the first carousel location for the movenext in case multiple picks are required for stop
                _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);

                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                PrintLabels(_currentPickStop);
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();
            }
            else
            {
                Task.Run(() => _logger.Log($"CloseBatch"));
                CloseBatch();
            }
        }

        private void PrintLabels(PickStop currentPickStop, int reqFunc = 1, int pos = 0)
        {
            foreach (var pickView in currentPickStop.PickViews)
            {
                var upc = _repoAka.GetUpc(pickView.Item);
                var labelDetail = GetLabelDetail(pickView.OrderDetail);
                ToteToPrint.Print(reqFunc, pickView.PickPosition, labelDetail, upc, _labelPrinter);
            }
        }

        private void PrintLabel(int reqFunc, int pos, PickView pickview)
        {
            var upc = _repoAka.GetUpc(pickview.Item);
            var labelDetail = GetLabelDetail(pickview.OrderDetail);
            ToteToPrint.Print(reqFunc, pos, labelDetail, upc, _labelPrinter);
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
                Origin = orderDetail.OrderDetailInfo.Trim()

            };
        }

        private int GetCurrentInventoryLocationQuantity(int inventoryLocationId)
        {
            int result = 0;
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
            bool result = false;
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

        private void UpdateInventoryQuantity(PickStop pickStop)
        {
            var sb = new StringBuilder();
            //currentPickStop.CurrentInventoryLocation.Quantity -= currentPickStop.PickedQty;
            // repoInventory.Update(currentPickStop.CurrentInventoryLocation);
            sb.AppendLine($"Update Inventory Quantity for PickStop Item: {pickStop.Item} - Start");

            foreach (var pickView in pickStop.PickViews)
            {
                UpdatePickViewInventoryQuantity(pickView);
            }
            _logger.Log($"{sb.ToString()}");
        }

        private void UpdatePickViewInventoryQuantity(PickView pickView)
        {
            var sb = new StringBuilder();
            //currentPickStop.CurrentInventoryLocation.Quantity -= currentPickStop.PickedQty;
            // repoInventory.Update(currentPickStop.CurrentInventoryLocation);
            sb.AppendLine($"Update Inventory Quantity for PickView Item: {pickView.Item} - Start");

            sb.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");

            if (pickView.PickLocations.Any())
            {
                foreach (var pickLocation in pickView.PickLocations)
                {
                    sb.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
                    //  pickLocation.Inventory.Quantity -= pickLocation.Quantity;

                    var inventory = _repoInventory.FindByKey(pickLocation.Inventory.Id);
                    if (inventory != null)
                    {
                        sb.AppendLine($"Inventory Qty: {inventory.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
                        inventory.Quantity -= pickLocation.Quantity;

                        _repoInventory.Update(inventory);
                        sb.AppendLine("Update Inventory");
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inventory, pickLocation.Quantity, pickView);
                        sb.AppendLine("Write Pick Order to History");
                    }
                    else
                    {
                        sb.AppendLine($"Inventory is NULL.");
                    }
                }
            }
            else
            {
                // no PickLocations so no inventory
                // use the PickViews first Inventory Location by default
                var inventory = pickView.Inventory[0];
                GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inventory, 0, pickView);
                sb.AppendLine("Write Pick Order of zero quantity to History");
            }

            _logger.Log($"{sb.ToString()}");
        }

        private void CloseBatch()
        {
            ClearAllShi();
            ClearAllBli();
            ClearOrderPositions();
            ClearBatchPositions();
            Console.WriteLine("Clear All Device Indicators - Close Batch");
            ClearAllDeviceIndicators();
            // check for Inventory locations that need to be Released
            using (var db = new NeutronDb())
            {
                var locationIds = new List<int>();
                var invs = db.Inventory.Where(r => r.Quantity == 0 && r.StationId == _station.StationId && r.StorageTypeId == (int)NeutronCore.Enums.StorageType.Release).ToList();
                if (invs.Count > 0)
                {
                    foreach (var inv in invs)
                    {
                        locationIds.Add(inv.LocationId);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inv);
                        db.Inventory.Remove(inv);
                    }

                    db.SaveChanges();
                }

                if (locationIds.Count > 0)
                {
                    foreach (var locationId in locationIds)
                    {
                        // Look for other items in inventory where the location is the same.
                        // Don't want to change InUse to False is there are other items using this location.

                        var item = db.Inventory.Where(r => r.LocationId == locationId).FirstOrDefault();
                        if (item == null)
                        {
                            var location = db.Locations.Find(locationId);
                            if (location != null)
                            {
                                location.InUse = false;
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }

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
                AvailableOrdersScreen();
                // ShowAllOrders();
                //ShowAvailableOrders();
                //tabControl1.SelectedTab = AvailableOrders;
            }
        }

        private void PrintAllDocuments()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;

                //Check to see if it has already been printed, if it has continue without printing.
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == id && r.PickDocument == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintPackingList(id, bp.PositionNumber.ToString());
                var order = _repoOrders.FindByKey(id);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDocument(int batchPosition)
        {
            var bp = _ordersToPick.Where(o => o.PositionNumber == batchPosition).FirstOrDefault();
            if (bp == null) return;
            if (bp.OrderId == null) return;
            var id = bp.OrderId.Value;
            var order = _repoOrders.FindByKey(id);
            var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
            if (printJob == null)
            {
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private List<AnticipatedOut> GetAnticipatedOuts(int stationId)
        {
            var outs = new List<AnticipatedOut>();
            using (var context = new NeutronDb())
            {
                var paramStation = new SqlParameter("@StationId", stationId);

                outs = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOuts @StationId", paramStation).ToList();
            }
            return outs;
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
        //        var stationId = IntegerExtensions.ParseInt(comboBoxValue);
        //        anticipatedOuts = anticipatedOuts.Where(r => r.Station == stationId).ToList();
        //    }
        //    _documentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinter, _neutronVariables.PrintPreview);
        //}

        private void MBPrintPick_Click(object sender, EventArgs e)
        {
            var comboBoxValue = ComboBoxStationNumber.Text;
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
                var stationId = IntegerExtensions.ParseInt(comboBoxValue);
                foreach (var order in orders)
                {
                    PrintPickListByStation(order.Id, stationId);
                }
            }
        }

        private void PrintPackingList(int orderId, string batchPosition = "")
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var packingList = GetPackingList(orderId);
            foreach (var pack in packingList)
            {
                pack.BatchPosition = batchPosition;
            }
            _documentToPrint.PrintPackingList(packingList, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private List<PackingList> GetPackingList(int orderId)
        {
            var outs = new List<PackingList>();
            using (var context = new NeutronDb())
            {
                var paramOrderId = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PackingList>("usp_GetPackingList @ORDERID", new object[] { paramOrderId }).ToList();
            }
            return outs;
        }

        // Rack print - Pick List
        private void MBPrintDocument_Click(object sender, EventArgs e)
        {
            PrintPickList(_rackStation);
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void PrintPickList(Station station)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Count <= 0) return;
            foreach (var order in orders)
            {
                //TODO  commented out because I'm not handling something correctly
                // and items are getting stuck in Pick status

                var orderDetails = order.OrderDetails.Where(r => r.StationNumber == station.StationNumber
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

                PrintPickListByStation(order.Id, station.Id);
            }
        }


        private void PrintPickListAll(int orderId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickList(orderId);
            _documentToPrint.PrintPickList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickList(int orderId)
        {
            var outs = new List<PickList>();
            using (var context = new NeutronDb())
            {
                var paramOrder = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                outs = context.Database.SqlQuery<PickList>("usp_GetPickList @ORDERID", new object[] { paramOrder }).ToList();
            }
            return outs;
        }

        private void PrintPickListByStation(int orderId, int stationId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickListByStation(orderId, stationId);
            _documentToPrint.PrintPickList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickListByStation(int orderId, int stationId)
        {
            var outs = new List<PickList>();
            using (var context = new NeutronDb())
            {
                var paramOrder = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                var paramStation = new SqlParameter(parameterName: "@STATIONID", value: stationId);
                outs = context.Database.SqlQuery<PickList>("usp_GetPickListByStation @ORDERID, @STATIONID", paramOrder, paramStation).ToList();
            }
            return outs;
        }

        private List<AnticipatedOut> GetAnticipatedOuts()
        {
            var outs = new List<AnticipatedOut>();
            using (var context = new NeutronDb())
            {
                outs = context.Database.SqlQuery<AnticipatedOut>("usp_GetAnticipatedOuts").ToList();
            }
            return outs;
        }

        private void PrintDoc(int positionNumber, Order order)
        {
            Task.Run(() => _logger.Log($"Printing Document. {order.Ord1}"));
            if (_neutronVariables.EnableDocumentPrinter)
            {
                Task.Run(() => _documentToPrint.Print(positionNumber, order.Ord1, _documentPrinter, order.Ord2));
            }
        }

        private void PrintAllToteLabels()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
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
            //    if (bp.OrderId == null) continue;

            var bp = _ordersToPick.Where(o => o.PositionNumber == batchPosition).FirstOrDefault();
            if (bp == null) return;
            if (bp.OrderId == null) return;

            var id = bp.OrderId.Value;
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
            Task.Run(() => _logger.Log($"Printing Tote Label. {order.Ord1}"));
            if (_neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(positionNumber, order, _labelPrinter));
            }
        }



        private void MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (var bp in ordersToPick)
            {
                if (!bp.OrderComplete)
                {
                    if (bp.OrderId != null)
                    {
                        var id = bp.OrderId.Value;
                        var order = _repoOrders.FindByKey(id);
                        CheckForOrderComplete(order);
                    }
                }
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

        // this is called when there are multiple locations on the same PickStop
        private bool NextPickingLocation()
        {
            bool result = false;

            var idx = _currentPickStop.InventoryIndex + 1 < _currentPickStop.Inventory.Count
                ? _currentPickStop.InventoryIndex + 1
                : 0;
            if (idx == 0)
            {
                // back to first location so loop thru all locations to see
                // if there is any inventory left
                for (int i = 0; i < _currentPickStop.Inventory.Count; i++)
                {
                    if (_currentPickStop.Inventory[i].Quantity > 0)
                    {
                        _currentPickStop.GroupBoxLocationInventoryIndex = i;
                        var inventory = _currentPickStop.Inventory[i];
                        _currentPickStop.InventoryIndex = i;
                        _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[i];
                        var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                        var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                        _logger.Log($"Get Next Picking Location: {loc1}-{loc2}");
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
                _logger.Log($"Get Next Picking Location: {loc1}-{loc2}");
                result = true;
            }
            return result;
        }

        private void MBPickChangeQuantity_Click(object sender, EventArgs e)
        {
            ChangeQuantity();
        }

        private void ChangeQuantity()
        {
            MBPickChangeQuantity.Enabled = false;
            using (var form = new FrmChangeQuantity(_currentPickStop))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var newQty = form.NewQty;
                    var pos = form.Position;
                    UpdateCurrentPickStopQuantities(pos, newQty);
                }
            }
            MBPickChangeQuantity.Enabled = true;
        }

        private void UpdateCurrentPickStopQuantities(int pos, int newQty)
        {
            var pickView = _currentPickStop.PickViews.Where(p => p.PickPosition == pos).FirstOrDefault();
            if (pickView == null) return;
            if (newQty <= pickView.GetQuantityToBePicked())
            {
                pickView.QuantityToBePicked = newQty;
                _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
                LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
                UpdatePickScreenAfterChangeQuantity();
            }
        }

        private async void ButtonMove_Click(object sender, EventArgs e)
        {
            _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[_currentPickStop.GroupBoxLocationInventoryIndex];
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

            UpdatePickScreen();
            UpdateCurrentDeviceIndicator();
            UpdatePickPosition();
            UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
            UpdateTowerDisplay();
            Task.Run(() => _logger.Log($"5331 PositionDevice Button MOVE"));
            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
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
                        priority = IntegerExtensions.ParseInt((form.NewPriority));
                    }
                }

                foreach (var order in orders)
                {
                    order.Priority = priority;
                    _repoOrders.Update(order);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangePriority, order);
                }
            }
            ShowAllOrders();
        }

        private void MBChangeOrderStatus_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);

            if (orders.Any())
            {
                var ord = orders.FirstOrDefault();
                using (var form = new FrmChangeOrderStatus(ord))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {

                    }
                }
            }
            ShowAllOrders();
        }

        private void MBChangeLineStatus_Click(object sender, EventArgs e)
        {
            OrderDetail line = null;
            var lines = GetSelectedOrderDetails(DataGridViewOrderDetails);

            if (lines.Any())
            {
                line = lines.FirstOrDefault();
                using (var form = new FrmChangeLineStatus(line))
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {

                    }
                }
            }
            ShowOrderDetailsByOrder(line.OrderId);
        }

        private void MBReturnToStock_Click(object sender, EventArgs e)
        {
            var uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger, _rackStation);
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
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderRts, order);
                        }
                        if (_neutronVariables.CreateStoreOrderWithRts)
                        {
                            CreateStoreOrderFromOrderDetailComplete(order);
                        }
                    }
                }
            }
            ShowAllOrders();
        }

        //private void SetOrderStatus(Order order, int status, ActionCode actionCode)
        //{
        //    try
        //    {
        //        order.OrderStatusId = status;
        //        _repoOrders.Update(order);
        //       GlobalVar.HistoryManager .SaveHistory(actionCode, order);
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
                GlobalVar.HistoryManager.SaveHistory(actionCode, detail, detail.StationNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString($"ErrorSettingOrderDetailStatusCode")}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}");
            }
        }

        //private void SetOrderDetailPickedQuantity(OrderDetail detail, int qty)
        //{
        //    try
        //    {
        //        detail.PickedQuantity = qty;
        //        repoOrderDetails.Update(detail);
        //        GlobalVar.HistoryManager .SaveHistory((int)ActionCode.PickOrder, detail);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error setting Order Detail Status Code. " + ex.Message);
        //    }
        //}

        private void MBJobDetails_Click(object sender, EventArgs e)
        {

            var row = DataGridView1.CurrentRow;
            if (row == null || row.Index < 0) return;
            var id = Convert.ToInt32(row.Cells["Id"].Value);
            if (id > 0)
            {
                ShowOrderDetailsByOrder(id);
            }
        }

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
        }

        private void ShowOrderDetailsByOrderAndStation(Order order, int stationNumber)
        {
            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndStation(order.Id, stationNumber);
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
            Cursor.Current = Cursors.WaitCursor;
            Task.Run(() => _logger.Log($"Job Manager Main Screen Start"));
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            MBOffCarousel.Visible = _station.StationTypeId == (int)StationType.Supervisor ||
                                    _station.StationTypeId == (int)StationType.Rack;
            ShowAllOrders();
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBCompress.Visible = false;
            tabControl1.SelectedTab = OrderListing;
            MBShowAvailable.Focus();
            Cursor.Current = Cursors.Default;
        }

        private void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _previousTab = null;
            if (_station.StationType.Id == (int)StationType.Supervisor ||
                _station.StationType.Id == (int)StationType.Rack)
            {
                ShowAvailableRackScreen();
            }
            else
            {
                AvailableOrdersScreen();
            }
            Cursor.Current = Cursors.Default;
        }

        public void AvailableOrdersScreen()
        {
            Task.Run(() => _logger.Log("Available Orders Screen START"));
            Cursor.Current = Cursors.WaitCursor;
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            MBCompress.Visible = false;
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            ClearBatchPositions();
            ClearOrderPositions();
            _showSkipped = false;
            ShowAvailableOrders();
            tabControl1.SelectedTab = AvailableOrders;
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log("Available Orders Screen END"));
        }

        private void MBAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            AllOrdersRack();
        }

        public void AllOrdersRack()
        {
            Cursor.Current = Cursors.WaitCursor;
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            ClearOrderPositions();
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            _showSkipped = true;
            ShowRackOrders();
            // tabControl1.SelectedTab = AvailableRack;
            Cursor.Current = Cursors.Default;
        }

        //Ready
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = _resourceManager.GetString($"NewJob");
            LabelFormTitle.BackColor = Color.RoyalBlue;
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
                LabelFormTitle.BackColor = Color.RoyalBlue;
                tabControl1.SelectedTab = PickScreen;
                _openHotPickFromPickScreen = false;
            }
            else
            {
                LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
                LabelFormTitle.BackColor = Color.RoyalBlue;
                tabControl1.SelectedTab = Main;
            }
        }

        //private void MBHotPickPickBack_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
        //    LabelFormTitle.BackColor = Color.RoyalBlue;
        //    tabControl1.SelectedTab = HotPickToDelete;
        //}

        private void MBNewOrderClose_Click(object sender, EventArgs e)
        {
            // AvailableOrdersScreen();
            tabControl1.SelectedTab = Main;
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            if (_previousTab == null)
            {
                //DataGridViewAvailableOrders.Refresh();
                if (_currentDataSet == CurrentDataSet.Available)
                {
                    ShowAllOrders();
                }
                if (_currentDataSet == CurrentDataSet.Rack)
                {
                    ShowRackOrders();
                }
                if (_currentDataSet == CurrentDataSet.Complete)
                {
                    ShowCompletedOrders();
                }
                LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
                LabelFormTitle.BackColor = Color.RoyalBlue;
            }
            tabControl1.SelectedTab = _previousTab == null ? OrderListing : _previousTab;
        }

        private void MBLocationCount_Click(object sender, EventArgs e)
        {
            LocationCount();
        }

        private void LocationCount()
        {
            var inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            var qty = OpenLocationCountForm(inventoryId);

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
        }

        private int OpenLocationCountForm(int inventoryId)
        {
            var qty = -1;
            using (var form = new FrmLocationCount())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    qty = IntegerExtensions.ParseInt((form.NewQty));
                    LocationCount(inventoryId, qty);
                }
                else
                {

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
            GlobalVar.HistoryManager.SaveHistory(ActionCode.LocationCount, locationCount);
        }

        private void SetCurrentInventoryView(int inventoryId)
        {
            var rec = _bindingSourceHot.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
            var pos = _bindingSourceHot.IndexOf(rec);
            _bindingSourceHot.Position = pos;
            _currentInventoryView = (SqlInventoryView)_bindingSourceHot.Current;
        }

        private void MBShowAvailable_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Available;
            ShowAllOrders();
            MBCompress.Visible = false;
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBKillOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
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

        private void ShowButtons()
        {
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBReturnToStock.Visible = _neutronVariables.UseReturnToStock;
            MBReturnToStockOrderDetail.Visible = _neutronVariables.UseReturnToStock;
            MBDeleteOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBCompress.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBKillLine.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBKillLineSkip.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBKillOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBKillOrderRack.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        }

        //private void HideButtons()
        //{
        //    MBPriority.Visible = false;
        //    MBHold.Visible = false;
        //    MBRelease.Visible = false;
        //    MBReturnToStock.Visible = false;
        //    MBReturnToStockOrderDetail.Visible = false;
        //    MBDeleteOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        //    MBCompress.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        //    MBKillLine.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        //    MBKillLineSkip.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        //    MBKillOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        //    MBKillOrderRack.Visible = _station.StationTypeId == (int)StationType.Supervisor;
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
                LabelNewOrderStationNumber.Text = currentItem.StationNumber.ToString();
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
                StationNumber = LabelNewOrderStationNumber.Text.ParseInt()
                ,
                Item = TextBoxNewOrderItem.Text
                ,
                Description = TextBoxNewOrderDescription.Text
                ,
                Quantity = TextBoxNewOrderQuantity.Text.ParseInt()
            };
            _bindingSourceNewItems.Add(rec);
            ClearNewOrderDetail();
            ButtonAddDetail.Enabled = false;
            TextBoxNewOrderFind.Focus();
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0 && ((NewItemView)_bindingSourceNewItems.Current).Item != null;
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
                    NewItemView view = row.DataBoundItem as NewItemView;
                    if (view == null) continue;
                    var itemDefinitionId = view.ItemDefinitionId;
                    var itemDefinition = _repoItemDefinition.FindByKey(itemDefinitionId);
                    if (itemDefinition == null) continue;
                    var rec = new OrderDetail()
                    {
                        ItemDefinitionId = itemDefinitionId,
                        OrderId = order.Id,
                        Quantity = view.Quantity,
                        StationNumber = view.StationNumber,
                        LineStatusId = (int)LineStatus.Available,
                        DateTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                        EmpId = GlobalVar.User.EmpId,
                        JobNum = TextBoxNewOrderOrd1.Text,
                        PartNum = itemDefinition.Item,
                        PartDesc = itemDefinition.Description,
                        Qty = view.Quantity.ToString(),
                        OrderDetailInfo = string.Empty,
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
                    g.ItemDefinition.Station.StationNumber
                    ,
                    g.ItemDefinition.Item
                    ,
                    g.ItemDefinition.Description
                })
                        .Select(r => new NewItemView()
                        {
                            ItemDefinitionId = r.Key.ItemDefinitionId
                            ,
                            StationNumber = r.Key.StationNumber
                            ,
                            Item = r.Key.Item
                            ,
                            Description = r.Key.Description
                            ,
                            Quantity = r.Sum(t => t.Quantity)
                        })
                    .ToList();
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

        private void MBPickNewItem_Click(object sender, EventArgs e)
        {
            using (var form = new FrmNewItemAuth())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.AuthCode == "topura")
                    {
                        UpdatePickViews();
                        UpdatePickScreen();
                        UpdateCurrentDeviceIndicator();
                        UpdatePickPosition();
                        UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                        UpdateTowerDisplay();
                    }
                }
            }
        }

        private void UpdatePickViews()
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

                    var exactInventorySequence = new List<Inventory>();
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

                    var exactInventorySequence = new List<Inventory>();
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
            var success = false;
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
                    success = true;
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
            ShowAllOrders();
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
        private List<OrderView> GetValidOrdersFromBindingSource(string orderNumber)
        {
            // Convert the AvailableOrders BindingSource to a List 
            var list = _bindingSourceAvailableOrders.List.OfType<OrderView>();

            // Return all the orders where the order number or invoice number
            // equals the passed in orderNumber value
            return list.Where(s => s.Ord1 == orderNumber || s.Ord2 == orderNumber).ToList();
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
            var textBox = (TextBox)sender;
            _currentTextBoxPos = textBox;
            //_gridClickedAvailableOrders = false;
            //textBox.SelectAll();
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
                SendKeys.Send("{Tab}");
            }
        }

        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                Cursor.Current = Cursors.WaitCursor;
                //ShowAvailableOrders();
                var find = TextBoxFindAvailableOrders.Text.Trim();
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

            //var find = TextBoxFindAvailableOrders.Text.Trim();
            //FilterAvailableOrders(find);
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
                _bindingListViewAvailableOrdersViews.ApplyFilter(r => r.Ord1.Contains(find) || r.Ord2.Contains(find));
            }


        }

        private void ButtonClearFindAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            TextBoxFindAvailableOrders.Focus();
            Cursor.Current = Cursors.Default;
        }

        private void MButtonSearch_Click(object sender, EventArgs e)
        {
            SearchDataSet();


            // ShowAllOrders();
        }

        private void SearchDataSet()
        {
            switch (_currentDataSet)
            {
                case CurrentDataSet.Available:
                    {
                        ShowAllOrders();
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
            }

        }

        private void TextBoxFind_KeyDown(object sender, KeyEventArgs e)
        {
            // MessageBox.Show($" TextBoxFind Key code: {e.KeyCode}");
            if (e.KeyCode == Keys.Return)
            {
                SearchDataSet();

                //ShowAllOrders();
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

            //ShowAllOrders();
            TextBoxFind.Focus();
        }


        //private void MBFindItem_Click(object sender, EventArgs e)
        //{
        //    Cursor.Current = Cursors.WaitCursor;
        //    FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    Cursor.Current = Cursors.Default;
        //}

        //private void FindHotRecord(string findWhat = @"")
        //{
        //    string find = _akaRepository.Get(findWhat);
        //    TextBoxFindItem.Text = find;

        //    try
        //    {
        //        //if (string.IsNullOrEmpty(s))
        //        //{
        //        //    bindingSourceHot.DataSource = repoInv.GetInventoryViewAll();
        //        //    DataGridViewInventory.DataSource = bindingSourceHot;
        //        //}
        //        //else
        //        //{
        //        _bindingSourceHot.DataSource = _repoInv.FindInventoryViewsByStation(find, _station.StationId);
        //        //IEnumerable<InventoryView> task = repoInv.GetInventoryViewAll();
        //        //bindingSourceHot.DataSource = task
        //        //    .Where(d => d.Item.ToLower().Contains(s) || d.Description.ToLower().Contains(s) || d.Slot.Contains(s)).ToList();
        //        DataGridViewInventory.DataSource = _bindingSourceHot;
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Hot Find Error: " + ex.Message);
        //    }
        //}

        //private void TextBoxFindItem_KeyDown(object sender, KeyEventArgs e)
        //{
        //    // MessageBox.Show($" TextBoxFindItem Key code: {e.KeyCode}");
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    }
        //    if (e.KeyCode == Keys.Escape)
        //    {
        //        TextBoxFindItem.Text = "";
        //    }
        //}

        //private void ButtonHotPickClear_Click(object sender, EventArgs e)
        //{
        //    TextBoxFindItem.Text = string.Empty;
        //    TextBoxFindItem.Focus();
        //}

        private void MBAvailableOrdersRefresh_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void MBRefresh_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
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
        }

        private void MBReturnToStockOrderDetail_Click(object sender, EventArgs e)
        {
            var orderId = 0;
            var uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger, _rackStation);
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
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderRts, orderDetail);
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
                    ShipperId = 1,
                    ShipMethodId = 1,
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
                            Quantity = IntegerExtensions.ParseInt((detailLine.Qty)),
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
                            StationNumber = rec.Location.Station.StationNumber
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
                        ShipperId = 1,
                        ShipMethodId = 1,
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
                                    Quantity = IntegerExtensions.ParseInt((hostOrder.Qty)),
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
                                    StationNumber = rec.Location.Station.StationNumber
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
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseLine, item);
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
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldLine, item);
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


        private void DataGridPickView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            return;
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}

            //for (int i = 0; i < DataGridPickView.RowCount ; i++)
            //{
            //    var totalQuantityCell = DataGridPickView.Rows[i].Cells["TotalQuantityInInventory"];
            //    var quantityCell = DataGridPickView.Rows[i].Cells["Quantity"];

            //    var totalValue = totalQuantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(totalQuantityCell.Value.ToString());
            //    var quantityValue = quantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(quantityCell.Value.ToString());

            //    if (quantityValue > totalValue)
            //    {
            //        DataGridPickView.Rows[i].DefaultCellStyle.BackColor = Color.Gold;
            //    }
            //    else
            //    {
            //        DataGridPickView.Rows[i].DefaultCellStyle.BackColor = Color.White;
            //    }
            //}

            //var totalQuantityCell = DataGridPickView.Rows[e.RowIndex].Cells["TotalQuantityInInventory"];
            //var quantityCell = DataGridPickView.Rows[e.RowIndex].Cells["Quantity"];

            //var totalValue = totalQuantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(totalQuantityCell.Value.ToString());
            //var quantityValue = quantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(quantityCell.Value.ToString());

            //if (quantityValue > totalValue)
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Gold;
            //}
            //else
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //}
        }

        private void MBPickScreenHotPick_Click(object sender, EventArgs e)
        {
            HotAction();
        }

        private void HotAction()
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                var item = LabelPickItemNumber.Text;
                Hide();
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository, _neutronVariables, _lacProcessor, _imageManager, _itemDefinitionsRepository, item))
                {
                    var result = frm.ShowDialog();
                    Show();
                    _deviceManager.Reset();
                    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
                }
                // UpdatePickViews();
                //  UpdatePickViewsAfterHotAction();
                LoadInventory();
                UpdateInventoryAfterHotAction();
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();


                //var location = _currentPickStop.CurrentInventoryLocation.Location;

                // _logger.Log($"After Reset get _currentPickStop.CurrentInventoryLocation.Location: {location.Slot}");

                // PositionDevice(location.Loc1, location.Loc2, location.Loc3, location.Loc4, true);
            }
        }

        private int ShowCompleted(int recId = 0)
        {
            // var views = _ordersRepository.GetCompletedOrders();
            // var bindingListView = new BindingListView<OrderView>(views.ToList());
            //_bindingSourceCompleted.DataSource = bindingListView;
            //GetRecordCount(_bindingSourceCompleted);
            //DataGridView1.DataSource = _bindingSourceCompleted;
            //DataGridView1.ClearSelection();
            //DataGridView1.Update();

            //-------------------

            Task.Run(() => _logger.Log($"Show Completed Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            var views = _ordersRepository.GetCompletedOrders(findWhat);
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
            Task.Run(() => _logger.Log($"Show Completed Orders End: [{DateTime.Now.ToLongTimeString()}]"));
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
        /// Set the current TextBox with a click
        /// If you click directly in a textboxpos, you override the
        /// automatic get of the next empty textbox
        /// to let the automatic process know to use the manually
        /// clicked textbox, set the flag to true
        /// unset the flag after the automatic check runs
        /// </summary>
        /// <param name="sender">The current <see cref="TextBox"/></param>
        /// <param name="e">Args</param>
        private void TextBoxPos_Click(object sender, EventArgs e)
        {
            // Set the TextBox that was just clicked to the Current TextBox  
            _currentTextBoxPos = sender as TextBox;
            // Clear the backcolor of the TextBox
            ClearTextBoxPosBackColor();

            _logger.Log($"Set _currentTextBoxPos.BackColor: the BackColor of the Current TextBox to Yellow");
            _currentTextBoxPos.BackColor = Color.Yellow;
            // Let the world know that the TextBox was directly clicked in focus
            ManualOverrideCurrentTextBoxPos = true;
        }

        /// <summary>
        /// Clear the BackColor of all the TextBoxPosx TextBoxes
        /// </summary>
        private void ClearTextBoxPosBackColor()
        {
            _logger.Log($"START ClearTextBoxPosBackColor ");
            foreach (var bp in _ordersToPick)
            {

                string pos = bp.PositionNumber.ToString();
                _logger.Log($"Each Position: {pos}");
                Control c = Controls.Find($"TextBoxPos{pos}", true).First();
                if (c != null) c.BackColor = Color.White;
            }
            _logger.Log($"END ClearTextBoxPosBackColor ");
        }

        private void MBFillStarters_Click(object sender, EventArgs e)
        {
            if (DataGridViewAvailableOrders.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
                {
                    var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];
                    var starterValue = row.Cells["Starter"].Value.ToString();
                    if (starterValue == @"S" && Convert.ToBoolean(checkBoxCell.Value) == false)
                    {
                        var id = Convert.ToInt32(row.Cells["Id"].Value);
                        var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                        var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                        var idx = AddItemToBatch(id, ord1, ord2);
                        if (idx == -1)
                        {
                            //no more locations
                            break;
                        }
                        else
                        {
                            row.Cells[0].Value = checkBoxCell.TrueValue;
                        }
                    }
                }
            }
        }

        private void MBFill_Click(object sender, EventArgs e)
        {
            if (DataGridViewAvailableOrders.Rows.Count <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];

                if (Convert.ToBoolean(checkBoxCell.Value) != false) continue;
                var id = Convert.ToInt32(row.Cells["Id"].Value);
                var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                var idx = AddItemToBatch(id, ord1, ord2);
                if (idx == -1)
                {
                    //no more locations
                    break;
                }
                else
                {
                    row.Cells[0].Value = checkBoxCell.TrueValue;
                }
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

        private bool CheckForOrderCompleteOnDevice(Order order)
        {
            var linesNotComplete = _repoOrderDetails.FindBy(r => r.OrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return false;

            order.OrderStatusId = (int)OrderStatus.Complete;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order, _station.StationId);
            _repoOrders.Update(order);
            Mediator.GetInstance().OnOrderComplete(this, order);
            return true;
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

            using (var form = new FrmReprint(_neutronVariables, position))
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
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
        }

        private void ReprintToteLabel(int batchPosition)
        {
            var view = _currentPickStop.PickViews.Where(r => r.PickPosition == batchPosition).FirstOrDefault();
            PrintLabel(2, batchPosition, view);

            //foreach (var bp in _ordersToPick)
            //{
            //    if (bp.PositionNumber != batchPosition) continue;
            //    if (bp.OrderId == null) continue;

            //var bp = _ordersToPick.Where(o => o.PositionNumber == batchPosition).FirstOrDefault();
            //if (bp == null) return;
            //if (bp.OrderId == null) return;

            //var id = bp.OrderId.Value;
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
            var bp = _ordersToPick.Where(o => o.PositionNumber == batchPosition).FirstOrDefault();
            if (bp == null) return;
            if (bp.OrderId == null) return;
            var id = bp.OrderId.Value;
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
            PictureBoxItemImage.Location = new Point(318, 117);
            PictureBoxItemImage.Size = new Size(512, 512);
            PictureBoxItemImage.BringToFront();
        }

        private void PictureBoxItemImage_MouseLeave(object sender, EventArgs e)
        {
            if (!_neutronVariables.AutoEnlargeImage) return;
            var y = GroupBoxLocation.Location.Y;
            PictureBoxItemImage.Location = new Point(398, y);
            PictureBoxItemImage.Size = new Size(256, 256);
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
            MBCompress.Visible = _station.StationType.Id == (int)StationType.Supervisor;

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
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderDetailDelete, orderDetail);
                    }
                    _repoOrders.Delete(order.Id);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderDelete, order);
                }
            }
            ShowAllOrders();
        }

        public void ProcessDataReceived(object sender, IptiController.MySerialDataReceivedEventArgs args)
        {
            _logger.Log($"ProcessDataReceived:  {args.FormText}");
            var t = args.FormText;
            var response = new IptiButtonResponse();
            if (t.Length > 14)
            {
                var cmd = t.Substring(1, 14);
                response = new IptiButtonResponse()
                {
                    BayId = cmd.Substring(0, 2),
                    Command = cmd.Substring(2, 2),
                    DisplayId = cmd.Substring(4, 2),
                    Quantity = cmd.Substring(6, 4),
                    Text = cmd.Substring(10, 4)
                };
                _logger.Log($"Pick Accept in Data Received: {t}");
                _logger.Log("Hitting the PickAccept button from ProcessDataReceived.");
                PickAccept();
            }


        }

        //private void zTextBoxFindAvailableOrdersRack_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return)
        //    {
        //        foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
        //        {
        //            if (row.Cells["Ord1"].Value.ToString().Trim() == TextBoxFindAvailableOrdersRack.Text)
        //            {
        //                var chk = (DataGridViewCheckBoxCell) row.Cells[0];
        //                row.Cells[0].Value = chk.TrueValue;
        //                TextBoxFindAvailableOrdersRack.Text = string.Empty;
        //                TextBoxFindAvailableOrdersRack.Select();
        //                break;
        //            }
        //        }
        //    }
        //}

        private void ButtonPrintAO_Click(object sender, EventArgs e)
        {
            var outs = new List<AnticipatedOut>();
            if (_station.StationType.Id == (int)StationType.Supervisor)
            {
                // print all station if Supervisor Station
                foreach (var pickStation in _pickStations)
                {
                    outs = GetAnticipatedOuts(pickStation.Id);
                    _documentToPrint.PrintAnticipatedOuts(outs, _documentPrinter, _neutronVariables.PrintPreview);
                }
            }
            else
            {
                outs = GetAnticipatedOuts(_station.StationId);
                _documentToPrint.PrintAnticipatedOuts(outs, _documentPrinter, _neutronVariables.PrintPreview);
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

            int stationNumber;
            if (_station.StationType.Id == (int)StationType.Supervisor)
            {
                stationNumber = _rackStation.StationNumber;
            }
            else
            {
                stationNumber = _station.StationNumber;
            }
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Any())
            {
                ShowOrderDetailsByOrderAndStation(orders.First(), stationNumber);
            }
        }

        private void MBAdjustOrderSave_Click(object sender, EventArgs e)
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
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreRack, detail);
                    _repoOrderDetails.Update(detail);
                }
                //Mediator.GetInstance().OnBatchComplete(this);
            }

            CheckForOrderComplete(order);

            ShowAvailableOrdersRack();
            DataGridViewAvailableOrdersRack.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = AvailableRack;
        }

        private void MBAdjustOrderBack_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrdersRack.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = AvailableRack;
        }

        private void MBRackOrderComplete_Click(object sender, EventArgs e)
        {
            int stationNumber;
            if (_station.StationType.Id == (int)StationType.Supervisor)
            {
                stationNumber = _rackStation.StationNumber;
            }
            else
            {
                stationNumber = _station.StationNumber;
            }
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    var detailLinesThisStation = _repoOrderDetails.FindBy(r => r.OrderId == order.Id && r.StationNumber == stationNumber).ToList();
                    if (detailLinesThisStation.Count > 0)
                    {
                        foreach (var detail in detailLinesThisStation)
                        {
                            detail.LineStatusId = (int)LineStatus.Complete;
                            detail.PickedQuantity = detail.Quantity;
                            detail.EmpId = GlobalVar.User.EmpId;
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.PickRack, detail, _rackStation.Id);
                            _repoOrderDetails.Update(detail);
                        }
                        // Mediator.GetInstance().OnBatchComplete(this);
                    }
                    if (CheckForOrderComplete(order))
                    {
                        if (_neutronVariables.PrintPackingListEnd)
                        {
                            PrintPackingList(order.Id);
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
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Skip;
        }

        private void MBBackSkip_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrders.Refresh();
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Available;
            ShowAllOrders();
            MBCompress.Visible = false;
            MBKillOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;


            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
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
                List<InventoryView> inventoryViews;
                var skipInventoryViews = new List<SkipInventoryView>();
                using (var repo = new InventoryRepository())
                {
                    inventoryViews = repo.GetInventoryViewByItem(cur.Item).ToList();
                }

                foreach (var invView in inventoryViews)
                {
                    var skipInventory = new SkipInventoryView();
                    skipInventory.InventoryId = invView.Id;
                    skipInventory.StorageType = invView.StorageTypeName;
                    skipInventory.StationNumber = invView.StationNumber;
                    skipInventory.Quantity = invView.Quantity;
                    skipInventory.Slot = invView.Slot;
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
                LabelFormTitle.BackColor = Color.RoyalBlue;
                tabControl1.SelectedTab = SkipInventory;

            }
        }

        private void MBSkipInventoryBack_Click(object sender, EventArgs e)
        {
            ShowSkipped();
        }



        private void MBPickComplete_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }

            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            currentSkip.Picked = currentSkip.Quantity;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

            currentSkip.Picked = currentSkip.Quantity;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

            var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

            detail.LineStatusId = (int)LineStatus.Complete;
            detail.PickedQuantity = detail.Quantity;
            detail.EmpId = GlobalVar.User.EmpId;
            _repoOrderDetails.Update(detail);

            if (CheckForOrderComplete(detail.Order))
            {
                if (_neutronVariables.PrintPackingListEnd)
                {
                    PrintPackingList(detail.Order.Id);
                }
            }


            ShowSkipped();
        }

        private void MBPickZero_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }
            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            currentSkip.Picked = 0;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

            //currentSkip.Picked = 0;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

            var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

            detail.LineStatusId = (int)LineStatus.Complete;
            detail.PickedQuantity = 0;
            detail.EmpId = GlobalVar.User.EmpId;
            _repoOrderDetails.Update(detail);

            if (CheckForOrderComplete(detail.Order))
            {
                if (_neutronVariables.PrintPackingListEnd)
                {
                    PrintPackingList(detail.Order.Id);
                }
            }
            ShowSkipped();
        }


        private void MBAdjustQuantity_Click(object sender, EventArgs e)
        {
            if (_bindingSourceSkipView.Current == null)
            {
                MessageBox.Show(_resourceManager.GetString($"NoJobsSelected"));
                return;
            }


            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            using (var form = new FrmChangeQuantityOnly())
            {
                form.NewQty = currentSkip.Quantity;
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var newQty = form.NewQty;
                    currentSkip.Picked = newQty;
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

                    //currentSkip.Picked = newQty;
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

                    // Mediator.GetInstance().OnBatchComplete(this);

                    var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

                    detail.LineStatusId = (int)LineStatus.Complete;
                    detail.PickedQuantity = newQty;
                    detail.EmpId = GlobalVar.User.EmpId;
                    _repoOrderDetails.Update(detail);

                    if (CheckForOrderComplete(detail.Order))
                    {
                        if (_neutronVariables.PrintPackingListEnd)
                        {
                            PrintPackingList(detail.Order.Id);
                        }
                    }
                    ShowSkipped();
                }
            }

        }

        private void MBCompress_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CompressOrders();
            Cursor.Current = Cursors.Default;
        }

        private void MBShowRackOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Rack;
            ShowRackOrders();
            MBCompress.Visible = false;
            MBKillOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            Cursor.Current = Cursors.Default;
        }

        private void MBShortPick_Click(object sender, EventArgs e)
        {
            ShortPick();
        }

        private void ShortPick()
        {
            if (_currentPickStop.PickViews.Count > 1 && _neutronVariables.SpecialBackOrder)
            {
                MBPickAccept.Enabled = false;
                SpecialPickAccept();
                MBPickAccept.Enabled = true;
            }
            else
            {
                var pickView = _currentPickStop.PickViews.FirstOrDefault();

                // get the total of all picklocations before clearing picklocations
                // use that total for the OrderDetail Update
                var pickViewTotal = _currentPickStop.GetPickViewTotal(pickView);

                ////write any picklocations to history
                UpdatePickViewInventoryQuantity(pickView);

                pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                pickView.OrderDetail.PickedQuantity = pickViewTotal;
                _repoOrderDetails.Update(pickView.OrderDetail);

                // function to finish the move and update the lights for the next pick
                CompleteSpecialPick();
            }
        }

        private void MBOffCarousel_Click(object sender, EventArgs e)
        {
            ShowAvailableRackScreen();
        }

        private void ShowAvailableRackScreen()
        {
            ShowAvailableOrdersRack();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = AvailableRack;
        }

        private void MBRackHotAction_Click(object sender, EventArgs e)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions]) return;
            var station = _stationRepository.GetStationView(8);
            Hide();
            using (MetroForm frm = new FrmHotAction(station, _jsonData, _akaRepository
                , _neutronVariables, _lacProcessor, _imageManager, _itemDefinitionsRepository))
            {
                var result = frm.ShowDialog();
                Show();
                //   Task.Run(() => _deviceManager.Reset());
                //    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
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
                //Main Panel
                LabelFormHeaderText.Text = _resourceManager.GetString($"LabelFormHeaderText");
                LabelFormTitle.Text = _resourceManager.GetString($"LabelFormTitle");
                MBMainClose.Text = _resourceManager.GetString($"MBMainClose");
                MBMainAvailableOrders.Text = _resourceManager.GetString($"MBMainAvailableOrders");
                MBMainOrderManager.Text = _resourceManager.GetString($"MBMainOrderManager");
                MBMainNewOrder.Text = _resourceManager.GetString($"MBMainNewOrder");
                MBMainLoadOrders.Text = _resourceManager.GetString($"MBMainLoadOrders");
                MBMainUpload.Text = _resourceManager.GetString($"MBMainUpload");
                MBRunLoader.Text = _resourceManager.GetString($"MBRunLoader");
                MBRunUpload.Text = _resourceManager.GetString($"MBRunUpload");
                //Order Listing Panel
                MBSkipped.Text = _resourceManager.GetString($"MBSkipped");
                MBShowAvailable.Text = _resourceManager.GetString($"MBShowAvailable");
                MBCompleted.Text = _resourceManager.GetString($"MBCompleted");
                MBShowRackOrders.Text = _resourceManager.GetString($"MBShowRackOrders");
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
                MBFillOptimized.Text = _resourceManager.GetString("MBFillOptimized");
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
            Task.Run(() => _logger.Log($"Reset After Reset Carousel Button Pushed : [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void ComboBoxStationNumber_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void LabelPos16_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos15_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos10_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos11_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos12_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos13_Click(object sender, EventArgs e)
        {

        }

        private void LabelPos14_Click(object sender, EventArgs e)
        {

        }

        private void FrmPick_KeyDown(object sender, KeyEventArgs e)
        {
            switch (tabControl1.SelectedTab.Name)
            {
                case "PickScreen":
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                {
                                    _logger.Log($"FrmPick_KeyDown: ENTER Key");
                                    e.Handled = true;
                                    //PickAccept();
                                    break;
                                }
                            case Keys.Space:
                                {
                                    _logger.Log($"FrmPick_KeyDown: SPACE Key");
                                    PickAccept();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.L:
                                {
                                    _logger.Log($"FrmPick_KeyDown: L  Key");
                                    LocationCount();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.A:
                                {
                                    _logger.Log($"FrmPick_KeyDown: A  Key");
                                    HotAction();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.S:
                                {
                                    _logger.Log($"FrmPick_KeyDown: S  Key");
                                    ShowOrderOrQuantityToggle();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.Q:
                                {
                                    _logger.Log($"FrmPick_KeyDown: Q  Key");
                                    ChangeQuantity();
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.B:
                                {
                                    _logger.Log($"FrmPick_KeyDown: B  Key");
                                    ShortPick();
                                    //e.Handled = true;
                                    break;
                                }
                            case Keys.H:
                                {
                                    _logger.Log($"FrmPick_KeyDown: H  Key");
                                    SkipPick();
                                    //e.Handled = true;
                                    break;
                                }
                            case Keys.F2:
                                {
                                    _logger.Log($"FrmPick_KeyDown: F2  Key");
                                    PrintLabels(_currentPickStop, 2);
                                    e.Handled = true;
                                    break;
                                }
                            case Keys.OemQuestion:
                                _logger.Log($"FrmPick_KeyDown: OEM QUESTION Key");
                                ShowShortCutForm();
                                e.Handled = true;
                                break;
                        }

                        break;
                    }
                case "AvailableOrders":
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                {
                                    //_logger.Log($"FrmPick_KeyDown: ENTER Key");
                                    //e.Handled = true;
                                    //Go();
                                    break;
                                }
                        }
                        break;
                    }
                    if (e.KeyCode == Keys.F12)
                    {
                        using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
                        {
                            var result = frm.ShowDialog();
                            Show();
                        }
                    }
                    //if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
                    //{
                    //    if (tabControl1.SelectedTab.Name == "PickScreen")
                    //    {
                    //        PickAccept();
                    //    }

                    //}

                    if (e.KeyCode == Keys.Escape)
                    {
                        if (tabControl1.SelectedTab.Name == "PickScreen")
                        {
                            PickBack();
                        }

                        if (tabControl1.SelectedTab.Name == "PickList")
                        {
                            PickListBack();
                        }

                        if (tabControl1.SelectedTab.Name == "AvailableOrders")
                        {
                            AvailableOrdersBack();
                        }
                    }

            }
        }

        private void FrmPick_KeyPress(object sender, KeyPressEventArgs e)
        {
            //e.KeyC Keys.A || Keys.
            // if (e.KeyChar >= 48 && e.KeyChar <= 57)
            // {
            //     MessageBox.Show("Form.KeyPress: '" +
            //                     e.KeyChar.ToString() + "' pressed.");

            //     switch (e.KeyChar)
            //     {
            //         case (char)49:
            //         case (char)52:
            //         case (char)55:
            //             MessageBox.Show("Form.KeyPress: '" +
            //                             e.KeyChar.ToString() + "' consumed.");
            //             e.Handled = true;
            //             break;
            //     }
            // }
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

        private void MBFillOptimized_Click(object sender, EventArgs e)
        {
            if (DataGridViewAvailableOrders.Rows.Count <= 0) return;
            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                var checkBoxCell = (DataGridViewCheckBoxCell)row.Cells[0];

                if (Convert.ToBoolean(checkBoxCell.Value) != false) continue;
                var id = Convert.ToInt32(row.Cells["Id"].Value);
                var ord1 = Convert.ToString(row.Cells["Ord1"].Value);
                var ord2 = Convert.ToString(row.Cells["Ord2"].Value);
                var idx = AddItemToBatch(id, ord1, ord2);
                if (idx == -1)
                {
                    //no more locations
                    break;
                }
                else
                {
                    row.Cells[0].Value = checkBoxCell.TrueValue;
                }
            }
        }

        private void DataGridPickView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridPickView.ClearSelection();
            DataGridPickView_FormatRows();
        }

        private void MBKillOrder_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridView1);
            if (orders.Any()) KillOrder(orders);
            if (_currentDataSet == CurrentDataSet.Available)
            {
                ShowAllOrders();
            }
            if (_currentDataSet == CurrentDataSet.Rack)
            {
                ShowRackOrders();
            }
        }

        private void KillOrder(List<Order> orders)
        {
            try
            {
                foreach (var order in orders)
                {
                    if (order != null)
                    {
                        if (order.OrderStatusId == (int)OrderStatus.Available)
                        {
                            KillLine(order.OrderDetails);
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.KillOrder, order, _station.StationId);
                            CheckForOrderComplete(order);
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

        private void KillLine(ICollection<OrderDetail> orderDetails)
        {
            try
            {
                foreach (var orderDetail in orderDetails)
                {
                    var order = _repoOrders.FindByKey(orderDetail.OrderId);
                    if ((orderDetail.LineStatusId == (int)LineStatus.Available || orderDetail.LineStatusId == (int)LineStatus.Skipped)
                        && order.OrderStatusId == (int)OrderStatus.Available)
                    {
                        var stationId = _stationRepository.GetStationId(orderDetail.StationNumber);
                        orderDetail.PickedQuantity = 0;
                        orderDetail.LineStatusId = (int)LineStatus.Complete;
                        _repoOrderDetails.Update(orderDetail);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.KillLine, orderDetail, stationId);
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

        private void MBKillLine_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewOrderDetails);
            if (orderDetails.Any()) KillLine(orderDetails);
            ShowOrderDetailsByOrder(_currentJobDetailsOrderId);
        }

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
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Any()) KillOrder(orders);
            ShowAvailableOrdersRack();
        }

        private void MBKillLineSkip_Click(object sender, EventArgs e)
        {
            var orderDetails = GetSelectedOrderDetails(DataGridViewSkip);
            if (orderDetails.Any()) KillLine(orderDetails);
            ShowSkipped();
        }

        private bool CheckForOrderComplete(Order order)
        {
            var linesNotComplete = _repoOrderDetails.FindBy(r => r.OrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return false;

            order.OrderStatusId = (int)OrderStatus.Complete;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order, _station.StationId);
            _repoOrders.Update(order);
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
            TextBoxPos1.Focus();
        }

        private void ClearItemFromBatchByPosition(int position)
        {
            _logger.Log($" Start ClearItemFromBatchByPosition: {position}");
            var bp = _ordersToPick.Where(o => o.PositionNumber == position).FirstOrDefault();
            if (bp != null)
            {
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                UpdateTextBoxPosition(bp);
                _logger.Log($" Start ClearItemFromBatchByPosition: {position} After UpdateTextBoxPosition");
            }
        }

        #region TextBoxPosLeave

        private void TextBoxPosLeave(object sender, EventArgs e)
        {

            var textBox = ((TextBox)sender);
            var orderNumber = textBox.Text;
            var position = IntegerExtensions.ParseInt(textBox.Tag.ToString());

            if (string.IsNullOrEmpty(orderNumber)) return;
            if (ValidateOrderAndPosition(position, orderNumber)) return;
            textBox.SelectAll();
            textBox.Focus();
        }

        private bool ValidateOrderAndPosition(int position, string orderNumber)
        {
            var orders = GetValidOrdersFromBindingSource(orderNumber);
            //ordersToPick 
            if (orders != null)
            {
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
                    var idValue = IntegerExtensions.ParseInt(row.Cells["Id"].Value.ToString());
                    var ord1 = row.Cells["Ord1"].Value.ToString();
                    var ord2 = row.Cells["Ord2"].Value.ToString();
                    var bp = _ordersToPick.FirstOrDefault(r => r.OrderId == idValue);

                    if (bp == null)
                    {
                        //not in a Batch POsition
                        //position is good
                        chk.Value = chk.TrueValue;
                        AddItemToBatch(idValue, ord1, ord2);
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        //private void TextBoxPos1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    return;
        //    // Determine the TextBox that fired the TextBoxLeave Event
        //    var textBox = ((TextBox)sender);
        //    var position = IntegerExtensions.ParseInt(textBox.Tag.ToString());
        //    // Get the value in the Text field
        //    var orderNumber = textBox.Text;


        //    if (string.IsNullOrEmpty(orderNumber))
        //    {
        //        var errorMsg = "No order number entered.";
        //        e.Cancel = true;
        //        textBox.Select(0, textBox.Text.Length);
        //        ClearItemFromBatchByPosition(position);
        //        // Set the ErrorProvider error with the text to display. 
        //        this.ErrorProvider1.SetError(TextBoxErrorProvider, errorMsg);
        //    }
        //    else
        //    {

        //        if (!ValidOrderAndPosition(position, orderNumber))
        //        {
        //            e.Cancel = true;
        //            textBox.Select(0, textBox.Text.Length);
        //            ClearItemFromBatchByPosition(position);
        //        }
        //    }



        //    // Get the Position number of the TextBox
        //    // All TextBoxes have a Tag property that is Preloaded
        //    // at Design time with the Position Number
        //    // var position = IntegerExtensions.ParseInt(textBox.Tag.ToString());

        //    // If there is nothing in the TextBox.Text field, just return;

        //}

        //private bool ValidOrderAndPosition(int position, string orderNumber)
        //{
        //    var orders = GetValidOrdersFromBindingSource(orderNumber);
        //    if (orders.Count == 0)
        //    {
        //        ErrorProvider1.SetError(TextBoxErrorProvider, "No Order Found");
        //        return false;
        //    }
        //    else if (orders.Count > 1)
        //    {
        //        ErrorProvider1.SetError(TextBoxErrorProvider, "Multiple Orders Found");
        //        return false;
        //    }
        //    else if (orders.Count == 1)
        //    {
        //        var order = orders.FirstOrDefault();
        //        AddOrderToBatch(order, position);
        //        return true;
        //    }

        //    return false;
        //}

        //private void TextBoxPos1_Validated(object sender, EventArgs e)
        //{
        //    // If all conditions have been met, clear the ErrorProvider of errors.
        //    ErrorProvider1.SetError(TextBoxErrorProvider, "");
        //}


        //private void AddOrderToBatch(OrderView order, int position)
        //{
        //    // If ManualOverrideCurrentTextBoxPos is true.  That means the TextBox
        //    // was directly clicked into.
        //    // If true then run function to set the current batch position
        //    // If false, set the batch position to the next empty position to the right
        //    // Both functions have the posibility of returning a -1
        //    // meaning they could not find or set a current batch position
        //    //var idx = ManualOverrideCurrentTextBoxPos ? SetBatchPositionToManualOverride() : SetBatchPositionToFirstEmpty();

        //    // Now we have to check for a valid position number
        //    // and -1 is not one of them
        //    // Also, the idx must be a valid position in the Order to Pick List
        //    // The number of positions in Orders to Pick is determined by the
        //    // PickBatchSize variable in NeutronVariables
        //    // This is the check to see if it falls in that range
        //    if (position >= 0 && position <= _neutronVariables.PickBatchSize)
        //    {
        //        // Set the initial Orders to Pick item values
        //        _ordersToPick[position].OrderId = order.Id;
        //        _ordersToPick[position].Ord1 = order.Ord1;
        //        _ordersToPick[position].Ord2 = order.Ord2;
        //        _ordersToPick[position].OrderComplete = false;
        //        // Set the Current TextBox Posx Text to the Order Number
        //        _currentTextBoxPos.Text = order.Ord1;
        //    }
        //    // Now we can turn off the Manual override
        //    //ManualOverrideCurrentTextBoxPos = false;
        //    // Clear the TextBox Posx BackColor
        //    ClearTextBoxPosBackColor();
        //    // Return the Position and it could be -1
        //    //return idx;
        //}

        #region TextBoxPosLeave

        ///// <summary>
        ///// All TextBoxes on the Induction screen use the same TextBoxPosLeave Event to
        ///// validate the value scanned or entered into the <see cref="TextBox"/>
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void TextBoxPosLeave(object sender, EventArgs e)
        //{
        //    //// Determine the TextBox that fired the TextBoxLeave Event
        //    //var textBox = ((TextBox)sender);
        //    //// Get the value in the Text field
        //    //var orderNumber = textBox.Text;
        //    //// Get the Position number of the TextBox
        //    //// All TextBoxes have a Tag property that is Preloaded
        //    //// at Design time with the Position Number
        //    //var position = IntegerExtensions.ParseInt(textBox.Tag.ToString());

        //    //// If there is nothing in the TextBox.Text field, just return;
        //    //if (string.IsNullOrEmpty(orderNumber))
        //    //{
        //    //    //Clear the Orders in Batch Position
        //    //   // ClearItemFromBatchByPosition(position);
        //    //    return;
        //    //}
        //    //// If there is something in the TextBox, see if it's a valid order
        //    //// to be picked on this station.
        //    //// If it is NOT, just return
        //    //if (ValidateOrderAndPosition(position, orderNumber)) return;
        //    //// If the order is NOT valid
        //    //// Select All the Text
        //    //textBox.SelectAll();
        //    //// Set the focus on the same TextBox
        //    //textBox.Focus();
        //}



        //private bool ValidateOrderAndPosition(int position, string orderNumber)
        //{
        //    // It's possible to have multiple orders with the same order number
        //    // Get a list of all of them
        //    var orders = GetValidOrdersFromBindingSource(orderNumber);
        //    int idx = -1;
        //    // Orders will NEVER be null, but that's what we're checking for and it works 
        //    // that means this code ALWAYS runs and
        //    // that makes it an unnecessary check, but we'll leave it for now
        //    // 
        //    if (orders != null)
        //    {
        //        // Create a variable to hold a List of DataGridViewRows
        //        var rowsWithThisOrderNumber = new List<DataGridViewRow>();

        //        // why am I looping over the Grid to get a list of DataGridViewRows?
        //        // Isn't the _bindingSourceAvailableOrders the DataSource for this Grid?
        //        // anyway, here we go
        //        foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
        //        {
        //            // Get the two field representing the Order and Invoice numbers
        //            var ord1 = (row.Cells["Ord1"].Value).ToString();
        //            var ord2 = (row.Cells["Ord2"].Value).ToString();
        //            // Check the passed in orderNumber against either order or invoice
        //            // If it matches either field add it to t he list Grid Rows
        //            if (orderNumber.Trim() == ord1.Trim() || orderNumber.Trim() == ord2.Trim())
        //            {
        //                // Add this Grid Row to DataGridViewRow variable
        //                rowsWithThisOrderNumber.Add(row);
        //            }
        //        }
        //        // Now that we have our list of Grid Rows copied from the DataGridViewAvailableOrders Grid
        //        // That means each row has all the same columns as DataGridViewAvailableOrders
        //        // Let's loop thru them and gather some additional information
        //        foreach (var row in rowsWithThisOrderNumber)
        //        {
        //            // Get the value of the CheckBox Column Name is "IsChecked"
        //            var chk = (DataGridViewCheckBoxCell)row.Cells[0];
        //            // Get the Order Id
        //            var idValue = IntegerExtensions.ParseInt(row.Cells["Id"].Value.ToString());
        //            // Get the Order Number
        //            var ord1 = row.Cells["Ord1"].Value.ToString();
        //            // Get the Invoice Number
        //            var ord2 = row.Cells["Ord2"].Value.ToString();

        //            // Check to see if this order is already in the Orders to Pick List
        //            var bp = _ordersToPick.FirstOrDefault(r => r.OrderId == idValue);

        //            // If it's not in the Orders to Pick List
        //            if (bp == null)
        //            {
        //                // The passed in position is not being used

        //                // Set the CheckBox value to Checked
        //                // Not sure why, we have stopped using Checkboxes but won't
        //                // change it until we know we don't use it of need it
        //                chk.Value = chk.TrueValue;
        //                // Add the Order information to a batch
        //                // If the function returns a -1, it means it failed to 
        //                // location an open position.
        //                // This shouldn't happen in this function because the user clicked
        //                // in the TextBox Posx
        //                idx = AddItemToBatch(idValue, ord1, ord2);
        //            }
        //        }
        //    }
        //    // If idx is -1 the function returns false, else it returns true
        //    return idx != -1;
        //}
        #endregion

    }
}
