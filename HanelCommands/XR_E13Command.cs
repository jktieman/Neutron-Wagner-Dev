using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HanelCommands
{
    public class XR_E13Command : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        private readonly string _commandString;
        private readonly string[] _commandSegments;
        private readonly string _lift;
        private readonly string _accessPoint;
        private string _tray = string.Empty;
        private string _over = string.Empty;
        private string _back = string.Empty;
        private string _width = "01";
        private string _screenSize;
        private string _displayText = String.Empty;
        private List<DisplayLine> _displayLines = new List<DisplayLine>();

        /// <summary>
        /// Change the Display Mode
        /// </summary>
        /// <param name="dataIn"></param>
        public XR_E13Command(byte[] dataIn)
        {
            _commandString = Encoding.UTF8.GetString(dataIn);
            _commandSegments = _commandString.Split('$');
            _lift = _commandSegments[0].Substring(2, 2);
            _accessPoint = _commandSegments[0].Substring(4, 1);
            _screenSize = _commandSegments.FirstOrDefault(r => r.StartsWith("C"))?.Substring(2) ?? "";
        }

        public XR_E13Command(string lift, string accessPoint, string screenSize = "08040")
        {
            _lift = lift.PadLeft(2, '0');
            _accessPoint = accessPoint;
            _screenSize = screenSize;
        }
        private string GetSegmentValue(string segmentType)
        {
            var result = _commandSegments.FirstOrDefault(r => r.StartsWith(segmentType));
            return result == null || result.Length == 1 ? "0" : result.Substring(1);
        }

        public string HostCommand => "M XR";
        public string HostSubCommand => "E13";
        public string Command => $"{AST}G{Lift}{AccessPoint}$M XR$E13$C{_screenSize}${CR}{LF}";
        public string[] CommandSegments => _commandSegments;
        public string CommandString => _commandString;
        public string CommandAccepted => $"{AST}G{Lift}{AccessPoint}$P XS$E00${CR}{LF}";
        public string CommandFailed => $"{AST}G{Lift}{AccessPoint}$P XS$E02${CR}{LF}";
        public string CommandBufferEmpty => $"{AST}BE${CR}{LF}";
        public string CommandExecuted => $"{AST}G{Lift}{AccessPoint}$P XA$A13$E00${CR}{LF}";
        public string DisplayText()
        {
            var text = string.Empty;
            foreach (var line in _displayLines)
            {
                text += $"X{line}";
            }
            return text;
        }
        public List<DisplayLine> DisplayLines => _displayLines;
        public string Lift => _lift;
        public int Device => int.Parse(_lift);
        public string AccessPoint  => _accessPoint;
        public string Tray
        {
            get => _tray;
            set => _tray = value;
        }

        public string Over => _over;
        public string Back => _back;
        public string Width => _width;
        public string ScreenSize => _screenSize;
        public bool Accepted { get; set; }
        public bool Failed { get; set; }
        public bool Executed { get; set; }
        public int TimesToFail { get; set; }
    }
}
