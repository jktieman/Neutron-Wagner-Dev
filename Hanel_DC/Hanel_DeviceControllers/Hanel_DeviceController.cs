using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hanel_DC.HanelStatics;
using AlliedLogger;
using Hanel_DC.HanelUtilities;
using System.Reflection;
using HanelCommands;


namespace Hanel_DC.Hanel_DeviceControllers
{
    public class Hanel_DeviceController
    {
        private readonly string ObjectID;
        private readonly string _ControllerType;
        private readonly int nThreadSleepMilliseconds = 500;
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
        private HanelMp12NDeviceController _MyMp12NController;

        private Queue<MachineRequestType> _Machine_CallersRequestsQueue;
        private Queue<IHanelCommand> _hanelCommandQueue;
        private HanelCommandService _hanelCommandService;
        private readonly IDynamicLogger _logger;

        public static string[] Valid_Controller_Types()
        {
            int num = -1;
            string[] strArray = new string[HanelDcStatics.Valid_Controller_Types().Count];
            foreach (string validControllerType in (IEnumerable<string>)HanelDcStatics.Valid_Controller_Types())
                strArray[++num] = validControllerType;
            return strArray;
        }

        public static string Controller_Type_Hanel_Mp12D() => HanelDcStatics.Controller_Type_Hanel_Mp12D();

        public static string Controller_Type_Hanel_Mp12N() => HanelDcStatics.Controller_Type_Hanel_Mp12N();

        public Hanel_DeviceController(string controllerType)
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("Hanel_DeviceController");
            _UnitNumbers_ValidList = new List<int>();
            _Machine_CallersRequestsQueue = new Queue<MachineRequestType>();
            _hanelCommandQueue = new Queue<IHanelCommand>();
            _currentHanelDeviceStatusList = new List<HanelDeviceStatus>();
            _Notifications_CallersRequestsList = new List<HanelDeviceNotificationType>();
            _ControllerType = controllerType;
            _hanelCommandService = new HanelCommandService(_currentHanelDeviceStatusList.Count, _currentHanelDeviceStatusList);
            if (controllerType == HanelDcStatics.Controller_Type_Hanel_Mp12D())
            {
                _MyDeviceController = new HanelMp12DDeviceController();
                _MyMp12DController = _MyDeviceController as HanelMp12DDeviceController;
            }
            //if (controllerType == HanelDcStatics.Controller_Type_Hanel_Mp12N())
            //{
            //    _MyDeviceController = new HanelMp12NDeviceController();
            //    _MyMp12NController = _MyDeviceController as HanelMp12NDeviceController;
            //}


