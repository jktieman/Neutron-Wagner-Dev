using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules.A12_Rules;
using HanelCommands.Builders.Rules.A20_Rules;
using HanelCommands.Builders.Rules.A21_Rules;
using HanelCommands.Builders.Rules.A24_Rules;

namespace HanelCommands.Builders.A_Processors
{
    // ReSharper disable once InconsistentNaming
    internal class A_ResultProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _commandRules = new List<IMatchRule>
        {
            new A12MatchRule(),
            new A20MatchRule(),
            new A21MatchRule(),
            new A24MatchRule(),
        };

        public void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                var commandA = commandSegments.FirstOrDefault(r => r.StartsWith("A"));
                if (commandA == null) return;

                var rule = _commandRules.FirstOrDefault(r => r.IsMatch(commandA));
                if (rule == null) return;
                rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
            }
        }
    }
}