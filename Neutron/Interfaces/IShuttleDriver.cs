using Neutron.Enums;
using Hart_DeviceControllers;

namespace Neutron.Interfaces
{
    public interface IShuttleDriver
    {
        DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 0, int depth = 0, int quantity = 0, string display = "");
        DeviceResponse Park();
        void CloseController();
        Hart_DeviceStatusType GetDeviceStatus(int deviceNumber);
        int InitStatus();
    }
}
