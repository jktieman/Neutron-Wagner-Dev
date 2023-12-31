using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules.A21_Rules;

namespace HanelCommands.Builders.A_Processors
{
    // ReSharper disable once InconsistentNaming
    public class HanelXAResultProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _resultRules = new List<IMatchRule>
        {
            new A21E01MatchRule()
        };

        public void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                var result = commandSegments.FirstOrDefault(r => r.StartsWith("E"));
                if (result != null)
                {
                    var rule = _resultRules.First(r => r.IsMatch(result));
                    rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
                }
            }
        }
    }
}

