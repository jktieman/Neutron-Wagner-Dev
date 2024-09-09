#region

using System;
using System.Collections.Generic;
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
using System.Threading.Tasks;
using System.Web;
using AlliedPostOffice;
using AlliedPostOffice.Concrete;
using Neutron.Classes;
using Neutron.Ninject;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using SqlSchemaManager;
using NeutronData.Repositories;
using NeutronData.Models.Lookups;
//using ProLiteController;
using StationType = NeutronCore.Enums.StationType;
using Application = System.Windows.Forms.Application;
using ProliteController;
using IPTI.Models;
using IDisplayController = IPTI.Models.IDisplayController;
using AsyncAwaitBestPractices;
using LogFileMaintenance;

#endregion

namespace Neutron
{
    public partial class FrmMain
    {
        private static User _currentUser;
        private CultureInfo _cultureInfo;
        private ResourceManager _resourceManager;
        private readonly IWorkstationRepository _workstationRepository;
        private readonly IImageManager _imageManager;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IReplenOrdersRepository _replenOrdersRepository;
        private readonly IEnumManager _enumManager;
        private readonly IItemDefinitionsRepository _itemDefinitionsRepository;
        private readonly IStoredProcedureManager _storedProcedureManager;
        private readonly IJsonData _jsonData;
        private readonly ISecurityProcessor _securityProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IAreaRepository _areaRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private WorkstationView _workstationView;
        private readonly IDynamicLogger _logger;
        private readonly IAkaRepository _akaRepository;
        private readonly ILacProcessor _lacProcessor;
        //  private TcpIptiCommandCenter _tcpIptiCommandCenter;
        private CompressService _compressService;

        private SendEmail _sendEmail;
        private StartStopLoaderManager _startStopLoaderManager;
        private StartStopUploadManager _startStopUploadManager;
        private IHistoryManager _historyManager;
        private IDisplayController _tcpIptiController;
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.DeviceType> _repoDeviceTypes = new GenericRepository<NeutronData.Models.Lookups.DeviceType>(new NeutronDb());
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.SerialConfiguration> _repoSerialConfiguration = new GenericRepository<NeutronData.Models.SerialConfiguration>(new NeutronDb());
        private string _lastEmailMessage;
        private int _lastEmailMessageCounter;
        private IptiConfig _iptiConfig;
        private IptiDisplayFunctions _iptiDisplayFunctions;
        private bool _monitorTransmitter = true;
        private bool _isClientConnected;

        /// <summary>
        /// Passed from NInject Kernel
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="akaRepository"></param>
        /// <param name="securityProcessor"></param>
        /// <param name="lacProcessor"></param>
        /// <param name="workstationRepository"></param>
        /// <param name="imageManager"></param>
        /// <param name="ordersRepository"></param>
        /// <param name="replenOrdersRepository"></param>
        /// <param name="enumManager"></param>
        /// <param name="itemDefinitionsRepository"></param>
        /// <param name="storedProcedureManager"></param>
        /// <param name="neutronVariables"></param>
        /// <param name="neutronLicense"></param>
        /// <param name="areaRepository"></param>
        /// <param name="locationsRepository"></param>
        /// <param name="inventoryRepository"></param>
        public FrmMain(IJsonData jsonData, IAkaRepository akaRepository
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IWorkstationRepository workstationRepository
            , IOrdersRepository ordersRepository, IReplenOrdersRepository replenOrdersRepository
            , IEnumManager enumManager, IItemDefinitionsRepository itemDefinitionsRepository
            , IStoredProcedureManager storedProcedureManager
            , NeutronVariables neutronVariables, NeutronLicense neutronLicense
            , IAreaRepository areaRepository
            , ILocationsRepository locationsRepository
            , IInventoryRepository inventoryRepository)
        {
            _jsonData = jsonData ?? throw new ArgumentNullException(nameof(jsonData));
            _akaRepository = akaRepository ?? throw new ArgumentNullException(nameof(akaRepository));
            _securityProcessor = securityProcessor ?? throw new ArgumentNullException(nameof(securityProcessor));
            _lacProcessor = lacProcessor ?? throw new ArgumentNullException(nameof(lacProcessor));
            _workstationRepository = workstationRepository ?? throw new ArgumentNullException(nameof(workstationRepository));
            _imageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
            _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
            _replenOrdersRepository = replenOrdersRepository ?? throw new ArgumentNullException(nameof(replenOrdersRepository));
            _enumManager = enumManager ?? throw new ArgumentNullException(nameof(enumManager));
            _itemDefinitionsRepository = itemDefinitionsRepository ?? throw new ArgumentNullException(nameof(itemDefinitionsRepository));
            _storedProcedureManager = storedProcedureManager ?? throw new ArgumentNullException(nameof(storedProcedureManager));
            _neutronVariables = neutronVariables ?? throw new ArgumentNullException(nameof(neutronVariables));
            _neutronLicense = neutronLicense ?? throw new ArgumentNullException(nameof(neutronLicense));
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _locationsRepository = locationsRepository ?? throw new ArgumentNullException(nameof(locationsRepository));
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
            // _tcpIptiCommandCenter = null;

            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            KeyPreview = true;
            _lacProcessor.UseLacProcessor = _neutronVariables.UseLAC;

            _logger = NeutronCore.Global.Logger.SetupLogger("Main");

            Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
                , "Inventory File", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            Mediator.GetInstance().InventoryFileCreatedError += (s, e) => MessageBox.Show(e.Text, "Inventory File Error"
                , MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            Mediator.GetInstance().LoaderError += (s, e) => EmailLoaderError(e.Message);
            Mediator.GetInstance().GeneralError += (s, e) => LogGeneralError(e.Message);
            Mediator.GetInstance().DisplayMessage += (s, e) => DisplayMessage(e.Message);
            Mediator.GetInstance().SendEmailMessage += (s, e) => EmailLoaderError(e.Message);
            Mediator.GetInstance().IsClientConnected += FrmMain_IsClientConnected;

            // _sendEmail.Message(this, e.Message);

            _ = Initialize();
        }

        private async Task Initialize()
        {
            //TODO Remove this or change to false for Production
            //GlobalVar.Testing = true;
            //Log on to Neutron
            await LogOn();
            //Init();
            var result = Init();
            _logger.LogDetailAsync($"After Task.Run INIT result: {result} ").SafeFireAndForget();
            if (result == false)
            {
                await CloseApp();
            }
        }

        private void FrmMain_IsClientConnected(object sender, IsClientConnectedEventArgs e)
        {
            if (_isClientConnected.Equals(e.IsClientConnected)) return;
            _isClientConnected = e.IsClientConnected;
            UpdateClientConnected(_isClientConnected);
        }

        private void UpdateClientConnected(bool state)
        {
            _logger.Log("Client connection state updated: " + state);
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(UpdateClientConnected), state);
                return;
            }

