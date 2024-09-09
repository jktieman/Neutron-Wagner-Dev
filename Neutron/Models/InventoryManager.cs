using System.Linq;
using System.Threading.Tasks;
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

        public InventoryManager(GenericRepository<Inventory> repoInventory, ILocationsRepository locationsRepository
        )
        {
            _repoInventory = repoInventory;
            _locationsRepository = locationsRepository;
        }

        public async Task<bool> DeleteInventoryRecordAsync(int invId, bool releaseOnly = false)
        {
            var inventory = await _repoInventory.FindByKeyAsync(invId);
            if (inventory == null || (releaseOnly && inventory.StorageTypeId != (int)StorageType.Release))
            {
                return false;
            }
            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
            var otherInventoryInLocation = await _repoInventory.FindByAsync(r => r.LocationId == inventory.LocationId);
            if (otherInventoryInLocation.Count() == 1)
            {
                await _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
            }
            return await _repoInventory.DeleteAsync(invId);
        }

        public async Task<bool> ReleaseCheckAsync(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            if (!isInventoryEmpty || !isStorageTypeRelease)
            {
                return false;
            }
            return await DeleteInventoryRecordAsync(inventory.Id, releaseOnly: true);
        }
    }
}
