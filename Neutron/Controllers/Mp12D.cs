using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Threading;
using AlliedLogger;
using Neutron.Enums;
using NeutronData.ModelViews;

using Neutron.Interfaces;

using Hanel_DC.Hanel_DeviceControllers;
using HanelCommands;
using NeutronCore;
using NeutronCore.Enums;
using AsyncAwaitBestPractices;
using Hanel_DC.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using Neutron.Models;
using NeutronData.Models;
using System.Text;
using Hanel_DC.HanelStatics;

namespace Neutron.Controllers
{

    /// <summary>
    /// Represents the controller for the Mp12D device, implementing the <see cref="Neutron.Interfaces.IHanelDriver"/> interface.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the interactions with the Mp12D device, including initialization, status checking, and handling notifications.
    /// </remarks>
    public class Mp12D : IHanelDriver
    {
        public static char CR = Convert.ToChar(13);
        public static char LF = Convert.ToChar(10);
        public static char AST = Convert.ToChar(42);

        private Hanel_DeviceController _hanel;
        
        private SendOrPostCallback _callBackHandlerInit;
        private HanelTellMeWhenTrayArrives _myTrayArrivedNotificationDelegate = MyTrayArrived;
        private Guid _myNotificationHandle;
        private int _notificationTimeOutSeconds = 60;
        private readonly bool _notificationAutoDeregister = true;
        private string cError;
        private readonly int _deviceStationary = -1;
        private readonly int _deviceAlignmentDontCare = 0;

        private IDynamicLogger _logger;
        private int _logLevel;
        private readonly IDialogService _dialogService;

        private readonly WorkstationView _workstationView;
        private readonly NeutronVariables _neutronVariables;
        private Form _currentForm;
        private readonly Object _locker = new Object();
        private int[] _previousTray;
        private bool _testing;

        public Mp12D(Form frm, WorkstationView workstationView, int logLevel, IDialogService dialogService)
        {
            _previousTray = new int[10];
            _workstationView = workstationView ?? throw new ArgumentNullException(nameof(workstationView));
            _logLevel = logLevel;
            _dialogService = dialogService;
            // FrmMain passed in
            _currentForm = frm;
            _testing = false;
            //_logger = NeutronCore.Global.Logger.SetupLogger("Mp12D");
            _logger = NeutronCore.Global.Logger.SetupLogger("HanelLog");

            Init();
        }

        /// <summary>
        /// Testing Constructor adds a logger
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="workstationView"></param>
        /// <param name="logger"></param>
        /// <param name="logLevel"></param>
        /// <param name="dialogService"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Mp12D(Form frm, WorkstationView workstationView, IDynamicLogger logger, int logLevel, IDialogService dialogService)
        {
            _previousTray = new int[10];
            _workstationView = workstationView ?? throw new ArgumentNullException(nameof(workstationView));
            _testing = true;
            _logger = logger;
            _logLevel = logLevel;
            _dialogService = dialogService;
            // FrmMain passed in
            _currentForm = frm;

            Init();
        }
        //private void Init()
        //{
        //    _logger.LogDetailAsync($"Mp12D Constructor - {_currentForm.Name}").SafeFireAndForget();
        //    _callBackHandlerInit = MyInitProgressDelegate;
        //    _hanel = new Hanel_DeviceController(Hanel_DeviceController.Controller_Type_Hanel_Mp12D());
        //    _logger.LogDetailAsync(@"Hanel Device Controller has been created: ").SafeFireAndForget();

        //    Init2();
        //}

