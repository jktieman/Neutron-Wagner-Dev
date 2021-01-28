using System.ComponentModel;

namespace NeutronCore.Enums
{
    public enum ActionCode
    {
        [Description("Pick - Order")]
        PickOrder = 1,
        [Description("Store - Order")]
        StoreOrder = 2,
        [Description("Pick - Hot")]
        PickHot = 3,
        [Description("Store - Hot")]
        StoreHot = 4,
        [Description("Pick - Rack")]
        PickRack = 5,
        [Description("Store - Rack")]
        StoreRack = 7,
        [Description("Transfer - From")]
        TransferFrom = 8,
        [Description("Transfer - To")]
        TransferTo = 9,
        [Description("Location - Add")]
        LocationAdd = 10,
        [Description("Location - Modify")]
        LocationModify = 11,
        [Description("Location - Delete")]
        LocationDelete = 12,
        [Description("Item - Add")]
        ItemAdd = 13,
        [Description("Item - Modify")]
        ItemModify = 14,
        [Description("Item - Delete")]
        ItemDelete = 15,
        [Description("Item - RTS")]
        ItemRts = 16,
        [Description("Inventory - Add")]
        InventoryAdd = 17,
        [Description("Inventory - Modify")]
        InventoryModify = 18,
        [Description("Inventory - Delete")]
        InventoryDelete = 19,
        [Description("New Item Request")]
        NewItemRequest = 20,
        [Description("Location Count")]
        LocationCount = 21,
        [Description("Cycle Count")]
        CycleCount = 22,
        [Description("Change Pick Quantity")]
        ChangePickLocation = 23,
        [Description("Hold Order")]
        HoldOrder = 24,
        [Description("Release Order")]
        ReleaseOrder = 25,
        [Description("Partial Order")]
        PartialOrder = 26,
        [Description("Order Complete")]
        OrderComplete = 27,
        [Description("Change Priority")]
        ChangePriority = 28,
        [Description("Order - RTS")]
        OrderRts = 29,
        [Description("Order Archive")]
        OrderArchived = 30,
        [Description("Order Detail - Complete")]
        OrderDetailComplete = 31,
        [Description("Order Detail - RTS")]
        OrderDetailRts = 32,
        [Description("Order Detail - Archive")]
        OrderDetailArchive = 33,
        [Description("Hold Line")]
        HoldLine = 34,
        [Description("Release Line")]
        ReleaseLine = 35,
        [Description("Replen Order - Delete")]
        ReplenOrderDelete = 36,
        [Description("Replen Detail - Delete")]
        ReplenDetailDelete = 37,
        [Description("Warranty - Hot Pick")]
        WarrantyHotPick = 39,
        [Description("Scrap - Hot Pick")]
        ScrapHotPick = 40,
        [Description("Other - Hot Pick")]
        OtherHotPick = 41,
        [Description("Warranty - Hot Store")]
        WarrantyHotStore = 42,
        [Description("Scrap - Hot Store")]
        ScrapHotStore = 43,
        [Description("Other - Hot Store")]
        OtherHotStore = 44,
        [Description("Skip")]
        Skip = 45,
        [Description("Cost Center - Hot Pick")]
        CostCenterHotPick = 46,
        [Description("Cost Center - Hot Store")]
        CostCenterHotStore = 47,
        [Description("Order - Delete")]
        OrderDelete = 48,
        [Description("Order Detail - Delete")]
        OrderDetailDelete = 49,
        [Description("Skip Replen")]
        SkipReplen = 50,
        [Description("Skip Pick")]
        SkipPick = 51,
        [Description("Rack Adjust")]
        RackAdjust = 52,
        [Description("Kill Line")]
        KillLine = 53,
        [Description("Kill Order")]
        KillOrder = 54
    }
}
