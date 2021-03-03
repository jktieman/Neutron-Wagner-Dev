using NeutronData.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models.Lookups
{
    [Table("LineStatusLookup")]
    public class  LineStatusLookup : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
