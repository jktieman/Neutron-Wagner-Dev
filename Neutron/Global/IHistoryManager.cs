using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeutronCore.Enums;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.PrintModels;

namespace Neutron.Global
{
    public interface IHistoryManager
    {
        void SaveHistory(ActionCode actionCode, Order order);
        void SaveHistory(ActionCode actionCode, ReplenOrder order);
        void SaveHistory(ActionCode actionCode, OrderView order);
        void SaveHistory(ActionCode actionCode, ReplenOrderView order);
        void SaveHistory(ActionCode actionCode, OrderDetail orderDetail);
        void SaveHistory(ActionCode actionCode, ReplenOrderDetail orderDetail);
        Task SaveHistoryAsync(ActionCode actionCode, ReplenOrderDetail orderDetail);
        void SaveHistory(ActionCode actionCode, PickStop pickStop);
        void SaveHistory(ActionCode actionCode, ReplenPickStop pickStop);
        void SaveHistory(ActionCode actionCode, Order value, int workstationId);
        void SaveHistory(ActionCode actionCode, OrderDetail value, int workstationId);
        void SaveHistory(ActionCode actionCode, Inventory inventory);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, PickView pickView);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, PickList pickList);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, OrderDetail orderDetail);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, ReplenOrderDetail orderDetail);
        
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, string costCenter);
        void SaveHistory(ActionCode actionCode, Inventory inventory, int prevQty, bool value);
        Task SaveHistoryAsync(ActionCode actionCode, Inventory inventory, int prevQty, bool value);

        void SaveHistory(ActionCode actionCode, LocationCount locationCount);
        Task SaveHistoryAsync(ActionCode actionCode, LocationCount locationCount);
        void SaveHistory(ActionCode actionCode, Location location);
        void SaveHistory(ActionCode actionCode, ItemDefinition itemDefinition);
        void SaveHistory(ActionCode actionCode, ReplenOrderDetail value, int workstationId);
        void SaveHistory(ActionCode actionCode, ReplenOrder order, int workstationId);
        void SaveHistory(ActionCode actionCode, SkipView skipView);
        Task SaveHistoryAsync(ActionCode actionCode, Location location);

        Task SaveHistoryAsync(ActionCode actionCode, ItemDefinition itemDefinition);
        Task SaveHistoryAsync(ActionCode actionCode, OrderDetail orderDetail);
        Task SaveHistoryAsync(ActionCode actionCode, Order order);
        Task SaveHistoryAsync(ActionCode actionCode, ReplenOrder order);
        Task SaveHistoryAsync(ActionCode actionCode, ReplenOrderView order);
        Task SaveHistoryAsync(ActionCode actionCode, Inventory inventory, int pickLocationQty, PickView pickView);
        Task SaveHistoryAsync(ActionCode actionCode, Inventory inventory, int pickLocationQty, ReplenPickView pickView);
        Task SaveHistoryAsync(ActionCode actionCode, InventoryView inventoryView);
        List<HistoryView> GetHistoryRecordsByUser(string empId);
        List<HistoryView> GetHistoryRecords();
        List<HistoryView> GetHistoryRecords(string codes, DateTime fromDate, DateTime toDate, string find);
    }
}