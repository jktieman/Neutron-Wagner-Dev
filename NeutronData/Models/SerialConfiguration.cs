using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class SerialConfiguration : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PortName { get; set; }
        public int PortNumber { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public int DeviceCount { get; set; }
        public int LogLevel { get; set; }
        public int NotificationTimeout { get; set; }
        public bool SimulationMode { get; set; }
        public int ControllerId { get; set; }
    }
}
