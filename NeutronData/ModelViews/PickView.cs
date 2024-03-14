using System;
using NeutronData.Models;
using System.Collections.Generic;
using NeutronCore.Enums;
using NeutronData.Interfaces;

namespace NeutronData.ModelViews
{
    public class PickView
    {
        public PickView()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickLocations = new List<PickLocation>();
        }
        public int Sequence { get; set; }
        public int PickPosition { get; set; }
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public string ItemKey { get; set; }
        public string Description { get; set; }
        public string UnitOfIssue { get; set; }
        public int Quantity { get; set; }
        // if the quantity is not changed, the QuantityThisPick is the same as Quantity
        public int QuantityThisPick { get; set; }
        public int QuantityToBePicked { get; set; }
        public int PickedQty { get; set; }  //sum of PickLocations
        public string Slot { get; set; }
        public int SlotQty { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public List<Inventory> Inventory { get; set; }
        public int InventoryIndex { get; set; }
        public Inventory CurrentInventoryLocation { get; set; }
        public int TotalQuantityInInventory { get; set; }
        public List<ItemImage> Images { get; set; }
        public List<PickLocation> PickLocations { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int AreaId { get; set; }
        public int PreviousLineStatusId { get; set; }
        public int GetQuantityToBePicked()
        {
            var result = Quantity - PickedQty;
            return result > 0 ? result : 0;
        }
    }
}
