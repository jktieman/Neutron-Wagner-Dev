using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronCore.Enums
{
    public enum LocationTypeEnum
    {
        [Description("Carousel")] Carousel = 1,
        [Description("Vertical")] Vertical = 2,
        [Description("Rack")] Rack = 3,
        [Description("EBin")] EBin = 4,
        [Description("Blastzone")] Blastzone = 5
    }
}
