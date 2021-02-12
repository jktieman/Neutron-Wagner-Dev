using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class HardwareDevice : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StationId { get; set; }
        public int DeviceNumber { get; set; }
        public int DeviceTypeId { get; set; }
        public int? CommunicationTypeId { get; set; }
        public int? TcpConfigurationId { get; set; }
        public int? SerialConfigurationId { get; set; }
        public int NumberOfCarriers { get; set; }
        public int CarrierLevel { get; set; }
        public int CarrierWidth { get; set; }
        public int CarrierDepth { get; set; }
        public bool Enabled { get; set; }
        public int LogLevel { get; set; }
        public bool SimulationMode { get; set; }

        [ForeignKey("StationId")]
        public virtual Station Station { get; set; }
        [ForeignKey("DeviceTypeId")]
        public virtual DeviceType DeviceType { get; set; }
        [ForeignKey("CommunicationTypeId")]
        public virtual CommunicationType CommunicationType { get; set; }
        [ForeignKey("TcpConfigurationId")]
        public virtual TcpConfiguration TcpConfiguration { get; set; }
        [ForeignKey("SerialConfigurationId")]
        public virtual SerialConfiguration SerialConfiguration { get; set; }

        
    }
}
