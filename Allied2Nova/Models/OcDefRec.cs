using System;
using System.Text;

namespace Allied2Nova.Models
{
    
    public class OcDefRec : IDefRec
    {
        public string Sku { get; set; }

        public string Des { get; set; }

        public int SysCap { get; set; }

        public int SysTrig { get; set; }

        public int Station
        {
            get
            {
                return 9;
            }
            set
            {
            }
        }
       
        public int LocCap
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public int LocTrig
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public byte SizeClass
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public byte VelClass
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public byte OcOnly
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public OcDefRec()
        {
        }

        public OcDefRec(string sku, string des, int cap, int trig)
        {
            Sku = sku;
            Des = des;
            SysCap = cap;
            SysTrig = trig;
        }

        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}|", Sku);
            sb.AppendFormat("{0}|", Des);
            sb.AppendFormat("{0}|", SysCap);
            sb.Append(SysTrig);
            return sb.ToString();
        }
    }
}