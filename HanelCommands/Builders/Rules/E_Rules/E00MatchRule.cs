using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.E_Rules
{

    internal class E00MatchRule : IMatchRule
    {
        private const string Result = "E00";
        private const int MinCommandSegmentsLength = 4;
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            string input = string.Empty;
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
                            // If there are more than 4 segments, then the Input field must have data
                            if (commandSegments.Length > 4)
                            {
                                input = commandSegments[3];
                            }

                            //device.InMotion = true;
                            device.CommandAccepted = true;
                            //device.CurrentTray = 0;
                            device.StatusMessage = $"E00 Command Accepted  Input: {input}";
                        }
                    }
                }
            }
        }
    }
}
