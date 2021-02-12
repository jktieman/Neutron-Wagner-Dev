using NeutronData.Interfaces;

namespace NeutronData.Models.Lookups
{
    public class UnitOfIssue : ILookup, IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
