using System;
using System.Linq;
using System.Text;

namespace Allied2Nova.Models
{
    public class ReserveRec
    {
        public string Sku { get; set; }

        public string Slot { get; set; }

        public int Qty {get; set;}

        public ReserveRec()
        {
        }

        public ReserveRec(string sku, string slot, int qty)
        {
            Sku = sku;
            Slot = slot;
            Qty = qty;
        }

        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}|", Sku);
            sb.AppendFormat("{0}|", Slot);
            sb.AppendFormat("{0}|", Qty.ToString());
            return sb.ToString();
        }
    }
}
