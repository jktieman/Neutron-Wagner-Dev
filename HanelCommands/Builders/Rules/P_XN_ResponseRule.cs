using System;
using System.Collections.Generic;
using System.Linq;
using HanelCommands.Builders.Interfaces;

namespace HanelCommands.Builders.Rules
{
    // ReSharper disable once InconsistentNaming
    public class P_XN_ResponseRule : IMatchRule
    {
        private const string Result = "P XN";
        private readonly IHanelResultProcessor _processor;

        public P_XN_ResponseRule(IHanelResultProcessor processor)
        {
            _processor = processor;
        }
        public bool IsMatch(string result)
        {
            return Result.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        public void ProcessResult(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList)
        {
            if (commandSegments.Length > 0 && hanelDeviceStatusList != null)
            {
                var result = commandSegments.FirstOrDefault(r => r.StartsWith("E"));
                if (result == null) return;

                _processor.Process(commandSegments, ref hanelDeviceStatusList);
            }
        }
    }
}
