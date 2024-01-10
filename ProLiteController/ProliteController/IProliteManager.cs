
namespace ProliteController
{
    public interface IProLiteManager
    {
        void AddProlite(int id, string name, int deviceNumber, bool enabled);
        void RemoveProlite(int deviceNumber);
        void TurnOn(int deviceNumber, int level, int part, int quantity);
        void TurnOnHot(int deviceNumber);
        void TurnOnBlindCycle(int deviceNumber, int level, int part);
        void ClearProlite(int deviceNumber);
        void ClearAllProlites();

        bool IsProliteManagerEnabled();
    }
}