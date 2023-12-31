using NeutronData.Interfaces;
using System.IO.Ports;
using System;

namespace NeutronData.Models
{
    public class SerialConfiguration : IEntity
    {
        private const int MaxNameLength = 50;
        private string _name;
        private string _portName;
        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name cannot be null or empty.");
                }
                if (value.Length > MaxNameLength)
                {
                    throw new ArgumentException($"Name cannot be longer than {MaxNameLength} characters.");
                }
                _name = value;
            }
        }

        public string PortName
        {
            get => _portName;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name cannot be null or empty.");
                }
                if (value.Length > MaxNameLength)
                {
                    throw new ArgumentException($"Name cannot be longer than {MaxNameLength} characters.");
                }
                _portName = value;
            }
        }

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
