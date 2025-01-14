using System.Threading.Tasks;
using NeutronData.Models;

namespace Neutron.Interfaces
{ 
    public interface IInventoryManager
    {
        Task<bool> DeleteInventoryRecordAsync(int invId, bool releaseOnly = false);
        Task<bool> ReleaseCheckAsync(Inventory inventory);
        bool QuickReleaseCheckAsync(Inventory inventory);
    }
}