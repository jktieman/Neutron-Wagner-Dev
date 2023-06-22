using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronEvents
{
    public class WorkItEventArgs : EventArgs
    {
        public WorkItEventArgs() { }
        public string Message { get; set; }
        public List<string> MessageList { get; set; }

        public WorkItEventArgs(string message) { Message = message; }
        public WorkItEventArgs(List<string> list) {MessageList = list; }
    }
}
