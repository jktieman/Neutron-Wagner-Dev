using System.ComponentModel;

namespace Neutron.Enums

{
    public enum DeviceResponse
    {
        [Description("Success")]
        Success = 0,
        [Description("Previous Tray Did Not Arrive, please try again.")]
        TrayDidNotArrive = 1,
        [Description("Unknown Failure")]
        UnknownFailure = 2,
        [Description("Device Not Initialized")]
        DeviceNotInitialized = 3,
        [Description("Device not Enabled")]
        DeviceNotEnabled = 4,
        [Description("Device Not Found")]
        DeviceNotFound = 5,
        [Description("Bad Status")]
        DeviceBadStatus = 6,
        [Description("Device In Motion")]
        DeviceInMotion = 7
    }
}
