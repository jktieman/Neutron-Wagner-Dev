using NeutronData.Interfaces;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
