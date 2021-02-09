using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronCore.Enums;
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
        public Station PickStation { get; set; }
        public int PickStationNumber => PickStation.StationNumber;
    }
}
