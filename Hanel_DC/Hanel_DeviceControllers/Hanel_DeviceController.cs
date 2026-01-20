using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hanel_DC.HanelStatics;
using AlliedLogger;
using Hanel_DC.HanelUtilities;
using HanelCommands;
using System.Text;
using AsyncAwaitBestPractices;
using NeutronEvents;
using NeutronCore.Models;


namespace Hanel_DC.Hanel_DeviceControllers
{
    /// <summary>
    /// Represents a controller for Hanel devices, providing various functionalities 
    /// such as initialization, command enqueuing, device driving, and status management.
    /// </summary>
    /// <remarks>
    /// This class is responsible for managing the communication and operations of Hanel devices.
    /// It supports different controller types and provides methods to initialize the controller, 
    /// enqueue commands, drive devices, and manage device statuses.
    /// </remarks>
    public class Hanel_DeviceController
    {
        private readonly string ObjectID;
        private readonly string _ControllerType;
        private readonly IDialogService _dialogService;
        private readonly int nThreadSleepMilliseconds = 1000;
        private readonly int MaximumStatusErrors = 3;
        private int _Init_PercentageComplete;
        private bool _Machine_HaltConversation;
        private bool _Machine_SimulationMode;
        private string _LogPath;
        private string _LastStatus_Message;
        private List<int> _UnitNumbers_ValidList;
        public List<HanelDeviceStatus> _currentHanelDeviceStatusList;
        private List<HanelDeviceNotificationType> _Notifications_CallersRequestsList;
        private SynchronizationContext _CallersContext_Init;
        private object _CallersObject_Init;
        private SendOrPostCallback _CallersDelegateHandler_Init;
        private SendOrPostCallback _Notifications_DelegateHandler;
        private HanelDeviceNotificationType _Notification_Current;
        private IHanelDeviceController _MyDeviceController;
        private HanelMp12DDeviceController _MyMp12DController;

        private Queue<MachineRequestType> _Machine_CallersRequestsQueue;
        private Queue<IHanelCommand> _hanelCommandQueue;
        private HanelCommandService _hanelCommandService;
        private readonly IDynamicLogger _logger;
        private int _logLevel = 2;
        private readonly bool _testing;
        private readonly object _statusListLock = new object();

        public static char CR = Convert.ToChar(13);
        public static char LF = Convert.ToChar(10);
        public static char AST = Convert.ToChar(42);

        public static string[] Valid_Controller_Types()
        {
            int num = -1;
            string[] strArray = new string[HanelDcStatics.Valid_Controller_Types().Count];
            foreach (string validControllerType in (IEnumerable<string>)HanelDcStatics.Valid_Controller_Types())
                strArray[++num] = validControllerType;
            return strArray;
        }

        public static string Controller_Type_Hanel_Mp12D() => HanelDcStatics.Controller_Type_Hanel_Mp12D();

        public Hanel_DeviceController(string controllerType, IDialogService dialogService)
        {
            // _logger = NeutronCore.Global.Logger.SetupLogger("Hanel_DeviceController");
            _logger = NeutronCore.Global.Logger.SetupLogger("HanelLog");

            InitializeCollections();
            _ControllerType = controllerType;
            _dialogService = dialogService;
            InitializeCommandService();
            InitializeDeviceController(controllerType);
            Init_Success = false;
            Set_Init_PercentageComplete(0);
            ObjectID = HanelUtil.GetUniqueObjectIdentifier();
            LogControllerCreation(controllerType);
        }

        public Hanel_DeviceController(string controllerType, IDialogService dialogService, IDynamicLogger logger, bool testing)
        {
            _logger = logger;
            _testing = testing;
            InitializeCollections();
            _ControllerType = controllerType;
            _dialogService = dialogService;
            InitializeCommandService();
            InitializeDeviceController(controllerType);
            Init_Success = false;
            Set_Init_PercentageComplete(0);
            ObjectID = HanelUtil.GetUniqueObjectIdentifier();
            LogControllerCreation(controllerType);
        }

        private void InitializeCollections()
        {
            _UnitNumbers_ValidList = new List<int>();
            _Machine_CallersRequestsQueue = new Queue<MachineRequestType>();
            _hanelCommandQueue = new Queue<IHanelCommand>();
            _currentHanelDeviceStatusList = new List<HanelDeviceStatus>();
            _Notifications_CallersRequestsList = new List<HanelDeviceNotificationType>();
            Mediator.GetInstance().SendPollCommand += (s, e) => SendPollCommandToQueue(e.SendPoll);
        }

        private void SendPollCommandToQueue(bool sendPoll)
        {
            //var response = $"{AST}{CR}{LF}";

            var hanelCommand = _hanelCommandService.Poll();

            //var hanelCommand = new HanelCommand
            //{
            //    Command = Encoding.UTF8.GetBytes(response),
            //    HostCommand = "P",
            //    HostSubCommand = "XS"
            //};
            EnqueueHanelCommand(hanelCommand);
        }

