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
    public class OrdersRepository
    {
        public GenericRepository<Order> repo = new GenericRepository<Order>(new NeutronDb());
        public GenericRepository<OrderDetail> repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        public GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        public GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        public IEnumerable<OrderView> GetOrderView()
        {
            // var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderView> recs = repo.AllInclude(r => r.OrderDetails)
                // .Where(r => statusToGet.Contains(r.OrderStatusId))
                .Select(s => new OrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    Order = s,
                    Station_1_HasPicks = CheckForPicks3(1, s.OrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.OrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.OrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.OrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.OrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.OrderDetails),
                    LoadDate = s.LoadDate.ToString(),
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
            .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<OrderView> GetOrderView(string search)
        {
            var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderView> recs = repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                Order = s,
                Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
                LoadDate = s.LoadDate.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderBy(o => o.Ord1).ToList();
            IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }

        public IEnumerable<OrderView> GetAvailableOrders(StationView station, string search)
        {
            var availableRecs = new List<OrderView>();
            IEnumerable<OrderView> recs = null;
            try
            {
                recs = repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Starter = StartOnThisStation(station.StationNumber, s.OrderDetails),
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    Order = s,
                    Station_1_HasPicks = CheckForPicks3(1, s.OrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.OrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.OrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.OrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.OrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.OrderDetails),
                    FirstPickStation = GetFirstPickStation(s.OrderDetails),
                    LoadDate = s.LoadDate.ToString(),
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                   .OrderByDescending(o => o.Priority).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetAvailableOrders Error Phase 1 Recs Count: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
            }

            try
            {
                foreach (var item in recs)
                {
                    if (!string.IsNullOrEmpty(item.Station_1_HasPicks))
                    {
                        if (!string.Equals(item.Station_1_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 1;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
                    {
                        if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 2;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
                    {
                        if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 3;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
                    {
                        if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 4;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
                    {
                        if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 5;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
                    {
                        if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickStation = 8;
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign Next Station: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
            }
           
            try
            {
                foreach (var item in recs.Where(r => r.CurrentPickStation == station.StationNumber))
                {
                    List<OrderDetail> details = item.Order.OrderDetails.Where(r => r.StationNumber == station.StationNumber).ToList();
                    item.Order.OrderDetails = details;
                    availableRecs.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign Detail Lines this Station - {station.StationNumber}: Rec Count: {recs.Count()}\r\n  {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
            }

            IEnumerable<OrderView> result = availableRecs.Where(s => s.SearchField.Contains(search));

            return result;
        }

        private string StartOnThisStation(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            string result = string.Empty;
            try
            {
                if (orderDetails.Count > 0 && stationNumber > 1)
                {
                    int firstPickStation = orderDetails.Min(o => o.StationNumber);
                    if (firstPickStation == stationNumber)
                    {
                        result = @"S";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start on this Station Error.  {ex.Message} \r\n {ex.InnerException}");
            }
            return result;
        }

        private int GetFirstPickStation(ICollection<OrderDetail> orderDetails)
        {
            return orderDetails.Min(o => o.StationNumber);
        }

        public IEnumerable<OrderView> GetCompletedOrders()
        {
            IEnumerable<OrderView> recs = repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                Order = s,
                Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
                LoadDate = s.LoadDate.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => r.OrderStatusId == 6)
            .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public Order GetOrder()
        {
            var ord = repo.All().FirstOrDefault();

            var lines = ord.Lines();
            var pieces = ord.Pieces();

            return ord;
        }

        public List<PickView> GetOrderLines()
        {
            var statusToGet = new int[] { 1, 4 };
            var pickList = new List<PickView>();
            List<Order> result = repo.AllInclude(n => n.OrderDetails).Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
            foreach (Order ord in result)
            {
                List<OrderDetail> orderDetails = ord.OrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();

                foreach (var detail in orderDetails)
                {
                    var pickView = new PickView()
                    {
                        OrderId = detail.OrderId,
                        Ord1 = detail.Order.Ord1,
                        Ord2 = detail.Order.Ord2,
                        ItemId = detail.ItemDefinitionId,
                        Item = string.Empty,
                        Description = string.Empty,
                        Quantity = detail.Quantity,
                        QuantityToBePicked = detail.Quantity,
                        Slot = string.Empty,
                        SlotQty = 0,
                        OrderDetail = detail,
                        StationNumber = detail.StationNumber
                    };
                    pickList.Add(pickView);
                }

            }
            //Add Item definition
            //Add Pick Location based on Inventory
            foreach (var item in pickList)
            {
                ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                if (def != null)
                {
                    item.Item = def.Item;
                    item.Description = def.Description;
                }
            }


            //Add Pick Location based on Inventory
            foreach (var item in pickList)
            {
                Inventory rec = repoInventory.FindBy(f => f.ItemDefinitionId == item.ItemId).FirstOrDefault();
                if (rec != null)
                {
                    item.Slot = rec.Location.Slot;
                    item.SlotQty = rec.Quantity;
                }
            }

            return pickList.OrderBy(o => o.Slot).ToList();
        }

        public IEnumerable<OrderView> GetAvailableOrders(string search)
        {
            var statusToGet = new int[] { 1, 4 };
            IEnumerable<OrderView> recs = repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                Order = s,
                Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
                LoadDate = s.LoadDate.ToString(),
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Ord1).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        private string CheckForPicks(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 6 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.StationNumber == stationNumber)
                {
                    if (statusToGet.Contains(item.Order.OrderStatusId))
                    {
                        result = item.Order.OrderStatus.Name.Substring(0, 1);
                        break;
                    }
                    else
                    {
                        result = @"-";
                        break;
                    }
                }
            }

            return result;
        }

        private string CheckForPicks2(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 6 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.StationNumber == stationNumber)
                {
                    if (statusToGet.Contains(item.Order.OrderStatusId))
                    {
                        result = item.Order.OrderStatus.Name.Substring(0, 1);
                        break;
                    }
                    else
                    {
                        result = @"-";
                        break;
                    }
                }
            }

            return result;
        }

        private string CheckForPicks3(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            var lineStatus = new List<int>();
            string result = @"";
            if (orderDetails.Count > 0)
            {
                foreach (var item in orderDetails)
                {
                    if (item.StationNumber == stationNumber)
                    {
                        lineStatus.Add(item.LineStatusId);
                    }
                }

                if (lineStatus.Count > 0 && lineStatus.Sum() > 0)
                {
                    int x = lineStatus.Sum();

                    if (x == (lineStatus.Count * 1))  // All 1's - Available
                    {
                        result = @"A";
                    }
                    else
                    {
                        if (lineStatus.Sum() == lineStatus.Count * 2)  // All 2's - Hold
                        {
                            result = @"H";
                        }
                        else
                        {
                            if (lineStatus.Sum() == lineStatus.Count * 6)  //All 6's - Complete 
                            {
                                result = @"C";
                            }
                            else
                            {
                                result = @"-";

                            }
                        }
                    }
                }
            }
            return result;
        }


        public List<PickView> GetOrderLines(List<BatchPosition> ordersToPick)
        {
            var pickViews = new List<PickView>();
            var statusToGet = new int[] { 1, 4 };
            int[] orderIds = GetOrderIdArray(ordersToPick);

            if (orderIds.Length > 0)
            {
                List<Order> orders = repo.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                if (orders.Count > 0)
                {
                    foreach (Order ord in orders)
                    {
                        int pos = GetPosition(ord.Id, ordersToPick);
                        List<OrderDetail> orderDetails = ord.OrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();

                        if (orderDetails.Count > 0)
                        {
                            foreach (var detail in orderDetails)
                            {
                                var pickView = new PickView()
                                {
                                    PickPosition = pos,
                                    OrderId = detail.Order.Id,
                                    Ord1 = detail.Order.Ord1,
                                    Ord2 = detail.Order.Ord2,
                                    ItemId = detail.ItemDefinitionId,
                                    Item = string.Empty,
                                    Description = string.Empty,
                                    Quantity = detail.Quantity,
                                    QuantityToBePicked = detail.Quantity,
                                    PickedQty = 0,
                                    Slot = string.Empty,
                                    SlotQty = 0,
                                    OrderDetail = detail,
                                    StationNumber = detail.StationNumber
                                };
                                pickViews.Add(pickView);
                            }
                        }
                    }
                    //Add Item definition
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

            return pickViews;
        }

        public IEnumerable<PickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum)
        {
            var pickViews = new List<PickView>();
            var statusToGet = new int[] { 1, 4 };
            int[] orderIds = GetOrderIdArray(ordersToPick);
            ItemDefinition newItemDefinition = repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            if (newItemDefinition != null)
            {
                if (orderIds.Length > 0)
                {
                    List<Order> orders = repo.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                    if (orders.Count > 0)
                    {
                        foreach (Order ord in orders)
                        {
                            //put the ItemDefinition back to a New Item
                            List<OrderDetail> orderDetailsToUpdate = repoOrderDetails.FindBy(r => r.PartNum == partNum && r.OrderId == ord.Id).ToList();
                            if (orderDetailsToUpdate.Count > 0)
                            {
                                foreach (var od in orderDetailsToUpdate)
                                {
                                    od.ItemDefinition = newItemDefinition;
                                    od.ItemDefinitionId = newItemDefinition.Id;
                                    repoOrderDetails.Update(od);
                                }

                                int pos = GetPosition(ord.Id, ordersToPick);
                                List<OrderDetail> orderDetails = repoOrderDetails.FindBy(o => statusToGet.Contains(o.LineStatusId) && o.ItemDefinitionId == newItemDefinition.Id).ToList();

                                if (orderDetails.Count > 0)
                                {
                                    foreach (var detail in orderDetails)
                                    {
                                        var pickView = new PickView()
                                        {
                                            PickPosition = pos,
                                            OrderId = detail.Order.Id,
                                            Ord1 = detail.Order.Ord1,
                                            Ord2 = detail.Order.Ord2,
                                            ItemId = detail.ItemDefinitionId,
                                            Item = string.Empty,
                                            Description = string.Empty,
                                            Quantity = detail.Quantity,
                                            QuantityToBePicked = detail.Quantity,
                                            PickedQty = 0,
                                            Slot = string.Empty,
                                            SlotQty = 0,
                                            OrderDetail = detail,
                                            StationNumber = detail.StationNumber
                                        };
                                        ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
                                        if (def != null)
                                        {
                                            pickView.Item = def.Item;
                                            pickView.Description = def.Description;
                                        }
                                        pickViews.Add(pickView);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return pickViews.ToList();
        }

        //public PickView CreatePickView(int pos, OrderDetail detail)
        //{

        //    var pickView = new PickView()
        //    {
        //        PickPosition = pos,
        //        OrderId = detail.Order.Id,
        //        Ord1 = detail.Order.Ord1,
        //        Ord2 = detail.Order.Ord2,
        //        ItemId = detail.ItemDefinitionId,
        //        Item = string.Empty,
        //        Description = string.Empty,
        //        Quantity = detail.Quantity,
        //        QuantityToBePicked = detail.Quantity,
        //        PickedQty = 0,
        //        Slot = string.Empty,
        //        SlotQty = 0,
        //        OrderDetail = detail
        //    };

        //    ItemDefinition def = repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
        //    if (def != null)
        //    {
        //        pickView.Item = def.Item;
        //        pickView.Description = def.Description;
        //    }

        //    return pickView;
        //}

        private int GetPosition(int id, List<BatchPosition> ordersToPick)
        {
            int result = 0;
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId == id)
                {
                    result = bp.PositionNumber;
                    break;
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
