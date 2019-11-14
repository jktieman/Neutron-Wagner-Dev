using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class ActionCodeItem : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
