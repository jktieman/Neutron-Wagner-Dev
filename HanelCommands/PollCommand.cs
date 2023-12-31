using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    public class PollCommand : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        public string HostCommand { get; }
        public string HostSubCommand { get; set; }
        public string Command => $"{AST}{CR}{LF}";
        public string[] CommandSegments { get; }
        public string CommandString { get; }
        public string CommandAccepted { get; }
        public string CommandFailed { get; }
        public string CommandBufferEmpty { get; }
        public string CommandExecuted { get; }
        public string DisplayText()
        {
            var text = string.Empty;
            foreach (var line in DisplayLines)
            {
                text += $"X{line}";
            }
            return text;
        }
        public List<DisplayLine> DisplayLines { get; }
        public string Lift { get; } = "0";
        public int Device => int.Parse(Lift);
        public string AccessPoint { get; }
        public string Tray { get; set; }
        public string Over { get; }
        public string Back { get; }
        public string Width { get; set; }
        public string ScreenSize { get; }
        public bool Accepted { get; set; }
        public bool Failed { get; set; }
        public bool Executed { get; set; }
        public int TimesToFail { get; set; }
    }
}
