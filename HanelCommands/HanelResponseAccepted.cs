using System.Linq;
using System;
using System.Runtime.InteropServices.ComTypes;

namespace HanelCommands
{
    public class HanelResponseAccepted : IHanelResponse
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        private const string HostCommand = "P XS";
        private const int MinSegments = 3;
        private string[] _dataIn;
        public string SubCommand = string.Empty;

        public HanelResponseAccepted(string[] dataIn)
        {
            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            SubCommand = dataIn[2];
            Lift = dataIn[0].Substring(2, 2);
            AccessPoint = dataIn[0].Substring(4, 1);

        }

        public HanelResponseAccepted(string lift, string accessPoint)
        {
            Lift = lift;
            AccessPoint = accessPoint;
        }

        public string Lift { get; set; }
        public string AccessPoint { get; set; }

        public void Response(string[] dataIn)
        {

            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            SubCommand = dataIn[2];
            Lift = dataIn[0].Substring(2, 2);
            AccessPoint = dataIn[0].Substring(4, 1);

        }

        public bool IsSuccess()
        {
            return true;
        }

        public string ErrorMessage()
        {
            return string.Empty;
        }
        public string Command()
        {
            return $"{AST}G{Lift}{AccessPoint}$P XS$E00${CR}{LF}";
        }

    }
}