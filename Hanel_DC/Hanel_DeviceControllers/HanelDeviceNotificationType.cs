using System;
using System.Threading;

namespace Hanel_DC.Hanel_DeviceControllers
{
    public class HanelDeviceNotificationType
    {
        public Guid Unique_ID { get; set; }

        public SynchronizationContext CallersContext { get; set; }

        public object CallersObject { get; set; }

        public int TimeOutSeconds { get; set; }

        public DateTime Registered { get; set; }

        public DateTime Expiry { get; set; }

        public HanelTellMeWhenTrayArrives CallBack { get; set; }

        public int TargetDevice { get; set; }

        public int TargetTray { get; set; }

        public int RequestedMotionStatus { get; set; }

        public int RequestedAlignmentStatus { get; set; }

        public bool MotionStatusUponNotification { get; set; }

        public bool AlignmentStatusUponNotification { get; set; }

        public bool AutoDeregister { get; set; }

        public bool Expired { get; set; }

        public string Message { get; set; }

        public HanelDeviceNotificationType(
          Guid Unique_ID,
          SynchronizationContext CallersContext,
          object CallersObject,
          int TimeOutSeconds,
          DateTime Registered,
          DateTime Expiry,
          HanelTellMeWhenTrayArrives CallBack,
          int TargetDevice,
          int TargetTray,
          int InMotion,
          int InAlignment,
          bool AutoDeregister)
        {
            this.Unique_ID = Unique_ID;
            this.CallersContext = CallersContext;
            this.CallersObject = CallersObject;
            this.TimeOutSeconds = TimeOutSeconds;
            this.Registered = Registered;
            this.Expiry = Expiry;
            this.CallBack = CallBack;
            this.TargetDevice = TargetDevice;
            this.TargetTray = TargetTray;
            this.RequestedMotionStatus = InMotion;
            this.RequestedAlignmentStatus = InAlignment;
            this.MotionStatusUponNotification = false;
            this.AlignmentStatusUponNotification = false;
            this.AutoDeregister = AutoDeregister;
            this.Expired = false;
            this.Message = "";
        }
    }
}
