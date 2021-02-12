using NeutronEvents;

namespace Neutron.Builders.Interfaces
{
    public interface IBayControllerBuilder
    {
        void BuildBayController(string response, ResponseInfo responseInfo);
    }
}
