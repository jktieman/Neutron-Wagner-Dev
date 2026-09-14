using System;

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
