using System;

namespace NeutronEvents
{ 
    public class SerialPortChangedEventArgs : EventArgs
    {
        public string Port { get; set; }
    }
}