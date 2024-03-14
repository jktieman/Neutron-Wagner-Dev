using NeutronEvents;

namespace IPTI.Builders.Interfaces
{
    public interface IBayControllerRule
    {
        bool IsMatch(string code, ResponseInfo responseInfo);
        void BuildInfo(string response, ResponseInfo responseInfo);
    }
}