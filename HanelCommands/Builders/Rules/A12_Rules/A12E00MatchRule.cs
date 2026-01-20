using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;
using NeutronCore.Extensions;
using NeutronEvents;

namespace HanelCommands.Builders.Rules.A12_Rules
{
    /// <summary>
    /// Command was executed
    /// </summary>
    internal class A12E00MatchRule : IMatchRule
    {
        private const string Result = "E00";
        private const int MinCommandSegmentsLength = 4;
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {

            var tray = 0;
            var b = false;  
            
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                if (commandSegments[0].Length >= MinCommandSegmentsLength)
                {
                     var commandT = commandSegments.FirstOrDefault(r => r.StartsWith("T"));
                    if (commandT != null)
                    {
                        b = int.TryParse(commandT.Substring(1),out tray);
                    }
                    if (!b) return;
                    {
                        var lift = commandSegments[0].Substring(2, 2);
                        if (int.TryParse(lift, out int liftNumber))
                        {
                            var device = hanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == liftNumber);
                            if (device != null)
                            {
                                device.CurrentTray = tray;
                                Mediator.GetInstance().OnTrayInPosition(this
                                    , new InPositionInfo
                                    {
                                        Lift = lift.ParseInt(),
                                        Tray = tray
                                    });
                                device.StatusMessage = $"Status Message -XA A12 E00 Command was Executed, Lift: {lift}";
                                device.CommandSent = false;
                                device.CommandAccepted = false;
                                device.CommandExecuted = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
