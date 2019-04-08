using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Common
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
            StringBuilder result = new StringBuilder();
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
