using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Extensions;
using NeutronData.SqlModelViews;

namespace NeutronData.Repositories
{
    public class OrdersRepository
    {
        private readonly GenericRepository<Order> _repo = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        public Order GetOrder(int id)
        {
           Order ord = null;

            using (var db = new NeutronDb())
            {
                ord = db.Orders.Include("OrderDetails").FirstOrDefault(r => r.Id == id);

            }
            return ord;

        }

        public IEnumerable<OrderView> GetOrderViewNotCompleted()
        {
            // var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails)
                // .Where(r => statusToGet.Contains(r.OrderStatusId))
                .Where(r => r.OrderStatusId != 6)
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
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<OrderView> GetOrderViewNotCompleted(string search)
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails)
                .Where(r => r.OrderStatusId != 6)
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
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                .OrderBy(o => o.Ord1).ToList();
            IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }

        public IEnumerable<OrderView> GetOrderView()
        {
            // var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails)
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
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
            .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<OrderView> GetOrderView(string search)
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            })
            .OrderBy(o => o.Ord1).ToList();
            IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }

        public List<AvailableOrdersView> GetAvailableOrders(StationView station, string search, bool serialPicking, bool showSkips = false)
        {
            var recs = new List<AvailableOrdersView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    List<OrderDetail> records;
                    //if (showSkips)
                    //{
                    //    records = context.OrderDetails.Where(o => o.StationNumber == station.StationNumber
                    //                                              && (o.LineStatusId == 1 || o.LineStatusId == 9))
                    //        .ToList();
                    //}
                    //else
                    //{
                    //    records = context.OrderDetails.Where(o => o.StationNumber == station.StationNumber && o.LineStatusId == 1)
                    //        .ToList();
                    //}


                    if (showSkips)
                    {
                        records = context.OrderDetails.Include("Order").Where(o => o.StationNumber == station.StationNumber && (o.Order.OrderStatusId == 1))
                            .Where(p => p.LineStatusId == 1 || p.LineStatusId == 9).ToList();
                    }
                    else
                    {
                        records = context.OrderDetails.Include("Order").Where(o => o.StationNumber == station.StationNumber && (o.Order.OrderStatusId == 1))
                            .Where(p => p.LineStatusId == 1).ToList();
                    }


                    var ords = records.GroupBy( r => new { r.OrderId, r.Order.Ord1, r.Order.Ord2, r.Order.Priority, r.Order.LoadDate })
                         .Select(r => new AvailableOrdersView
                         {
                             Id = r.Key.OrderId
                             ,
                             Ord1 = r.Key.Ord1
                             ,
                             Ord2 = r.Key.Ord2
                             ,
                             Priority = r.Key.Priority
                             ,
                             Lines = r.Count()
                             ,
                             Pieces = r.Sum(s => s.Quantity)
                             ,
                             LoadDate = r.Key.LoadDate
                            

                         }).ToList();

                    //var ords = records
                    //    .Select(r => new AvailableOrdersView
                    //    {
                    //        Id = r.Id
                    //        ,
                    //        Ord1 = r.Order.Ord1
                    //        ,
                    //        Ord2 = r.Order.Ord2
                    //        ,
                    //        Priority = r.Order.Priority
                    //        ,
                    //        Lines = r.Order.OrderDetails.Count()
                    //        ,
                    //        Pieces = r.Order.OrderDetails.Sum(s => s.Quantity)
                    //        ,
                    //        LoadDate = r.Order.LoadDate)
                    //        , Order = r.Order

                    //    }).Where(s => s.Ord1.Contains(search) || s.Ord2.Contains(search)) .ToList();

                    foreach (var ord in ords)
                    {
                        ord.Order =  context.Orders.Find(ord.Id);
                    }

                    foreach (var ord in ords)
                    {
                        foreach (var detail in ord.Order.OrderDetails)
                        {
                            detail.ItemDefinition = context.ItemDefinitions
                                .Include("UnitOfIssue")
                                .Include("SizeCode")
                                .Include("HeightCode")
                                .Include("VelocityCode")
                                .Include("Station")
                                .FirstOrDefault(d => d.Id == detail.ItemDefinitionId);
                        }
                    }

                    recs = !string.IsNullOrEmpty(search) ? ords.Where(o => o.Ord1.ToLower().Contains(search) || o.Ord2.ToLower().Contains(search)).ToList() : ords;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get AvailableOrders View Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }


        public IEnumerable<OrderView> GetAvailableOrders__OLD
            (StationView station, string search, bool serialPicking)
        {
            var station1Orders = new List<OrderView>();
            var station2Orders = new List<OrderView>();
            var station3Orders = new List<OrderView>();
            var station4Orders = new List<OrderView>();
            var station5Orders = new List<OrderView>();
            var station8Orders = new List<OrderView>();
            var result = new List<OrderView>();
            var availableRecs = new List<OrderView>();
            var recs = new List<OrderView>();  // null;
            try
            {
                recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Starter = "1", // StartOnThisStation(station.StationNumber, s.OrderDetails),
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
                    LoadDate = s.LoadDate,
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
                            station1Orders.Add(item);
                            item.CurrentPickStation = 1;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
                    {
                        if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            station2Orders.Add(item);
                            item.CurrentPickStation = 2;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
                    {
                        if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            station3Orders.Add(item);
                            item.CurrentPickStation = 3;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
                    {
                        if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            station4Orders.Add(item);
                            item.CurrentPickStation = 4;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
                    {
                        if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            station5Orders.Add(item);
                            item.CurrentPickStation = 5;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
                    {
                        if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            station8Orders.Add(item);
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

            // Serial Picking
            if (serialPicking)
            {
                try
                {
                    foreach (var item in recs.Where(r => r.CurrentPickStation == station.StationNumber))
                    {
                        var details = item.Order.OrderDetails.Where(r => r.StationNumber == station.StationNumber).ToList();
                        item.Order.OrderDetails = details;
                        availableRecs.Add(item);
                    }
                    result = availableRecs.Where(s => s.SearchField.Contains(search)).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Assign Detail Lines this Station - {station.StationNumber}: Rec Count: {recs.Count()}\r\n  {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
                }
                //End Serial Picking
            }
            //Parallel Picking
            else
            {
                station1Orders.Clear();
                station2Orders.Clear();
                station3Orders.Clear();
                station4Orders.Clear();
                station5Orders.Clear();
                station8Orders.Clear();
                try
                {
                    foreach (var item in recs)
                    {
                        if (!string.IsNullOrEmpty(item.Station_1_HasPicks))
                        {
                            if (!string.Equals(item.Station_1_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station1Orders.Add(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
                        {
                            if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station2Orders.Add(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
                        {
                            if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station3Orders.Add(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
                        {
                            if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station4Orders.Add(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
                        {
                            if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station5Orders.Add(item);
                            }
                        }

                        if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
                        {
                            if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                            {
                                station8Orders.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Assign Next Station: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
                }

                switch (station.StationNumber)
                {
                    case 1:
                        {
                            result = station1Orders;
                            break;
                        }
                    case 2:
                        {
                            result = station2Orders;
                            break;
                        }
                    case 3:
                        {
                            result = station3Orders;
                            break;
                        }
                    case 4:
                        {
                            result = station4Orders;
                            break;
                        }
                    case 5:
                        {
                            result = station5Orders;
                            break;
                        }
                    case 8:
                        {
                            result = station8Orders;
                            break;
                        }
                }
                result = result.Where(s => s.SearchField.Contains(search)).ToList();
            }

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
            var result = 0;
            if (orderDetails.Count <= 0) return result;
            result = orderDetails.Min(o => o.StationNumber);
            return result;
        }

        public IEnumerable<OrderView> GetCompletedOrders()
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => r.OrderStatusId == 6)
            .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public IEnumerable<OrderView> GetCompletedOrders(string search)
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).Where(r => r.OrderStatusId == 6 && r.SearchField.Contains(search))
                .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public Order GetOrder()
        {
            var ord = _repo.All().FirstOrDefault();

            if (ord != null)
            {
                var lines = ord.Lines();
            }

            if (ord != null)
            {
                var pieces = ord.Pieces();
            }

            return ord;
        }

        public List<PickView> GetOrderLines()
        {
            var statusToGet = new int[] { 1, 4, 9 };
            var pickList = new List<PickView>();
            List<Order> result = _repo.AllInclude(n => n.OrderDetails).Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
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
                ItemDefinition def = _repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                if (def != null)
                {
                    item.Item = def.Item;
                    item.Description = def.Description;
                }
            }


            //Add Pick Location based on Inventory
            foreach (var item in pickList)
            {
                Inventory rec = _repoInventory.FindBy(f => f.ItemDefinitionId == item.ItemId).FirstOrDefault();
                if (rec != null)
                {
                    item.Slot = rec.Location.Slot;
                    item.SlotQty = rec.Quantity;
                }
            }

            return pickList.OrderBy(o => o.Slot).ToList();
        }

        public IEnumerable<OrderView> GetAvailableOrders(string search = "")
        {
            var statusToGet = new int[] { 1, 4 };
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Ord1).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        private string CheckForPicks(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 3, 6, 9 };
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


                //if (orderDetails.First().Order.Ord2 == "2208222")
                //{
                //     MessageBox.Show("order 55555");
                //}
                if (orderDetails.Count > 0)
                {
                    foreach (var item in orderDetails)
                    {
                        if (item.StationNumber == stationNumber)
                        {
                            if (item.LineStatusId == 9)
                            {
                                result = @"S";
                                return result;
                            }
                            lineStatus.Add(item.LineStatusId);
                        }
                    }

                    if (lineStatus.Count > 0 && lineStatus.Sum() > 0 && result == @"")
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
                                if (lineStatus.Sum() == lineStatus.Count * 3)  // All 2's - Hold
                                {

                                    result = @"P";
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
                }
            }
            return result;
        }


        public List<PickView> GetOrderLines(List<BatchPosition> ordersToPick)
        {
            var pickViews = new List<PickView>();
            var statusToGet = new int[] { 1, 4, 9 };
            int[] orderIds = GetOrderIdArray(ordersToPick);

            if (orderIds.Length > 0)
            {
                List<Order> orders = _repo.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
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
                        ItemDefinition def = _repoItemDefinition.FindBy(f => f.Id == item.ItemId).FirstOrDefault();
                        if (def != null)
                        {
                            item.Item = def.Item;
                            item.Description = def.Description;
                        }
                    }

                    //Add Pick Location based on Inventory
                    foreach (var item in pickViews)
                    {
                        Inventory rec = _repoInventory.FindBy(f => f.ItemDefinitionId == item.ItemId).FirstOrDefault();
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
            var statusToGet = new int[] { 1, 4, 9 };
            int[] orderIds = GetOrderIdArray(ordersToPick);
            ItemDefinition newItemDefinition = _repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            if (newItemDefinition != null)
            {
                if (orderIds.Length > 0)
                {
                    List<Order> orders = _repo.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                    if (orders.Count > 0)
                    {
                        foreach (Order ord in orders)
                        {
                            //put the ItemDefinition back to a New Item
                            List<OrderDetail> orderDetailsToUpdate = _repoOrderDetails.FindBy(r => r.PartNum == partNum && r.OrderId == ord.Id).ToList();
                            if (orderDetailsToUpdate.Count > 0)
                            {
                                foreach (var od in orderDetailsToUpdate)
                                {
                                    od.ItemDefinition = newItemDefinition;
                                    od.ItemDefinitionId = newItemDefinition.Id;
                                    _repoOrderDetails.Update(od);
                                }

                                int pos = GetPosition(ord.Id, ordersToPick);
                                List<OrderDetail> orderDetails = _repoOrderDetails.FindBy(o => statusToGet.Contains(o.LineStatusId) && o.ItemDefinitionId == newItemDefinition.Id).ToList();

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
                                        ItemDefinition def = _repoItemDefinition.FindBy(f => f.Id == pickView.ItemId).FirstOrDefault();
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

        public List<SkipView> GetSkippedOrders()
        {
            var skipViews = new List<SkipView>();
            var records = _repoOrderDetails.AllInclude(r => r.Order).Where(o => o.LineStatusId == 9).ToList();
            foreach (var rec in records)
            {
                var totalInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == rec.ItemDefinitionId)
                    .Sum(s => s.Quantity);


                var skip = new SkipView();
                skip.OrderDetail = rec;
                skip.StationNumber = rec.StationNumber;
                skip.Description = rec.PartDesc;
                skip.Id = rec.Id;
                skip.Ord1 = rec.Order.Ord1;
                skip.Ord2 = rec.Order.Ord2;
                skip.Item = rec.PartNum;
                skip.Priority = rec.Order.Priority.ToString();
                skip.OrderStatusName = $"{rec.LineStatus.Name}  {totalInventory}";
                skip.Picked = rec.PickedQuantity;
                skip.Quantity = rec.Quantity;
                skip.OrderId = rec.OrderId;
                skip.LoadDate = rec.Order.LoadDate;

                skipViews.Add(skip);
            }

            return skipViews;

            //// var statusToGet = new int[] { 1, 2, 3, 4 };
            //List<OrderView> recs = _repo.AllInclude(r => r.OrderDetails)
            //    //.Where(r => r.OrderDetails.                                           //statusToGet.Contains(r.OrderStatusId))
            //    .Select(s => new OrderView
            //    {
            //        Id = s.Id,
            //        Ord1 = s.Ord1,
            //        Ord2 = s.Ord2,
            //        OrderStatusName = s.OrderStatus.Name,
            //        ShipMethodName = s.ShipMethod.Name,
            //        Priority = s.Priority,
            //        Order = s,
            //        Station_1_HasPicks = CheckForPicks3(1, s.OrderDetails),
            //        Station_2_HasPicks = CheckForPicks3(2, s.OrderDetails),
            //        Station_3_HasPicks = CheckForPicks3(3, s.OrderDetails),
            //        Station_4_HasPicks = CheckForPicks3(4, s.OrderDetails),
            //        Station_5_HasPicks = CheckForPicks3(5, s.OrderDetails),
            //        Station_8_HasPicks = CheckForPicks3(8, s.OrderDetails),
            //        LoadDate = s.LoadDate,
            //        OrderStatusId = s.OrderStatusId,
            //        ShipMethodId = s.ShipMethodId
            //    })
            //    .OrderBy(o => o.Id).ToList();

            //return recs;

        }

        public IEnumerable<OrderView> GetRackOrders()
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderByDescending(o => o.Priority).ToList();

            return recs;
        }

        public IEnumerable<OrderView> GetRackOrders(string search)
        {
            IEnumerable<OrderView> recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new OrderView
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
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderBy(o => o.Ord1).ToList();
            IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }

        public IEnumerable<RackOrderView> GetRackOrdersView(string search)
        {
            IEnumerable<RackOrderView> recs;

            //using (var db = new NeutronDb())
            //{
            //  recs = db.Orders.Select(s => new RackOrderView()
            //    {
            //        Id = s.Id
            //        , Order = s
            //        , LoadDate = s.LoadDate
            //        , Ord1 = s.Ord1
            //        , Ord2 = s.Ord2
            //        , Priority = s.Priority
            //        , OrderDetails = s.OrderDetails.Where(o => o.LineStatusId != 6 && o.StationNumber == 8).ToList()
            //    }).Where(o => o.Order.OrderStatusId != 6 && o.OrderDetails.Count > 0)
            //        .OrderBy(o => o.Ord2).ToList();
            //}



            recs = _repo.AllInclude(r => r.OrderDetails).Select(s => new RackOrderView
            {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Priority = s.Priority,
                    Order = s,
                    LoadDate = s.LoadDate,
                    OrderDetails = s.OrderDetails.Where(o => o.LineStatusId != 6 && o.StationNumber == 8).ToList()
                }).Where(o => o.Order.OrderStatusId != 6)
                .OrderBy(o => o.Ord2).ToList();

            var result = recs.Where(s => s.SearchField.Contains(search) && s.OrderDetails.Count > 0);
            return result;
        }
    }
}
