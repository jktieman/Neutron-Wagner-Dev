using System;
using System.Collections.Generic;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class SkipView
    {
        public SkipView()
        {
            PickLocations = new List<PickLocation>();
        }
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string Priority { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string OrderStatusName { get; set; }
        public int Quantity { get; set; }
        public int Picked { get; set; }
        public DateTime LoadDate { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public List<PickLocation> PickLocations { get; set; }
    }
}
