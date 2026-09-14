using System;

namespace HanelCommands
{
    public class HanelResponseBufferEmpty : IHanelResponse
    {
        private static readonly char CR = Convert.ToChar(13);
        private static readonly char LF = Convert.ToChar(10);
        private static readonly char AST = Convert.ToChar(42);
        public string Lift { get; set; } = string.Empty;
        public string AccessPoint { get; set; } = string.Empty;

        private string[] _dataIn;

        public void Response(string[] dataIn)
        {
            _dataIn = dataIn;
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
           return $"{AST}BE${CR}{LF}";
       }
    }
}
