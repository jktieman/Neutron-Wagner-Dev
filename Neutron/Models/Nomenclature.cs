using Neutron.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Models
{
    public class Nomenclature : INomenclature
    {
        public string MBPickAccept { get; set; }
        public string MBStoreAccept { get; set; }
        public string MBDelete { get; set; }
        public string LabelDevice { get; set; }
        public string LabelTray { get; set; }
        public string LabelOver { get; set; }
        public string LabelBack { get; set; }
    }
}
