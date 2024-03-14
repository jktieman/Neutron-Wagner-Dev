using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronEvents
{
    public class IsClientConnectedEventArgs :EventArgs
    {
        public bool IsClientConnected { get; set; }
    }
}
