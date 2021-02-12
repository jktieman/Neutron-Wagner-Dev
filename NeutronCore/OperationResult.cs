using System.Collections.Generic;
using System.Text;

namespace NeutronCore
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public List<string> MessageList { get; set; }

        public OperationResult()
        {
            Success = true;
            MessageList = new List<string>();
        }

        public string Message()
        {
            var result = new StringBuilder();
            foreach (var msg in MessageList)
            {
                result.AppendLine(msg);
            }
            return result.ToString();
        }

        public void AddMessage(string message)
        {
            MessageList.Add(message);
        }
    }
}