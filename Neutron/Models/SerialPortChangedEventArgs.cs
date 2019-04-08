using System;

namespace Neutron.Models
{
    public class SerialPortChangedEventArgs : EventArgs
    {
        public string Port { get; set; }
    }
}