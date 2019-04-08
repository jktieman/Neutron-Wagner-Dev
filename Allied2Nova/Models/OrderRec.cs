using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allied2Nova.Models
{
    public class OrderRec
    {
        public string Order { get; set; }
        public string Invoice { get; set; }
        

        public OrderRec() { }

        public OrderRec(string order, string invoice)
        {
            this.Order = order;
            this.Invoice = invoice;
        }

        //public DetailRec(string order, string invoice, string sku, int qty)
        //{
        //    this.Order = order;
        //    this.Invoice = invoice;
        //    this.Sku = sku;
        //    this.Qty = qty;
        //}
    }
}
