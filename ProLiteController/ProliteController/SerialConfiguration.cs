using System.IO.Ports;

namespace ProliteController
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
        public int StopBits { get; set; }
        public int DeviceCount { get; set; }
        public int LogLevel { get; set; }
        public int NotificationTimeout { get; set; }
        public bool SimulationMode { get; set; }
        public int ControllerId { get; set; }
    }
}
