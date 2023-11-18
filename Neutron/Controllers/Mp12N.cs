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

using NeutronCore;
using NeutronCore.Enums;
using Hanel_DC.Hanel_DeviceControllers;
using HanelCommands;

namespace Neutron.Controllers
{

    public class Mp12N : IHanelDriver
    {
        private readonly Hanel_DeviceController _hanel;
        readonly SendOrPostCallback CallBackHandler_Init;
        HanelTellMeWhenTrayArrives MyTrayArrivedNotificationDelegate = MyTrayArrived;
        Guid MyNotificationHandle;
        int NotificationTimeOutSeconds = 60;
        readonly bool NotificationAutoDeregister = true;
        string cError;
        private readonly int Device_Stationary = -1;
        private readonly int Device_Alignment_DontCare = 0;

        private readonly IDynamicLogger _logger;
        private readonly WorkstationView _workstation;
        Form _currentForm;
        private readonly Object _locker = new Object();
        private int[] _previousTray;

        public Mp12N(Form frm, WorkstationView workstation)
        {
            _previousTray = new int[10];
            _workstation = workstation;
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = string.Format(format: @"RCC2_Station_{0}", arg0: workstation.WorkstationId.ToString());
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
            _currentForm = frm;
            Task.Run(() => _logger.LogDetailAsync($"Mp12N Constructor - {frm.Name}"));
            CallBackHandler_Init = new SendOrPostCallback(MyInitProgressDelegate);
           //var hanelLog = ($"{logFileDir}Hanel");
            _hanel = new Hanel_DeviceController(Hanel_DeviceController.Controller_Type_Hanel_Mp12N(),_logger);
            if (_hanel != null)
            {
                Task.Run(() => _logger.LogDetailAsync(@"Shuttle has been created: "));
                RCC2Init();
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync("Shuttle has NOT been created:  Exiting "));
            }
        }

        public Form CurrentForm
        {
            private get { return _currentForm; }
            set
            {
                _currentForm = value;
                Task.Run(() => _logger.LogDetailAsync($"Changed Form - {_currentForm}"));
            }
        }

