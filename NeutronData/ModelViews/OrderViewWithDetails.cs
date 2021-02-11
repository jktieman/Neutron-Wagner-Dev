using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    
    public class OrderViewWithDetails : OrderView
    {
        public List<StationOrderDetail> StationOrderDetails { get; set; }
    }
}
