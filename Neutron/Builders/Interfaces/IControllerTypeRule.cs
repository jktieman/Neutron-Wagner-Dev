using NeutronEvents;

namespace Neutron.Builders.Interfaces
{
    public interface IControllerTypeRule
    {
        bool IsMatch(string controllerType, ResponseInfo responseInfo);
        void BuildInfo(string response, ResponseInfo responseInfo);
    }
}