        private void Init()
        {
            LogInitializationStart();
            InitializeCallbackHandler();
            CreateHanelDeviceController();
            LogDeviceControllerCreation();
            Init2();
        }
        private void LogInitializationStart()
        {
            if (_logLevel == 2 || _logLevel == 8)
                _logger.LogDetailAsync($"Mp12D Constructor - {_currentForm.Name}").SafeFireAndForget();
        }
        private void InitializeCallbackHandler()
        {
            _callBackHandlerInit = MyInitProgressDelegate;
        }
        private void CreateHanelDeviceController()
        {
            _hanel = _testing ? new Hanel_DeviceController(HanelDcStatics.Controller_Type_Hanel_Mp12D(), _dialogService, _logger, true) : new Hanel_DeviceController(Hanel_DeviceController.Controller_Type_Hanel_Mp12D(), _dialogService);
        }
        private void LogDeviceControllerCreation()
        {
            if (_logLevel == 2 || _logLevel == 8)
                _logger.LogDetailAsync("Hanel Device Controller has been created: ").SafeFireAndForget();
        }


        public Form CurrentForm
        {
            private get => _currentForm;
            set
            {
                _currentForm = value;
                if (_logLevel == 2 || _logLevel == 8)
                    _logger.LogDetailAsync($"Changed Form - {_currentForm}").SafeFireAndForget();
            }
        }
        /// <summary>
        /// Resets the array that stores the previous tray states.
        /// </summary>
        /// <remarks>
        /// The size of the array is determined by the number of Hanel devices in the workstation view plus one.
        /// </remarks>
        public void ResetPreviousTray()
        {
            var deviceCount = _workstationView.Hanels.Count;
            _previousTray = new int[deviceCount + 1];
        }

        private void Init2()
        {
            if (_workstationView != null)
            {
                var firstDevice = _workstationView.Hanels.FirstOrDefault();

                if (firstDevice != null)
                {
                    var serialConfiguration = firstDevice.SerialConfiguration;

                    if (serialConfiguration != null)
                    {
                        var deviceCount = _workstationView.Hanels.Count;
                        // an int array that keeps track of the last tray on each Hanel
                        // _previousTray[0] doesn't hold anything, it's just a filler so the
                        // array element aligns with the Hanel Number
                        //
                        // _previousTray[1] may be Tray 12, 
                        // _previousTray[2] may be Tray 2, 
                        // _previousTray[3] may be Tray 24, 

                        _previousTray = new int[deviceCount + 1];
                        _notificationTimeOutSeconds = serialConfiguration.NotificationTimeout;
                        var simulationMode = firstDevice.SimulationMode;
                        var logLevel = firstDevice.LogLevel;
                        var enabledUnitNumbers = _workstationView.Hanels.Where(r => r.Enabled == true).Select(s => s.DeviceNumber).ToList();

                        _logger.LogDetailAsync($"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()} Device Count: {serialConfiguration.DeviceCount}").SafeFireAndForget();

                        // _hanel is a Hanel_DeviceController
                        var success = _hanel.Init_Controller(serialConfiguration.ControllerId
                            , serialConfiguration.PortNumber
                            , serialConfiguration.BaudRate
                            , serialConfiguration.DataBits
                            , serialConfiguration.Parity.ToString()
                            , serialConfiguration.StopBits
                            , simulationMode
                            , logLevel
                            , enabledUnitNumbers
                            , this
                            , _callBackHandlerInit
                            , ref cError);

                        if (success)
                        {
                            _logger.LogDetailAsync("Initialization Requested").SafeFireAndForget();
                        }
                        else
                        {
                            _logger.LogDetailAsync("Problem requesting initialization. " + cError).SafeFireAndForget();
                        }
                    }
                    else
                    {
                        _logger.LogDetailAsync("SerialConfiguration is null ").SafeFireAndForget();
                    }
                }
                else
                {
                    _logger.LogDetailAsync($"Unknown Serial Configuration.").SafeFireAndForget();
                    MessageBox.Show($"Unknown Serial Configuration.");
                }
            }
            else
            {
                _logger.LogDetailAsync("Workstation is null or empty ").SafeFireAndForget();
            }
        }
        public int InitStatus()
        {
            // Note that the sequence of the following assignments is critical. Success must be first. Others follow in any sequence.
            var success = _hanel.Init_Success;
            var initCode = _hanel.LastStatus_Code;
            var initMsg = _hanel.LastStatus_Message;

            _logger.LogDetailAsync($"InitStatus: Success: {success} initCode: {initCode} initMsg: {initMsg}").SafeFireAndForget();
            if (success)
            {
                // life is good, you can drive the device
                _logger.LogDetailAsync("InitStatus: success is true.").SafeFireAndForget();
                if (initCode == 0)
                {
                    // life is good, no warning messages
                    _logger.LogDetailAsync($"Initialization is complete and was successful initCode is {initCode}").SafeFireAndForget();
                }
                else
                {
                    // You need to report the warning to the operator or to a log that is monitored frequently
                    _logger.LogDetailAsync($"Hanel Controller Warning - Initialization was successful but there is a warning." + Environment.NewLine +
                    "Please provide the following information to your IT support." + Environment.NewLine +
                    "Code is: " + initCode.ToString() + Environment.NewLine +
                    "Message is: " + initMsg).SafeFireAndForget();
                }
            }
            else
            {
                // Darn, cannot drive the device at this time
                if (_hanel.Get_Init_PercentageComplete() == 0)
                {
                    _logger.LogDetailAsync($"Not Initialized.  Code is: {initCode.ToString()}  Message is: {initMsg}").SafeFireAndForget();
                }
                else if (_hanel.Get_Init_PercentageComplete() < 100)
                {
                    _logger.LogDetailAsync($"Initialization is in progress.  Init {_hanel.Get_Init_PercentageComplete().ToString()}% complete...").SafeFireAndForget();
                    _logger.LogDetailAsync($"Code is: {initCode.ToString()}  Message is: {initMsg}").SafeFireAndForget();
                }
                else
                {
                    _logger.LogDetailAsync($"Initialization was unsuccessful.  Code is: {initCode.ToString()}  Message is: {initMsg}").SafeFireAndForget();
                }
            }
            return initCode;
        }
        public static void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }

