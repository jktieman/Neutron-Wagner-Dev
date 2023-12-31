using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    public class XOCommand : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        private readonly byte[] _dataIn;
        private readonly string _commandString;
        private readonly string[] _commandSegments;
        private readonly string _lift;
        private readonly string _accessPoint;
        private string _tray = string.Empty;
        private string _over = string.Empty;
        private string _back = string.Empty;
        private string _width = "01";
        private List<DisplayLine> _displayLines = new List<DisplayLine>();
        private string _displayText = string.Empty;
        private string _screenSize;


        /// <summary>
        /// Display Information on the Hanel Monitor
        /// </summary>
        /// <param name="dataIn"></param>
        public XOCommand(byte[] dataIn)
        {
            _dataIn = dataIn;
            _commandString = Encoding.UTF8.GetString(_dataIn);
            _commandSegments = _commandString.Split('$');
            _lift = _commandSegments[0].Substring(2, 2);
            _accessPoint = _commandSegments[0].Substring(4, 1);
            GetDisplayLines();
        }

        public XOCommand(string lift, string accessPoint, List<DisplayLine> displayLines)
        {
            _lift = lift.PadLeft(2, '0');
            _accessPoint = accessPoint;
            _displayLines = displayLines;

        }

        public string HostCommand => "M XO";
        public string HostSubCommand => "";
        public string Command => $"{AST}G{Lift}{AccessPoint}$M XO{DisplayText()}$C00000${CR}{LF}";
        public string[] CommandSegments => _commandSegments;
        public string CommandString => _commandString;
        public string CommandAccepted => $"{AST}G{Lift}{AccessPoint}$P XS$E00${CR}{LF}";
        public string CommandFailed => $"{AST}G{Lift}{AccessPoint}$P XS$E02${CR}{LF}";
        public string CommandBufferEmpty => $"{AST}BE${CR}{LF}";
        public string CommandExecuted => $"{AST}G{Lift}{AccessPoint}$P XA$A11$E00${CR}{LF}";

        private void GetDisplayLines()
        {
            try
            {
                _displayLines.Clear();
                var segments = _commandSegments.Where(r => r.StartsWith("X")).ToList();
                foreach (var segment in segments)
                {
                    if (segment.Length >= 5)
                    {
                        var line = segment.Substring(1, 2);
                        var column = segment.Substring(3, 3);
                        var text = segment.Substring(6);
                        var displayLine = new DisplayLine(line, column, text);
                        _displayLines.Add(displayLine);
                    }
                }
            }
            catch (Exception ex)
            {
                var str = ex.Message;

            }
        }
        public List<DisplayLine> DisplayLines => _displayLines;

        public string DisplayText()
        {
            var text = string.Empty;
            foreach (var line in _displayLines)
            {
                text += $"$X{line}";
            }
            return text;
        }
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
