using NeutronData.BaseClasses;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;

namespace NeutronData.ModelViews
{
    public class PickStop : PickStopBase
    {
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());

        public PickStop()
        {
            Inventory = new List<Inventory>();
            Images = new List<ItemImage>();
            PickViews = new List<PickView>();
        }
        public List<PickView> PickViews { get; set; }
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
                var pickLocation = new PickLocation {
                    Inventory = CurrentInventoryLocation
                    , Quantity = pickView.QuantityToBePicked
                    , PickDate = DateTime.Now
                    , RequestedQuantity = pickView.QuantityToBePicked
                    , User = user};
                pickView.PickLocations.Add(pickLocation);
                pickView.PickedQty = pickView.PickLocations.Sum(p => p.Quantity);
                // save this for the close when  all it picked or all that's going to be picked
                //pickView.OrderDetail.PickedQuantity = pickView.PickedQty;
                pickView.QuantityToBePicked = pickView.GetQuantityToBePicked();
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

        private int GetPickedSoFar()
        {
            var total = 0;
            foreach (var pickview in PickViews)
            {
                total += GetPickViewTotal(pickview);
            }
            return total;
        }

        private int GetPickViewTotal(PickView pickview)
        {
            var total = 0;
            foreach (var pickLocation in pickview.PickLocations)
            {
                total += pickLocation.Quantity;
            }
            return total;
        }

        public void SetPickViewsComplete(User user, DynamicLogger logger)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Set Pick Views Complete, Update OrderDetail Record");
            try
            {
                foreach (var item in PickViews)
                {
                    var total = GetPickViewTotal(item);
                    item.OrderDetail.PickedQuantity = total;
                    item.OrderDetail.LineStatusId = 6;
                    item.OrderDetail.EmpId = user.EmpId;
                    _repoOrderDetails.Update(item.OrderDetail);
                    sb.AppendLine(
                        $"Item: {item.Item}  Picked Qty: {item.OrderDetail.PickedQuantity} Line Status: {item.OrderDetail.LineStatusId}  Emp: {user.Fullname} ");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error Updating Order Details. {Environment.NewLine} {ex.Message} ");
            }
            logger.Log($"{sb.ToString()}");
        }
    }
}
