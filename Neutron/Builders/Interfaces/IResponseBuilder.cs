using NeutronEvents;

namespace Neutron.Builders.Interfaces
{
    public interface IResponseBuilder
    {
        void BuildInfo(string response, ResponseInfo responseInfo);
    }
}
