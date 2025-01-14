using HanelCommands.Builders.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HanelCommands.Builders.Rules.A20_Rules;

namespace HanelCommands.Builders.A_Processors
{
    public class HanelXaProcessor : IHanelProcessor
    {
        private readonly List<IMatchRule> _matchRules = new List<IMatchRule>
        {
            new A20MatchRule(),
        };

        public void Process(byte[] dataIn, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (dataIn.Length > 0 && hanelDeviceStatusList != null)
            {
                var commandString = Encoding.UTF8.GetString(dataIn);
                var commandSegments = commandString.Split('$');

                if (commandSegments.Length > 0)
                {
                    var commandA = commandSegments.FirstOrDefault(r => r.StartsWith("A"));
                    if (commandA != null)
                    {
                        var rule = _matchRules.FirstOrDefault(r => r.IsMatch(commandA));
                        if (rule != null)
                        {
                            rule.ProcessResult(commandSegments, ref hanelDeviceStatusList);
                        }

                    }

                }
            }
        }
    }
}