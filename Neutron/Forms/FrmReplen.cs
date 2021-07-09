using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MetroFramework.Forms;
using Neutron.Global;
using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronCore.Extensions;
using System.Threading.Tasks;
using System.Threading;
using Neutron.Interfaces;
using System.Diagnostics;
using System.Globalization;
using JsonManager;
using Neutron.Enums;
using Neutron.Classes;
using Equin.ApplicationFramework;
using EnumsNET;
using System.Resources;
using NeutronLoader;
using NeutronCore.Global;
using NeutronCore;
using AlliedLogger;
using System.Text;
using CurrentDeviceIndicator;
using NeutronData.Interfaces;
using NeutronCore.Models;
using NeutronCore.Enums;
using NeutronDllu;
using Neutron.Controllers;
using NeutronEvents;
using Remotion.FunctionalProgramming;
using StorageType = NeutronData.Models.Lookups.StorageType;

namespace Neutron.Forms
{
    public partial class FrmReplen : MetroForm
    {
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        private ResourceManager _gridResourceManager;
        //private GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        //private StationRepository _repoStation = new StationRepository();
        //private GenericRepository<History> repoHistory = new GenericRepository<History>(new NeutronDb());
        // private InventoryRepository repoInv = new InventoryRepository();
        // private GenericRepository<Order> repoOrders = new GenericRepository<Order>(new NeutronDb());
        // private GenericRepository<OrderDetail> repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        //private BindingListView<ReplenOrderView> _bindingSourceOrderViewEquin;
        //private BindingListView<ReplenOrderView> bindingSourceAvailableOrdersEquin;
        //public bool CloseForm = false;
        //private SqlInventoryView currentInventoryView = new SqlInventoryView();
        //private string textToFind = string.Empty;

        private readonly GenericRepository<Inventory>
            _repoInventory = new GenericRepository<Inventory>(new NeutronDb());

        // private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocationRepository =
            new GenericRepository<Location>(new NeutronDb());

        private readonly GenericRepository<ItemDefinition> _repoItemDefinition =
            new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly GenericRepository<StorageType> _repoStorageTypes =
            new GenericRepository<StorageType>(new NeutronDb());

        private readonly GenericRepository<ReplenOrder> _repoReplenOrder =
            new GenericRepository<ReplenOrder>(new NeutronDb());

        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails =
            new GenericRepository<ReplenOrderDetail>(new NeutronDb());

        private readonly IReplenOrdersRepository _replenOrdersRepository;
        private readonly ReplenOrderDetailsRepository _orderDetailsRepository = new ReplenOrderDetailsRepository();
        private readonly GenericRepository<Station> _repoStation = new GenericRepository<Station>(new NeutronDb());
        private readonly IStationRepository _stationRepository;
        private readonly ItemDefinitionsRepository _itemDefinitionsRepository = new ItemDefinitionsRepository();
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

        private TextBox _currentTextBoxPos;
        private bool _manualOverrideCurrentTextBoxPos;
        private List<BatchPosition> _ordersToPick = new List<BatchPosition>();
        private ReplenPickStop _currentPickStop = new ReplenPickStop();

        //private bool openHotPickFromPickScreen = false;
        //private bool openHotStoreFromPickScreen = false;
        //private InterfaceProcessorTmg _interfaceProcessorTmg;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private DynamicLogger _logger;
        private string _imagesDirectory;
        private ReplenDeviceManager _deviceManager;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;
        private DocumentToPrint _documentToPrint;
        private readonly IJsonData _jsonData;
        private readonly StationView _station;
        private readonly IAkaRepository _akaRepository;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;
        private readonly IImageManager _imageManager;
        private StorageType _defaultStorageType;

        private CurrentDataSet _currentDataSet;
        private List<Inventory> _currentInventory;
        private int _numberOfInventoryLocations = 5;
        private List<Location> _tempAllocatedLocations;

        private Dictionary<int, DeviceIndicator> _deviceIndicators;
        private string _activeGrid = "Available";
        private readonly int[] _moveableDeviceTypes;
        private readonly Station _rackStation;
        private readonly List<Station> _pickStations;

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



        public FrmReplen(IJsonData jsonData, StationView station
            , IAkaRepository akaRepository, NeutronVariables neutronVariables
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IStationRepository stationRepository
            , IReplenOrdersRepository replenOrdersRepository, NeutronLicense neutronLicense)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            _station = station;
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _lacProcessor = lacProcessor;
            _imageManager = imageManager;
            _replenOrdersRepository = replenOrdersRepository;
            _stationRepository = stationRepository;
            _akaRepository = akaRepository;
            _securityProcessor = securityProcessor;
            _rackStation = _stationRepository.GetRackStation();
            _moveableDeviceTypes = _stationRepository.GetMoveableDeviceTypeIds();
            _pickStations = _stationRepository.GetPickStations();
            _currentInventory = new List<Inventory>();
            InitForm();
            //SetupPrinters();
            //_synchronizationContext = SynchronizationContext.Current;            
            //InitGrids();
            //InitOrdersToPick(neutronVariables.StoreBatchSize);

            //
            //_rackStation = _stationRepository.GetRackStation();
            //_moveableDeviceTypes = _stationRepository.GetMoveableDeviceTypeIds();

            //LoadInventory();
            //_documentToPrint = new DocumentToPrint();
            //ShowButtons();
            //HideTabControlTabs();
            //mlUserInfo.Text = GlobalVar.User?.UserInfo;
            //CloseButtonPressed = false;
            //_currentTextBoxPos = TextBoxPos1;

            //InitDataGridViewNewItems();
            //_imagesDirectory = LoaderSettings.GetImagesDirectory();
            //_defaultStorageType = new NeutronData.Models.Lookups.StorageType { Id = 2, Name = "Release", Sequence = 20 };
        }

        private void InitForm()
        {
            KeyPreview = true;
            SetupLogger();
            _logger.Log($"Form Replen Company Code: {_neutronLicense.CompanyCode}");
            // No Loader Control from Replen
            //if (_station.StationType.Id == (int)StationType.Supervisor)
            //{
            //    MBMainLoadOrders.Visible = true;
            //    MBMainUpload.Visible = true;
            //    MBRunLoader.Visible = true;
            //    MBRunUpload.Visible = true;
            //}
            SetupPrinters();
            _synchronizationContext = SynchronizationContext.Current;
            InitGrids();

            SetupPickPositions(_neutronVariables.StoreBatchSize);
            InitOrdersToPick(_neutronVariables.StoreBatchSize);
            HideTabControlTabs();
            ShowButtons();
            //SetLoaderButtonText(); no loader on replen form
            //SetUploadButtonText(); no loader on replen form
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
            InitDataGridViewNewItems();
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
            MBPickScreenHotPick.Enabled = _securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions];
            if (_station.StationType.Id == (int)StationType.Supervisor ||
                _station.StationType.Id == (int)StationType.Rack)
            {
                MBMainAvailableOrders.Text = _resourceManager.GetString($"OffCarousel");
            }

            _defaultStorageType = _repoStorageTypes.FindByKey(_neutronVariables.DefaultStorageTypeId);
            _tempAllocatedLocations = new List<Location>();

