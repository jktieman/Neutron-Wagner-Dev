using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlliedLogger;
using NeutronCore.Enums;

namespace NeutronData.ModelViews
{
    public class PickStop
    {
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private IDynamicLogger _logger;

        public PickStop()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickViews = [];
        }
        public List<PickView> PickViews { get; set; }
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
            return QuantityToBePicked == 0;
        }

        public void UpdatePickViews(User user)
        {
            foreach (var pickView in PickViews)
            {
                UpdatePickView(pickView, user);
            }
        }

        public void UpdatePickView(PickView pickView, User user)
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
                pickView.QuantityToBePicked = pickView.Quantity - pickView.PickedQty <= 0
                    ? 0
                    : pickView.Quantity - pickView.PickedQty;


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

        public int GetQuantityToBePicked()
        {
            return GetTotalQuantityToBePicked() - GetPickedSoFar();
        }

        public int GetPickViewTotal(PickView pickview)
        {
            var total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }
            return total;
        }

        public void SetPickViewsComplete(User user, IDynamicLogger logger)
        {
            _logger = logger;

            var sb = new StringBuilder();
            sb.AppendLine($"Set Pick Views Complete, Update OrderDetail Record");
            try
            {
                foreach (var pickView in PickViews)
                {
                    var total = GetPickViewTotal(pickView);
                    pickView.OrderDetail.PickedQuantity = total;
                    pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                    
                    
                    // If this is a Transfer Order and the PickView is complete, then Create a new Replenishment Order
                    if (pickView.Ord1.Equals("TRANSFER", StringComparison.CurrentCultureIgnoreCase))
                    {
                        _ = _logger.LogDetailAsync($"Transfer Order: {pickView.Ord1}");
                        CreateReplenOrder(pickView);
                    }

                    pickView.OrderDetail.EmpId = user.EmpId;
                    _repoOrderDetails.Update(pickView.OrderDetail);
                    sb.AppendLine(
                        $"pickView: {pickView.Item}  Picked Qty: {pickView.OrderDetail.PickedQuantity} Line Status: {pickView.OrderDetail.LineStatusId}  Emp: {user.Fullname} ");
                }
            }
            catch (Exception ex)
            {
               _ = _logger.LogDetailAsync($"Error Updating Order Details. {Environment.NewLine} {ex.Message} ");
            }
            _ = _logger.LogDetailAsync($"{sb.ToString()}");
        }

        private void CreateReplenOrder(PickView pickView)
        {
            var sb = new StringBuilder();
            try
            {
                var replenOrder = new ReplenOrder
                {
                    Ord1 = pickView.Ord1,
                    Ord2 = pickView.Ord2,
                    Priority = 99,
                    OrderInfo = "Transfer Order",
                    LoadDate = DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = (int)OrderStatus.Available,
                };
                _repoReplenOrders.Insert(replenOrder);

                var replenOrderDetail = new ReplenOrderDetail
                {
                    ReplenOrderId = replenOrder.Id,
                    ItemDefinitionId = pickView.OrderDetail.ItemDefinitionId,
                    Quantity = pickView.OrderDetail.Quantity,
                    PickedQuantity = 0,
                    LineStatusId = (int)LineStatus.Available,
                    DateTime = DateTime.Now.ToString(),
                    OrderDetailInfo = "Transfer Order",
                    AreaId = pickView.OrderDetail.AreaId,
                    PartNum = pickView.OrderDetail.PartNum,
                    PartDesc = pickView.OrderDetail.PartDesc,
                    NewBin = pickView.OrderDetail.NewBin,
                    PrimeBin = pickView.OrderDetail.PrimeBin,
                };
                _repoReplenOrderDetails.Insert(replenOrderDetail);

                sb.AppendLine(
                    $"Create Replenishment Order: {replenOrder.Ord1}  Description: {replenOrderDetail.PartDesc}  Item: {replenOrderDetail.PartNum}  Qty: {replenOrderDetail.Quantity}");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error Updating Order Details. {Environment.NewLine} {ex.Message} ");
            }
            _ = _logger.LogDetailAsync($"{sb.ToString()}");
        }
    }
}
