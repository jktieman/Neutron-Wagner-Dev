using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronData.Repositories
{
    public class ReplenOrdersRepository
    {
        public GenericRepository<ReplenOrder> repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        public GenericRepository<ReplenOrderDetail> repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        public GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        public GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        public IEnumerable<ReplenOrderView> GetOrderView()
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderView> recs = repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Lines = s.Lines(),
                Pieces = s.Pieces(),
                LoadDate = DateTime.Now.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
                SearchField = string.Format("{0},{1},{2},{3}", s.Ord1, s.Ord2, s.OrderStatus.Name, s.ShipMethod.Name)
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetOrderView(string search)
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderView> recs = repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Lines = s.Lines(),
                Pieces = s.Pieces(),
                LoadDate = DateTime.Now.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
                SearchField = string.Format("{0},{1},{2},{3}", s.Ord1.ToLower()
                , s.Ord2.ToLower(), s.OrderStatus.Name.ToLower(), s.ShipMethod.Name.ToLower())
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Priority).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetAvailableOrders()
        {
            var statusToGet = new int[] { 1, 4 };
            IEnumerable<ReplenOrderView> recs = repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Lines = s.Lines(),
                Pieces = s.Pieces(),
                LoadDate = DateTime.Now.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
                SearchField = string.Format("{0},{1},{2},{3}", s.Ord1.ToLower()
                , s.Ord2.ToLower(), s.OrderStatus.Name.ToLower(), s.ShipMethod.Name.ToLower())
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetCompletedOrders()
        {
            var statusToGet = new int[] { 6 };
            IEnumerable<ReplenOrderView> recs = repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Lines = s.Lines(),
                Pieces = s.Pieces(),
                LoadDate = DateTime.Now.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
                SearchField = string.Format("{0},{1},{2},{3}", s.Ord1.ToLower()
                , s.Ord2.ToLower(), s.OrderStatus.Name.ToLower(), s.ShipMethod.Name.ToLower())
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public ReplenOrder GetOrder()
        {
            var ord = repoReplenOrders.All().FirstOrDefault();

            var lines = ord.Lines();
            var pieces = ord.Pieces();

            return ord;
        }

        public List<ReplenPickView> GetOrderLines()
        {
            var statusToGet = new int[] { 1, 4 };
            var pickViews = new List<ReplenPickView>();
            List<ReplenOrder> result = repoReplenOrders.AllInclude(n => n.ReplenOrderDetails).Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
            foreach (ReplenOrder ord in result)
            {
                List<ReplenOrderDetail> orderDetails = ord.ReplenOrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();

                foreach (var detail in orderDetails)
                {
                    var pickView = new ReplenPickView()
                    {
                        ReplenOrderId = detail.ReplenOrderId,
                        Ord1 = detail.ReplenOrder.Ord1,
                        Ord2 = detail.ReplenOrder.Ord2,
                        ItemId = detail.ItemDefinitionId,
                        Item = string.Empty,
                        Description = string.Empty,
                        Quantity = detail.Quantity,
                        QuantityToBePicked = detail.Quantity,
                        Slot = string.Empty,
                        SlotQty = 0,
                        OrderDetail = detail
                    };
                    pickViews.Add(pickView);
                }

            }
            foreach (var item in pickViews)
            {
                ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                if (def != null)
                {
                    item.Item = def.Item;
                    item.Description = def.Description;
                }
            }


            //Add Pick Location based on Inventory
            foreach (var item in pickViews)
            {
                Inventory rec = repoInventory.FindBy(f => f.ItemDefinitionId == item.ItemId).FirstOrDefault();
                if (rec != null)
                {
                    item.Slot = rec.Location.Slot;
                    item.SlotQty = rec.Quantity;
                }
            }

            return pickViews;
        }

        public List<ReplenPickView> GetOrderLines(List<BatchPosition> ordersToPick)
        {
            var pickViews = new List<ReplenPickView>();
            var statusToGet = new int[] { 1, 4 };
            int[] orderIds = GetOrderIdArray(ordersToPick);
            if (orderIds.Length > 0)
            {
                List<ReplenOrder> orders = repoReplenOrders.AllInclude(n => n.ReplenOrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                if (orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        int pos = GetPosition(order.Id, ordersToPick);
                        List<ReplenOrderDetail> orderDetails = order.ReplenOrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();
                        if (orderDetails.Count > 0)
                        {
                            foreach (var detail in orderDetails)
                            {
                                var pickView = new ReplenPickView()
                                {
                                    PickPosition = pos,
                                    ReplenOrderId = detail.ReplenOrderId,
                                    Ord1 = detail.ReplenOrder.Ord1,
                                    Ord2 = detail.ReplenOrder.Ord2,
                                    ItemId = detail.ItemDefinitionId,
                                    Item = string.Empty,
                                    Description = string.Empty,
                                    Quantity = detail.Quantity,
                                    QuantityToBePicked = detail.Quantity,
                                    Slot = string.Empty,
                                    SlotQty = 0,
                                    OrderDetail = detail
                                };
                                pickViews.Add(pickView);
                            }
                        }
                        foreach (var item in pickViews)
                        {
                            ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                            if (def != null)
                            {
                                item.Item = def.Item;
                                item.Description = def.Description;
                            }
                        }

                        //Add Pick Location based on Inventory
                        foreach (var item in pickViews)
                        {
                            Inventory rec = repoInventory.FindBy(f => f.ItemDefinitionId == item.ItemId).FirstOrDefault();
                            if (rec != null)
                            {
                                item.Slot = rec.Location.Slot;
                                item.SlotQty = rec.Quantity;
                            }
                        }
                    }
                }
            }
            return pickViews;
        }

        private List<Inventory> GetInventory(int itemId)
        {
            var recs = new List<Inventory>();

            recs = repoInventory.AllInclude(l => l.Location, l => l.ItemDefinition)
                .Where(f => f.ItemDefinitionId == itemId).ToList();
            return recs;
        }

        private int GetPosition(int id, List<BatchPosition> ordersToPick)
        {
            int result = 0;
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId == id)
                {
                    result = bp.PositionNumber;
                }
            }
            return result;
        }

        private int[] GetOrderIdArray(List<BatchPosition> ordersToPick)
        {
            var orderIds = new List<int>();
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }
            return orderIds.ToArray();
        }
    }
}
