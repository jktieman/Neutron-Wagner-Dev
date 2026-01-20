
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProliteController
{
    public interface IProLiteManager
    {
        void AddProlite(int id, string name, int deviceNumber, bool enabled);
        void RemoveProlite(int deviceNumber);
        void TurnOn(int deviceNumber, int level, int part, int quantity);
        void TurnOnLocation(int deviceNumber, int tray, int level, int part, int quantity);
        void TurnOnHot(int deviceNumber);
        void TurnOnBlindCycle(int deviceNumber, int level, int part);
        void ClearProlite(int deviceNumber);
        Task ClearAllProlites();
        Task StartProcessingCommands();
        Task StopProcessingCommands();
        bool IsProliteManagerEnabled();
        List<Prolite> GetProlites();
    }
}