#region

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using JsonManager;
using MetroFramework.Forms;
using Neutron.Controllers;
using Neutron.Enums;
using Neutron.Forms;
using Neutron.Global;
using Neutron.Interfaces;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronEvents;
using NeutronLoader;
using SlotNameFactory;
using System.Linq;
using System.Text;
using System.Timers;
using Neutron.Models;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using Timer = System.Timers.Timer;

#endregion

namespace Neutron
{
    public partial class FrmMain
    {
        private static User _currentUser;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly IStationRepository _stationRepository;
        private readonly IImageManager _imageManager;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IReplenOrdersRepository _replenOrdersRepository;
        private readonly IEnumManager _enumManager;
        private readonly IJsonData _jsonData;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private StationView _station;
        private int _stationId;
        private DynamicLogger _logger;
        private string _logFileDir = string.Empty;
        private readonly IAkaRepository _akaRepository;
        private readonly ILacProcessor _lacProcessor;
        private readonly Timer _compressTimer;
        private bool _compressRunning;
        private Station _rackStation;
        private StartStopLoaderManager _startStopLoaderManager;
        private StartStopUploadManager _startStopUploadManager;

        /// <summary>
        /// Passed from NInject Kernel
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="akaRepository"></param>
        /// <param name="securityProcessor"></param>
        /// <param name="lacProcessor"></param>
        /// <param name="stationRepository"></param>
        /// <param name="imageManager"></param>
        /// <param name="ordersRepository"></param>
        /// <param name="replenOrdersRepository"></param>
        /// <param name="enumManager"></param>
        public FrmMain(IJsonData jsonData, IAkaRepository akaRepository
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IStationRepository stationRepository
            , IOrdersRepository ordersRepository, IReplenOrdersRepository replenOrdersRepository
            , IEnumManager enumManager)
        {
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            KeyPreview = true;

            _jsonData = jsonData;
            _akaRepository = akaRepository;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;
            _stationRepository = stationRepository;
            _imageManager = imageManager;
            _ordersRepository = ordersRepository;
            _replenOrdersRepository = replenOrdersRepository;
            _enumManager = enumManager;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = _jsonData.LoadFile<NeutronLicense>();
            _rackStation = _stationRepository.GetRackStation();

            GlobalVar.HistoryManager = new HistoryManager();


            Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
                , "Inventory File", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            Mediator.GetInstance().InventoryFileCreatedError += (s, e) => MessageBox.Show(e.Text, "Inventory File Error"
                , MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            LogOnOff();

            if (!InitForm())
            {
                MessageBox.Show("Neutron has failed to load properly.  Close Neutron and fix error before restarting.", "Main Form Error", MessageBoxButtons.OK);
                return;
            }
            if (_station.StationTypeId == (int)StationType.Supervisor)
            {
                if (_neutronVariables.UseAutoCompress)
                {
                    _compressTimer = new Timer();
                    _compressTimer.Interval = _neutronVariables.RunCompressInterval * 60 * 60; 
                    _compressTimer.Elapsed += OnRunCompress;
                    _compressTimer.AutoReset = true;
                    _compressTimer.Enabled = true;
                }
            }

            var id = Thread.CurrentThread.ManagedThreadId;
            Trace.WriteLine("FrmMain thread: " + id);
        }

        private void OnRunCompress(object sender, ElapsedEventArgs e)
        {
            if (_compressRunning) return;

            var compressLastRunDate = _jsonData.LoadFile<CompressLastRunDate>();
            int days = (DateTime.Now.Date - compressLastRunDate.DateTime.Date).Days;
            //Run once each day
            if (days > 0)
            {
                var daysToKeep = _neutronVariables.CompressDays * -1;
                var compressBefore = DateTime.Now.Date.AddDays(daysToKeep);
                CompressOrders(compressBefore);
                CompressReplenOrders(compressBefore);

                compressLastRunDate = new CompressLastRunDate { DateTime = DateTime.Now };
                _jsonData.SaveFile(compressLastRunDate);

            }
            _compressRunning = false;
        }

        private void CompressOrders(DateTime compressBefore)
        {
            _compressRunning = true;
            // Compress Normal Orders

            var completedOrders = _ordersRepository.GetCompletedOrders(string.Empty).ToList();
            var ordersToCompress = completedOrders.Where(r => r.LoadDate < compressBefore).ToList();

            if (!ordersToCompress.Any()) return;
            var orderType = "PICK";
            var sb = new StringBuilder();
            var firstTime = true;
            foreach (var order in ordersToCompress)
            {
                if (firstTime)
                {
                    sb.Append(order.Id);
                    firstTime = false;
                }
                else
                {
                    sb.Append(", " + order.Id);
                }
            }

            var orderIds = sb.ToString();

            try
            {
                using (var context = new NeutronDb())
                {
                    var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                    var paramOrderType = new SqlParameter("@ORDERTYPE", orderType);
                    var parameters = new object[] { paramOrderIds, paramOrderType };
                    context.Database.ExecuteSqlCommand("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds,
                        paramOrderType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Compressing Orders {Environment.NewLine}{ex.Message}", "Compress Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CompressReplenOrders(DateTime compressBefore)
        {
            _compressRunning = true;
            // Compress Replenishment Orders
            var completedReplenOrders = _replenOrdersRepository.GetCompletedOrders(string.Empty).ToList();
            var replenOrdersToCompress = completedReplenOrders.Where(r => r.LoadDate < compressBefore).ToList();

            if (!replenOrdersToCompress.Any()) return;
            var orderType = "REPLEN";
            var sb = new StringBuilder();
            var firstTime = true;
            foreach (var order in replenOrdersToCompress)
            {
                if (firstTime)
                {
                    sb.Append(order.Id);
                    firstTime = false;
                }
                else
                {
                    sb.Append(", " + order.Id);
                }
            }

            var orderIds = sb.ToString();

            try
            {
                using (var context = new NeutronDb())
                {
                    var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                    var paramOrderType = new SqlParameter("@ORDERTYPE", orderType);
                    var parameters = new object[] { paramOrderIds, paramOrderType };
                    context.Database.ExecuteSqlCommand("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds,
                        paramOrderType);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Compressing Replenishment Orders {Environment.NewLine}{ex.Message}", "Compress Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool InitForm()
        {
            var result = false;
            _enumManager.SaveActionCodesToDatabase();
            _enumManager.SaveLineStatusToDatabase();
            try
            {
                ButtonPark.Visible = _neutronLicense.CompanyCode == "TOP";
                if (LoaderSettings.Init())
                {
                    _stationId = _neutronVariables.StationId;
                    if (_stationId > 0)
                    {
                        _station = _stationRepository.GetStationView(_stationId);
                        if (CreateLog("Main", _station.StationNumber))
                        {
                            _logger.Log($"Startup: CompanyCode: {_neutronLicense.CompanyCode}");
                            var rackStation = _stationRepository.GetRackStation();
                            _startStopLoaderManager = new StartStopLoaderManager(_jsonData, _logger, _neutronVariables, _neutronLicense, rackStation);
                            _startStopUploadManager = new StartStopUploadManager(_jsonData, _logger, _neutronVariables, _neutronLicense, rackStation);
                            if (_station != null)
                            {
                                if (SetupShuttle())
                                {
                                    if (SetupDisplay())
                                    {
                                        if (SetupSlotFactory())
                                        {
                                            if (StartLoader())
                                            {
                                                if (StartUpload())
                                                {
                                                    result = true;
                                                }
                                                else
                                                {
                                                    MessageBox.Show("Main Form: Auto Upload Initialization Error.");
                                                }
                                            }
                                            else
                                            {
                                                MessageBox.Show("Main Form: Auto Loader Initialization Error.");
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Main Form: Slot Factory Initialization Error.");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Main Form: Display Initialization Error.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Main Form: Device Initialization Error.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Main Form: Station Initialization Error.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Unable to create the log file.   Neutron Exiting.",
                                caption: "File Error", buttons: MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Station has not been configured.   Neutron Exiting.",
                            caption: "Bad Configuration", buttons: MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show("Main Form: LoaderSettings Initialization Error.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main Form Initialization Error.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            return result;
        }

        private bool StartLoader()
        {
            var result = false;
            try
            {
                if (_neutronVariables.RunLoaderOnStartup)
                {
                    Mediator.GetInstance().OnStartStopLoader(this, "Start");
                    GlobalVar.LoaderRunning = true;
                    result = true;
                }
                else
                {
                    //don't run on startup, return true
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The Loader has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            return result;
        }

        private bool StartUpload()
        {
            var result = false;
            try
            {
                if (_neutronVariables.RunUploadOnStartup)
                {
                    Mediator.GetInstance().OnStartStopUpload(this, "Start");
                    GlobalVar.UploadRunning = true;
                    result = true;
                }
                else
                {
                    //don't run on startup, return true
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The Upload has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            return result;
        }

        private bool CreateLog(string name, int stationNumber)
        {
            bool result;
            try
            {
                _logFileDir = LoaderSettings.GetLogFileDirectory();
                var folderName = ($"{name}_{stationNumber.ToString()}");
                var logActivity = LoaderSettings.EnableLogging;
                _logger = new DynamicLogger(_logFileDir, folderName, logActivity);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Neutron was unable to create the Main Form Log.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            return result;
        }

        private bool SetupDisplay()
        {
            bool result;
            try
            {
                if (_neutronVariables.DisplaysEnabled)
                {
                    if (GlobalVar.Displays == null)
                    {
                        if (_neutronVariables.IptiDisplays)
                        {
                            // ReSharper disable once UseObjectOrCollectionInitializer
                            GlobalVar.Displays = new IptiController(_jsonData, _station, _neutronVariables);
                            //GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
                            result = GlobalVar.Displays != null;
                        }
                        else
                        {
                            //Remstar Displays are the default
                            GlobalVar.Displays = new DisplayController(_jsonData, _station);
                            result = GlobalVar.Displays != null;
                        }
                    }
                    else
                    {
                        //Displays already exist
                        result = true;
                    }
                }
                else
                {
                    // no displays enabled, return true
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up display controller.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            return result;
        }

        private bool SetupShuttle()
        {
            var result = false;
            try
            {
                if (_neutronVariables.ShuttleEnabled)
                {
                    _logger.Log("Shuttle Enabled - Setup.");
                    if (_station.HardwareDevices.Count > 0)
                    {
                        if (_neutronVariables.DeviceDriver == DeviceDriverName.C3000() && GlobalVar.Shuttle == null)
                        {
                            _logger.Log("C3000 Controller.");
                            GlobalVar.Shuttle = new C3000(this, _station);
                            GlobalVar.Shuttle.InitStatus();
                            result = GlobalVar.Shuttle != null;
                        }

                        if (_neutronVariables.DeviceDriver == DeviceDriverName.C2000() && GlobalVar.Shuttle == null)
                        {
                            _logger.Log("C2000 Controller.");
                            GlobalVar.Shuttle = new C2000(this, _station);
                            GlobalVar.Shuttle.InitStatus();
                            result = GlobalVar.Shuttle != null;
                        }

                        if (_neutronVariables.DeviceDriver == DeviceDriverName.RCC2() && GlobalVar.Shuttle == null)
                        {
                            _logger.Log("RCC2 Controller.");
                            GlobalVar.Shuttle = new RCC2(this, _station);
                            GlobalVar.Shuttle.InitStatus();
                            result = GlobalVar.Shuttle != null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Main Form Init: Hardware device count error.");
                    }
                }
                else
                {
                    // no shuttle enabled, return true
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error setting up device controller.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            return result;
        }

        public void UpdateInitStatus(bool success, int percentage)
        {
            MessageBox.Show($"Success: {success}  Percent: {percentage}%");
        }

        private bool SetupSlotFactory()
        {
            bool result;
            switch (_neutronVariables.SlotNameType)
            {
                case "Default":
                    GlobalVar.SlotNameFactory = DefaultSlotNameFactory.GetInstance();
                    result = GlobalVar.SlotNameFactory != null;
                    break;
                case "T101-01-01":
                    GlobalVar.SlotNameFactory = Type1SlotNameFactory.GetInstance();
                    result = GlobalVar.SlotNameFactory != null;
                    break;
                case "V101":
                    GlobalVar.SlotNameFactory = Type2SlotNameFactory.GetInstance();
                    result = GlobalVar.SlotNameFactory != null;
                    break;
                case "01--01--01--01":
                    GlobalVar.SlotNameFactory = Type3SlotNameFactory.GetInstance();
                    result = GlobalVar.SlotNameFactory != null;
                    break;
                default:
                    GlobalVar.SlotNameFactory = DefaultSlotNameFactory.GetInstance();
                    result = GlobalVar.SlotNameFactory != null;
                    break;
            }

            return result;
        }

        public void SetMtLogOffText()
        {
            MtLogOff.Text = _resourceManager.GetString("LogOn");
            GlobalVar.User = null;
            _currentUser = null;
            mlUserInfo.Text = "";
            _securityProcessor.ReprocessSecuritySet("");
        }

        private void MtLogOff_Click(object sender, EventArgs e)
        {
            LogOnOff();
            //if (MtLogOff.Text == "Log Off")
            //{
            //    MtLogOff.Text = "Log On";
            //    GlobalVar.User = null;
            //    currentUser = null;
            //    mlUserInfo.Text = "";
            //    securityProcessor.ReprocessSecuritySet("");
            //}
            //else
            //{
            //    try
            //    {
            //        MtLogOff.Text = "Log Off";
            //        if (neutronVariables.PinLoginOnly)
            //        {
            //            using (var frm = new FrmPin())
            //            {
            //                DialogResult result = frm.ShowDialog();
            //                if (result == DialogResult.OK)
            //                {
            //                    currentUser = frm.CurrentUser;
            //                    mlUserInfo.Text = currentUser.UserInfo;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            using (var frm = new FrmLogin())
            //            {
            //                DialogResult result = frm.ShowDialog();
            //                if (result == DialogResult.OK)
            //                {
            //                    currentUser = frm.CurrentUser;
            //                    mlUserInfo.Text = currentUser.UserInfo;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("Login Error " + ex.Message);
            //    }
            //}
            //if (currentUser != null)
            //{
            //    GlobalVar.User = currentUser;
            //    securityProcessor.ReprocessSecuritySet(currentUser.Pin);
            //}
        }

        private void LogOnOff()
        {
            if (MtLogOff.Text == _resourceManager.GetString("LogOff"))
            {
                MtLogOff.Text = _resourceManager.GetString("LogOn");
                GlobalVar.User = null;
                _currentUser = null;
                mlUserInfo.Text = "";
                _securityProcessor.ReprocessSecuritySet("");
                _lacProcessor.ReprocessLacSet(0);
            }
            else
            {
                try
                {
                    MtLogOff.Text = _resourceManager.GetString("LogOff");
                    if (_neutronVariables.PinLoginOnly)
                    {
                        using (var frm = new FrmPin())
                        {
                            DialogResult result = frm.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                _currentUser = frm.CurrentUser;

                                mlUserInfo.Text = $"{_resourceManager.GetString("CurrentUser")}{_currentUser.UserInfo}";
                            }
                        }
                    }
                    else
                    {
                        using (var frm = new FrmLogin())
                        {
                            DialogResult result = frm.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                _currentUser = frm.CurrentUser;
                                mlUserInfo.Text = $"{_resourceManager.GetString("CurrentUser")}{_currentUser.UserInfo}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Login Error " + ex.Message);
                }
            }

            if (_currentUser != null)
            {
                GlobalVar.User = _currentUser;
                var cultureInfo = GlobalVar.User.Language.CultureInfo;
                if (cultureInfo.Length == 5 && cultureInfo.Contains('-'))
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cultureInfo);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo);

                }
                else
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                }
                _cultureInfo = Thread.CurrentThread.CurrentCulture;
                SetCulture(_cultureInfo.Name);

                _securityProcessor.ReprocessSecuritySet(GlobalVar.User.Pin);
                _lacProcessor.ReprocessLacSet(GlobalVar.User.Id);
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MtItemDefinitions_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageItems])
            {
                Hide();
                using (MetroForm frm = new FrmItemDefinitions(_jsonData, _station, _akaRepository, _imageManager))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtLocations_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageLocations])
            {
                Hide();
                using (MetroForm frm = new FrmLocations(_jsonData, _station, _neutronVariables, _lacProcessor))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtInventory_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageInventory])
            {
                var main = this;

                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
                {
                    main.Hide();
                    frm.ShowDialog();
                    main.Show();
                }
            }
        }

        private void MtHotAction_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions])
            {
                Hide();
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository
                    , _neutronVariables, _lacProcessor, _imageManager))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtSystem_Click(object sender, EventArgs e)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageSystem]) return;
            Hide();
            using (MetroForm frm = new FrmSystem(_jsonData, _logger, _rackStation))
            {
                frm.ShowDialog();
                Show();
            }
        }

        private void MtPick_Click(object sender, EventArgs e)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.PickItemsandOrders]) return;
            Hide();
            using (MetroForm frm = new FrmPick(_jsonData, _station, _akaRepository, _neutronVariables
                                                , _securityProcessor, _lacProcessor, _imageManager
                                                , _stationRepository, _ordersRepository, _neutronLicense))
            {
                frm.ShowDialog();

                if (_neutronVariables.AutoLogOff)
                {
                    SetMtLogOffText();
                    LogOnOff();
                }

                Show();
            }
        }

        private void MtUtilities_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUtilities])
            {
                Hide();
                using (MetroForm frm = new FrmUtilities(_jsonData, _neutronVariables, _neutronLicense))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtHistory_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ViewHistory])
            {
                Hide();
                using (MetroForm frm = new FrmHistory(_akaRepository))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtProductivity_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUsers])
            {
                Hide();
                using (MetroForm frm = new FrmProductivity(_jsonData))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtStore_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.StoreItemsandOrders])
            {
                Hide();
                using (MetroForm frm = new FrmReplen(_jsonData, _station, _akaRepository,
                    _neutronVariables, _securityProcessor, _lacProcessor, _imageManager,
                    _stationRepository, _replenOrdersRepository, _neutronLicense))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtUsers_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUsers])
            {
                Hide();
                using (Form frm = new FrmSecurity(_neutronVariables))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void MtLac_Click(object sender, EventArgs e)
        {
            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageLac])
            {
                Hide();
                using (Form frm = new FrmLAC(_neutronVariables))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (GlobalVar.Shuttle != null)
            {
                GlobalVar.Shuttle.CloseController();
                if (GlobalVar.Shuttle != null)
                {
                    GlobalVar.Shuttle = null;
                }
            }

            if (GlobalVar.Displays != null)
            {
                GlobalVar.Displays.CloseController();
                if (GlobalVar.Displays != null)
                {
                    GlobalVar.Displays = null;
                }
            }
        }

        private void ButtonPark_Click(object sender, EventArgs e)
        {
            if (GlobalVar.Shuttle != null)
            {
                GlobalVar.Shuttle.Park();
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            _compressTimer?.Stop();
            Close();
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                using (MetroForm frm = new FrmInventory(_jsonData, _station, _akaRepository, _lacProcessor))
                {
                    frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F5 || e.KeyCode == Keys.F6)
            {
                using (MetroForm frm = new FrmHotAction(_station, _jsonData, _akaRepository
                    , _neutronVariables, _lacProcessor, _imageManager))
                {
                    frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.F8)
            {

                using (MetroForm frm = new FrmPick(_jsonData, _station, _akaRepository, _neutronVariables,
                    _securityProcessor, _lacProcessor, _imageManager, _stationRepository
                    , _ordersRepository, _neutronLicense))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }

        private void SetCulture(string lang)
        {
            try
            {
                var languageDirectory = LoaderSettings.GetLanguageDirectory();

                _cultureInfo = CultureInfo.CreateSpecificCulture(lang);
                _resourceManager = ResourceManager.CreateFileBasedResourceManager(baseName: "FrmMain",
                    resourceDir: languageDirectory, usingResourceSet: null);
                LabelWarehouseManagement.Text = _resourceManager.GetString("WarehouseManagement");
                MtInventory.Text = _resourceManager.GetString("Inventory");
                MtItemDefinitions.Text = _resourceManager.GetString("ItemDefinitions");
                MtLocations.Text = _resourceManager.GetString("Locations");
                MtHistory.Text = _resourceManager.GetString("History");
                MtProductivity.Text = _resourceManager.GetString("Productivity");
                MtHotAction.Text = _resourceManager.GetString("HotAction");
                MtPick.Text = _resourceManager.GetString("Pick");
                MtStore.Text = _resourceManager.GetString("Store");
                MtUsers.Text = _resourceManager.GetString("Users");
                MtLogOff.Text = _resourceManager.GetString("LogOn");
                MtUtilities.Text = _resourceManager.GetString("Utilities");
                MtSystem.Text = _resourceManager.GetString("System");
                MtLac.Text = _resourceManager.GetString("LocationAccessControl");
                ButtonPark.Text = _resourceManager.GetString("Park");
                ButtonClose.Text = _resourceManager.GetString("Close");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages.  FrmMain  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void RadioButtonLanguage_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioButtonEnglish.Checked)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }

            if (RadioButtonFrenchCanadian.Checked)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
            }
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            mlUserInfo.Text = $"{_resourceManager.GetString("CurrentUser")}{_currentUser.UserInfo}";
        }
    }
}