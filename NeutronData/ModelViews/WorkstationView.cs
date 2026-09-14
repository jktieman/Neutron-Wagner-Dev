using NeutronData.Models;
using ProliteController;
using NeutronData.Models.Lookups;
using NeutronData.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NeutronData.ModelViews;

public class WorkstationView
{
    public IBlastzone Blastzone { get; set; }
    public IProLiteManager ProLiteManager { get; set; }
    public Workstation Workstation { get; set; }
    public int WorkstationId { get; set; }
    public int WorkstationNumber { get; set; }
    public string Name { get; set; }
    public StationType StationType { get; set; }
    public int StationTypeId { get; set; }
    public Area Area { get; set; }
    public int AreaId { get; set; }
    public int Sequence { get; set; }
    public List<HardwareDevice> HardwareDevices { get; set; } = [];
    public List<HardwareDevice> Hanels { get; set; } = [];
    public List<HardwareDevice> CarouselShuttles { get; set; } = [];
    public List<HardwareDevice> Prolites { get; set; } = [];
    public List<HardwareDevice> Blastzones { get; set; } = [];
    public HardwareDevice BatchTable { get; set; } = new();

    public IReadOnlyCollection<int> EnabledDevices
    {
        get
        {
            var list = new List<int>();
            foreach (var item in HardwareDevices)
            {
                if (item.Enabled && item.DeviceTypeId is 1 or 2)
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