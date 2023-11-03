using SAPServer.Models;

namespace SAPServer
{
    public interface ISapService
    {
        void Init();
        void ProcessRecords(SapVariables sapVariables);
        void ErrorAlert(string err);
    }
}