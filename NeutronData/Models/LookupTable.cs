using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class LookupTable : IEntity
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string TableName { get; set; }
    }
}
