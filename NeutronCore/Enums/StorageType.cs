using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronCore.Enums
{
    public enum StorageType
    {
        [Description("Static")]
        Static = 1,
        [Description("Release")]
        Release = 2,
        [Description("Non-Pickable")]
        NonPickable = 3,
        [Description("Kit")]
        Kit = 4
    }
}
