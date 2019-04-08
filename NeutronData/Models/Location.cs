using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class Location : IEntity
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public int LocationCodeId { get; set; }
        public bool InUse { get; set; }
        [ForeignKey("SizeCodeId")]
        public virtual SizeCode SizeCode { get; set; }
        [ForeignKey("VelocityCodeId")]
        public virtual VelocityCode VelocityCode { get; set; }
        [ForeignKey("HeightCodeId")]
        public virtual HeightCode HeightCode { get; set; }
        [ForeignKey("LocationCodeId")]
        public virtual LocationCode LocationCode { get; set; }
        [ForeignKey("StationId")]
        public virtual Station Station { get; set; }
    }
}
