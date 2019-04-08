using System;
using System.Linq;

namespace Core.Common
{
    public static class OperationResultProcessor
    {
        public static OperationResult CombineOperationResultMessages(OperationResult main, OperationResult or)
        {
            if (or.MessageList.Count > 0)
            {
                foreach (string msg in or.MessageList)
                {
                    main.MessageList.Add(msg);
                }
            }
            return main;
        }
    }
}
