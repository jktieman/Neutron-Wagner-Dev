using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class OrderDetailsRepository
    {
        private readonly GenericRepository<OrderDetail> repo = new GenericRepository<OrderDetail>(new NeutronDb());

        public List<OrderDetailsView> GetOrderDetailsView()
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderDetailsView> recs = repo.AllInclude(r => r.Order, r => r.ItemDefinition)
                .Where(r => statusToGet.Contains(r.LineStatusId))
                .Select(s => new OrderDetailsView
            {
                OrderId = s.OrderId,
                Ord1 = s.Order.Ord1,
                Ord2 = s.Order.Ord2,
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

        public List<OrderDetailsView> GetOrderDetailsViewByOrder(int orderId)
        {
            //var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderDetailsView> recs = repo.AllInclude(r => r.Order, r => r.ItemDefinition)
                .Where(r => r.OrderId == orderId).Select(s => new OrderDetailsView
                //.Where(r => r.OrderId == orderId && statusToGet.Contains(r.LineStatusId)).Select(s => new OrderDetailsView
                {
                    OrderId = s.OrderId,
                    Ord1 = s.Order.Ord1,
                    Ord2 = s.Order.Ord2,
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

        public List<OrderDetailsView> GetOrderDetailsViewByOrderAndStation(int orderId, int stationNumber)
        {
            //var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderDetailsView> recs = repo.AllInclude(r => r.Order, r => r.ItemDefinition)
                .Where(r => r.OrderId == orderId && r.StationNumber == stationNumber).Select(s => new OrderDetailsView
                    {
                        OrderId = s.OrderId,
                        Ord1 = s.Order.Ord1,
                        Ord2 = s.Order.Ord2,
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

        public List<OrderDetail> GetOrderDetailsByOrderAndStation(int orderId, int stationNumber)
        {
            var recs = repo.All().Where(r => r.OrderId == orderId && r.StationNumber == stationNumber).ToList();
            return recs.ToList();
        }
    }
}
