using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.PrintModels
{
    public class ShortReport
    {
        public int Position { get; set; }
        public string Order { get; set; }
        public string Invoice { get; set; }
        public string Item { get; set; }
        public int ReqQty { get; set; }
        public int IssQty { get; set; }
    }
}
