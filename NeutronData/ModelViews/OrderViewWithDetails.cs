using System.Collections.Generic;

namespace NeutronData.ModelViews
{
    
    public class OrderViewWithDetails : OrderView
    {
        public List<StationOrderDetail> StationOrderDetails { get; set; }
    }
}