            Mediator.GetInstance().IptiButtonPressed += (s, e) => IptiButtonPickAccept(e.ResponseInfo);
            // Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            // Mediator.GetInstance().StartStopUpload += (s, e) => StartStopUploadAction(e.StartStop);
            Mediator.GetInstance().OrderComplete += (s, e) => ShowOrderComplete(e.Order);
        }

        private void ShowOrderComplete(Order order)
        {
            Task.Run(() => _logger.Log($"Show Order Complete Event: Order Number _ {order.Ord1} -- {order.Ord2}"));
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

        public void IptiButtonPickAccept(ResponseInfo responseInfo)
        {
            _logger.Log($"IptiButtonPickAccept Display Number:  {responseInfo.DisplayNumber}");
            StoreAccept();
        }

        private void SetupPickPositions(int pickBatchSize)
        {
            return;
            switch (pickBatchSize)
            {
                case 6:
                    {
                        // 
                        // LabelPickPos1
                        // 
                        LabelPickPos1.BackColor = Color.RoyalBlue;
                        LabelPickPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPickPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPickPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPickPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPickPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPickPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPickPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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

                        // 
                        // TextBoxPickPos1
                        // 
                        TextBoxPickPos1.BackColor = SystemColors.ButtonHighlight;
                        TextBoxPickPos1.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPickPos2.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPickPos3.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPickPos4.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPickPos5.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPickPos6.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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

                        //-----------------Induction Screen ----------------------
                        // 
                        // LabelPos1
                        // 
                        LabelPos1.BackColor = Color.RoyalBlue;
                        LabelPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        LabelPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        LabelPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        // 
                        // TextBoxPos1
                        // 
                        TextBoxPos1.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
                        TextBoxPos1.Location = new Point(28, 109);
                        TextBoxPos1.Name = "TextBoxPos1";
                        TextBoxPos1.Size = new Size(134, 44);
                        TextBoxPos1.TabIndex = 0;
                        TextBoxPos1.Tag = "0";
                        TextBoxPos1.TextAlign = HorizontalAlignment.Center;
                        TextBoxPos1.Click += new EventHandler(TextBoxPos_Click);
                        TextBoxPos1.Enter += new EventHandler(TextBoxEnter);
                        TextBoxPos1.KeyDown += new KeyEventHandler(TextBoxPosKeyDown);
                        TextBoxPos1.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos2
                        // 
                        TextBoxPos2.BackColor = SystemColors.Control;
                        TextBoxPos2.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPos2.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos3
                        // 
                        TextBoxPos3.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPos3.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos4
                        // 
                        TextBoxPos4.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPos4.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos5
                        // 
                        TextBoxPos5.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPos5.Leave += new EventHandler(TextBoxPosLeave);
                        // 
                        // TextBoxPos6
                        // 
                        TextBoxPos6.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Bold, GraphicsUnit.Point,
                            ((byte)(0)));
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
                        TextBoxPos6.Leave += new EventHandler(TextBoxPosLeave);
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
                        break;
                    }
            }
        }

        private void SetupLogger()
        {
            string logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = @"ReplenModule";
            string logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        private void SetupPrinters()
        {
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _labelPrinter = _jsonData.LoadFile<LabelPrinterPreferences>();
        }

        private void FrmReplen_Load(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log($"Not Loading ShowAllOrders on INIT FrmReplen"));
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
            uiContext.Post(setupGrid, null);
        }

        private void InitDeviceIndicators()
        {
            Task.Run(() => _logger.Log($"Init Device Indicators START"));
            if (PickScreen.Controls.ContainsKey("PanelDeviceIndicators")) return;

            Console.WriteLine("Initialize Device Indicators - InitDeviceIndicators");
            _deviceIndicators = new Dictionary<int, DeviceIndicator>();

            var hardwareDevices = _station.HardwareDevices.Where(x => _moveableDeviceTypes.Contains(x.DeviceTypeId))
                .ToList();
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
            Task.Run(() => _logger.Log($"Init Device Indicators END"));
        }


        public void ProcessDataReceived(object sender, IptiController.MySerialDataReceivedEventArgs args)
        {
            _logger.Log($"Process Data Received:  {args.FormText} START");
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
                _logger.Log($"Store Accept in Data Received: {t}");
                _logger.Log("Hitting the Store Accept button from ProcessDataReceived.");
                StoreAccept();
            }
            _logger.Log($"Process Data Received:  {args.FormText} END");
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


            // if (_neutronVariables.SerialPicking) //Show the Starter column
            // {
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

            // }

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

            var adjust = _gridResourceManager.GetString("Adjust");
            var bCol = new DataGridViewButtonColumn
            {
                HeaderText = _gridResourceManager.GetString(""),
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
                DataPropertyName = "Station",
                HeaderText = _resourceManager.GetString($"Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station",
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

        #endregion

        // Set the focus to the passed in recId if it's passed in
        private int ShowAllOrders(int recId = 0)
        {
            Task.Run(() =>
                _logger.Log($"ShowAllOrders Replen Start: [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = _akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            var views = _replenOrdersRepository.GetOrderViewNotCompleted(findWhat);
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

            Task.Run(() => _logger.Log($"ShowAllOrders End: [{DateTime.Now.ToLongTimeString()}]"));
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

        private int ShowAvailableOrders(int recId = 0, string findWhat = "")
        {
            Task.Run(() => _logger.Log("Show Available Orders START"));
            var idx = 0;
            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = !string.IsNullOrEmpty(findWhat)
                    ? _replenOrdersRepository.GetAvailableOrders(_station, findWhat, _neutronVariables.SerialPicking)
                    : _replenOrdersRepository.GetAvailableOrders(_station);

                _bindingListViewAvailableOrdersViews = new BindingListView<AvailableReplenOrdersView>(views.ToList());
                _bindingSourceAvailableOrders.DataSource = _bindingListViewAvailableOrdersViews;

            }
            catch (Exception ex)
            {
                Task.Run(() =>
                    _logger.Log(
                        $"ShowAvailableOrders Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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

                CheckMarkSelectedAvailableOrders();
                ClearTextBoxPosBackColor();
                SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();

                _currentAvailableOrdersView =
                    ((ObjectView<AvailableReplenOrdersView>)_bindingSourceAvailableOrders.Current).Object;

            }

            Task.Run(() => _logger.Log($"ShowAvailableOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        //private int ShowAvailableOrders(int recId = 0)
        //{
        //    Task.Run(() => _logger.Log($"ShowAvailableOrders Replen: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    var idx = 0;

        //    var findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();

        //    try
        //    {
        //        var views = replenOrdersRepository.GetAvailableOrders(_station, findWhat, _neutronVariables.SerialPicking);

        //        var bindingListView = new BindingListView<AvailableReplenOrdersView>(views.ToList());

        //        bindingSourceAvailableOrders.DataSource = bindingListView;
        //    }
        //    catch (Exception ex)
        //    {
        //        Task.Run(() => _logger.Log($"ShowAvailableOrders Replen Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]"));
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

        //    Task.Run(() => _logger.Log($"ShowAvailableOrders Replen End: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    return idx;
        //}

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

        private int SetBatchPositionToFirstEmpty()
        {
            var result = -1;

            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var bp = _ordersToPick[i];
                if (bp.OrderId != null) continue;
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

        private int GetRecordCount(BindingSource bs)
        {
            int count = bs.Count;
            LabelRecordCount.Text = string.Format("Records: {0}", count.ToString());
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
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldOrder, order);
                    }
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
                    if (order.OrderStatusId == (int)OrderStatus.Hold)
                    {
                        order.OrderStatusId = (int)OrderStatus.Available;
                        _repoReplenOrder.Update(order);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseOrder, order);
                    }
                }
            }

            ShowAllOrders();
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
            LabelFormTitle.Text = "Available Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = AvailableOrders;
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

        private void MBBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = OrderListing;
        }

        private void MbNewClose_Click_1(object sender, EventArgs e)
        {

        }

        private void MBAvailableOrdersBack_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
            ClearBatchPositions();
            LabelFormTitle.Text = "Jobs";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = Main;
        }

        private void MBGo_Click(object sender, EventArgs e)
        {
            Task.Run(() => _logger.Log($"Go Batch START"));
            Cursor.Current = Cursors.WaitCursor;
            TextBoxFindAvailableOrders.Text = string.Empty;
            var numOrders = _ordersToPick.Count(o => o.OrderId != null);
            if (numOrders > 0)
            {
                LabelFormTitle.Text = _resourceManager.GetString($"PickList");
                LabelFormTitle.BackColor = Color.Green;

                var pickableViews = PickListLoad();

                if (pickableViews.Count > 0)
                {
                    _bindingSourcePickViews.DataSource = pickableViews;
                    DataGridPickView.DataSource = _bindingSourcePickViews;
                    Task.Run(() => _logger.Log($"Binding Source Pick Views Count:{_bindingSourcePickViews.Count.ToString()}"));
                    GetRecordCount(_bindingSourcePickViews);

                    Start();
                }
                else
                {
                    MessageBox.Show(_resourceManager.GetString($"NothingtoPick"));
                    ClearAllSelectOrdersToPick();
                    LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
                    LabelFormTitle.BackColor = Color.Green;
                    AvailableOrdersScreen();
                }
            }

            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log($"Go Batch END"));
        }

        private void Start()
        {
            Task.Run(() => _logger.Log($"Start START"));
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

            var pickViews = (IList<ReplenPickView>)_bindingSourcePickViews.DataSource;
            if (pickViews == null) return;

            pickViews = pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
                 .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
                 .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                 .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();

            //TODO SetOrderStatusToPicking(pickViews);
            Task.Run(() => _logger.Log($"Start_Click 1: [{DateTime.Now.ToLongTimeString()}]"));
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

            Task.Run(() => _logger.Log($"Start_Click 2: [{DateTime.Now.ToLongTimeString()}]"));
            var finalPickSequence = FinalPickSequence(pickStops);
            _bindingSourcePickStops.DataSource = finalPickSequence;
            Task.Run(() => _logger.Log($"Start_Click 3 Run GetFirstStop?: [{DateTime.Now.ToLongTimeString()}]"));
            // GetFirstStop();
            _bindingSourcePickStops.MoveFirst();
            _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
            UpdatePickScreen();

            UpdateCurrentDeviceIndicator();
            UpdatePickPosition();
            UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
            UpdateTowerDisplay();

            tabControl1.SelectedTab = PickScreen;

            //feels good to here
            Task.Run(() => _logger.Log($"Start_Click End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private List<ReplenPickView> PickListLoad()
        {
            Task.Run(() => _logger.Log($"Pick List Load START"));
            var pickViews = GetPickViews();
            Task.Run(() => _logger.Log($"Pickviews Count: {pickViews.Count}"));

            var pickableViews = new List<ReplenPickView>();
            LoadInventory();
            foreach (var pickView in pickViews)
            {
                var exactInventorySequence = new List<Inventory>();

                exactInventorySequence = _neutronVariables.UsePrimeBin ? PrimeBinFirst(pickView) : Fifo(pickView);

                var neededLocations = 2 - exactInventorySequence.Count;
                if (neededLocations > 0)
                {
                    var additionInventoryLocations =
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
                    pickableViews.Add(pickView);
                }
                else
                {
                    MessageBox.Show($"There are no locations for item: {pickView.Item}{Environment.NewLine} " +
                                    $"{pickView.Description}.  This item will not be included in this batch.");
                }
            }

            Task.Run(() => _logger.Log($"PickListLoad End: [{DateTime.Now.ToLongTimeString()}]"));
            return pickableViews;
        }

        private List<Inventory> PrimeBinFirst(ReplenPickView pickView)
        {
            Task.Run(() => _logger.Log($"Prime Bin First Inventory START"));
            var inventorySequence = new List<Inventory>();

            var recs = GetInventory(pickView.ItemId);

            if (!recs.Any()) return inventorySequence;
            //if there is a prime bin make it first, remove it from the list of inventory locations
            if (recs.Count == 1)
            {
                inventorySequence = recs;
            }
            else if (recs.Count > 1)
            {
                var prime = recs.FirstOrDefault(r => r.Location.Slot == pickView.OrderDetail.PrimeBin);
                if (prime != null)
                {
                    inventorySequence.Add(prime);
                    recs.Remove(prime);
                }

                //sequence the inventory Recs by Received Date
                var sortedRecs = recs.OrderBy(o => o.ReceivedDate);
                inventorySequence.AddRange(sortedRecs);
            }
            Task.Run(() => _logger.Log($"Prime Bin First Inventory END"));
            return inventorySequence;
        }

        private List<Inventory> Fifo(ReplenPickView pickView)
        {
            Task.Run(() => _logger.Log($"1536 START FIFO Inventory "));
            var inventorySequence = new List<Inventory>();
            var recs = GetInventory(pickView.ItemId);
            if (!recs.Any()) return inventorySequence;
            if (recs.Count == 1)
                inventorySequence = recs;
            else if (recs.Count > 1)
                inventorySequence = recs.OrderBy(o => o.ReceivedDate).ToList();
            else
                inventorySequence = inventorySequence;

            return inventorySequence;
        }

        private List<ReplenPickView> GetPickViews()
        {
            Task.Run(() => _logger.Log($"Get Pick Views START"));
            var prevPartNum = "";

            var pickViews = new List<ReplenPickView>();
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var itemFound = _bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                if (itemFound > -1) _bindingSourceAvailableOrders.Position = itemFound;
                var currentOrder = ((ObjectView<AvailableReplenOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                var firstTime = true;
                var counter = 0;
                var orderAndDetails = _replenOrdersRepository.GetOrderAndOrderDetails(bp.OrderId, _station.StationNumber);
                currentOrder.Order = orderAndDetails;
                var details = currentOrder.Order.ReplenOrderDetails.OrderBy(o => o.PartNum);
                foreach (var detail in details)
                {
                    //already checked the line status in the Repository
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

                    var pickView = new ReplenPickView()
                    {
                        PickPosition = bp.PositionNumber,
                        OrderId = currentOrder.Id,
                        Ord1 = currentOrder.Ord1,
                        Ord2 = currentOrder.Ord2,
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
                        ItemKey = key
                    };
                    pickViews.Add(pickView);
                }
            }
            Task.Run(() => _logger.Log($"Get Pick Views END"));
            return pickViews;
        }

        private int GetBatchPosition(int orderId)
        {
            var result = -1;
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId != orderId) continue;
                result = bp.PositionNumber;
                break;
            }

            return result;
        }

        private int[] GetOrderIdArray(List<BatchPosition> ordersToPick)
        {
            var orderIds = new List<int>();
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }

            return orderIds.ToArray();
        }

        private ReplenPickView CreatePickView(int pos, ReplenOrderDetail detail)
        {
            Task.Run(() => _logger.Log($"CreatePickView Start: [{DateTime.Now.ToLongTimeString()}]"));
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

            ItemDefinition def = _repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
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

            Task.Run(() => _logger.Log($"CreatePickView End: [{DateTime.Now.ToLongTimeString()}]"));
            return pickView;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            Task.Run(() => _logger.Log($"Get Inventory Item: {itemId} START"));

            //var pickableLocations = new int[] { 1, 2 };
            //recs = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
            //    .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            var recs = _currentInventory.FindAll(r => r.ItemDefinitionId == itemId);

            Task.Run(() => _logger.Log($"Get Inventory END"));
            return recs;
        }

        private void LoadInventory()
        {
            Task.Run(() => _logger.Log($"Load Inventory START"));
            //_currentInventory = new List<Inventory>();
            var pickableLocations = new int[] { 1, 2 };
            //if (_currentInventory.Count != 0) return;

            _currentInventory = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => pickableLocations.Contains(f.StorageTypeId)).ToList();
            Task.Run(() => _logger.Log($"Load Inventory END"));
        }

        //private void FrmReplen_FormClosing(object sender, FormClosingEventArgs e)
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
            _gridClickedAvailableOrders = true;
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    int id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    RemoveItemFromBatch(id);
                }
                else
                {
                    DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    int id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                    if (id > 0)
                    {
                        string ord1 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                        string ord2 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord2"].Value.ToString();
                        AddItemToBatch(id, ord1, ord2);
                    }
                }
            }
        }

        private void RemoveItemFromBatch(int orderId)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId != orderId) continue;
                bp.OrderId = null;
                bp.Ord1 = string.Empty;
                bp.Ord2 = string.Empty;
                bp.OrderComplete = false;
                UpdateTextBoxPosition(bp);
            }
        }

        private int AddItemToBatch(int orderId, string ord1, string ord2)
        {
            var idx = _manualOverrideCurrentTextBoxPos
                ? SetBatchPositionToManualOverride()
                : SetBatchPositionToFirstEmpty();

            if (idx >= 0 && idx <= _neutronVariables.StoreBatchSize)
            {
                _ordersToPick[idx].OrderId = orderId;
                _ordersToPick[idx].Ord1 = ord1;
                _ordersToPick[idx].Ord2 = ord2;
                _ordersToPick[idx].OrderComplete = false;
                _currentTextBoxPos.Text = ord1;
            }

            _manualOverrideCurrentTextBoxPos = false;
            ClearTextBoxPosBackColor();
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
                bool checkedBox = Convert.ToBoolean(row.Cells[0].Value);

                if (checkedBox)
                {
                    int orderId = Convert.ToInt32(row.Cells["Id"].Value);
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

        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            Control c = Controls.Find($"TextBoxPos{batchPositionNumber}", true).Single() as TextBox;
            if (c != null) _currentTextBoxPos = (TextBox)c;
            _currentTextBoxPos.BackColor = Color.Yellow;
        }

        private void InitOrdersToPick(int pickBatchSize)
        {
            _ordersToPick = new List<BatchPosition>();
            for (var i = 0; i < pickBatchSize; i++)
            {
                var bp = new BatchPosition()
                {
                    PositionNumber = i + 1,
                    OrderId = null,
                    Ord1 = string.Empty,
                    Ord2 = string.Empty,
                    OrderComplete = false
                };
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
                c.Visible = true;
                c.Font = font;
            }

            c = Controls.Find("TextBoxPos" + pos, true).Single() as TextBox;
            if (c != null) c.Visible = true;

            c = Controls.Find("Pos" + pos + "Display", true).Single();
            ((Panel)c).Visible = true;

            c = Controls.Find("AvailablePos" + pos + "Display", true).Single();
            ((Panel)c).Visible = true;
        }

        private void ShowOrdersToPick()
        {
            Task.Run(() => _logger.Log($"ShowOrdersToPick Start: [{DateTime.Now.ToLongTimeString()}]"));
            var font = new Font("Microsoft Sans Serif", 10);
            for (var i = 0; i < _ordersToPick.Count; i++)
            {
                var pos = (i + 1).ToString();
                Control c = Controls.Find("TextBoxPickPos" + pos, true).Single() as TextBox;
                if (c == null) continue;
                c.Font = font;
                c.Text = _ordersToPick[i].Ord1 + Environment.NewLine + _ordersToPick[i].Ord2;
            }

            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Show Orders To Pick On Displays Clear ALL Bli"));
                        //GlobalVar.Displays.ClearAllBli();
                        foreach (var item in _ordersToPick)
                        {
                            if (string.IsNullOrEmpty(item.Ord1)) continue;
                            Task.Run(() => _logger.Log($"Show Each Order To Pick {item.Ord1}"));
                            GlobalVar.Displays.ShowBli(item.PositionNumber, beacon: 2, text: item.Ord1);
                        }
                    }
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
            LabelFormTitle.Text = "Pick List";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = PickList;
        }

        private void MBStart_Click(object sender, EventArgs e)
        {
            Start();
        }
        //private void Start()
        //{
        //    Task.Run(() => _logger.Log($"Start_Click Start: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    var pickViews = (IList<ReplenPickView>)bindingSourcePickViews.DataSource;
        //    pickViews.OrderBy(p => p.CurrentInventoryLocation.Location.Loc1)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc2)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
        //        .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4).ToList();
        //    //TODO SetOrderStatusToPicking(pickViews);
        //    Task.Run(() => _logger.Log($"Start_Click 1: [{System.DateTime.Now.ToLongTimeString()}]"));
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
        //    Task.Run(() => _logger.Log($"Start_Click 2: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    List<ReplenPickStop> finalPickSequence = FinalPickSequence(pickStops);
        //    bindingSourcePickStops.DataSource = finalPickSequence;
        //    Task.Run(() => _logger.Log($"Start_Click 3 Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    // GetFirstStop();
        //    bindingSourcePickStops.MoveFirst();
        //    currentPickStop = (ReplenPickStop)bindingSourcePickStops.Current;
        //    UpdatePickScreen();
        //    Task.Run(() => _logger.Log($"Start_Click 4  Run GetFirstStop?: [{System.DateTime.Now.ToLongTimeString()}]"));
        //    // PrintAllDocuments();
        //    // PrintAllToteLabels();

        //    tabControl1.SelectedTab = PickScreen;
        //    //feels good to here
        //    Task.Run(() => _logger.Log($"Start_Click End: [{System.DateTime.Now.ToLongTimeString()}]"));
        //}

        private List<ReplenPickStop> FinalPickSequence(List<ReplenPickStop> pickStops)
        {
            Task.Run(() => _logger.Log($"FinalPickSequence Start: [{DateTime.Now.ToLongTimeString()}]"));
            var newCarList = new List<List<ReplenPickStop>>();
            var newList = new List<ReplenPickStop>();
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
            _deviceManager = new ReplenDeviceManager(newCarList, _neutronVariables.ShuttleEnabled);
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
            Task.Run(() => _logger.Log($"3384 PositionDevice"));
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
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;

                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"3012 GetFirstStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
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
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"3445 GetNextStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
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
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                _logger.Log($"3470 GetPreviousStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
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
                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
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
                _logger.Log($"3494 GetLastStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
            }
            Task.Run(() => _logger.Log($"GetLastStop Return"));
        }

        private void ButtonStopMoveFirst_Click(object sender, EventArgs e)
        {
            GetFirstStop(moveDevice: false);
        }

        private void ButtonStopMovePrevious_Click(object sender, EventArgs e)
        {
            GetPreviousStop(moveDevice: false);
        }

        private void ButtonStopMoveNext_Click(object sender, EventArgs e)
        {
            GetNextStop(moveDevice: false);
        }

        private void ButtonStopMoveLast_Click(object sender, EventArgs e)
        {
            GetLastStop(moveDevice: false);
        }


        private void UpdatePickScreen()
        {
            Task.Run(() => _logger.Log($"UpdatePickScreen Start: [{DateTime.Now.ToLongTimeString()}]"));


            if (_neutronVariables.UseImages) PictureBoxItemImage.LoadAsync(_imageManager.GetImageFile(_currentPickStop.Item));
            LabelFormTitle.Text = _resourceManager.GetString($"Selection");
            LabelPickDescription.Text = _currentPickStop.Description;
            LabelPickItemNumber.Text = _currentPickStop.Item;
            LabelPickUOI.Text = _currentPickStop.UnitOfIssue;
            LabelLineOfLines.Text = string.Format("{0} of {1}"
                , _currentPickStop.Sequence.ToString(), _bindingSourcePickStops.Count);
            TextBoxRequestedQty.Text = _currentPickStop.Quantity.ToString();

            var pickedSoFar = GetPickedSoFar(_currentPickStop.PickViews);
            TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
            Task.Run(() => _logger.Log($"UpdatePickScreen End: [{DateTime.Now.ToLongTimeString()}]"));
            MBStoreAccept.Enabled = _currentPickStop.Quantity != pickedSoFar;
            MBPickChangeQuantity.Enabled = _currentPickStop.Quantity != pickedSoFar;
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
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            var text = _currentPickStop.QuantityToBePicked.ToString();
            ShowShi(loc1, loc2, loc3, loc4, text);

        }


        private void UpdateGroupBoxLocation(Inventory inventory)
        {
            Task.Run(() => _logger.Log($"Update GroupBox Location Start : [{DateTime.Now.ToLongTimeString()}]"));

            TextBoxPickLoc1.Text = inventory.Location.Loc1.ToString();
            TextBoxPickLoc2.Text = inventory.Location.Loc2.ToString();
            TextBoxPickLoc3.Text = inventory.Location.Loc3.ToString();
            TextBoxPickLoc4.Text = inventory.Location.Loc4.ToString();
            TextBoxPickLoc5.Text = inventory.Location.Loc5.ToString();
            LabelLocationNumber.Text =
                $"{_currentPickStop.GroupBoxLocationInventoryIndex + 1} of {_currentPickStop.Inventory.Count}";
            TextBoxLocationQuantity.Text = inventory.Quantity.ToString();
            TextBoxTotalQuantity.Text = _currentPickStop.Inventory.Sum(r => r.Quantity).ToString();
            TextBoxReceivedDate.Text = inventory.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = inventory.PrimeBin;
            LabelStaticRelease.Text = inventory.StorageType.Name;
            Task.Run(() => _logger.Log($"Update GroupBox Location End : [{DateTime.Now.ToLongTimeString()}]"));
        }


        //private void UpdateInventoryLocation()
        //{
        //    Task.Run(() => _logger.Log($"UpdateInventoryLocation Start : [{System.DateTime.Now.ToLongTimeString()}]"));

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
        //    Task.Run(() => _logger.Log($"UpdateInventoryLocation End : [{System.DateTime.Now.ToLongTimeString()}]"));
        //}

        private void ShowShi(int loc1, int loc2, int loc3, string loc4, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        ClearAllShi();
                        Task.Run(() =>
                            _logger.Log(
                                $"Frm Replen Show SHI {loc1}-{loc2}-{loc3}-{loc4}{Environment.NewLine}-{text}"));
                        GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text);
                    }
                }
            }

        }

        private void ClearAllShi()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.ShiEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen ClearAllShi"));
                        GlobalVar.Displays.ClearAllShi();
                    }
                }
            }
        }

        private void ClearAllBli()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen ClearAllBli"));
                        GlobalVar.Displays.ClearAllBli();
                    }
                }
            }
        }

        private void TurnOnOcDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => GlobalVar.Displays.ShowOc(position, beacon, text));
                    }
                }
            }
        }

        private void UpdatePickPosition()
        {
            Task.Run(() => _logger.Log($"UpdatePickPosition Start : [{DateTime.Now.ToLongTimeString()}]"));
            SetOrderCompleteThisStation();
            ClearPickPositions();
            ClearPickDisplays();
            ClearAllBli();
            var font = new Font("Microsoft Sans Serif", 20F, FontStyle.Bold);
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
                    textBox.Font = font;
                    textBox.Text = pickView.QuantityToBePicked.ToString();
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

            //-------------------------
            //foreach (var pickView in _currentPickStop.PickViews)
            //{
            //    var pos = pickView.PickPosition;

            //    Control control = Controls.Find($"TextBoxPickPos{pos}", true).First();
            //    if (control != null)
            //    {
            //        var textBox = ((TextBox)control);
            //        textBox.Text = pickView.QuantityToBePicked.ToString();
            //    }
            //    control = Controls.Find($"LabelPickPos{pos}", true).First();
            //    if (control != null)
            //    {
            //        var label = ((Label)control);
            //        label.BackColor = GetBackColor(pickView.QuantityToBePicked);
            //    }
            //    control = Controls.Find($"Pos{pos}Display", true).First();
            //    if (control != null)
            //    {
            //        var panel = ((Panel)control);
            //        panel.BackColor = Color.Red;
            //    }
            //    TurnOnBatchPositionDisplay(position: pos, beacon: 2, text: pickView.QuantityToBePicked.ToString());
            //}
            //Task.Run(() => _logger.Log($"UpdatePickPosition End : [{DateTime.Now.ToLongTimeString()}]"));

        }

        private void SetOrderCompleteThisStation()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var linesNotComplete = _repoReplenOrderDetails
                    .FindBy(r => r.ReplenOrderId == bp.OrderId && r.StationNumber == _station.StationNumber)
                    .Where(r => r.LineStatusId != (int)LineStatus.Complete).ToList();
                if (linesNotComplete.Count != 0) continue;
                bp.OrderComplete = true;
            }
        }

        private void TurnOnBatchPositionDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Frm Replen TurnOnBatchPositionDisplay"));
                        GlobalVar.Displays.ShowBli(position, beacon, text);
                    }
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
                return Color.Gray;
            }
        }

        private string GetRemainingToPick(int totalQty, int pickedSoFar)
        {
            int result = totalQty - pickedSoFar;

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

        private void ClearPickPositions()
        {
            TextBoxPickPos1.Text = string.Empty;
            TextBoxPickPos2.Text = string.Empty;
            TextBoxPickPos3.Text = string.Empty;
            TextBoxPickPos4.Text = string.Empty;
            TextBoxPickPos5.Text = string.Empty;
            TextBoxPickPos6.Text = string.Empty;
            TextBoxPickPos7.Text = string.Empty;
            TextBoxPickPos8.Text = string.Empty;
        }

        private void ClearPickDisplays()
        {
            Pos1Display.BackColor = Color.Transparent;
            Pos2Display.BackColor = Color.Transparent;
            Pos3Display.BackColor = Color.Transparent;
            Pos4Display.BackColor = Color.Transparent;
            Pos5Display.BackColor = Color.Transparent;
            Pos6Display.BackColor = Color.Transparent;
            Pos7Display.BackColor = Color.Transparent;
            Pos8Display.BackColor = Color.Transparent;
        }

        private int GetTotalRequiredThisStop(ReplenPickStop currentPickStop)
        {
            int total = TextBoxPickPos1.Text.ParseInt();
            total += TextBoxPickPos2.Text.ParseInt();
            total += TextBoxPickPos3.Text.ParseInt();
            total += TextBoxPickPos4.Text.ParseInt();
            total += TextBoxPickPos5.Text.ParseInt();
            total += TextBoxPickPos6.Text.ParseInt();
            total += TextBoxPickPos7.Text.ParseInt();
            total += TextBoxPickPos8.Text.ParseInt();
            return total;
        }

        //private void MBPickStore_Click(object sender, EventArgs e)
        //{
        //    PickAccept();
        //    MBStoreAccept.Focus();
        //}

        //private void PickAccept()
        //{



        //    Cursor.Current = Cursors.WaitCursor;
        //    Task.Run(() => _logger.Log($"PickAccept_Click Start : [{DateTime.Now.ToLongTimeString()}]"));
        //    bool pick = false;
        //    pick = _currentPickStop.CurrentInventoryLocation.Quantity < _currentPickStop.QuantityToBePicked ? false : true;

        //    if (pick)
        //    {
        //        _currentPickStop.UpdatePickViews(GlobalVar.User);  //good
        //        _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);
        //        _currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews
        //        Task.Run(() => _logger.Log($"PickAccept_Click 1 : [{DateTime.Now.ToLongTimeString()}]"));
        //        if (StopComplete())
        //        {
        //            //Getting next location on the current device/ the one that was just picked from.
        //            Task.Run(() => _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1));
        //            Task.Run(() => _logger.Log($"PickAccept_Click 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]"));

        //            UpdateInventoryQuantity(_currentPickStop);

        //            GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreOrder, _currentPickStop);

        //            _currentPickStop.SetPickViewsComplete(GlobalVar.User);

        //            Task.Run(() => _logger.Log($"PickAccept_Click Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

        //            int numberOfStops = _bindingSourcePickStops.Count;
        //            if (_currentPickStop.Sequence < numberOfStops)
        //            {
        //                _bindingSourcePickStops.MoveNext();
        //                _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
        //                UpdatePickScreen();
        //            }
        //            else
        //            {
        //                CloseBatch();
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
        //    Task.Run(() => _logger.Log($"PickAccept_Click End : [{DateTime.Now.ToLongTimeString()}]"));
        //    Cursor.Current = Cursors.Default;
        //}

        private void MBStoreAccept_Click(object sender, EventArgs e)
        {
            StoreAccept();
            MBStoreAccept.Focus();
        }

        private void StoreAccept()
        {
            if (InvokeRequired)
            {
                var method = new MethodInvoker(StoreAccept);
                Invoke(method);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Task.Run(() => _logger.Log($"StoreAccept_Click Start : [{DateTime.Now.ToLongTimeString()}]"));
            MBStoreAccept.Enabled = false;
            //ClearActiveDeviceIndicators();
            //ClearAllDeviceIndicators();
            _currentPickStop.UpdatePickViews(GlobalVar.User); //good
            _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);
            _currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);
            //_currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews
            Task.Run(() => _logger.Log($"PickAccept_Click 1 : [{DateTime.Now.ToLongTimeString()}]"));
            if (StopComplete())
            {
                //Getting next location on the current device/ the one that was just picked from.
                Task.Run(() => _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1));
                Task.Run(() =>
                    _logger.Log($"PickAccept_Click 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]"));

                UpdateInventoryQuantity(_currentPickStop);

                GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreOrder, _currentPickStop);

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
                    _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                    _bindingSourcePickStops.MoveNext();
                    _currentPickStop = (ReplenPickStop)_bindingSourcePickStops.Current;
                    UpdatePickScreen();
                    UpdateCurrentDeviceIndicator();
                    UpdatePickPosition();
                    UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                    UpdateTowerDisplay();
                }
                else
                {
                    CloseBatch();
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

                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();
            }


            Task.Run(() => _logger.Log($"StoreAccept_Click End : [{DateTime.Now.ToLongTimeString()}]"));
            MBStoreAccept.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private bool CheckForOrderCompleteOnDevice(ReplenOrder order)
        {
            var linesNotComplete = _repoReplenOrderDetails.FindBy(r => r.ReplenOrderId == order.Id).Where(r => r.LineStatusId != (int)LineStatus.Complete)
                .ToList();
            if (linesNotComplete.Any()) return false;

            order.OrderStatusId = (int)OrderStatus.Complete;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order, _station.StationId);
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

        private bool StopComplete()
        {
            var result = false;
            // they could put more away than the Replen order indicated
            if (_currentPickStop.QuantityToBePicked <= 0)
            {
                result = true; //Stop Complete
            }
            else if (_currentPickStop.QuantityToBePicked > 0) // Still have qty to put away
            {
                var movedToNewLocation =
                    NextPickingLocation(); // moved to next existing location so don't end batch return false
                if (!movedToNewLocation)
                {
                    // no more locations
                    //var locations = Task.Run(() => LoadNewLocations(_currentPickStop.ItemId, _numberOfInventoryLocations)).Result;
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
                    //        StationId = _currentPickStop.CurrentInventoryLocation.StationId,
                    //        Station = _currentPickStop.CurrentInventoryLocation.Station,
                    //        StorageTypeId = storageType.Id,
                    //        StorageType = storageType
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

        private List<Inventory> GetNewInventoryLocations(ItemDefinition itemDefinition,
            int numberOfInventoryLocations = 5)
        {
            var inventory = new List<Inventory>();
            var locations = Task.Run(() => LoadNewLocations(itemDefinition, numberOfInventoryLocations)).Result;
            //var itemDefinition = _repoItemDefinition.FindByKey(itemId);
            //var station = _repoStation.FindByKey(itemDefinition.StationId);
            var storageType = _defaultStorageType;
            if (locations.Any())
            {
                foreach (var loc in locations)
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
                        StationId = itemDefinition.Station.Id,
                        Station = itemDefinition.Station,
                        StorageTypeId = storageType.Id,
                        StorageType = storageType
                    };
                    inventory.Add(inv);
                }
            }

            return inventory;
        }

        private async Task<List<Location>> LoadNewLocations(ItemDefinition itemDefinition,
            int numberOfInventoryLocations)
        {
            // Return top 20 locations
            // First get exact matches
            // Then add the balance needed from the rest in order of size sequence
            var stationNumber = itemDefinition.Station.StationNumber;
            var item = itemDefinition;

            var views = await Task.Run(() => _repoLocationRepository.All().Where(s =>
                s.Station.StationNumber == stationNumber && s.SizeCodeId == item.SizeCodeId &&
                s.VelocityCodeId == item.VelocityCodeId && s.HeightCodeId == item.HeightCodeId &&
                s.LocationCodeId == item.LocationCodeId && s.InUse == false));

            //var locations = views.Except(_tempAllocatedLocations).Take(numberOfInventoryLocations).ToList();
            var locations = views.Where(p => _tempAllocatedLocations.All(s => s.Id != p.Id))
                .Take(numberOfInventoryLocations).ToList();
            //list1.Where(p => !list2.Any(x => x.ID == p.ID

            if (locations.Count >= numberOfInventoryLocations)
            {
                _tempAllocatedLocations.AddRange(locations);
                return locations;
            }


            //if short of 10 new locations get new locations with larger size codes
            // based on its Sequence number.
            var numberNeeded = numberOfInventoryLocations - locations.Count;
            var sequenceNumber = item.SizeCode.Sequence;
            views = await Task.Run(() => _repoLocationRepository.All().Where(s =>
                s.Station.StationNumber == stationNumber &&
                s.SizeCode.Sequence > sequenceNumber && s.InUse == false).OrderBy(s => s.SizeCode.Sequence));


            var moreLocations = views.Where(p => _tempAllocatedLocations.All(s => s.Id != p.Id))
                .Take(numberNeeded).ToList();
            locations.AddRange(moreLocations);
            _tempAllocatedLocations.AddRange(locations);
            return locations;
        }

        // this is called when there are multiple locations on the same PickStop
        private bool NextPickingLocation()
        {
            bool result = false;

            var idx = _currentPickStop.InventoryIndex + 1 < _currentPickStop.Inventory.Count
                ? _currentPickStop.InventoryIndex + 1
                : 0;
            if (idx > 0)
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

        private void UpdateInventoryQuantity(ReplenPickStop pickStop)
        {
            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    if (pickLocation.Quantity > 0)
                    {
                        pickLocation.Inventory.Quantity += pickLocation.Quantity;
                        var inventory = _repoInventory.FindByKey(pickLocation.Inventory.Id);
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
                                    StationId = pickLocation.Inventory.StationId,
                                    StorageTypeId = pickLocation.Inventory.StorageTypeId
                                };
                                _repoInventory.Insert(inv);
                                pickLocation.Inventory.Id = inv.Id;
                            }


                            //  _repoInventory.Insert(pickLocation.Inventory);
                        }
                        else
                        {
                            pickLocation.Inventory.Quantity += pickLocation.Quantity;
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
                StationId = pickLocation.Inventory.StationId,
                StorageTypeId = pickLocation.Inventory.StorageTypeId
            };
            _repoInventory.Insert(inventory);

            return inventory;
        }

        //TODO
        private void CloseBatch()
        {
            ClearAllShi();
            ClearAllBli();
            ClearOrderPositions();
            ClearBatchPositions();
            Console.WriteLine("Clear All Device Indicators - Close Batch");
            ClearAllDeviceIndicators();

            _logger.Log($"Start Upload Processor: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "TMG":
                    //var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    //uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "SFH":
                    // Mediator.GetInstance().OnBatchComplete(this);
                    _logger.Log("Choosing the SFH case.");
                    //uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    //uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "AES":
                    //var uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger, _rackStation);
                    //uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "TOP":
                    //var topUploadProcessor = new TopUploadProcessor(_neutronVariables, _neutronLicense, _rackStation);
                    //topUploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;

                default:
                    break;
            }

            using (var db = new NeutronDb())
            {
                var locationIds = new List<int>();
                var invs = db.Inventory.Where(r =>
                    r.Quantity == 0 && r.StationId == _station.StationId &&
                    r.StorageTypeId == (int)NeutronCore.Enums.StorageType.Release).ToList();
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

                        var item = db.Inventory.FirstOrDefault(r => r.LocationId == locationId);
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

            ParkPositionAfterBatch();

            if (_neutronVariables.AutoLogOff)
            {
                CloseButtonPressed = true;
                Close();
            }
            else
            {
                ShowAllOrders();
                ShowAvailableOrders();
                tabControl1.SelectedTab = AvailableOrders;
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

        private void PrintAllDocuments()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true)
                    .FirstOrDefault();
                if (printJob != null) continue;
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDocument(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true)
                    .FirstOrDefault();
                if (printJob != null) continue;
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintDoc(int positionNumber, ReplenOrder order)
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
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintToteLabel(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintTote(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintTote(int positionNumber, ReplenOrder order)
        {
            Task.Run(() => _logger.Log($"Printing Tote Label. {order.Ord1}"));
            if (_neutronVariables.EnableLabelPrinter)
            {
                Task.Run(() => ToteToPrint.Print(positionNumber, order, _labelPrinter));
            }
        }



        private void MarkCompleted(List<BatchPosition> ordersToPick)
        {
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    int id = bp.OrderId.Value;
                    ReplenOrder rec = _repoReplenOrder.FindByKey(id);
                    rec.OrderStatusId = (int)OrderStatus.Complete;
                    _repoReplenOrder.Update(rec);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, rec);
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
            var pickView = _currentPickStop.PickViews.FirstOrDefault(p => p.PickPosition == pos);
            if (pickView == null) return;
            if (newQty <= pickView.GetQuantityToBePicked())
            {
                pickView.QuantityToBePicked = newQty;
                _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
                LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
                UpdatePickScreenAfterChangeQuantity();
            }
        }

 

        private void ButtonMove_Click(object sender, EventArgs e)
        {
            //ClearActiveDeviceIndicators();
            //ClearAllDeviceIndicators();
            _currentPickStop.CurrentInventoryLocation =
                _currentPickStop.Inventory[_currentPickStop.GroupBoxLocationInventoryIndex];
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
            UpdateCurrentDeviceIndicator();
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
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangePriority, order);
                }
            }

            ShowAllOrders();
        }

        private void SetOrderStatus(ReplenOrder order, int status, ActionCode actionCode)
        {
            try
            {
                order.OrderStatusId = status;
                _repoReplenOrder.Update(order);
                GlobalVar.HistoryManager.SaveHistory(actionCode, order);
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
                GlobalVar.HistoryManager.SaveHistory(actionCode, detail);
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
        //        GlobalVar.HistoryManager.SaveHistory((int)ActionCode.PickOrder, detail);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error setting ReplenOrder Detail Status Code. " + ex.Message);
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
            var details = _orderDetailsRepository.GetOrderDetailsViewByOrder(orderId);
            _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = _bindingSourceOrderDetailsView;
            GetRecordCount(_bindingSourceOrderDetailsView);
            LabelFormTitle.Text = "Job Details";
            tabControl1.SelectedTab = OrderDetails;
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
            Task.Run(() => _logger.Log($"Job Manager Main Screen Start"));
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.Green;
            ShowAllOrders();
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBCompress.Visible = false;
            tabControl1.SelectedTab = OrderListing;
            Cursor.Current = Cursors.Default;
        }

        private void MBShowAvailable_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _activeGrid = "Available";
            MBDeleteOrder.Visible = _station.StationType.Id == (int)StationType.Supervisor;
            MBCompress.Visible = false;
            _currentDataSet = CurrentDataSet.Available;
            ShowAllOrders();
            Cursor.Current = Cursors.Default;
        }

        private void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
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
            InitOrdersToPick(_neutronVariables.StoreBatchSize);
            MBCompress.Enabled = false;
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.Green;
            ClearBatchPositions();
            ClearOrderPositions();
            ShowAvailableOrders();
            tabControl1.SelectedTab = AvailableOrders;
            Cursor.Current = Cursors.Default;
            Task.Run(() => _logger.Log("Available Orders Screen END"));
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
            Task.Run(() => _logger.Log($"ShowAvailableOrdersRack Replen: [{DateTime.Now.ToLongTimeString()}]"));
            var idx = 0;

            if (string.IsNullOrEmpty(findWhat))
            {
                findWhat = TextBoxFindAvailableOrdersRack.Text.Trim().ToLower();
            }

            try
            {
                var views = _replenOrdersRepository.GetRackOrdersView(_rackStation.StationNumber, findWhat);

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
                Task.Run(() =>
                    _logger.Log(
                        $"ShowAvailableOrdersRack Replen Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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

            Task.Run(() => _logger.Log($"ShowAvailableOrdersRack Replen End: [{DateTime.Now.ToLongTimeString()}]"));
            return idx;
        }

        //Ready
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
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
            DataGridViewAvailableOrders.Refresh();
            LabelFormTitle.Text = "Job Listing";
            LabelFormTitle.BackColor = Color.Green;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBLocationCount_Click(object sender, EventArgs e)
        {
            int inventoryId = _currentPickStop.CurrentInventoryLocation.Id;
            int qty = OpenLocationCountForm(inventoryId);

            if (qty >= 0)
            {
                TextBoxLocationQuantity.Text = qty.ToString();
                _currentPickStop.CurrentInventoryLocation.Quantity = qty;
                int total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                TextBoxTotalQuantity.Text = total.ToString();
                LoadInventory();
            }
        }


        private int OpenLocationCountForm(int inventoryId)
        {
            int qty = -1;
            using (FrmLocationCount form = new FrmLocationCount())
            {
                DialogResult result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    qty = (form.NewQty).ParseInt();
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
            Inventory inv = _repoInventory.FindByKey(inventoryId);
            int prevQty = inv.Quantity;
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
            // _repoLocationCount.Insert(cnt);
            GlobalVar.HistoryManager.SaveHistory(ActionCode.LocationCount, cnt);
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
            MBCompress.Visible = _station.StationType.Id == (int)StationType.Supervisor;
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
                Task.Run(() =>
                    _logger.Log(
                        $"Select All Error: {ex.Message} {Environment.NewLine} {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
            }
        }


        private void ShowCompleted(int recId = 0)
        {
            Task.Run(() =>
                _logger.Log(
                    $"ShowCompletedOrders Replen Start: [{DateTime.Now.ToString(CultureInfo.InvariantCulture)}]"));
            var idx = 0;

            var findWhat = string.IsNullOrEmpty(TextBoxFind.Text.Trim().ToLower())
                ? string.Empty
                : TextBoxFind.Text.Trim().ToLower();
            // string find = _akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            var views = _replenOrdersRepository.GetCompletedOrders(findWhat);
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

            Task.Run(() => _logger.Log($"ShowCompletedOrders End: [{DateTime.Now.ToLongTimeString()}]"));
            return;

        }

        private void ShowButtons()
        {
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBDeleteOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBCompress.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        }

        private void HideButtons()
        {
            MBPriority.Visible = false;
            MBHold.Visible = false;
            MBRelease.Visible = false;
            MBDeleteOrder.Visible = _station.StationTypeId == (int)StationType.Supervisor;
            MBCompress.Visible = _station.StationTypeId == (int)StationType.Supervisor;
        }

        //Ready
        private void MBNewOrderSearch_Click(object sender, EventArgs e)
        {
            var findWhat = TextBoxNewOrderFind.Text.Trim().ToLower();
            Task.Run(() => _logger.Log($"MBNewOrderSearch_Click  {findWhat}"));
            FindItemRecord(findWhat);
        }

        //Ready
        private void FindItemRecord(string s)
        {
            Task.Run(() => _logger.Log($"FindItemRecord  {s}"));
            try
            {
                _bindingSourceItems.DataSource = _itemDefinitionsRepository.GetNewItemViews(s.Trim()).ToList();
                Task.Run(() => _logger.Log($"Return from Getting Datasource Count:  {_bindingSourceItems.Count}"));
                DataGridViewNewOrder.DataSource = _bindingSourceItems;
                Task.Run(() => _logger.Log($"Bind Datasource to Grid"));
                DataGridViewNewOrder.ClearSelection();
                Task.Run(() => _logger.Log($"Clear and Update Grid "));
                DataGridViewNewOrder.Update();
                Task.Run(() => _logger.Log($"FindItemRecord  Complete"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Find Error: " + ex.Message);
            }
        }

        //Ready
        private void TextBoxNewOrderFind_KeyDown(object sender, KeyEventArgs e)
        {
            Task.Run(() => _logger.Log($"TextBoxNewOrderFind_KeyDown  {e.KeyCode}"));
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
            Task.Run(() => _logger.Log($"DataGridViewNewOrder_CellContentClick Row:  {e.RowIndex}"));
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
                StationNumber = LabelNewOrderStationNumber.Text.ParseInt(),
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
                    OrderStatusId = (int)OrderStatus.Available
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
                        StationNumber = view.StationNumber,
                        LineStatusId = (int)OrderStatus.Available,
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
                AddItemToBatch(idValue, ord1, ord2);
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
            ShowOrders();
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
                if (orderDetail.LineStatusId == (int)OrderStatus.Hold)
                {
                    orderDetail.LineStatusId = (int)OrderStatus.Available;
                    _repoReplenOrderDetails.Update(orderDetail);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseLine, orderDetail);
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
                if (orderDetail.LineStatusId == (int)OrderStatus.Available)
                {
                    orderDetail.LineStatusId = (int)OrderStatus.Hold;
                    _repoReplenOrderDetails.Update(orderDetail);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldLine, orderDetail);
                }
            }
            ShowOrderDetails(orderId);
        }

        private void DataGridPickView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //if (e.RowIndex < 0)
            //{
            //    return;
            //}
            //DataGridViewCell totalQuantityCell = DataGridPickView.Rows[e.RowIndex].Cells["TotalQuantityInInventory"];
            //DataGridViewCell quantityCell = DataGridPickView.Rows[e.RowIndex].Cells["Quantity"];

            //int totalValue = totalQuantityCell.Value == null ? 0 : totalQuantityCell.Value.ToString().ParseInt();
            //int quantityValue = quantityCell.Value == null ? 0 : quantityCell.Value.ToString().ParseInt();

            //if (quantityValue > totalValue)
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
            //}
            //else
            //{
            //    DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            //}
        }

        private void MBPickScreenHotPick_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                var item = LabelPickItemNumber.Text;
                Hide();
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository, _neutronVariables,
                    _lacProcessor, _imageManager, _itemDefinitionsRepository, item))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                    Task.Run(() => _deviceManager.Reset());
                    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
                }
                LoadInventory();
                UpdateInventoryAfterHotAction();
                UpdatePickScreen();
                UpdateCurrentDeviceIndicator();
                UpdatePickPosition();
                UpdateGroupBoxLocation(_currentPickStop.CurrentInventoryLocation);
                UpdateTowerDisplay();

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

        private void UpdateInventoryAfterHotAction()
        {
            var exactInventorySequence = new List<Inventory>();
            var success = false;
            var pickView = _currentPickStop.PickViews.FirstOrDefault();
            if (pickView != null)
            {
                exactInventorySequence = _neutronVariables.UsePrimeBin ? PrimeBinFirst(pickView) : Fifo(pickView);

                var neededLocations = 2 - exactInventorySequence.Count;
                if (neededLocations > 0)
                {
                    var additionInventoryLocations =
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
            ClearTextBoxPosBackColor();
            _currentTextBoxPos.BackColor = Color.Yellow;
            _manualOverrideCurrentTextBoxPos = true;
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


        private void label4_Click(object sender, EventArgs e)
        {

        }


        private void FrmReplen_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                // MessageBox.Show("Launch Find Box in PICK");

                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
                {
                    DialogResult result = frm.ShowDialog();
                    Show();
                }


            }
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

            if (!orders.Any()) return;
            foreach (var order in orders)
            {
                var detailLinesThisStation = _repoReplenOrderDetails
                    .FindBy(r => r.ReplenOrderId == order.Id && r.StationNumber == stationNumber).ToList();
                if (detailLinesThisStation.Count > 0)
                {
                    foreach (var orderDetail in detailLinesThisStation)
                    {
                        orderDetail.LineStatusId = (int)LineStatus.Complete;
                        orderDetail.PickedQuantity = orderDetail.Quantity;
                        orderDetail.EmpId = GlobalVar.User.EmpId;
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickRack, orderDetail);
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

            order.OrderStatusId = (int)OrderStatus.Complete;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order: order);
            _repoReplenOrder.Update(order);
        }

        private void MBRefreshRack_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MbPrintAvailableOrdersRack_Click(object sender, EventArgs e)
        {
            CsvUtility.SaveToCsv(DataGridViewAvailableOrdersRack);
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrintDocumentAndToteLabel_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrint_Click(object sender, EventArgs e)
        {
            var position = _currentPickStop.PickViews.First().PickPosition;

            using (FrmReprint form = new FrmReprint(_neutronVariables, position))
            {
                DialogResult result = form.ShowDialog();
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
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
                if (printJob == null)
                {
                    PrintTote(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                    _repoPrintJob.Insert(printJob);
                }
                else
                {
                    PrintTote(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                    _repoPrintJob.Update(printJob);
                }
            }
        }

        private void ReprintDocument(int batchPosition)
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoReplenOrder.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id).FirstOrDefault();
                if (printJob == null)
                {
                    PrintDoc(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                    _repoPrintJob.Insert(printJob);
                }
                else
                {
                    PrintDoc(bp.PositionNumber, order);
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                    _repoPrintJob.Update(printJob);
                }
            }
        }

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
            PictureBoxItemImage.Location = new Point(318, 117);
            PictureBoxItemImage.Size = new Size(512, 512);
            PictureBoxItemImage.BringToFront();
        }

        private void PictureBoxItemImage_MouseLeave(object sender, EventArgs e)
        {
            if (!_neutronVariables.UseImages) return;
            if (!_neutronVariables.AutoEnlargeImage) return;
            PictureBoxItemImage.Location = new Point(398, 499);
            PictureBoxItemImage.Size = new Size(256, 256);
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
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.ReplenDetailDelete, orderDetail);
                        }

                        _repoReplenOrder.Delete(order.Id);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.ReplenOrderDelete, order);
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
                MBShowRackOrders.Text = _resourceManager.GetString($"MBShowRackOrders");
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

        private void MBShowRackOrders_Click(object sender, EventArgs e)
        {
            //Cursor.Current = Cursors.WaitCursor;
            //DataGridView1.Columns.Clear();
            //_currentDataSet = CurrentDataSet.Rack;
            ////  SetupOrderGrid();
            //ShowRackOrders();
            //Cursor.Current = Cursors.Default;
            _activeGrid = "Rack";
            ShowAvailableRackScreen();
        }

        private int ShowRackOrders(int recId = 0)
        {
            Task.Run(
                () => _logger.Log($"Show Rack Orders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
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

            Task.Run(() => _logger.Log($"Show Rack Orders End: [{DateTime.Now.ToLongTimeString()}]"));
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
            var station = _stationRepository.GetStationView(_rackStation.Id);
            Hide();
            var item = pickList.Item;
            var quantity = pickList.Ordered.ParseInt();
            using (MetroForm frm = new FrmHotAction(station, _jsonData, _akaRepository
                , _neutronVariables, _lacProcessor, _imageManager, _itemDefinitionsRepository, item, quantity, pickList))
            {
                var result = frm.ShowDialog();
                Show();
                //   Task.Run(() => _deviceManager.Reset());
                //    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
            }
        }


        private void MBPrintRackDocument_Click(object sender, EventArgs e)
        {

            PrintPickList(_rackStation);
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void PrintPickList(Station station)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (!orders.Any()) return;
            foreach (var order in orders)
            {
                var orderDetails = order.ReplenOrderDetails.Where(r =>
                    r.StationNumber == station.StationNumber && r.LineStatusId != (int)LineStatus.Complete).ToList();

                foreach (var orderDetail in orderDetails)
                {
                    if (orderDetail == null) continue;
                    orderDetail.LineStatusId = (int)OrderStatus.Picking;

                    _repoReplenOrderDetails.Update(orderDetail);
                }

                order.OrderStatusId = (int)OrderStatus.Picking;
                _repoReplenOrder.Update(order);
                PrintPickListByStation(order.Id, station);
            }
        }

        private void PrintPickListByStation(int orderId, Station station)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickListByStation(orderId, station);
            _documentToPrint.PrintReplenList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private List<PickList> GetPickListByStation(int orderId, Station station)
        {
            var outs = new List<PickList>();

            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndStationNotCompleted(orderId, station.StationNumber);
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
                        Station = station.StationNumber.ToString(),
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
                            Station = station.StationNumber.ToString(),
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
                        Station = station.StationNumber.ToString(),
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

        private List<PickList> GetPickListByStationAdjust(int orderId, Station station)
        {
            var outs = new List<PickList>();
            var details = _orderDetailsRepository.GetOrderDetailsByOrderAndStationNotCompleted(orderId, station.StationNumber);
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
                        Station = station.StationNumber.ToString(),
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
                        Station = station.StationNumber.ToString(),
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

        private void MBAdjustOrder_Click(object sender, EventArgs e)
        {
            var orders = GetSelectedOrders(DataGridViewAvailableOrdersRack);
            if (orders.Any())
            {
                ShowOrderDetailsByOrderAndStation(orders.First(), _rackStation);
            }
        }

        private void ShowOrderDetailsByOrderAndStation(ReplenOrder order, Station station)
        {
            var details = GetPickListByStationAdjust(order.Id, station);
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

        //private List<ReplenOrder> GetCheckedOrdersRack()
        //{
        //    var orders = new List<ReplenOrder>();
        //    var ids = GetCheckedOrderIds(DataGridViewAvailableOrdersRack);
        //    foreach (var id in ids)
        //    {
        //        var order = _repoReplenOrder.AllInclude(o => o.ReplenOrderDetails).FirstOrDefault(r => r.Id == id);
        //        if (order != null)
        //        {
        //            orders.Add(order);
        //        }
        //    }

        //    if (!orders.Any())
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orders;
        //}

        //private List<int> GetAllOrderIds(DataGridView grid)
        //{
        //    var orderIds = new List<int>();

        //    var rows = grid.Rows
        //        .OfType<DataGridViewRow>()
        //        .Where(row => !row.IsNewRow)
        //        .ToArray();
        //    foreach (var row in rows)
        //    {
        //        orderIds.Add((int)row.Cells["Id"].Value);
        //    }

        //    if (!orderIds.Any())
        //    {
        //        MessageBox.Show(text: "No Jobs Selected.");
        //    }

        //    return orderIds;
        //}

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
                GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreRack, orderDetail);

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

            var details = GetPickListByStationAdjust(orderId, _rackStation);
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
    }
}
