using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.A12_Rules
{
    /// <summary>
    /// Lift is Full
    /// </summary>
    internal class A12E03MatchRule : IMatchRule
    {
        private const string Result = "E03";
        private const int MinCommandSegmentsLength = 4;
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                if (commandSegments[0].Length >= MinCommandSegmentsLength)
                {
                    var lift = commandSegments[0].Substring(2, 2);
                    if (int.TryParse(lift, out int liftNumber))
                    {
                        var device = hanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == liftNumber);
                        if (device != null)
                        {
                            device.CurrentTray = 0;
                            device.StatusMessage = "XA A12 E03 Unknown Shelf in Access Point";
                            device.CommandSent = false;
                            device.CommandAccepted = false;
                            device.CommandExecuted = true;
                        }
                    }
                }
            }
        }
    }
}
