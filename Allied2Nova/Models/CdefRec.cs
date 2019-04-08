using System;
using System.Linq;
using System.Text;

namespace Allied2Nova.Models
{
    public class CdefRec : IDefRec
    {
        public string Sku { get; set; }
        public string Des { get; set; }
        public int SysCap { get; set; }
        public int SysTrig { get; set; }
        public int Station { get; set; }
        public int LocCap { get; set; }
        public int LocTrig { get; set; }
        public byte SizeClass { get; set; }
        public byte VelClass { get; set; }
        public byte OcOnly { get; set; }

        public CdefRec() { }

        public CdefRec(string sku, string des, int cap, int trig, int station
            , int locCap, int locTrig, byte sizeClass, byte velClass, byte ocOnly)
        {
            this.Sku = sku;
            this.Des = des;
            this.SysCap = cap;
            this.SysTrig = trig;
            this.Station = station;
            this.LocCap = locCap;
            this.LocTrig = locTrig;
            this.SizeClass = sizeClass;
            this.VelClass = velClass;
            this.OcOnly = ocOnly;
        }

        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Sku + "|");
            sb.Append(Des + "|");
            sb.Append(SysCap + "|");
            sb.Append(SysTrig + "|");
            sb.Append(Station + "|");
            sb.Append(LocCap + "|");
            sb.Append(LocTrig + "|");
            sb.Append(SizeClass + "|");
            sb.Append(VelClass + "|");
            sb.Append(OcOnly);
            return sb.ToString();
        }
    }
}
