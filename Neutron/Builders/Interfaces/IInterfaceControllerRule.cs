using Neutron.Models;
using NeutronEvents;

namespace Neutron.Builders.Interfaces
{
    public interface IInterfaceControllerRule
    {
        bool IsMatch(string code, ResponseInfo responseInfo);
        void BuildInfo(string response, ResponseInfo responseInfo);
    }
}
