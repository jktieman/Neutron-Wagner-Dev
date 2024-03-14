using System;
using System.Collections.Generic;
using NeutronData.Interfaces;
using NeutronData.Models;

namespace NeutronData.ModelViews;

public interface IPickView
{
    int Sequence { get; set; }
    int PickPosition { get; set; }
    int OrderId { get; set; }
    string Ord1 { get; set; }
    string Ord2 { get; set; }
    int ItemId { get; set; }
    string Item { get; set; }
    string ItemKey { get; set; }
    string Description { get; set; }
    string UnitOfIssue { get; set; }
    int Quantity { get; set; }
    int QuantityThisPick { get; set; }
    int QuantityToBePicked { get; set; }
    int PickedQty { get; set; } //sum of PickLocations
    string Slot { get; set; }
    int SlotQty { get; set; }
    IOrderDetail OrderDetail { get; set; }
    List<Inventory> Inventory { get; set; }
    int InventoryIndex { get; set; }
    Inventory CurrentInventoryLocation { get; set; }
    int TotalQuantityInInventory { get; set; }
    List<ItemImage> Images { get; set; }
    List<PickLocation> PickLocations { get; set; }
    DateTime ReceivedDate { get; set; }
    int AreaId { get; set; }
    int PreviousLineStatusId { get; set; }
    int GetQuantityToBePicked();
}