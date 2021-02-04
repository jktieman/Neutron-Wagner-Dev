using NeutronData.Interfaces;

namespace NeutronData.Models.Lookups
{
    public class Language : ILookup, IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CultureInfo { get; set; }
        public int Sequence { get; set; }
    }
}
