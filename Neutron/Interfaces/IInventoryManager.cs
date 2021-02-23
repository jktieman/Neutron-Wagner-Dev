using NeutronData.Models;

namespace Neutron.Interfaces
{ 
    public interface IInventoryManager
    {
        void DeleteInventoryRecord(int invId, bool releaseOnly = false);
        void ReleaseCheck(Inventory inventory);
    }
}