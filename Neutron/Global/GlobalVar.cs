using Neutron.Controllers;
using Neutron.Interfaces;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using ProliteController;
using SlotNameFactory;

namespace Neutron.Global
{
    public static class GlobalVar
    {
        public static IDisplayController  Displays { get; set; }

        public static WorkstationView Workstation { get; set; }

        public static IShuttleDriver Shuttle { get; set; }
        public static IHanelDriver Hanel { get; set; }
        public static IBlastzone Blastzone { get; set; }
        public static IProliteManager ProliteManager { get; set; }
        public static ISlotNameFactory SlotNameFactory { get; set; }

        public static User User { get; set; }

        public static bool LoaderRunning { get; set; }

        public static bool UploadRunning { get; set; }

        public static HistoryManager HistoryManager { get; set; }

    }
}
