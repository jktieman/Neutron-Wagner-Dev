using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules.E_Rules;

namespace HanelCommands.Builders.E_Processors
{
    // ReSharper disable once InconsistentNaming
    internal class E_ResultProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _commandRules = new List<IMatchRule>
        {
            new E00MatchRule(),
            new E02MatchRule(),
            new E05MatchRule(),
            new E95MatchRule(),
            new E97MatchRule(),
            new E98MatchRule(),
            new E99MatchRule(),
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