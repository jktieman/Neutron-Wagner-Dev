using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neutron.Global;
using NeutronData.Models;
using NeutronData.Repositories;
using Neutron.Enums;
using NeutronCore.Enums;
using NeutronData.DataContexts;

namespace Neutron.Models
{
    public class InventoryManager
    {
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly LocationsRepository _locationsRepository;
        private NeutronDb db = new NeutronDb();  

        public InventoryManager(GenericRepository<Inventory> repoInventory, LocationsRepository locationsRepository)
        {
            _repoInventory = repoInventory;
            _locationsRepository = locationsRepository;
        }


        public void DeleteInventoryRecord(int invId, bool releaseOnly = false)
        {
            var inventory = db.Inventory.Find(invId);
            //var inventory = _repoInventory.FindByKey(invId);
            if (inventory == null) return;
            if (releaseOnly)
            {
                if (inventory.StorageTypeId == (int)StorageType.Release)
                {
                    GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
                    _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
                   // _repoInventory.Delete(inventory.Id);
                    db.Inventory.Remove(inventory);
                    db.SaveChanges();
                }
            }
            else
            {
                GlobalVar.HistoryManager.SaveHistory(ActionCode.InventoryDelete, inventory);
                _locationsRepository.SetLocationInUse(inventory.LocationId, b: false);
               // _repoInventory.Delete(inventory.Id);
                db.Inventory.Remove(inventory);
                db.SaveChanges();
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
