using System;
using System.Collections.Generic;
//using System.IO.Ports;
using RJCP.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlliedLogger;
using Hanel_DC.Extensions;
using Hanel_DC.HanelUtilities;
using HanelCommands;

namespace Hanel_DC.Hanel_DeviceControllers
{
    internal class HanelMp12DDeviceController : IHanelDeviceController
    {
        private readonly IDynamicLogger _logger;
        private string _xErrorMsg = "";
        private readonly bool _Serial_Type = true;
        private readonly string _LogFileName = "Hanel_MP12D";   
        private readonly int _PB_True = -1;
        private uint _hMachine;
        private int _xErrorCode;
        private readonly string _ObjectID;
        private readonly int _PB_False;
        private HanelMp12DSerialPortMonitor _serialPortMonitor;

        private List<HanelDeviceStatus> _currentHanelDeviceStatusList;

        public HanelMp12DDeviceController(IDynamicLogger logger)
        {
            _logger = logger;
            this._ObjectID = HanelUtil.GetUniqueObjectIdentifier();
        }

        public int Get_LastErrorCode()
        {
            return this._xErrorCode;
        }

        public string Get_LastErrorMessage()
        {
            return this._xErrorMsg;
        }
        public string Get_ObjectID()
        {
            return this._ObjectID;
        }

        public string Get_LogFileName()
        {
            return this._LogFileName;
        }

        public bool Is_ChannelType_Serial()
        {
            return this._Serial_Type;
        }

        public bool Is_ChannelType_IP()
        {
            return !this._Serial_Type;
        }


        public bool OpenChannel(int nCommPort, int nBaudRate
            , int nDataBits, string cParity, int nStopBits, ref string cError
            , bool simulationMode, int logLevel, ref List<HanelDeviceStatus> currentHanelDeviceStatusList, string logPath = "")
        {
            var parity = Parity.None;
            var stopBits = GetStopBits(nStopBits);  // StopBits.One;
            _currentHanelDeviceStatusList = currentHanelDeviceStatusList;
            if (cParity == "NONE") parity = Parity.None;
            if (nStopBits == 1) stopBits = StopBits.One;

            var comPort = $"COM{nCommPort}";
            bool flag = false;
            cError = "";
            if (_serialPortMonitor == null)
            {
                try
                {
                    //_serialPortMonitor = new HanelMp12DSerialPortMonitor( comPort
                    //    , nBaudRate, nDataBits, parity, stopBits, ref cError, ref _currentHanelDeviceStatusList, _logger);
                    _serialPortMonitor = new HanelMp12DSerialPortMonitor(comPort
                        , nBaudRate, nDataBits, parity, stopBits, ref cError, ref currentHanelDeviceStatusList, _logger);
                }
                catch (Exception ex)
                {
                    cError = $"{cError}{Environment.NewLine}{ex.Message}";
                 _ = _logger.LogDetailAsync(cError);
                    throw;
                }
            }

            if (_serialPortMonitor != null)
            {
                if (_serialPortMonitor.IsPortOpen)
                {
                    _serialPortMonitor.Start();
                    flag = true;
                }



                //    if (HanelMp12DDeviceController.Mp12D_Open(nCommPort, nBaudRate, nDataBits, ref cParity, nStopBits, SimulationMode ? -1 : 0, ref this._hMachine, ref cError, LogLevel, ref LogPath) == this._PB_True && this._hMachine > 0U)
                //    {
                //        cError = "";
                //        flag = true;
                //    }
                //    else if (cError == "")
                //    {
                //        this._hMachine = 0U;
                //        cError = "Something went wrong attempting to open a channel to the machine controller.";
                //    }
                //    else
                //        this._hMachine = 0U;

                else
                    cError = "Unable to open serial port.";
            }


            return flag;
        }

        private static StopBits GetStopBits(int stopBits)
        {
            switch (stopBits)
            {
                case 1:
                    return StopBits.One;
                case 2:
                    return StopBits.Two;
                case 3:
                    return StopBits.One5;
                default:
                    return StopBits.One;
            }
        }

