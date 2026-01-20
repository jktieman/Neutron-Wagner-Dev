using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HanelCommands.Builders.Interfaces;
using NeutronEvents;

namespace HanelCommands.Builders.Rules
{
    /// <summary>
    /// Start up message to the HOST
    /// E00 Indicates the lift is Switched On
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class P_XNE00MatchRule : IMatchRule
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
                            //device.CommandAccepted = false;
                            //device.CommandExecuted = false;
                            device.SwitchedOn = true;
                            device.StatusMessage = CreateStatusMessage(lift, commandSegments);
                            Mediator.GetInstance().OnDisplayMessage(this, device.StatusMessage);
                        }
                    }
                }
            }
        }
        private string CreateStatusMessage(string lift, string[] segments)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"TOWER {lift}");
            sb.AppendLine();
            sb.AppendLine($"Tower {lift} Switched On");
            sb.AppendLine();
            var x = segments.Where(s => s.StartsWith("X")).ToList();
            foreach (var segment in x)
            {
                sb.AppendLine(segment.Substring(6).Trim());
            }

            return sb.ToString();

        }
    }
}
