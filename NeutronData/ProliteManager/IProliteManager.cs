using NeutronData.Models;

namespace NeutronData.ProLiteManager;

public interface IProLiteManager
{
    void AddProlite(HardwareDevice hardwareDevice);
    void RemoveProlite(int deviceNumber);
    void TurnOn(int deviceNumber, int level, int part, int quantity);
    void TurnOnHot(int deviceNumber);
    void TurnOnBlindCycle(int deviceNumber, int level, int part);
    void ClearProlite(int deviceNumber);

}