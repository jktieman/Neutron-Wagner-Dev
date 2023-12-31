using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules
{
    /// <summary>
    /// Start up message to the HOST
    /// E00 Indicates the lift is Switched On
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class P_XSE05MatchRule : IMatchRule
    {
        private const string Result = "E05";
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
                            device.CommandAccepted = false;
                            device.StatusMessage = "Command is not possible";
                        }
                    }
                }
            }
        }
    }
}