            Init_Success = false;
            Set_Init_PercentageComplete(0);
            //Hart_WebAuthorization.WebAuth_Reset();
            //this.LastStatus_Code = Hart_WebAuthorization.WebAuth_ErrorCode;
            //this.LastStatus_Message = Hart_WebAuthorization.WebAuth_Message;
            ObjectID = HanelUtil.GetUniqueObjectIdentifier();
            //_logger = new DynamicLogger(LogFolder, "HanelDeviceController", "true");
            //_LogPath = _logger.LogFileDir = LogFolder;
            //// Logger.FileName =
            //_logger.FileName = _MyDeviceController.Get_LogFileName();
            _ = _logger.LogDetailAsync(ObjectID + " " + controllerType + " Device Controller Constructor ");
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
            IReadOnlyCollection<int> enabledDeviceUnitNumbers,
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
            IReadOnlyCollection<int> enabledDeviceUnitNumbers,
            object callersObject,
            SendOrPostCallback callersDelegateHandler,
            ref string cError)
        {
            string thisProc = ObjectID + " Init_Controller(): ";
            bool flag = false;
            bool openSuccess = false;
            if (_MyMp12DController != null)
                _ = _logger.LogDetailAsync(thisProc + "In - Controller ID: " + controllerId.ToString() + " Com Port: " +
                                          commPort.ToString() + " BaudRate: " + baudRate.ToString() + " DataBits: " +
                                          dataBits.ToString() + " Parity: " + parity + " StopBits: " +
                                          stopBits.ToString() + " Simulation: " + (machineSimulationMode ? "Yes" : "No") +
                                          " LogLevel: " + logLevel.ToString());
            if (_MyMp12NController != null)
                _ = _logger.LogDetailAsync(thisProc + "In - Com Port: " + commPort.ToString() + " BaudRate: " +
                                          baudRate.ToString() + " DataBits: " + dataBits.ToString() + " Parity: " +
                                          parity + " StopBits: " + stopBits.ToString() + " Simulation: " +
                                          (machineSimulationMode ? "Yes" : "No") + " LogLevel: " + logLevel.ToString());
            cError = "";

            // Simulation Mode
            if (machineSimulationMode)
            {
                Init_Success = true;
                LastStatus_Code = 0;
                LastStatus_Message = "";
                Init_Success = true;
                flag = true;
                return true;
            }


            if (!Init_Success)
            {
                _ = _logger.LogDetailAsync(thisProc + "Not Init");
                if (Get_Init_PercentageComplete() == 0 || Get_Init_PercentageComplete() == 100)
                {
                    _ = _logger.LogDetailAsync(thisProc + "0 or 100");
                    Close_Device_Controller();
                    _ = _logger.LogDetailAsync(thisProc + "Controller Closed");

                    if (_MyDeviceController != null)
                    {
                        _ = _logger.LogDetailAsync(thisProc + "_MyDeviceController");
                        //_UnitNumbers_ValidList.Clear();
                        _UnitNumbers_ValidList =
                            HanelDC.GetSortedListOfUniqueDeviceUnitNumbers(enabledDeviceUnitNumbers);
                        _ = _logger.LogDetailAsync(thisProc + "There are " + _UnitNumbers_ValidList.Count.ToString() +
                                                  " Enabled Units.");
                        if (_UnitNumbers_ValidList.Count > 0)
                        {
                            _CallersContext_Init = SynchronizationContext.Current;
                            _CallersObject_Init = callersObject;
                            _CallersDelegateHandler_Init = callersDelegateHandler;
                            _Notifications_DelegateHandler = new SendOrPostCallback(NotificationInContextofCaller);
                            Set_Init_PercentageComplete(10);
                            _ = _logger.LogDetailAsync(thisProc + "10%");
                            _currentHanelDeviceStatusList.Clear();
                            for (int index = 0; index < _UnitNumbers_ValidList.Count; ++index)
                            {
                                _ = _logger.LogDetailAsync(thisProc + "Enabled Unit: " +
                                                          _UnitNumbers_ValidList[index].ToString());
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
                                    _ = _logger.LogDetailAsync(thisProc +
                                                              "Opening a channel to the Hanel MP12D machine controller.");

                                    openSuccess = _MyMp12DController.OpenChannel(commPort, baudRate, dataBits, parity,
                                        stopBits, ref cError1, machineSimulationMode, logLevel,
                                        _currentHanelDeviceStatusList, _LogPath);
                                }

                                _ = _logger.LogDetailAsync($"Open Success: {openSuccess}");
                                //if (this._MyMp12NController != null)
                                //{
                                // _ = _logger.LogDetailAsync(ThisProc + "Opening a channel to the Hanel MP12N machine controller.");
                                //    Open_Success = this._MyMp12NController.OpenChannel(Host_IP, Host_Port, Machine_SimulationMode, LogLevel, this._LogPath, ref cError1);
                                //}
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
                                    LastStatus_Message = "Unable to connect with the serial port. \n" + cError1;
                                    _ = _logger.LogDetailAsync(thisProc + "Error Message: " + LastStatus_Message);
                                }

                                Set_Init_PercentageComplete(100);
                                _ = _logger.LogDetailAsync(thisProc + "End of task.");
                            }));
                        }
                        else
                        {
                            cError = thisProc + "No device unit numbers have been defined.";
                            _ = _logger.LogDetailAsync(cError);
                            Set_Init_PercentageComplete(100);
                            flag = false;
                        }
                    }
                    else
                    {
                        // cError = ThisProc + "Incorrect version of Hart_LV.DLL. Version " + this._MyDeviceController.Get_Required_LV_Version() + " of " + this._ControllerType + " is required.";
                        _ = _logger.LogDetailAsync(cError);
                        Set_Init_PercentageComplete(100);
                        flag = false;
                    }
                }
                else
                    _ = _logger.LogDetailAsync(thisProc + "Existing request in progress. Let it run...");
            }
            else
            {
                _ = _logger.LogDetailAsync(thisProc + "Request already completed. Let the caller know.");
                Set_Init_PercentageComplete(100);
            }

            return flag;
        }

        public int Get_Init_PercentageComplete() => _Init_PercentageComplete;

        private void Set_Init_PercentageComplete(int value)
        {
            _ = _logger.LogDetailAsync($"Percentage Value: {value}");
            if (value == 100 && Init_Success && _Init_PercentageComplete != 100)
                //_Machine_ConversationStart_Async();
                _Hanel_ConversationStart_Async();
            if (value <= _Init_PercentageComplete)
                return;
            _ = _logger.LogDetailAsync(ObjectID + " Init_PercentageComplete(): New Value: " + value.ToString() +
                                      " Current Value: " + _Init_PercentageComplete.ToString());
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

        public bool EnqueueHanelCommand(IHanelCommand hanelCommand)
        {
            _hanelCommandQueue.Enqueue(hanelCommand);
            return true;
        }

        public bool GetTray(IHanelCommand hanelCommand)
        {
            _hanelCommandQueue.Enqueue(hanelCommand);
            return true;
        }
        public bool Drive_Device(IHanelCommand hanelCommand)
        {
            _hanelCommandQueue.Enqueue(hanelCommand);
            return true;
        }

        public bool Drive_Device(int DeviceUnit, int Tray, ref string cError) =>
            Drive_Device(DeviceUnit, Tray, 0, 0, 0, "", ref cError);

        public bool Drive_Device(
            int DeviceUnit,
            int Tray,
            int Facing,
            int Depth,
            int Quantity,
            string DisplayText,
            ref string cError)
        {
            //DisplayText = "$X01001PRESS RETURN$X03010DO IT NOW$";

            string str = ObjectID + " Drive_Device(): ";
            bool flag = false;
            int num = 0;
            _ = _logger.LogDetailAsync(str + "Device " + DeviceUnit.ToString() + " to Tray " + Tray.ToString() +
                                      " Facing " + Facing.ToString() + " Depth " + Depth.ToString() + " Quantity " +
                                      Quantity.ToString());
            while (Init_Success)
            {
                _ = _logger.LogDetailAsync(str + "Controller has been initialized");
                if (!_UnitNumbers_ValidList.Contains(DeviceUnit))
                {
                    cError = DeviceUnit.ToString() + " is an invalid device unit number.";
                    goto label_21;
                }
                else
                {
                    int index = _UnitNumbers_ValidList.IndexOf(DeviceUnit);
                    _ = _logger.LogDetailAsync(str + "Valid device unit number. List entry is " + index.ToString());
                    if (Tray < 0)
                    {
                        cError = Tray.ToString() + " is an invalid tray number. Must be greater than zero.";
                        goto label_21;
                    }
                    else
                    {
                        _ = _logger.LogDetailAsync(str + "Valid Tray");
                        if (Facing < 0 || Facing > 99)
                        {
                            cError = Facing.ToString() + " is an invalid facing. Must be in the range of zero to 99.";
                            goto label_21;
                        }
                        else
                        {
                            _ = _logger.LogDetailAsync(str + "Valid Facing");
                            if (Depth < 0 || Depth > 99)
                            {
                                cError = Depth.ToString() + " is an invalid depth. Must be in the range of zero to 99.";
                                goto label_21;
                            }
                            else
                            {
                                _ = _logger.LogDetailAsync(str + "Valid Depth");
                                if (Quantity < 0 || Quantity > 9999)
                                {
                                    cError = Quantity.ToString() +
                                             " is an invalid quantity. Must be in the range of zero to 9999.";
                                    goto label_21;
                                }
                                else
                                {
                                    _ = _logger.LogDetailAsync(str + "Valid Qty");
                                    if (!_Machine_SimulationMode)
                                    {
                                        if (!_currentHanelDeviceStatusList[index].GoodStatus)
                                        {
                                            ++num;
                                            cError = "Device " + DeviceUnit.ToString() + " appears to be off-line. " +
                                                     num.ToString() + " attempt(s). Request rejected.";
                                            _ = _logger.LogDetailAsync(str + cError);
                                            // What does 4 represent? 
                                            if (num <= 4)
                                            {
                                                Thread.Sleep(nThreadSleepMilliseconds);
                                                continue;
                                            }

                                            goto label_21;
                                        }
                                        else
                                        {
                                            _ = _logger.LogDetailAsync(str + "Device appears to be on-line.");
                                            if (_currentHanelDeviceStatusList[index].InMotion)
                                            {
                                                cError = "Device " + DeviceUnit.ToString() +
                                                         " is in motion. Request rejected.";
                                                goto label_21;
                                            }
                                            else
                                                _ = _logger.LogDetailAsync(str + "Not in motion");
                                        }

                                        _logger.LogDetailAsync($"Calling Enqueue: Tray  {Tray}");
                                    }

                                    var hanelCommand =
                                        _hanelCommandService.MoveDeviceCommand(DeviceUnit, 1, Tray, Facing, Depth);

                                    _hanelCommandQueue.Enqueue(hanelCommand);


                                    //_Machine_CallersRequestsQueue.Enqueue(new MachineRequestType()
                                    //{
                                    //    DeviceUnit = DeviceUnit,
                                    //    Tray = Tray,
                                    //    Facing = Facing,
                                    //    Depth = Depth,
                                    //    Quantity = Quantity,
                                    //    Text = DisplayText
                                    //});

                                    //_Machine_CallersRequestsQueue.Enqueue(new MachineRequestType()
                                    //{
                                    //    DeviceUnit = DeviceUnit,
                                    //    Tray = Tray,
                                    //    Facing = Facing,
                                    //    Depth = Depth,
                                    //    Quantity = Quantity,
                                    //    Text = DisplayText
                                    //});
                                    _ = _logger.LogDetailAsync(str + "Drive Request Submitted.");
                                    cError = "";
                                    flag = true;
                                    goto label_21;
                                }
                            }
                        }
                    }
                }
            }

            cError = "Controller not initialized. Unable to drive device.";
        label_21:
            if (!flag)
                _ = _logger.LogDetailAsync(str + cError);
            return flag;
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
            string str = ObjectID + " Notification_Register(): ";
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
                _ = _logger.LogDetailAsync(str + cError);
            return Unique_ID;
        }

        public virtual bool Notification_DeRegister(Guid NotificationHandle, ref string cError)
        {
            string str = ObjectID + " Notification_DeRegister(): ";
            bool flag = false;
            if (Init_Success)
            {
                cError = "Notification handle not found. No notifications were deregistered.";
                for (int index = _Notifications_CallersRequestsList.Count - 1; index >= 0; --index)
                {
                    if (_Notifications_CallersRequestsList[index].Unique_ID == NotificationHandle)
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
                _ = _logger.LogDetailAsync(str + cError);
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
            var str = ObjectID + " Get_Device_Status()";
            _logger.LogDetailAsync($"{str}");
            var hanelDeviceStatusList = new List<HanelDeviceStatus>();
            if (!Init_Success) return hanelDeviceStatusList;
            return _currentHanelDeviceStatusList ?? hanelDeviceStatusList;
        }

        public bool Get_Device_Status(ref List<HanelDeviceStatus> newList, ref string cError)
        {
            string str = ObjectID + " Get_Device_Status(): ";
            bool deviceStatus = false;
            if (Init_Success)
            {
                if (_currentHanelDeviceStatusList.Count > 0)
                {
                    newList = _currentHanelDeviceStatusList.ConvertAll<HanelDeviceStatus>(
                        (Converter<HanelDeviceStatus, HanelDeviceStatus>)(abc => new HanelDeviceStatus(abc.DeviceNumber,
                            abc.GoodStatus, abc.LastStatus, abc.LastCommand, abc.Device, abc.TargetTray,
                            abc.CurrentTray, abc.InMotion, abc.InAlignment, abc.StatusMessage, abc.CommandAccepted,
                            abc.CommandExecuted)));
                    deviceStatus = true;
                    cError = "";
                }
                else
                {
                    cError = "No device status available.";
                    _ = _logger.LogDetailAsync(str + cError);
                }
            }
            else
            {
                cError = "Controller not initialized. No device status available.";
                _ = _logger.LogDetailAsync(str + cError);
            }

            return deviceStatus;
        }

        public bool Show_Text(
            int RequestedDeviceNumber,
            string TextRow_1,
            string TextRow_2,
            string TextRow_3,
            string TextRow_4,
            ref string cError)
        {
            string str1 = ObjectID + " Show_Text(): ";
            bool flag = false;
            int num = 20;
            //if (_MyMp12NController != null)
            if (_MyMp12DController != null)
            {
                if (Init_Success)
                {
                    if (_UnitNumbers_ValidList.Contains(RequestedDeviceNumber))
                    {
                        if ((TextRow_1 + TextRow_2 + TextRow_3 + TextRow_4).Trim().Length > 0)
                        {
                            string str2 = (TextRow_1 + new string(' ', num)).Substring(0, num) +
                                          (TextRow_2 + new string(' ', num)).Substring(0, num) +
                                          (TextRow_3 + new string(' ', num)).Substring(0, num) +
                                          (TextRow_4 + new string(' ', num)).Substring(0, num);
                            if (_currentHanelDeviceStatusList[RequestedDeviceNumber].GoodStatus)
                            {
                                _Machine_CallersRequestsQueue.Enqueue(new MachineRequestType()
                                {
                                    DeviceUnit = RequestedDeviceNumber,
                                    TextAction = 1,
                                    Text = str2
                                });
                                cError = "";
                                flag = true;
                            }
                            else
                                cError = "Device off-line. Request rejected.";
                        }
                        else
                            cError = "Invalid text. May not send blanks in all text parameters.";
                    }
                    else
                        cError = "Invalid device unit number specified.";
                }
                else
                    cError = "Controller not initialized. Unable to drive device.";
            }
            else
            {
                cError = "";
                flag = true;
            }

            if (!flag)
                _ = _logger.LogDetailAsync(str1 + cError);
            return flag;
        }

        public bool Clear_Text(int RequestedDeviceNumber, ref string cError)
        {
            string str = ObjectID + " Clear_Text(): ";
            bool flag = false;
            _ = _logger.LogDetailAsync(str + "Beg");
            // if (_MyMp12NController != null)
            if (_MyMp12DController != null)
            {
                if (Init_Success)
                {
                    if (_UnitNumbers_ValidList.Contains(RequestedDeviceNumber))
                    {
                        if (_currentHanelDeviceStatusList[_UnitNumbers_ValidList.IndexOf(RequestedDeviceNumber)]
                            .GoodStatus)
                        {
                            _Machine_CallersRequestsQueue.Enqueue(new MachineRequestType()
                            {
                                DeviceUnit = RequestedDeviceNumber,
                                TextAction = -1
                            });
                            cError = "";
                            flag = true;
                        }
                        else
                            cError = "Device in motion. Request rejected.";
                    }
                    else
                        cError = "Invalid device unit number specified.";
                }
                else
                    cError = "Controller not initialized. Unable to drive device.";
            }
            else
            {
                cError = "";
                flag = true;
            }

            if (!flag)
                _ = _logger.LogDetailAsync(str + cError);
            _ = _logger.LogDetailAsync(str + "End");
            return flag;
        }

        public bool Close_Controller(ref string cError)
        {
            string str = ObjectID + " Close_Controller(): ";
            bool flag = false;
            _ = _logger.LogDetailAsync(str + "Beg");
            if (Init_Success)
            {
                Close_Device_Controller();
                flag = true;
                cError = "";
            }
            else
            {
                cError = "Controller not initialized. Cannot close.";
                _ = _logger.LogDetailAsync(str + cError);
            }

            _ = _logger.LogDetailAsync(str + "End");
            return flag;
        }

        private void Close_Device_Controller()
        {
            string str = ObjectID + " Close_Device_Controller(): ";
            _ = _logger.LogDetailAsync(str + "Beg");
            Init_Success = false;
            _Machine_HaltConversation = true;
            _UnitNumbers_ValidList.Clear();
            _currentHanelDeviceStatusList.Clear();
            _Notifications_CallersRequestsList.Clear();
            Set_Init_PercentageComplete(0);
            _MyDeviceController.CloseChannel();
            _Machine_CallersRequestsQueue.Clear();
            _hanelCommandQueue.Clear();
            _ = _logger.LogDetailAsync(str + "End");
        }

        private void _Hanel_ConversationStart_Async(bool blockThisCall = false)
        {
            var thisProc = ObjectID + " _Hanel_ConversationStart_Async(): ";
            _logger.LogDetailAsync(thisProc + "Starting conversation.");
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
                    _ = _logger.LogDetailAsync(thisProc + "There is a request in the queue, remove it.");
                    // Get the next request from the queue

                    var hanelCommand = _hanelCommandQueue.Dequeue();

                    _ = _logger.LogDetailAsync($"Hanel Command: {hanelCommand.Command}");

                    var status = _currentHanelDeviceStatusList.FirstOrDefault(r => r.Device == hanelCommand.Device);
                    
                    if (status != null)
                    {
                        status.LastCommand = DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(hanelCommand.Tray))
                        {
                            status.TargetTray = int.Parse(hanelCommand.Tray);
                        }

                        status.CommandAccepted = false;
                        status.StatusMessage = "Displaying Text!!!";


                        string[] strArray = new string[5]
                        {
                            thisProc,
                            "Calling _MyMp12DController.DriveDevice ",
                            null,
                            null,
                            null
                        };
                        strArray[2] = hanelCommand.Lift;
                        strArray[3] = " and Tray ";
                        strArray[4] = hanelCommand.Tray;
                        var lineToWrite12 = string.Concat(strArray);

                        _ = _logger.LogDetailAsync(lineToWrite12);

                        if (!_MyMp12DController.DriveDevice(hanelCommand, ref cError2))
                        {
                            _ = _logger.LogDetailAsync(thisProc + "Machine Drive error Message: " + cError2);
                            str1 = "Machine error message: " + cError2;
                        }

                        status.GoodStatus = true;
                        status.LastStatus = DateTime.Now;
                        status.CurrentTray = status.TargetTray;
                        status.InMotion = false;
                        status.InAlignment = true;
                        status.StatusMessage = "";
                        status.CommandAccepted = false;
                        status.CommandExecuted = false;
                    }



                    //_currentHanelDeviceStatusList[index1].GoodStatus = true;
                    //_currentHanelDeviceStatusList[index1].LastStatus = DateTime.Now;
                    //_currentHanelDeviceStatusList[index1].CurrentTray =
                    //    _currentHanelDeviceStatusList[index1].TargetTray;
                    //_currentHanelDeviceStatusList[index1].InMotion = false;
                    //_currentHanelDeviceStatusList[index1].InAlignment = true;
                    //_currentHanelDeviceStatusList[index1].StatusMessage = "";
                    //_currentHanelDeviceStatusList[index1].CommandAccepted = false;
                    //_currentHanelDeviceStatusList[index1].CommandExecuted = false;

                    //    strArray = new string[6]
                    //    {
                    //        thisProc,
                    //        "DriveDevice for Unit ",
                    //        null,
                    //        null,
                    //        null,
                    //        null
                    //    };
                    //    strArray[2] = hanelCommand.Lift;
                    //    strArray[3] = " and Tray ";
                    //    strArray[4] = hanelCommand.Tray;
                    //    strArray[5] = " but in Simulation Mode or Suspended.";
                    //    var lineToWrite13 = string.Concat(strArray);

                    //    _ = _logger.LogDetailAsync($"13-{lineToWrite13}");
                    //}

                    //_logger.LogDetailAsync(thisProc + "Go back to the top of the while (true) in case there is another request in the queue...");
                    Thread.Sleep(nThreadSleepMilliseconds);
                    continue;
                }
                //_logger.LogDetailAsync(thisProc + "Ignore the request in the queue and fall through to obtaining status, it has been awhile...");


                //num1 = 0;
                //     _ = _logger.LogDetailAsync(thisProc + "Request status from the controller for each device & update the controller status collection.");
                //foreach (HanelDeviceStatus currentHanelDeviceStatus in _currentHanelDeviceStatusList)
                //{
                //    //IDynamicLogger logger14 = Logger;
                //    string str28 = thisProc;
                //    num2 = currentHanelDeviceStatus.Device;
                //    string str29 = num2.ToString();
                //    var lineToWrite14 = str28 + $"14-Calling Machine Controller GetDeviceStatus() for Unit {str29}";
                //    //logger14.Log(LineToWrite14);
                //    //         _ = _logger.LogDetailAsync(lineToWrite14);
                //    var deviceStatus = _MyDeviceController.GetDeviceStatus(currentHanelDeviceStatus.Device, ref nTray, ref nInMotion, ref nInAlignment, ref cError2);
                //    if (!_Machine_SimulationMode)
                //    {
                //        if (deviceStatus)
                //        {
                //            //IDynamicLogger logger15 = Logger;
                //            var strArray = new string[9];
                //            strArray[0] = thisProc;
                //            strArray[1] = "Unit ";
                //            num2 = currentHanelDeviceStatus.Device;
                //            strArray[2] = num2.ToString();
                //            strArray[3] = " Tray:";
                //            strArray[4] = nTray.ToString();
                //            strArray[5] = " Motion:";
                //            strArray[6] = nInMotion.ToString();
                //            strArray[7] = " Alignment:";
                //            strArray[8] = nInAlignment.ToString();
                //            var lineToWrite15 = string.Concat(strArray);
                //            //logger15.Log(LineToWrite15);
                //         _ = _logger.LogDetailAsync($"15-{lineToWrite15}");

                //            currentHanelDeviceStatus.GoodStatus = true;
                //            currentHanelDeviceStatus.LastStatus = DateTime.Now;
                //            currentHanelDeviceStatus.CurrentTray = nTray;
                //            currentHanelDeviceStatus.InMotion = nInMotion;
                //            currentHanelDeviceStatus.InAlignment = nInAlignment;
                //            currentHanelDeviceStatus.StatusMessage = "";
                //        }
                //        else
                //        {
                //            //IDynamicLogger logger16 = Logger;
                //            string[] strArray = new string[7];
                //            strArray[0] = thisProc;
                //            strArray[1] = "Machine Controller GetDeviceStatus() Device Unit ";
                //            num2 = currentHanelDeviceStatus.Device;
                //            strArray[2] = num2.ToString();
                //            strArray[3] = " Count: ";
                //            num2 = currentHanelDeviceStatus.ActiveErrorCount;
                //            strArray[4] = num2.ToString();
                //            strArray[5] = " Error: ";
                //            strArray[6] = cError2;
                //            string LineToWrite16 = string.Concat(strArray);
                //            //logger16.Log(LineToWrite16);
                //            //_logger.LogDetailAsync(LineToWrite16);
                //            if (currentHanelDeviceStatus.ActiveErrorCount < MaximumStatusErrors)
                //            {
                //                num2 = currentHanelDeviceStatus.ActiveErrorCount++;
                //            }
                //            else
                //            {
                //                currentHanelDeviceStatus.GoodStatus = false;
                //                currentHanelDeviceStatus.StatusMessage = cError2;
                //                currentHanelDeviceStatus.ActiveErrorCount = 0;
                //            }
                //        }
                //    }
                //    for (int index2 = _Notifications_CallersRequestsList.Count - 1; index2 >= 0; --index2)
                //    {
                //        bool flag2 = false;
                //        HanelDeviceNotificationType notificationsCallersRequests = _Notifications_CallersRequestsList[index2];
                //        if (notificationsCallersRequests.TimeOutSeconds > 0 && DateTime.Compare(DateTime.Now, notificationsCallersRequests.Expiry) > 0)
                //            notificationsCallersRequests.Expired = true;
                //        else if (currentHanelDeviceStatus.GoodStatus && currentHanelDeviceStatus.CurrentTray == notificationsCallersRequests.TargetTray)
                //        {
                //            if (notificationsCallersRequests.RequestedMotionStatus == 0)
                //                flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                //            else if (notificationsCallersRequests.RequestedMotionStatus < 0 && !currentHanelDeviceStatus.InMotion)
                //                flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                //            else if (notificationsCallersRequests.RequestedMotionStatus > 0 && currentHanelDeviceStatus.InMotion)
                //                flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                //        }
                //        if (flag2)
                //        {
                //            notificationsCallersRequests.AlignmentStatusUponNotification = _currentHanelDeviceStatusList[index1].InAlignment;
                //            notificationsCallersRequests.MotionStatusUponNotification = _currentHanelDeviceStatusList[index1].InMotion;
                //            notificationsCallersRequests.Message = str1;
                //            str1 = "";
                //        }
                //        if (flag2 || notificationsCallersRequests.Expired)
                //        {
                //            if (notificationsCallersRequests.AutoDeregister)
                //                _Notifications_CallersRequestsList.RemoveAt(index2);
                //            if (notificationsCallersRequests.CallBack != null && notificationsCallersRequests.CallersObject != null)
                //            {
                //                _Notification_Current = notificationsCallersRequests;
                //                notificationsCallersRequests.CallersContext.Post(_Notifications_DelegateHandler, notificationsCallersRequests.CallersObject);
                //            }
                //        }
                //    }
                //}
                Thread.Sleep(nThreadSleepMilliseconds);
            }
            //});
            _ = _logger.LogDetailAsync(thisProc + "Someone or something has requested this process to shut down.");
        }
        private void _Machine_ConversationStart_Async(bool blockThisCall = false)
        {
            var thisProc = ObjectID + " _Machine_ConversationStart_Async(): ";
            _ = _logger.LogDetailAsync($"{thisProc}");
            Task.Run((Action)(() =>
            {
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
                    if (_Machine_CallersRequestsQueue.Count > 0)
                    {
                        _ = _logger.LogDetailAsync(thisProc + "There is a request in the queue, remove it.");
                        // Get the next request from the queue

                        var ready = IsReadyToDequeue(_Machine_CallersRequestsQueue.Peek());

                        MachineRequestType machineRequestType = _Machine_CallersRequestsQueue.Dequeue();
                        // num1 is initially set to 0,  so the first time thru this will be true
                        if (num1 < _UnitNumbers_ValidList.Count)
                        {
                            // Add 1 to num1
                            ++num1;
                            //_logger.LogDetailAsync(thisProc + "More chances...");
                            // Make sure the DeviceUnit in the request is in the Valid Unit Numbers list
                            if (!_UnitNumbers_ValidList.Contains(machineRequestType.DeviceUnit))
                            {
                                // NOT in list, invalid request, log it and continue
                                //_logger.LogDetailAsync(thisProc + "Ignore (or Log) invalid requests (1). Continue...");
                                continue;
                            }
                            //_logger.LogDetailAsync(thisProc + "At this point, you need to determine if you are dealing with a drive command or a display command.");
                            // TextActionShow is const of 1, so set TextAction = 1 to show text or 0 NOT to show text
                            // Another option is to check the Text field for content.
                            // If the Text field is NOT null or empty, this is a display command
                            //if (machineRequestType.TextAction == machineRequestType.TextActionShow)
                            if (!string.IsNullOrWhiteSpace(machineRequestType.Text))
                            {
                                string str2 = thisProc;
                                num2 = machineRequestType.DeviceUnit;
                                string str3 = num2.ToString();
                                string LineToWrite = str2 + "Calling Machine Controller ShowText() for Unit " + str3;
                                _ = _logger.LogDetailAsync(LineToWrite);
                                _MyMp12DController.ShowText(machineRequestType.DeviceUnit, machineRequestType.Text, ref cError2);
                                //_MyMp12NController.ShowText(machineRequestType.DeviceUnit, machineRequestType.Text, ref cError2);
                                //_logger.LogDetailAsync(thisProc + "Continue...");
                                continue;
                            }
                            //_logger.LogDetailAsync(thisProc + "Not ShowText");
                            if (machineRequestType.TextAction == machineRequestType.TextActionClear)
                            {
                                // IDynamicLogger logger = Logger;
                                string str4 = thisProc;
                                num2 = machineRequestType.DeviceUnit;
                                string str5 = num2.ToString();
                                string LineToWrite = str4 + "Calling Machine Controller ClearText() for Unit " + str5;
                                _ = _logger.LogDetailAsync(LineToWrite);
                                _MyMp12DController.ClearText(machineRequestType.DeviceUnit, ref cError2);
                                //_MyMp12NController.ClearText(machineRequestType.DeviceUnit, ref cError2);
                                //_logger.LogDetailAsync(thisProc + "Continue...");
                                continue;
                            }
                            //_logger.LogDetailAsync(thisProc + "Not ClearText");
                            //IDynamicLogger logger1 = Logger;
                            string str6 = thisProc;
                            num2 = machineRequestType.DeviceUnit;
                            string str7 = num2.ToString();
                            string LineToWrite1 = str6 + "Should be a drive command for unit " + str7 + ". Have to validate first.";
                            // logger1.Log(LineToWrite1);
                            _ = _logger.LogDetailAsync(LineToWrite1);
                            if (!_UnitNumbers_ValidList.Contains(machineRequestType.DeviceUnit))
                            {
                                //IDynamicLogger logger2 = Logger;
                                string str8 = thisProc;
                                num2 = machineRequestType.DeviceUnit;
                                string str9 = num2.ToString();
                                string LineToWrite2 = str8 + "Invalid device unit number: " + str9 + ", continue...";
                                //logger2.Log(LineToWrite2);
                                _ = _logger.LogDetailAsync(LineToWrite2);
                                continue;
                            }
                            // IDynamicLogger logger3 = Logger;
                            string str10 = thisProc;
                            num2 = machineRequestType.DeviceUnit;
                            string str11 = num2.ToString();
                            string LineToWrite3 = str10 + "Valid device unit " + str11;
                            //logger3.Log(LineToWrite3);
                            _ = _logger.LogDetailAsync(LineToWrite3);
                            if (machineRequestType.Tray < 0)
                            {
                                //IDynamicLogger logger4 = Logger;
                                string str12 = thisProc;
                                num2 = machineRequestType.Tray;
                                string str13 = num2.ToString();
                                string LineToWrite4 = str12 + "Invalid tray: " + str13 + ", continue...";
                                // logger4.Log(LineToWrite4);
                                _ = _logger.LogDetailAsync(LineToWrite4);
                                continue;
                            }
                            //  IDynamicLogger logger5 = Logger;
                            string str14 = thisProc;
                            num2 = machineRequestType.Tray;
                            string str15 = num2.ToString();
                            string LineToWrite5 = str14 + "Valid tray " + str15;
                            //logger5.Log(LineToWrite5);
                            _ = _logger.LogDetailAsync(LineToWrite5);

                            //if (_MyMp12NController != null)
                            if (_MyMp12DController != null)
                            {
                                if (machineRequestType.Facing < 0 || machineRequestType.Facing > 99)
                                {
                                    //IDynamicLogger logger6 = Logger;
                                    string str16 = thisProc;
                                    num2 = machineRequestType.Facing;
                                    string str17 = num2.ToString();
                                    string LineToWrite6 = str16 + "Invalid facing: " + str17 + ", continue...";
                                    //logger6.Log(LineToWrite6);
                                    _ = _logger.LogDetailAsync(LineToWrite6);
                                    continue;
                                }
                                //IDynamicLogger logger7 = Logger;
                                string str18 = thisProc;
                                num2 = machineRequestType.Facing;
                                string str19 = num2.ToString();
                                string LineToWrite7 = str18 + "Valid facing " + str19;
                                //logger7.Log(LineToWrite7);
                                _ = _logger.LogDetailAsync(LineToWrite7);
                                if (machineRequestType.Depth < 0 || machineRequestType.Depth > 99)
                                {
                                    //IDynamicLogger logger8 = Logger;
                                    string str20 = thisProc;
                                    num2 = machineRequestType.Depth;
                                    string str21 = num2.ToString();
                                    string LineToWrite8 = str20 + "Invalid depth: " + str21 + ", continue...";
                                    //logger8.Log(LineToWrite8);
                                    _ = _logger.LogDetailAsync(LineToWrite8);
                                    continue;
                                }
                                //IDynamicLogger logger9 = Logger;
                                string str22 = thisProc;
                                num2 = machineRequestType.Depth;
                                string str23 = num2.ToString();
                                string LineToWrite9 = str22 + "Valid depth " + str23;
                                //logger9.Log(LineToWrite9);
                                _ = _logger.LogDetailAsync(LineToWrite9);
                                if (machineRequestType.Quantity < 0 || machineRequestType.Quantity > 9999)
                                {
                                    //IDynamicLogger logger10 = Logger;
                                    string str24 = thisProc;
                                    num2 = machineRequestType.Quantity;
                                    string str25 = num2.ToString();
                                    string LineToWrite10 = str24 + "Invalid quantity: " + str25 + ", continue...";
                                    //logger10.Log(LineToWrite10);
                                    _ = _logger.LogDetailAsync(LineToWrite10);
                                    continue;
                                }
                                //IDynamicLogger logger11 = Logger;
                                string str26 = thisProc;
                                num2 = machineRequestType.Quantity;
                                string str27 = num2.ToString();
                                string LineToWrite11 = str26 + "Valid quantity " + str27;
                                //logger11.Log(LineToWrite11);
                                _ = _logger.LogDetailAsync(LineToWrite11);
                                if (machineRequestType.Text.Trim().Length > 80)
                                {
                                    _ = _logger.LogDetailAsync(thisProc + "Text is too long: " + machineRequestType.Text + ", continue...");
                                    continue;
                                }
                                //_logger.LogDetailAsync(thisProc + "Valid text " + machineRequestType.Text);
                            }
                            _ = _logger.LogDetailAsync(thisProc + "This is a drive command.");
                            index1 = _UnitNumbers_ValidList.IndexOf(machineRequestType.DeviceUnit);
                            _currentHanelDeviceStatusList[index1].LastCommand = DateTime.Now;
                            _currentHanelDeviceStatusList[index1].TargetTray = machineRequestType.Tray;
                            _currentHanelDeviceStatusList[index1].CommandAccepted = false;
                            _currentHanelDeviceStatusList[index1].StatusMessage = "Holy Shit!!!";
                            if (!flag1)
                            {
                                //IDynamicLogger logger12 = Logger;
                                string[] strArray = new string[5]
                                {
                  thisProc,
                  "Calling Machine Controller DriveDevice() for Unit ",
                  null,
                  null,
                  null
                                };
                                num2 = machineRequestType.DeviceUnit;
                                strArray[2] = num2.ToString();
                                strArray[3] = " and Tray ";
                                num2 = machineRequestType.Tray;
                                strArray[4] = num2.ToString();
                                string LineToWrite12 = string.Concat(strArray);
                                //logger12.Log(LineToWrite12);
                                _ = _logger.LogDetailAsync(LineToWrite12);
                                //if (!(_MyMp12NController == null ? _MyDeviceController.DriveDevice(
                                //            machineRequestType.DeviceUnit
                                //            , machineRequestType.Tray
                                //            , ref cError2)
                                //        : _MyMp12NController.DriveDevice(
                                //            machineRequestType.DeviceUnit
                                //            , machineRequestType.Tray
                                //            , machineRequestType.Facing
                                //            , machineRequestType.Depth
                                //            , machineRequestType.Quantity
                                //            , machineRequestType.Text
                                //            , ref cError2)))
                                //{
                                // _ = _logger.LogDetailAsync(thisProc + "Machine Drive error Message: " + cError2);
                                //    str1 = "Machine error message: " + cError2;
                                //}
                                if (!(_MyMp12DController == null ? _MyDeviceController.DriveDevice(
                                            machineRequestType.DeviceUnit
                                            , machineRequestType.Tray
                                            , ref cError2)
                                        : _MyMp12DController.DriveDevice(
                                            machineRequestType.DeviceUnit
                                            , machineRequestType.Tray
                                            , machineRequestType.Facing
                                            , machineRequestType.Depth
                                            , machineRequestType.Quantity
                                            , machineRequestType.Text
                                            , ref cError2)))
                                {
                                    _ = _logger.LogDetailAsync(thisProc + "Machine Drive error Message: " + cError2);
                                    str1 = "Machine error message: " + cError2;
                                }
                            }
                            if (flag1 || _Machine_SimulationMode)
                            {
                                _currentHanelDeviceStatusList[index1].GoodStatus = true;
                                _currentHanelDeviceStatusList[index1].LastStatus = DateTime.Now;
                                _currentHanelDeviceStatusList[index1].CurrentTray = _currentHanelDeviceStatusList[index1].TargetTray;
                                _currentHanelDeviceStatusList[index1].InMotion = false;
                                _currentHanelDeviceStatusList[index1].InAlignment = true;
                                _currentHanelDeviceStatusList[index1].StatusMessage = "";
                                _currentHanelDeviceStatusList[index1].CommandAccepted = false;
                                _currentHanelDeviceStatusList[index1].CommandExecuted = false;
                                //IDynamicLogger logger13 = Logger;
                                string[] strArray = new string[6]
                                {
                                      thisProc,
                                      "DriveDevice for Unit ",
                                      null,
                                      null,
                                      null,
                                      null
                                };
                                num2 = machineRequestType.DeviceUnit;
                                strArray[2] = num2.ToString();
                                strArray[3] = " and Tray ";
                                num2 = machineRequestType.Tray;
                                strArray[4] = num2.ToString();
                                strArray[5] = " but in Simulation Mode or Suspended.";
                                var lineToWrite13 = string.Concat(strArray);
                                //logger13.Log(LineToWrite13);
                                _ = _logger.LogDetailAsync($"13-{lineToWrite13}");
                            }
                            //_logger.LogDetailAsync(thisProc + "Go back to the top of the while (true) in case there is another request in the queue...");
                            Thread.Sleep(nThreadSleepMilliseconds);
                            continue;
                        }
                        //_logger.LogDetailAsync(thisProc + "Ignore the request in the queue and fall through to obtaining status, it has been awhile...");
                    }
                    num1 = 0;
                    //     _ = _logger.LogDetailAsync(thisProc + "Request status from the controller for each device & update the controller status collection.");
                    foreach (HanelDeviceStatus currentHanelDeviceStatus in _currentHanelDeviceStatusList)
                    {
                        //IDynamicLogger logger14 = Logger;
                        string str28 = thisProc;
                        num2 = currentHanelDeviceStatus.Device;
                        string str29 = num2.ToString();
                        var lineToWrite14 = str28 + $"14-Calling Machine Controller GetDeviceStatus() for Unit {str29}";
                        //logger14.Log(LineToWrite14);
                        //         _ = _logger.LogDetailAsync(lineToWrite14);
                        var deviceStatus = _MyDeviceController.GetDeviceStatus(currentHanelDeviceStatus.Device, ref nTray, ref nInMotion, ref nInAlignment, ref cError2);
                        if (!_Machine_SimulationMode)
                        {
                            if (deviceStatus)
                            {
                                //IDynamicLogger logger15 = Logger;
                                var strArray = new string[9];
                                strArray[0] = thisProc;
                                strArray[1] = "Unit ";
                                num2 = currentHanelDeviceStatus.Device;
                                strArray[2] = num2.ToString();
                                strArray[3] = " Tray:";
                                strArray[4] = nTray.ToString();
                                strArray[5] = " Motion:";
                                strArray[6] = nInMotion.ToString();
                                strArray[7] = " Alignment:";
                                strArray[8] = nInAlignment.ToString();
                                var lineToWrite15 = string.Concat(strArray);
                                //logger15.Log(LineToWrite15);
                                _ = _logger.LogDetailAsync($"15-{lineToWrite15}");

                                currentHanelDeviceStatus.GoodStatus = true;
                                currentHanelDeviceStatus.LastStatus = DateTime.Now;
                                currentHanelDeviceStatus.CurrentTray = nTray;
                                currentHanelDeviceStatus.InMotion = nInMotion;
                                currentHanelDeviceStatus.InAlignment = nInAlignment;
                                currentHanelDeviceStatus.StatusMessage = "";
                            }
                            else
                            {
                                //IDynamicLogger logger16 = Logger;
                                string[] strArray = new string[7];
                                strArray[0] = thisProc;
                                strArray[1] = "Machine Controller GetDeviceStatus() Device Unit ";
                                num2 = currentHanelDeviceStatus.Device;
                                strArray[2] = num2.ToString();
                                strArray[3] = " Count: ";
                                num2 = currentHanelDeviceStatus.ActiveErrorCount;
                                strArray[4] = num2.ToString();
                                strArray[5] = " Error: ";
                                strArray[6] = cError2;
                                string LineToWrite16 = string.Concat(strArray);
                                //logger16.Log(LineToWrite16);
                                //_logger.LogDetailAsync(LineToWrite16);
                                if (currentHanelDeviceStatus.ActiveErrorCount < MaximumStatusErrors)
                                {
                                    num2 = currentHanelDeviceStatus.ActiveErrorCount++;
                                }
                                else
                                {
                                    currentHanelDeviceStatus.GoodStatus = false;
                                    currentHanelDeviceStatus.StatusMessage = cError2;
                                    currentHanelDeviceStatus.ActiveErrorCount = 0;
                                }
                            }
                        }
                        for (int index2 = _Notifications_CallersRequestsList.Count - 1; index2 >= 0; --index2)
                        {
                            bool flag2 = false;
                            HanelDeviceNotificationType notificationsCallersRequests = _Notifications_CallersRequestsList[index2];
                            if (notificationsCallersRequests.TimeOutSeconds > 0 && DateTime.Compare(DateTime.Now, notificationsCallersRequests.Expiry) > 0)
                                notificationsCallersRequests.Expired = true;
                            else if (currentHanelDeviceStatus.GoodStatus && currentHanelDeviceStatus.CurrentTray == notificationsCallersRequests.TargetTray)
                            {
                                if (notificationsCallersRequests.RequestedMotionStatus == 0)
                                    flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                                else if (notificationsCallersRequests.RequestedMotionStatus < 0 && !currentHanelDeviceStatus.InMotion)
                                    flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                                else if (notificationsCallersRequests.RequestedMotionStatus > 0 && currentHanelDeviceStatus.InMotion)
                                    flag2 = AlignmentQualificationsHaveBeenMet(notificationsCallersRequests.RequestedAlignmentStatus, currentHanelDeviceStatus.InAlignment);
                            }
                            if (flag2)
                            {
                                notificationsCallersRequests.AlignmentStatusUponNotification = _currentHanelDeviceStatusList[index1].InAlignment;
                                notificationsCallersRequests.MotionStatusUponNotification = _currentHanelDeviceStatusList[index1].InMotion;
                                notificationsCallersRequests.Message = str1;
                                str1 = "";
                            }
                            if (flag2 || notificationsCallersRequests.Expired)
                            {
                                if (notificationsCallersRequests.AutoDeregister)
                                    _Notifications_CallersRequestsList.RemoveAt(index2);
                                if (notificationsCallersRequests.CallBack != null && notificationsCallersRequests.CallersObject != null)
                                {
                                    _Notification_Current = notificationsCallersRequests;
                                    notificationsCallersRequests.CallersContext.Post(_Notifications_DelegateHandler, notificationsCallersRequests.CallersObject);
                                }
                            }
                        }
                    }
                    Thread.Sleep(nThreadSleepMilliseconds);
                }
                _ = _logger.LogDetailAsync(thisProc + "Someone or something has requested this process to shut down.");
            }));
        }

        private bool IsReadyToDequeue(MachineRequestType machineRequestType)
        {
            var result = false;
            var device = machineRequestType.DeviceUnit;
            var status = _currentHanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == device);
            if (status != null)
            {
                _ = _logger.LogDetailAsync($"Peek at Status: {status.CommandAccepted}");
                if (status.CommandAccepted)
                {
                    if (status.CommandExecuted)
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
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
    }
}
