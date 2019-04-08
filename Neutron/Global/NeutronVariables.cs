using Neutron.Controllers;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Global
{

    public class NeutronVariables
    {
        public bool SendAllPicksToHost = false;
        public bool UseReturnToStock = false;
        public bool CreateStoreOrderWithRts = false;
        public bool DisableShuttle = false;
        public bool UsePrimeBin = true;
        public bool UseLAC = false;
        public bool UseMenuSecurity = false;
        public int StationNumber = 3;
        public string DeviceDriver = "C3000";
        public bool SimulationMode = false;
        public int LogLevel { get; set; }
        public string SlotNameType = "Type1";
        public bool AutoLogOff = true;
    }
}
