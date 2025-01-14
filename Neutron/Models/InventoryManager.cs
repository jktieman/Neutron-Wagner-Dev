using System.Linq;
using System.Threading.Tasks;
using Neutron.Global;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronCore.Enums;
using Neutron.Interfaces;
using NeutronData.Interfaces;
using NeutronData.UnitOfWorks;

namespace Neutron.Models
{
    public class InventoryManager : IInventoryManager
    {
        private readonly IInventoryUnitOfWork _inventoryUnitOfWork;
        private readonly ILocationsRepository _locationsRepository;

        public InventoryManager(IInventoryUnitOfWork inventoryUnitOfWork, ILocationsRepository locationsRepository
        )
        {
            _inventoryUnitOfWork = inventoryUnitOfWork;
            _locationsRepository = locationsRepository;
        }

        public async Task<bool> DeleteInventoryRecordAsync(int invId, bool releaseOnly = false)
        {
            var inventory = await _inventoryUnitOfWork.Inventory.FindByKeyAsync(invId);
            if (inventory == null || (releaseOnly && inventory.StorageTypeId != (int)StorageType.Release))
            {
                return false;
            }
            await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventory);
            var otherInventoryInLocation = await _inventoryUnitOfWork.Inventory.FindByAsync(r => r.LocationId == inventory.LocationId);
            if (otherInventoryInLocation.Count() == 1)
            {
                await _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
            }
            return await _inventoryUnitOfWork.Inventory.DeleteAsync(invId);
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
        public bool QuickReleaseCheckAsync(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            return isInventoryEmpty && isStorageTypeRelease;
        }
    }
}
