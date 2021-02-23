using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class Station : ILookup, IEntity
    {
        [Key, Required]
        public int Id { get; set; }
        [Required]
        public int StationNumber { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int StationTypeId { get; set; }
        public int? TcpConfigurationId { get; set; }
        public int? SerialConfigurationId { get; set; }
        public int Sequence { get; set; }
        [ForeignKey("StationTypeId")]
        public virtual StationType StationType { get; set; }
        public virtual List<HardwareDevice> HardwareDevices { get; set; }
    }
}