        /*
     This is a delegate method and it will be called when any init notifications fire. 
     All delegates passed to Init_Controller() must have this same signature.
     */
        public static void MyInitProgressDelegate(object formObject)
        {
            // This method is running in the UI synchronization context.
            // You just need to create a reference to your original object
            var hanelDcInit = (HanelDcInit)formObject;

            // You may now refer to any components of your UI as below.
            // formAlias.LabelNotify.Text = "Init " + formAlias._hanel.Init_PercentageComplete.ToString() + "% complete...";
            //formAlias .ShowMessage("Init " + formAlias._hanel.Get_Init_PercentageComplete().ToString() + "% complete...");
            // var x = 1;
            ShowMessage("Init " + hanelDcInit.PercentComplete.ToString() + "% complete...");
        }

        public static void MyTrayArrived(HanelDeviceNotificationType firedNotification)
        {
            // This method is running in the UI synchronization context but
            // you need to create a reference to your original form in order
            // to reference any of its controls.
            var formAlias = (Mp12D)firedNotification.CallersObject;
            // You may now refer to any components of your UI as below.
            // formAlias.LabelNotify.Text = "Drive Notification Received";
            ShowMessage("Drive Notification Received");
            //Task.Run(() => formAlias.logger.Log($"Shuttle {firedNotification.TargetDevice.ToString()} Notification"));

            //if (firedNotification.Message.ToString().Length > 0)
            //{
            //    // you should report these messages to the operator or write them to a log that is frequently monitored.
            //    Task.Run(() => formAlias.logger.Log($"Notification request for tray {firedNotification.TargetTray.ToString()} on shuttle {firedNotification.TargetDevice.ToString()} returned with a message.  Message: {firedNotification.Message.ToString()}"));
            //}

            //if (firedNotification.Expired)
            //{
            //    Task.Run(() => formAlias.logger.Log($"Notification request for tray {firedNotification.TargetTray.ToString()} on shuttle {firedNotification.TargetDevice.ToString()}  has timed-out."));
            //}

            //else if (firedNotification.MotionStatusUponNotification)
            //{
            //    // Note that the current implementation of Notification_Register() does not support notifications
            //    // for devices in motion, so this particular logic will not execute at this time. 
            //    Task.Run(() => formAlias.logger.Log($"Shuttle {firedNotification.TargetDevice.ToString()} is in motion."));
            //    // If this was a horizontal or vertical carousel, NotifyTarget.Current_Tray would describe the shelf/carrier currently in position as it moves past.
            //}

            //else
            //{
            //    string alignmentStatus = firedNotification.AlignmentStatusUponNotification ? "in alignment." : "out of alignment.";
            //    Task.Run(() => formAlias.logger.Log($"Shuttle {firedNotification.TargetDevice.ToString()} is stationary and tray {firedNotification.TargetTray.ToString()} is {alignmentStatus}"));
            //}
        }

