using System.Collections.Generic;
using NeutronCore.Enums;
using NeutronData.Models;

namespace Neutron.Models
{
    public class StationOrderDetail
    {
        public StationOrderDetail()
        {
            OrderDetails = new List<OrderDetail>();
        }
        public int OrderId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public int Lines { get; set; }
        public int Pieces { get; set; }
    }
}