        private void InitializeCommandService()
        {
            _hanelCommandService = _testing ? new HanelCommandService(_currentHanelDeviceStatusList.Count, _currentHanelDeviceStatusList, _logger) : new HanelCommandService(_currentHanelDeviceStatusList.Count, _currentHanelDeviceStatusList);
        }
        private void InitializeDeviceController(string controllerType)
        {
            if (controllerType == HanelDcStatics.Controller_Type_Hanel_Mp12D())
            {
                _MyDeviceController = new HanelMp12DDeviceController();
                _MyMp12DController = _MyDeviceController as HanelMp12DDeviceController;
            }
            // Uncomment and implement if needed
            // if (controllerType == HanelDcStatics.Controller_Type_Hanel_Mp12N())
            // {
            //     _MyDeviceController = new HanelMp12NDeviceController();
            //     _MyMp12NController = _MyDeviceController as HanelMp12NDeviceController;
            // }
        }
        private void LogControllerCreation(string controllerType)
        {
            _logger.LogDetailAsync($"{ObjectID} {controllerType} Device Controller Constructor").SafeFireAndForget();
        }

        public bool Init_Success { get; private set; }

        public int LastStatus_Code { get; private set; }

        public string LastStatus_Message
        {
            get => LastStatus_Code != 0 ? _LastStatus_Message : "";
            private set => _LastStatus_Message = value;
        }

        public bool Init_Controller(
            int controllerId,
            int commPort,
            int baudRate,
            int dataBits,
            string parity,
            int stopBits,
            bool machineSimulationMode,
            int logLevel,
            List<int> enabledDeviceUnitNumbers,
            object callersObject,
            SendOrPostCallback callersDelegateHandler,
            ref string cError)
        {
            return Init_Controller("", 0, controllerId, commPort, baudRate, dataBits, parity, stopBits,
                machineSimulationMode, logLevel, enabledDeviceUnitNumbers, callersObject, callersDelegateHandler,
                ref cError);
        }

        private bool Init_Controller(
            string hostIp,
            int hostPort,
            int controllerId,
            int commPort,
            int baudRate,
            int dataBits,
            string parity,
            int stopBits,
            bool machineSimulationMode,
            int logLevel,
            List<int> enabledDeviceUnitNumbers,
            object callersObject,
            SendOrPostCallback callersDelegateHandler,
            ref string cError)
        {
            string thisProc = $"{ObjectID} Init_Controller(): ";
            bool flag = false;
            bool openSuccess = false;
            if (_MyMp12DController != null)
                _logger.LogDetailAsync(
                    $"{thisProc}In - Controller ID: {controllerId} Com Port: {commPort} BaudRate: {baudRate} DataBits: {dataBits} Parity: {parity} StopBits: {stopBits} Simulation: {(machineSimulationMode ? "Yes" : "No")} LogLevel: {logLevel}").SafeFireAndForget();
            //if (_MyMp12NController != null)
            //    _logger.LogDetailAsync(
            //        $"{thisProc}In - Com Port: {commPort} BaudRate: {baudRate} DataBits: {dataBits} Parity: {parity} StopBits: {stopBits} Simulation: {(machineSimulationMode ? "Yes" : "No")} LogLevel: {logLevel}").SafeFireAndForget();
            //cError = "";

            // Simulation Mode
            if (machineSimulationMode)
            {
                // Init_Success = true;
                LastStatus_Code = 0;
                LastStatus_Message = "";
                Init_Success = true;
                flag = true;
                return true;
            }


            if (!Init_Success)
            {
                if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}Not Init").SafeFireAndForget();
                if (Get_Init_PercentageComplete() == 0 || Get_Init_PercentageComplete() == 100)
                {
                    if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}0 or 100").SafeFireAndForget();
                    Close_Device_Controller();
                    if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}Controller Closed").SafeFireAndForget();