        //public bool GetTrayInWindow(int lift, int accessPoint)
        //{
        //    var response = _hanel.GetTrayInWindow(lift, accessPoint);
        //    return response;
        //}

        public void GetTraysInWindow()
        {
            _hanel.GetTraysInWindow();
        }

        #region New Position Device
        public DeviceResponse PositionDeviceNEW(
    int deviceNumber,
    int trayNumber,
    int facing = 0,
    int depth = 0,
    int quantity = 0,
    string display = "")
        {
            var deviceResponse = DeviceResponse.UnknownFailure;
            LogDevicePositionAttempt(deviceNumber, trayNumber);
            var device = FindDeviceByNumber(deviceNumber);
            if (device == null)
            {
                LogDeviceNotFound();
                return DeviceResponse.DeviceNotFound;
            }
            if (!device.Enabled)
            {
                LogDeviceNotEnabled();
                return DeviceResponse.DeviceNotEnabled;
            }
            if (!_hanel.Init_Success)
            {
                LogDeviceNotInitialized(deviceNumber, trayNumber);
                return DeviceResponse.DeviceNotInitialized;
            }
            return ProcessDevicePositioning(deviceNumber, trayNumber);
        }
        private void LogDevicePositionAttempt(int deviceNumber, int trayNumber)
        {
            _logger.LogDetailAsync(
                $"Position Device: {deviceNumber} Tray: {trayNumber} Time: {DateTime.Now}")
                .SafeFireAndForget();
        }
        private HardwareDevice FindDeviceByNumber(int deviceNumber)
        {
            return _workstationView.HardwareDevices
                .FirstOrDefault(r => r.DeviceNumber == deviceNumber);
        }
        private void LogDeviceNotFound()
        {
            _logger.LogDetailAsync("Position Device: Device not Found.")
                .SafeFireAndForget();
        }
        private void LogDeviceNotEnabled()
        {
            _logger.LogDetailAsync("Position Device: Device not Enabled.")
                .SafeFireAndForget();
        }
        private void LogDeviceNotInitialized(int deviceNumber, int trayNumber)
        {
            _logger.LogDetailAsync(
                $"Device Not Initialized. Device: {deviceNumber} Tray: {trayNumber} " +
                $"Code: {_hanel.LastStatus_Code} Message: {_hanel.LastStatus_Message}")
                .SafeFireAndForget();
        }
        private DeviceResponse ProcessDevicePositioning(int deviceNumber, int trayNumber)
        {
            var continueLoop = true;
            while (continueLoop)
            {
                var status = GetDeviceStatus(deviceNumber);
                if (status.GoodStatus)
                {
                    // Handle good status logic
                }
                else
                {
                    // Handle bad status logic
                }
            }
            return DeviceResponse.Success; // Adjust based on actual logic
        }



        #endregion


