using System;
using System.Collections.Generic;
using System.Linq;


namespace HanelCommands
{

    public class HanelCommand : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);

        private byte[] _command;

        public string HostCommand { get; set; }
        public string HostSubCommand { get; set; }

        public byte[] Command
        {
            get => _command;
            set => _command = value;
        }

        public string[] CommandSegments => CommandString.Split('$');

        public virtual string CommandAccepted => $"{AST}G{Lift}{AccessPoint}$P XS$E00{Tray}{CR}{LF}";
        public virtual string CommandFailed => $"{AST}G{Lift}{AccessPoint}$P XS$E02{CR}{LF}";
        public string CommandBufferEmpty => $"{AST}BE${CR}{LF}";
        public virtual string CommandExecuted => $"{AST}G{Lift}{AccessPoint}$P XA$A12$E00${CR}{LF}";
        public virtual string Lift => CommandString.Substring(2, 2);
        public virtual int Device => int.Parse(Lift);
        public virtual string AccessPoint => CommandString.Substring(4, 1);

        public virtual string Tray
        {
            get
            {
                var segment = CommandSegments.FirstOrDefault(r => r.StartsWith("T"));
                return segment == null ? string.Empty: segment.Substring(1);
            }
        }

        public virtual string Over
        {
            get
            {
                var segment = CommandSegments.FirstOrDefault(r => r.StartsWith("F"));
                return segment == null ? string.Empty : segment.Substring(1);
            }
        }

        public virtual string Back
        {
            get
            {
                var segment = CommandSegments.FirstOrDefault(r => r.StartsWith("O"));
                return segment == null ? string.Empty : segment.Substring(1);
            }
        }

        public int TimesToFail { get; set; } = 3;

        public string CommandString { get; set; } = string.Empty;
        public bool Accepted { get; set; } = false;
        public bool Failed { get; set; } = false;
        public bool Executed { get; set; } = false;

        public List<DisplayLine> DisplayLines => throw new NotImplementedException();

        public string Width => throw new NotImplementedException();

        public string ScreenSize => throw new NotImplementedException();

        string IHanelCommand.Command => throw new NotImplementedException();

        string IHanelCommand.Tray { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string DisplayText()
        {
            throw new NotImplementedException();
        }
    }
}
