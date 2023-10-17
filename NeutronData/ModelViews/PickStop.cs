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

        public PickStop()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickViews = new List<PickView>();
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
                //var pickLocation = new PickLocation {
                //    Inventory = CurrentInventoryLocation
                //    , Quantity = pickView.QuantityToBePicked
                //    , PickDate = DateTime.Now
                //    , RequestedQuantity = pickView.QuantityToBePicked
                //    , User = user};
                //pickView.PickLocations.Add(pickLocation);
                //pickView.PickedQty = pickView.PickLocations.Sum(p => p.Quantity);
                //pickView.QuantityToBePicked = pickView.Quantity - pickView.PickedQty;
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
                pickView.QuantityToBePicked = pickView.Quantity - pickView.PickedQty;
            
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
            var sb = new StringBuilder();
            sb.AppendLine($"Set Pick Views Complete, Update OrderDetail Record");
            try
            {
                foreach (var pickView in PickViews)
                {
                    var total = GetPickViewTotal(pickView);
                    pickView.OrderDetail.PickedQuantity = total;
                    pickView.OrderDetail.LineStatusId = (int)LineStatus.Complete;
                    pickView.OrderDetail.EmpId = user.EmpId;
                    _repoOrderDetails.Update(pickView.OrderDetail);
                    sb.AppendLine(
                        $"pickView: {pickView.Item}  Picked Qty: {pickView.OrderDetail.PickedQuantity} Line Status: {pickView.OrderDetail.LineStatusId}  Emp: {user.Fullname} ");
                }
            }
            catch (Exception ex)
            {
               _ = logger.LogDetailAsync($"Error Updating Order Details. {Environment.NewLine} {ex.Message} ");
            }
            _ = logger.LogDetailAsync($"{sb.ToString()}");
        }
    }
}