        private void RCC2Init()
        {
            if (_workstation != null)
            {
                var firstDevice = _workstation.HardwareDevices.FirstOrDefault();

                if (firstDevice != null)
                {
                    var serialConfiguration = firstDevice.SerialConfiguration;

                    if (serialConfiguration != null)
                    {

                        var deviceCount = _workstation.HardwareDevices.Count;
                        _previousTray = new int[deviceCount + 1];
                        NotificationTimeOutSeconds = serialConfiguration.NotificationTimeout;
                        var simulationMode = firstDevice.SimulationMode;
                        var logLevel = firstDevice.LogLevel;
                        var enabledUnitNumbers = _workstation.HardwareDevices.Where(r => r.Enabled == true).Select(s => s.DeviceNumber).ToList();

                        Task.Run(() => _logger.LogDetailAsync($"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()}Device Count: {serialConfiguration.DeviceCount}"));

                        if (_hanel.Init_Controller(serialConfiguration.ControllerId, serialConfiguration.PortNumber, serialConfiguration.BaudRate, serialConfiguration.DataBits,
                            serialConfiguration.Parity.ToString(), serialConfiguration.StopBits, simulationMode, logLevel, enabledUnitNumbers, this, CallBackHandler_Init, ref cError))
                        {
                            Task.Run(() => _logger.LogDetailAsync("Initialization Requested"));
                        }
                        else
                        {
                            Task.Run(() => _logger.LogDetailAsync("Problem requesting initialization. " + cError));
                        }
                    }
                    else
                    {
                        Task.Run(() => _logger.LogDetailAsync("SerialConfiguration is null "));
                    }
                }
                else
                {
                    Task.Run(() => _logger.LogDetailAsync($"Unknown Serial Configuration."));
                    MessageBox.Show($"Unknown Serial Configuration.");
                }
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync("Workstation is null or empty "));
            }
        }

        public int InitStatus()
        {
            // Note that the sequence of the following assignments is critical. Success must be first. Others follow in any sequence.
            var success = _hanel.Init_Success;
            var initCode = _hanel.LastStatus_Code;
            var initMsg = _hanel.LastStatus_Message;

            Task.Run(() => _logger.LogDetailAsync($"InitStatus: Success: {success} initCode: {initCode} initMsg: {initMsg}"));
            if (success)
            {
                // life is good, you can drive the device
                Task.Run(() => _logger.LogDetailAsync("InitStatus: success is true."));
                if (initCode == 0)
                {
                    // life is good, no warning messages
                    Task.Run(() => _logger.LogDetailAsync($"Initialization is complete and was successful initCode is {initCode}"));
                }
                else
                {
                    // You need to report the warning to the operator or to a log that is monitored frequently
                    Task.Run(() => _logger.LogDetailAsync($"Kardex Controller Warning - Initialization was successful but there is a warning." + Environment.NewLine +
                    "Please provide the following information to your IT support." + Environment.NewLine +
                    "Code is: " + initCode.ToString() + Environment.NewLine +
                    "Message is: " + initMsg));
                }
            }
            else
            {
                // Darn, cannot drive the device at this time
                if (_hanel.Get_Init_PercentageComplete() == 0)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Not Initialized.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else if (_hanel.Get_Init_PercentageComplete() < 100)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Initialization is in progress.  Init {_hanel.Get_Init_PercentageComplete().ToString()}% complete..."));
                    Task.Run(() => _logger.LogDetailAsync($"Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else
                {
                    Task.Run(() => _logger.LogDetailAsync($"Initialization was unsuccessful.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
            }
            return initCode;
        }

        public void ShowMessage(string msg)
        {
            //MessageBox.Show(msg);
        }

        /*
     This is a delegate method and it will be called when any init notifications fire. 
     All delegates passed to Init_Controller() must have this same signature.
     */
        public static void MyInitProgressDelegate(object formObject)
        {
            //// This method is running in the UI synchronization context.
            //// You just need to create a reference to your original object
            //var formAlias = (Mp12N) formObject;
            //// You may now refer to any components of your UI as below.
            //// formAlias.LabelNotify.Text = "Init " + formAlias._hanel.Init_PercentageComplete.ToString() + "% complete...";
            //formAlias.ShowMessage("Init " + formAlias._hanel.Get_Init_PercentageComplete().ToString() + "% complete...");
        }

        public static void MyTrayArrived(HanelDeviceNotificationType firedNotification)
        {
            // This method is running in the UI synchronization context but
            // you need to create a reference to your original form in order
            //// to reference any of its controls.
            //var formAlias = (Mp12N) firedNotification.CallersObject;
            //// You may now refer to any components of your UI as below.
            //// formAlias.LabelNotify.Text = "Drive Notification Received";
            //formAlias.ShowMessage("Drive Notification Received");
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

        public DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 0, int depth = 0, int quantity = 0, string display = "")
        {
            var deviceResponse = DeviceResponse.UnknownFailure;
            Task.Run(() => _logger.LogDetailAsync($"Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()}  Time: {DateTime.Now}  Thread: {Thread.CurrentThread.ManagedThreadId}"));
            var continueLoop = true;
            var loopCounter = 0;
            var device = _workstation.HardwareDevices.FirstOrDefault(r => r.DeviceNumber == deviceNumber);
            if (device != null)
            {
                if (device.Enabled)
                {
                    if (_hanel.Init_Success)
                    {
                        while (continueLoop)
                        {
                            var status = GetDeviceStatus(deviceNumber);

                            if (status.GoodStatus)
                            {
                                loopCounter = 0;
                                if (!status.InMotion)
                                {
                                    if (status.CurrentTray != trayNumber)
                                    {
                                        if (_previousTray[deviceNumber] != 0)
                                        {
                                            if (status.CurrentTray != _previousTray[deviceNumber])
                                            {
                                                Task.Run(() => _logger.LogDetailAsync($"Tray did NOT arrive."));
                                                Task.Run(() => _logger.LogDetailAsync($"Status.Current_Tray: {status.CurrentTray}  Tray Number: {trayNumber}"));
                                                Task.Run(() => _logger.LogDetailAsync($"PreviousTray: {_previousTray[deviceNumber]}"));
                                                deviceResponse = DeviceResponse.TrayDidNotArrive;
                                                _previousTray[deviceNumber] = 0;
                                                break;
                                            }
                                        }

                                        cError = "";
                                        if (_hanel.Drive_Device(deviceNumber, trayNumber, ref cError))
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"Drive tray {trayNumber.ToString()} on device {deviceNumber.ToString()} request submitted.  Facing:{facing.ToString()}  Depth:{depth.ToString()}  Quantity:{quantity.ToString()}"));
                                            continueLoop = false;
                                            deviceResponse = DeviceResponse.Success;
                                            _previousTray[deviceNumber] = trayNumber;
                                            Task.Run(() => _logger.LogDetailAsync($"PreviousTray Set to Device {deviceNumber.ToString()}  Tray: {trayNumber.ToString()}"));
                                        }
                                        else
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"Problem submitting drive request.  {cError}"));
                                            continueLoop = false;
                                        }
                                    }
                                    else  // current and requested trays are the same
                                    {
                                        continueLoop = false;
                                        deviceResponse = DeviceResponse.Success;
                                        Task.Run(() => _logger.LogDetailAsync($"Pick is on the same tray: Current Tray:  {status.CurrentTray.ToString()}  Tray Number:  {trayNumber.ToString()}"));
                                    }
                                }
                                else //InMotion = true
                                {
                                    if (loopCounter >= 10)
                                    {
                                        continueLoop = false;
                                        deviceResponse = DeviceResponse.DeviceInMotion;
                                    }
                                    else
                                    {
                                        loopCounter += 1;
                                        Thread.Sleep(millisecondsTimeout: 100);
                                        var counter = loopCounter;
                                        Task.Run(() => _logger.LogDetailAsync($"Position Device: Waiting for tray to be in position to send new command.  Current Tray: {status.CurrentTray} In Motion: {status.InMotion}  Loop Count: {counter.ToString()}"));
                                    }
                                }
                            }
                            else  //status.Good_Status = false
                            {
                                if (loopCounter >= 10)
                                {
                                    continueLoop = false;
                                    deviceResponse = DeviceResponse.DeviceBadStatus;
                                }
                                else
                                {
                                    loopCounter += 1;
                                    Thread.Sleep(millisecondsTimeout: 100);
                                    var counter = loopCounter;
                                    Task.Run(() => _logger.LogDetailAsync($"Device Response was Bad Status  LoopCounter: {counter}"));
                                }
                            }
                        } //while continue loop
                    }
                    else
                    {
                        Task.Run(() => _logger.LogDetailAsync($"Device Not Initialized.  Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()} Code is: {_hanel.LastStatus_Code.ToString()}  Message is: {_hanel.LastStatus_Message}"));
                        deviceResponse = DeviceResponse.DeviceNotInitialized;
                    }
                }
                else
                {
                    Task.Run(() => _logger.LogDetailAsync($"Position Device: Device not Enabled."));
                    deviceResponse = DeviceResponse.DeviceNotEnabled;
                }
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync($"Position Device: Device not Found."));
                deviceResponse = DeviceResponse.DeviceNotFound;
            }

            return deviceResponse;
        }

        private void AbortNotification()
        {
            if (!_hanel.Init_Success)
            {
                Task.Run(() => _logger.LogDetailAsync($"Not Initialized.  Code is: {_hanel.LastStatus_Code.ToString()}  Message is: {_hanel.LastStatus_Message}"));
                return;
            }
            cError = "";
            if (_hanel.Notification_DeRegister(MyNotificationHandle, ref cError))
                Task.Run(() => _logger.LogDetailAsync($"Notification aborted successfully..."));
            else
                Task.Run(() => _logger.LogDetailAsync($"De-registration Error...  {cError}"));
        }

        public DeviceResponse Park()
        {
            var response = DeviceResponse.UnknownFailure;
            foreach (var item in _workstation.HardwareDevices)
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

        public void CloseController()
        {
            try
            {
                _hanel.Close_Controller(ref cError);
                
                Task.Run(() => _logger.LogDetailAsync($"Close RCC Controller - Success {cError}"));
            }
            catch(Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Close RCC Controller - cError  {cError}  {Environment.NewLine} {ex.Message}  {Environment.NewLine} {ex.InnerException}"));
            }
  
        }

        public HanelDeviceStatus GetDeviceStatus(int deviceNumber)
        {
            var msg = string.Empty;
            var deviceStatus = new HanelDeviceStatus();
         _ = _logger.LogDetailAsync($"Device Number Status: {deviceNumber}");
            if (!_hanel.Init_Success)
            {
             _ = _logger.LogDetailAsync($"Device Status: Not Initialized. Code is: {_hanel.LastStatus_Code.ToString()} Message is: {_hanel.LastStatus_Message}");
             _ = _logger.LogDetailAsync("Problem getting device status." + "\n\n" + cError);
            }
            else
            {
                cError = "";
                // When you request device status, you get status for all devices. That is the reason for the list.
                // Even if there is only a single device, it comes back in a list.
                var myDeviceStatusList = new List<HanelDeviceStatus>();
                if (_hanel.Get_Device_Status(ref myDeviceStatusList, ref cError))
                {
                    // At this point, you have current status for every device in your list
                 _ = _logger.LogDetailAsync($"Device Status DeviceNumber: {deviceNumber}   Hardware Count: {_workstation.EnabledDevices.Count}");
                    foreach (var item in myDeviceStatusList)
                    {
                        if (item.Device == deviceNumber)
                        {
                            deviceStatus = item;

                            msg = "Device: \t" + deviceStatus.Device.ToString() + "\n" +
                                "Target Tray: \t" + deviceStatus.TargetTray.ToString() + "\n" +
                                "Current Tray: \t" + deviceStatus.CurrentTray.ToString() + "\n" +
                                "In Motion: \t" + deviceStatus.InMotion.ToString() + "\n" +
                                "In Alignment: \t" + deviceStatus.InAlignment.ToString() + "\n" +
                                "Last Command: \t" + deviceStatus.LastCommand.ToString() + "\n" +
                                "Last Status: \t" + deviceStatus.LastStatus.ToString() + "\n" +
                                "Message: \t" + deviceStatus.StatusMessage.ToString();
                        }
                    }

                 _ = _logger.LogDetailAsync(msg);
                    //ShowMessage(msg);
                }
                else
                {
                 _ = _logger.LogDetailAsync("Get Device Status request aborted...");
                }
            }
            return deviceStatus;
        }
    }
}