        public DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 0, int depth = 0,
            int quantity = 0, string display = "")
        {
            var success = _hanel.Drive_Device(deviceNumber, trayNumber, facing, depth, quantity, display, ref cError);
            switch (success)
            {
                case true:
                    _logger.LogDetailAsync(
                            $"Drive tray {trayNumber} on device {deviceNumber} request submitted. Facing: {facing} Depth: {depth} Quantity: {quantity}")
                        .SafeFireAndForget();
                    return DeviceResponse.Success;

                default:
                    _logger.LogDetailAsync($"Problem submitting drive request. {cError}").SafeFireAndForget();
                    return DeviceResponse.UnknownFailure;
            }
        }

        //    var deviceResponse = DeviceResponse.UnknownFailure;
        //    _logger.LogDetailAsync($"Position Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()}  Time: {DateTime.Now}").SafeFireAndForget();
        //    var continueLoop = true;
        //    var loopCounter = 0;
        //    var device = _workstationView.HardwareDevices.FirstOrDefault(r => r.DeviceNumber == deviceNumber);
        //    if (device != null)
        //    {
        //        if (device.Enabled)
        //        {
        //            if (_hanel.Init_Success)
        //            {
        //                while (continueLoop)
        //                {
        //                    var status = GetDeviceStatus(deviceNumber);

        //                    // always true in the current implementation
        //                    if (status.GoodStatus)
        //                    {
        //                        status.TargetTray = trayNumber;
        //                        loopCounter = 0;
        //                        // Command Executed should indicate that the tray has arrived
        //                        // always true in the current implementation
        //                        if (!status.InMotion)
        //                        {
        //                            if (status.CommandAccepted && !status.CommandExecuted)
        //                            {
        //                                _logger.LogDetailAsync($"Command Accepted: true  Executed: false  Tray: {status.CurrentTray} NOT executed");
        //                                // first indication is that the tray did not arrive
        //                                // the tray did not arrive, so we need to let the operator know
        //                                // tell them to fix the problem and retrieve the tray manually
                                        
        //                                var prompt = new StringBuilder();
        //                                prompt.AppendLine($"TOWER: {deviceNumber}{Environment.NewLine}" +
        //                                                  $"{Environment.NewLine} It appears that tray {status.CurrentTray} is not in the window." +
        //                                                  $"{Environment.NewLine} Fix the problem and retrieve the tray manually." +
        //                                                  $"{Environment.NewLine} Press Yes when {status.CurrentTray} is in the window." +
        //                                                  $"{Environment.NewLine}");

        //                                var result = _dialogService.TowerError($"Tower Error - {deviceNumber}",
        //                                    prompt.ToString(), "Yes", "No");
        //                                if (result)
        //                                {
        //                                    // set status.CommandAccepted to false so that the next command can be sent
        //                                    status.CommandAccepted = false;
        //                                    status.CommandExecuted = false;
        //                                    status.CurrentTray = trayNumber;
        //                                    deviceResponse = DeviceResponse.Success;
        //                                    _previousTray[deviceNumber] = trayNumber;
        //                                    _logger.LogDetailAsync(
        //                                        $"SET Accepted and Executed = false. CurrentTray equal to TargetTray.  Return Success.");
        //                                    return deviceResponse;
        //                                }
        //                            }

        //                            //if (status.CurrentTray != trayNumber)
        //                            //{
        //                            //var previousTray = _previousTray[deviceNumber];
        //                            //if (previousTray != 0)
        //                            //{
        //                            //    // here's where the CurrentTray should be the same as the previousTray
        //                            //    if (status.CurrentTray != _previousTray[deviceNumber])
        //                            //    {
        //                            //        _logger.LogDetailAsync($"Tray did NOT arrive.").SafeFireAndForget();
        //                            //        _logger.LogDetailAsync($"Status.Current_Tray: {status.CurrentTray} Tray Number: {trayNumber}").SafeFireAndForget();
        //                            //        _logger.LogDetailAsync($"PreviousTray: {_previousTray[deviceNumber]}").SafeFireAndForget();
        //                            //        deviceResponse = DeviceResponse.TrayDidNotArrive;
        //                            //        _previousTray[deviceNumber] = 0;
        //                            //        break;
        //                            //    }
        //                            //}

        //                            cError = "";
        //                            if (_hanel.Drive_Device(deviceNumber, trayNumber, facing, depth, quantity, display, ref cError))
        //                            {
        //                                _logger.LogDetailAsync($"Drive tray {trayNumber.ToString()} on device {deviceNumber.ToString()} request submitted.  Facing:{facing.ToString()}  Depth:{depth.ToString()}  Quantity:{quantity.ToString()}").SafeFireAndForget();

        //                                continueLoop = false;
        //                                deviceResponse = DeviceResponse.Success;
        //                                _previousTray[deviceNumber] = trayNumber;


        //                                // status.TargetTray = trayNumber;
        //                                //status.CurrentTray = trayNumber;
        //                                status.CommandExecuted = false;
        //                                status.CommandAccepted = false;
        //                                status.InMotion = false;

        //                                _logger.LogDetailAsync($"PreviousTray Set to Device {deviceNumber.ToString()}  Tray: {trayNumber.ToString()}").SafeFireAndForget();
        //                            }
        //                            else
        //                            {
        //                                _logger.LogDetailAsync($"Problem submitting drive request.  {cError}").SafeFireAndForget();
        //                                continueLoop = false;
        //                            }
        //                            //}
        //                            //else  // current and requested trays are the same
        //                            //{
        //                            //    cError = "";
        //                            //    if (_hanel.Drive_Device(deviceNumber, trayNumber, facing, depth, quantity, display, ref cError))
        //                            //    {
        //                            //        _logger.LogDetailAsync($"Drive tray {trayNumber.ToString()} on device {deviceNumber.ToString()} request submitted.  Facing:{facing.ToString()}  Depth:{depth.ToString()}  Quantity:{quantity.ToString()}").SafeFireAndForget();

        //                            //        continueLoop = false;
        //                            //        deviceResponse = DeviceResponse.Success;
        //                            //        _previousTray[deviceNumber] = trayNumber;


        //                            //        // status.TargetTray = trayNumber;
        //                            //        //status.CurrentTray = trayNumber;
        //                            //        status.CommandExecuted = false;
        //                            //        status.CommandAccepted = false;
        //                            //        status.InMotion = false;

        //                            //        _logger.LogDetailAsync($"PreviousTray Set to Device {deviceNumber.ToString()}  Tray: {trayNumber.ToString()}").SafeFireAndForget();
        //                            //    }
        //                            //    else
        //                            //    {
        //                            //        _logger.LogDetailAsync($"Problem submitting drive request.  {cError}").SafeFireAndForget();
        //                            //        continueLoop = false;
        //                            //    }
        //                            //    _logger.LogDetailAsync($"Pick is on the same tray: Current Tray:  {status.CurrentTray.ToString()}  Tray Number:  {trayNumber.ToString()}").SafeFireAndForget();
        //                            //}
        //                        }
        //                        else //Waiting for Command to execute
        //                        {
        //                            if (loopCounter >= 10)
        //                            {
        //                                continueLoop = false;
        //                                deviceResponse = DeviceResponse.DeviceInMotion;
        //                            }
        //                            else
        //                            {
        //                                loopCounter += 1;
        //                                Thread.Sleep(millisecondsTimeout: 50);
        //                                var counter = loopCounter;
        //                                _logger.LogDetailAsync($"Position Device: Waiting for tray to be in position to send new command.  Current Tray: {status.CurrentTray} CommandExecuted: {status.CommandExecuted}  Loop Count: {counter.ToString()}").SafeFireAndForget();
        //                            }
        //                        }
        //                    }
        //                    else  //status.Good_Status = false
        //                    {
        //                        if (loopCounter >= 10)
        //                        {
        //                            continueLoop = false;
        //                            deviceResponse = DeviceResponse.DeviceBadStatus;
        //                        }
        //                        else
        //                        {
        //                            loopCounter += 1;
        //                            Thread.Sleep(millisecondsTimeout: 100);
        //                            var counter = loopCounter;
        //                            _logger.LogDetailAsync($"Device Response was Bad Status  LoopCounter: {counter}").SafeFireAndForget();
        //                        }
        //                    }
        //                } //while continue loop
        //            }
        //            else
        //            {
        //                _logger.LogDetailAsync($"Device Not Initialized.  Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()} Code is: {_hanel.LastStatus_Code.ToString()}  Message is: {_hanel.LastStatus_Message}").SafeFireAndForget();
        //                deviceResponse = DeviceResponse.DeviceNotInitialized;
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogDetailAsync($"Position Device: Device not Enabled.").SafeFireAndForget();
        //            deviceResponse = DeviceResponse.DeviceNotEnabled;
        //        }
        //    }
        //    else
        //    {
        //        _logger.LogDetailAsync($"Position Device: Device not Found.").SafeFireAndForget();
        //        deviceResponse = DeviceResponse.DeviceNotFound;
        //    }

        //    return deviceResponse;
        //}



        private void AbortNotification()
        {
            if (!_hanel.Init_Success)
            {
                _logger.LogDetailAsync($"Not Initialized.  Code is: {_hanel.LastStatus_Code.ToString()}  Message is: {_hanel.LastStatus_Message}").SafeFireAndForget();
                return;
            }
            cError = "";
            if (_hanel.Notification_DeRegister(_myNotificationHandle, ref cError))
                _logger.LogDetailAsync($"Notification aborted successfully...").SafeFireAndForget();
            else
                _logger.LogDetailAsync($"De-registration Error...  {cError}").SafeFireAndForget();
        }
        
        public DeviceResponse Park()
        {
            var response = DeviceResponse.UnknownFailure;
            foreach (var item in _workstationView.HardwareDevices)
            {
                if (!item.Enabled) continue;
                switch (item.DeviceTypeId)
                {
                    //Shuttle
                    case (int)DeviceTypeEnum.Shuttle:
                        response = PositionDevice(item.DeviceNumber, 0);
                        break;
                    //Carousel
                    case (int)DeviceTypeEnum.Carousel:
                        response = PositionDevice(item.DeviceNumber, 1);
                        break;
                }
            }
            return response;
        }

        public bool CloseController()
        {

            _logger.LogDetailAsync($"Close Hanel MP12D");
            var result = false;
            try
            {
                _logger.LogDetailAsync($"Call Stop");
                _hanel.Stop();
                _logger.LogDetailAsync($"Call Stop Return");
                _logger.LogDetailAsync($"Call Close Controller");

                result = _hanel.Close_Controller(ref cError);
                _logger.LogDetailAsync($"Call Close Controller Return");

                _logger.LogDetailAsync($"Close Hanel MP12D Controller - Success {cError}").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Close Hanel MP12D Controller - cError  {cError}  {Environment.NewLine} {ex.Message}  {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }

            return result;
        }

        public HanelDeviceStatus GetDeviceStatus(int deviceNumber)
        {
            string error = string.Empty;
            var msg = string.Empty;

            _logger.LogDetailAsync($"Device Number Status: {deviceNumber}").SafeFireAndForget();
            if (!_hanel.Init_Success)
            {
                LogDeviceNotInitialized();
                return new HanelDeviceStatus();
            }

            // When you request device status, you get status for all devices. That is the reason for the list.
            // Even if there is only a single device, it comes back in a list.
            // var deviceStatusList = new List<HanelDeviceStatus>();

            var deviceStatusList = _hanel.Get_Device_Status();
            if (!deviceStatusList.Any())
            {
                _logger.LogDetailAsync("Get Device Status request aborted...").SafeFireAndForget();
                return new HanelDeviceStatus();
            }
            LogDeviceStatus(deviceNumber);
            var deviceStatus = deviceStatusList.FirstOrDefault(item => item.Device == deviceNumber);
            if (deviceStatus != null)
            {
                msg = "Device: \t" + deviceStatus.Device + "\n" +
                                         "Device Number: \t" + deviceStatus.DeviceNumber + "\n" +

                                         "Target Tray: \t" + deviceStatus.TargetTray + "\n" +
                                         "Current Tray: \t" + deviceStatus.CurrentTray + "\n" +
                                         "In Motion: \t" + deviceStatus.InMotion + "\n" +
                                         "Good Status: \t" + deviceStatus.GoodStatus + "\n" +
                                         "In Alignment: \t" + deviceStatus.InAlignment + "\n" +
                                         "Last Command: \t" + deviceStatus.LastCommand + "\n" +
                                         "Last Status: \t" + deviceStatus.LastStatus + "\n" +
                                         "Command Accepted: \t" + deviceStatus.CommandAccepted + "\n" +
                                         "Command Executed: \t" + deviceStatus.CommandExecuted + "\n" +
                                         "Message: \t" + deviceStatus.StatusMessage;

            }
            _logger.LogDetailAsync(msg).SafeFireAndForget();

            return deviceStatus;
        }

        private void LogDeviceNotInitialized()
        {
            _logger.LogDetailAsync($"Device Status: Not Initialized. Code is: {_hanel.LastStatus_Code.ToString()} Message is: {_hanel.LastStatus_Message}").SafeFireAndForget();
            _logger.LogDetailAsync("Problem getting device status.").SafeFireAndForget();
        }
        private void LogDeviceStatus(int deviceNumber)
        {
            _logger.LogDetailAsync($"Device Status DeviceNumber: {deviceNumber}   Hardware Count: {_workstationView.EnabledDevices.Count}").SafeFireAndForget();
        }
        /// <summary>
        /// Resets the status of the Hanel device.
        /// </summary>
        /// <remarks>
        /// This method performs two operations:
        /// 1. Calls the ResetHanelDeviceStatus method of the Hanel_DeviceController instance if it is not null.
        /// 2. Calls the ResetPreviousTray method of the current Mp12D instance.
        /// </remarks>
        public void ResetHanelDeviceStatus()
        {
            _hanel?.ResetHanelDeviceStatus();
            ResetPreviousTray();
        }

        public byte[] ValidCommand(byte[] dataIn)
        {

            byte[] byteArray = null;
            if (dataIn.Length == 0)
            {
                return null;
            }

            _logger.LogDetailAsync($"dataIn: {dataIn.ByteArrayToHexString()}");
            // extract the byte array starting with 42 and ending with 10
            //var startIndex =   Array.IndexOf(dataIn, AST);
            var startIndex = FindAsterisk(dataIn);
            _logger.LogDetailAsync($"Start Index: {startIndex}");
            if (startIndex == -1)
            {
                _logger.LogDetailAsync($"Start Index = -1 {startIndex}");
                return null;
            }

            if (startIndex >= 0)
            {
                while (dataIn.First() != AST)
                {

                    dataIn = dataIn.Skip(1).ToArray();
                    _logger.LogDetailAsync($"Building dataIn: {dataIn.ByteArrayToHexString()} ");
                }
            }
            _logger.LogDetailAsync($"Final dataIn: {dataIn.ByteArrayToHexString()} ");
            startIndex = Array.IndexOf(dataIn, AST);
            _logger.LogDetailAsync($"Final dataIn Start Index: {startIndex} ");
            var endIndex = Array.IndexOf(dataIn, LF);
            _logger.LogDetailAsync($"Final dataIn End Index: {endIndex} ");
            if (endIndex == -1)
            {
                return null;
            }

            byteArray = dataIn.Skip(startIndex + 1).Take(endIndex - startIndex - 1).ToArray();
            _logger.LogDetailAsync($"Return ByteArray: {byteArray.ByteArrayToHexString()}");
            return byteArray;

        }

        public void ProcessCommand(string command)
        {

        }

        static int FindAsterisk(byte[] byteArray)
        {
            byte asterisk = (byte)'*'; // ASCII value of '*'

            for (int i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] == asterisk)
                {
                    return i; // Return the index of the asterisk
                }
            }

            return -1; // Return -1 if asterisk is not found
        }

        public void Stop()
        {

        }
    }
}


