using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.A20_Rules
{
    /// <summary>
    /// Command was executed
    /// </summary>
    internal class A20E00MatchRule : IMatchRule
    {
        private const string Result = "E00";
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
                            device.CommandExecuted = true;
                            device.InMotion = false;
                            device.StatusMessage = "XA A20 E00 Command was Executed";
                        }
                    }
                }
            }
        }
    }
}
