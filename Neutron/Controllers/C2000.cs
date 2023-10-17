using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Windows.Forms;
using NeutronData.Models;
using System.Threading;
using AlliedLogger;
using Neutron.Enums;
using NeutronData.ModelViews;

using Neutron.Interfaces;

using Hart_DeviceControllers;
using NeutronCore;
using NeutronCore.Enums;

namespace Neutron.Controllers
{

    public class C2000 : IShuttleDriver
    {
        private readonly Hart_DeviceController Shuttle_1;
        readonly SendOrPostCallback CallBackHandler_Init;
        readonly Hart_TellMeWhenTrayArrives MyTrayArrivedNotificationDelegate = MyTrayArrived;
        readonly Guid MyNotificationHandle;
        int NotificationTimeOutSeconds = 60;
        readonly bool NotificationAutoDeregister = true;
        string cError = "";
        readonly int Device_Stationary = -1;
        readonly int Device_Alignment_DontCare = 0;

        readonly DynamicLogger _logger;
        private readonly WorkstationView _workstationView;
        private Form _currentForm;
        private readonly Object locker = new Object();
        private int[] _previousTray;

        public C2000(Form frm, WorkstationView workstationView)
        {

            _previousTray = new int[10];
            _workstationView = workstationView;
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = string.Format(format: @"C2000_Station_{0}", arg0: workstationView.WorkstationId.ToString());
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
            _currentForm = frm;
            Task.Run(() => _logger.LogDetailAsync($"C2000 Constructor - {frm.Name}"));
            CallBackHandler_Init = new SendOrPostCallback(MyInitProgressDelegate);
            var hartLog = ($"{logFileDir}Hart");
            Shuttle_1 = new Hart_DeviceController(Hart_DeviceController.Controller_Type_Remstar_C2000(), hartLog);
            if (Shuttle_1 != null)
            {
                Task.Run(() => _logger.LogDetailAsync(@"Shuttle has been created: "));
                C2000Init();
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync("Shuttle has NOT been created:  Exiting "));
            }

        }
        public Form CurrentForm
        {
            get { return _currentForm; }
            set
            {
                _currentForm = value;
                Task.Run(() => _logger.LogDetailAsync($"Changed Form - {_currentForm}"));
            }
        }

