using Hart_DisplayControllers;
using System;

namespace Neutron.Controllers
{
    public interface IDisplayController
    {
        event EventHandler<IptiController.MySerialDataReceivedEventArgs> MySerialDataReceived;
        void CloseController();
        bool Ready { get; set; }
        void ClearAllBli();
        void ClearAllShi();
        void ShowBli(int address, int beacon, string text);
        void ShowBli(Hart_BLI bli);
        void ShowShi(int device, int bin, int level, string part, string text);
        void ShowShi(Hart_SHI shi);
        void ClearBli(Hart_BLI bli);
        void ClearShi(Hart_SHI shi);
        void ShowBli(Ipti_BLI bli);
        void ClearBli(Ipti_BLI bli);
        int GetInitStatus();
        void ShowOc(int address, int beacon, string text);
        void ClearOc(int address);
    }
}