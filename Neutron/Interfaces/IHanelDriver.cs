using Hanel_DC.Hanel_DeviceControllers;
using HanelCommands;
using Neutron.Enums;

namespace Neutron.Interfaces
{
    public interface IHanelDriver
    {
        DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 0, int depth = 0, int quantity = 0, string display = "");
        DeviceResponse Park();
        bool CloseController();
        HanelDeviceStatus GetDeviceStatus(int deviceNumber);
        int InitStatus();
        void ResetHanelDeviceStatus();

        byte[] ValidCommand(byte[] byteArray);
    }
}
