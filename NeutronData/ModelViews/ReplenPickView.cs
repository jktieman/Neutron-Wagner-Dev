using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class ReplenPickView
    {
        public ReplenPickView()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickLocations = new List<PickLocation>();
        }
        public int Sequence { get; set; }
        public int PickPosition { get; set; }
        public int ReplenOrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string UnitOfIssue { get; set; }
        public int Quantity { get; set; }
        public int QuantityToBePicked { get; set; }
        public int PickedQty { get; set; }
        public string Slot { get; set; }
        public int SlotQty { get; set; }
        public ReplenOrderDetail OrderDetail { get; set; }
        public List<Inventory> Inventory { get; set; }
        public int InventoryIndex { get; set; }
        public Inventory CurrentInventoryLocation { get; set; }
        public int TotalQuantityInInventory { get; set; }
        public List<ItemImage> Images { get; set; }
        public List<PickLocation> PickLocations { get; set; }
        public string ReceivedDate { get; set; }
        public int StationNumber { get; set; }

        public int GetQuantityToBePicked()
        {
            int result = Quantity - PickedQty;
            if (result > 0)
            {
                return result;
            }
            return 0;
        }
    }
}
