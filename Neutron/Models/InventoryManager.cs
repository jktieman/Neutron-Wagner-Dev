using Neutron.Global;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronCore.Enums;
using Neutron.Interfaces;
using NeutronData.Interfaces;

namespace Neutron.Models
{
    public class InventoryManager : IInventoryManager
    {
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly ILocationsRepository _locationsRepository;
       
        public InventoryManager(GenericRepository<Inventory> repoInventory, ILocationsRepository locationsRepository)
        {
            _repoInventory = repoInventory;
            _locationsRepository = locationsRepository;
        }

        public void DeleteInventoryRecord(int invId, bool releaseOnly = false)
        {
            var inventory = _repoInventory.FindByKey(invId);
            if (inventory == null) return;
            if (releaseOnly)
            {
                if (inventory.StorageTypeId == (int)StorageType.Release)
                {
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
                    _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
                    _repoInventory.Delete(invId);
                }
            }
            else
            {
                GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
                _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
                _repoInventory.Delete(invId);
            }
        }

        public void ReleaseCheck(Inventory inventory)
        {
            if (inventory.Quantity <= 0 && inventory.StorageTypeId == (int)StorageType.Release)
            {
                DeleteInventoryRecord(inventory.Id, releaseOnly: true);
            }
        }
    }
}
