using System.Collections.Generic;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class StationOrderDetail
    {
        public StationOrderDetail()
        {
            OrderDetails = new List<OrderDetail>();
        }
        public int OrderId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public string OrderStatus { get; set; }
        public int Lines { get; set; }
        public int Pieces { get; set; }
        public Workstation PickStation { get; set; }
        public int PickStationNumber => PickStation.StationNumber;
    }
}
