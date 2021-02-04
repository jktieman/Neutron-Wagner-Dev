using NeutronData.Models;
using NeutronData.Models.Lookups;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NeutronData.ModelViews
{
    public class StationView
    {
        public StationView()
        {
            HardwareDevices = new List<HardwareDevice>();
        }
        public int StationId { get; set; }
        public int StationNumber { get; set; }
        public string Name { get; set; }
        public StationType StationType { get; set; }
        //public int? CommunicationTypeId { get; set; }
        //public string CommunicationTypeName { get; set; }
        //public TcpConfiguration TcpConfiguration { get; set; }
        //public int? TcpConfigurationId { get; set; }
        //public string TcpConfigurationName { get; set; }
        //public SerialConfiguration SerialConfiguration { get; set; }
        //public int? SerialConfigurationId { get; set; }
        //public string SerialConfigurationName { get; set; }
        public int Sequence { get; set; }
        public List<HardwareDevice> HardwareDevices { get; set; }
        //public HardwareDevice CurrentShuttle { get; set; }
        public IReadOnlyCollection<int> EnabledDevices
        {
            get
            {
                var list = new List<int>();
                foreach (var item in HardwareDevices)
                {
                    if (item.Enabled)
                    {
                        list.Add(item.DeviceNumber);
                    }
                }
                return new ReadOnlyCollection<int>(list);
            }
        }
    }
}
