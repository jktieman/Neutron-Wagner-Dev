using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

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
        public string SubCommand = string.Empty;
        public string Lift { get; set; }
        public string AccessPoint { get; set; }

        public HanelResponseExecuted(string lift, string accessPoint)
        {
            Lift = lift;
            AccessPoint = accessPoint;
        }

        public HanelResponseExecuted(string[] dataIn)
        {
            if (dataIn.Length < MinSegments) return;
            _dataIn = dataIn;
            SubCommand = dataIn[2];
            Lift = dataIn[0].Substring(2, 2);
            AccessPoint = dataIn[0].Substring(4, 1);
        }

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
            return SubCommand == "E00";
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
