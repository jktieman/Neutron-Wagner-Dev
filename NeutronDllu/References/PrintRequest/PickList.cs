using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintRequest
{
    public class PickList
    {
        public string Station { get; set; }
        public string OrderDetailId { get; set; }
        public string Order { get; set; }
        public string Invoice { get; set; }
        public string CostCenter { get; set; }
        public string Recipient { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Slot { get; set; }
        public string OnHand { get; set; }
        public string Ordered { get; set; }
    }
}
