using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class ReplenOrderDetailsRepository
    {
        private readonly GenericRepository<ReplenOrderDetail> repo = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly NeutronDb db = new NeutronDb();

        public List<ReplenOrderDetailsView> GetOrderDetailsView()
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderDetailsView> recs = repo.AllInclude(r => r.ReplenOrder, r => r.ItemDefinition)
                .Where(r => statusToGet.Contains(r.LineStatusId))
                .Select(s => new ReplenOrderDetailsView
                {
                    OrderId = s.ReplenOrderId,
                    Ord1 = s.ReplenOrder.Ord1,
                    Ord2 = s.ReplenOrder.Ord2,
                    OrderDetailId = s.Id,
                    ItemId = s.ItemDefinitionId,
                    Item = s.ItemDefinition.Item,
                    Description = s.ItemDefinition.Description,
                    Quantity = s.Quantity,
                    PickedQuantity = s.PickedQuantity,
                    LineStatusId = s.LineStatusId,
                    LineStatusName = s.LineStatus.Name,
                    StationNumber = s.StationNumber
                }).Where(s => statusToGet.Contains(s.LineStatusId))
            .OrderBy(o => o.Ord1);

            return recs.ToList();
        }

        //public List<ReplenOrderDetailsView> GetOrderDetailsViewByOrder(int orderId)
        //{
        //    var statusToGet = new int[] { 1, 2, 3, 4 };

        //    var recss = db.ReplenOrderDetails.Include("ReplenOrders").ToList();   //.Include("ReplenOrder").Include("ItemDefinition").ToList();


        //    IEnumerable<ReplenOrderDetailsView> recs = db.ReplenOrderDetails.Include("ReplenOrder").Include("ItemDefinition")
        //        .Where(r => r.ReplenOrderId == orderId && statusToGet.Contains(r.LineStatusId)).Select(s => new ReplenOrderDetailsView
        //        {
        //            OrderId = s.ReplenOrderId,
        //            Ord1 = s.ReplenOrder.Ord1,
        //            Ord2 = s.ReplenOrder.Ord2,
        //            OrderDetailId = s.Id,
        //            ItemId = s.ItemDefinitionId,
        //            Item = s.ItemDefinition.Item,
        //            Description = s.ItemDefinition.Description,
        //            Quantity = s.Quantity,
        //            PickedQuantity = s.PickedQuantity,
        //            LineStatusId = s.LineStatusId,
        //            LineStatusName = s.LineStatus.Name,
        //            StationNumber = s.StationNumber
        //        })
        //    .OrderBy(o => o.Ord1);

        //    return recs.ToList();
        //}

        public List<ReplenOrderDetailsView> GetOrderDetailsViewByOrder(int orderId)
        {
            //var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderDetailsView> recs = repo.AllInclude(r => r.ReplenOrder, r => r.ItemDefinition)
                .Where(r => r.ReplenOrderId == orderId).Select(s => new ReplenOrderDetailsView
                //.Where(r => r.OrderId == orderId && statusToGet.Contains(r.LineStatusId)).Select(s => new OrderDetailsView
                {
                    OrderId = s.ReplenOrderId,
                    Ord1 = s.ReplenOrder.Ord1,
                    Ord2 = s.ReplenOrder.Ord2,
                    OrderDetailId = s.Id,
                    ItemId = s.ItemDefinitionId,
                    Item = s.ItemDefinition.Item,
                    Description = s.ItemDefinition.Description,
                    Quantity = s.Quantity,
                    PickedQuantity = s.PickedQuantity,
                    LineStatusId = s.LineStatusId,
                    LineStatusName = s.LineStatus.Name,
                    StationNumber = s.StationNumber
                }).OrderBy(o => o.StationNumber).ThenBy(p => p.Item);

            return recs.ToList();
        }

        public List<ReplenOrderDetail> GetOrderDetailsByOrderAndStation(int orderId, int stationNumber)
        {
            var recs = repo.All().Where(r => r.ReplenOrderId == orderId && r.StationNumber == stationNumber).ToList();
            return recs.ToList();
        }
    }
}
