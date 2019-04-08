using System;
using System.ComponentModel;
using System.Linq;

namespace Neutron.Enums
{
    public enum OrderStatus
    {
        [Description("Available")]
        Available = 1,
        [Description("Hold")]
        Hold = 2,
        [Description("Picking")]
        Picking = 3,
        [Description("Partial")]
        Partial = 4,
        [Description("Deleted")]
        Deleted = 5,
        [Description("Complete")]
        Complete = 6,
        [Description("Returned")]
        Returned = 7,
        [Description("Archive")]
        Archive = 8,
    }
}
