using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class HardwareDeviceView
    {
        public int Id { get; set; }
        public int DeviceNumber { get; set; }
        public string Name { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; }
        public int NumberOfCarriers { get; set; }
        public int CarrierWidth { get; set; }
        public int CarrierDepth { get; set; }
        public string DeviceTypeName { get; set; }
        public bool Enabled { get; set; }
        public bool SimulationMode { get; set; }
        public int LogLevel { get; set; }


    }
}
