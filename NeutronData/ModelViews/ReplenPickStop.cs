using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NeutronCore.Enums;

namespace NeutronData.ModelViews
{
    public class ReplenPickStop
    {
        private readonly GenericRepository<ReplenOrder> _repoOrders;
        private readonly GenericRepository<ReplenOrderDetail> _repoOrderDetails;
        public ReplenPickStop(Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _repoOrders = new GenericRepository<ReplenOrder>(contextFactory);
            _repoOrderDetails = new GenericRepository<ReplenOrderDetail>(contextFactory);
        }

        public List<ReplenPickView> PickViews { get; set; } = [];
        public int Sequence { get; set; }
        public int OrderId { get; set; }
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
        public List<Inventory> Inventory { get; set; } = [];
        public int InventoryIndex { get; set; }
        public int GroupBoxLocationInventoryIndex { get; set; }
        public Inventory CurrentInventoryLocation { get; set; }
        public int TotalQuantityInInventory { get; set; }
        public List<ItemImage> Images { get; set; } = [];

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
                    , Quantity = pickView.QuantityToBePicked
                    , PickDate = DateTime.Now
                    , RequestedQuantity = pickView.QuantityToBePicked
                    , User = user
                };
                pickView.PickLocations.Add(pickLocation);
                pickView.PickedQty = pickView.PickLocations.Sum(p => p.Quantity);
                pickView.QuantityToBePicked = pickView.Quantity - pickView.PickedQty;
            }
        }

        public int GetTotalQuantityToBePicked()
        {
            var total = 0;
            foreach (var pickview in PickViews)
            {
                total += pickview.QuantityToBePicked;
            }
            return total;
        }

        public int GetPickedSoFar()
        {
            var total = 0;
            foreach (var pickview in PickViews)
            {
                total += GetPickViewTotal(pickview);
            }
            return total;
        }

        public int GetPickViewTotal(ReplenPickView pickview)
        {
            var total = 0;
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
                    var total = GetPickViewTotal(item);
                    item.OrderDetail.PickedQuantity = total;
                    item.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                    item.OrderDetail.EmpId = user.EmpId;
                    _repoOrderDetails.Update(item.OrderDetail);
                    SetOrderComplete(item.OrderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error Updating Order Details.  {ex.Message}");
            }
        }

        private void SetOrderComplete(int orderId)
        {
            var order = _repoOrders.FindByKey(orderId);
            var recs = _repoOrderDetails.All(d
                => d.ReplenOrderId == orderId && d.LineStatusId != 6).ToList();
            if (recs.Count == 0)
            {
                order.OrderStatusId = 6;
                _repoOrders.Update(order);
            }
        }

        public void UpdatePickView(ReplenPickView pickView, User user)
        {
            var pickLocation = new PickLocation
            {
                Inventory = CurrentInventoryLocation
                ,
                Quantity = pickView.QuantityToBePicked
                ,
                PickDate = DateTime.Now
                ,
                RequestedQuantity = pickView.QuantityToBePicked
                ,
                User = user
            };
            pickView.PickLocations.Add(pickLocation);
            pickView.PickedQty = pickView.PickLocations.Sum(p => p.Quantity);
            pickView.QuantityToBePicked = pickView.Quantity - pickView.PickedQty;

        }
    }
}
