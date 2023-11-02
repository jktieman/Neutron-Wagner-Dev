using AlliedLogger;
using SAPServer.Models;

namespace SAPServer
{
    public interface ISAPService
    {
        void Init();
        void ProcessRecords(SapNovaVariables sapNovaVariables);
    }
}