using System;

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
        public string SubCommand { get; set; } = string.Empty;

        public HanelResponseAccepted(string[] dataIn)
        {
            Lift = string.Empty;
            AccessPoint = string.Empty;
            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            var firstSegment = dataIn[0];
            SubCommand = dataIn[2];
            Lift = firstSegment.Length >= 4 ? firstSegment.Substring(2, 2) : string.Empty;
            AccessPoint = firstSegment.Length >= 5 ? firstSegment.Substring(4, 1) : string.Empty;
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
            var firstSegment = dataIn[0];
            SubCommand = dataIn[2];
            Lift = firstSegment.Length >= 4 ? firstSegment.Substring(2, 2) : string.Empty;
            AccessPoint = firstSegment.Length >= 5 ? firstSegment.Substring(4, 1) : string.Empty;
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