        private static Parity GetParity(string parity)
        {
            switch (parity)
            {
                case "Even":
                    return Parity.Even;
                case "Mark":
                    return Parity.Mark;
                case "None":
                    return Parity.None;
                case "Odd":
                    return Parity.Odd;
                case "Space":
                    return Parity.Space;
                default:
                    return Parity.None;
            }
        }

        public bool DriveDevice(IHanelCommand hanelCommand, ref string cError)
        {
           return _serialPortMonitor.SendData(hanelCommand.Command);
        }

        public bool DriveDevice(int nAccess, int nTray, ref string cError)
        {
            return this.DriveDevice(nAccess, nTray, 0, 0, 0, "", ref cError);
        }

        public static char CR = Convert.ToChar(13);
        public static char LF = Convert.ToChar(10);
        public static char AST = Convert.ToChar(42);

        public bool DriveDevice(int nAccess, int nTray, int nFacing, int nDepth, int nQuantity, string cDisplayText, ref string cError)
        {
            var device = nAccess.ToString().PadLeft(2, '0');
            var ap = "1";
            var tray = nTray.ToString();
            var over = nFacing.ToString();
            var back = nDepth.ToString();

 
            int num = this._PB_False;
            cError = "";
            var hanelCommand = $"{AST}G{device}{ap}$M XR$E20$T{tray}$F{over}$O{back}$P1${CR}{LF}";
            _serialPortMonitor.SendData(Encoding.UTF8.GetBytes(hanelCommand));
            
            
            //if (this._hMachine > 0U)
            //    num = nTray != 0 ? HanelMp12DDeviceController.C2000_DriveDevice(this._hMachine, nAccess, nTray, nFacing, nDepth, nQuantity, ref cDisplayText, ref cError) : Hart_C2000_DeviceController.C2000_ReturnTray(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            //return num == this._PB_True;
            return true;
        }

        public bool ReturnTray(int nAccess, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            // //   num = HanelMp12DDeviceController.C2000_ReturnTray(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public bool GetDeviceStatus(int nAccess, ref int nTray, ref bool nInMotion, ref bool nInAlignment, ref string cError)
        {
            // IF the command has already been accepted, don't send it again
            // Let the input processing update the command Executed and update the Tray Number
            // If accepted but not executed, make sure the Polling is happening
            var currentHanelDeviceStatus = _currentHanelDeviceStatusList.FirstOrDefault(r => r.Device == nAccess);
            if (currentHanelDeviceStatus == null) return false;

            //if (currentHanelDeviceStatus.CommandAccepted && currentHanelDeviceStatus.CommandExecuted == false )
            //{
            //    _serialPortMonitor.CancelPolling = false;
            //    return true;
            //}
            //else
            //{
            //    // The Command has NOT been accepted so Accept it here
            //    currentHanelDeviceStatus.CommandAccepted 
            //}
            //var device = nAccess.ToString().PadLeft(2, '0');
            //var command = $"{AST}G{device}1$M XR$E12${CR}{LF}";
            //_serialPortMonitor.SendData(command.StringToByteArray());
            
            
            //int num = this._PB_False;
            //if (this._hMachine > 0U)
            //{
            //    int nInMotion1 = 0;
            //    int nInAlignment1 = 0;
            //    // num = HanelMp12DDeviceController.C2000_GetDeviceStatus(this._hMachine, nAccess, ref nInMotion1, ref nInAlignment1, ref nTray, ref cError);
            //    nInMotion = nInMotion1 == -1;
            //    nInAlignment = nInAlignment1 == -1;
            //}
            //else
            //    cError = "Channel to the machine controller is not open";
            //return num == this._PB_True;
            nTray = currentHanelDeviceStatus.CurrentTray;
            nInMotion = currentHanelDeviceStatus.InMotion;
            nInAlignment = currentHanelDeviceStatus.InAlignment;

            return true;
        }

        public bool ShowText(int nAccess, string DisplayText, ref string cError)
        {
            _serialPortMonitor.ShowText(nAccess,DisplayText, ref cError);
           
            return true;
        }

        public bool ClearText(int nAccess, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            //    num = HanelMp12DDeviceController.C2000_ClearText(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public void CloseChannel()
        {
         _ = _logger.LogDetailAsync("Begin");
            _serialPortMonitor?.Stop();
         _ = _logger.LogDetailAsync("End");

        }

    }
}
