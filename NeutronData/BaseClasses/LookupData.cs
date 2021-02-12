using NeutronData.Interfaces;

namespace NeutronData.BaseClasses
{
    public class LookupData : ILookup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
