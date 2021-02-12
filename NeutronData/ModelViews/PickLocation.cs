using NeutronData.Models;
using System;

namespace NeutronData.ModelViews
{
    public class PickLocation
    {
        public int RequestedQuantity { get; set; }
        public int Quantity { get; set; }
        public Inventory Inventory { get; set; }
        public DateTime PickDate { get; set; }
        public User User { get; set; }
    }
}
