using HanelCommands.Builders.Interfaces;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Rules.A20_Rules;

namespace HanelCommands.Builders.A_Processors
{
    internal class A20EProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _commandRules = new List<IMatchRule>
        {
            new A20E00MatchRule(),
            new A20E01MatchRule(),
            new A20E02MatchRule(),
            new A20E96MatchRule(),
        };

        public void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                var commandE = commandSegments.FirstOrDefault(r => r.StartsWith("E"));
                if (commandE == null) return;
                var rule = _commandRules.FirstOrDefault(r => r.IsMatch(commandE));
                if(rule == null) return;
                rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
            }
        }
    }
}