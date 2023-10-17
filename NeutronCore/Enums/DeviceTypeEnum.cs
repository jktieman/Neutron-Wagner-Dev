using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum DeviceTypeEnum
    {
        [Description("None")] None = 0,
        [Description("Shuttle")] Shuttle = 1,
        [Description("Carousel")] Carousel = 2,
        [Description("Rack")] Rack = 3,
        [Description("IPTI Displays")] IptiDisplays = 4,
        Birds = 5,
        [Description("Remstar Displays")] RemstarDisplays = 6,
        [Description("Blastzone")] Blastzone = 7,
        [Description("Hanel 12D")] Hanel12D = 8,
        [Description("Hanel 12N")] Hanel12N = 9,
        [Description("Pro-Lite")] ProLite = 10,

    }
}
