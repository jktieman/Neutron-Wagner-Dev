using Neutron.Controllers;
using Neutron.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using SlotNameFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Global
{
    public static class GlobalVar
    {
        private static IShuttleDriver shuttle;
        private static IDisplayController displays;
        private static StationView station;
        private static ISlotNameFactory slotNameFactory;
        private static User user;
        private static bool loaderRunning;
        private static HistoryManager historyManager;

        public static IDisplayController Displays
        {
            get { return displays; }
            set { displays = value; }
        }

        public static StationView Station
        {
            get { return station; }
            set { station = value; }
        }

        public static IShuttleDriver Shuttle
        {
            get { return shuttle; }
            set { shuttle = value; }
        }

        public static ISlotNameFactory SlotNameFactory
        {
            get { return slotNameFactory; }
            set { slotNameFactory = value; }
        }

        public static User User
        {
            get { return user; }
            set { user = value; }
        }

        public static bool LoaderRunning
        {
            get { return loaderRunning; }
            set { loaderRunning = value; }
        }

        public static HistoryManager HistoryManager
        {
            get { return historyManager; }
            set { historyManager = value; }
        }
    }
}
