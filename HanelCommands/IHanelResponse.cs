using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands
{
    public interface IHanelResponse
    {
        void Response(string[] s);
        bool IsSuccess();
        string ErrorMessage();
        string Command();
        string Lift { get; }
        string AccessPoint { get; }
    }
}
