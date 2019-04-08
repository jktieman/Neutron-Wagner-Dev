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

namespace NeutronData.ModelViews
{
    public class PickStop : PickStopBase
    {
        private readonly GenericRepository<Order> repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());

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

        private int GetPickViewTotal(PickView pickview)
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
                    repoOrderDetails.Update(item.OrderDetail);

                    SetOrderComplete(item.OrderId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Updating Order Details.  " + ex.Message);
            }
        }

        public void SetPickViewsSkipped(User user)
        {

        }

        private void SetOrderComplete(int orderId)
        {
            Order order = repoOrders.FindByKey(orderId);
            List<OrderDetail> recs = repoOrderDetails.All().Where(d => d.OrderId == orderId && d.LineStatusId != 6).ToList();
            if (recs.Count == 0)
            {
                order.OrderStatusId = 6;
                repoOrders.Update(order);
            }
        }
    }
}
