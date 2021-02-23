using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NeutronData.ModelViews
{
    public class ReplenPickStop
    {
        private readonly GenericRepository<ReplenOrder> _repoOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());

        public ReplenPickStop()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickViews = new List<ReplenPickView>();
        }
        public List<ReplenPickView> PickViews { get; set; }
        public int Sequence { get; set; }
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string UnitOfIssue { get; set; }
        public int Quantity { get; set; }
        public int QuantityToBePicked => Quantity - PickedQty > 0 ? Quantity - PickedQty : 0;
        public int PickedQty { get; set; }
        public string Slot { get; set; }
        public int SlotQty { get; set; }
        public List<Inventory> Inventory { get; set; }
        public int InventoryIndex { get; set; }
        public int GroupBoxLocationInventoryIndex { get; set; }
        public Inventory CurrentInventoryLocation { get; set; }
        public int TotalQuantityInInventory { get; set; }
        public List<ItemImage> Images { get; set; }

        public bool Skipped { get; set; }
        public string ItemKey { get; set; }

        public bool StopComplete()
        {
            if (QuantityToBePicked == 0)
            {
                return true;
            }
            return false;
        }

        public void UpdatePickViews(User user)
        {
            foreach (var pickView in PickViews)
            {
                var pickLocation = new PickLocation
                {
                    Inventory = CurrentInventoryLocation
                    , Quantity = pickView.QuantityThisPick
                    , PickDate = DateTime.Now
                    , RequestedQuantity = pickView.Quantity
                    , User = user
                };
                pickView.PickLocations.Add(pickLocation);
                pickView.PickedQty = pickView.PickLocations.Sum(p => p.Quantity);
                // save this for the close when  all it picked or all that's going to be picked
                //pickView.OrderDetail.PickedQuantity = pickView.PickedQty;
                //pickView.QuantityToBePicked = pickView.GetQuantityToBePicked();
            }
        }

        public int GetTotalQuantityToBePicked()
        {
            int total = 0;
            foreach (var pickview in PickViews)
            {
                total += pickview.QuantityToBePicked;
            }
            return total;
        }

        private int GetPickedSoFar()
        {
            int total = 0;
            foreach (var pickview in PickViews)
            {
                total += GetPickViewTotal(pickview);
            }
            return total;
        }

        private int GetPickViewTotal(ReplenPickView pickview)
        {
            int total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }
            return total;
        }

        public void SetPickViewsComplete(User user)
        {
            try
            {
                foreach (var item in PickViews)
                {
                    int total = GetPickViewTotal(item);
                    item.OrderDetail.PickedQuantity = total;
                    item.OrderDetail.LineStatusId = 6;
                    item.OrderDetail.EmpId = user.EmpId;
                    _repoOrderDetails.Update(item.OrderDetail);
                    SetOrderComplete(item.OrderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Updating Order Details.  " + ex.Message);
            }
        }

        private void SetOrderComplete(int orderId)
        {
            ReplenOrder order = _repoOrders.FindByKey(orderId);
            List <ReplenOrderDetail> recs = _repoOrderDetails.All().Where(d => d.ReplenOrderId == orderId && d.LineStatusId != 6).ToList();
            if (recs.Count == 0)
            {
                order.OrderStatusId = 6;
                _repoOrders.Update(order);
            }
        }
    }
}
