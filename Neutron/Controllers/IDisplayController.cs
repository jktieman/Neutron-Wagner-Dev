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
        Task ClearAllBli();
        void ClearAllShi();
        Task TurnOnAllBli();
        Task TurnOnAllBlastzones();
        Task ShowBli(int address, int beacon, string text);
        Task ShowBli(int bayControllerId, int address, int beacon, string text);
        Task ShowBlastzone(int bayController, int address, int beacon, string text);
        Task ClearBlastzone();
        Task ShowBlastzoneOc(int bayController, int address, int beacon, string text);
        Task ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text);
        void ShowBli(Hart_BLI bli);
        void ShowShi(int device, int bin, int level, string part, string text);
        void ShowShi(Hart_SHI shi);
        void ClearBli(Hart_BLI bli);
        void ClearShi(Hart_SHI shi);
        Task ShowBli(Ipti_BLI bli);
        Task ClearBli(Ipti_BLI bli);
        int GetInitStatus();
       // void ShowOc(int address, int beacon, string text);
        void ShowOc(int bayControllerId, int address, int beacon, string text);
        void ClearOc(int bayControllerId, int address);
    }
}