                    if (_MyDeviceController != null)
                    {
                        if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}_MyDeviceController").SafeFireAndForget();
                        _UnitNumbers_ValidList = enabledDeviceUnitNumbers;
                        // _UnitNumbers_ValidList =
                        //    HanelDC.GetSortedListOfUniqueDeviceUnitNumbers(enabledDeviceUnitNumbers);
                        if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}There are {_UnitNumbers_ValidList.Count} Enabled Units.").SafeFireAndForget();
                        if (_UnitNumbers_ValidList.Count > 0)
                        {
                            _CallersContext_Init = SynchronizationContext.Current;
                            _CallersObject_Init = callersObject;
                            _CallersDelegateHandler_Init = callersDelegateHandler;
                            _Notifications_DelegateHandler = new SendOrPostCallback(NotificationInContextofCaller);
                            Set_Init_PercentageComplete(10);
                            if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}10%").SafeFireAndForget();
                            _currentHanelDeviceStatusList.Clear();
                            for (int index = 0; index < _UnitNumbers_ValidList.Count; ++index)
                            {
                                if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}Enabled Unit: {_UnitNumbers_ValidList[index]}").SafeFireAndForget();
                                _currentHanelDeviceStatusList.Add(new HanelDeviceStatus(index));
                                _currentHanelDeviceStatusList[index].Device = _UnitNumbers_ValidList[index];
                                _currentHanelDeviceStatusList[index].DeviceNumber = _UnitNumbers_ValidList[index];
                                _currentHanelDeviceStatusList[index].GoodStatus = true;
                                _currentHanelDeviceStatusList[index].LastStatus = DateTime.Now;
                                _currentHanelDeviceStatusList[index].LastCommand = DateTime.Now;
                                _currentHanelDeviceStatusList[index].TargetTray = 0;
                                _currentHanelDeviceStatusList[index].CurrentTray = 0;
                                _currentHanelDeviceStatusList[index].InMotion = false;
                                _currentHanelDeviceStatusList[index].InAlignment = true;
                                _currentHanelDeviceStatusList[index].StatusMessage = "";
                                _currentHanelDeviceStatusList[index].CommandSent = false;
                                _currentHanelDeviceStatusList[index].CommandAccepted = false;
                                _currentHanelDeviceStatusList[index].CommandExecuted = false;

                                //    if (machineSimulationMode == false)
                                //    {
                                //        _currentHanelDeviceStatusList[index].GoodStatus = true;
                                //    }

