using NeutronData.Models;

namespace NeutronData.ProliteManager;

public interface IProliteManager
{
    void AddProlite(HardwareDevice hardwareDevice);
    void RemoveProlite(int deviceNumber);
    void TurnOn(int deviceNumber, int level, int part, int quantity);
    void TurnOnHot(int deviceNumber);
    void TurnOnBlindCycle(int deviceNumber, int level, int part);

}