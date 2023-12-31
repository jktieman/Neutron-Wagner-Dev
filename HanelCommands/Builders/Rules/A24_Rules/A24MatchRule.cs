using System;
using System.Collections.Generic;
using HanelCommands.Builders.A_Processors;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.A24_Rules
{
    internal class A24MatchRule : IMatchRule
    {
        private const string Result = "A24";
        private readonly HanelXAResultProcessor _processor = new HanelXAResultProcessor();
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                _processor.Process(commandSegments, ref hanelDeviceStatusList);
            }
        }
    }
}
