using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum StationType
    {
        [Description("Carousel")]
        Carousel = 1,
        [Description("Vertical")]
        Vertical = 2,
        [Description("Rack-Tablet")]
        RackTablet = 3,
        [Description("Supervisor")]
        Supervisor = 4,
        [Description("EBin")]
        EBin = 5,
        [Description("Rack-Paper")]
        RackPaper = 6,
        [Description("Blastzone")]
        Blastzone = 7
    }
}
