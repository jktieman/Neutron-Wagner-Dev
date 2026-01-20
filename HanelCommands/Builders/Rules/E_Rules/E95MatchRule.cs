using HanelCommands.Builders.Interfaces;
using NeutronEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands.Builders.Rules.E_Rules
{
    internal class E95MatchRule : IMatchRule
    {
        private const string Result = "E95";
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
            try
            {
                var message = segments.FirstOrDefault(r => r.StartsWith("S"));
                if (message == null) return string.Empty;
                sb.AppendLine($"TOWER {lift}");
                sb.AppendLine();
                sb.AppendLine("Lift run was interrupted due to contents of the data field are not permitted.");
                sb.AppendLine(" E95 Contents of Data Field is not permitted.");
                sb.AppendLine("Press the Down Arrow/Get Tray to retrieve tray manually.");

                sb.AppendLine(message);
                
                var x = segments.Where(s => s.StartsWith("X")).ToList();
                foreach (var segment in x)
                {
                    sb.AppendLine(segment.Substring(6).Trim());
                }

            }
            catch (Exception)
            {
                throw;
            }
            return sb.ToString();

        }
    }
}
