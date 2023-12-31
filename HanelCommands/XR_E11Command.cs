using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    public class XR_E11Command : IHanelCommand
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);

        private readonly string _commandString;
        private readonly string[] _commandSegments;
        private readonly string _lift;
        private readonly string _accessPoint;
        private string _tray;
        private string _over;
        private string _back;
        private string _width;
        private string _screenSize ;
        private string _displayText = String.Empty;
        private List<DisplayLine> _displayLines = new List<DisplayLine>();

        /// <summary>
        /// Display a Storage Location on the lights
        /// </summary>
        /// <param name="dataIn"></param>
        public XR_E11Command(byte[] dataIn)
        {
            _commandString = Encoding.UTF8.GetString(dataIn);
            _commandSegments = _commandString.Split('$');
            _lift = _commandSegments[0].Substring(2, 2);
            _accessPoint = _commandSegments[0].Substring(4, 1);
            _tray =  GetSegmentValue("T"); 
            _over  = GetSegmentValue("F"); 
            _back  = GetSegmentValue("O"); 
            _width = GetSegmentValue("H"); 

        }

        public XR_E11Command(string lift, string accessPoint)
        {
            _lift = lift.PadLeft(2, '0');
            _accessPoint = accessPoint;
        }

        private string GetSegmentValue(string segmentType)
        {
            var result = _commandSegments.FirstOrDefault(r => r.StartsWith(segmentType));
            return result == null || result.Length == 1 ? "0" : result.Substring(1);
        }


        public string HostCommand => "M XR";
        public string HostSubCommand => "E11";
        public string Command => $"{AST}G{Lift}{AccessPoint}$M XR$E11${CR}{LF}";
        public string[] CommandSegments => _commandSegments;
        public string CommandString => _commandString;
        public string CommandAccepted => $"{AST}G{Lift}{AccessPoint}$P XS$E00{CR}{LF}";
        public string CommandFailed => $"{AST}G{Lift}{AccessPoint}$P XS$E02{CR}{LF}";
        public string CommandBufferEmpty => $"{AST}BE${CR}{LF}";
        public string CommandExecuted => $"{AST}G{Lift}{AccessPoint}$P XA$A11$E00${CR}{LF}";
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
