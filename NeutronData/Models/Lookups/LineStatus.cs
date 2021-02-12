using NeutronData.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models.Lookups
{
    [Table("LineStatus")]
    public class  LineStatus : ILookup, IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }
}
