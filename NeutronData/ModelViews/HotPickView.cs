using NeutronData.Models;
using System.Collections.Generic;

namespace NeutronData.ModelViews
{
    public class HotPickView
    {
        public HotPickView()
        {
            Inventory = new List<Inventory>();
        }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int PickedQty { get; set; }
        public string Slot { get; set; }
        public int SlotQty { get; set; }
        public List<Inventory> Inventory { get; set; }
        public int InventoryIndex { get; set; }
        public Inventory CurrentInventoryLocation { get; set; }
        public int TotalQuantityInInventory { get; set; }
    }

}
