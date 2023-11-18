using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanelCommands;

namespace Hanel_DC.EventArgs
{
    public class HanelDeviceStatusEventArgs : System.EventArgs
    {
        public HanelDeviceStatusEventArgs()
        {
            
        }

        public HanelDeviceStatusEventArgs(HanelDeviceStatus hanelDeviceStatus)
        {
            HanelDeviceStatus = hanelDeviceStatus;
        }

        public HanelDeviceStatus HanelDeviceStatus { get; set; }
    }
}
