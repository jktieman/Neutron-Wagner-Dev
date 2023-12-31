using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    // ReSharper disable once InconsistentNaming
    public class XR_E20Command : IHanelCommand
    {
        // ReSharper disable once InconsistentNaming
        private static readonly char CR = Convert.ToChar(13);
        // ReSharper disable once InconsistentNaming
        private static readonly char LF = Convert.ToChar(10);
        // ReSharper disable once InconsistentNaming
        private static readonly char AST = Convert.ToChar(42);

        private const int MinSegments = 7;
        
        private readonly string _subCommand = string.Empty;
        private readonly string _lift = string.Empty;
        private readonly string _accessPoint = string.Empty;
        private string _tray = string.Empty;
        private string _over = string.Empty;
        private string _back = string.Empty;
        private string _width = "01";
        private string _screenSize = String.Empty;
        private readonly string _commandString;
        private readonly string[] _commandSegments;
        private string _displayText = String.Empty;
        private List<DisplayLine> _displayLines = new List<DisplayLine>();


        public XR_E20Command(byte[] dataIn)
        {
            if (dataIn.Length > 0)
            {
                _commandString = Encoding.UTF8.GetString(dataIn);
                _commandSegments = _commandString.Split('$');

                if (_commandSegments.Length < MinSegments) return;

                _lift = _commandSegments[0].Substring(2, 2);
                _accessPoint = _commandSegments[0].Substring(4, 1);

                _tray = GetSegmentValue("T");
                _over = GetSegmentValue("F");
                _back = GetSegmentValue("O");
                _width = GetSegmentValue("H"); 
            }

        }
        private string GetSegmentValue(string segmentType)
        {
            var result = _commandSegments.FirstOrDefault(r => r.StartsWith(segmentType));
            return result == null || result.Length == 1 ? "0" : result.Substring(1);
        }

        public XR_E20Command(string lift, string ap)
        {
            _lift = lift.Trim().PadLeft(2, '0');
            _accessPoint = ap;
        }


        public XR_E20Command(string lift, string ap, string tray, string over, string back, string width = "01")
        {
            _lift = lift.Trim().PadLeft(2,'0');
            _accessPoint = ap;
            _tray = tray.Trim().PadLeft(3, '0');
            _over = over.Trim().PadLeft(2, '0');
            _back = back.Trim().PadLeft(2, '0');
            _width = width.Trim().PadLeft(2, '0');
        }

        public string HostCommand => "M XR";
        public string HostSubCommand => "E20";
        public string Command => $"{AST}G{_lift}{_accessPoint}$M XR$E20$T{_tray}$F{_over}$O{_back}$H{_width}$P1${CR}{LF}";
        public string[] CommandSegments => _commandSegments;
        public string CommandString => _commandString;
        public string CommandAccepted => $"{AST}G{_lift}{_accessPoint}$P XS$E00${CR}{LF}";
        public string CommandFailed => $"{AST}G{_lift}{_accessPoint}$P XS$E02${CR}{LF}";
        public string CommandBufferEmpty => $"{AST}BE${CR}{LF}";
        public string CommandExecuted => $"{AST}G{_lift}{_accessPoint}$P XA$A20$E00${CR}{LF}";
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
        public string AccessPoint => _accessPoint;
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
