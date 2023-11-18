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
using System.Threading.Tasks;
using System.Timers;
using AlliedPostOffice;
using AlliedPostOffice.Concrete;
using Neutron.Classes;
using Neutron.Models;
using Neutron.Ninject;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.PrintModels;
using SqlSchemaManager;
using Timer = System.Timers.Timer;
using NeutronData.ProliteManager;
using NeutronData.Repositories;
using NeutronData.Models.Lookups;
using StationType = NeutronCore.Enums.StationType;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Forms.Application;

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
        private readonly IBlastzone _blastzone;
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
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());
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
            InitializeComponent();
            _cultureInfo = Thread.CurrentThread.CurrentCulture;
            SetCulture(_cultureInfo.Name);
            KeyPreview = true;

            _jsonData = jsonData;
            _akaRepository = akaRepository;
            _securityProcessor = securityProcessor;
            _lacProcessor = lacProcessor;
            _workstationRepository = workstationRepository;
            _imageManager = imageManager;
            _ordersRepository = ordersRepository;
            _replenOrdersRepository = replenOrdersRepository;
            _enumManager = enumManager;
            _itemDefinitionsRepository = itemDefinitionsRepository;
            _storedProcedureManager = storedProcedureManager;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _areaRepository = areaRepository;
            _locationsRepository = locationsRepository;
            _lacProcessor.UseLacProcessor = _neutronVariables.UseLAC;
            _logger = NeutronCore.Global.Logger.SetupLogger("Main");


            Mediator.GetInstance().InventoryFileCreated += (s, e) => MessageBox.Show("Inventory File Created."
                , "Inventory File", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            Mediator.GetInstance().InventoryFileCreatedError += (s, e) => MessageBox.Show(e.Text, "Inventory File Error"
                , MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            Mediator.GetInstance().LoaderError += (s, e) => EmailLoaderError(e.Message);
            Mediator.GetInstance().GeneralError += (s, e) => LogGeneralError(e.Message);
            Mediator.GetInstance().DisplayMessage += (s, e) => DisplayMessage(e.Message);
            Mediator.GetInstance().SendEmailMessage += (s, e) => _sendEmail.Message(e.Message, _logger.LastLogLines());

            //Log on to Neutron
            LogOn();

            _ = Init();
        }

        private async Task Init()
        {
            await _logger.LogDetailAsync("Init Started");
            var result = await InitForm();
            await _logger.LogDetailAsync($"Init Result: {result}");
            
            GlobalVar.Testing = true;
            
            if (result == false)
            {
                MessageBox.Show("Neutron has failed to load properly.  Close Neutron and fix error before restarting.", "Main Form Error", MessageBoxButtons.OK);
                return;
            }

            if (_workstationView.WorkstationId == _neutronVariables.LoaderStation)
            {
                if (!_neutronVariables.UseAutoCompress) return;
                _compressService = new CompressService(_jsonData, _workstationView, _neutronVariables, _historyManager, _ordersRepository, _replenOrdersRepository);
                _compressService.StartCompressService();
                await _logger.LogDetailAsync("Compress Service Started");
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
            if (_sendEmail != null && _neutronVariables.EnableEmailNotification)
            {
                _sendEmail.Message(message, _logger.LastLogLines());
            }
        } 
        #endregion
        private async Task<bool> InitForm()
        {
            var result = false;

            _enumManager.SaveActionCodesToDatabase();
            _enumManager.SaveLineStatusToDatabase();

            try
            {
                ButtonPark.Visible = _neutronLicense.CompanyCode == "TOP";
                if (LoaderSettings.Init())
                {
                    var workstationId = _neutronVariables.WorkstationId;
                    if (workstationId == 0) workstationId = 1;
                    if (workstationId > 0)
                    {
                        _workstationView = await _workstationRepository.GetStationView(workstationId);
                        if (_workstationView != null)
                        {
                            SetupEmail();

                            _historyManager = DI.Create<HistoryManager>(_workstationView);

                            await _logger.LogDetailAsync($"Startup: CompanyCode: {_neutronLicense.CompanyCode}");

                            if (_neutronVariables.LoaderStation == _workstationView.WorkstationId)
                            {
                                _startStopLoaderManager = DI.Create<StartStopLoaderManager>(_neutronVariables, _neutronLicense, _workstationView);

                                _startStopUploadManager = DI.Create<StartStopUploadManager>(_neutronVariables, _neutronLicense, _workstationView);
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

                            var setupSlotFactory = SetupSlotFactory();
                            if (setupSlotFactory == true)
                            {
                                result = true;
                            }
                            else
                            {
                                MessageBox.Show("Main Form: Slot Factory Initialization Error.");
                            }
                            // if the workstation is a supervisor, return the workstationView
                            if (_workstationView.StationTypeId != (int)NeutronCore.Enums.StationType.Supervisor)
                            {
                                await SetupHardwareDevices();

                                // GlobalVar.ProliteManager.TurnOn(1,1,1,1);

                                //var setupShuttle = SetupShuttle();
                                //if (setupShuttle == true)
                                //{
                                //    result = true;
                                //}
                                //else
                                //{
                                //    MessageBox.Show("Main Form: Device Initialization Error.");
                                //}

                                //var setupDisplay = SetupDisplay();
                                //if (setupDisplay == true)
                                //{
                                //    result = true;
                                //}
                                //else
                                //{
                                //    MessageBox.Show("Main Form: Display Initialization Error.");
                                //}
                            }


                        }
                        else  // _workstationView is null
                        {
                            MessageBox.Show("Workstation has not been configured.   Neutron Exiting.",
                                caption: "Bad Configuration", buttons: MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Workstation has not been configured.   Neutron Exiting.",
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

        private async Task SetupHardwareDevices()
        {
            // get all the hardware devices on this workstation; carousel, lights scale, etc
            try
            {
                //get all the hardware devices on this workstation; carousel, lights scale, etc
                var hardwareDevices = _repoHardwareDevices.All().Where(r => r.WorkstationId == _workstationView.WorkstationId).ToList();
                await _logger.LogDetailAsync($"Workstation Name: " + _workstationView.Name + " Number of Devices: " + hardwareDevices.Count);

                // loop through the hardware devices
                // get the communication type
                // get the tcp or serial configuration
                // create the hardware device view object
                foreach (var device in hardwareDevices)
                {
                    await _logger.LogDetailAsync($"Loading Hardware Device: {device.Name}");

                    // get the device type
                    switch (device.DeviceTypeId)
                    {
                        // DeviceType = 1 or Shuttle
                        case (int)DeviceTypeEnum.Shuttle:
                            {
                                await _logger.LogDetailAsync($"This is a Shuttle Device");
                                _workstationView.HardwareDevices.Add(device);

                                break;
                            }

                        // DeviceType = 2 or Carousel
                        case (int)DeviceTypeEnum.Carousel:
                            {
                                await _logger.LogDetailAsync(@"This is a Carousel Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 3 or Rack
                        case (int)DeviceTypeEnum.Rack:   //Rack
                            {
                                await _logger.LogDetailAsync(@"This is a Rack Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }
                        // DeviceType = 4 or IPTI
                        case (int)DeviceTypeEnum.IptiDisplays:   //IPTI 
                            {
                                await _logger.LogDetailAsync(@"This is a IPTI Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (GlobalVar.Displays == null)
                                {
                                    if (_neutronVariables.IptiDisplays)
                                    {
                                        await _logger.LogDetailAsync("IPTI Displays are being used.");
                                        // ReSharper disable once UseObjectOrCollectionInitializer
                                        GlobalVar.Displays =
                                            new TCP_IptiController(_jsonData, _workstationView, _neutronVariables, device);
                                        //GlobalVar.Displays.MySerialDataReceived += ProcessDataReceived;
                                        var result = GlobalVar.Displays != null;

                                        if (GlobalVar.Testing)
                                        {
                                            Mediator.GetInstance().OnDisplayMessage(this, $"Display Testing");
                                        
                                        await TestBli();
                                        await Task.Delay(5000);
                                        //await _logger.LogDetailAsync($"IPTI Displays Result: {result}");
                                        await GlobalVar.Displays.TurnOnAllBli();
                                        //await _logger.LogDetailAsync($"IPTI Displays Turned On");
                                        await Task.Delay(5000);
                                        // await _logger.LogDetailAsync($"IPTI Displays Turn Off");
                                        await GlobalVar.Displays.ClearAllBli();
                                        // await _logger.LogDetailAsync($"IPTI Displays Turned Off");
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
                                await _logger.LogDetailAsync(@"This is a Remstar Display Device");
                                _workstationView.HardwareDevices.Add(device);
                                break;
                            }

                        // DeviceType = 7 or Blastzone
                        case (int)DeviceTypeEnum.Blastzone: //Blastzone
                            {
                                await _logger.LogDetailAsync(@"This is a Blastzone Device");
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
                                await _logger.LogDetailAsync($"This is a Hanel 12D Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (device.Enabled)
                                {
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
                                    {
                                        await _logger.LogDetailAsync("Mp12D Controller.");
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
                                await _logger.LogDetailAsync($"This is a Hanel 12N Device");
                                _workstationView.HardwareDevices.Add(device);
                                if (device.Enabled)
                                {
                                    if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
                                    {
                                        await _logger.LogDetailAsync("MP12N Controller.");
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
                                await _logger.LogDetailAsync($"This is a ProLite Device");
                                _workstationView.HardwareDevices.Add(device);
                                // if the GlobalVar.ProliteManager is null, create a new ProliteManager
                                if (_workstationView.ProliteManager == null)
                                {
                                    _workstationView.ProliteManager = new ProliteManager(device, _neutronVariables);
                                }

                                //if (GlobalVar.ProliteManager == null)
                                //{
                                //    GlobalVar.ProliteManager = new ProliteManager(device, _neutronVariables);
                                //}

                                //if (device.DeviceType == null)
                                //{
                                //    device.DeviceType = _repoDeviceTypes.FindBy(d => d.Id == device.DeviceTypeId).FirstOrDefault();
                                //}

                                //if (device.CommunicationType == null)
                                //{
                                //    device.CommunicationType = _repoCommunicationTypes.FindBy(c => c.Id == device.CommunicationTypeId).FirstOrDefault();
                                //}

                                //if (device.CommunicationType != null && device.CommunicationType.Name == "Serial")
                                //{
                                //    await _logger.LogDetailAsync("This is a Serial Device");
                                //    var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                //    await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString());
                                //    device.SerialConfiguration = _repoSerialConfiguration.FindBy(s => s.Id == device.SerialConfigurationId).FirstOrDefault();

                                //}
                                await _logger.LogDetailAsync($"Adding Prolite Device to ProliteManager");
                                _workstationView.ProliteManager.AddProlite(device);

                                _workstationView.ProliteManager.TurnOn(device.DeviceNumber, 3, 2, 99);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}");
            }
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

        private bool SetupShuttle()
        {
            var result = false;
            if (_workstationView.StationTypeId == (int)StationType.Carousel ||
                _workstationView.StationTypeId == (int)StationType.Vertical)
            {
                try
                {
                    if (_neutronVariables.ShuttleEnabled)
                    {
                        Task.Run(() => _logger.LogDetailAsync("Shuttle Enabled - Setup."));
                        if (_workstationView.HardwareDevices.Count > 0)
                        {
                            if (_neutronVariables.DeviceDriver == DeviceDriverName.C3000() && GlobalVar.Shuttle == null)
                            {
                                Task.Run(() => _logger.LogDetailAsync("C3000 Controller."));
                                GlobalVar.Shuttle = new C3000(this, _workstationView);
                                GlobalVar.Shuttle.InitStatus();
                                result = GlobalVar.Shuttle != null;
                            }

                            if (_neutronVariables.DeviceDriver == DeviceDriverName.C2000() && GlobalVar.Shuttle == null)
                            {
                                Task.Run(() => _logger.LogDetailAsync("C2000 Controller."));
                                GlobalVar.Shuttle = new C2000(this, _workstationView);
                                GlobalVar.Shuttle.InitStatus();
                                result = GlobalVar.Shuttle != null;
                            }

                            if (_neutronVariables.DeviceDriver == DeviceDriverName.RCC2() && GlobalVar.Shuttle == null)
                            {
                                Task.Run(() => _logger.LogDetailAsync("RCC2 Controller."));
                                GlobalVar.Shuttle = new RCC2(this, _workstationView);
                                GlobalVar.Shuttle.InitStatus();
                                result = GlobalVar.Shuttle != null;
                            }

                            if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12D() && GlobalVar.Hanel == null)
                            {
                                Task.Run(() => _logger.LogDetailAsync("Mp12D Controller."));
                                GlobalVar.Hanel = new Mp12D(this, _workstationView);
                                GlobalVar.Hanel.InitStatus();
                                result = GlobalVar.Hanel != null;
                            }

                            if (_neutronVariables.DeviceDriver == DeviceDriverName.Mp12N() && GlobalVar.Hanel == null)
                            {
                                Task.Run(() => _logger.LogDetailAsync("MP12N Controller."));
                                GlobalVar.Hanel = new Mp12N(this, _workstationView);
                                GlobalVar.Hanel.InitStatus();
                                result = GlobalVar.Hanel != null;
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
            }
            else
            {
                // not a carousel or vertical, return true
                result = true;
            }

            return result;
        }

        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
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
                case "0101010101":
                    GlobalVar.SlotNameFactory = Type4SlotNameFactory.GetInstance();
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
        // }

        private void LogOff()
        {
            //if (MtLogOff.Text == _resourceManager.GetString("LogOff"))
            //{
            // MtLogOff.Text = _resourceManager.GetString("LogOn");
            GlobalVar.User = null;
            _currentUser = null;
            mlUserInfo.Text = "";
            _securityProcessor.ReprocessSecuritySet("");
            _lacProcessor.ReprocessLacSet(0);
            //}
            //else
            //{
            //    try
            //    {
            //        MtLogOff.Text = _resourceManager.GetString("LogOff");
            //        if (_neutronVariables.PinLoginOnly)
            //        {
            //            using (var frm = new FrmPin())
            //            {
            //                DialogResult result = frm.ShowDialog();
            //                if (result == DialogResult.OK)
            //                {
            //                    _currentUser = frm.CurrentUser;

            //                    mlUserInfo.Text = $"{_resourceManager.GetString("CurrentUser")}{_currentUser.UserInfo}";
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
            //                    _currentUser = frm.CurrentUser;
            //                    mlUserInfo.Text = $"{_resourceManager.GetString("CurrentUser")}{_currentUser.UserInfo}";
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(@"Login Error " + ex.Message);
            //    }
            //}

            //if (_currentUser != null)
            //{
            //    GlobalVar.User = _currentUser;
            //    if (_currentUser.Pin == "2277")
            //    {
            //        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            //        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            //    }
            //    else
            //    {
            //        var cultureInfo = GlobalVar.User.Language.CultureInfo;
            //        if (cultureInfo.Length == 5 && cultureInfo.Contains('-'))
            //        {
            //            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cultureInfo);
            //            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo);

            //        }
            //        else
            //        {
            //            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            //            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            //        }
            //    }

            //    _cultureInfo = Thread.CurrentThread.CurrentCulture;
            //    SetCulture(_cultureInfo.Name);
            //   _securityProcessor.ReprocessSecuritySet(GlobalVar.User.Pin);
            //    _lacProcessor.ReprocessLacSet(GlobalVar.User.Id);
            //}
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

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MtItemDefinitions_Click(object sender, EventArgs e)
        {
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
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageLocations]) return;
            Hide();
            using (MetroForm frm = new FrmLocations(_jsonData, _workstationRepository, _workstationView, _neutronVariables, _lacProcessor, _historyManager))
            {
                frm.ShowDialog();
                Show();
            }
        }
        private void MtInventory_Click(object sender, EventArgs e)
        {
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
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.HotActions]) return;
            Task.Run(() => _logger.LogDetailAsync("FrmMain HotAction button Pressed"));
            Hide();
            using (MetroForm frm = new FrmHotAction(_jsonData, _akaRepository
                       , _lacProcessor, _imageManager, _workstationRepository, _itemDefinitionsRepository, _neutronVariables
                       , _neutronLicense, _workstationView, _historyManager, _locationsRepository))
            {
                frm.ShowDialog();
                Show();
            }
            Task.Run(() => _logger.LogDetailAsync("FrmMain HotAction Exit"));
        }
        private void MtSystem_Click(object sender, EventArgs e)
        {
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
            if (!_securityProcessor.SecurityProfile[(int)NeutronSecurity.ManageUtilities]) return;
            Hide();
            using (var frm = DI.CreateUtilitiesForm(_neutronVariables, _neutronLicense, _sendEmail))
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
                           , _lacProcessor, _imageManager, _workstationRepository, _itemDefinitionsRepository, _neutronVariables
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

        private async void button1_Click(object sender, EventArgs e)
        {
            await TestBli();
        }

        private async Task TestBli()
        {
            await _logger.LogDetailAsync($"button1_Click IPTI Displays ");
            await GlobalVar.Displays.TurnOnAllBli();
            await _logger.LogDetailAsync($"button1_Click IPTI Displays Turned On");
            await Task.Delay(3000);
            await _logger.LogDetailAsync($"button1_Click IPTI Displays Turn Off");
            await GlobalVar.Displays.ClearAllBli();
            await _logger.LogDetailAsync($"button1_Click IPTI Displays Turned Off");
        }
    }
}