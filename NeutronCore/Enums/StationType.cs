using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum StationType
    {
        [Description("Carousel")]
        Carousel = 1,
        [Description("Vertical")]
        Vertical = 2,
        [Description("Rack")]
        Rack = 3,
        [Description("Supervisor")]
        Supervisor = 4
    }
}
