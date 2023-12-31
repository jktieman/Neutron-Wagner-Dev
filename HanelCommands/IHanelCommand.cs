using System.Collections.Generic;

namespace HanelCommands
{
    public interface IHanelCommand
    {
        string HostCommand { get; }
        string HostSubCommand { get; }
        /// <summary>
        /// This is the actual Serial Command to Send
        /// </summary>
        string Command { get;  }
        string[] CommandSegments { get; }
        string CommandString { get; }
        string CommandAccepted { get; }
        string CommandFailed { get; }
        string CommandBufferEmpty { get; }
        string CommandExecuted { get; }
        string DisplayText();
        List<DisplayLine> DisplayLines { get; }
    
        string Lift { get; }
        int Device { get; }
        string AccessPoint { get; }
        string Tray { get; set; }
        string Over { get; }
        string Back { get; }
        string Width { get; }
        string ScreenSize { get; }
        bool Accepted { get; set; }
        bool Failed { get; set; }
        bool Executed { get; set; }
        int TimesToFail { get; set; }
    }
}