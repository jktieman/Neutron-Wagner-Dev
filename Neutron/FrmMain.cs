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
using System.Threading.Tasks;
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
        private WorkstationView _workstationView;
        private readonly IDynamicLogger _logger;
        private readonly IAkaRepository _akaRepository;
        private readonly ILacProcessor _lacProcessor;

        private CompressService _compressService;

        private SendEmail _sendEmail;
        private StartStopLoaderManager _startStopLoaderManager;
        private StartStopUploadManager _startStopUploadManager;
        private HistoryManager _historyManager;
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.DeviceType> _repoDeviceTypes = new GenericRepository<NeutronData.Models.Lookups.DeviceType>(new NeutronDb());
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.SerialConfiguration> _repoSerialConfiguration = new GenericRepository<NeutronData.Models.SerialConfiguration>(new NeutronDb());
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

        public FrmMain(IJsonData jsonData, IAkaRepository akaRepository
            , ISecurityProcessor securityProcessor, ILacProcessor lacProcessor
            , IImageManager imageManager, IWorkstationRepository workstationRepository
            , IOrdersRepository ordersRepository, IReplenOrdersRepository replenOrdersRepository
            , IEnumManager enumManager, IItemDefinitionsRepository itemDefinitionsRepository
            , IStoredProcedureManager storedProcedureManager
            , NeutronVariables neutronVariables, NeutronLicense neutronLicense
            , IAreaRepository areaRepository
            , ILocationsRepository locationsRepository)
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
            Mediator.GetInstance().SendEmailMessage += (s, e) => DisplayMessage(e.Message); // _sendEmail.Message(this, e.Message);

            _ = InitializeAsync();

            ////TODO Remove this or change to false for Production
            //GlobalVar.Testing = true;

            ////Log on to Neutron
            //LogOn();

            ////Init();
            //var result = Init();

            //_ = _logger.LogDetailAsync($"After Task.Run INIT result: {result} ");

            //if (result == false)
            //{
            //    _ = CloseApp();
            //}


        }

        private async Task InitializeAsync()
        {
            //TODO Remove this or change to false for Production
            GlobalVar.Testing = true;
            //Log on to Neutron
            LogOn();
            //Init();
            var result = await InitAsync();
            _ = _logger.LogDetailAsync($"After Task.Run INIT result: {result} ");
            if (result == false)
            {
                _ = CloseApp();
            }
        }

        private async Task<bool> InitAsync()
        {
            await _logger.LogDetailAsync("Init Started");
            var result = InitForm();
            _ = _logger.LogDetailAsync($"Init Result: {result}");


            if (result == false)
            {
                MessageBox.Show("Neutron has failed to load properly.  Close Neutron and fix error before restarting.", "Main Form Error", MessageBoxButtons.OK);
                return false;
            }

            if (_workstationView.WorkstationId == _neutronVariables.LoaderStation)
            {
                if (!_neutronVariables.UseAutoCompress) return true;
                _compressService = new CompressService(_jsonData, _workstationView, _neutronVariables, _historyManager, _ordersRepository, _replenOrdersRepository);
                _compressService.StartCompressService();
                _ = _logger.LogDetailAsync("Compress Service Started");
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
            Task.Run(() => _logger.LogDetailAsync($"General Error: {message}"));
            MessageBox.Show($"Alert: {message}", "Error Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            //{
            //    _sendEmail.Message(message, _logger.LastLogLines());
            //}
        }

        private void DisplayMessage(string message)
        {
            //Task.Run(() => _logger.LogDetailAsync($"Display Message: {message}"));
            MessageBox.Show($"{message}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            //{
            //    _sendEmail.Message(message, _logger.LastLogLines());
            //}
        }

        private void EmailLoaderError(string message)
        {
            Task.Run(() => _logger.LogDetailAsync($"Loader Error: {message}"));
            if (_neutronVariables.EnableEmailNotification)
            {
                _sendEmail.Message(message, _logger.LastLogLines());
            }
        }
        #endregion
        private bool InitForm()
        {
            _ = _logger.LogDetailAsync("InitForm Started");
            var result = true;

            //_enumManager.SaveActionCodesToDatabase();
            //_enumManager.SaveLineStatusToDatabase();

            try
            {
                ButtonPark.Visible = _neutronLicense.CompanyCode == "TOP";
                if (LoaderSettings.Init())
                {
                    var workstationId = _neutronVariables.WorkstationId;
                    if (workstationId == 0) workstationId = 1;
                    if (workstationId > 0)
                    {
                        _workstationView = _ = _workstationRepository.GetStationView(workstationId);
                        if (_workstationView != null)
                        {
                            SetupEmail();

                            _historyManager = DI.Create<HistoryManager>(_workstationView);

                            _ = _logger.LogDetailAsync($"Startup: CompanyCode: {_neutronLicense.CompanyCode}");

                            _ = _logger.LogDetailAsync($"Loader Station Test:{_neutronVariables.LoaderStation}=={_workstationView.WorkstationId}");
                            // If this station is responsible for the loader, start the loader
                            if (_neutronVariables.LoaderStation == _workstationView.WorkstationId)
                            {
                                _ = _logger.LogDetailAsync("Workstation is a Loader Station");
                                _startStopLoaderManager = DI.Create<StartStopLoaderManager>(_neutronVariables, _neutronLicense, _workstationView);
                                StartLoader();

                                _startStopUploadManager = DI.Create<StartStopUploadManager>(_neutronVariables, _neutronLicense, _workstationView);
                                StartUpload();
                            }
                            _ = _logger.LogDetailAsync("SetupSlotFactory - Before");
                            SetupSlotFactory();
                            _ = _logger.LogDetailAsync("SetupSlotFactory - After");
                            // if the workstation is a supervisor, return the workstationView
                            // otherwise, set up the hardware devices
                            if (_workstationView.StationTypeId != (int)StationType.Supervisor)
                            {
                                _ = _logger.LogDetailAsync("Workstation is NOT a Supervisor Station. Setup Hardware - Before");
                                SetupHardwareDevices();
                                _ = _logger.LogDetailAsync("Workstation is NOT a Supervisor Station. Setup Hardware - After");
                            }
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

            _ = _logger.LogDetailAsync("InitForm Complete");

            return result;
        }
        private void SetupHardwareDevices()
        {
            _ = _logger.LogDetailAsync($"Workstation Name: " + _workstationView.Name);
            // get all the hardware devices on this workstation; carousel, lights scale, etc
            try
            {
                //get all the hardware devices on this workstation; carousel, lights scale, etc
                var hardwareDevices = _repoHardwareDevices.All().Where(r => r.WorkstationId == _workstationView.WorkstationId).ToList();
                if (hardwareDevices.Any())
                {
                    _ = _logger.LogDetailAsync($"Workstation Name: " + _workstationView.Name + " Number of Devices: " + hardwareDevices.Count);
                }
                else
                {
                    _ = _logger.LogDetailAsync("No hardware devices found on this workstation");
                    return;
                }



                // loop through the hardware devices
                // get the communication type
                // get the tcp or serial configuration
                // create the hardware device view object
                foreach (var device in hardwareDevices)
                {
                    _ = _logger.LogDetailAsync($"Loading Hardware Device: {device.Name}");

                    // get the device type
                    switch (device.DeviceTypeId)
                    {
                        // DeviceType = 1 or Shuttle
                        case (int)DeviceTypeEnum.Shuttle:
                            {
                                _ = _logger.LogDetailAsync($"This is a Shuttle Device");
                                _workstationView.HardwareDevices.Add(device);

                                break;
                            }

                        // DeviceType = 2 or Carousel
                        case (int)DeviceTypeEnum.Carousel:
                            {
                                _ = _logger.LogDetailAsync(@"This is a Carousel Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 3 or Rack
                        case (int)DeviceTypeEnum.Rack:   //Rack
                            {
                                _ = _logger.LogDetailAsync(@"This is a Rack Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }
                        // DeviceType = 4 or IPTI
                        case (int)DeviceTypeEnum.IptiDisplays:   //IPTI 
                            {
                                _ = _logger.LogDetailAsync(@"This is a IPTI Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (GlobalVar.Displays == null)
                                {
                                    if (_neutronVariables.IptiDisplays)
                                    {
                                        _ = _logger.LogDetailAsync("IPTI Displays are being used.");
                                        // ReSharper disable once UseObjectOrCollectionInitializer
                                        GlobalVar.Displays =
                                            new TCP_IptiController(_jsonData, _workstationView, _neutronVariables, device);
                                        if (GlobalVar.Displays == null)
                                        {
                                            Mediator.GetInstance().OnDisplayMessage(this, $"Batch Pick Displays were unable to initialize.");
                                        }

                                        if (GlobalVar.Testing)
                                        {
                                            //  await TestBli();
                                        }
                                    }
                                }

                                break;
                            }

                        // DeviceType = 5 or Not Used
                        case 5:   //Not Used
                            {
                                break;
                            }

                        // DeviceType = 6 or Remstar Displays
                        case (int)DeviceTypeEnum.RemstarDisplays:   //Remstar BPI/SHI
                            {
                                _ = _logger.LogDetailAsync(@"This is a Remstar Display Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 7 or Blastzone
                        case (int)DeviceTypeEnum.Blastzone: //Blastzone
                            {
                                _ = _logger.LogDetailAsync(@"This is a Blastzone Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (GlobalVar.Displays == null)
                                {
                                    if (_neutronVariables.IptiDisplays)
                                    {
                                        _ = Task.Run(() => _logger.LogDetailAsync("IPTI Displays are being used."));
                                        GlobalVar.Displays =
                                            new TCP_IptiController(_jsonData, _workstationView, _neutronVariables, device);
                                        var result = GlobalVar.Displays != null;
                                    }
                                }

                                break;
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
                                    _ = _logger.LogDetailAsync($"This is a Hanel 12D Station with {totalHanelUnits} Towers.");
                                   
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
                                    {
                                        _ = _logger.LogDetailAsync("FrmMain building Mp12D Controller.");
                                        GlobalVar.Hanel = new Mp12D(this, _workstationView);
                                        GlobalVar.Hanel.InitStatus();
                                        var result = GlobalVar.Hanel != null;
                                    }
                                }
                                break;
                            }
                        // DeviceType = 9 or Hanel12N
                        case (int)DeviceTypeEnum.Hanel12N:
                            {
                                _ = _logger.LogDetailAsync($"This is a Hanel 12N Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (device.Enabled)
                                {
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
                                    {
                                        _ = _logger.LogDetailAsync("MP12N Controller.");
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
                                _ = _logger.LogDetailAsync($"This is a ProLite Device");
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

                                _ = _logger.LogDetailAsync($"Adding Prolite Device to ProLiteManager");


                                _workstationView.ProLiteManager.AddProlite(device.Id, device.Name, device.DeviceNumber, device.Enabled);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync("Hardware Loading Complete");
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
            _ = _logger.LogDetailAsync($"Start Loader - Before. Start Loader - {_neutronVariables.RunLoaderOnStartup}");
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
                _ = _logger.LogDetailAsync(
                    $"The Loader has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                MessageBox.Show(
                    $"The Loader has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
        private void StartUpload()
        {
            _ = _logger.LogDetailAsync($"Start Upload - Before. Start Upload - {_neutronVariables.RunUploadOnStartup}");
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
                _ = _logger.LogDetailAsync(
                    $"The Upload has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                MessageBox.Show(
                    $"The Upload has failed to start on Startup.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        #region UnUsed

        private bool CreateLog(string name, int stationNumber)
        {
            bool result;
            try
            {
                // _logger = new DynamicLogger(_logFileDir, folderName, logActivity);
                _logger.LogFileDir = LoaderSettings.GetLogFileDirectory();
                _logger.FolderName = $"{name}_{stationNumber.ToString()}";
                _logger.LogActivity = LoaderSettings.EnableLogging;
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


            if (_workstationView.StationTypeId == (int)StationType.Carousel ||
                _workstationView.StationTypeId == (int)StationType.Vertical ||
                _workstationView.StationTypeId == (int)StationType.Blastzone)
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
                                Task.Run(() => _logger.LogDetailAsync("IPTI Displays are being used."));
                                // ReSharper disable once UseObjectOrCollectionInitializer
                                GlobalVar.Displays =
                                    new TCP_IptiController(_jsonData, _workstationView, _neutronVariables, null);
                                //GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
                                result = GlobalVar.Displays != null;
                            }
                            else
                            {

                                Task.Run(() => _logger.LogDetailAsync("Remstar Displays are being used."));
                                GlobalVar.Displays = new DisplayController(_jsonData, _workstationView, _neutronVariables, _neutronLicense);
                                result = GlobalVar.Displays != null;
                                if (!GlobalVar.Displays.Ready)
                                {
                                    MessageBox.Show($"Error creating Display Controller.");
                                    Task.Run(() => _logger.LogDetailAsync("Error creating Display Controller."));
                                }

                                //initialize the controller
                                var counter = 1;
                                while (GlobalVar.Displays.GetInitStatus() != 0)
                                {
                                    var seconds = 250 * counter / 1000;
                                    Task.Run(() =>
                                     _ = _logger.LogDetailAsync(
                                            $"Unable to initialize display controller for {seconds} seconds."));
                                    if (counter >= 20)
                                    {
                                        MessageBox.Show(
                                            $"Unable to initialize display controller after {seconds} seconds.");

                                        Task.Run(() =>
                                         _ = _logger.LogDetailAsync(
                                                $"Unable to initialize display controller after {seconds} seconds."));
                                        break;
                                    }

                                    Thread.Sleep(250);
                                    counter += 1;
                                }

                                Task.Run(() =>
                                 _ = _logger.LogDetailAsync(
                                        $"Display Controller Initialized. Status Code: {GlobalVar.Displays.GetInitStatus()}"));

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
                    MessageBox.Show(
                        $"Error setting up display controller.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    result = false;
                }

                return result;
            }

            return true;
        }
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
        //                Task.Run(() => _logger.LogDetailAsync("Shuttle Enabled - Setup."));
        //                if (_workstationView.HardwareDevices.Count > 0)
        //                {
        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.C3000() && GlobalVar.Shuttle == null)
        //                    {
        //                        Task.Run(() => _logger.LogDetailAsync("C3000 Controller."));
        //                        GlobalVar.Shuttle = new C3000(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.C2000() && GlobalVar.Shuttle == null)
        //                    {
        //                        Task.Run(() => _logger.LogDetailAsync("C2000 Controller."));
        //                        GlobalVar.Shuttle = new C2000(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.RCC2() && GlobalVar.Shuttle == null)
        //                    {
        //                        Task.Run(() => _logger.LogDetailAsync("RCC2 Controller."));
        //                        GlobalVar.Shuttle = new RCC2(this, _workstationView);
        //                        GlobalVar.Shuttle.InitStatus();
        //                        result = GlobalVar.Shuttle != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
        //                    {
        //                        Task.Run(() => _logger.LogDetailAsync("Mp12D Controller."));
        //                        GlobalVar.Hanel = new Mp12D(this, _workstationView);
        //                        GlobalVar.Hanel.InitStatus();
        //                        result = GlobalVar.Hanel != null;
        //                    }

        //                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
        //                    {
        //                        Task.Run(() => _logger.LogDetailAsync("MP12N Controller."));
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

        private void MtLogOff_Click(object sender, EventArgs e)
        {
            //LogOnOff();
            if (MtLogOff.Text == _resourceManager.GetString("LogOff"))
            {
                SetMtLogOffText();
            }

            else if (MtLogOff.Text == _resourceManager.GetString("LogOn"))
            {
                LogOn();
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
        private void LogOn()
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
                            _ = CloseApp();
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
            using (var frm = DI.Create<FrmLocations>(_workstationView, _neutronVariables))
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
            using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables))
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
            Task.Run(() => _logger.LogDetailAsync("FrmMain HotAction button Pressed"));
            Hide();
            using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                       , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                       , _neutronLicense, _workstationView, _historyManager, _locationsRepository))
            {
                frm.ShowDialog();
                Show();
            }
            Task.Run(() => _logger.LogDetailAsync("FrmMain HotAction Exit"));
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
        private void MtPick_Click(object sender, EventArgs e)
        {
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.PickItemsandOrders]) return;
            Hide();
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


            using (var frm = DI.Create<FrmPick>(
                       _neutronVariables
                       , _neutronLicense
                       , _workstationView
                       , _historyManager))
            {
                frm.ShowDialog();

                if (_neutronVariables.AutoLogOff)
                {
                    SetMtLogOffText();
                }
                Show();
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
            using (var frm = DI.CreateUtilitiesForm(_neutronVariables, _neutronLicense, _workstationView))
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
                       , _historyManager))
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
            if (_compressService != null)
            {
                await _compressService.StopCompressService();
            }

            Close();
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                using (var frm = DI.Create<FrmInventory>(_workstationView, _neutronVariables))
                {
                    frm.ShowDialog();
                    Show();
                }
            }

            if (e.KeyCode == Keys.F5 || e.KeyCode == Keys.F6)
            {
                using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                           , _lacProcessor, _imageManager, _itemDefinitionsRepository, _neutronVariables
                           , _neutronLicense, _workstationView, _historyManager, _locationsRepository))
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
                           , _historyManager))
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