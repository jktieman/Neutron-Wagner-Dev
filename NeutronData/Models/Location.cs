using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class Location : IEntity
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public int PickSequence { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public string LocationCode { get; set; }
        public bool InUse { get; set; }
        
        [ForeignKey("SizeCodeId")]
        public virtual SizeCode SizeCode { get; set; }
        
        [ForeignKey("VelocityCodeId")]
        public virtual VelocityCode VelocityCode { get; set; }
        
        [ForeignKey("HeightCodeId")]
        public virtual HeightCode HeightCode { get; set; }
        
        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }
    }
}
