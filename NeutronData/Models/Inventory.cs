#nullable enable
using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("Inventory")]
    public class Inventory : Expiration, IEntity
    {
        public int Id { get; set; }
        public int ItemDefinitionId { get; set; }
        public int LocationId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool PrimeBin { get; set; }
        public int AreaId { get; set; }
        [MaxLength(50)]
        public string? RFID { get; set; }
        public int StorageTypeId { get; set; }
        [ForeignKey("ItemDefinitionId")]
        public virtual ItemDefinition? ItemDefinition { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }
        [ForeignKey("StorageTypeId")]
        public virtual StorageType? StorageType { get; set; }
        [ForeignKey("AreaId")]
        public virtual Area? Area { get; set; }
    }
}
