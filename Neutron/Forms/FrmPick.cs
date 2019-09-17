using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Equin.ApplicationFramework;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Classes;
using Neutron.Enums;
using Neutron.Global;
using Neutron.Models;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronData.SqlModelViews;
using NeutronLoader;
using PrintRequest;
using NeutronCore.Enums;
using Neutron.Interfaces;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using Neutron.Controllers;
using Neutron.Extensions;
using NeutronEvents;
//using IntegerExtensions = NeutronCore.Extensions.IntegerExtensions;
using static NeutronCore.Extensions.IntegerExtensions;
using Timer = System.Threading.Timer;

namespace Neutron.Forms
{
    public partial class FrmPick : MetroForm
    {
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly InventoryRepository _repoInv = new InventoryRepository();
        private readonly GenericRepository<LocationCount> _repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly OrdersRepository _ordersRepository = new OrdersRepository();
        private readonly OrderDetailsRepository _orderDetailsRepository = new OrderDetailsRepository();
        private readonly GenericRepository<PrintJob> _repoPrintJob = new GenericRepository<PrintJob>(new NeutronDb());
        private readonly LocationsRepository _locationsRepository = new LocationsRepository();
        private readonly StationRepository _stationRepository = new StationRepository();

        private readonly BindingSource _bindingSourceCompleted = new BindingSource();
        private readonly BindingSource _bindingSourceOrderView = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrders = new BindingSource();
        private readonly BindingSource _bindingSourceAvailableOrdersRack = new BindingSource();
        private readonly BindingSource _bindingSourcePickViews = new BindingSource();
        private readonly BindingSource _bindingSourcePickStops = new BindingSource();
        private readonly BindingSource _bindingSourceHot = new BindingSource();
        private readonly BindingSource _bindingSourceOrderDetailsView = new BindingSource();
        private readonly BindingSource _bindingSourceSkipView = new BindingSource();
        //New Order
        private readonly BindingSource _bindingSourceItems = new BindingSource();
        private BindingSource _bindingSourceNewItems = new BindingSource();
        //----
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;

        public bool CloseButtonPressed { get; set; }
        public OrderView CurrentItem;
        public RackOrderView CurrentRackItem;
        private AvailableOrdersView _currentAvailableOrdersViewRack;
        private AvailableOrdersView _currentAvailableOrdersView;
        public TextBox CurrentTextBoxPos;
        public bool ManualOverrideCurrentTextBoxPos;
        private List<BatchPosition> _ordersToPick = new List<BatchPosition>();
        private PickStop _currentPickStop = new PickStop();
        private SqlInventoryView _currentInventoryView = new SqlInventoryView();
        private bool _openHotPickFromPickScreen;
        private bool _openHotStoreFromPickScreen;
        private bool _showSkipped;
        private bool _shortPick;
        static Timer _timer;

        private InterfaceProcessor _interfaceProcessor;
        readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;

        private string _imagesDirectory;
        private DeviceManager _deviceManager;
        private DocumentPrinterPreferences _documentPrinter;
        private LabelPrinterPreferences _labelPrinter;

        private readonly IJsonData _jsonData;
        private readonly StationView _station;
        private readonly IAkaRepository _akaRepository;
        private readonly INomenclature _nomenclature;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly ILacProcessor _lacProcessor;
        private DynamicLogger _logger;
        private CurrentDataSet _currentDataSet;

        public delegate void UpdateTextBoxDelegate1(ResponseInfo responseInfo);
        public delegate void UpdateTextBoxDelegate2(ResponseInfo responseInfo);

        public delegate void UpdateListBoxDelegate(byte[] request);

        public FrmPick(IJsonData jsonData, StationView station
            , IAkaRepository akaRepository, INomenclature nomenclature
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);

            _station = station;
            _jsonData = jsonData;
            _nomenclature = nomenclature;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _lacProcessor = lacProcessor;
            _akaRepository = akaRepository;
            _securityProcessor = securityProcessor;
            InitForm();
        }

