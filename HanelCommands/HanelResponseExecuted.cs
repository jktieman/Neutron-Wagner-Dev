using System;

namespace HanelCommands
{
    public class HanelResponseExecuted : IHanelResponse
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        private const string HostCommand = "P XA";
        private const int MinSegments = 4;
        private string[] _dataIn;
        public string SubCommand { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string Lift { get; set; } = string.Empty;
        public string AccessPoint { get; set; } = string.Empty;

        public HanelResponseExecuted(string lift, string accessPoint)
        {
            Lift = lift;
            AccessPoint = accessPoint;
        }

        public HanelResponseExecuted(string[] dataIn)
        {
            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            var firstSegment = dataIn[0];
            SubCommand = dataIn[2];
            ErrorCode = dataIn[3];
            Lift = firstSegment.Length >= 4 ? firstSegment.Substring(2, 2) : string.Empty;
            AccessPoint = firstSegment.Length >= 5 ? firstSegment.Substring(4, 1) : string.Empty;
        }

        public void Response(string[] dataIn)
        {
            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            var firstSegment = dataIn[0];
            SubCommand = dataIn[2];
            ErrorCode = dataIn[3];
            Lift = firstSegment.Length >= 4 ? firstSegment.Substring(2, 2) : string.Empty;
            AccessPoint = firstSegment.Length >= 5 ? firstSegment.Substring(4, 1) : string.Empty;
        }
        public bool IsSuccess()
        {
            return ErrorCode == "E00";
        }
        public string ErrorMessage()
        {
            return string.Empty;
        }
        public string Command()
        {
            return $"{AST}G{Lift}{AccessPoint}$P XA$A20$E00${CR}{LF}";
        }
    }
}
