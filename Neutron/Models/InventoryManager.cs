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
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryManager(IInventoryUnitOfWork inventoryUnitOfWork, ILocationsRepository locationsRepository, IInventoryRepository inventoryRepository
        )
        {
            _inventoryUnitOfWork = inventoryUnitOfWork;
            _locationsRepository = locationsRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<bool> DeleteInventoryRecord(int invId, bool releaseOnly = false)
        {
            var result = false;
            var inventory = _inventoryUnitOfWork.Inventory.FindByKey(invId);
            if (inventory == null || (releaseOnly && inventory.StorageTypeId != (int)StorageType.Release))
            {
                return false;
            }
            var inventoryView = _inventoryRepository.GetInventoryViewById(inventory.Id);
            
            var otherInventoryInLocation = _inventoryUnitOfWork.Inventory.FindBy(r => r.LocationId == inventory.LocationId);
            if (otherInventoryInLocation.Count() == 1)
            {
                await _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
            }

            result = await _inventoryUnitOfWork.Inventory.DeleteAsync(invId);
            if (result)
            {
                await GlobalVar.HistoryManager.SaveHistoryAsync(ActionCode.InventoryDelete, inventoryView);
            }
            return result;
        }

        public async Task<bool> ReleaseCheck(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            if (!isInventoryEmpty || !isStorageTypeRelease)
            {
                return false;
            }
            return await DeleteInventoryRecord(inventory.Id, releaseOnly: true);
        }
        public async Task<bool> QuickReleaseCheck(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            return await Task.FromResult(isInventoryEmpty && isStorageTypeRelease);
        }
    }
}
