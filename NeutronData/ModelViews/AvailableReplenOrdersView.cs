using System;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class AvailableReplenOrdersView
    {
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Starter { get; set; }
        public int Priority { get; set; }
        public int Lines { get; set; }
        public int Pieces { get; set; }
        public DateTime LoadDate { get; set; }
        public ReplenOrder Order { get; set; }
        public int FirstStation { get; set; }
        public int NextStation { get; set; } 
    }
}