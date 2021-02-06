using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