        private void C2000Init()
        {
            if (_workstationView != null)
            {
                var firstDevice = _workstationView.HardwareDevices.FirstOrDefault();

                if (firstDevice != null)
                {
                    var serialConfiguration = firstDevice.SerialConfiguration;

                    if (serialConfiguration != null)
                    {
                        var deviceCount = _workstationView.HardwareDevices.Count;
                        _previousTray = new int[deviceCount + 1];
                        NotificationTimeOutSeconds = serialConfiguration.NotificationTimeout;
                        var simulationMode = firstDevice.SimulationMode;
                        var logLevel = firstDevice.LogLevel;
                        var enabledUnitNumbers = _workstationView.HardwareDevices.Where(r => r.Enabled).Select(s => s.DeviceNumber).ToList();

                        Task.Run(() =>
                            _logger.LogDetailAsync(
                                $"Serial Address: {serialConfiguration.PortName} Baud Rate: {serialConfiguration.BaudRate.ToString()} Parity: {serialConfiguration.Parity.ToString()}  Device Count: {serialConfiguration.DeviceCount}"));

                        if (Shuttle_1.Init_Controller(serialConfiguration.PortNumber, serialConfiguration.BaudRate,
                            serialConfiguration.DataBits,
                            serialConfiguration.Parity.ToString(), serialConfiguration.StopBits, simulationMode,
                            logLevel,
                            enabledUnitNumbers, this, CallBackHandler_Init, ref cError))
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
            // Note that the sequesnce of the following assignments is critical. Success must be first. Others follow in any sequence.
            bool success = Shuttle_1.Init_Success;
            int initCode = Shuttle_1.LastStatus_Code;
            string initMsg = Shuttle_1.LastStatus_Message;
            Task.Run(() => _logger.LogDetailAsync($"InitStatus: Success: {success} initCode: {initCode} initMsg: {initMsg}"));
            if (success)
            {
                // life is good, you can drive the device
                Task.Run(() => _logger.LogDetailAsync($"InitStatus: success is {success}"));
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
                if (Shuttle_1.Get_Init_PercentageComplete() == 0)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Not Initialized.  Code is: {initCode.ToString()}  Message is: {initMsg}"));
                }
                else if (Shuttle_1.Get_Init_PercentageComplete() < 100)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Initialization is in progress.  Init {Shuttle_1.Get_Init_PercentageComplete().ToString()}% complete..."));
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
            // MessageBox.Show(msg);
        }

        /*
     This is a delegate method and it will be called when any init notifications fire. 
     All delegates passed to Init_Controller() must have this same signature.
     */
        public static void MyInitProgressDelegate(object formObject)
        {

            //Hart_DC_Init myInit_Progress = (Hart_DC_Init) formObject;



            //// This method is running in the UI synchronization context.
            //// You just need to create a reference to your original object
            //var Original_CallingObject = (C2000) myInit_Progress.CallersObj;
            //// You may now refer to any components of your UI if
            //// you prefix them with: Original_Form_Alias.

            //var newForm = (FrmMain) Original_CallingObject.currentForm;

            //newForm.UpdateInitStatus(myInit_Progress.Success, myInit_Progress.PercentComplete);


            // This method is running in the UI synchronization context.
            //// You just need to create a reference to your original object
            //var formAlias = (C2000) formObject;
            //// You may now refer to any components of your UI as below.
            //// formAlias.LabelNotify.Text = "Init " + formAlias.Shuttle_1.Init_PercentageComplete.ToString() + "% complete...";
            //formAlias.ShowMessage("Init " + formAlias.Shuttle_1.Get_Init_PercentageComplete().ToString() + "% complete...");
        }

        public static void MyTrayArrived(Hart_DeviceNotificationType firedNotification)
        {
            // This method is running in the UI synchronization context but
            // you need to create a reference to your original form in order
            //// to reference any of its controls.
            //var formAlias = (C2000) firedNotification.CallersObject;
            //// You may now refer to any components of your UI as below.
            //// formAlias.LabelNotify.Text = "Drive Notification Received";
            //formAlias.ShowMessage("Drive Notification Received");
            //Task.Run(() => formAlias.logger.LogDetailAsync($"Shuttle {firedNotification.TargetDevice.ToString()} Notification"));

            //if (firedNotification.Message.ToString().Length > 0)
            //{
            //    // you should report these messages to the operator or write them to a log that is frequently monitored.
            //    Task.Run(() => formAlias.logger.LogDetailAsync($"Notification request for tray {firedNotification.TargetTray.ToString()} on shuttle {firedNotification.TargetDevice.ToString()} returned with a message.  Message: {firedNotification.Message.ToString()}"));
            //}

            //if (firedNotification.Expired)
            //{
            //    Task.Run(() => formAlias.logger.LogDetailAsync($"Notification request for tray {firedNotification.TargetTray.ToString()} on shuttle {firedNotification.TargetDevice.ToString()}  has timed-out."));
            //}

            //else if (firedNotification.MotionStatusUponNotification)
            //{
            //    // Note that the current implementation of Notification_Register() does not support notifications
            //    // for devices in motion, so this particular logic will not execute at this time. 
            //    Task.Run(() => formAlias.logger.LogDetailAsync($"Shuttle {firedNotification.TargetDevice.ToString()} is in motion."));
            //    // If this was a horizontal or vertical carousel, NotifyTarget.Current_Tray would describe the shelf/carrier currently in position as it moves past.
            //}

            //else
            //{
            //    string alignmentStatus = firedNotification.AlignmentStatusUponNotification ? "in alignment." : "out of alignment.";
            //    Task.Run(() => formAlias.logger.LogDetailAsync($"Shuttle {firedNotification.TargetDevice.ToString()} is stationary and tray {firedNotification.TargetTray.ToString()} is {alignmentStatus}"));
            //}
        }

        public DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 1, int depth = 1, int quantity = 0, string display = "")
        {

            DeviceResponse deviceResponse = DeviceResponse.UnknownFailure;
            Task.Run(() => _logger.LogDetailAsync($"Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()}  Time: {DateTime.Now}  Thread: {Thread.CurrentThread.ManagedThreadId}"));
            bool continueLoop = true;
            int loopCounter = 0;
            HardwareDevice device = _workstationView.HardwareDevices.FirstOrDefault(r => r.DeviceNumber == deviceNumber);
            if (device != null && device.Enabled)
            {
                if (device.Enabled)
                {
                    if (Shuttle_1.Init_Success)
                    {
                        while (continueLoop)
                        {
                            Hart_DeviceStatusType status = GetDeviceStatus(deviceNumber);

                            if (status.Good_Status)
                            {
                                loopCounter = 0;
                                if (!status.In_Motion)
                                {
                                    if (status.Current_Tray != trayNumber)
                                    {
                                        if (_previousTray[deviceNumber] != 0)
                                        {
                                            if (status.Current_Tray != _previousTray[deviceNumber])
                                            {
                                                Task.Run(() => _logger.LogDetailAsync($"Tray did NOT arrive."));
                                                Task.Run(() => _logger.LogDetailAsync($"Status.Current_Tray: {status.Current_Tray.ToString()}  Tray Number: {trayNumber.ToString()}"));
                                                Task.Run(() => _logger.LogDetailAsync($"PreviousTray: {_previousTray[deviceNumber].ToString()}"));
                                                deviceResponse = DeviceResponse.TrayDidNotArrive;
                                                _previousTray[deviceNumber] = 0;
                                                break;
                                            }
                                        }

                                        cError = "";
                                        if (Shuttle_1.Drive_Device(deviceNumber, trayNumber, facing, depth, quantity, display, ref cError))
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
                                        Task.Run(() => _logger.LogDetailAsync($"Pick is on the same tray: Current Tray:  {status.Current_Tray.ToString()}  Tray Number:  {trayNumber.ToString()}"));
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
                                        Task.Run(() =>
                                            _logger.LogDetailAsync(
                                                $"Position Device: Waiting for tray to be in position to send new command.  Current Tray: {status.Current_Tray.ToString()}  In Motion is {status.In_Motion.ToString()}  Loop Count: {counter.ToString()}"));
                                  
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
                                    Task.Run(() => _logger.LogDetailAsync(msg: "Device Response was Bad Status  LoopCounter: " + counter.ToString()));
                                }
                            }
                        }
                    }
                    else
                    {
                        Task.Run(() => _logger.LogDetailAsync($"Device Not Initialized.  Device: {deviceNumber.ToString()} Tray: {trayNumber.ToString()} Code is: {Shuttle_1.LastStatus_Code.ToString()}  Message is: {Shuttle_1.LastStatus_Message}"));
                        deviceResponse = DeviceResponse.DeviceNotInitialized;
                    }
                }
                else
                {
                    Task.Run(() => _logger.LogDetailAsync($"Postition Device: Device not Enabled."));
                    deviceResponse = DeviceResponse.DeviceNotEnabled;
                }
            }
            else
            {
                Task.Run(() => _logger.LogDetailAsync($"Postition Device: Device not Found."));
                deviceResponse = DeviceResponse.DeviceNotFound;
            }
            return deviceResponse;
        }


        private void AbortNotification()
        {
            if (!Shuttle_1.Init_Success)
            {
                Task.Run(() => _logger.LogDetailAsync($"Not Initialized.  Code is: {Shuttle_1.LastStatus_Code.ToString()}  Message is: {Shuttle_1.LastStatus_Message}"));
                return;
            }
            string cError = "";
            if (Shuttle_1.Notification_DeRegister(MyNotificationHandle, ref cError))
                Task.Run(() => _logger.LogDetailAsync($"Notification aborted successfully..."));
            else
                Task.Run(() => _logger.LogDetailAsync($"Deregistration Error...  {cError}"));
        }

        public DeviceResponse Park()
        {
            DeviceResponse response = DeviceResponse.UnknownFailure;
            foreach (var item in _workstationView.HardwareDevices)
            {

                if (item.Enabled)
                {
                    if (item.DeviceTypeId == (int)DeviceTypeEnum.Shuttle) //Shuttle
                    {
                        response = PositionDevice(item.DeviceNumber, 0);
                    }
                    if (item.DeviceTypeId == (int)DeviceTypeEnum.Carousel) //Carousel
                    {
                        response = PositionDevice(item.DeviceNumber, 1);
                    }
                }
            }
            return response;
        }

        public void CloseController()
        {
            try
            {
                Shuttle_1.Close_Controller(ref cError);

                _logger.LogDetailAsync($"Close 2000 Controller - Success {cError}");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Close 2000 Controller - cError  {cError}  {Environment.NewLine} {ex.Message}  {Environment.NewLine} {ex.InnerException}");
            }
        }

        public Hart_DeviceStatusType GetDeviceStatus(int deviceNumber)
        {
            string msg = string.Empty;
            var deviceStatus = new Hart_DeviceStatusType();
            _logger.LogDetailAsync($"Device Number Status: {deviceNumber}");
            if (!Shuttle_1.Init_Success)
            {
                _logger.LogDetailAsync($"Device Status: Not Initialized. Code is: {Shuttle_1.LastStatus_Code.ToString()} Message is: {Shuttle_1.LastStatus_Message}");
                _logger.LogDetailAsync("Problem getting device status." + "\n\n" + cError);
            }
            else
            {
                string cError = "";
                // When you request device status, you get status for all devices. That is the reason for the list.
                // Even if there is only a single device, it comes back in a list.
                var myDeviceStatusList = new List<Hart_DeviceStatusType>();
                if (Shuttle_1.Get_Device_Status(ref myDeviceStatusList, ref cError))
                {
                    // At this point, you have current status for every device in your list
                    _logger.LogDetailAsync($"Device Status DeviceNumber: {deviceNumber}   Hardware Count: {_workstationView.EnabledDevices.Count}");
                    foreach (var item in myDeviceStatusList)
                    {
                        if (item.Device == deviceNumber)
                        {
                            deviceStatus = item;

                            //msg = "Device: \t" + deviceStatus.Device.ToString() + "\n" +
                            //    "Target Tray: \t" + deviceStatus.Target_Tray.ToString() + "\n" +
                            //    "Current Tray: \t" + deviceStatus.Current_Tray.ToString() + "\n" +
                            //    "In Motion: \t" + deviceStatus.In_Motion.ToString() + "\n" +
                            //    "In Alignment: \t" + deviceStatus.In_Alignment.ToString() + "\n" +
                            //    "Last Command: \t" + deviceStatus.Last_Command.ToString() + "\n" +
                            //    "Last Status: \t" + deviceStatus.Last_Status.ToString() + "\n" +
                            //    "Message: \t" + deviceStatus.Status_Message.ToString();
                        }
                    }

                    _logger.LogDetailAsync(msg);
                    //ShowMessage(msg);
                }
                else
                {
                    _logger.LogDetailAsync("Get Device Status request aborted...");
                }
            }
            return deviceStatus;
        }
    }
}