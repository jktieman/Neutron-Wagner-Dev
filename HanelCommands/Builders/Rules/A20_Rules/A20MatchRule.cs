using System;
using System.Collections.Generic;
using HanelCommands.Builders.A_Processors;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.A20_Rules
{
    internal class A20MatchRule : IMatchRule
    {
        private const string Result = "A20";
        private readonly A20EProcessor  _processor = new A20EProcessor();
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