        private void InitForm()
        {
            KeyPreview = true;
            SetupLogger();
            _logger.Log($"Form Pick Company Code: {_neutronLicense.CompanyCode}");
            if (_station.StationNumber == 10)
            {
                MBMainLoadOrders.Visible = true;
            }
            UpdateNomenclature();
            SetupPrinters();
            SetupGrids();
            SetupPickPositions(_neutronVariables.PickBatchSize);
            HideTabControlTabs();
            ShowButtons();
            SetLoaderButtonText();
            mlUserInfo.Text = GlobalVar.User?.UserInfo;
            CloseButtonPressed = false;
            CurrentTextBoxPos = TextBoxPos1;
            ToolTipPickScreen.SetToolTip(ButtonMove, _resourceManager.GetString("GetBin"));
            if (_neutronVariables.IptiDisplays)
            {
                GlobalVar.Displays.MySerialDataReceived -= ProcessDataReceived;
                GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
            }
            ComboBoxStationNumber.SelectedIndex = 5;
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            InitDataGridViewNewItems();
            _imagesDirectory = LoaderSettings.GetImagesDirectory();
            MBPickScreenHotPick.Enabled = _securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions];
            if (_station.StationNumber >= 8) MBMainAvailableOrders.Text = "Off Carousel";
            //var logFilePath = LoaderSettings.GetLogFileDirectory() + "FrmPick.log";
            // _locationsRepository = new LocationsRepository();
            //Mediator.GetInstance().IptiButtonPressed += (s, e) => SetFocus(e.ResponseInfo);
            Mediator.GetInstance().IptiButtonPressed += (s, e) => IptiButtonPickAccept(e.ResponseInfo);
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
            Mediator.GetInstance().OrderComplete += (s, e) => ShowOrderComplete(e.Order);
            // Mediator.GetInstance().SerialPortWrite += (s, e) => ShowCommand(e.Request);

        }

        private void ShowCommand(string request)
        {
            //if (IsHandleCreated)
            //{
            //    ListBoxRequests.Invoke(new Action(() => ListBoxRequests.Items.Add(request)));
            //}

        }

        private void ShowOrderComplete(Order order)
        {
            Task.Run(() => _logger.Log($"Show Order Complete Event: Order Number _ {order.Ord1} -- {order.Ord2}"));
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId != order.Id) continue;
                string pos = bp.PositionNumber.ToString();
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
                        this.LabelPickPos1.BackColor = System.Drawing.Color.RoyalBlue;
                        this.LabelPickPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos1.ForeColor = System.Drawing.SystemColors.ControlText;
                        this.LabelPickPos1.Location = new System.Drawing.Point(77, 107);
                        this.LabelPickPos1.Name = "LabelPickPos1";
                        this.LabelPickPos1.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos1.TabIndex = 120;
                        this.LabelPickPos1.Text = "1";
                        this.LabelPickPos1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        // 
                        // LabelPickPos2
                        // 
                        this.LabelPickPos2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos2.Location = new System.Drawing.Point(267, 107);
                        this.LabelPickPos2.Name = "LabelPickPos2";
                        this.LabelPickPos2.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos2.TabIndex = 122;
                        this.LabelPickPos2.Text = "2";
                        this.LabelPickPos2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPickPos2.Visible = false;
                        // 
                        // LabelPickPos3
                        // 
                        this.LabelPickPos3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos3.Location = new System.Drawing.Point(457, 107);
                        this.LabelPickPos3.Name = "LabelPickPos3";
                        this.LabelPickPos3.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos3.TabIndex = 124;
                        this.LabelPickPos3.Text = "3";
                        this.LabelPickPos3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPickPos3.Visible = false;
                        // 
                        // LabelPickPos4
                        // 
                        this.LabelPickPos4.BackColor = System.Drawing.Color.RoyalBlue;
                        this.LabelPickPos4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos4.Location = new System.Drawing.Point(647, 107);
                        this.LabelPickPos4.Name = "LabelPickPos4";
                        this.LabelPickPos4.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos4.TabIndex = 126;
                        this.LabelPickPos4.Text = "4";
                        this.LabelPickPos4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPickPos4.Visible = false;
                        // 
                        // LabelPickPos5
                        // 
                        this.LabelPickPos5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos5.Location = new System.Drawing.Point(837, 107);
                        this.LabelPickPos5.Name = "LabelPickPos5";
                        this.LabelPickPos5.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos5.TabIndex = 128;
                        this.LabelPickPos5.Text = "5";
                        this.LabelPickPos5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPickPos5.Visible = false;
                        // 
                        // LabelPickPos6
                        // 
                        this.LabelPickPos6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPickPos6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPickPos6.Location = new System.Drawing.Point(1027, 107);
                        this.LabelPickPos6.Name = "LabelPickPos6";
                        this.LabelPickPos6.Size = new System.Drawing.Size(36, 26);
                        this.LabelPickPos6.TabIndex = 130;
                        this.LabelPickPos6.Text = "6";
                        this.LabelPickPos6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPickPos6.Visible = false;
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
                        this.LabelPickPos7.Visible = false;
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
                        this.LabelPickPos8.Visible = false;

                        this.LabelPickPos9.Visible = false;
                        this.LabelPickPos10.Visible = false;
                        this.LabelPickPos11.Visible = false;
                        this.LabelPickPos12.Visible = false;
                        this.LabelPickPos13.Visible = false;
                        this.LabelPickPos14.Visible = false;
                        this.LabelPickPos15.Visible = false;
                        this.LabelPickPos16.Visible = false;

                        // 
                        // TextBoxPickPos1
                        // 
                        this.TextBoxPickPos1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos1.Location = new System.Drawing.Point(28, 139);
                        this.TextBoxPickPos1.Multiline = true;
                        this.TextBoxPickPos1.Name = "TextBoxPickPos1";
                        this.TextBoxPickPos1.ReadOnly = true;
                        this.TextBoxPickPos1.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos1.TabIndex = 121;
                        this.TextBoxPickPos1.Tag = "0";
                        this.TextBoxPickPos1.Text = "";
                        this.TextBoxPickPos1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        // 
                        // TextBoxPickPos2
                        // 
                        this.TextBoxPickPos2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos2.Location = new System.Drawing.Point(218, 139);
                        this.TextBoxPickPos2.Multiline = true;
                        this.TextBoxPickPos2.Name = "TextBoxPickPos2";
                        this.TextBoxPickPos2.ReadOnly = true;
                        this.TextBoxPickPos2.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos2.TabIndex = 123;
                        this.TextBoxPickPos2.Tag = "1";
                        this.TextBoxPickPos2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPickPos2.Visible = false;
                        // 
                        // TextBoxPickPos3
                        // 
                        this.TextBoxPickPos3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos3.Location = new System.Drawing.Point(408, 139);
                        this.TextBoxPickPos3.Multiline = true;
                        this.TextBoxPickPos3.Name = "TextBoxPickPos3";
                        this.TextBoxPickPos3.ReadOnly = true;
                        this.TextBoxPickPos3.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos3.TabIndex = 125;
                        this.TextBoxPickPos3.Tag = "2";
                        this.TextBoxPickPos3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPickPos3.Visible = false;
                        // 
                        // TextBoxPickPos4
                        // 
                        this.TextBoxPickPos4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos4.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos4.Location = new System.Drawing.Point(598, 139);
                        this.TextBoxPickPos4.Multiline = true;
                        this.TextBoxPickPos4.Name = "TextBoxPickPos4";
                        this.TextBoxPickPos4.ReadOnly = true;
                        this.TextBoxPickPos4.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos4.TabIndex = 127;
                        this.TextBoxPickPos4.Tag = "3";
                        this.TextBoxPickPos4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPickPos4.Visible = false;
                        // 
                        // TextBoxPickPos5
                        // 
                        this.TextBoxPickPos5.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos5.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos5.Location = new System.Drawing.Point(788, 139);
                        this.TextBoxPickPos5.Multiline = true;
                        this.TextBoxPickPos5.Name = "TextBoxPickPos5";
                        this.TextBoxPickPos5.ReadOnly = true;
                        this.TextBoxPickPos5.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos5.TabIndex = 129;
                        this.TextBoxPickPos5.Tag = "4";
                        this.TextBoxPickPos5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPickPos5.Visible = false;
                        // 
                        // TextBoxPickPos6
                        // 
                        this.TextBoxPickPos6.BackColor = System.Drawing.SystemColors.ButtonHighlight;
                        this.TextBoxPickPos6.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPickPos6.Location = new System.Drawing.Point(978, 139);
                        this.TextBoxPickPos6.Multiline = true;
                        this.TextBoxPickPos6.Name = "TextBoxPickPos6";
                        this.TextBoxPickPos6.ReadOnly = true;
                        this.TextBoxPickPos6.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPickPos6.TabIndex = 131;
                        this.TextBoxPickPos6.Tag = "5";
                        this.TextBoxPickPos6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPickPos6.Visible = false;
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
                        this.TextBoxPickPos7.Visible = false;
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
                        this.TextBoxPickPos8.Visible = false;
                        this.TextBoxPickPos9.Visible = false;
                        this.TextBoxPickPos10.Visible = false;
                        this.TextBoxPickPos11.Visible = false;
                        this.TextBoxPickPos12.Visible = false;
                        this.TextBoxPickPos13.Visible = false;
                        this.TextBoxPickPos14.Visible = false;
                        this.TextBoxPickPos15.Visible = false;
                        this.TextBoxPickPos16.Visible = false;

                        // 
                        // Pos1Display
                        // 
                        this.Pos1Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos1Display.Location = new System.Drawing.Point(20, 139);
                        this.Pos1Display.Name = "Pos1Display";
                        this.Pos1Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos1Display.TabIndex = 153;
                        // 
                        // Pos2Display
                        // 
                        this.Pos2Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos2Display.Location = new System.Drawing.Point(210, 139);
                        this.Pos2Display.Name = "Pos2Display";
                        this.Pos2Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos2Display.TabIndex = 154;
                        this.Pos2Display.Visible = false;
                        // 
                        // Pos3Display
                        // 
                        this.Pos3Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos3Display.Location = new System.Drawing.Point(400, 139);
                        this.Pos3Display.Name = "Pos3Display";
                        this.Pos3Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos3Display.TabIndex = 155;
                        this.Pos3Display.Visible = false;
                        // 
                        // Pos4Display
                        // 
                        this.Pos4Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos4Display.Location = new System.Drawing.Point(590, 139);
                        this.Pos4Display.Name = "Pos4Display";
                        this.Pos4Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos4Display.TabIndex = 156;
                        this.Pos4Display.Visible = false;
                        // 
                        // Pos5Display
                        // 
                        this.Pos5Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos5Display.Location = new System.Drawing.Point(780, 139);
                        this.Pos5Display.Name = "Pos5Display";
                        this.Pos5Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos5Display.TabIndex = 157;
                        this.Pos5Display.Visible = false;
                        // 
                        // Pos6Display
                        // 
                        this.Pos6Display.BackColor = System.Drawing.Color.Transparent;
                        this.Pos6Display.Location = new System.Drawing.Point(970, 139);
                        this.Pos6Display.Name = "Pos6Display";
                        this.Pos6Display.Size = new System.Drawing.Size(150, 53);
                        this.Pos6Display.TabIndex = 158;
                        this.Pos6Display.Visible = false;
                        // 
                        // Pos7Display
                        // 
                        //this.Pos7Display.BackColor = System.Drawing.Color.Transparent;
                        //this.Pos7Display.Location = new System.Drawing.Point(856, 44);
                        //this.Pos7Display.Name = "Pos7Display";
                        //this.Pos7Display.Size = new System.Drawing.Size(128, 53);
                        //this.Pos7Display.TabIndex = 162;
                        this.Pos7Display.Visible = false;

                        // 
                        // Pos8Display
                        // 
                        //this.Pos8Display.BackColor = System.Drawing.Color.Transparent;
                        //this.Pos8Display.Location = new System.Drawing.Point(997, 44);
                        //this.Pos8Display.Name = "Pos8Display";
                        //this.Pos8Display.Size = new System.Drawing.Size(128, 53);
                        //this.Pos8Display.TabIndex = 165;
                        this.Pos8Display.Visible = false;
                        this.Pos9Display.Visible = false;
                        this.Pos10Display.Visible = false;
                        this.Pos11Display.Visible = false;
                        this.Pos12Display.Visible = false;
                        this.Pos13Display.Visible = false;
                        this.Pos14Display.Visible = false;
                        this.Pos15Display.Visible = false;
                        this.Pos16Display.Visible = false;

                        //-----------------Induction Screen ----------------------
                        // 
                        // LabelPos1
                        // 
                        this.LabelPos1.BackColor = System.Drawing.Color.RoyalBlue;
                        this.LabelPos1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos1.ForeColor = System.Drawing.SystemColors.ControlText;
                        this.LabelPos1.Location = new System.Drawing.Point(77, 107);
                        this.LabelPos1.Name = "LabelPos1";
                        this.LabelPos1.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos1.TabIndex = 151;
                        this.LabelPos1.Text = "1";
                        this.LabelPos1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        // 
                        // LabelPos2
                        // 
                        this.LabelPos2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos2.Location = new System.Drawing.Point(267, 107);
                        this.LabelPos2.Name = "LabelPos2";
                        this.LabelPos2.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos2.TabIndex = 152;
                        this.LabelPos2.Text = "2";
                        this.LabelPos2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPos2.Visible = false;
                        // 
                        // LabelPos3
                        // 
                        this.LabelPos3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos3.Location = new System.Drawing.Point(457, 107);
                        this.LabelPos3.Name = "LabelPos3";
                        this.LabelPos3.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos3.TabIndex = 153;
                        this.LabelPos3.Text = "3";
                        this.LabelPos3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPos3.Visible = false;
                        // 
                        // LabelPos4
                        // 
                        this.LabelPos4.BackColor = System.Drawing.Color.RoyalBlue;
                        this.LabelPos4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos4.Location = new System.Drawing.Point(647, 107);
                        this.LabelPos4.Name = "LabelPos4";
                        this.LabelPos4.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos4.TabIndex = 154;
                        this.LabelPos4.Text = "4";
                        this.LabelPos4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPos4.Visible = false;
                        // 
                        // LabelPos5
                        // 
                        this.LabelPos5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos5.Location = new System.Drawing.Point(837, 107);
                        this.LabelPos5.Name = "LabelPos5";
                        this.LabelPos5.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos5.TabIndex = 155;
                        this.LabelPos5.Text = "5";
                        this.LabelPos5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPos5.Visible = false;
                        // 
                        // LabelPos6
                        // 
                        this.LabelPos6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                        this.LabelPos6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.LabelPos6.Location = new System.Drawing.Point(1027, 107);
                        this.LabelPos6.Name = "LabelPos6";
                        this.LabelPos6.Size = new System.Drawing.Size(36, 26);
                        this.LabelPos6.TabIndex = 156;
                        this.LabelPos6.Text = "6";
                        this.LabelPos6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                        this.LabelPos6.Visible = false;
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
                        this.LabelPos7.Visible = false;
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
                        this.LabelPos8.Visible = false;
                        this.LabelPos9.Visible = false;
                        this.LabelPos10.Visible = false;
                        this.LabelPos11.Visible = false;
                        this.LabelPos12.Visible = false;
                        this.LabelPos13.Visible = false;
                        this.LabelPos14.Visible = false;
                        this.LabelPos15.Visible = false;
                        this.LabelPos16.Visible = false;
                        // 
                        // TextBoxPos1
                        // 
                        this.TextBoxPos1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos1.Location = new System.Drawing.Point(28, 139);
                        this.TextBoxPos1.Name = "TextBoxPos1";
                        this.TextBoxPos1.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos1.TabIndex = 0;
                        this.TextBoxPos1.Tag = "0";
                        this.TextBoxPos1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos1.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos1.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos1.Leave += new System.EventHandler(this.TextBoxPosLeave);
                        // 
                        // TextBoxPos2
                        // 
                        this.TextBoxPos2.BackColor = System.Drawing.SystemColors.Control;
                        this.TextBoxPos2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos2.Location = new System.Drawing.Point(218, 139);
                        this.TextBoxPos2.Name = "TextBoxPos2";
                        this.TextBoxPos2.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos2.TabIndex = 1;
                        this.TextBoxPos2.Tag = "1";
                        this.TextBoxPos2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos2.Visible = false;
                        this.TextBoxPos2.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos2.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos2.Leave += new System.EventHandler(this.TextBoxPosLeave);
                        // 
                        // TextBoxPos3
                        // 
                        this.TextBoxPos3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos3.Location = new System.Drawing.Point(408, 139);
                        this.TextBoxPos3.Name = "TextBoxPos3";
                        this.TextBoxPos3.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos3.TabIndex = 2;
                        this.TextBoxPos3.Tag = "2";
                        this.TextBoxPos3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos3.Visible = false;
                        this.TextBoxPos3.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos3.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos3.Leave += new System.EventHandler(this.TextBoxPosLeave);
                        // 
                        // TextBoxPos4
                        // 
                        this.TextBoxPos4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos4.Location = new System.Drawing.Point(598, 139);
                        this.TextBoxPos4.Name = "TextBoxPos4";
                        this.TextBoxPos4.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos4.TabIndex = 3;
                        this.TextBoxPos4.Tag = "3";
                        this.TextBoxPos4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos4.Visible = false;
                        this.TextBoxPos4.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos4.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos4.Leave += new System.EventHandler(this.TextBoxPosLeave);
                        // 
                        // TextBoxPos5
                        // 
                        this.TextBoxPos5.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos5.Location = new System.Drawing.Point(788, 139);
                        this.TextBoxPos5.Name = "TextBoxPos5";
                        this.TextBoxPos5.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos5.TabIndex = 4;
                        this.TextBoxPos5.Tag = "4";
                        this.TextBoxPos5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos5.Visible = false;
                        this.TextBoxPos5.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos5.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos5.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos5.Leave += new System.EventHandler(this.TextBoxPosLeave);
                        // 
                        // TextBoxPos6
                        // 
                        this.TextBoxPos6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        this.TextBoxPos6.Location = new System.Drawing.Point(978, 139);
                        this.TextBoxPos6.Name = "TextBoxPos6";
                        this.TextBoxPos6.Size = new System.Drawing.Size(134, 44);
                        this.TextBoxPos6.TabIndex = 5;
                        this.TextBoxPos6.Tag = "5";
                        this.TextBoxPos6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                        this.TextBoxPos6.Visible = false;
                        this.TextBoxPos6.Click += new System.EventHandler(this.TextBoxPos_Click);
                        this.TextBoxPos6.Enter += new System.EventHandler(this.TextBoxEnter);
                        this.TextBoxPos6.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxPosKeyDown);
                        this.TextBoxPos6.Leave += new System.EventHandler(this.TextBoxPosLeave);
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
                        this.TextBoxPos7.Visible = false;
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
                        this.TextBoxPos8.Visible = false;
                        this.TextBoxPos9.Visible = false;
                        this.TextBoxPos10.Visible = false;
                        this.TextBoxPos11.Visible = false;
                        this.TextBoxPos12.Visible = false;
                        this.TextBoxPos13.Visible = false;
                        this.TextBoxPos14.Visible = false;
                        this.TextBoxPos15.Visible = false;
                        this.TextBoxPos16.Visible = false;


                        // 
                        // AvailablePos1Display
                        // 
                        // this.AvailablePos1Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos1Display.Location = new System.Drawing.Point(20, 139);
                        //  this.AvailablePos1Display.Name = "Pos1Display";
                        this.AvailablePos1Display.Size = new System.Drawing.Size(150, 53);
                        //  this.AvailablePos1Display.TabIndex = 161;
                        this.AvailablePos1Display.Visible = true;
                        // 
                        // AvailablePos2Display
                        // 
                        // this.AvailablePos2Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos2Display.Location = new System.Drawing.Point(210, 139);
                        // this.AvailablePos2Display.Name = "Pos2Display";
                        this.AvailablePos2Display.Size = new System.Drawing.Size(150, 53);
                        // this.AvailablePos2Display.TabIndex = 162;
                        this.AvailablePos2Display.Visible = true;
                        // 
                        // AvailablePos3Display
                        // 
                        //  this.AvailablePos3Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos3Display.Location = new System.Drawing.Point(400, 139);
                        //   this.AvailablePos3Display.Name = "Pos3Display";
                        this.AvailablePos3Display.Size = new System.Drawing.Size(150, 53);
                        //  this.AvailablePos3Display.TabIndex = 163;
                        this.AvailablePos3Display.Visible = true;
                        // 
                        // AvailablePos4Display
                        // 
                        //  this.AvailablePos4Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos4Display.Location = new System.Drawing.Point(590, 139);
                        //  this.AvailablePos4Display.Name = "Pos4Display";
                        this.AvailablePos4Display.Size = new System.Drawing.Size(150, 53);
                        //  this.AvailablePos4Display.TabIndex = 164;
                        this.AvailablePos4Display.Visible = true;
                        // 
                        // AvailablePos5Display
                        // 
                        // this.AvailablePos5Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos5Display.Location = new System.Drawing.Point(780, 139);
                        //  this.AvailablePos5Display.Name = "Pos5Display";
                        this.AvailablePos5Display.Size = new System.Drawing.Size(150, 53);
                        // this.AvailablePos5Display.TabIndex = 165;
                        this.AvailablePos5Display.Visible = true;
                        // 
                        // AvailablePos6Display
                        // 
                        // this.AvailablePos6Display.BackColor = System.Drawing.Color.Transparent;
                        this.AvailablePos6Display.Location = new System.Drawing.Point(970, 139);
                        // this.AvailablePos6Display.Name = "Pos6Display";
                        this.AvailablePos6Display.Size = new System.Drawing.Size(150, 53);
                        //  this.AvailablePos6Display.TabIndex = 166;
                        this.AvailablePos6Display.Visible = true;
                        // 
                        // AvailablePos7Display
                        // 
                        //this.AvailablePos7Display.BackColor = System.Drawing.Color.Transparent;
                        //this.AvailablePos7Display.Location = new System.Drawing.Point(569, 590);
                        //this.AvailablePos7Display.Name = "Pos7Display";
                        //this.AvailablePos7Display.Size = new System.Drawing.Size(126, 53);
                        //this.AvailablePos7Display.TabIndex = 173;
                        this.AvailablePos7Display.Visible = false;
                        // 
                        // AvailablePos8Display
                        // 
                        //this.AvailablePos8Display.BackColor = System.Drawing.Color.Transparent;
                        //this.AvailablePos8Display.Location = new System.Drawing.Point(709, 590);
                        //this.AvailablePos8Display.Name = "Pos8Display";
                        //this.AvailablePos8Display.Size = new System.Drawing.Size(126, 53);
                        //this.AvailablePos8Display.TabIndex = 174;
                        this.AvailablePos8Display.Visible = false;
                        this.AvailablePos9Display.Visible = false;
                        this.AvailablePos10Display.Visible = false;
                        this.AvailablePos11Display.Visible = false;
                        this.AvailablePos12Display.Visible = false;
                        this.AvailablePos13Display.Visible = false;
                        this.AvailablePos14Display.Visible = false;
                        this.AvailablePos15Display.Visible = false;
                        this.AvailablePos16Display.Visible = false;

                        break;
                    }
                case 8:
                    {
                        // 
                        // LabelPickPos1
                        // 
                        this.LabelPickPos9.Visible = false;
                        this.LabelPickPos10.Visible = false;
                        this.LabelPickPos11.Visible = false;
                        this.LabelPickPos12.Visible = false;
                        this.LabelPickPos13.Visible = false;
                        this.LabelPickPos14.Visible = false;
                        this.LabelPickPos15.Visible = false;
                        this.LabelPickPos16.Visible = false;

                        // 
                        // TextBoxPickPos1
                        // 

                        this.TextBoxPickPos9.Visible = false;
                        this.TextBoxPickPos10.Visible = false;
                        this.TextBoxPickPos11.Visible = false;
                        this.TextBoxPickPos12.Visible = false;
                        this.TextBoxPickPos13.Visible = false;
                        this.TextBoxPickPos14.Visible = false;
                        this.TextBoxPickPos15.Visible = false;
                        this.TextBoxPickPos16.Visible = false;

                        // 
                        // Pos1Display
                        // 

                        this.Pos9Display.Visible = false;
                        this.Pos10Display.Visible = false;
                        this.Pos11Display.Visible = false;
                        this.Pos12Display.Visible = false;
                        this.Pos13Display.Visible = false;
                        this.Pos14Display.Visible = false;
                        this.Pos15Display.Visible = false;
                        this.Pos16Display.Visible = false;

                        //-----------------Induction Screen ----------------------
                        // 
                        // LabelPos1
                        // 

                        this.LabelPos9.Visible = false;
                        this.LabelPos10.Visible = false;
                        this.LabelPos11.Visible = false;
                        this.LabelPos12.Visible = false;
                        this.LabelPos13.Visible = false;
                        this.LabelPos14.Visible = false;
                        this.LabelPos15.Visible = false;
                        this.LabelPos16.Visible = false;

                        // 
                        // TextBoxPos1
                        // 

                        this.TextBoxPos9.Visible = false;
                        this.TextBoxPos10.Visible = false;
                        this.TextBoxPos11.Visible = false;
                        this.TextBoxPos12.Visible = false;
                        this.TextBoxPos13.Visible = false;
                        this.TextBoxPos14.Visible = false;
                        this.TextBoxPos15.Visible = false;
                        this.TextBoxPos16.Visible = false;

                        // 
                        // AvailablePos1Display
                        // 

                        this.AvailablePos9Display.Visible = false;
                        this.AvailablePos10Display.Visible = false;
                        this.AvailablePos11Display.Visible = false;
                        this.AvailablePos12Display.Visible = false;
                        this.AvailablePos13Display.Visible = false;
                        this.AvailablePos14Display.Visible = false;
                        this.AvailablePos15Display.Visible = false;
                        this.AvailablePos16Display.Visible = false;

                        break;
                    }
            }
        }


        private void SetLoaderButtonText()
        {
            if (GlobalVar.LoaderRunning)
            {
                MBMainLoadOrders.Text = "Stop Loader";
            }
            else
            {
                MBMainLoadOrders.Text = "Start Loader";
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
                MBMainLoadOrders.Text = "Stop Loader";
                GlobalVar.LoaderRunning = true;
            }
            else
            {
                MBMainLoadOrders.Text = "Start Loader";
                GlobalVar.LoaderRunning = false;
            }
        }

        private void SetFocus(ResponseInfo responseInfo)
        {
            switch (responseInfo.DisplayNumber)
            {
                case "01":
                    UpdateTextBox1(responseInfo);
                    break;
                case "02":
                    UpdateTextBox2(responseInfo);
                    break;
            }
        }

        private void MBMainLoadOrders_Click(object sender, EventArgs e)
        {
            Mediator.GetInstance().OnStartStopLoader(this, !GlobalVar.LoaderRunning ? "Start" : "Stop");
        }

        public void UpdateTextBox1(ResponseInfo responseInfo)
        {
            if (this.TextBoxPos1.InvokeRequired)
            {
                var d = new UpdateTextBoxDelegate1(UpdateTextBox1);
                this.BeginInvoke(d, new object[] { responseInfo });
            }
            else
            {
                _logger.Log($"Update TextBoxPos1 Display Number:  {responseInfo.DisplayNumber}");

                PickAccept();
                // TextBoxPos1.Focus();
                // TextBoxPos1.Text = responseInfo.DisplayNumber;
            }
        }
        public void UpdateTextBox2(ResponseInfo responseInfo)
        {
            if (this.TextBoxPos2.InvokeRequired)
            {
                var d = new UpdateTextBoxDelegate2(UpdateTextBox2);
                this.BeginInvoke(d, new object[] { responseInfo });
            }
            else
            {
                _logger.Log($"Update TextBoxPos2 Display Number:  {responseInfo.DisplayNumber}");
                TextBoxPos2.Focus();
                TextBoxPos2.Text = responseInfo.DisplayNumber;
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

        private void UpdateNomenclature()
        {
            MBPickAccept.Text = _nomenclature.MBPickAccept;
            LabelTray.Text = _nomenclature.LabelTray;
            LabelOver.Text = _nomenclature.LabelOver;
            LabelBack.Text = _nomenclature.LabelBack;
            LabelDevice.Text = _nomenclature.LabelDevice;
        }

        private void FrmPick_Load(object sender, EventArgs e)
        {
            ////Communication Monitoring Form use for TEsting
            //var frmCommunication = new FrmCommunication();
            //frmCommunication.Show();


            if (GlobalVar.LoaderRunning)
            {
                MBMainLoadOrders.Text = _resourceManager.GetString("StopLoader");
            }
        }

        // Set the focus to the passed in recId if it's passed in
        private int ShowAllOrders(int recId = 0)
        {
            Task.Run(() => _logger.Log($"ShowAllOrders Start: [{DateTime.Now.ToString(CultureInfo.CurrentCulture)}]"));
            var idx = 0;
            var findWhat = TextBoxFind.Text.Trim().ToLower();
            // string find = akaRepository.Get(findWhat);
            // TextBoxFind.Text = find;

            if (!string.IsNullOrEmpty(findWhat))
            {
                var views = _ordersRepository.GetOrderViewNotCompleted(findWhat);
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceOrderView.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceOrderView;
            }
            else
            {
                var views = _ordersRepository.GetOrderViewNotCompleted();
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceOrderView.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceOrderView;
            }

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
                //PictureBoxItemImage.Load(@"C:\Images\1121.jpg");
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

            if (!string.IsNullOrEmpty(findWhat))
            {
                var views = _ordersRepository.GetRackOrders(findWhat);
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceOrderView.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceOrderView;
            }
            else
            {
                var views = _ordersRepository.GetRackOrders();
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceOrderView.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceOrderView;
            }

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
                findWhat = TextBoxFindAvailableOrders.Text.Trim().ToLower();
            }

            try
            {
                var views = _ordersRepository.GetRackOrdersView(findWhat);

                var rackOrderViews = views.ToList();
                foreach (var rackOrderView in rackOrderViews)
                {
                    if (rackOrderView.OrderDetails.First().LineStatusId == 3)
                    {
                        rackOrderView.StatusName = "On Floor";
                    }
                }


                //var filteredViews = views.Where(v => v.Station_8_HasPicks != "C").ToList();

                var bindingListView = new BindingListView<RackOrderView>(rackOrderViews.ToList());

                _bindingSourceAvailableOrdersRack.DataSource = bindingListView;
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"ShowAvailableOrdersRack Error: {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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
                var views = _ordersRepository.GetAvailableOrders(_station, findWhat, _neutronVariables.SerialPicking, _showSkipped);

                var bindingListView = new BindingListView<AvailableOrdersView>(views.ToList());

                _bindingSourceAvailableOrders.DataSource = bindingListView;
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"ShowAvailableOrders Error: {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
            }

            DataGridViewAvailableOrders.DataSource = _bindingSourceAvailableOrders;
            // DataGridViewAvailableOrdersRack.DataSource = _bindingSourceAvailableOrders;
            if (GetRecordCount(_bindingSourceAvailableOrders) > 0)
            {
                if (recId != 0)
                {
                    idx = IndexOf(_bindingSourceAvailableOrders, recId);
                    DataGridViewAvailableOrders.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    DataGridViewAvailableOrders.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    DataGridViewAvailableOrders.Rows[idx].Selected = true;
                    // DataGridViewAvailableOrdersRack.FirstDisplayedScrollingRowIndex = DataGridViewAvailableOrders.Rows[idx].Index;
                    //  DataGridViewAvailableOrdersRack.CurrentCell = DataGridViewAvailableOrders.Rows[idx].Cells[1];
                    // DataGridViewAvailableOrdersRack.Rows[idx].Selected = true;
                }
                else
                {
                    DataGridViewAvailableOrders.ClearSelection();
                    DataGridViewAvailableOrders.Update();
                    //   DataGridViewAvailableOrdersRack.ClearSelection();
                    //   DataGridViewAvailableOrdersRack.Update();
                }
                CheckMarkSelectedAvailableOrders();
                ClearTextBoxPosBackColor();
                SetBatchPositionToFirstEmpty();
                DataGridViewAvailableOrders.Refresh();
                //  DataGridViewAvailableOrdersRack.Refresh();

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
                Task.Run(() => _logger.Log($"ShowAvailableOrders Error: {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
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
            var records = _resourceManager.GetString("Records");
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
            var records = _resourceManager.GetString("Records");
            LabelRecordCount.Text = string.Format(format: "{0}: {1}", arg0: records, arg1: count.ToString());
            return count;
        }

        #region Button Clicks


        private void MButtonClose_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = _resourceManager.GetString("Jobs");
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

        #endregion

        private void SetupOrderGrid()
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
                Visible = true
            };
            DataGridView1.Columns.Add(colx);

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString("Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString("Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString("Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _resourceManager.GetString(@"OrderStatusName"),
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
                HeaderText = @"Off",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Station_8_HasPicks",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _resourceManager.GetString(@"Lines"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _resourceManager.GetString(@"Pieces"),
                Name = "Pieces",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString(@"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ShipMethodName",
                HeaderText = _resourceManager.GetString(@"ShipMethod"),
                Visible = false,
                Name = "ShipMethodName"
            };
            DataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridView1.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridView1.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }


        private void SetupSkipGrid()
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
                HeaderText = "St",  // _resourceManager.GetString("Station"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "StationNumber"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString("Ord1"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord1"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString("Ord2"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Ord2"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = "Pri",  // _resourceManager.GetString("Priority"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Priority"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString("Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Name = "Item"
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderStatusName",
                HeaderText = _resourceManager.GetString(@"OrderStatusName"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "OrderStatusName",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Picked",
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                HeaderText = _resourceManager.GetString(@"Picked"),
                Name = "Picked",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString(@"LoadDate"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "LoadDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString(@"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                Visible = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkip.Columns.Add(col);


            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewSkip.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = @"OrderId",
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewSkip.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewSkip.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }

        private void SetupGrids()
        {
            SetupOrderGrid();
            SetupSkipInventoryGrid();
            //*****************************************************************************
            //DataGridPickView

            DataGridPickView.AutoGenerateColumns = false;
            DataGridPickView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridPickView.DefaultCellStyle.ForeColor = Color.Black;
            DataGridPickView.DefaultCellStyle.BackColor = Color.White;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Sequence",
                HeaderText = @"Seq",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Visible = false,
                Name = "Sequence"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickPosition",
                HeaderText = _resourceManager.GetString(@"PickPosition"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "PickPosition",
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString(@"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString(@"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString(@"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Quantity"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _resourceManager.GetString(@"Slot"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Slot"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TotalQuantityInInventory",
                HeaderText = _resourceManager.GetString(@"TotalQuantityInInventory"),
                Name = "TotalQuantityInInventory",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString(@"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ReceivedDate",
                HeaderText = _resourceManager.GetString(@"ReceivedDate"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "ReceivedDate"
            };
            //col.DefaultCellStyle.Format = "{0:dd.MM.yyyy}";
            DataGridPickView.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = @"Job Id",
                Visible = false,
                Name = "OrderId"
            };
            DataGridPickView.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridPickView.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

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
                Visible = true
            };
            DataGridViewAvailableOrders.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString("Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString("Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewAvailableOrders.Columns.Add(col);


            if (_neutronVariables.SerialPicking)  //Show the Starter column
            {
                col = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Starter",
                    HeaderText = _resourceManager.GetString("Starter"),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                    Name = "Starter",
                    Visible = true
                };
                DataGridViewAvailableOrders.Columns.Add(col);

            }
            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString("Priority"),
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
                HeaderText = _resourceManager.GetString("Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _resourceManager.GetString("Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrders.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString("LoadDate"),
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
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewAvailableOrders.Columns.Add(col);


            foreach (DataGridViewColumn column in DataGridViewAvailableOrders.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

            //*****************************************************************************
            //DataGridViewAvailableOrdersRack

            DataGridViewAvailableOrdersRack.AutoGenerateColumns = false;
            DataGridViewAvailableOrdersRack.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAvailableOrdersRack.DefaultCellStyle.BackColor = Color.White;

            colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false,
                Visible = true
            };
            DataGridViewAvailableOrdersRack.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString(@"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString(@"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StatusName",
                HeaderText = @"Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StatusName",
                Visible = true
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Priority",
                HeaderText = _resourceManager.GetString(@"Priority"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "Priority"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Lines",
                HeaderText = _resourceManager.GetString(@"Lines"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Lines"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Pieces",
                HeaderText = _resourceManager.GetString(@"Pieces"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Pieces"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LoadDate",
                HeaderText = _resourceManager.GetString(@"LoadDate"),
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
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewAvailableOrdersRack.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewAvailableOrdersRack.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }


            ////**********************************************************************************************           
            ////DataGridViewInventory

            //DataGridViewInventory.AutoGenerateColumns = false;
            //DataGridViewInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //DataGridViewInventory.DefaultCellStyle.ForeColor = Color.Black;
            //DataGridViewInventory.DefaultCellStyle.BackColor = Color.White;

            //var bCol = new DataGridViewButtonColumn
            //{
            //    HeaderText = @"   ",
            //    Visible = false,
            //    Name = "HotPick",
            //    Text = _resourceManager.GetString(@"HotPick"),
            //    FlatStyle = FlatStyle.Popup,
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    UseColumnTextForButtonValue = true
            //};

            //DataGridViewInventory.Columns.Add(bCol);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Item",
            //    HeaderText = _resourceManager.GetString(@"Item"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Item"
            //};
            //DataGridViewInventory.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Description",
            //    HeaderText = _resourceManager.GetString(@"Description"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
            //    Name = "Description"
            //};
            //DataGridViewInventory.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Quantity",
            //    HeaderText = _resourceManager.GetString(@"Quantity"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Quantity"
            //};
            //DataGridViewInventory.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Slot",
            //    HeaderText = _resourceManager.GetString(@"Slot"),
            //    Name = "Slot",
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            //};
            //DataGridViewInventory.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "ReceivedDate",
            //    HeaderText = _resourceManager.GetString(@"ReceivedDate"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
            //    Name = "ReceivedDate"
            //};
            //col.DefaultCellStyle.Format = "MM-dd-yyyy";

            //DataGridViewInventory.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Id",
            //    HeaderText = "Id",
            //    Visible = false,
            //    Name = "Id"
            //};
            //DataGridViewInventory.Columns.Add(col);

            //foreach (DataGridViewColumn column in DataGridViewInventory.Columns)
            //{
            //    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //    column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            //}


            //********************************************************************
            // DataGridViewNewOrder

            DataGridViewNewOrder.AutoGenerateColumns = false;
            DataGridViewNewOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewOrder.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewOrder.DefaultCellStyle.BackColor = Color.White;

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString("Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString("Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString("Description"),
                Name = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString("Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ItemDefinitionId",
                HeaderText = @"Id",
                Visible = false,
                Name = "ItemDefinitionId"
            };
            DataGridViewNewOrder.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewNewOrder.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

            //********************************************************************
            //DataGridViewOrderDetails

            DataGridViewOrderDetails.AutoGenerateColumns = false;
            DataGridViewOrderDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewOrderDetails.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewOrderDetails.DefaultCellStyle.BackColor = Color.White;
            DataGridViewOrderDetails.ScrollBars = ScrollBars.Both;

            colx = new DataGridViewCheckBoxColumn
            {
                HeaderText = @"   ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "IsChecked",
                TrueValue = true,
                FalseValue = false
            };
            DataGridViewOrderDetails.Columns.Add(colx);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString("Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord1",
                HeaderText = _resourceManager.GetString(@"Ord1"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord1"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Ord2",
                HeaderText = _resourceManager.GetString(@"Ord2"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Ord2"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString(@"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _resourceManager.GetString(@"PickedQuantity"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString(@"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _resourceManager.GetString(@"LineStatus"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "LineStatusName"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderId",
                HeaderText = @"OrderId",
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewOrderDetails.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = @"OrderDetailId",
                Visible = false,
                Name = "OrderDetailId"
            };
            DataGridViewOrderDetails.Columns.Add(col);


            foreach (DataGridViewColumn column in DataGridViewOrderDetails.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }


            //        //********************************************************************
            //        //DataGridViewAdjust
            //
            DataGridViewAdjust.AutoGenerateColumns = false;
            // DataGridViewAdjust.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewAdjust.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewAdjust.DefaultCellStyle.BackColor = Color.White;
            DataGridViewAdjust.ScrollBars = ScrollBars.Both;


            colx = new DataGridViewCheckBoxColumn
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

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString("Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber",
                ReadOnly = true

            };
            DataGridViewAdjust.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Ord1",
            //    HeaderText = _resourceManager.GetString(@"Ord1"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Ord1",
            //    ReadOnly = true
            //};
            //DataGridViewAdjustOrder.Columns.Add(col);

            //col = new DataGridViewTextBoxColumn
            //{
            //    DataPropertyName = "Ord2",
            //    HeaderText = _resourceManager.GetString(@"Ord2"),
            //    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            //    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
            //    Name = "Ord2",
            //    ReadOnly = true
            //};
            //DataGridViewAdjustOrder.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartNum",
                HeaderText = _resourceManager.GetString(@"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "PartNum",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartDesc",
                HeaderText = _resourceManager.GetString(@"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "PartDesc",
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = true
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PickedQuantity",
                HeaderText = _resourceManager.GetString(@"PickedQuantity"),
                Name = "PickedQuantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                ReadOnly = false
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "LineStatusName",
                HeaderText = _resourceManager.GetString(@"LineStatus"),
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
                HeaderText = @"OrderId",
                Visible = false,
                Name = "OrderId"
            };
            DataGridViewAdjust.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OrderDetailId",
                HeaderText = @"OrderDetailId",
                Visible = false,
                Name = "OrderDetailId"
            };
            DataGridViewAdjust.Columns.Add(col);

            DataGridViewAdjust.Columns[5].DefaultCellStyle.Padding = new Padding(0, 0, 20, 0);
            foreach (DataGridViewColumn column in DataGridViewAdjust.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

            //********************************************************************
            //DataGridViewNewItems

            DataGridViewNewItems.AutoGenerateColumns = false;
            DataGridViewNewItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewNewItems.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewNewItems.DefaultCellStyle.BackColor = Color.White;
            DataGridViewNewItems.ScrollBars = ScrollBars.Both;

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString("Station"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                Name = "StationNumber"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString(@"Item"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Item"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString(@"Description"),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description"
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
                Name = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            DataGridViewNewItems.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = @"Id",
                Visible = false,
                Name = "Id"
            };
            DataGridViewNewItems.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewNewItems.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            }

        }

        private void FrmPick_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CloseButtonPressed;
        }

        private void HideTabControlTabs()
        {
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;
        }



        private void MButtonClearSelection_Click(object sender, EventArgs e)
        {
            ClearSelection(DataGridView1);

        }

        private void ClearSelection(DataGridView dataGridView)
        {
            Cursor.Current = Cursors.WaitCursor;
            dataGridView.ClearSelection();
            try
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    var cell = (DataGridViewCheckBoxCell)row.Cells["IsChecked"];

                    if (cell.Value != null)
                    {
                        if (cell.Value.Equals(cell.TrueValue))
                        {
                            cell.Value = cell.FalseValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"ClearSelection Error: {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
            }
            Cursor.Current = Cursors.Default;
        }

        private void MBSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll(DataGridView1);
        }

        private void SelectAll(DataGridView dataGridView)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
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
                Task.Run(() => _logger.Log($"Select All Error: {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]"));
            }
            Cursor.Current = Cursors.Default;
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (e.RowIndex < 0) return;
            var chk = (DataGridViewCheckBoxCell)dgv.Rows[e.RowIndex].Cells[0];
            dgv.Rows[e.RowIndex].Cells[0].Value = chk.Value == chk.TrueValue ? chk.FalseValue : chk.TrueValue;


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
            //var watch = new Stopwatch();
            //watch.Start();

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
            //watch.Stop();
            //Task.Run(() => logger.Log($"DataGridView1_CellFormatting Elapsed MSec:  {watch.ElapsedMilliseconds}ms"));
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
            var recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                foreach (var id in recs)
                {
                    var ord = _repoOrders.FindByKey(id);
                    if (ord.OrderStatusId != 1) continue;
                    ord.OrderStatusId = 2;
                    _repoOrders.Update(ord);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldOrder, ord);
                }
            }
            ShowAllOrders();
        }

        private void MBRelease_Click(object sender, EventArgs e)
        {
            var recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                foreach (var id in recs)
                {
                    var ord = _repoOrders.FindByKey(id);
                    if (ord.OrderStatusId != 2) continue;
                    ord.OrderStatusId = 1;
                    _repoOrders.Update(ord);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseOrder, ord);
                }
            }
            ShowAllOrders();
        }

        private List<int> GetCheckedOrderIds()
        {
            var orderIds = new List<int>();

            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value)
                {
                    orderIds.Add((int)row.Cells["Id"].Value);
                }
            }
            if (!orderIds.Any())
            {
                MessageBox.Show(_resourceManager.GetString("NoJobsSelected"));
            }
            return orderIds;
        }

        private List<OrderView> GetCheckedOrders()
        {
            var ordViews = new List<OrderView>();
            var orderIds = new List<int>();
            foreach (DataGridViewRow row in DataGridView1.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value)
                {
                    var ordId = (int)row.Cells["Id"].Value;
                    var view = _ordersRepository.GetOrderView().FirstOrDefault(r => r.Id == ordId);
                    if (view != null)
                    {
                        ordViews.Add(view);
                    }
                }
            }
            if (!ordViews.Any())
            {
                MessageBox.Show(_resourceManager.GetString("NoJobsSelected"));
            }
            return ordViews;
        }

        private List<Order> GetCheckedOrdersRack()
        {
            var orders = new List<Order>();
            foreach (DataGridViewRow row in DataGridViewAvailableOrdersRack.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value)
                {
                    var ordId = (int)row.Cells["Id"].Value;
                    var ord = _ordersRepository.GetOrder(ordId);
                    if (ord != null)
                    {
                        orders.Add(ord);
                    }
                }
            }
            if (!orders.Any())
            {
                MessageBox.Show(_resourceManager.GetString("NoJobsSelected"));
            }
            return orders;
        }

        private List<int> GetCheckedAvailableOrderIds()
        {
            var orderIds = new List<int>();

            foreach (DataGridViewRow row in DataGridViewAvailableOrders.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value)
                {
                    orderIds.Add((int)row.Cells["Id"].Value);
                }
            }
            if (!orderIds.Any())
            {
                MessageBox.Show(_resourceManager.GetString("NoJobsSelected"));
            }
            return orderIds;
        }

        private void MBPickListBack_Click(object sender, EventArgs e)
        {
            PickListBack();
        }

        private void PickListBack()
        {
            LabelFormTitle.Text = _resourceManager.GetString("AvailableJobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = AvailableOrders;
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
            AvailableOrdersBack();
        }

        private void AvailableOrdersBack()
        {
            ClearBatchPositions();
            LabelFormTitle.Text = _resourceManager.GetString("Jobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Main;
        }

        private void MBGo_Click(object sender, EventArgs e)
        {
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            var numOrders = _ordersToPick.Where(o => o.OrderId != null).Count();
            if (numOrders > 0)
            // if (GetCheckedAvailableOrderIds().Count > 0)
            {
                Task.Run(() => _logger.Log($"Batch Start: [{DateTime.Now.ToLongTimeString()}]"));

                LabelFormTitle.Text = _resourceManager.GetString("PickList");
                LabelFormTitle.BackColor = Color.RoyalBlue;

                var itemShort = PickListLoad();


                Task.Run(() => _logger.Log($"Call Printing Start: [{DateTime.Now.ToLongTimeString()}]"));

                //  PrintAllToteLabels();

                //   PrintAllDocuments();

                Task.Run(() => _logger.Log($"Call Printing End: [{DateTime.Now.ToLongTimeString()}]"));

                if (itemShort)
                {
                    tabControl1.SelectedTab = PickList;
                    DataGridPickView.ClearSelection();
                    DataGridPickView.Update();
                    Task.Run(() => _logger.Log($"Batch Start End: [{DateTime.Now.ToLongTimeString()}]"));
                }
                else
                {
                    Start();
                }
            }
        }

        private bool PickListLoad()
        {
            var itemShort = false;
            Task.Run(() => _logger.Log($"PickListLoad Start: [{DateTime.Now.ToLongTimeString()}]"));
            TextBoxFindAvailableOrders.Text = string.Empty;
            //            ShowAvailableOrders();
            //int[] orderIds = GetOrderIdArray(ordersToPick);
            // pickviews from bindingSourceAvailableOrders?
            var pickViews = GetPickViews();
            //List<PickView> pickViews = ordersRepository.GetOrderLines(ordersToPick);
            //Add Pick Location based on Inventory
            Task.Run(() => _logger.Log($"Pickviews Count: {pickViews.Count}"));

            var pickableViews = new List<PickView>();

            foreach (var item in pickViews)
            {
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
                    //if (item.OrderDetail.LineStatusId == 9) continue;
                    MessageBox.Show($"No Inventory for Item: {item.Item}." + Environment.NewLine +
                                    "Item will be Skipped.");

                    item.OrderDetail.LineStatusId = 9;
                    _repoOrderDetails.Update(item.OrderDetail);
                }
            }

            if (pickableViews.Count > 0)
            {
                _bindingSourcePickViews.DataSource = pickableViews;
                DataGridPickView.DataSource = _bindingSourcePickViews;
                Task.Run(() => _logger.Log($"bindingSourcePickViews [{DateTime.Now.ToLongTimeString()}] \r\nCount:{_bindingSourcePickViews.Count.ToString()}"));
                GetRecordCount(_bindingSourcePickViews);
                DataGridPickView.ClearSelection();
                DataGridPickView.Update();
                DataGridPickView.ScrollBars = ScrollBars.Both;
            }
            else
            {
                //  MessageBox.Show(_resourceManager.GetString("NothingtoPick."));
                ClearAllSelectOrdersToPick();
                LabelFormTitle.Text = _resourceManager.GetString("AvailableJobs");
                LabelFormTitle.BackColor = Color.RoyalBlue;
                tabControl1.SelectedTab = AvailableOrders;
            }
            foreach (var item in pickableViews)
            {
                if (item.QuantityToBePicked > item.TotalQuantityInInventory)
                {
                    itemShort = true;
                    break;
                }

            }
            Task.Run(() => _logger.Log($"PickListLoad End: [{DateTime.Now.ToLongTimeString()}]"));
            return itemShort;
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
            if (recs.Count > 0)
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

            string prevPartNum = "";

            Task.Run(() => _logger.Log($"GetPickViews Start: [{DateTime.Now.ToLongTimeString()}]"));
            var pickViews = new List<PickView>();
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var itemFound = _bindingSourceAvailableOrders.Find("Id", bp.OrderId);
                _bindingSourceAvailableOrders.Position = itemFound;
                var currentItem = ((ObjectView<AvailableOrdersView>)_bindingSourceAvailableOrders.Current).Object;
                var firstTime = true;
                var counter = 0;
                var details = currentItem.Order.OrderDetails.OrderBy(o => o.PartNum);
                foreach (var detail in details)
                {
                    if (detail.LineStatusId != (int)LineStatus.Available &&
                        detail.LineStatusId != (int)LineStatus.Skipped) continue;
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
                        ItemKey = key
                    };
                    pickViews.Add(pickView);
                }
            }

            //Add Item definition
            foreach (var item in pickViews)
            {
                var def = _repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                if (def != null)
                {
                    item.Item = def.Item;
                    item.Description = def.Description;
                }
            }
            Task.Run(() => _logger.Log($"GetPickViews End: [{DateTime.Now.ToLongTimeString()}]"));
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
                QuantityToBePicked = detail.Quantity,
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
            Task.Run(() => _logger.Log($"GetInventory Start: [{DateTime.Now.ToLongTimeString()}]"));
            var pickableLocations = new[] { 1, 2 };

            var recs = _repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => f.ItemDefinitionId == itemId && pickableLocations.Contains(f.StorageTypeId)).ToList();
            Task.Run(() => _logger.Log($"GetInventory End: [{DateTime.Now.ToLongTimeString()}]"));
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
            if (e.RowIndex < 0) return;
            var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0];

            if (chk.Value == chk.TrueValue)
            {
                DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                var id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                RemoveItemFromBatch(id);
            }
            else
            {
                DataGridViewAvailableOrders.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                var id = Convert.ToInt32(DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Id"].Value);
                if (id <= 0) return;
                var ord1 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                var ord2 = DataGridViewAvailableOrders.Rows[e.RowIndex].Cells["Ord2"].Value.ToString();
                AddItemToBatch(id, ord1, ord2);
            }
        }

        //select Available Rack Orders
        private void DataGridViewAvailableOrdersRack_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                    //if (id > 0)
                    //{
                    //    string ord1 = DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                    //}
                }
            }
        }

        //select Available Rack Orders
        private void DataGridViewSkip_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewSkip.Rows[e.RowIndex].Cells[0];

                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewSkip.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    DataGridViewSkip.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    //int id = Convert.ToInt32(DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Id"].Value);
                    //if (id > 0)
                    //{
                    //    string ord1 = DataGridViewAvailableOrdersRack.Rows[e.RowIndex].Cells["Ord1"].Value.ToString();
                    //}
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
                UpdateTextBoxPosition(bp);
            }
        }

        private int AddItemToBatch(int orderId, string ord1, string ord2)
        {
            var idx = ManualOverrideCurrentTextBoxPos ? SetBatchPositionToManualOverride() : SetBatchPositionToFirstEmpty();

            if (idx >= 0 && idx <= _neutronVariables.PickBatchSize)
            {
                _ordersToPick[idx].OrderId = orderId;
                _ordersToPick[idx].Ord1 = ord1;
                _ordersToPick[idx].Ord2 = ord2;
                _ordersToPick[idx].OrderComplete = false;
                CurrentTextBoxPos.Text = ord1;
            }
            ManualOverrideCurrentTextBoxPos = false;
            ClearTextBoxPosBackColor();
            return idx;
        }

        private int SetBatchPositionToManualOverride()
        {
            var result = -1;
            result = int.Parse(CurrentTextBoxPos.Tag.ToString());
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

        private void SetCurrentTextBoxPos(int batchPositionNumber)
        {
            Control c = Controls.Find($"TextBoxPos{batchPositionNumber}", true).Single() as TextBox;
            if (c != null) CurrentTextBoxPos = (TextBox)c;
            CurrentTextBoxPos.BackColor = Color.Yellow;

            //switch (batchPositionNumber)
            //{
            //    case 1:
            //        CurrentTextBoxPos = TextBoxPos1;
            //        break;
            //    case 2:
            //        CurrentTextBoxPos = TextBoxPos2;
            //        break;
            //    case 3:
            //        CurrentTextBoxPos = TextBoxPos3;
            //        break;
            //    case 4:
            //        CurrentTextBoxPos = TextBoxPos4;
            //        break;
            //    case 5:
            //        CurrentTextBoxPos = TextBoxPos5;
            //        break;
            //    case 6:
            //        CurrentTextBoxPos = TextBoxPos6;
            //        break;
            //    case 7:
            //        CurrentTextBoxPos = TextBoxPos7;
            //        break;
            //    case 8:
            //        CurrentTextBoxPos = TextBoxPos8;
            //        break;
            //    default:
            //        CurrentTextBoxPos = TextBoxPos1;
            //        break;
            //}

        }

        private void UpdateTextBoxPosition(BatchPosition bp)
        {
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition Start: [{DateTime.Now.ToLongTimeString()}]"));
            var orderNumber = bp.Ord1;
            var pos = bp.PositionNumber;

            Control c = Controls.Find($"TextBoxPos{pos}", true).Single() as TextBox;
            if (c != null) c.Text = orderNumber;


            //switch (pos)
            //{
            //    case 1:
            //        TextBoxPos1.Text = orderNumber;
            //        // SendKeys.Send("{TAB}");
            //        break;
            //    case 2:
            //        TextBoxPos2.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos3;
            //        //  SendKeys.Send("{TAB}");
            //        break;
            //    case 3:
            //        TextBoxPos3.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos4;
            //        //  SendKeys.Send("{TAB}");
            //        break;
            //    case 4:
            //        TextBoxPos4.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos5;
            //        //  SendKeys.Send("{TAB}");
            //        break;
            //    case 5:
            //        TextBoxPos5.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos6;
            //        //   SendKeys.Send("{TAB}");
            //        break;
            //    case 6:
            //        TextBoxPos6.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos7;
            //        //    SendKeys.Send("{TAB}");
            //        break;
            //    case 7:
            //        TextBoxPos7.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos8;
            //        //    SendKeys.Send("{TAB}");
            //        break;
            //    case 8:
            //        TextBoxPos8.Text = orderNumber;
            //        //CurrentTextBoxPos = TextBoxPos1;
            //        //    SendKeys.Send("{TAB}");
            //        break;
            //}
            Task.Run(() => _logger.Log($"UpdateTextBoxPosition End: [{DateTime.Now.ToLongTimeString()}]"));
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
            ClearAllBli();
            foreach (var item in _ordersToPick)
            {
                Task.Run(() => _logger.Log($"ShowOrdersToPick Display Pos: {item.PositionNumber} Order: {item.Ord2}"));
                if (!string.IsNullOrEmpty(item.Ord2))
                {
                    TurnOnBatchPositionDisplay(item.PositionNumber, beacon: 2, text: item.Ord2.Substring(item.Ord2.Length - 4));
                }
            }



            //if (_neutronVariables.DisplaysEnabled)
            //{
            //    if (GlobalVar.Displays != null)
            //    {
            //        if (_neutronVariables.BliEnabled)
            //        {
            //            Task.Run(() => _logger.Log($"ShowOrdersToPick Clear All Bli"));
            //            GlobalVar.Displays.ClearAllBli();
            //            foreach (var item in _ordersToPick)
            //            {
            //                Task.Run(() => _logger.Log($"ShowOrdersToPick Display Pos: {item.PositionNumber} Order: {item.Ord1}"));
            //                if (!string.IsNullOrEmpty(item.Ord1))
            //                {
            //                    TurnOnBatchPositionDisplay(item.PositionNumber, beacon: 2, text: item.Ord1.Substring(item.Ord1.Length - 4));
            //                }
            //            }
            //        }
            //    }
            //}
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
                ShowOrdersToPick();
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowQuantity");
            }
            else
            {
                ClearAllBli();
                SetOrderCompleteThisStation();
                ClearPickPositions();
                ClearPickDisplays();
                MBShowOrderOrQuantityToggle.Text = _resourceManager.GetString($"ShowJobs");
                UpdatePickScreen();
            }
        }

        private void MBPickBack_Click(object sender, EventArgs e)
        {
            PickBack();
        }

        private void PickBack()
        {
            LabelFormTitle.Text = _resourceManager.GetString($"PickList");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = PickList;
            ClearAllShi();
            ClearAllBli();
        }

        private void MBStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void Start()
        {
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
            UpdatePickScreen();
            Task.Run(() => _logger.Log($"Start_Click 4  Run GetFirstStop?: [{DateTime.Now.ToLongTimeString()}]"));
            // PrintAllDocuments();
            // PrintAllToteLabels();

            tabControl1.SelectedTab = PickScreen;

            //feels good to here
            Task.Run(() => _logger.Log($"Start_Click End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private List<PickStop> FinalPickSequence(List<PickStop> pickStops)
        {
            Task.Run(() => _logger.Log($"FinalPickSequence Start: [{DateTime.Now.ToLongTimeString()}]"));
            var newList = new List<PickStop>();
            var car1List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 1)
                .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
                .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
                .ToList();
            var car2List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 2)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();
            var car3List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 3)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();
            var car4List = pickStops.Where(p => p.CurrentInventoryLocation.Location.Loc1 == 4)
               .OrderBy(p => p.CurrentInventoryLocation.Location.Loc2)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc3)
               .ThenBy(p => p.CurrentInventoryLocation.Location.Loc4)
               .ToList();

            newList = BuildNewList(car1List, car2List, car3List, car4List);

            Task.Run(() => _logger.Log($"FinalPickSequence Start Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));

            _deviceManager = new DeviceManager(car1List, car2List, car3List
                , car4List, _neutronVariables.ShuttleEnabled, _logger);

            Task.Run(() => _deviceManager.FirstMoveAsync());

            //Task.Run(() => _deviceManager.MoveNext(1));
            //Task.Run(() => _deviceManager.MoveNext(2));
            //Task.Run(() => _deviceManager.MoveNext(3));
            //Task.Run(() => _deviceManager.MoveNext(4));

            Task.Run(() => _logger.Log($"FinalPickSequence End Carousel Move: [{DateTime.Now.ToLongTimeString()}]"));
            Task.Run(() => _logger.Log($"FinalPickSequence End: [{DateTime.Now.ToLongTimeString()}]"));
            return newList;
        }

        private List<PickStop> BuildNewList(List<PickStop> car1List, List<PickStop> car2List, List<PickStop> car3List, List<PickStop> car4List)
        {
            var car1Index = 0;
            var car2Index = 0;
            var car3Index = 0;
            var car4Index = 0;

            var newList = new List<PickStop>();
            var totalStops = car1List.Count + car2List.Count + car3List.Count + car4List.Count;
            var seq = 1;
            var prevLoc1 = 0;
            var prevLoc2 = 0;
            var currLoc1 = 0;
            var currLoc2 = 0;
            PickStop currentCar;

            while (seq <= totalStops)
            {
                while (car1Index < car1List.Count)
                {
                    currentCar = car1List[car1Index];
                    currLoc1 = currentCar.CurrentInventoryLocation.Location.Loc1;
                    currLoc2 = currentCar.CurrentInventoryLocation.Location.Loc2;

                    if (prevLoc1 == 0 || (prevLoc1 == currLoc1 && prevLoc2 == currLoc2))
                    {
                        currentCar.Sequence = seq;
                        newList.Add(currentCar);
                        car1Index += 1;
                        seq += 1;
                        prevLoc1 = currLoc1;
                        prevLoc2 = currLoc2;
                    }
                    else
                    {
                        break;
                    }
                }
                prevLoc1 = 0;
                prevLoc2 = 0;

                while (car2Index < car2List.Count)
                {
                    currentCar = car2List[car2Index];
                    currLoc1 = currentCar.CurrentInventoryLocation.Location.Loc1;
                    currLoc2 = currentCar.CurrentInventoryLocation.Location.Loc2;

                    if (prevLoc1 == 0 || (prevLoc1 == currLoc1 && prevLoc2 == currLoc2))
                    {
                        currentCar.Sequence = seq;
                        newList.Add(currentCar);
                        car2Index += 1;
                        seq += 1;
                        prevLoc1 = currLoc1;
                        prevLoc2 = currLoc2;
                    }
                    else
                    {
                        break;
                    }
                }

                prevLoc1 = 0;
                prevLoc2 = 0;

                while (car3Index < car3List.Count)
                {
                    currentCar = car3List[car3Index];
                    currLoc1 = currentCar.CurrentInventoryLocation.Location.Loc1;
                    currLoc2 = currentCar.CurrentInventoryLocation.Location.Loc2;

                    if (prevLoc1 == 0 || (prevLoc1 == currLoc1 && prevLoc2 == currLoc2))
                    {
                        currentCar.Sequence = seq;
                        newList.Add(currentCar);
                        car3Index += 1;
                        seq += 1;
                        prevLoc1 = currLoc1;
                        prevLoc2 = currLoc2;
                    }
                    else
                    {
                        break;
                    }
                }

                prevLoc1 = 0;
                prevLoc2 = 0;

                while (car4Index < car4List.Count)
                {
                    currentCar = car4List[car4Index];
                    currLoc1 = currentCar.CurrentInventoryLocation.Location.Loc1;
                    currLoc2 = currentCar.CurrentInventoryLocation.Location.Loc2;

                    if (prevLoc1 == 0 || (prevLoc1 == currLoc1 && prevLoc2 == currLoc2))
                    {
                        currentCar.Sequence = seq;
                        newList.Add(currentCar);
                        car4Index += 1;
                        seq += 1;
                        prevLoc1 = currLoc1;
                        prevLoc2 = currLoc2;
                    }
                    else
                    {
                        break;
                    }
                }
                prevLoc1 = 0;
                prevLoc2 = 0;
            }

            return newList;
        }

        //private void SetOrderStatusToPartial(IList<PickView> recs)
        //{
        //    List<int> ids = recs.Select(r => r.OrderId).Distinct().ToList();
        //    foreach (var item in ids)
        //    {
        //        try
        //        {
        //            Order ord = _repoOrders.FindByKey(item);
        //            ord.OrderStatusId = 4;
        //            _repoOrders.Update(ord);
        //           GlobalVar.HistoryManager.SaveHistory(ActionCode.PartialOrder, ord);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Error Saving Order Status to Partial. " + ex.Message);
        //        }
        //    }
        //}

        private void PositionDevice(int loc1, int loc2, int loc3, int loc4, bool moveDevice)
        {
            if (_neutronVariables.ShuttleEnabled)
            {
                if (GlobalVar.Shuttle != null)
                {
                    if (moveDevice)
                    {
                        _logger.Log($"2909 Position Device Tray:{loc1} Bin:{loc2} Level:{loc3} Partition:{loc4}");


                        var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4));

                        _logger.Log($"3012 PositionDevice Response: {response.Result.AsString(EnumFormat.Description)}");

                        if (response.Result != DeviceResponse.Success)
                        {
                            if (response.Result == DeviceResponse.TrayDidNotArrive)
                            {

                            }
                            else
                            {
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                    caption: "Device Response Move Next"
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void GetFirstStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetFirstStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (numberOfStops > 0)
            {
                _bindingSourcePickStops.MoveFirst();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);

                _logger.Log($"3012 GetFirstStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");
            }
            Task.Run(() => _logger.Log($"GetFirstStop End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void GetNextStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetNextStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence < numberOfStops)
            {
                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);

                _logger.Log($"3012 GetNextStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");

            }
            Task.Run(() => _logger.Log($"GetNextStop Return: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void GetPreviousStop(bool moveDevice = true)
        {
            Task.Run(() => _logger.Log($"GetPrevStop: [{DateTime.Now.ToLongTimeString()}]"));
            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence > 0)
            {
                _bindingSourcePickStops.MovePrevious();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();

                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);

                _logger.Log($"3012 GetPrevtStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");

            }
            Task.Run(() => _logger.Log($"GetPrevStop Return: [{DateTime.Now.ToLongTimeString()}]"));
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
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

                PositionDevice(loc1, loc2, loc3, loc4, moveDevice);
                _logger.Log($"3012 GetLastStop PositionDevice : {loc1}-{loc2}-{loc3}-{loc4}");

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
            UpdateTowerDisplay();
            UpdatePickPosition();
            UpdateInventoryLocation();
            UpdateImages();
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

        private void UpdateTowerDisplay()
        {
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            var text = _currentPickStop.QuantityToBePicked.ToString();
            ShowShi(loc1, loc2, loc3, loc4, text);

        }

        private void UpdatePickScreenAfterChangeQuantity()
        {
            Task.Run(() => _logger.Log($"UpdatePickScreenAfterChangeQuantity Start: [{DateTime.Now.ToLongTimeString()}]"));
            // MBPickNewItem.Visible = _neutronLicense.CompanyCode == "TOP" ? true : false;
            UpdatePickPosition();
            // UpdateInventoryLocation();
            //  UpdateImages();
            //  LabelFormTitle.Text = _resourceManager.GetString($"Selection");
            //  LabelPickDescription.Text = _currentPickStop.Description;
            //  LabelPickItemNumber.Text = _currentPickStop.Item;
            //  LabelPickUOI.Text = _currentPickStop.UnitOfIssue;
            //  LabelLineOfLines.Text = string.Format("{0} of {1}"
            //      , (_currentPickStop.Sequence).ToString(), _bindingSourcePickStops.Count);
            //  TextBoxRequestedQty.Text = _currentPickStop.Quantity.ToString();

            //  var pickedSoFar = GetPickedSoFar(_currentPickStop.PickViews);
            //  TextBoxPickedSoFar.Text = pickedSoFar.ToString();

            LabelPickQty.Text = (_currentPickStop.QuantityToBePicked).ToString();
            Task.Run(() => _logger.Log($"UpdatePickScreenAfterChangeQuantity End: [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void UpdateImages()
        {

            Task.Run(() => _logger.Log($"UpdateImages Start : [{DateTime.Now.ToLongTimeString()}]"));
            if (!string.IsNullOrEmpty(_imagesDirectory))
            {
                try
                {
                    var path = string.Concat(_imagesDirectory, _currentPickStop.Item, str2: @".jpg");
                    if (File.Exists(path))
                    {
                        PictureBoxItemImage.Load(path);
                    }
                    else
                    {
                        path = string.Concat(_imagesDirectory, str1: @"Unknown.jpg");
                        if (File.Exists(path))
                        {
                            PictureBoxItemImage.Load(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error getting Image.  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            Task.Run(() => _logger.Log($"UpdateImages End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        //private void UpdateHotImage(string image)
        //{

        //    Task.Run(() => _logger.Log($"Update Hot Images Start : [{DateTime.Now.ToLongTimeString()}]"));
        //    if (!string.IsNullOrEmpty(_imagesDirectory))
        //    {
        //        try
        //        {
        //            string path = string.Concat(_imagesDirectory, image, str2: @".jpg");
        //            if (File.Exists(path))
        //            {
        //                PictureBoxItemHotImage.Load(path);
        //            }
        //            else
        //            {
        //                path = string.Concat(_imagesDirectory, str1: @"Unknown.jpg");
        //                if (File.Exists(path))
        //                {
        //                    PictureBoxItemHotImage.Load(path);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Error getting Hot Image.  {ex.Message} \r\n {ex.InnerException}");
        //        }
        //    }
        //    Task.Run(() => _logger.Log($"Update Hot Images End : [{DateTime.Now.ToLongTimeString()}]"));
        //}

        private void UpdateInventoryLocation()
        {
            Task.Run(() => _logger.Log($"UpdateInventoryLocation Start : [{DateTime.Now.ToLongTimeString()}]"));

            TextBoxPickLoc1.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
            TextBoxPickLoc2.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
            TextBoxPickLoc3.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
            TextBoxPickLoc4.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc4.ToString();
            TextBoxPickLoc5.Text = _currentPickStop.CurrentInventoryLocation.Location.Loc5.ToString();

            LabelLocationNumber.Text = string.Format(format: "{0} of {1}"
                , arg0: _currentPickStop.InventoryIndex + 1, arg1: _currentPickStop.Inventory.Count);
            TextBoxLocationQuantity.Text = _currentPickStop.CurrentInventoryLocation.Quantity.ToString();
            TextBoxTotalQuantity.Text = _currentPickStop.TotalQuantityInInventory.ToString();
            TextBoxReceivedDate.Text = _currentPickStop.CurrentInventoryLocation.ReceivedDate.ToString("G");
            LabelPrimeBin.Visible = _currentPickStop.CurrentInventoryLocation.PrimeBin;
            LabelStaticRelease.Text = _currentPickStop.CurrentInventoryLocation.StorageType.Name;

            Task.Run(() => _logger.Log($"UpdateInventoryLocation End : [{DateTime.Now.ToLongTimeString()}]"));

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

                        Task.Run(() => GlobalVar.Displays.ShowShi(loc1, loc2, loc3, loc4, text));
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
                    _logger.Log($"ClearAllShi Function");
                    GlobalVar.Displays.ClearAllShi();
                }
            }
        }

        private void ClearAllBli()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    Task.Run(() => _logger.Log($"ClearAllBli Function"));
                    GlobalVar.Displays.ClearAllBli();
                    GlobalVar.Displays.ClearOc(1);
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
            var font = new Font("Microsoft San Serif", 24);
            TextBoxPickPos1.Font = font;
            TextBoxPickPos2.Font = font;
            TextBoxPickPos3.Font = font;
            TextBoxPickPos4.Font = font;
            TextBoxPickPos5.Font = font;
            TextBoxPickPos6.Font = font;
            TextBoxPickPos7.Font = font;
            TextBoxPickPos8.Font = font;

            foreach (var pickView in _currentPickStop.PickViews)
            {
                var pos = pickView.PickPosition;
                if (_neutronVariables.IptiDisplays)
                {
                    TurnOnOcDisplay(1, 1, pickView.Item);
                }

                switch (pos)
                {
                    case 1:
                        TextBoxPickPos1.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos1.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos1Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 1, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 2:
                        TextBoxPickPos2.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos2.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos2Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 2, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 3:
                        TextBoxPickPos3.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos3.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos3Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 3, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 4:
                        TextBoxPickPos4.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos4.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos4Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 4, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 5:
                        TextBoxPickPos5.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos5.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos5Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 5, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 6:
                        TextBoxPickPos6.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos6.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos6Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 6, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 7:
                        TextBoxPickPos7.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos7.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos7Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 7, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    case 8:
                        TextBoxPickPos8.Text = pickView.QuantityToBePicked.ToString();
                        LabelPickPos8.BackColor = GetBackColor(pickView.QuantityToBePicked);
                        Pos8Display.BackColor = Color.Red;
                        TurnOnBatchPositionDisplay(position: 8, beacon: 2, text: pickView.QuantityToBePicked.ToString());
                        break;
                    default:
                        break;
                }
            }
            Task.Run(() => _logger.Log($"UpdatePickPosition End : [{DateTime.Now.ToLongTimeString()}]"));
        }

        private void TurnOnBatchPositionDisplay(int position, int beacon, string text)
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => GlobalVar.Displays.ShowBli(position, beacon, text));
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

            //TextBoxPos1.Text = string.Empty;
            //TextBoxPos2.Text = string.Empty;
            //TextBoxPos3.Text = string.Empty;
            //TextBoxPos4.Text = string.Empty;
            //TextBoxPos5.Text = string.Empty;
            //TextBoxPos6.Text = string.Empty;
            //TextBoxPos7.Text = string.Empty;
            //TextBoxPos8.Text = string.Empty;
        }

        private void SetOrderCompleteThisStation()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var linesNotComplete = _repoOrderDetails
                    .FindBy(r => r.OrderId == bp.OrderId && r.StationNumber == _station.StationNumber)
                    .Where(r => r.LineStatusId != 6).ToList();
                if (linesNotComplete.Count != 0) continue;
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
            Cursor.Current = Cursors.WaitCursor;
            MBSkipPick.Enabled = false;
            Task.Run(() => _logger.Log($"SkipPick_Click Start : [{DateTime.Now.ToLongTimeString()}]"));

            _currentPickStop.Skipped = true;
            Task.Run(() => _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1));
            var pickViewCount = _currentPickStop.PickViews.Count;
            foreach (var pickView in _currentPickStop.PickViews)
            {
                SetOrderDetailLineStatus(pickView.OrderDetail, 9, ActionCode.Skip);
            }

            GlobalVar.HistoryManager.SaveHistory(ActionCode.Skip, _currentPickStop);
            Task.Run(() => _logger.Log($"History Done"));

            var numberOfStops = _bindingSourcePickStops.Count;
            if (_currentPickStop.Sequence < numberOfStops)
            {
                _bindingSourcePickStops.MoveNext();
                _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                UpdatePickScreen();
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

        private void CloseBatchWithSkip()
        {
            if (_neutronVariables.DisplaysEnabled)
            {
                if (GlobalVar.Displays != null)
                {
                    if (_neutronVariables.BliEnabled)
                    {
                        Task.Run(() => _logger.Log($"Clear all BLI's on CLOSE"));
                        ClearAllBli();
                    }

                    if (_neutronVariables.ShiEnabled)
                    {
                        Task.Run(() => _logger.Log($"Clear all SHI's on CLOSE"));
                        ClearAllShi();
                    }
                }
            }

            ClearOrderPositions();
            ClearBatchPositions();
            _logger.Log($"Start Upload Processor: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "TMG":
                    var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "SFH":

                    _logger.Log("Choosing the SFH case.");
                    // Mediator.GetInstance().OnBatchComplete(this);
                    //uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    //uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "AES":
                    uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "TOP":
                    var topUploadProcessor = new TopUploadProcessor(_neutronVariables, _neutronLicense);
                    topUploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;

                default:
                    break;
            }

            if (_neutronLicense.CompanyCode == "SFH")
            {
                var shortReportProcessor = new ShortReportProcessor(_bindingSourcePickStops, _labelPrinter);
            }

            if (_neutronVariables.ParkPositionAfterBatch)
            {
                if (_neutronVariables.ShuttleEnabled)
                {
                    if (GlobalVar.Shuttle != null)
                    {
                        var response = GlobalVar.Shuttle.Park();
                        if (response != DeviceResponse.Success)
                        {
                            if (response == DeviceResponse.TrayDidNotArrive)
                            {

                            }
                            else
                            {
                                MessageBox.Show(response.AsString(EnumFormat.Description),
                                    caption: "Device Information"
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }

                    }
                }
            }
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

        private void MBPickAccept_Click(object sender, EventArgs e)
        {

            PickAccept();
            MBPickAccept.Focus();
        }

        private void PickAccept()
        {


            if (InvokeRequired)
            {
                var method = new MethodInvoker(PickAccept);
                Invoke(method);
                return;
            }

            MBPickAccept.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            var thisPick = IntegerExtensions.ParseInt(LabelPickQty.Text);

            Task.Run(() => _logger.Log($"PickAccept_Click Start : [{DateTime.Now.ToLongTimeString()}]"));
            var pick = false;

            // in case a hot action or Location Count changes the current inventory
            // let's refresh the currentInventory
            var key = _currentPickStop.CurrentInventoryLocation.Id;
            _currentPickStop.CurrentInventoryLocation.Quantity = _repoInventory.FindByKey(key).Quantity;

            pick = _currentPickStop.CurrentInventoryLocation.Quantity >= _currentPickStop.QuantityToBePicked;

            if (pick)
            {
                _currentPickStop.UpdatePickViews(GlobalVar.User);  //good
                _currentPickStop.PickedQty = GetPickedSoFar(_currentPickStop.PickViews);
                _currentPickStop.QuantityToBePicked = GetTotalQuantityToBePicked(_currentPickStop.PickViews);  // QuantityToBePicked on ALL PickViews

                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                _currentPickStop.TotalQuantityInInventory = total;
                TextBoxTotalQuantity.Text = total.ToString();

                Task.Run(() => _logger.Log($"PickAccept_Click 1 : [{DateTime.Now.ToLongTimeString()}]"));

                bool stopComplete;
                if (_shortPick)
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


                    // _deviceManager.MoveNext(_currentPickStop.CurrentInventoryLocation.Location.Loc1);


                    Task.Run(() => _logger.Log($"PickAccept_Click 2 Stop Complete Start : [{DateTime.Now.ToLongTimeString()}]"));

                    UpdateInventoryQuantity(_currentPickStop);

                    Task.Run(() => _logger.Log($"UpdateInventoryQuantity"));

                    Task.Run(() => _logger.Log($"History Done"));

                    _currentPickStop.SetPickViewsComplete(GlobalVar.User, _logger);

                    foreach (var pickView in _currentPickStop.PickViews)
                    {
                        CheckForOrderComplete(pickView.OrderDetail.Order);
                    }

                    Task.Run(() => _logger.Log($"PickAccept_Click Stop Complete End : [{DateTime.Now.ToLongTimeString()}]"));

                    var numberOfStops = _bindingSourcePickStops.Count;
                    if (_currentPickStop.Sequence < numberOfStops)
                    {
                        //Use the first carousel location for the movenext in case multiple picks are required for stop
                        _deviceManager.MoveNext(_currentPickStop.Inventory[0].Location.Loc1);
                        _bindingSourcePickStops.MoveNext();
                        _currentPickStop = (PickStop)_bindingSourcePickStops.Current;
                        UpdatePickScreen();
                    }
                    else
                    {
                        Task.Run(() => _logger.Log($"CloseBatch"));
                        CloseBatch();
                    }
                }
                else  //PickStop is NOT complete, why?
                {
                    UpdatePickScreen();
                }
            }
            else
            {
                MessageBox.Show(text: "Pick Exceeds Inventory at this location.  Add Inventory or Change Quantity before continuing.", caption: "Inventory", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Stop);
            }
            Task.Run(() => _logger.Log($"PickAccept_Click End : [{DateTime.Now.ToLongTimeString()}]"));
            Cursor.Current = Cursors.Default;
            MBPickAccept.Enabled = true;
            MBPickAccept.Focus();
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
            if (_currentPickStop.QuantityToBePicked == 0)
            {
                return true;
            }
            else if (_currentPickStop.QuantityToBePicked < 0)
            {
                var result = MessageBox.Show("Do you want to overpick this item?", "Overpick Question"
                    , MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return false;
                return true;
            }
            else if (_currentPickStop.QuantityToBePicked > 0)
            {
                //does the stop have more than one inventory lcoation
                var b = GetNextInventoryLocation();

                if (b)
                {
                    return false;
                }
                else  // no more locations
                {
                    var result = MessageBox.Show("No more locations, Pick Short?", "Short Pick", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private void UpdateInventoryQuantity(PickStop pickStop)
        {
            var sb = new StringBuilder();
            //currentPickStop.CurrentInventoryLocation.Quantity -= currentPickStop.PickedQty;
            // repoInventory.Update(currentPickStop.CurrentInventoryLocation);
            sb.AppendLine($"Update Inventory Quantity for PickStop Item: {pickStop.Item} - Start");

            foreach (var pickView in pickStop.PickViews)
            {
                sb.AppendLine($"PickView: {pickView.Ord1}  {pickView.Ord2}");
                foreach (var pickLocation in pickView.PickLocations)
                {
                    sb.AppendLine($"Pick Location: {pickLocation.Inventory.Location.Slot}");
                    //  pickLocation.Inventory.Quantity -= pickLocation.Quantity;

                    var inv = _repoInventory.FindByKey(pickLocation.Inventory.Id);
                    if (inv != null)
                    {
                        sb.AppendLine($"Inventory Qty: {inv.Quantity}  Pick Location Qty: {pickLocation.Quantity}");
                        inv.Quantity -= pickLocation.Quantity;

                        _repoInventory.Update(inv);
                        sb.AppendLine("Update Inventory");
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.PickOrder, inv, pickLocation.Quantity, pickView);
                        sb.AppendLine("Write Pick Order to History");
                    }
                    else
                    {
                        sb.AppendLine($"Inventory is NULL.");
                    }
                }
            }
            _logger.Log($"{sb.ToString()}");
        }

        private void CloseBatch()
        {
            ClearAllShi();
            ClearAllBli();
            ClearOrderPositions();
            ClearBatchPositions();
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
                    var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                    uploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;
                case "TOP":
                    var topUploadProcessor = new TopUploadProcessor(_neutronVariables, _neutronLicense);
                    topUploadProcessor.CreateHostFile(_bindingSourcePickStops);
                    break;

                default:
                    break;
            }

            using (var db = new NeutronDb())
            {
                var locationIds = new List<int>();
                var invs = db.Inventory.Where(r => r.Quantity == 0 && r.StationId == _station.StationId && r.StorageTypeId == (int)StorageType.Release).ToList();
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

            if (_neutronLicense.CompanyCode == "SFH")
            {
                var shortReportProcessor = new ShortReportProcessor(_bindingSourcePickStops, _labelPrinter);
            }

            if (_neutronVariables.ParkPositionAfterBatch)
            {
                if (_neutronVariables.ShuttleEnabled)
                {
                    if (GlobalVar.Shuttle != null)
                    {
                        var response = GlobalVar.Shuttle.Park();
                        if (response != DeviceResponse.Success)
                        {
                            if (response == DeviceResponse.TrayDidNotArrive)
                            {

                            }
                            else
                            {
                                MessageBox.Show(response.AsString(EnumFormat.Description),
                                    caption: "Device Response Move Next"
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }

                    }
                }
            }
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

        private void PrintAllDocuments()
        {
            foreach (var bp in _ordersToPick)
            {
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoOrders.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
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
                var order = _repoOrders.FindByKey(id);
                var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
                if (printJob != null) continue;
                PrintDoc(bp.PositionNumber, order);
                printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                _repoPrintJob.Insert(printJob);
            }
        }

        private void PrintAnticipatedOuts()
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            List<AnticipatedOut> outs = new List<AnticipatedOut>();
            var comboBoxValue = ComboBoxStationNumber.Text;
            var anticipatedOuts = GetAnticipatedOuts();

            if (anticipatedOuts.Count <= 0) return;

            if (comboBoxValue != "ALL")
            {
                var stationId = IntegerExtensions.ParseInt(comboBoxValue);
                anticipatedOuts = anticipatedOuts.Where(r => r.Station == stationId).ToList();
            }
            DocumentToPrint.PrintAnticipatedOuts(anticipatedOuts, _documentPrinter, _neutronVariables.PrintPreview);
        }

        private void MBPrintPick_Click(object sender, EventArgs e)
        {
            var comboBoxValue = ComboBoxStationNumber.Text;
            var orderViews = GetCheckedOrders();
            if (orderViews.Count <= 0) return;

            if (comboBoxValue == "ALL")
            {
                foreach (var orderView in orderViews)
                {
                    PrintPickListAll(orderView.Id);
                }
            }
            else
            {
                var stationId = IntegerExtensions.ParseInt(comboBoxValue);
                foreach (var orderView in orderViews)
                {
                    PrintPickListByStation(orderView.Id, stationId);
                }
            }
        }

        private void PrintPackingList(int orderId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var packingList = GetPackingList(orderId);
            DocumentToPrint.PrintPackingList(packingList, _documentPrinter, _neutronVariables.PrintPreview);
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

        private void MBPrintDocument_Click(object sender, EventArgs e)
        {
            PrintPickList(8);
            ShowAvailableOrdersRack();
            TextBoxFindAvailableOrdersRack.Focus();
        }

        private void PrintPickList(int stationId = 8)
        {
            var orderViews = GetCheckedOrdersRack();
            if (orderViews.Count <= 0) return;
            foreach (var orderView in orderViews)
            {
                var recs = orderView.OrderDetails.Where(r => r.StationNumber == 8 && r.LineStatusId != 6).ToList();
                foreach (var rec in recs)
                {
                    var recToUpdate = _repoOrderDetails.FindByKey(rec.Id);
                    if (recToUpdate != null)
                    {
                        recToUpdate.LineStatusId = 3;

                        _repoOrderDetails.Update(recToUpdate);
                    }
                }

                PrintPickListByStation(orderView.Id, stationId);
            }
        }


        private void PrintPickListAll(int orderId)
        {
            if (!_neutronVariables.EnableDocumentPrinter) return;
            var pickList = GetPickList(orderId);
            DocumentToPrint.PrintPickList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
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
            DocumentToPrint.PrintPickList(pickList, _documentPrinter, _neutronVariables.PrintPreview);
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
                // Task.Run(() => DocumentToPrint.Print(positionNumber, order.Ord1, _documentPrinter, order.Ord2));
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
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
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

        private void PrintTote(int positionNumber, Order order)
        {
            Task.Run(() => _logger.Log($"Printing Tote Label. {order.Ord1}"));
            if (_neutronVariables.EnableLabelPrinter)
            {
                //  Task.Run(() => ToteToPrint.Print(positionNumber, order, _labelPrinter));
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
            if (_currentPickStop.InventoryIndex - 1 < 0)
            {
                _currentPickStop.InventoryIndex = 0;
            }
            else
            {
                _currentPickStop.InventoryIndex -= 1;
            }
            _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[_currentPickStop.InventoryIndex];
            UpdateInventoryLocation();
        }


        private void ButtonNextInventoryLocation_Click(object sender, EventArgs e)
        {
            GetNextInventoryLocation();
        }

        private bool GetNextInventoryLocation()
        {
            var result = false;
            if (_currentPickStop.InventoryIndex + 1 < _currentPickStop.Inventory.Count)
            {
                _currentPickStop.InventoryIndex += 1;
                _currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[_currentPickStop.InventoryIndex];

                //var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1.ToString();
                //var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2.ToString();
                //var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3.ToString();
                var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
                var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
                var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
                var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;
                var text = _currentPickStop.QuantityToBePicked.ToString();
                _logger.Log($"Get Next Inventory Location: {loc1}-{loc2}");

                PositionDevice(loc1, loc2, loc3, loc4, true);

                ShowShi(loc1, loc2, loc3, loc4.ToString(), text);

                UpdateInventoryLocation();
                result = true;
            }
            //else
            //{
            //    for (var i = 0; i < _currentPickStop.Inventory.Count; i++)
            //    {
            //        if (_currentPickStop.Inventory[i].Quantity <= 0) continue;
            //        _currentPickStop.InventoryIndex = i;
            //        result = true;
            //        break;
            //    }

            //    //MessageBox.Show($"No More Locations.");

            //}

            //_currentPickStop.CurrentInventoryLocation = _currentPickStop.Inventory[_currentPickStop.InventoryIndex];
            //  await UpdateInventoryLocation();
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
            foreach (var pickView in _currentPickStop.PickViews)
            {
                if (pickView.PickPosition == pos)
                {
                    if (newQty <= pickView.GetQuantityToBePicked())
                    {
                        pickView.QuantityToBePicked = newQty;
                        _currentPickStop.QuantityToBePicked = _currentPickStop.GetTotalQuantityToBePicked();
                        LabelPickQty.Text = _currentPickStop.QuantityToBePicked.ToString();
                        UpdatePickScreen();
                    }
                }
            }
        }

        private void ButtonMove_Click(object sender, EventArgs e)
        {
            var loc1 = _currentPickStop.CurrentInventoryLocation.Location.Loc1;
            var loc2 = _currentPickStop.CurrentInventoryLocation.Location.Loc2;
            var loc3 = _currentPickStop.CurrentInventoryLocation.Location.Loc3;
            var loc4 = _currentPickStop.CurrentInventoryLocation.Location.Loc4;

            PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);

            //if (neutronVariables.ShuttleEnabled)
            //{
            //    int loc1 = currentPickStop.CurrentInventoryLocation.Location.Loc1;
            //    int loc2 = currentPickStop.CurrentInventoryLocation.Location.Loc2;

            //    DeviceResponse response = GlobalVar.Shuttle.PositionDevice(loc1, loc2);
            //    if (response != DeviceResponse.Success)
            //    {
            //        MessageBox.Show(response.AsString(EnumFormat.Description), caption: "Device Information"
            //            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            //    }
            //}
        }

        private void MBPriority_Click(object sender, EventArgs e)
        {
            var priority = 0;
            var recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                using (var form = new FrmChangePriority())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        priority = IntegerExtensions.ParseInt((form.NewPriority));
                    }
                }

                foreach (var id in recs)
                {
                    var ord = _repoOrders.FindByKey(id);
                    ord.Priority = priority;
                    _repoOrders.Update(ord);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ChangePriority, ord);
                }
            }
            ShowAllOrders();
        }

        private void MBReturnToStock_Click(object sender, EventArgs e)
        {
            var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
            var recs = GetCheckedOrders();
            if (recs.Any())
            {
                foreach (var ov in recs)
                {
                    if (ov != null)
                    {
                        if (_neutronVariables.UseReturnToStock)
                        {
                            //int rtsCode = (int)OrderStatus.Returned;

                            foreach (var detail in ov.Order.OrderDetails)
                            {
                                SetOrderDetailLineStatus(detail, (int)OrderStatus.Returned, ActionCode.OrderDetailRts);
                            }

                            uploadProcessor.ReturnOrderToStock(ov.Order);  //sets the RTS code to each OrderDetail line
                            ov.Order.OrderStatusId = (int)OrderStatus.Returned;  //Returned
                            _repoOrders.Update(ov.Order);
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderRts, ov.Order);
                        }
                        if (_neutronVariables.CreateStoreOrderWithRts)
                        {
                            CreateStoreOrderFromOrderDetailComplete(ov.Order);
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

        private void SetOrderDetailLineStatus(OrderDetail detail, int code, ActionCode actionCode)
        {
            try
            {
                detail.LineStatusId = code;
                _repoOrderDetails.Update(detail);
                GlobalVar.HistoryManager.SaveHistory(actionCode, detail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error setting Order Detail Status Code. " + ex.Message);
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
            var details = _orderDetailsRepository.GetOrderDetailsViewByOrder(orderId);
            if (!details.Any()) return;
            _bindingSourceOrderDetailsView.DataSource = details;
            DataGridViewOrderDetails.DataSource = _bindingSourceOrderDetailsView;
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
            var watch = new Stopwatch();
            watch.Start();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            ShowAllOrders();
            tabControl1.SelectedTab = OrderListing;
            Task.Run(() => _logger.Log($"Job Manager Main screen Elasped MSec:  {watch.ElapsedMilliseconds}ms"));
            Cursor.Current = Cursors.Default;
        }

        private void MBMainAvailableOrders_Click(object sender, EventArgs e)
        {
            if (_station.StationNumber >= 8)
            {
                ShowAvailableRackScreen();

            }
            else
            {
                AvailableOrdersScreen();
            }
        }

        public void AvailableOrdersScreen()
        {
            Cursor.Current = Cursors.WaitCursor;
            MBCompress.Enabled = false;
            LabelFormTitle.Text = _resourceManager.GetString($"AvailableJobs");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            ClearSelection(DataGridViewAvailableOrders);
            ClearOrderPositions();
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            _showSkipped = false;
            ShowAvailableOrders();
            if (_station.StationNumber == 8)
            {
                tabControl1.SelectedTab = AvailableRack;
            }
            else
            {
                tabControl1.SelectedTab = AvailableOrders;
            }
            Cursor.Current = Cursors.Default;
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
            ClearSelection(DataGridView1);
            ClearOrderPositions();
            InitOrdersToPick(_neutronVariables.PickBatchSize);
            _showSkipped = true;
            ShowRackOrders();
            // tabControl1.SelectedTab = AvailableRack;
            Cursor.Current = Cursors.Default;
        }

        //New Order
        private void MBMainNewOrder_Click(object sender, EventArgs e)
        {
            LabelFormTitle.Text = _resourceManager.GetString($"NewJob");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            // _bindingSourceItems.DataSource = GetItemsList(string.Empty);
            //  DataGridViewNewOrder.DataSource = _bindingSourceItems;
            ClearNewOrderForm();
            tabControl1.SelectedTab = NewOrder;
            //  DataGridViewNewOrder.ClearSelection();
            //  DataGridViewNewOrder.Update();
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
            AvailableOrdersScreen();
        }

        private void MBOrderDetailsBack_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrders.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = OrderListing;
        }

        //private void DataGridViewInventory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (e.ColumnIndex == 0)
        //    {
        //        _currentInventoryView = (SqlInventoryView)_bindingSourceHot.Current;
        //        int loc1 = _currentInventoryView.Loc1;
        //        int loc2 = _currentInventoryView.Loc2;
        //        int loc3 = _currentInventoryView.Loc3;
        //        int loc4 = _currentInventoryView.Loc4;

        //        PositionDevice(loc1, loc2, loc3, loc4, moveDevice: true);
        //        ShowShi(loc1, loc2, _currentInventoryView.Loc3, _currentInventoryView.Loc4.ToString()
        //            , _currentInventoryView.Quantity.ToString());

        //        UpdateHotPickScreen(_currentInventoryView);
        //        LabelFormTitle.BackColor = Color.Red;
        //        LabelFormTitle.Text = _resourceManager.GetString($"HotPick");
        //        tabControl1.SelectedTab = HotPickPickToDelete;
        //    }
        //}

        //private void UpdateHotPickScreen(SqlInventoryView invItem)
        //{
        //    LabelHotPickDescription.Text = invItem.Description;
        //    LabelHotPickItem.Text = invItem.Item;
        //    LabelHotPickUOI.Text = invItem.UnitOfIssueName;
        //    TextBoxHotPickLoc1.Text = invItem.Loc1.ToString();
        //    TextBoxHotPickLoc2.Text = invItem.Loc2.ToString();
        //    TextBoxHotPickLoc3.Text = invItem.Loc3.ToString();
        //    TextBoxHotPickLoc4.Text = invItem.Loc4.ToString();
        //    TextBoxHotPickLoc5.Text = invItem.Loc5.ToString();
        //    TextBoxHotPickLocationQuantity.Text = invItem.Quantity.ToString();
        //    UpdateHotImage(invItem.Item);
        //}

        ////Hot Pick Screen
        //private void ButtonLocationCount_Click(object sender, EventArgs e)
        //{
        //    //int inventoryId = currentInventoryView.Id;
        //    //int qty = OpenLocationCountForm(inventoryId);
        //    ////refresh the datasource using the Search function
        //    //if (qty >= 0)
        //    //{
        //    //    FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    //    SetCurrentInventoryView(inventoryId);

        //    //    UpdateHotPickScreen(currentInventoryView);
        //    //}
        //}


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
                TextBoxLocationQuantity.Text = qty.ToString();
                _currentPickStop.CurrentInventoryLocation.Quantity = qty;
                var total = _currentPickStop.Inventory.Sum(r => r.Quantity);
                _currentPickStop.TotalQuantityInInventory = total;
                TextBoxTotalQuantity.Text = total.ToString();
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

        private void SetCurrentInventoryView(int inventoryId)
        {
            var rec = _bindingSourceHot.List.OfType<InventoryView>().ToList().Find(f => f.Id == inventoryId);
            var pos = _bindingSourceHot.IndexOf(rec);
            _bindingSourceHot.Position = pos;
            _currentInventoryView = (SqlInventoryView)_bindingSourceHot.Current;
        }

        //private void MBHotPickAccept_Click(object sender, EventArgs e)
        //{
        //    Cursor.Current = Cursors.WaitCursor;
        //    int pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
        //    Inventory inv = _repoInventory.FindByKey(_currentInventoryView.Id);
        //    inv.Quantity -= pickQty;
        //    _repoInventory.Update(inv);
        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.PickHot, inv);
        //    //FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
        //    LabelFormTitle.BackColor = Color.RoyalBlue;
        //    tabControl1.SelectedTab = HotPickToDelete;
        //    Cursor.Current = Cursors.Default;
        //}

        //private void MBHotStoreAccept_Click(object sender, EventArgs e)
        //{
        //    Cursor.Current = Cursors.WaitCursor;
        //    int pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
        //    Inventory inv = _repoInventory.FindByKey(_currentInventoryView.Id);
        //    inv.Quantity += pickQty;
        //    _repoInventory.Update(inv);
        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.StoreHot, inv);
        //    //FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
        //    LabelFormTitle.BackColor = Color.RoyalBlue;
        //    tabControl1.SelectedTab = HotPickToDelete;
        //    Cursor.Current = Cursors.Default;
        //}

        //private void MBHotAccept_Click(object sender, EventArgs e)
        //{
        //    Inventory inv;
        //    ClearAllShi();
        //    int actionCode = GetHotActionCode();
        //    Cursor.Current = Cursors.WaitCursor;
        //    int pickQty = (TextBoxHotPickQuantity.Text).ParseInt();
        //    inv = _repoInventory.FindByKey(_currentInventoryView.Id);

        //    if (RadioButtonPick.Text == _resourceManager.GetString($"Pick"))
        //    {
        //        inv.Quantity -= pickQty;
        //        if (_currentInventoryView.Item == _currentPickStop.Item)
        //        {
        //            TextBoxLocationQuantity.Text = inv.Quantity.ToString(); // TextBoxHotPickLocationQuantity.Text;
        //            TextBoxTotalQuantity.Text = (int.Parse(TextBoxTotalQuantity.Text) - pickQty).ToString();
        //        }
        //    }
        //    else if (RadioButtonPick.Text == _resourceManager.GetString($"Store"))
        //    {
        //        inv.Quantity += pickQty;
        //        if (_currentInventoryView.Item == _currentPickStop.Item)
        //        {
        //            TextBoxLocationQuantity.Text = inv.Quantity.ToString(); //  TextBoxHotPickLocationQuantity.Text;
        //            TextBoxTotalQuantity.Text = (int.Parse(TextBoxTotalQuantity.Text) + pickQty).ToString();
        //        }
        //    }
        //    TextBoxHotPickLocationQuantity.Text = inv.Quantity.ToString();
        //    _repoInventory.Update(inv);
        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryModify, inv);
        //    FindHotRecord(TextBoxFindItem.Text.Trim().ToLower());
        //    DataGridViewInventory.Refresh();
        //    LabelFormTitle.Text = _resourceManager.GetString($"HotSearch");
        //    LabelFormTitle.BackColor = Color.RoyalBlue;
        //    Cursor.Current = Cursors.Default;
        //    tabControl1.SelectedTab = HotPickToDelete;
        //}

        //private int GetHotActionCode()
        //{
        //    int result = 3;
        //    var radioButtons = new List<RadioButton> { RadioButtonPick, RadioButtonWarranty, RadioButtonScrap, RadioButtonOther };

        //    if (RadioButtonPick.Text == _resourceManager.GetString($"Pick"))
        //    {
        //        foreach (RadioButton item in radioButtons)
        //        {
        //            if (item.Checked)
        //            {
        //                if (item.Text == _resourceManager.GetString($"Pick"))
        //                {
        //                    result = 3;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Warranty"))
        //                {
        //                    result = 39;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Scrap"))
        //                {
        //                    result = 40;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Other"))
        //                {
        //                    result = 41;
        //                }


        //            }
        //        }
        //    }
        //    else if (RadioButtonPick.Text == "Store")
        //    {
        //        foreach (RadioButton item in radioButtons)
        //        {
        //            if (item.Checked)
        //            {
        //                if (item.Text == _resourceManager.GetString($"Store"))
        //                {
        //                    result = 4;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Warranty"))
        //                {
        //                    result = 42;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Scrap"))
        //                {
        //                    result = 43;
        //                }
        //                else if (item.Text == _resourceManager.GetString($"Other"))
        //                {
        //                    result = 44;
        //                }
        //                else
        //                {
        //                    result = 4;
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        private void MBShowAvailable_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            //ShowAvailable();
            _currentDataSet = CurrentDataSet.Available;
            ShowAllOrders();
            // MBCompress.Enabled = true;
            Cursor.Current = Cursors.Default;



            //DataGridView1.Columns.Clear();
            //SetupOrderGrid();
            //ShowAvailableOrders();
        }

        private void CompressOrders()
        {
            var ids = GetCheckedOrderIds();
            var sb = new StringBuilder();
            foreach (var i in ids)
            {
                sb.Append(i + ",");
            }
            var orderIds = sb.ToString().TrimEnd(',');

            using (var context = new NeutronDb())
            {
                var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                var paramOrderType = new SqlParameter("@ORDERTYPE", "PICK");
                var parameters = new object[] { paramOrderIds, paramOrderType };

                context.Database.ExecuteSqlCommand("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds, paramOrderType);

            }

            ShowAllOrders();
        }

        private void ShowButtons()
        {
            MBPriority.Visible = true;
            MBHold.Visible = true;
            MBRelease.Visible = true;
            MBReturnToStock.Visible = _neutronVariables.UseReturnToStock;
            MBReturnToStockOrderDetail.Visible = _neutronVariables.UseReturnToStock;
            MBDeleteOrder.Visible = true;
        }

        private void HideButtons()
        {
            MBPriority.Visible = false;
            MBHold.Visible = false;
            MBRelease.Visible = false;
            MBReturnToStock.Visible = false;
            MBReturnToStockOrderDetail.Visible = false;
            MBDeleteOrder.Visible = false;
        }

        #region New Order
        //New Order
        private void MBNewOrderSearch_Click(object sender, EventArgs e)
        {
            FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());
        }

        private void FindItemRecord(string s)
        {
            try
            {
                _bindingSourceItems.DataSource = GetItemsList(s);
                DataGridViewNewOrder.DataSource = _bindingSourceItems;

                //if (string.IsNullOrEmpty(s))
                //{
                //    _bindingSourceItems.DataSource = GetItemsList();
                //    DataGridViewNewOrder.DataSource = _bindingSourceItems;
                //}
                //else
                //{
                //    IEnumerable<NewItemView> projection = GetItemsList();
                //    _bindingSourceItems.DataSource = projection
                //        .Where(d => d.Item.ToLower().Contains(s) || d.Description.ToLower().Contains(s)).ToList();
                //    DataGridViewNewOrder.DataSource = _bindingSourceItems;
                //}
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
            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0;
        }

        private void TextBoxNewOrderFind_KeyDown(object sender, KeyEventArgs e)
        {
            // MessageBox.Show($" TextBoxNewOrderFind Key code: {e.KeyCode}");
            if (e.KeyCode == Keys.Return)
            {
                FindItemRecord(TextBoxNewOrderFind.Text.Trim().ToLower());
            }
            if (e.KeyCode == Keys.Escape)
            {
                TextBoxNewOrderFind.Text = "";
            }
        }

        private void TextBoxNewOrderQuantity_TextChanged(object sender, EventArgs e)
        {
            ButtonAddDetail.Enabled = IntegerExtensions.ParseInt(TextBoxNewOrderQuantity.Text) > 0;
        }

        private void TextBoxNewOrderQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                AddDetail();
            }
        }

        private void ButtonNewOrderClear_Click(object sender, EventArgs e)
        {
            TextBoxNewOrderFind.Text = string.Empty;
            TextBoxNewOrderFind.Focus();
            FindItemRecord(string.Empty);
        }

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

        private void ButtonAddDetail_Click(object sender, EventArgs e)
        {
            AddDetail();
        }

        private void AddDetail()
        {
            var rec = new NewItemView()
            {
                ItemDefinitionId = IntegerExtensions.ParseInt((LabelNewOrderItemId.Text))
                ,
                StationNumber = IntegerExtensions.ParseInt(LabelNewOrderStationNumber.Text)
                ,
                Item = TextBoxNewOrderItem.Text
                ,
                Description = TextBoxNewOrderDescription.Text
                ,
                Quantity = IntegerExtensions.ParseInt((TextBoxNewOrderQuantity.Text))
            };

            _bindingSourceNewItems.Add(rec);

            ClearNewOrderDetail();
            ButtonAddDetail.Enabled = false;
            TextBoxNewOrderFind.Focus();

            ButtonRemoveLine.Enabled = _bindingSourceNewItems.Count > 0;


        }

        private void ClearNewOrderDetail()
        {

            TextBoxNewOrderItem.Text = "";
            TextBoxNewOrderDescription.Text = "";
            TextBoxNewOrderQuantity.Text = "";
            TextBoxNewOrderFind.Text = "";
            LabelNewOrderStationNumber.Text = "";
            LabelNewOrderItemId.Text = "";
        }

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

        private void InitDataGridViewNewItems()
        {
            DataGridViewNewItems.DataSource = _bindingSourceNewItems;
        }

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
                    OrderStatusId = 1
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
                        LineStatusId = 1,
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

            //recs = _repoInventory.AllInclude(r => r.ItemDefinition).Select(d => new NewItemView()
            //{
            //    InventoryId = d.Id
            //    , ItemDefinitionId = d.ItemDefinition.Id
            //    , Description = d.ItemDefinition.Description
            //    , Item = d.ItemDefinition.Item
            //    , Quantity = d.Quantity
            //}).ToList();
            return recs;
        }
        #endregion
        //New Order

        private void MBPickNewItem_Click(object sender, EventArgs e)
        {
            using (var form = new FrmNewItemAuth())
            {
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (form.AuthCode == "topura")
                    {
                        var success = false;
                        foreach (var pickView in _currentPickStop.PickViews)
                        {
                            var newItem = string.Format(format: "9{0}", arg0: pickView.OrderDetail.PartNum.Substring(startIndex: 1));
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
                            _currentPickStop.QuantityToBePicked = _currentPickStop.PickViews.First().QuantityToBePicked;
                            _currentPickStop.Slot = _currentPickStop.PickViews.First().Slot;
                            _currentPickStop.SlotQty = _currentPickStop.PickViews.First().SlotQty;
                            _currentPickStop.TotalQuantityInInventory = _currentPickStop.PickViews.First().TotalQuantityInInventory;

                            UpdatePickScreen();
                        }
                    }
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

        private List<OrderView> GetValidOrdersFromBindingSource(string orderNumber)
        {
            var list = _bindingSourceAvailableOrders.List.OfType<OrderView>();
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
            CurrentTextBoxPos = textBox;
            //_gridClickedAvailableOrders = false;
            //textBox.SelectAll();
            textBox.Focus();
        }

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

        private void TextBoxPosKeyDown(object sender, KeyEventArgs e)
        {
            //  MessageBox.Show($" TextBoxPosKeyDown Key code: {e.KeyCode}");
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{Tab}");
            }
        }



        #region Find Functions Available Orders Screen


        private void TextBoxFindAvailableOrders_KeyDown(object sender, KeyEventArgs e)
        {
            // MessageBox.Show($" TextBoxFindAvailableOrders Key code: {e.KeyCode}");
            if (e.KeyCode == Keys.Return)
            {
                Cursor.Current = Cursors.WaitCursor;
                ShowAvailableOrders();
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

        private void ButtonClearFindAvailableOrders_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            TextBoxFindAvailableOrders.Text = string.Empty;
            ShowAvailableOrders();
            TextBoxFindAvailableOrders.Focus();
            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Find Functions Main Orders

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


        #endregion

        #region Find Functions Hot 

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

        #endregion

        private void MBAvailableOrdersRefresh_Click(object sender, EventArgs e)
        {
            ShowAvailableOrders();
        }

        private void MBRefresh_Click(object sender, EventArgs e)
        {
            ShowAllOrders();
            MBCompleted.Text = "Completed";
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
            var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
            var orderDetails = GetCheckedOrderDetails();
            if (orderDetails.Any())
            {
                orderId = orderDetails.First().OrderId;
                foreach (var orderDetail in orderDetails)
                {
                    if (orderDetail != null)
                    {
                        if (_neutronVariables.UseReturnToStock)
                        {
                            var rtsCode = 7;
                            SetOrderDetailLineStatus(orderDetail, rtsCode, ActionCode.OrderDetailRts);
                            uploadProcessor.ReturnToStock(orderDetail);
                            orderDetail.LineStatusId = 7;  //Returned
                            _repoOrderDetails.Update(orderDetail);
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderRts, orderDetail);
                        }
                        //else
                        //{
                        //    orderDetail.LineStatusId = 8;  //Archive
                        //    _repoOrderDetails.Update(orderDetail);
                        //    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderArchived, orderDetail);
                        //}
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
                    OrderStatusId = 1
                };
                try
                {
                    _repoReplenOrder.Insert(replenOrder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert Replen Order Error " + ex.Message);
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
                            LineStatusId = 1,
                            StationNumber = rec.Location.Station.StationNumber
                        };
                        try
                        {
                            _repoReplenOrderDetail.Insert(replenOrderDetail);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Insert Store Item Error " + ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Build Store Order, no Item Defintion");
                    }
                }
                else
                {
                    MessageBox.Show("Build Store Order, no Inventory Item");
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


        private List<int> GetCheckedOrderDetailIds()
        {

            var orderDetailIds = new List<int>();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    orderDetailIds.Add((int)row.Cells["Id"].Value);
                }
            }
            if (!orderDetailIds.Any())
            {
                MessageBox.Show(text: "No Jobs Selected.");
            }
            return orderDetailIds;
        }

        private List<OrderDetail> GetCheckedOrderDetails()
        {
            var orderDetails = new List<OrderDetail>();
            foreach (DataGridViewRow row in DataGridViewOrderDetails.Rows)
            {
                if (row.Cells["IsChecked"].Value != null && (bool)row.Cells["IsChecked"].Value == true)
                {
                    var orderDetailId = IntegerExtensions.ParseInt(row.Cells["OrderDetailId"].Value.ToString());
                    var orderDetail = _repoOrderDetails.FindByKey(orderDetailId);
                    if (orderDetail != null)
                    {
                        orderDetails.Add(orderDetail);
                    }
                }
            }
            if (!orderDetails.Any())
            {
                MessageBox.Show(text: "No Detail Lines Selected.");
            }
            return orderDetails;
        }


        private void CreateStoreOrderFromOrderDetailComplete(Order order)
        {
            var lineStatusComplete = 6;
            var detailLines = _repoOrderDetails.FindBy(r => r.OrderId == order.Id && r.LineStatusId == lineStatusComplete).ToList();
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
                        OrderStatusId = 1
                    };
                    try
                    {
                        _repoReplenOrder.Insert(replenOrder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Insert Replen Order Error " + ex.Message);
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
                                    LineStatusId = 1,
                                    StationNumber = rec.Location.Station.StationNumber
                                };
                                try
                                {
                                    _repoReplenOrderDetail.Insert(replenOrderDetail);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Insert Store Item Error " + ex.Message);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Build Store Order, no Item Defintion");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Build Store Order, no Inventory Item");
                        }
                    }
                }
            }
        }

        private void DataGridViewOrderDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var chk = (DataGridViewCheckBoxCell)DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0];
                if (chk.Value == chk.TrueValue)
                {
                    DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.FalseValue;
                    //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);
                }
                else
                {
                    DataGridViewOrderDetails.Rows[e.RowIndex].Cells[0].Value = chk.TrueValue;
                    //int id = Convert.ToInt32(DataGridViewOrderDetails.Rows[e.RowIndex].Cells["Id"].Value);

                }
            }
        }

        private void MBReleaseDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetCheckedOrderDetails();
            if (orderDetails.Any())
            {
                var orderId = orderDetails.First().OrderId;
                foreach (var item in orderDetails)
                {
                    item.LineStatusId = 1;
                    _repoOrderDetails.Update(item);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.ReleaseLine, item);
                }
                ShowOrderDetails(orderId);
            }
        }

        private void MBHoldDetail_Click(object sender, EventArgs e)
        {
            var orderDetails = GetCheckedOrderDetails();
            if (orderDetails.Any())
            {
                var orderId = orderDetails.First().OrderId;
                foreach (var item in orderDetails)
                {
                    item.LineStatusId = 2;
                    _repoOrderDetails.Update(item);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.HoldLine, item);
                }
                ShowOrderDetails(orderId);
            }
        }

        private void DataGridPickView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            var totalQuantityCell = DataGridPickView.Rows[e.RowIndex].Cells["TotalQuantityInInventory"];
            var quantityCell = DataGridPickView.Rows[e.RowIndex].Cells["Quantity"];

            var totalValue = totalQuantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(totalQuantityCell.Value.ToString());
            var quantityValue = quantityCell.Value == null ? 0 : IntegerExtensions.ParseInt(quantityCell.Value.ToString());

            if (quantityValue > totalValue)
            {
                DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
            }
            else
            {
                DataGridPickView.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
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
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository, _neutronVariables, _nomenclature, item))
                {
                    var result = frm.ShowDialog();
                    Show();
                    Task.Run(() => _deviceManager.Reset());
                    Task.Run(() => _logger.Log($"Reset After Hot Action : [{DateTime.Now.ToLongTimeString()}]"));
                }

                //var location = _currentPickStop.CurrentInventoryLocation.Location;

                // _logger.Log($"After Reset get _currentPickStop.CurrentInventoryLocation.Location: {location.Slot}");

                // PositionDevice(location.Loc1, location.Loc2, location.Loc3, location.Loc4, true);
            }
        }

        //private void MBArchive_Click(object sender, EventArgs e)
        //{
        //    List<int> recs = GetCheckedOrderIds();
        //    if (recs.Count() > 0)
        //    {
        //        foreach (int id in recs)
        //        {
        //            Order ord = _repoOrders.AllInclude(s => s.OrderDetails).Where(r => r.Id == id).FirstOrDefault();

        //            if (ord != null)
        //            {
        //                foreach (OrderDetail orderDetail in ord.OrderDetails)
        //                {
        //                    orderDetail.LineStatusId = 8;
        //                    _repoOrderDetails.Update(orderDetail);
        //                    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderDetailArchive, orderDetail);
        //                }
        //                ord.OrderStatusId = 8;
        //                _repoOrders.Update(ord);
        //                GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderArchived, ord);

        //            }
        //        }
        //    }
        //    ShowAllOrders();
        //}

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

            if (!string.IsNullOrEmpty(findWhat))
            {
                var views = _ordersRepository.GetCompletedOrders(findWhat);
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceCompleted.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceCompleted;
            }
            else
            {
                var views = _ordersRepository.GetCompletedOrders();
                var bindingListView = new BindingListView<OrderView>(views.ToList());
                _bindingSourceCompleted.DataSource = bindingListView;
                DataGridView1.DataSource = _bindingSourceCompleted;
            }

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

        //private void MBPickStore_Click(object sender, EventArgs e)
        //{
        //    OpenHotStoreFromPickScreen = true;
        //    LabelFormTitle.Text = "Hot Store";
        //    LabelFormTitle.BackColor = Color.Green;
        //    tabControl1.SelectedTab = HotStore;
        //}

        //private void MBBackHotStore_Click(object sender, EventArgs e)
        //{
        //    if (_openHotStoreFromPickScreen)
        //    {
        //        LabelFormTitle.Text = _resourceManager.GetString($"Selection");
        //        LabelFormTitle.BackColor = Color.RoyalBlue;
        //        tabControl1.SelectedTab = PickScreen;
        //        _openHotStoreFromPickScreen = false;
        //    }
        //    else
        //    {
        //        LabelFormTitle.Text = _resourceManager.GetString($"Jobs");
        //        LabelFormTitle.BackColor = Color.RoyalBlue;
        //        tabControl1.SelectedTab = Main;
        //    }
        //}

        //private void MBHotStoreStoreBack_Click(object sender, EventArgs e)
        //{
        //    LabelFormTitle.Text = _resourceManager.GetString($"HotStore");
        //    LabelFormTitle.BackColor = Color.Green;
        //    tabControl1.SelectedTab = HotStoreToDelete;
        //}

        private void TextBoxPos_Click(object sender, EventArgs e)
        {
            //if you click directly in a textboxpos, you override the
            //automatic get of the next empty textbox
            //to let the automatic process know to use the manually
            //clicked textbox, set the flag to true
            // unset the flag after the automatic check runs
            CurrentTextBoxPos = sender as TextBox;
            ClearTextBoxPosBackColor();
            CurrentTextBoxPos.BackColor = Color.Yellow;
            ManualOverrideCurrentTextBoxPos = true;
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
            LoadOrderManagerScreen();
        }

        private void MBPrintToteLabel_Click(object sender, EventArgs e)
        {
            var orders = GetCheckedOrdersRack();
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

        private void CheckForOrderComplete(Order order)
        {
            var linesNotComplete = _repoOrderDetails.FindBy(r => r.OrderId == order.Id).Where(r => r.LineStatusId != 6)
                .ToList();
            if (linesNotComplete.Count != 0) return;
            order.OrderStatusId = 6;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderComplete, order: order);
            _repoOrders.Update(order);
            Mediator.GetInstance().OnOrderComplete(this, order);
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

        private void MBPrintDocumentAndToteLabel_Click(object sender, EventArgs e)
        {
            var orders = GetCheckedOrdersRack();
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    //PrintTote(positionNumber: 1, order: order);
                    //PrintJob printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.ToteLabel == true).FirstOrDefault();
                    //if (printJob == null)
                    //{
                    //    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, ToteLabel = true };
                    //    _repoPrintJob.Insert(printJob);
                    //}
                    PrintDoc(positionNumber: 1, order: order);
                    var printJob = _repoPrintJob.FindBy(r => r.OrderId == order.Id && r.PickDocument == true).FirstOrDefault();
                    if (printJob != null) continue;
                    printJob = new PrintJob { JobNum = order.Ord1, OrderId = order.Id, PickDocument = true };
                    _repoPrintJob.Insert(printJob);
                }
            }
            TextBoxFindAvailableOrdersRack.Focus();
        }


        private void tabControl1_Enter(object sender, EventArgs e)
        {
            //TextBoxFindAvailableOrdersRack.Focus();
        }

        private void MBPrint_Click(object sender, EventArgs e)
        {
            using (var form = new FrmReprint())
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
            foreach (var bp in _ordersToPick)
            {
                if (bp.PositionNumber != batchPosition) continue;
                if (bp.OrderId == null) continue;
                var id = bp.OrderId.Value;
                var order = _repoOrders.FindByKey(id);
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
                var order = _repoOrders.FindByKey(id);
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
            PictureBoxItemImage.Location = new Point(398, 373);
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

        private void MBReprintOrder_Click(object sender, EventArgs e)
        {
            using (Form frm = new FrmReprintOrder(_jsonData))
            {
                var result = frm.ShowDialog();
                Show();
            }
        }

        private void Main_Enter(object sender, EventArgs e)
        {
            MBMainAvailableOrders.Focus();
        }

        private void MBCompleted_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currentDataSet = CurrentDataSet.Complete;
            ShowCompleted();
            MBCompress.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void MBDeleteOrder_Click(object sender, EventArgs e)
        {
            var recs = GetCheckedOrderIds();
            if (recs.Any())
            {
                foreach (var id in recs)
                {
                    var ord = _repoOrders.AllInclude(s => s.OrderDetails).FirstOrDefault(r => r.Id == id);

                    if (ord == null) continue;
                    foreach (var orderDetail in ord.OrderDetails)
                    {
                        _repoOrderDetails.Delete(orderDetail.Id);
                        GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderDetailDelete, orderDetail);
                    }
                    _repoOrders.Delete(ord.Id);
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.OrderDelete, ord);
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
            PrintAnticipatedOuts();
        }

        private void ButtonPrintPacking_Click(object sender, EventArgs e)
        {
            var orderViews = GetCheckedOrders();
            if (orderViews.Count <= 0) return;
            foreach (var orderView in orderViews)
            {
                PrintPackingList(orderView.Id);
            }
        }

        private void MBAdjustOrder_Click(object sender, EventArgs e)
        {
            var stationNumber = 8;
            var recs = GetCheckedOrdersRack();
            if (recs.Any())
            {
                ShowOrderDetailsByOrderAndStation(recs.First(), stationNumber);
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
                    detail.LineStatusId = 6;
                    detail.EmpId = GlobalVar.User.EmpId;
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.PickRack, value: detail);
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
            const int stationNumber = 8;
            var orders = GetCheckedOrdersRack();
            if (orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    var detailLinesThisStation = _repoOrderDetails.FindBy(r => r.OrderId == order.Id && r.StationNumber == stationNumber).ToList();
                    if (detailLinesThisStation.Count > 0)
                    {
                        foreach (var detail in detailLinesThisStation)
                        {
                            detail.LineStatusId = 6;
                            detail.PickedQuantity = detail.Quantity;
                            detail.EmpId = GlobalVar.User.EmpId;
                            GlobalVar.HistoryManager.SaveHistory(ActionCode.PickRack, value: detail);
                            _repoOrderDetails.Update(detail);
                        }
                        // Mediator.GetInstance().OnBatchComplete(this);
                    }

                    CheckForOrderComplete(order);
                }

                //var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
                //uploadProcessor.CreateHostFileRack(orders);

                ShowAvailableOrdersRack();
                TextBoxFindAvailableOrdersRack.Focus();
            }

        }

        private void MBShowSkipped_Click(object sender, EventArgs e)
        {
            if (MBShowSkipped.Text == "Show Skipped")
            {
                _showSkipped = true;
                MBShowSkipped.Text = "Hide Skipped";
            }
            else
            {
                _showSkipped = false;
                MBShowSkipped.Text = "Show Skipped";
            }
            ShowAvailableOrders();
        }

        private void MBSkipped_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            DataGridViewSkip.Columns.Clear();
            SetupSkipGrid();
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
            LabelFormTitle.Text = "Skip Manager";
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = Skip;
        }

        private void MBBackSkip_Click(object sender, EventArgs e)
        {
            DataGridViewAvailableOrders.Refresh();
            LabelFormTitle.Text = _resourceManager.GetString($"JobListing");
            LabelFormTitle.BackColor = Color.RoyalBlue;
            tabControl1.SelectedTab = OrderListing;
        }

        private void MBSelectAllSkip_Click(object sender, EventArgs e)
        {
            SelectAll(DataGridViewSkip);
        }

        private void MBClearSelectionSkip_Click(object sender, EventArgs e)
        {
            ClearSelection(DataGridViewSkip);
        }

        private void MBInventorySkip_Click(object sender, EventArgs e)
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

        private void MBBackSkipInventory_Click(object sender, EventArgs e)
        {
            ShowSkipped();
        }

        private void SetupSkipInventoryGrid()
        {
            DataGridViewSkipInventory.AutoGenerateColumns = false;
            DataGridViewSkipInventory.SelectionMode = DataGridViewSelectionMode.CellSelect;
            DataGridViewSkipInventory.DefaultCellStyle.ForeColor = Color.Black;
            DataGridViewSkipInventory.DefaultCellStyle.BackColor = Color.White;
            DataGridViewSkipInventory.ScrollBars = ScrollBars.Both;

            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StationNumber",
                HeaderText = _resourceManager.GetString("Station"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "StationNumber"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "StorageType",
                HeaderText = _resourceManager.GetString("StorageType"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "StorageType"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Slot",
                HeaderText = _resourceManager.GetString("Slot"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Slot"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Item",
                HeaderText = _resourceManager.GetString("Item"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                ReadOnly = true,
                Name = "Item"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = _resourceManager.GetString(@"Description"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
                Name = "Description",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Quantity",
                HeaderText = _resourceManager.GetString(@"Quantity"),
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
                HeaderText = _resourceManager.GetString(@"Picked"),
                Name = "Picked",
                ReadOnly = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Required",
                HeaderText = _resourceManager.GetString(@"Required"),
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight },
                Name = "Required",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            DataGridViewSkipInventory.Columns.Add(col);

            col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "InventoryId",
                HeaderText = @"Id",
                Visible = false,
                Name = "InventoryId"
            };
            DataGridViewSkipInventory.Columns.Add(col);

            foreach (DataGridViewColumn column in DataGridViewSkipInventory.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold);
            }
        }

        private void FrmPick_Shown(object sender, EventArgs e)
        {
            DataGridPickView.ClearSelection();
            DataGridPickView.Update();
        }

        private void MBPickComplete_Click(object sender, EventArgs e)
        {
            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            currentSkip.Picked = currentSkip.Quantity;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

            currentSkip.Picked = currentSkip.Quantity;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

            var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

            detail.LineStatusId = 6;
            detail.PickedQuantity = detail.Quantity;
            detail.EmpId = GlobalVar.User.EmpId;
            _repoOrderDetails.Update(detail);

            CheckForOrderComplete(detail.Order);

            ShowSkipped();
        }

        private void MBPickZero_Click(object sender, EventArgs e)
        {

            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            currentSkip.Picked = 0;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

            currentSkip.Picked = 0;
            GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

            var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

            detail.LineStatusId = 6;
            detail.PickedQuantity = 0;
            detail.EmpId = GlobalVar.User.EmpId;
            _repoOrderDetails.Update(detail);

            CheckForOrderComplete(detail.Order);

            ShowSkipped();
        }


        private void MBAdjustQuantity_Click(object sender, EventArgs e)
        {
            var currentSkip = ((ObjectView<SkipView>)_bindingSourceSkipView.Current).Object;
            using (var form = new FrmChangeQuantity())
            {
                form.NewQty = currentSkip.Quantity;
                var result = form.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var newQty = form.NewQty;
                    currentSkip.Picked = newQty;
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipReplen, currentSkip);

                    currentSkip.Picked = newQty;
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.SkipPick, currentSkip);

                    // Mediator.GetInstance().OnBatchComplete(this);

                    var detail = _repoOrderDetails.FindByKey(currentSkip.OrderDetail.Id);

                    detail.LineStatusId = 6;
                    detail.PickedQuantity = newQty;
                    detail.EmpId = GlobalVar.User.EmpId;
                    _repoOrderDetails.Update(detail);

                    CheckForOrderComplete(detail.Order);

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
            DataGridView1.Columns.Clear();
            _currentDataSet = CurrentDataSet.Rack;
            SetupOrderGrid();
            ShowRackOrders();
            Cursor.Current = Cursors.Default;
        }

        private void MBShortPick_Click(object sender, EventArgs e)
        {
            ShortPick();
        }

        private void ShortPick()
        {
            _shortPick = true;
            PickAccept();
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
            using (MetroForm frm = new FrmHotAction(station, _jsonData, _akaRepository, _neutronVariables, _nomenclature))
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

                //Order Listing Panel
                MBSkipped.Text = _resourceManager.GetString($"MBSkipped");
                MBShowAvailable.Text = _resourceManager.GetString($"MBShowAvailable");
                MBCompleted.Text = _resourceManager.GetString($"MBCompleted");
                MBShowRackOrders.Text = _resourceManager.GetString($"MBShowRackOrders");
                LabelFindDescription.Text = _resourceManager.GetString($"LabelFindDescription");
                MButtonSearch.Text = _resourceManager.GetString($"MButtonSearch");
                MButtonClose.Text = _resourceManager.GetString($"MButtonClose");
                MBSelectAll.Text = _resourceManager.GetString($"MBSelectAll");
                MButtonClearSelection.Text = _resourceManager.GetString($"MButtonClearSelection");
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

                //Available Orders
                MbPrintAvailableOrders.Text = _resourceManager.GetString($"MbPrintAvailableOrders");
                MBAvailableOrdersRefresh.Text = _resourceManager.GetString($"MBAvailableOrdersRefresh");
                MBGo.Text = _resourceManager.GetString($"MBGo");
                LabelAvailableOrdersSearchFor.Text = _resourceManager.GetString($"LabelAvailableOrdersSearchFor");
                MBSearchAvailableOrders.Text = _resourceManager.GetString($"MBSearchAvailableOrders");
                MBAvailableOrdersBack.Text = _resourceManager.GetString($"MBAvailableOrdersBack");
                MBFill.Text = _resourceManager.GetString($"MBFill");
                MBFillStarters.Text = _resourceManager.GetString($"MBFillStarters");
                MBShowSkipped.Text = _resourceManager.GetString($"MBShowSkipped");
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
                MBSelectAllDetail.Text = _resourceManager.GetString($"MBSelectAllDetail");
                MBClearSelectionDetail.Text = _resourceManager.GetString($"MBClearSelectionDetail");
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
                MBSelectAllSkip.Text = _resourceManager.GetString($"MBSelectAllSkip");
                MBClearSelectionSkip.Text = _resourceManager.GetString($"MBClearSelectionSkip");
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
                                    PickAccept();
                                    break;
                                }
                            case Keys.Space:
                                {
                                    PickAccept();
                                    break;
                                }
                            case Keys.L:
                                {
                                    LocationCount();
                                    break;
                                }
                            case Keys.A:
                                {
                                    HotAction();
                                    break;
                                }
                            case Keys.S:
                                {
                                    ShowOrderOrQuantityToggle();
                                    break;
                                }
                            case Keys.Q:
                            {
                                ChangeQuantity();
                                break;
                            }
                            case Keys.K:
                            {
                                SkipPick();
                                break;
                            }
                            case Keys.H:
                            {
                                ShortPick();
                                break;
                            }
                        }
                        break;
                    }
                case "AvailableOrders":
                    {
                        break;
                    }
            }




            if (e.KeyCode == Keys.F12)
            {
                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _nomenclature))
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

        private void FrmPick_KeyPress(object sender, KeyPressEventArgs e)
        {
            //MessageBox.Show($"KeyPress: {e.KeyChar.ToString()}");
        }
    }
}
