using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hanel_DC.HanelUtilities;

namespace Hanel_DC.Hanel_DeviceControllers
{
    internal class HanelMp12NDeviceController : IHanelDeviceController
    {
        private string _xErrorMsg = "";
        private readonly bool _Serial_Type = true;
        //private readonly string _WebAuthID = Hart_DC_Statics.Controller_Type_Remstar_C2000();
        private readonly string _LogFileName = "Hanel_MP12N";
       // private readonly string _Required_LV_Version = "2018.04.09";
        private readonly int _PB_True = -1;
        private uint _hMachine;
        private int _xErrorCode;
        private readonly string _ObjectID;
        private readonly int _PB_False;

        public HanelMp12NDeviceController()
        {
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

        //public string Get_WebAuthID()
        //{
        //    return this._WebAuthID;
        //}

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

        //public string Get_Required_LV_Version()
        //{
        //    return this._Required_LV_Version;
        //}

        //public bool Is_Required_LV_Version()
        //{
        //    return Hart_C2000_DeviceController.C2000_Version() == this.Get_Required_LV_Version();
        //}

        public bool OpenChannel(int nCommPort, int nBaudRate, int nDataBits, string cParity, int nStopBits, bool SimulationMode, int LogLevel, string LogPath, ref string cError)
        {
            bool flag = false;
            int pbFalse = this._PB_False;
            cError = "";
            if (this._hMachine == 0U)
            {
                //if (HanelMp12NDeviceController .Open(nCommPort, nBaudRate, nDataBits, ref cParity, nStopBits, SimulationMode ? -1 : 0, ref this._hMachine, ref cError, LogLevel, ref LogPath) == this._PB_True && this._hMachine > 0U)
                //{
                //    cError = "";
                //    flag = true;
                //}
                //else if (cError == "")
                //{
                //    this._hMachine = 0U;
                //    cError = "Something went wrong attempting to open a channel to the machine controller.";
                //}
                //else
                //    this._hMachine = 0U;
            }
            else
                cError = "Channel to the machine controller is already open";
            return flag;
        }

        public bool DriveDevice(int nAccess, int nTray, ref string cError)
        {
            return this.DriveDevice(nAccess, nTray, 0, 0, 0, "", ref cError);
        }

        public bool DriveDevice(int nAccess, int nTray, int nFacing, int nDepth, int nQuantity, string cDisplayText, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            //    num = nTray != 0 ? Hart_C2000_DeviceController.C2000_DriveDevice(this._hMachine, nAccess, nTray, nFacing, nDepth, nQuantity, ref cDisplayText, ref cError) : Hart_C2000_DeviceController.C2000_ReturnTray(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public bool ReturnTray(int nAccess, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            //    num = Hart_C2000_DeviceController.C2000_ReturnTray(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public bool GetDeviceStatus(int nAccess, ref int nTray, ref bool nInMotion, ref bool nInAlignment, ref string cError)
        {
            int num = this._PB_False;
            if (this._hMachine > 0U)
            {
                int nInMotion1 = 0;
                int nInAlignment1 = 0;
              //  num = Hart_C2000_DeviceController.C2000_GetDeviceStatus(this._hMachine, nAccess, ref nInMotion1, ref nInAlignment1, ref nTray, ref cError);
                nInMotion = nInMotion1 == -1;
                nInAlignment = nInAlignment1 == -1;
            }
            else
                cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public bool ShowText(int nAccess, string DisplayText, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            //    num = Hart_C2000_DeviceController.C2000_ShowText(this._hMachine, nAccess, ref DisplayText, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public bool ClearText(int nAccess, ref string cError)
        {
            int num = this._PB_False;
            cError = "";
            //if (this._hMachine > 0U)
            //    num = Hart_C2000_DeviceController.C2000_ClearText(this._hMachine, nAccess, ref cError);
            //else
            //    cError = "Channel to the machine controller is not open";
            return num == this._PB_True;
        }

        public void CloseChannel()
        {
            if (this._hMachine <= 0U)
                return;
           // Hart_C2000_DeviceController.C2000_Close(this._hMachine);
          //  Hart_C2000_DeviceController.C2000_Close(this._hMachine);
            this._hMachine = 0U;
        }

    }
}
