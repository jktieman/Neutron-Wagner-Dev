using System;
using System.Linq;

namespace Allied2Nova.Models
{
    public interface IDefRec
    {
        string Sku { get; set; }
        string Des { get; set; }
        int SysCap { get; set; }
        int SysTrig { get; set; }
        int Station { get; set; }
        int LocCap { get; set; }
        int LocTrig { get; set; }
        byte SizeClass { get; set; }
        byte VelClass { get; set; }
        byte OcOnly { get; set; }
    }
}
