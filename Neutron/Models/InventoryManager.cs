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

        public bool DeleteInventoryRecord(int invId, bool releaseOnly = false)
        {
            var inventory = _inventoryUnitOfWork.Inventory.FindByKey(invId);
            if (inventory == null || (releaseOnly && inventory.StorageTypeId != (int)StorageType.Release))
            {
                return false;
            }
            GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
            var otherInventoryInLocation = _inventoryUnitOfWork.Inventory.FindBy(r => r.LocationId == inventory.LocationId);
            if (otherInventoryInLocation.Count() == 1)
            {
                 _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
            }
            return _inventoryUnitOfWork.Inventory.DeleteWithReturn(invId);
        }

        public bool ReleaseCheck(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            if (!isInventoryEmpty || !isStorageTypeRelease)
            {
                return false;
            }
            return DeleteInventoryRecord(inventory.Id, releaseOnly: true);
        }
        public bool QuickReleaseCheck(Inventory inventory)
        {
            var isInventoryEmpty = inventory.Quantity <= 0;
            var isStorageTypeRelease = inventory.StorageTypeId == (int)StorageType.Release;
            return isInventoryEmpty && isStorageTypeRelease;
        }
    }
}
