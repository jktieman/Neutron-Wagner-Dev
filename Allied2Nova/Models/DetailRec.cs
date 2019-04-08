using System;
using System.Linq;
using System.Text;

namespace Allied2Nova.Models
{
    public class DetailRec
    {
        public string Order { get; set; }
        public string Invoice { get; set; }
        public string Sku { get; set; }
        public int Qty { get; set; }

        public DetailRec() { }

        public DetailRec(string sku, int qty)
        {
            this.Sku = sku;
            this.Qty = qty;
        }

        public DetailRec(string order, string invoice, string sku, int qty)
        {
            this.Order = order;
            this.Invoice = invoice;
            this.Sku = sku;
            this.Qty = qty;
        }
    }
}
