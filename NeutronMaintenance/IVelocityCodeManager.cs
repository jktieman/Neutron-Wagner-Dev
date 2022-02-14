using NeutronData.Models.Lookups;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public interface IVelocityCodeManager
    {
        VelocityCode Get(string code);
        VelocityCode Get(int id);
    }
}