using System.Collections.Generic;
using NeutronData.Models;

namespace Neutron.Models
{
    public class StationOrderDetailsCompleteEventArgs
    {
        public int StationNumber { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}