using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum DeviceType
    {
        [Description("None")]
        None = 0,
        [Description("Shuttle")]
        Shuttle = 1,
        [Description("Carousel")]
        Carousel = 2,
        [Description("Rack")]
        Rack = 3,
        [Description("IPTI Displays")]
        IptiDisplays = 4,
        [Description("Remstar Displays")]
        RemstarDisplays = 5
    }
}
