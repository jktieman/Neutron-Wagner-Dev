
using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum LineStatus
    {
        [Description("Available")] Available = 1,
        [Description("Hold")] Hold = 2,
        [Description("Picking")] Picking = 3,
        [Description("Partial")] Partial = 4,
        [Description("Deleted")] Deleted = 5,
        [Description("Complete")] Complete = 6,
        [Description("Returned")] Returned = 7,
        [Description("Archive")] Archive = 8,
        [Description("Skipped")] Skipped = 9,
        [Description("Kill")] Kill = 10
    }
}