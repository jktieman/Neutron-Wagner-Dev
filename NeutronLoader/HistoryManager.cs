using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronCore.Enums;
using System.Data.SqlClient;

namespace NeutronLoader
{

    public class HistoryManager
    {
        readonly GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        readonly InventoryRepository _repoInventory = new InventoryRepository();

        public void SaveHistory(ActionCode actionCode, Order order, string empId = "")
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                EmpId = empId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, PickStop pickStop, string empId = "")
        {
            foreach (PickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionDateTime = pickLocation.PickDate,
                        Ord1 = pickView.Ord1,
                        Ord2 = pickView.Ord2,
                        OrderId = pickView.OrderId,
                        Item = pickView.Item,
                        Description = pickView.Description,
                        RequestedQuantity = pickView.Quantity,
                        IssuedQuantity = pickLocation.Quantity,
                        StationId = pickLocation.Inventory.Location.StationId,
                        Loc1 = pickLocation.Inventory.Location.Loc1,
                        Loc2 = pickLocation.Inventory.Location.Loc2,
                        Loc3 = pickLocation.Inventory.Location.Loc3,
                        Loc4 = pickLocation.Inventory.Location.Loc4,
                        Loc5 = pickLocation.Inventory.Location.Loc5,
                        Slot = pickLocation.Inventory.Location.Slot,
                        EmpId = empId
                    };
                    Save(history);
                }
            }
        }

        public void SaveHistory(ActionCode actionCode, ReplenPickStop pickStop, string empId = "")
        {
            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionDateTime = pickLocation.PickDate,
                        Ord1 = pickView.Ord1,
                        Ord2 = pickView.Ord2,
                        Item = pickView.Item,
                        Description = pickView.Description,
                        RequestedQuantity = pickView.Quantity,
                        IssuedQuantity = pickLocation.Quantity,
                        StationId = pickLocation.Inventory.Location.StationId,
                        Loc1 = pickLocation.Inventory.Location.Loc1,
                        Loc2 = pickLocation.Inventory.Location.Loc2,
                        Loc3 = pickLocation.Inventory.Location.Loc3,
                        Loc4 = pickLocation.Inventory.Location.Loc4,
                        Loc5 = pickLocation.Inventory.Location.Loc5,
                        Slot = pickLocation.Inventory.Location.Slot,
                        EmpId = empId
                    };
                    Save(history);
                }
            }
        }

        public void SaveHistory(ActionCode actionCode, OrderDetail value, string empId = "")
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                StationId = value.StationNumber,
                Ord1 = value.Order.Ord1,
                Ord2 = value.Order.Ord2,
                OrderId = value.OrderId,
                Item = value.PartNum,
                Description = value.PartDesc,
                RequestedQuantity = value.Quantity,
                IssuedQuantity = value.PickedQuantity,
                Slot = value.PrimeBin,
                EmpId = empId,
                OrderDetailId = value.Id
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, Inventory inventory, string empId = "")
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                // Ord1 = pickStop.Ord1,
                // Ord2 = pickStop.Ord2,
                //OrderId = pickStop.OrderId,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = inv.Quantity,
                //RequestedQuantity = inv.Quantity,
                StationId = inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                EmpId = empId
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, LocationCount cnt, string empId = "")
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(cnt.InventoryId);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = cnt.CountDate,
                Item = inv.Item,
                Description = inv.Description,
                RequestedQuantity = cnt.PreviousQty,
                IssuedQuantity = cnt.NewQty,
                StationId = inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                
                EmpId = empId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, Location location, string empId = "")
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                Item = null,
                Description = null,
               // RequestedQuantity = null,
               //IssuedQuantity = NewQty,
                StationId = location.StationId,
                Loc1 = location.Loc1,
                Loc2 = location.Loc2,
                Loc3 = location.Loc3,
                Loc4 = location.Loc4,
                Loc5 = location.Loc5,
                Slot = location.Slot,
                       
                EmpId = empId
            };
            Save(history);

        }

        private void Save(History history)
        {
            try
            {
                _repoHistory.Insert(history);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error, unable to save history record. " + ex.Message);
            }
        }

        public void SaveHistory(ActionCode actionCode, ReplenOrderDetail value, string empId = "")
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                StationId = value.StationNumber,
                Ord1 = value.ReplenOrder.Ord1,
                Ord2 = value.ReplenOrder.Ord2,
                OrderId = value.ReplenOrder.Id,
                Item = value.PartNum,
                Description = value.PartDesc,
                RequestedQuantity = value.Quantity,
                IssuedQuantity = value.PickedQuantity,
                Slot = value.PrimeBin,
                EmpId = empId,
                OrderDetailId = value.Id
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, ReplenOrder order, string empId = "")
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                EmpId = empId
            };
            Save(history);
        }

      
    }
}
