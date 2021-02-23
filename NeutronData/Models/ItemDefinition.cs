using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class ItemDefinition : IEntity
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int UnitOfIssueId { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public int LocationCodeId { get; set; }
        public int LocationMax { get; set; }
        public int LocationMin { get; set; }
        public int SystemMax { get; set; }
        public int SystemMin { get; set; }
        public int StorageTypeId { get; set; }
        public float Weight { get; set; }
        public bool Scale { get; set; }
        [ForeignKey("StorageTypeId")]
        public virtual StorageType StorageType { get; set; }
        [ForeignKey("UnitOfIssueId")]
        public virtual UnitOfIssue UnitOfIssue { get; set; }
        [ForeignKey("SizeCodeId")]
        public virtual SizeCode SizeCode { get; set; }
        [ForeignKey("VelocityCodeId")]
        public virtual VelocityCode VelocityCode { get; set; }
        [ForeignKey("HeightCodeId")]
        public virtual HeightCode HeightCode { get; set; }
        [ForeignKey("LocationCodeId")]
        public virtual LocationCode LocationCode { get; set; }
       // [ForeignKey("StationId")]
        public virtual Station Station { get; set; }

    }
}
