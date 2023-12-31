using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules.A20_Rules
{
    // ReSharper disable once InconsistentNaming
    public class P_XA_ResponseRule : IMatchRule
    {
        private const string Result = "P XA";
        private readonly IHanelResultProcessor _aResultProcessor;
        private readonly IHanelResultProcessor _eResultProcessor;
        public P_XA_ResponseRule(IHanelResultProcessor aResultProcessor, IHanelResultProcessor eResultProcessor)
        {
            _aResultProcessor = aResultProcessor;
            _eResultProcessor = eResultProcessor;
        }
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                // Check for A field
                var commandA = commandSegments.FirstOrDefault(r => r.StartsWith("A"));
                if (commandA != null)
                {
                    // Use the A_ResultProcessor to determine the A number, A20, A21, A24, etc
                    _aResultProcessor.Process(commandSegments, ref hanelDeviceStatusList);
                }
                else
                {
                    // Use the E_ResultProcessor to determine the response
                    _eResultProcessor.Process(commandSegments, ref hanelDeviceStatusList);
                }
            }
        }
    }
}
