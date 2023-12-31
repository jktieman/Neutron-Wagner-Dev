using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules
{
    /// <summary>
    /// Start up message to the HOST
    /// E00 Indicates the lift is Switched On
    /// </summary>
    internal class PXsE02MatchRule : IMatchRule
    {
        private const string Result = "E02";
        public bool IsMatch(string result)
        {
            return Result.Equals(result);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            var lift = commandSegments[0].Substring(2, 2);
            var device = hanelDeviceStatusList.FirstOrDefault(r => r.DeviceNumber == int.Parse(lift));
            if (device != null)
            {
                device.CommandAccepted = false;
                device.StatusMessage = "Command is NOT accepted";
            }
        }
    }
}
