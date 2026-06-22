using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintReplenishments.Models
{
    public class Replen
    {
        {
            Locations = new List<v_inv_for_carousel>();
        }
    public string StationNumber { get; set; }
    public string Sku { get; set; }
    public int Qty { get; set; }
    public List<v_inv_for_carousel> Locations { get; set; }
}
}
