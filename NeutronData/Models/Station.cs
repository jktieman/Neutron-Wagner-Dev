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
    public class Station : ILookup, IEntity
    {
       // public Station() { }
        //{
        //   // HardwareDevices = new List<HardwareDevice>();
        //}
        public int Id { get; set; }
        public int StationNumber { get; set; }
        public string Name { get; set; }
        public int? CommunicationTypeId { get; set; }
        public int? TcpConfigurationId { get; set; }
        public int? SerialConfigurationId { get; set; }
        public int Sequence { get; set; }
        [ForeignKey("CommunicationTypeId")]
        public virtual CommunicationType CommunicationType { get; set; }
        public virtual List<HardwareDevice> HardwareDevices { get; set; }
    }
}
