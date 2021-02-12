using System;

namespace NeutronEvents
{
    public class SerialPortWriteEventArgs : EventArgs
    {
        public string Request { get; set; }
    }
}
