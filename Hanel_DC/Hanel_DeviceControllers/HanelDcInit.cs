namespace Hanel_DC.Hanel_DeviceControllers
{
    public struct HanelDcInit
    {
        public object CallersObj { get; set; }

        public int PercentComplete { get; set; }

        public bool Success { get; set; }

        public HanelDcInit(object callersObject, int percentageComplete, bool successful)
        {
            this.CallersObj = callersObject;
            this.PercentComplete = percentageComplete;
            this.Success = successful;
        }
    }
}

