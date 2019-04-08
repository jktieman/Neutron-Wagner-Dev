using Neutron.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hart_DeviceControllers;

namespace Neutron.Interfaces
{
    public interface IShuttleDriver
    {
        DeviceResponse PositionDevice(int deviceNumber, int trayNumber, int facing = 0, int depth = 0, int quantity = 0, string display = "");
        DeviceResponse Park();
        void CloseController();
        Hart_DeviceStatusType GetDeviceStatus(int deviceNumber);
    }
}
