using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class Area : IEntity
    {
        [Key, Required]
        public int Id { get; set; }
        public int AreaNumber { get; set; }
        public string Name { get; set; }
        public int LocationTypeId { get; set; }
        public string Description { get; set; }
        public bool Pickable { get; set; }
        [ForeignKey("LocationTypeId")]
        public virtual LocationType LocationType { get; set; }
        //public virtual ICollection<StorageDevice> Devices { get; set; }=new List<StorageDevice>();

    }
}
