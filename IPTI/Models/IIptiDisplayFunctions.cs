using System.Threading.Tasks;

namespace IPTI.Models
{
    public interface IIptiDisplayFunctions
    {
        Task TurnOnBlastzoneDisplay(int bayController, int position, string text);
        Task TurnOffBlastzoneDisplay(int bayController, int position);
        Task TurnOnBlastzoneOrderControl(int bayController, string text);
        Task TurnOffBlastzoneOrderControl(int bayController);
        Task ClearBlastzone();
        Task ClearBatchTable();
        Task TurnOnBatchDisplay(int position, string text);
        Task TurnOffBatchDisplay(int position);
        Task TurnOnBatchOrderControl(string text);
        Task TurnOffBatchOrderControl();
        Task TurnOnBatchDisplayEnd(int positionNumber);
    }
}