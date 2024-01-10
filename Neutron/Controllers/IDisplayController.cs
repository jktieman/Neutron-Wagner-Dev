using Hart_DisplayControllers;
using System;
using System.Threading.Tasks;
using Neutron.Models;

namespace Neutron.Controllers
{
    public interface IDisplayController
    {
        event EventHandler<MyDataReceivedEventArgs> MyDataReceived;
        void CloseController();
        bool Ready { get; set; }
        void SendText(string text);
        void ClearAllBli();
        void ClearAllShi();
        void TurnOnAllBli();
        void TurnOnAllBlastzones();
        void ShowBli(int address, int beacon, string text);
        void ShowBli(int bayControllerId, int address, int beacon, string text);
        void ShowBlastzone(int bayController, int address, int beacon, string text);
        void ClearBlastzone();
        void ShowBlastzoneOc(int bayController, int address, int beacon, string text);
        void ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text);
        void ShowBli(Hart_BLI bli);
        void ShowShi(int device, int bin, int level, string part, string text);
        void ShowShi(Hart_SHI shi);
        void ClearBli(Hart_BLI bli);
        void ClearShi(Hart_SHI shi);
        void ShowBli(Ipti_BLI bli);
        void ClearBli(Ipti_BLI bli);
        int GetInitStatus();
       // void ShowOc(int address, int beacon, string text);
        void ShowOc(int bayControllerId, int address, int beacon, string text);
        void ClearOcAsync(int bayControllerId, int address);
    }
}