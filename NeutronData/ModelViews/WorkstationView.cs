using NeutronData.Models;
using NeutronData.Models.Lookups;
using ProliteController;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using IBlastzone = NeutronData.Interfaces.IBlastzone;


namespace NeutronData.ModelViews
{
    public class WorkstationView
    {
        public IBlastzone Blastzone { get; set; }
        public ProliteManager ProliteManager { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationView()
        {
            HardwareDevices = new List<HardwareDevice>();
        }
        public int WorkstationId { get; set; }
        public int WorkstationNumber { get; set; }
        public string Name { get; set; }
        public StationType StationType { get; set; }
        public int StationTypeId { get; set; }
        public Area Area { get; set; }
        public int AreaId { get; set; }
        public int Sequence { get; set; }
        public List<HardwareDevice> HardwareDevices { get; set; }
        public IReadOnlyCollection<int> EnabledDevices
        {
            get
            {
                var list = new List<int>();
                foreach (var item in HardwareDevices)
                {
                    if (item.Enabled && (item.DeviceTypeId == 1 || item.DeviceTypeId == 2))
                    {
                        list.Add(item.DeviceNumber);
                    }
                }
                return new ReadOnlyCollection<int>(list);
            }
        }
        
        public override string ToString()
        {
           return $"{Name}  Area: {Area.Name}";
        }
    }
}
