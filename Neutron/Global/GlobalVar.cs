using Neutron.Controllers;
using Neutron.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using SlotNameFactory;

namespace Neutron.Global
{
    public static class GlobalVar
    {
        public static IDisplayController Displays { get; set; }

        public static StationView Station { get; set; }

        public static IShuttleDriver Shuttle { get; set; }

        public static ISlotNameFactory SlotNameFactory { get; set; }

        public static User User { get; set; }

        public static bool LoaderRunning { get; set; }

        public static bool UploadRunning { get; set; }

        public static HistoryManager HistoryManager { get; set; }
    }
}
