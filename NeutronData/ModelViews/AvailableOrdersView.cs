using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class AvailableOrdersView
    {
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string Starter { get; set; }
        public int Priority { get; set; }
        public int Lines { get; set; }
        public int Pieces { get; set; }
        public DateTime LoadDate { get; set; }
        public Order Order { get; set; }
    }
}
