using NeutronData.Models;

namespace Neutron.Interfaces
{ 
    public interface IInventoryManager
    {
        bool DeleteInventoryRecord(int invId, bool releaseOnly = false);
        bool ReleaseCheck(Inventory inventory);
        bool QuickReleaseCheck(Inventory inventory);
    }
}