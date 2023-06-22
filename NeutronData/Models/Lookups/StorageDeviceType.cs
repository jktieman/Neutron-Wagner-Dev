using NeutronData.Interfaces;

namespace NeutronData.Models.Lookups
{
    public class StorageDeviceType : IEntity, ILookup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
