using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.General
{
    public class TransferRec
    {
        public string Sku { get; set; }
        public string Des { get; set; }
        public int Car { get; set; }
        public int Bin { get; set; }
        public int Level { get; set; }
        public int Part { get; set; }
        public int Qty { get; set; }
        public int Station { get; set; }
        public string Slot { get; set; }
    }
}
