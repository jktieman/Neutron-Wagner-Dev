using System.Threading.Tasks;
using NeutronData.Models;

namespace Neutron.Interfaces
{ 
    public interface IInventoryManager
    {
        Task<bool> DeleteInventoryRecord(int invId, bool releaseOnly = false);
        Task<bool> ReleaseCheck(Inventory inventory);
        Task<bool> QuickReleaseCheck(Inventory inventory);
    }
}