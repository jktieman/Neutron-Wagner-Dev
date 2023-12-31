using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules;
using HanelCommands.Builders.Rules.E_Rules;

namespace HanelCommands.Builders
{
    // ReSharper disable once InconsistentNaming
    public class HanelXSResultProcessor : IHanelResultProcessor
    {

        private readonly List<IMatchRule> _resultRules = new List<IMatchRule>
        {
            new E00MatchRule(),
            new P_XSE02MatchRule(),
            new P_XSE05MatchRule()
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
