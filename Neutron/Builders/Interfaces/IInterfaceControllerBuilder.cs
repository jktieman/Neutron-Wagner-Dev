using NeutronEvents;

namespace Neutron.Builders.Interfaces
{
    public interface IInterfaceControllerBuilder
    {
        void BuildInterfaceController(string response, ResponseInfo responseInfo);
    }
}
