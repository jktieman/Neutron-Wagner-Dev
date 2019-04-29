using System.ComponentModel;

namespace Neutron.Enums

{
    public enum StorageType
    {
        [Description("Static")]Static = 1,
        [Description("Release")]Release = 2,
        [Description("Non-Pickable")]NonPickable = 3
    }
}
