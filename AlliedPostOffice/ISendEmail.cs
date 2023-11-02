using System.Collections.Generic;
using System.Text;

namespace AlliedPostOffice
{
    public interface ISendEmail
    {
        void StartUp();
        void ShutDown();
        void StartUpUpload();
        void ShutDownUpload();
        void Message(string body, List<string> logLastLines);
        void Message(string subject, StringBuilder body);
        void StartUpSingleRun(List<string> logLastLines);
        void StartUpSingleRunUpload(List<string> logLastLines);
        void StartUpSap();
        void ShutDownSap();
    }
}