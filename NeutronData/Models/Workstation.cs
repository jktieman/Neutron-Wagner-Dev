using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NeutronData.Models.Lookups;

using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class Workstation : ILookup, IEntity
    {
        [Key, Required]
        public int Id { get; set; }
        [Required]
        public int StationNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int StationTypeId { get; set; }

        public int AreaId { get; set; }

        public int Sequence { get; set; }

        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }
        
        [ForeignKey("StationTypeId")]
        public virtual StationType StationType { get; set; }
        
        public List<HardwareDevice> HardwareDevices { get; set; } = new List<HardwareDevice>();


    }
}
