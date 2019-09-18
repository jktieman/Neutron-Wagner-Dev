using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using SlotNameFactory;
using System;
using System.Linq;
using System.Windows.Forms;

namespace NeutronLoader
{
    public class InventoryProcessor
    {
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        
        public Inventory GetOrCreate(ItemDefinition itemDef, Location location)
        {
            var inventory = new Inventory();
            try
            {
                inventory = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id 
                && r.LocationId == location.Id).FirstOrDefault();
                if (inventory == null)
                {
                    inventory = CreateNewInventory(itemDef, location);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error Finding Inventory Item - {itemDef.Item}  Location - {location.Slot}.  {ex.Message} {Environment.NewLine} {ex.InnerException}");

            }
            return inventory;
        }

        private Inventory CreateNewInventory(ItemDefinition itemDef, Location location)
        {

            var newDefinition = new Inventory();
            Inventory def;  
            try
            {
                def = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id
                && r.LocationId == location.Id).FirstOrDefault();

                if (def == null)
                {
                    newDefinition = new Inventory
                    {
                        StationId = location.StationId
                        , ItemDefinitionId = itemDef.Id
                        , LocationId = location.Id
                        , StorageTypeId = 1
                        , PrimeBin = true
                        , Quantity = 99999
                        , ReceivedDate = DateTime.Now
                   
                    };
                    try
                    {
                        _repoInventory.Insert(newDefinition);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error Inserting Inventory Item - {itemDef.Item}  Location - {location.Slot}.  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    }
                }
                else
                {
                    Logger.Log(msg: "No Default Inventory.  Create Inventory Failed.");
                    var msg = "A default Inventory must be set up in ";
                    msg += "order to create definitions during the Order Load process.";
                    msg += "The Slot number MUST be called, DEFAULT .";
                    MessageBox.Show(msg);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Unable to create New Inventory.  {ex.Message} {Environment.NewLine} {ex.InnerException.Message}");
            }
            return newDefinition;
        }
    }
}
