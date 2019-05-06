using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronEvents
{
    public class SerialPortWriteEventArgs : EventArgs
    {
        public string Request { get; set; }
    }
}
