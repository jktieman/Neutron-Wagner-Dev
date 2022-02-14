using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NeutronMaintenance.Models
{
    public class NovaRandomLocation
    {
        public string RecordTypeName { get; set; } = "RANDOMLOCATION";
        public string Car { get; set; }
        public string Bin { get; set; }
        public string  Lvl { get; set; }
        public string Prt { get; set; }
        public string Velocity { get; set; }
        public string Size { get; set; }
        public string Height { get; set; }
        public string Operation { get; set; }
        public string SystemNumber { get; set; }
    }
}
