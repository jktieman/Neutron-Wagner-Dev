using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules;
using HanelCommands.Builders.Rules.E_Rules;
using P_XSE05MatchRule = HanelCommands.Builders.Rules.P_XSE05MatchRule;

namespace HanelCommands.Builders
{
    public class HanelXNResultProcessor : IHanelResultProcessor
    {
        private readonly List<IMatchRule> _resultRules;

        public HanelXNResultProcessor()
        {
            _resultRules = new List<IMatchRule>
            {
                new P_XNE00MatchRule(),
                new E00MatchRule(),
                new P_XSE02MatchRule(),
                new P_XSE05MatchRule()
            };
        }

        public void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            var lift = commandSegments[0].Substring(2, 2);
            var accessPoint = commandSegments[0].Substring(4, 1);
            var device = hanelDeviceStatusList.FirstOrDefault(r => r.Device == int.Parse(lift));
            var command = commandSegments.FirstOrDefault(r => r.StartsWith("P"));
            var result = commandSegments.FirstOrDefault(r => r.StartsWith("E"));

            try
            {
                if (result != null)
                {
                    var rule = _resultRules.First(r => r.IsMatch(result));
                    rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
