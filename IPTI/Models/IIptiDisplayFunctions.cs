using System.Threading.Tasks;

namespace IPTI.Models
{
    public interface IIptiDisplayFunctions
    {
        void TurnOnBlastzoneDisplay(int bayController, int position, string text);
        void TurnOffBlastzoneDisplay(int bayController, int position);
        void TurnOnBlastzoneOrderControl(int bayController, string text);
        void TurnOffBlastzoneOrderControl(int bayController);
        void ClearBlastzone();
        void ClearBatchTable();
        void TurnOnBatchDisplay(int position, string text);
        void TurnOffBatchDisplay(int position);
        void TurnOnBatchOrderControl(string text);
        void TurnOffBatchOrderControl();
        void TurnOnBatchDisplayEnd(int positionNumber);
        void DisposeServer();
        bool IsClientConnected();
    }
}