                                //    _currentHanelDeviceStatusList[index].CommandAccepted = false;
                                //_currentHanelDeviceStatusList[index].CommandExecuted = false;
                            }

                            Set_Init_PercentageComplete(25);
                            Task.Run((Action)(() =>
                            {
                                Set_Init_PercentageComplete(50);
                                string cError1 = "";
                                string cError2 = "";
                                _Machine_SimulationMode = machineSimulationMode;
                                if (_MyMp12DController != null)
                                {
                                    if (_logLevel == 8) _logger.LogDetailAsync(
                                        $"{thisProc}Opening a channel to the Hanel MP12D machine controller.").SafeFireAndForget();

                                    openSuccess = _MyMp12DController.OpenChannel(commPort, baudRate, dataBits, parity,
                                        stopBits, cError1, machineSimulationMode, logLevel,
                                        _currentHanelDeviceStatusList, _LogPath).Result;
                                }

                                if (_logLevel == 8) _logger.LogDetailAsync($"Open Success: {openSuccess}").SafeFireAndForget();

                                if (openSuccess)
                                {
                                    LastStatus_Code = 0;
                                    LastStatus_Message = "";
                                    Init_Success = true;
                                    flag = true;
                                }
                                else
                                {
                                    LastStatus_Code = -1;
                                    LastStatus_Message = $"Unable to connect with the serial port. \n{cError1}";
                                    if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}Error Message: {LastStatus_Message}").SafeFireAndForget();
                                }

                                //Set_Init_PercentageComplete(100);
                                if (_logLevel == 8) _logger.LogDetailAsync($"{thisProc}End of task.").SafeFireAndForget();
                            })).ContinueWith(t =>
                            {
                                OpenChannelComplete(); // Notify completion
                            });
                        }
                        else
                        {
                            cError = $"{thisProc}No device unit numbers have been defined.";
                            _logger.LogDetailAsync(cError).SafeFireAndForget();
                            Set_Init_PercentageComplete(100);
                            flag = false;
                        }
                    }
                    else
                    {
                        // cError = ThisProc + "Incorrect version of Hart_LV.DLL. Version " + this._MyDeviceController.Get_Required_LV_Version() + " of " + this._ControllerType + " is required.";
                        _logger.LogDetailAsync(cError).SafeFireAndForget();
                        Set_Init_PercentageComplete(100);
                        flag = false;
                    }
                }
                else
                    _logger.LogDetailAsync($"{thisProc}Existing request in progress. Let it run...").SafeFireAndForget();
            }
            else
            {
                _logger.LogDetailAsync($"{thisProc}Request already completed. Let the caller know.").SafeFireAndForget();
                Set_Init_PercentageComplete(100);
            }

            return flag;
        }

        private void OpenChannelComplete()
        {
            if (Init_Success)
            {
                _Hanel_ConversationStart_Async();
            }


            //Set_Init_PercentageComplete(100);
        }

        public int Get_Init_PercentageComplete() => _Init_PercentageComplete;

        private void Set_Init_PercentageComplete(int value)
        {
            if (_logLevel == 8) _logger.LogDetailAsync($"Percentage Value: {value}").SafeFireAndForget();
            if (value == 100 && Init_Success && _Init_PercentageComplete != 100)
                //_Machine_ConversationStart_Async();
                _Hanel_ConversationStart_Async();
            if (value <= _Init_PercentageComplete)
                return;
            if (_logLevel == 8) _logger.LogDetailAsync(
                $"{ObjectID} Init_PercentageComplete(): New Value: {value} Current Value: {_Init_PercentageComplete}").SafeFireAndForget();
            _Init_PercentageComplete = value;
            if (_Init_PercentageComplete <= 0 || _CallersContext_Init == null || _CallersDelegateHandler_Init == null ||
                _CallersObject_Init == null)
                return;
            HanelDcInit state = new HanelDcInit(_CallersObject_Init, value, Init_Success);
            //if (SynchronizationContext.Current == _CallersContext_Init)
            //    SynchronizationContext.Current.Post(_CallersDelegateHandler_Init, state);
            //else
            //    _CallersContext_Init.Post(_CallersDelegateHandler_Init, state);
        }

        public void EnqueueHanelCommand(IHanelCommand hanelCommand)
        {
            lock (_hanelCommandQueue)
            {
                _hanelCommandQueue.Enqueue(hanelCommand);
            }
            _logger.LogDetailAsync($"*G0  Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();

        }

        public bool GetTrayInWindow(int lift, int accessPoint = 1)
        {
            return true;

            foreach (var i in _UnitNumbers_ValidList)
            {
                var hanelCommand =
                _hanelCommandService.GetTrayInWindow(i, accessPoint);
                if (_logLevel == 8) _logger.LogDetailAsync($"Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();
                _hanelCommandQueue.Enqueue(hanelCommand);
            }

            return true;
        }

        public void GetTraysInWindow()
        {
            // Don't get the trays in windows for this version
            return;
            if (_logLevel == 8) _logger.LogDetailAsync($"Valid Unit Numbers - {_UnitNumbers_ValidList[0]} - {_UnitNumbers_ValidList[1]} - {_UnitNumbers_ValidList[2]}").SafeFireAndForget();
            var accessPoint = 1;
            foreach (var validUnitNumber in _UnitNumbers_ValidList)
            {
                if (_logLevel == 8) _logger.LogDetailAsync($"Valid Unit Number - {validUnitNumber}").SafeFireAndForget();
                var hanelCommand =
                    _hanelCommandService.GetTrayInWindow(validUnitNumber, accessPoint);
                if (_logLevel == 8) _logger.LogDetailAsync($"Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();
                _hanelCommandQueue.Enqueue(hanelCommand);
                Thread.Sleep(200);
            }
        }

        public bool GetTray(IHanelCommand hanelCommand)
        {
            if (_logLevel == 8) _logger.LogDetailAsync($"Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();
            _hanelCommandQueue.Enqueue(hanelCommand);
            return true;
        }
        public bool Drive_Device(IHanelCommand hanelCommand)
        {
            if (_logLevel == 8) _logger.LogDetailAsync($"Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();
            _hanelCommandQueue.Enqueue(hanelCommand);
            return true;
        }

        // new Drive_Device
        public bool Drive_Device(
            int deviceUnit,
            int tray,
            int facing,
            int depth,
            int quantity,
            string displayText,
            ref string cError)
        {
            const int maxFacing = 99;
            const int maxDepth = 99;
            const int maxQuantity = 9999;

            if (!Init_Success)
            {
                cError = "Controller not initialized. Unable to drive device.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }
            if (!_UnitNumbers_ValidList.Contains(deviceUnit))
            {
                cError = $"{deviceUnit} is an invalid device unit number.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }
            int index = _UnitNumbers_ValidList.IndexOf(deviceUnit);
            if (tray < 0)
            {
                cError = $"{tray} is an invalid tray number. Must be greater than zero.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }
            if (facing < 0 || facing > maxFacing)
            {
                cError = $"{facing} is an invalid facing. Must be in the range of zero to {maxFacing}.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }
            if (depth < 0 || depth > maxDepth)
            {
                cError = $"{depth} is an invalid depth. Must be in the range of zero to {maxDepth}.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }
            if (quantity < 0 || quantity > maxQuantity)
            {
                cError = $"{quantity} is an invalid quantity. Must be in the range of zero to {maxQuantity}.";
                if (_logLevel == 8) _logger.LogDetailAsync(cError).SafeFireAndForget();
                return false;
            }


            if (!_Machine_SimulationMode)
            {
                if (!_currentHanelDeviceStatusList[index].GoodStatus)
                {
                    cError =
                        $"Device {deviceUnit} appears to be off-line. Request rejected.";
                    if (_logLevel == 8) _logger.LogDetailAsync($"{cError}").SafeFireAndForget();
                    return false;
                }

                if (_logLevel == 8) _logger.LogDetailAsync($"Device appears to be on-line.").SafeFireAndForget();
                if (_currentHanelDeviceStatusList[index].InMotion)
                {
                    cError = $"Device {deviceUnit} is in motion. Request rejected.";
                    if (_logLevel == 8) _logger.LogDetailAsync($"{cError}").SafeFireAndForget();
                    return false;
                }
            }

            _currentHanelDeviceStatusList[index].TargetTray = tray;

            var hanelCommand =
                _hanelCommandService.MoveDeviceCommand(deviceUnit, 1, tray, facing, depth);
            //_ = Mediator.GetInstance().OnDisplayMessage(this, hanelCommand.Command);
            _logger.LogDetailAsync($"*G0  Hanel Command Enqueue: {hanelCommand.Command}  Drive Request Submitted.").SafeFireAndForget();
            cError = "";

            _hanelCommandQueue.Enqueue(hanelCommand);

            return true;
        }

        private void NotificationInContextofCaller(object Callersobject) =>
            _Notification_Current.CallBack(_Notification_Current);

        public Guid Notification_Register(
            object Callersobject,
            int TargetDevice,
            int TargetTray,
            int RequiredMotionStatus,
            int RequiredAlignmentStatus,
            HanelTellMeWhenTrayArrives MyCallBack,
            bool AutoDeRegister,
            int TimeOutSeconds,
            ref string cError)
        {
            string str = $"{ObjectID} Notification_Register(): ";
            Guid Unique_ID = Guid.Empty;
            SynchronizationContext current = SynchronizationContext.Current;
            cError = "";
            if (!Init_Success)
                cError = "Controller not initialized.";
            else if (_UnitNumbers_ValidList.Contains(TargetDevice))
            {
                if (TargetTray < 0)
                    cError = "Invalid tray number.";
                else if (TargetTray == 0)
                    cError = "Cannot register a notification for tray zero.";
                else if (MyCallBack == null)
                    cError = "Callback may not be null.";
                else if (TimeOutSeconds < 0)
                    cError = "TimeOutSeconds may not be negative.";
                else if (RequiredMotionStatus < -1 || RequiredMotionStatus > 1)
                    cError = "RequiredMotionStatus is out of range.";
                else if (RequiredAlignmentStatus < -1 || RequiredAlignmentStatus > 1)
                {
                    cError = "RequiredAlignmentStatus is out of range.";
                }
                else
                {
                    Unique_ID = Guid.NewGuid();
                    DateTime now = DateTime.Now;
                    int count = _Notifications_CallersRequestsList.Count;
                    _Notifications_CallersRequestsList.Add(new HanelDeviceNotificationType(Unique_ID, current,
                        Callersobject, TimeOutSeconds, now, now.AddSeconds((double)TimeOutSeconds), MyCallBack,
                        TargetDevice, TargetTray, RequiredMotionStatus, RequiredAlignmentStatus, AutoDeRegister));
                    cError = "";
                }
            }

            if (cError != "")
                _logger.LogDetailAsync(str + cError).SafeFireAndForget();
            return Unique_ID;
        }

        public virtual bool Notification_DeRegister(Guid notificationHandle, ref string cError)
        {
            string str = $"{ObjectID} Notification_DeRegister(): ";
            bool flag = false;
            if (Init_Success)
            {
                cError = "Notification handle not found. No notifications were deregistered.";
                for (int index = _Notifications_CallersRequestsList.Count - 1; index >= 0; --index)
                {
                    if (_Notifications_CallersRequestsList[index].Unique_ID == notificationHandle)
                    {
                        _Notifications_CallersRequestsList.RemoveAt(index);
                        cError = "";
                        flag = true;
                    }
                }
            }
            else
                cError = "Controller not initialized. Call Init_Controller().";

            if (!flag)
                _logger.LogDetailAsync(str + cError).SafeFireAndForget();
            return flag;
        }

        /// <summary>
        /// Retrieves the current status of the device.
        /// </summary>
        /// <returns>
        /// A list of <see cref="HanelDeviceStatus"/> representing the status of the device. 
        /// If the device initialization is not successful, an empty list is returned.
        /// </returns>
        /// <remarks>
        /// The method logs the operation details using the <see cref="IDynamicLogger"/> instance.
        /// </remarks>
        public List<HanelDeviceStatus> Get_Device_Status()
        {

            var str = $"{ObjectID} Get_Device_Status()";
            if (_logLevel == 8) _logger.LogDetailAsync($"{str}").SafeFireAndForget();
            var hanelDeviceStatusList = new List<HanelDeviceStatus>();
            if (!Init_Success) return hanelDeviceStatusList;
            return _currentHanelDeviceStatusList ?? hanelDeviceStatusList;
        }

        public void ResetHanelDeviceStatus()
        {
            if (_currentHanelDeviceStatusList.Count <= 0) return;
            foreach (var device in _currentHanelDeviceStatusList)
            {
                device.GoodStatus = true;
                device.LastStatus = DateTime.Now;
                device.LastCommand = DateTime.Now;
                device.TargetTray = 0;
                device.CurrentTray = 0;
                device.InMotion = false;
                device.InAlignment = true;
                device.StatusMessage = "";
                device.CommandAccepted = false;
                device.CommandExecuted = false;
            }

            if (_logLevel == 8) DumpStatus();
        }

        private void DumpStatus()
        {
            foreach (var deviceStatus in _currentHanelDeviceStatusList)
            {
                if (deviceStatus == null) continue;

                var sb = new StringBuilder();
                sb.AppendLine("Hanel_DeviceController - ResetHanelDeviceStatus");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  DeviceNumber = {deviceStatus.DeviceNumber}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  GoodStatus = {deviceStatus.GoodStatus}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastStatus = {deviceStatus.LastStatus}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  LastCommand = {deviceStatus.LastCommand}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  Device = {deviceStatus.Device}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  TargetTray = {deviceStatus.TargetTray}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CurrentTray = {deviceStatus.CurrentTray}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InMotion = {deviceStatus.InMotion}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  InAlignment = {deviceStatus.InAlignment}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  StatusMessage = {deviceStatus.StatusMessage}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  ActiveErrorCount ={deviceStatus.ActiveErrorCount}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandAccepted = {deviceStatus.CommandAccepted}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber}  CommandExecuted = {deviceStatus.CommandExecuted}");
                sb.AppendLine($"Device #{deviceStatus.DeviceNumber} Switched On = {deviceStatus.SwitchedOn}");
                sb.AppendLine($"--------------------------------------------------");
                sb.AppendLine();

                if (_logLevel == 8) _logger.LogDetailAsync(sb.ToString()).ConfigureAwait(false);
            }
        }

        //public bool Get_Device_Status(ref List<HanelDeviceStatus> newList, ref string cError)
        //{
        //    string str = $"{ObjectID} Get_Device_Status(): ";
        //    bool deviceStatus = false;
        //    if (Init_Success)
        //    {
        //        if (_currentHanelDeviceStatusList.Count > 0)
        //        {
        //            newList = _currentHanelDeviceStatusList.ConvertAll<HanelDeviceStatus>(
        //                (Converter<HanelDeviceStatus, HanelDeviceStatus>)(abc => new HanelDeviceStatus(abc.DeviceNumber,
        //                    abc.GoodStatus, abc.LastStatus, abc.LastCommand, abc.Device, abc.TargetTray,
        //                    abc.CurrentTray, abc.InMotion, abc.InAlignment, abc.StatusMessage, abc.CommandSent, abc.CommandAccepted,
        //                    abc.CommandExecuted)));
        //            deviceStatus = true;
        //            cError = "";
        //        }
        //        else
        //        {
        //            cError = "No device status available.";
        //            _logger.LogDetailAsync(str + cError).SafeFireAndForget();
        //        }
        //    }
        //    else
        //    {
        //        cError = "Controller not initialized. No device status available.";
        //        _logger.LogDetailAsync(str + cError).SafeFireAndForget();
        //    }

        //    return deviceStatus;
        //}

        //public bool Show_Text(
        //    int deviceNumber,
        //    string textRow1,
        //    string textRow2,
        //    string textRow3,
        //    string textRow4,
        //    ref string cError,
        //    int accessPoint = 1)
        //{

        //    bool flag = false;
        //    int twenty = 20;

        //    if (_MyMp12DController != null)
        //    {
        //        if (Init_Success)
        //        {
        //            if (_UnitNumbers_ValidList.Contains(deviceNumber))
        //            {
        //                if ((textRow1 + textRow2 + textRow3 + textRow4).Trim().Length > 0)
        //                {
        //                    var displayLines = new List<DisplayLine>
        //                    {
        //                       new DisplayLine(line: "1", column: "0", text: textRow1.TrimStart().Substring(0, twenty)),
        //                       new DisplayLine(line: "2", column: "0", text: textRow2.TrimStart().Substring(0, twenty)),
        //                       new DisplayLine(line: "3", column: "0", text: textRow3.TrimStart().Substring(0, twenty)),
        //                       new DisplayLine(line: "4", column :"0", text: textRow4.TrimStart().Substring(0, twenty))
        //                    };

        //                    if (_currentHanelDeviceStatusList[deviceNumber].GoodStatus)
        //                    {
        //                        var command = _hanelCommandService.DisplayText(deviceNumber, accessPoint, displayLines);

        //                        _hanelCommandQueue.Enqueue(command);
        //                        cError = "";
        //                        flag = true;
        //                    }
        //                    else
        //                        cError = "Device off-line. Request rejected.";
        //                }
        //                else
        //                    cError = "Invalid text. May not send blanks in all text parameters.";
        //            }
        //            else
        //                cError = "Invalid device unit number specified.";
        //        }
        //        else
        //            cError = "Controller not initialized. Unable to drive device.";
        //    }
        //    else
        //    {
        //        cError = "";
        //        flag = true;
        //    }

        //    if (!flag)
        //        _logger.LogDetailAsync(cError).SafeFireAndForget();
        //    return flag;
        //}

        //public bool Clear_Text(int requestedDeviceNumber, ref string cError)
        //{
        //    string str = $"{ObjectID} Clear_Text(): ";
        //    bool flag = false;
        //    _logger.LogDetailAsync($"{str}Beg").SafeFireAndForget();
        //    // if (_MyMp12NController != null)
        //    if (_MyMp12DController != null)
        //    {
        //        if (Init_Success)
        //        {
        //            if (_UnitNumbers_ValidList.Contains(requestedDeviceNumber))
        //            {
        //                if (_currentHanelDeviceStatusList[_UnitNumbers_ValidList.IndexOf(requestedDeviceNumber)]
        //                    .GoodStatus)
        //                {
        //                    _Machine_CallersRequestsQueue.Enqueue(new MachineRequestType()
        //                    {
        //                        DeviceUnit = requestedDeviceNumber,
        //                        TextAction = -1
        //                    });
        //                    cError = "";
        //                    flag = true;
        //                }
        //                else
        //                    cError = "Device in motion. Request rejected.";
        //            }
        //            else
        //                cError = "Invalid device unit number specified.";
        //        }
        //        else
        //            cError = "Controller not initialized. Unable to drive device.";
        //    }
        //    else
        //    {
        //        cError = "";
        //        flag = true;
        //    }

        //    if (!flag)
        //        _logger.LogDetailAsync(str + cError).SafeFireAndForget();
        //    _logger.LogDetailAsync($"{str}End").SafeFireAndForget();
        //    return flag;
        //}

        public bool Close_Controller(ref string cError)
        {
            string str = $"{ObjectID} Close_Controller(): ";
            bool flag = false;
            _logger.LogDetailAsync($"{str}Beg").SafeFireAndForget();
            if (Init_Success)
            {
                Close_Device_Controller();
                flag = true;
                cError = "";
            }
            else
            {
                cError = "Controller not initialized. Cannot close.";
                _logger.LogDetailAsync(str + cError).SafeFireAndForget();
            }

            _logger.LogDetailAsync($"{str}End").SafeFireAndForget();
            return flag;
        }

        private void Close_Device_Controller()
        {
            string str = $"{ObjectID} Close_Device_Controller(): ";
            _logger.LogDetailAsync($"{str}Beg").SafeFireAndForget();
            Init_Success = false;
            _Machine_HaltConversation = true;
            _UnitNumbers_ValidList.Clear();
            _currentHanelDeviceStatusList.Clear();
            _Notifications_CallersRequestsList.Clear();
            Set_Init_PercentageComplete(0);
            _MyDeviceController.CloseChannel();
            _Machine_CallersRequestsQueue.Clear();
            _hanelCommandQueue.Clear();
            _logger.LogDetailAsync($"{str}End").SafeFireAndForget();

        }

        #region Current Working DeQueue Process

        private async Task _Hanel_ConversationStart_Async()
        {
            var thisProc = $"_Hanel_ConversationStart_Async(): ";
            _logger.LogDetailAsync($"{thisProc} Starting conversation.").SafeFireAndForget();
            //Task.Run(() =>
            //{
            var num1 = 0;
            int nTray = 0;
            int index1 = 0;
            bool nInMotion = false;
            bool nInAlignment = false;
            string str1 = "";
            bool flag1 = false;
            string cError1 = "";
            string cError2 = "";
            LastStatus_Code = 0;
            LastStatus_Message = "";
            // Clear the StatusMessage for ALL devices
            foreach (HanelDeviceStatus currentHanelDeviceStatus in _currentHanelDeviceStatusList)
                currentHanelDeviceStatus.StatusMessage = "";
            _Machine_HaltConversation = false;

            // Run while _Machine_HaltConversation is TRUE
            while (!_Machine_HaltConversation)
            {
                int num2;
                // If there is something in the queue
                if (_hanelCommandQueue.Count > 0)
                {
                    // _logger.LogDetailAsync($"{thisProc}There is a request in the queue, Dequeue.").SafeFireAndForget();

                    // Peek at the next request from the queue
                    var ready = true;    // IsReadyToDequeue(_hanelCommandQueue.Peek());
                    // _logger.LogDetailAsync($"Back From Peek, Ready?: {ready}").SafeFireAndForget();
                    if (ready)
                    {
                        lock (_hanelCommandQueue)
                        {
                            _logger.LogDetailAsync($"*G0  Hanel Command Queue Count Before: {_hanelCommandQueue.Count}").SafeFireAndForget();

                            var hanelCommand = _hanelCommandQueue.Dequeue();

                            _logger.LogDetailAsync($"*G0  Dequeued Hanel Command: {hanelCommand.Command}").SafeFireAndForget();
                            _logger.LogDetailAsync($"*G0  Hanel Command Queue Count After: {_hanelCommandQueue.Count}").SafeFireAndForget();

                            if (!_MyMp12DController.DriveDevice(hanelCommand, ref cError2))
                            {
                                _logger.LogDetailAsync($"{thisProc}Machine Drive error Message: {cError2}").SafeFireAndForget();
                                str1 = $"Machine error message: {cError2} Command: {hanelCommand.Command}";
                                Mediator.GetInstance().OnDisplayMessage(this, str1);
                            }
                            else
                            {
                                if (_logLevel == 8) _logger.LogDetailAsync($"Hanel Command Drive Device Success. {hanelCommand.Command}").SafeFireAndForget();
                            }
                        }

                    }
                }
                await Task.Delay(nThreadSleepMilliseconds);
                //    continue;

                //Thread.Sleep(nThreadSleepMilliseconds);
            }
            _logger.LogDetailAsync($"{thisProc}Someone or something has requested this process to shut down.").SafeFireAndForget();
        }


        #endregion

        private bool IsReadyToDequeue(IHanelCommand hanelCommand)
        {
            if (_logLevel == 8) _logger.LogDetailAsync($"Start the PEEK process. {hanelCommand.Command}").SafeFireAndForget();
            var result = false;

            // Polling Command
            if (hanelCommand.Device == 0)
            {
                return true;
            }

            if (_currentHanelDeviceStatusList == null)
            {
                _logger.LogDetailAsync("Device status list is null.").SafeFireAndForget();
                return false;
            }

            var device = hanelCommand.Device;
            var status = default(HanelDeviceStatus);

            if (_logLevel == 8) _logger.LogDetailAsync("Lock the StatusList for peek operation.").SafeFireAndForget();
            lock (_statusListLock)
            {
                status = _currentHanelDeviceStatusList?.FirstOrDefault(r => r.DeviceNumber == device);
            }
            if (status != null)
            {
                if (_logLevel == 8) _logger.LogDetailAsync($"Peek at Tower {device} Status Command Sent: {status.CommandSent} - Accepted: {status.CommandAccepted} - Executed: {status.CommandExecuted}").SafeFireAndForget();
                if (status.CommandSent == true || status.CommandAccepted == true)
                {
                    return false;
                }
                else
                {
                    return true;
                }


                //if (status.CommandAccepted)  // && !status.CommandExecuted)
                //{
                //    _logger.LogDetailAsync($"Command Accepted: true  Executed: false  Tray: {status.CurrentTray} NOT executed").SafeFireAndForget();
                //    // first indication is that the tray did not arrive
                //    // the tray did not arrive, so we need to let the operator know
                //    // tell them to fix the problem and retrieve the tray manually

                //    var prompt = new StringBuilder();
                //    prompt.AppendLine($"TOWER: {device}{Environment.NewLine}" +
                //                      $"{Environment.NewLine} Tray {status.CurrentTray} has not arrived in the window." +
                //                      $"{Environment.NewLine} If the tray is moving, wait until it's in the window to Press 'YES', " +
                //                      $"{Environment.NewLine} otherwise fix the problem and retrieve the tray manually." +
                //                      $"{Environment.NewLine} Press 'YES' when {status.CurrentTray} is in the window." +
                //                      $"{Environment.NewLine}");

                //    result = _dialogService.TowerError($"Tower Information - {device}",
                //        prompt.ToString(), "Yes", "No");
                //    if (result)
                //    {
                //        // set status.CommandAccepted to false so that the next command can be sent
                //        status.CommandAccepted = false;
                //        status.CommandExecuted = false;
                //        status.CurrentTray = status.TargetTray;
                //        _logger.LogDetailAsync(
                //            $"SET Accepted and Executed = false. CurrentTray equal to TargetTray.  Return Success.").SafeFireAndForget();
                //    }
                //    else
                //    {
                //        // User said No, so we need to make sure the CommandAccepted is true
                //        // so that the command can be re-queued.
                //        status.CommandAccepted = true;
                //    }
                //}
                //else
                //{
                //    result = true; // Command is ready to be executed
                //}
            }
            return result;
        }

        private bool AlignmentQualificationsHaveBeenMet(int RequiredAlignment, bool ActualAlignment)
        {
            bool flag = false;
            if (RequiredAlignment == 0)
                flag = true;
            else if (RequiredAlignment < 0 && !ActualAlignment)
                flag = true;
            else if (RequiredAlignment > 0 & ActualAlignment)
                flag = true;
            return flag;
        }

        public void Stop()
        {
            _logger.LogDetailAsync($"Stop - Begin").SafeFireAndForget();
            _MyDeviceController.Stop();
            _logger.LogDetailAsync($"Stop - End").SafeFireAndForget();

        }
    }
}