            CheckBoxClientConnected.Checked = state;
        }


        private bool Init()
        {
            _logger.LogDetailAsync("Init Started").SafeFireAndForget();
            var result = InitForm();
            _logger.LogDetailAsync($"Init Result: {result}").SafeFireAndForget();


            if (result == false)
            {
                MessageBox.Show("Neutron has failed to load properly.  Close Neutron and fix error before restarting.", "Main Form Error", MessageBoxButtons.OK);
                return false;
            }

            if (_workstationView.WorkstationId == _neutronVariables.LoaderStation)
            {
                if (!_neutronVariables.UseAutoCompress) return true;
                _compressService = new CompressService(_jsonData, _workstationView, _neutronVariables, _ordersRepository, _replenOrdersRepository);
                _compressService.StartCompressService();
                _logger.LogDetailAsync("Compress Service Started").SafeFireAndForget();
            }
            return true;
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
        #region EventHandlers
        private void LogGeneralError(string message)
        {
            _logger.LogDetailAsync($"General Error: {message}").SafeFireAndForget();
            MessageBox.Show($"Alert: {message}", "Error Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            {
                // _sendEmail.Message(message, _logger.LastLogLines());
            }
        }

        private void DisplayMessage(string message)
        {
            //_logger.LogDetailAsync($"Display Message: {message}").SafeFireAndForget();
            MessageBox.Show($"{message}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            {
                // _sendEmail.Message(message, _logger.LastLogLines());
            }
        }

        private void EmailLoaderError(string message)
        {
            // split the message by CR LF
            var messageLines = message.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (messageLines[0] == _lastEmailMessage)
            {
                _logger.LogDetailAsync($"Loader Error Email Message: {messageLines[0]} = {_lastEmailMessage}");
                _lastEmailMessageCounter += 1;
                if (_lastEmailMessageCounter < 60) return;
                _lastEmailMessage = string.Empty;
                _lastEmailMessageCounter = 0;
                return;
            }

            _lastEmailMessage = messageLines[0];
            _lastEmailMessageCounter = 0;

            _logger.LogDetailAsync($"Loader Error Email: {message}").SafeFireAndForget();
            if (_neutronVariables.EnableEmailNotification)
            {
                var subject = "Loader Message";
                var sb = new StringBuilder(message);
                _sendEmail.Message(subject, sb);
            }
        }
        #endregion
        private bool InitForm()
        {
            _logger.LogDetailAsync("InitForm Started").SafeFireAndForget();
            var result = true;

            try
            {
                ButtonPark.Visible = _neutronLicense.CompanyCode == "TOP";
                if (LoaderSettings.Init())
                {
                    var workstationId = _neutronVariables.WorkstationId;
                    if (workstationId == 0) workstationId = 1;
                    if (workstationId > 0)
                    {
                        _workstationView = _workstationRepository.GetStationView(workstationId);
                        if (_workstationView != null)
                        {
                            SetupEmail();

                            _historyManager = DI.Create<HistoryManager>(_workstationView);
                            GlobalVar.HistoryManager = _historyManager;  

                            _logger.LogDetailAsync($"Startup: CompanyCode: {_neutronLicense.CompanyCode}").SafeFireAndForget();

                            _logger.LogDetailAsync($"Loader Station Test:{_neutronVariables.LoaderStation}=={_workstationView.WorkstationId}").SafeFireAndForget();
                            // If this station is responsible for the loader, start the loader
                            if (_neutronVariables.LoaderStation == _workstationView.WorkstationId)
                            {
                                _logger.LogDetailAsync("Workstation is a Loader Station").SafeFireAndForget();
                                _startStopLoaderManager = DI.Create<StartStopLoaderManager>(_neutronVariables, _neutronLicense, _workstationView);
                                StartLoader();

                                _startStopUploadManager = DI.Create<StartStopUploadManager>(_neutronVariables, _neutronLicense, _workstationView);
                                StartUpload();
                            }
                            _logger.LogDetailAsync("SetupSlotFactory - Before").SafeFireAndForget();
                            SetupSlotFactory();
                            _logger.LogDetailAsync("SetupSlotFactory - After").SafeFireAndForget();
                            // if the workstation is a supervisor, return the workstationView
                            // otherwise, set up the hardware devices
                            // if (_workstationView.StationTypeId != (int)StationType.Supervisor)
                           
                           //var areas = new List<int> { 1, 2, 3, 4 };

                           // var areas = _repoHardwareDevices.All().Select(r => r.WorkstationId).Distinct().ToList();
                            
                            _workstationView.BatchTable = null;
                            
                            //if (areas.Contains(_workstationView.AreaId))
                            //{
                              //  _logger.LogDetailAsync("Workstation is NOT a Supervisor Station. Setup Hardware - Before").SafeFireAndForget();

                                SetupHardwareDevices();

                               // _logger.LogDetailAsync("Workstation is NOT a Supervisor Station. Setup Hardware - After").SafeFireAndForget();
                           // }

                            // Removes old log files based on days to keep in Options/NeutronVariables
                            _logger.LogDetailAsync($"Start Log File Maintenance with Fire And Forget").SafeFireAndForget();
                            Task.Run(MaintainLogFiles).SafeFireAndForget();

                        }
                        else  // _workstationView is null
                        {
                            MessageBox.Show("Workstation has not been configured.   Neutron Exiting.",
                                caption: "Bad Configuration", buttons: MessageBoxButtons.OK);
                            result = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Workstation has not been configured.   Neutron Exiting.",
                            caption: "Bad Configuration", buttons: MessageBoxButtons.OK);
                        result = false;
                    }
                }
                else
                {
                    MessageBox.Show("Main Form: LoaderSettings Initialization Error.");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Main Form Initialization Error.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                result = false;
            }

            _logger.LogDetailAsync("InitForm Complete").SafeFireAndForget();

            return result;
        }

        private void MaintainLogFiles()
        {
            var logger = NeutronCore.Global.Logger.SetupLogger("LogFileMaintenance");
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            if (string.IsNullOrEmpty(logFileDir)) return;

            if (_neutronVariables.LogFilesDaysToKeep == uint.MinValue)
            {
                // save new value into _neutronVariables
                _neutronVariables.LogFilesDaysToKeep = 30;
                // save the Json file
                _jsonData.SaveFile(_neutronVariables);
            }

            var daysToKeep = _neutronVariables.LogFilesDaysToKeep;
            var logFileManager = new LogFileManager(logFileDir, daysToKeep, logger);
            logFileManager.StartLogFileManagementService();

        }

        private void SetupHardwareDevices()
        {
            _logger.LogDetailAsync($"Workstation Name: {_workstationView.Name}").SafeFireAndForget();
            // get all the hardware devices on this workstation; carousel, lights scale, etc
            try
            {
                //get all the hardware devices on this workstation; carousel, lights scale, etc
                var hardwareDevices = _repoHardwareDevices.All().Where(r => r.WorkstationId == _workstationView.WorkstationId).ToList();
                if (hardwareDevices.Any())
                {
                    _logger.LogDetailAsync($"Workstation Name: {_workstationView.Name} Number of Devices: {hardwareDevices.Count}").SafeFireAndForget();
                }
                else
                {
                    _logger.LogDetailAsync("No hardware devices found on this workstation").SafeFireAndForget();
                    return;
                }

                // loop through the hardware devices
                // get the communication type
                // get the tcp or serial configuration
                // create the hardware device view object
                foreach (var device in hardwareDevices)
                {
                    _logger.LogDetailAsync($"Loading Hardware Device: {device.Name}").SafeFireAndForget();

                    // get the device type
                    switch (device.DeviceTypeId)
                    {
                        // DeviceType = 1 or Shuttle
                        case (int)DeviceTypeEnum.Shuttle:
                            {
                                _logger.LogDetailAsync($"This is a Shuttle Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 2 or Carousel
                        case (int)DeviceTypeEnum.Carousel:
                            {
                                _logger.LogDetailAsync(@"This is a Carousel Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 3 or Rack
                        case (int)DeviceTypeEnum.Rack:   //Rack
                            {
                                _logger.LogDetailAsync(@"This is a Rack Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }
                        // DeviceType = 4 or IPTI BatchTable
                        case (int)DeviceTypeEnum.IptiDisplays:   //IPTI 
                            {
                                _logger.LogDetailAsync(@"This is a IPTI BatchTable Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                _workstationView.BatchTable = device;
                                continue;
                            }

                        // DeviceType = 5 or Not Used
                        case 5:   //Not Used
                            {
                                break;
                            }

                        // DeviceType = 6 or Remstar Displays
                        case (int)DeviceTypeEnum.RemstarDisplays:   //Remstar BPI/SHI
                            {
                                _logger.LogDetailAsync(@"This is a Remstar Display Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 7 or Blastzone
                        case (int)DeviceTypeEnum.Blastzone: //Blastzone
                            {
                                _logger.LogDetailAsync(@"This is a Blastzone Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                _workstationView.Blastzones.Add(device);
                                continue;
                            }
                        // DeviceType = 8 or Hanel12D
                        case (int)DeviceTypeEnum.Hanel12D:
                            {
                                // Represents the total number of Hanel 12D devices in the hardware devices list.
                                var totalHanelUnits = hardwareDevices.Count(r => r.DeviceTypeId == (int)DeviceTypeEnum.Hanel12D);

                                _workstationView.Hanels.Add(device);

                                _workstationView.HardwareDevices.Add(device);

                                if (_workstationView.Hanels.Count == totalHanelUnits)
                                {
                                    _logger.LogDetailAsync($"This is a Hanel 12D Station with {totalHanelUnits} Towers.").SafeFireAndForget();
                                    // After I have all the Hanel Units
                                    // Set up the Hanel Controller
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
                                    {
                                        _logger.LogDetailAsync("FrmMain building Mp12D Controller.").SafeFireAndForget();
                                        GlobalVar.Hanel = new Mp12D(this, _workstationView);
                                        GlobalVar.Hanel.InitStatus();
                                        var result = GlobalVar.Hanel != null;
                                    }
                                }
                                continue;
                            }
                        // DeviceType = 9 or Hanel12N
                        case (int)DeviceTypeEnum.Hanel12N:
                            {
                                _logger.LogDetailAsync($"This is a Hanel 12N Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                if (device.Enabled)
                                {
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
                                    {
                                        _logger.LogDetailAsync("MP12N Controller.").SafeFireAndForget();
                                        GlobalVar.Hanel = new Mp12N(this, _workstationView);
                                        GlobalVar.Hanel.InitStatus();
                                        var result = GlobalVar.Hanel != null;
                                    }

                                }
                                break;
                            }

                        // DeviceType = 10 or ProLite
                        case (int)DeviceTypeEnum.ProLite:
                            {
                                _logger.LogDetailAsync($"This is a ProLite Device").SafeFireAndForget();
                                _workstationView.HardwareDevices.Add(device);
                                //var proLite = new ProLite(device.Id, device.Name, device.DeviceNumber, device.Enabled);
                                // if the GlobalVar.ProLiteManager is null, create a new ProLiteManager
                                if (_workstationView.ProLiteManager == null)
                                {
                                    _workstationView.ProLiteManager = new ProLiteManager(device.SerialConfiguration.PortName
                                        , device.SerialConfiguration.BaudRate
                                        , device.SerialConfiguration.Parity
                                        , device.SerialConfiguration.DataBits
                                        , device.SerialConfiguration.StopBits
                                        , _neutronVariables
                                        , _workstationView);
                                }

                                _logger.LogDetailAsync($"Adding Prolite Device to ProLiteManager").SafeFireAndForget();


                                _workstationView.ProLiteManager.AddProlite(device.Id, device.Name, device.DeviceNumber, device.Enabled);
                                continue;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}").SafeFireAndForget();
            }

            // set up the hardware on this Pick Station
            // if there is a defined Blastzone then set the global variable _blastzone to true
            //_workstationView.Blastzones = _workstationView.HardwareDevices.Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.Blastzone).ToList();
            // quick reference flag shows if a Blastzone is used
            //if (blastzone != null) _blastzone = true;

            // if there is a defined BatchTable then set the global variable _batchTable to true
            //_workstationView.BatchTable = _workstationView.HardwareDevices.FirstOrDefault(r => r.DeviceTypeId == (int)DeviceTypeEnum.IptiDisplays);

            if (_workstationView.BatchTable != null || _workstationView.Blastzones.Any())
            {
                //_workstationView.BatchTable.DeviceNumber = _neutronVariables.BliController;
                // var tcpConfiguration = _workstationView.BatchTable.TcpConfiguration;

                // if (_tcpIptiController == null)
                // {
                if (_neutronVariables.IptiDisplays)
                    //{
                    //    _iptiConfig = _jsonData.LoadFile<IptiConfig>();
                    //    _logger.LogDetailAsync("IPTI Displays are being used.").SafeFireAndForget();

                    //    var tcpIptiController = new TcpIptiController(_jsonData, _workstationView, _neutronVariables, _workstationView.BatchTable, _workstationView.BatchTable.TcpConfiguration, _iptiConfig);

                    //   // GlobalVar.Displays = _tcpIptiController;
                    //    if (tcpIptiController == null)
                    //    {
                    //        _logger.LogDetailAsync("Batch Pick Displays were unable to initialize.").SafeFireAndForget();
                    //        Mediator.GetInstance().OnDisplayMessage(this, $"Batch Pick Displays were unable to initialize.");
                    //    }
                    //    else
                    //    {
                    _iptiDisplayFunctions = new IptiDisplayFunctions(_jsonData, _neutronVariables, _workstationView);
                // }
                // }
                // }
            }

            if (_workstationView.ProLiteManager != null)
            {
                // Represents a collection of ProLite hardware devices associated with the current workstation view.
                _workstationView.Prolites = _workstationView.HardwareDevices.Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.ProLite && r.WorkstationId == _workstationView.WorkstationId).ToList();


                // check to see if any _workstationView.Prolites are enabled
                var prolitesEnabled = _workstationView.Prolites.FirstOrDefault(p => p.Enabled == true);
                if (prolitesEnabled != null)
                {
                    _workstationView.ProLiteManager.StartProcessingCommands();
                }

            }
            
            // quick reference flag shows if Prolites are used
            //_prolite = prolites.Any();

            // Represents a collection of Hanel hardware devices associated with the current workstation view.
            _workstationView.Hanels = _workstationView.HardwareDevices.Where(r => r.DeviceTypeId == (int)DeviceTypeEnum.Hanel12D).ToList();

            _logger.LogDetailAsync("Hardware Loading Complete").SafeFireAndForget();
        }
        private void SetupEmail()
        {
            if (_neutronVariables.EnableEmailNotification)
            {
                try
                {
                    var emailServerSettings = _jsonData.LoadFile<EmailSettings>();
                    var emailListing = _jsonData.LoadFile<List<EmailAddressData>>();
                    var emailProcessor = new EmailProcessor(emailServerSettings);
                    _sendEmail = new SendEmail(emailProcessor, emailListing);
                }
                catch (Exception ex)
                {
                    LogGeneralError($"Unable to setup Email Notification. {Environment.NewLine}{ex.Message}");
                }
            }
        }
        private void StartLoader()
        {
            _logger.LogDetailAsync($"Start Loader - Before. Start Loader - {_neutronVariables.RunLoaderOnStartup}").SafeFireAndForget();
            try
            {
                if (_neutronVariables.RunLoaderOnStartup)
                {
                    Mediator.GetInstance().OnStartStopLoader(this, "Start");
                    GlobalVar.LoaderRunning = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                    $"The Loader has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
                MessageBox.Show(
                    $"The Loader has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
        private void StartUpload()
        {
            _logger.LogDetailAsync($"Start Upload - Before. Start Upload - {_neutronVariables.RunUploadOnStartup}").SafeFireAndForget();
            try
            {
                if (_neutronVariables.RunUploadOnStartup)
                {
                    Mediator.GetInstance().OnStartStopUpload(this, "Start");
                    GlobalVar.UploadRunning = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                    $"The Upload has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
                MessageBox.Show(
                    $"The Upload has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        #region UnUsed

        //private bool CreateLog(string name, int stationNumber)
        //{
        //    bool result;
        //    try
        //    {
        //        // _logger = new DynamicLogger(_logFileDir, folderName, logActivity);
        //        _logger.LogFileDir = LoaderSettings.GetLogFileDirectory();
        //        _logger.FolderName = $"{name}_{stationNumber.ToString()}";
        //        _logger.LogActivity = LoaderSettings.EnableLogging;
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Neutron was unable to create the Main Form Log.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //        result = false;
        //    }

        //    return result;
        //}
        //private bool SetupDisplay()
        //{


        //    if (_workstationView.StationTypeId == (int)StationType.Carousel ||
        //        _workstationView.StationTypeId == (int)StationType.Vertical ||
        //        _workstationView.StationTypeId == (int)StationType.Blastzone)
        //    {
        //        bool result;
        //        try
        //        {
        //            if (_neutronVariables.DisplaysEnabled)
        //            {
        //                if (GlobalVar.Displays == null)
        //                {
        //                    if (_neutronVariables.IptiDisplays)
        //                    {
        //                        _logger.LogDetailAsync("IPTI Displays are being used.").SafeFireAndForget();
        //                        // ReSharper disable once UseObjectOrCollectionInitializer

        //                        //GlobalVar.Displays =
        //                        //    new TCP_IptiController(_jsonData, _workstationView, _neutronVariables, null);

        //                        //GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
        //                        result = GlobalVar.Displays != null;
        //                    }
        //                    else
        //                    {

        //                        _logger.LogDetailAsync("Remstar Displays are being used.").SafeFireAndForget();
        //                        GlobalVar.Displays = new DisplayController(_jsonData, _workstationView, _neutronVariables, _neutronLicense);
        //                        result = GlobalVar.Displays != null;
        //                        if (!GlobalVar.Displays.Ready)
        //                        {
        //                            MessageBox.Show($"Error creating Display Controller.");
        //                            _logger.LogDetailAsync("Error creating Display Controller.").SafeFireAndForget();
        //                        }

        //                        //initialize the controller
        //                        var counter = 1;
        //                        while (GlobalVar.Displays.GetInitStatus() != 0)
        //                        {
        //                            var seconds = 250 * counter / 1000;

        //                            _logger.LogDetailAsync(
        //                                   $"Unable to initialize display controller for {seconds} seconds.").SafeFireAndForget();
        //                            if (counter >= 20)
        //                            {
        //                                MessageBox.Show(
        //                                    $"Unable to initialize display controller after {seconds} seconds.");


        //                                _logger.LogDetailAsync(
        //                                       $"Unable to initialize display controller after {seconds} seconds.").SafeFireAndForget();
        //                                break;
        //                            }

        //                            Thread.Sleep(250);
        //                            counter += 1;
        //                        }


        //                        _logger.LogDetailAsync(
        //                               $"Display Controller Initialized. Status Code: {GlobalVar.Displays.GetInitStatus()}").SafeFireAndForget();

        //                    }
        //                }
        //                else
        //                {
        //                    //Displays already exist
        //                    result = true;
        //                }
        //            }
        //            else
        //            {
        //                // no displays enabled, return true
        //                result = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(
        //                $"Error setting up display controller.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //            result = false;
        //        }

        //        return result;
        //    }

        //    return true;
        //}
        //private bool SetupShuttle()
        //{
        //    var result = false;
        //    if (_workstationView.StationTypeId == (int)StationType.Carousel ||
        //        _workstationView.StationTypeId == (int)StationType.Vertical)
        //    {
        //        try
        //        {
        //            if (_neutronVariables.ShuttleEnabled)
        //            {
        //                _logger.LogDetailAsync("Shuttle Enabled - Setup.").SafeFireAndForget();
        //                if (_workstationView.HardwareDevices.Count > 0)
        //                {
        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.C3000() && GlobalVar.Shuttle == null)
        //                    {
        //                        _logger.LogDetailAsync("C3000 Controller.").SafeFireAndForget();
        //                        GlobalVar.Shuttle = new C3000(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.C2000() && GlobalVar.Shuttle == null)
        //                    {
        //                        _logger.LogDetailAsync("C2000 Controller.").SafeFireAndForget();
        //                        GlobalVar.Shuttle = new C2000(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.RCC2() && GlobalVar.Shuttle == null)
        //                    {
        //                        _logger.LogDetailAsync("RCC2 Controller.").SafeFireAndForget();
        //                        GlobalVar.Shuttle = new RCC2(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
        //                    {
        //                        _logger.LogDetailAsync("Mp12D Controller.").SafeFireAndForget();
        //                        GlobalVar.Hanel = new Mp12D(this, _workstationView);
        //                        GlobalVar.Hanel.InitStatus();
        //                        result = GlobalVar.Hanel != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
        //                    {
        //                        _logger.LogDetailAsync("MP12N Controller.").SafeFireAndForget();
        //                        GlobalVar.Hanel = new Mp12N(this, _workstationView);
        //                        GlobalVar.Hanel.InitStatus();
        //                        result = GlobalVar.Hanel != null;
        //                    }
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Main Form Init: Hardware device count error.");
        //                }
        //            }
        //            else
        //            {
        //                // no shuttle enabled, return true
        //                result = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(
        //                $"Error setting up device controller.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //            result = false;
        //        }
        //    }
        //    else
        //    {
        //        // not a carousel or vertical, return true
        //        result = true;
        //    }

        //    return result;
        //}
        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }
        public void UpdateInitStatus(bool success, int percentage)
        {
            MessageBox.Show($"Success: {success}  Percent: {percentage}%");
        }

        #endregion

        #region Slot Factory
        private void SetupSlotFactory()
        {
            try
            {
                switch (_neutronVariables.SlotNameType)
                {
                    case "Default":
                        GlobalVar.SlotNameFactory = DefaultSlotNameFactory.GetInstance();
                        break;
                    case "T101-01-01":
                        GlobalVar.SlotNameFactory = Type1SlotNameFactory.GetInstance();
                        break;
                    case "V101":
                        GlobalVar.SlotNameFactory = Type2SlotNameFactory.GetInstance();
                        break;
                    case "01--01--01--01":
                        GlobalVar.SlotNameFactory = Type3SlotNameFactory.GetInstance();
                        break;
                    case "0101010101":
                        GlobalVar.SlotNameFactory = Type4SlotNameFactory.GetInstance();
                        break;
                    default:
                        GlobalVar.SlotNameFactory = DefaultSlotNameFactory.GetInstance();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Slot Name Factory has failed to initialize.  {Environment.NewLine}" +
                                $"{ex.Message}");
            }
        }

        #endregion

        #region Login/Logout Functions
        public void SetMtLogOffText()
        {
            MtLogOff.Text = _resourceManager.GetString("LogOn");
            GlobalVar.User = null;
            _currentUser = null;
            mlUserInfo.Text = "";
            _securityProcessor.ReprocessSecuritySet("");
        }
        private async void MtLogOff_Click(object sender, EventArgs e)
        {
            //LogOnOff();
            if (MtLogOff.Text == _resourceManager.GetString("LogOff"))
            {
                SetMtLogOffText();
            }

            else if (MtLogOff.Text == _resourceManager.GetString("LogOn"))
            {
                await LogOn();
            }
        }
        private void LogOff()
        {
            GlobalVar.User = null;
            _currentUser = null;
            mlUserInfo.Text = "";
            _securityProcessor.ReprocessSecuritySet("");
            _lacProcessor.ReprocessLacSet(0);
        }
        private async Task LogOn()
        {
            try
            {
                MtLogOff.Text = _resourceManager.GetString("LogOff");
                MtLogOff.Refresh();

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
                        else
                        {
                            await CloseApp();
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


            if (_currentUser != null)
            {
                GlobalVar.User = _currentUser;
                if (_currentUser.Pin == "2277")
                {
                    CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                }
                else
                {
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
                }

                _cultureInfo = Thread.CurrentThread.CurrentCulture;
                SetCulture(_cultureInfo.Name);
                _securityProcessor.ReprocessSecuritySet(GlobalVar.User.Pin);
                _lacProcessor.ReprocessLacSet(GlobalVar.User.Id);
            }
        }
        #endregion

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void MtItemDefinitions_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageItems])
            {
                Hide();
                using (MetroForm frm = new FrmItemDefinitions(_workstationRepository, _jsonData, _workstationView
                           , _akaRepository, _imageManager, _areaRepository, _historyManager))
                {
                    frm.ShowDialog();
                    Show();
                }
            }
        }
        private void MtLocations_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageInventory]) return;
            var main = this;
            using (var frm = DI.Create<FrmLocations>(_workstationView, _neutronVariables, _iptiDisplayFunctions))
            {
                main.Hide();
                frm.ShowDialog();
                main.Show();
            }



            // if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageLocations]) return;
            //  Hide();
            //using (MetroForm frm = new FrmLocations(_jsonData, _workstationRepository, _workstationView, _neutronVariables, _lacProcessor, _historyManager))
            //{
            //    frm.ShowDialog();
            //    Show();
            //}
        }
        private void MtInventory_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageInventory]) return;
            var main = this;
            using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables, _iptiDisplayFunctions))
            {
                main.Hide();
                frm.ShowDialog();
                main.Show();
            }
        }
        private void MtHotAction_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions]) return;
            _logger.LogDetailAsync("FrmMain HotAction button Pressed").SafeFireAndForget();
            Hide();
            using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                       , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                       , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _iptiDisplayFunctions, _inventoryRepository))
            {
                frm.ShowDialog();
                Show();
            }
            _logger.LogDetailAsync("FrmMain HotAction Exit").SafeFireAndForget();
        }
        private void MtSystem_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageSystem]) return;
            Hide();
            using (var frm = DI.Create<FrmSystem>(_neutronVariables, _neutronLicense, false))
            {
                frm.ShowDialog();
                Show();
            }
        }
        //private void MtPick_Click(object sender, EventArgs e)
        //{
        //    if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.PickItemsandOrders]) return;
        //    Hide();
        //    var counter = 0;
        //    while (true)
        //    {
        //        if (_workstationView != null) break;
        //        Thread.Sleep(100);
        //        counter++;
        //        if (counter > 20)
        //        {
        //            MessageBox.Show($"Unable to load workstation data.");
        //            return;
        //        }
        //    }

        //    using (var frm = DI.Create<FrmPick>(
        //               _neutronVariables
        //               , _neutronLicense
        //               , _workstationView
        //               , _historyManager
        //               , _iptiDisplayFunctions))
        //    {
        //        frm.ShowDialog();

        //        if (_neutronVariables.AutoLogOff)
        //        {
        //            SetMtLogOffText();
        //        }

        //        Show();
        //    }
        //}

        private void MtPick_Click(object sender, EventArgs e)
        {
            if (!HasSecurityAccess())
            {
                return;
            }
            Hide();
            if (!WaitForWorkstationData())
            {
                MessageBox.Show($"Unable to load workstation data.");
                return;
            }
            ShowPickForm();
            Show();
        }
        private bool HasSecurityAccess()
        {
            return _securityProcessor.SecurityProfile[(int)NeutronSecurity.PickItemsandOrders];
        }
        private bool WaitForWorkstationData()
        {
            var counter = 0;
            while (_workstationView == null && counter <= 20)
            {
                Thread.Sleep(100);
                counter++;
            }
            return _workstationView != null;
        }
        private void ShowPickForm()
        {
            using (var frm = DI.Create<FrmPick>(
                       _neutronVariables,
                       _neutronLicense,
                       _workstationView,
                       _historyManager,
                       _iptiDisplayFunctions))
            {
                frm.ShowDialog();
                if (_neutronVariables.AutoLogOff)
                {
                    SetMtLogOffText();
                }
            }
        }

        private void MtUtilities_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUtilities]) return;
            Hide();
            using (var frm = DI.CreateUtilitiesForm(_neutronVariables, _neutronLicense, _workstationView, _iptiDisplayFunctions))
            {
                frm.ShowDialog();
                Show();
            }
            //using (MetroForm frm = new FrmUtilities(_jsonData, _neutronVariables, _neutronLicense, _sendEmail))
            //{
            //    frm.ShowDialog();
            //    Show();
            //}
        }
        private void MtHistory_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ViewHistory]) return;
            Hide();
            using (MetroForm frm = new FrmHistory(_akaRepository, _historyManager, _workstationView))
            {
                frm.ShowDialog();
                Show();
            }
        }
        private void MtProductivity_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUsers]) return;
            Hide();
            using (MetroForm frm = new FrmProductivity(_jsonData, _neutronVariables))
            {
                frm.ShowDialog();
                Show();
            }
        }
        private void MtStore_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.StoreItemsandOrders]) return;
            Hide();
            using (var frm = DI.Create<FrmReplen>(
                       _neutronVariables
                       , _neutronLicense
                       , _workstationView
                       , _historyManager
                       , _iptiDisplayFunctions))
            {
                frm.ShowDialog();
                Show();
            }
        }
        private void MtUsers_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUsers]) return;
            Hide();
            using (Form frm = new FrmSecurity(_neutronVariables, _historyManager))
            {
                frm.ShowDialog();
                Show();
            }
        }
        private void MtLac_Click(object sender, EventArgs e)
        {
            var counter = 0;
            while (true)
            {
                if (_workstationView != null) break;
                Thread.Sleep(100);
                counter++;
                if (counter > 50)
                {
                    MessageBox.Show($"Unable to load workstation data.");
                    return;
                }
            }

            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageLac]) return;
            Hide();
            using (Form frm = new FrmLAC(_workstationRepository, _neutronVariables))
            {
                frm.ShowDialog();
                Show();
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

            if (GlobalVar.Hanel != null)
            {
                GlobalVar.Hanel.CloseController();
                if (GlobalVar.Hanel != null)
                {
                    GlobalVar.Hanel = null;
                }
            }

            //if (GlobalVar.Displays != null)
            //{
            //    GlobalVar.Displays.CloseController();
            //    if (GlobalVar.Displays != null)
            //    {
            //        GlobalVar.Displays = null;
            //    }
            //}
        }
        private void ButtonPark_Click(object sender, EventArgs e)
        {
            if (GlobalVar.Shuttle != null)
            {
                GlobalVar.Shuttle.Park();
            }
            if (GlobalVar.Hanel != null)
            {
                GlobalVar.Hanel.Park();
            }
        }
        private async void ButtonClose_Click(object sender, EventArgs e)
        {
            await CloseApp();
        }
        private async Task CloseApp()
        {
            _monitorTransmitter = false;

            if (_compressService != null)
            {
                await _compressService.StopCompressService();
            }

            if (_iptiDisplayFunctions != null)
            {
                _iptiDisplayFunctions?.DisposeServer();
            }

            Close();
        }
        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables, _iptiDisplayFunctions))
                {
                    frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F5 || e.KeyCode == Keys.F6)
            {
                using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                           , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                           , _neutronLicense, _workstationView, _historyManager, _locationsRepository, _iptiDisplayFunctions, _inventoryRepository))
                {
                    frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.F8)
            {
                using (var frm = DI.Create<FrmPick>(
                           _neutronVariables
                           , _neutronLicense
                           , _workstationView
                           , _historyManager
                           , _iptiDisplayFunctions))
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
                MtLogOff.Text = _resourceManager.GetString("LogOff");
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
        private void ButtonRemstar_Click(object sender, EventArgs e)
        {
            using (Form frm = new FrmRemstar())
            {
                frm.ShowDialog();
                Show();
            }
        }
    }
}