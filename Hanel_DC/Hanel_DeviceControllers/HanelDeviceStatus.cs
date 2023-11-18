using HanelCommands;
using System;
using System.Security.AccessControl;
using System.Text;

namespace Hanel_DC.Hanel_DeviceControllers
{
    public class HanelDeviceStatus
    {
        public HanelDeviceStatus(int deviceNumber,
            bool newGoodStatus = false,
            DateTime newLastStatus = default,
            DateTime newLastCommand = default,
            int newDevice = 0,
            int newTargetTray = 0,
            int newCurrentTray = 0,
            bool newInMotion = false,
            bool newInAlignment = false,
            string newStatusMessage = "not initialized",
            bool commandAccepted = false,
            bool commandExecuted = false
        )
        {
            DeviceNumber = deviceNumber;
            GoodStatus = newGoodStatus;
            LastStatus = newLastStatus;
            LastCommand = newLastCommand;
            Device = newDevice;
            TargetTray = newTargetTray;
            CurrentTray = newCurrentTray;
            InMotion = newInMotion;
            InAlignment = newInAlignment;
            StatusMessage = newStatusMessage;
            ActiveErrorCount = 0;
            CommandAccepted = commandAccepted;
            CommandExecuted = commandExecuted;
        }

        public int DeviceNumber { get; set; }
        public IHanelCommand LastHanelCommand { get; set; }
        public IHanelCommand PreviousHanelCommand { get; set; }
        public string DisplayText { get; set; }
        public string Lift { get; set; }
        public string AccessPoint { get; set; }
        public string Tray { get; set; }
        public string Over { get; set; }
        public string Back { get; set; }
        public bool SwitchedOn { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Lift: {DeviceNumber}  Tray: {Tray}  Over: {Over}  Back: {Back} {Environment.NewLine}");
            if (LastHanelCommand != null)
            {
                sb.AppendLine($"LastCommand: {LastHanelCommand.Command} {Environment.NewLine}");
            }

            return sb.ToString();
        }


        public bool GoodStatus { get; set; }
        public DateTime LastStatus { get; set; }
        public DateTime LastCommand { get; set; }
        public int Device { get; set; }
        public int TargetTray { get; set; }
        public int CurrentTray { get; set; }
        public bool InMotion { get; set; }
        public bool InAlignment { get; set; }
        public string StatusMessage { get; set; }
        public int ActiveErrorCount { get; set; }
        public bool CommandAccepted { get; set; }
        public bool CommandExecuted { get; set; }
    }
}