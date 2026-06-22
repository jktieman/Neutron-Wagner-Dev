using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronDllu
{
    public class LabelDetail
    {
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string EmpId { get; set; }
        public string Order { get; set; }
        public string Invoice { get; set; }
        public DateTime LoadDate { get; set; }
        public string Origin { get; set; }
    }
}
