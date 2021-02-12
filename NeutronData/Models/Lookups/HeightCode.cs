using NeutronData.Interfaces;

namespace NeutronData.Models.Lookups
{
    public class HeightCode : ILookup, IEntity 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
