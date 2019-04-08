using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    [Table("LocationView")]
    public class LocationView
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public bool InUse { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public int LocationCodeId { get; set; }
        public string SizeCodeName { get; set; }
        public string VelocityCodeName { get; set; }
        public string HeightCodeName { get; set; }
        public string LocationCodeName { get; set; }
        public string StationName { get; set; }
        public string DeviceName { get; set; }
    }
}
