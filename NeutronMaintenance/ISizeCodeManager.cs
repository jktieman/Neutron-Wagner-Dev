using NeutronData.Models.Lookups;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public interface ISizeCodeManager
    {
        SizeCode Get(string code);
        SizeCode Get(int id);
    }
}