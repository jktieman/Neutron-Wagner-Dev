
using System;
using System.Threading.Tasks;
using Neutron.Models;

namespace IPTI.Models
{
    public interface IDisplayController
    {
        void CloseController();
        bool Ready { get; set; }
        Task SendText(string text);
        Task TurnOnAllBli();
        Task ClearAllBli();
        Task ClearAllShi();
        Task TurnOnAllBlastzones();
        Task ShowBli(int address, int beacon, string text);
        Task ShowBli(int bayControllerId, int address, int beacon, string text);
        Task ShowBlastzone(int bayController, int address, int beacon, string text);
        Task ClearBlastzone();
        Task ShowBlastzoneOc(int bayController, int address, int beacon, string text);
        Task ClearBlastzoneOc(int bayControllerId, int address, int beacon, string text);
        
        Task ShowBli(IptiBli bli);
        Task ClearBli(IptiBli bli);
        bool GetInitStatus();
       // void ShowOc(int address, int beacon, string text);
        Task ShowOc(int bayControllerId, int address, int beacon, string text);
        Task ClearOcAsync(int bayControllerId, int address);
        Task ShowShi(int device, int bin, int level, string part, string text);
        Task ShowShiAsync(IptiShi shi);
        Task ClearShi(IptiShi shi);
        void DisposeServer();
    }
}