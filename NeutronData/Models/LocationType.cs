using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class LocationType : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Loc1Label { get; set; }
        public string Loc2Label { get; set; }
        public string Loc3Label { get; set; }
        public string Loc4Label { get; set; }
        public string Loc5Label { get; set; }
    }
}
