using NeutronData.Models;
using System.Collections.Generic;


namespace NeutronEvents
{
    public class StationOrderDetailsCompleteEventArgs
    {
        public int StationNumber { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}