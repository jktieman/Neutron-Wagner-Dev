using HanelCommands.Builders.Interfaces;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Rules.A24_Rules;

namespace HanelCommands.Builders.A_Processors
{
    internal class A24EProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _commandRules = new List<IMatchRule>
        {
            new A24E00MatchRule(),
            new A24E01MatchRule(),
            new A24E02MatchRule(),
        };

        public void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                var commandE = commandSegments.FirstOrDefault(r => r.StartsWith("E"));
                if (commandE == null) return;
                var rule = _commandRules.FirstOrDefault(r => r.IsMatch(commandE));
                if (rule == null) return;
                rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
            }
        }
    }
}