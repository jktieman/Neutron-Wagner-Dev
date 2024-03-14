using System;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class AvailableOrdersView
    {
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Starter { get; set; }
        public int Priority { get; set; }
        public int Lines { get; set; }
        public int Available { get; set; }
        public int Picked { get; set; }
        public int Skipped { get; set; }
        public int Pieces { get; set; }
        public DateTime LoadDate { get; set; }
        public Order Order { get; set; }
        public int FirstStation { get; set; }
        public int NextStation { get; set; }
        public string Route { get; set; }
        public int OcLines { get; set; }
        public int OcPieces { get; set; }
    }
}
