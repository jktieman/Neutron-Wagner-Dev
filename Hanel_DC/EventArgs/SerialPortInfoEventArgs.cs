using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanel_DC.EventArgs
{
    public class SerialPortInfoEventArgs : System.EventArgs
    {
        public SerialPortInfoEventArgs()
        {
        }

        public SerialPortInfoEventArgs(string serialPortInfo)
        {
            SerialPortInfo = serialPortInfo;
        }

        public string SerialPortInfo { get; set; }
    }
    
}
