using System.ComponentModel;

namespace Neutron.Enums

{
    public enum DeviceResponse
    {
        [Description("Success")]
        Success = 0,
        [Description("Previous Bin/Tray Did Not Arrive, please try again.")]
        TrayDidNotArrive = 1,
        [Description("Unknown Failure")]
        UnknownFailure = 2,
        [Description("Hardware Device Not Initialized")]
        DeviceNotInitialized = 3,
        [Description("Hardware Device not Enabled")]
        DeviceNotEnabled = 4,
        [Description("Hardware Device Not Found")]
        DeviceNotFound = 5,
        [Description("Bad Status")]
        DeviceBadStatus = 6,
        [Description("Hardware Device In Motion")]
        DeviceInMotion = 7
    }
}
