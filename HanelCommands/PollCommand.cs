using System;
using System.Collections.Generic;

namespace HanelCommands
{
    public class PollCommand : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        public string HostCommand { get; } = string.Empty;
        public string HostSubCommand { get; set; } = string.Empty;
        public string Command => $"{AST}{CR}{LF}";
        public string[] CommandSegments { get; } = new string[0];
        public string CommandString { get; } = string.Empty;
        public string CommandAccepted { get; } = string.Empty;
        public string CommandFailed { get; } = string.Empty;
        public string CommandBufferEmpty { get; } = string.Empty;
        public string CommandExecuted { get; } = string.Empty;
        public string DisplayText()
        {
            var text = string.Empty;
            foreach (var line in DisplayLines)
            {
                text += $"X{line}";
            }
            return text;
        }
        public List<DisplayLine> DisplayLines { get; } = new List<DisplayLine>();
        public string Lift { get; } = "0";
        public int Device => int.TryParse(Lift, out var d) ? d : 0;
        public string AccessPoint { get; } = string.Empty;
        public string Tray { get; set; } = string.Empty;
        public string Over { get; } = string.Empty;
        public string Back { get; } = string.Empty;
        public string Width { get; set; } = string.Empty;
        public string ScreenSize { get; } = string.Empty;
        public bool Accepted { get; set; }
        public bool Failed { get; set; }
        public bool Executed { get; set; }
        public int TimesToFail { get; set; }
    }
}
