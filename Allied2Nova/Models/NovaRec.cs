using System;
using System.Linq;
using System.Text;

namespace Allied2Nova.Models
{
    public class NovaRec
    {

        public string Sku { get; set; }
        public string Des { get; set; }
        public int Qty { get; set; }
        public int Station { get; set; }

        public NovaRec()
        { }

        public NovaRec(string sku, string des, int qty, int station)
        {
            this.Sku = sku;
            this.Des = des;
            this.Qty = qty;
            this.Station = station;
        }

        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Sku + "|");
            sb.Append(Des + "|");
            sb.Append(Qty + "|");
            sb.Append(Station + "|");
            sb.Append(Station);
            return sb.ToString();
        }
    }
}
