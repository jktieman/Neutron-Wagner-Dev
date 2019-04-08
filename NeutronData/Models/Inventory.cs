using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    [Table("Inventory")]
    public class Inventory : IEntity
    {
        public int Id { get; set; }
        public int ItemDefinitionId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool PrimeBin { get; set; }
        public int StationId { get; set; }
        public int StorageTypeId { get; set; }
        [ForeignKey("ItemDefinitionId")]
        public virtual ItemDefinition ItemDefinition { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        [ForeignKey("StorageTypeId")]
        public virtual StorageType StorageType { get; set; }
        [ForeignKey("StationId")]
        public virtual Station Station { get; set; }
    }
}
