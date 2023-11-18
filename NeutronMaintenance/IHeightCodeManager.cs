using NeutronData.Models.Lookups;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public interface IHeightCodeManager
    {
        HeightCode Get(string code);
        HeightCode Get(int id);
    }
}