using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Models
{
    public class MyDataReceivedEventArgs :EventArgs
    {
        public string FormText { get; set; }
    }
}
