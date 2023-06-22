using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System.Collections.Generic;
using System.Linq;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.Interfaces;

namespace NeutronData.Repositories
{
    public class ReplenOrderDetailsRepository
    {

        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly NeutronDb db = new NeutronDb();

        public ReplenOrderDetailsRepository()
        {
 
        }
        public List<ReplenOrderDetailsView> GetOrderDetailsView()
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderDetailsView> recs = _repoReplenOrderDetails.AllInclude(r => r.ReplenOrder, r => r.ItemDefinition)
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
                    LineStatusName = ((LineStatus)s.LineStatusId).GetEnumDescription(),
                    AreaId = s.AreaId
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
            IEnumerable<ReplenOrderDetailsView> recs = _repoReplenOrderDetails.AllInclude(r => r.ReplenOrder, r => r.ItemDefinition)
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
                    LineStatusName = ((LineStatus)s.LineStatusId).GetEnumDescription(),
                    AreaId = s.AreaId
                }).OrderBy(o => o.AreaId).ThenBy(p => p.Item);

            return recs.ToList();
        }

        //public List<ReplenOrderDetail> GetOrderDetailsByOrderAndWorkstation(int orderId, WorkstationView workstationView)
        //{
        //    var areaIds = workstationView.Areas.Select(r => r.Id).ToList();
        //    var orderDetails = new List<ReplenOrderDetail>();
        //    if (areaIds.Count <= 0) return orderDetails;
        //    {
        //        foreach (var areaId in areaIds)
        //        {
        //            var recs = _repoReplenOrderDetails.All().Where(r => r.ReplenOrderId == orderId && r.AreaId == areaId).ToList();
        //            orderDetails.AddRange(recs);
        //        }
        //    }
        //    return orderDetails;
        //}

        //public List<ReplenOrderDetail> GetOrderDetailsByOrderAndWorkstationNotCompleted(int orderId, WorkstationView workstationView)
        //{
        //    var areaIds = workstationView.Areas.Select(r => r.Id).ToList();
        //    var orderDetails = new List<ReplenOrderDetail>();
        //    if (areaIds.Count <= 0) return orderDetails;
        //    {
        //        foreach (var areaId in areaIds)
        //        {
        //            var recs = _repoReplenOrderDetails.All().Where(r => r.ReplenOrderId == orderId && r.AreaId == areaId
        //                && r.LineStatusId != (int)LineStatus.Complete).ToList();
        //            orderDetails.AddRange(recs);
        //        }
        //    }
        //    return orderDetails;
        //}
        public List<ReplenOrderDetail> GetOrderDetailsByOrderAndAreaNotCompleted(int orderId, int areaId)
        {
           // var areaIds = workstationView.Areas.Select(r => r.Id).ToList();
            //var orderDetails = new List<ReplenOrderDetail>();
            //if (areaIds.Count <= 0) return orderDetails;
            //{
                //foreach (var areaId in areaIds)
               // {
                    var orderDetails = _repoReplenOrderDetails.All().Where(r => r.ReplenOrderId == orderId && r.AreaId == areaId
                        && r.LineStatusId != (int)LineStatus.Complete).ToList();
                   // orderDetails.AddRange(recs);
               // }
            //}
            return orderDetails;
        }
    }
}
