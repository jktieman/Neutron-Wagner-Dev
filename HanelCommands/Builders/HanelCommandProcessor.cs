using System.Collections.Generic;
using System.Linq;
using System.Text;
using HanelCommands.Builders.A_Processors;
using HanelCommands.Builders.E_Processors;
using HanelCommands.Builders.Interfaces;
using HanelCommands.Builders.Rules;
using HanelCommands.Builders.Rules.A20_Rules;

namespace HanelCommands.Builders
{
    public class HanelCommandProcessor : IHanelProcessor
    {
        private readonly List<IMatchRule> _commandRules = new List<IMatchRule>
        {
            new P_XN_ResponseRule(new HanelResultProcessor()),
            new P_XS_ResponseRule( new HanelResultProcessor()),
            new P_XA_ResponseRule( new A_ResultProcessor(), new E_ResultProcessor()),
        };

        public void Process(byte[] dataIn, List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (dataIn.Length > 0 && hanelDeviceStatusList != null)
            {
                var commandString = Encoding.UTF8.GetString(dataIn);
                var commandSegments = commandString.Split('$');

                if (commandSegments.Length > 0)
                {
                    var commandP = commandSegments.FirstOrDefault(r => r.StartsWith("P"));
                    if (commandP != null)
                    {
                        var rule = _commandRules.FirstOrDefault(r => r.IsMatch(commandP));
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
