using NeutronEvents;

namespace IPTI.Builders.Interfaces
{
    public interface IInterfaceControllerRule
    {
        bool IsMatch(string code, ResponseInfo responseInfo);
        void BuildInfo(string response, ResponseInfo responseInfo);
    }
}
