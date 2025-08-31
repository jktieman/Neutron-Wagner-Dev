using HanelCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanel_DC.Hanel_DeviceControllers
{
    internal interface IHanelDeviceController
    {
        string Get_ObjectID();

        string Get_LogFileName();

        bool Is_ChannelType_Serial();

        bool Is_ChannelType_IP();

        int Get_LastErrorCode();

        string Get_LastErrorMessage();

        bool DriveDevice(IHanelCommand hanelCommand, ref string cError);

        bool DriveDevice(int nAccess, int nTray, ref string cError);

        bool ReturnTray(int nAccess, ref string cError);

        bool GetDeviceStatus(
            int nAccess,
            ref int nTray,
            ref bool nInMotion,
            ref bool nInAlignment,
            ref string cError);

        void CloseChannel();
        void Stop();
    }
}
