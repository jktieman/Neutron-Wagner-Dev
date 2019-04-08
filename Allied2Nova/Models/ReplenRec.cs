using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Allied2Nova.Models
{
    public class ReplenRec
    {
        public string Order { get; set; }
        public string Invoice { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }

        public ReplenRec(string order, string invoice, string sku, int qty)
        {
            this.Order = order;
            this.Invoice = invoice;
            this.Sku = sku;
            this.Qty = qty;
        }

        public ReplenRec()
        {
            // TODO: Complete member initialization
        }
    }
}
