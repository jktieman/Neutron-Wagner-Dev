using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.Interfaces;


namespace NeutronData.Repositories
{

    public class OrdersRepository : IOrdersRepository
    {
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly IStationRepository _stationRepository;
        private static int _totalOrderDetailCount;
        private static int _runningOrderDetailCount;
        private int[] _moveablePickStationIds;
        private Station _rackStation;
        private List<Station> _pickStations;
        private int[] _pickStationIds;

        public OrdersRepository(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
            Init();
        }

        private void Init()
        {
            _moveablePickStationIds = _stationRepository.GetMoveablePickStationIds();
            _rackStation = _stationRepository.GetRackStation();
            _pickStations = _stationRepository.GetPickStations();
            _pickStationIds = _stationRepository.GetPickStationIds();
            _totalOrderDetailCount = 0;
            _runningOrderDetailCount = 0;
        }

        public Order GetOrder(int id)
        {
            Order ord;
            using (var db = new NeutronDb())
            {
                ord = db.Orders.Include("OrderDetails").FirstOrDefault(r => r.Id == id);
            }
            return ord;
        }

        public IEnumerable<OrderView> GetOrderViewNotCompleted(string search = "")
        {
            IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
                .Where(r => r.OrderStatusId != (int)NeutronCore.Enums.OrderStatus.Complete)
                .Select(s => new OrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    Order = s,
                    Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
                    Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                    Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                    Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                    Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                    Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                .OrderByDescending(o => o.Priority).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search)) : recs;
        }

        //public IEnumerable<OrderView> GetOrderViewNotCompleted()
        //{
        //    IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
        //        .Where(r => r.OrderStatusId != 6)
        //        .Select(s => new OrderView
        //        {
        //            Id = s.Id,
        //            Ord1 = s.Ord1,
        //            Ord2 = s.Ord2,
        //            OrderStatusName = s.OrderStatus.Name,
        //            ShipMethodName = s.ShipMethod.Name,
        //            Priority = s.Priority,
        //            Order = s,
        //            Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
        //            Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
        //            Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
        //            Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
        //            Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
        //            Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
        //            LoadDate = s.LoadDate,
        //            OrderStatusId = s.OrderStatusId,
        //            ShipMethodId = s.ShipMethodId
        //        })
        //        .OrderBy(o => o.Ord1).ToList();
        //    IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
        //    return result;
        //}

        public IEnumerable<OrderView> GetOrderView()
        {
            // var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
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
                    Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
                    Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                    Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                    Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                    Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                    Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
            .OrderBy(o => o.Id).ToList();

            return recs;
        }

        //public IEnumerable<OrderView> GetOrderView(string search)
        //{
        //    IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails).Select(s => new OrderView
        //    {
        //        Id = s.Id,
        //        Ord1 = s.Ord1,
        //        Ord2 = s.Ord2,
        //        OrderStatusName = s.OrderStatus.Name,
        //        ShipMethodName = s.ShipMethod.Name,
        //        Priority = s.Priority,
        //        Order = s,
        //        Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
        //        Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
        //        Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
        //        Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
        //        Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
        //        Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
        //        LoadDate = s.LoadDate,
        //        OrderStatusId = s.OrderStatusId,
        //        ShipMethodId = s.ShipMethodId
        //    })
        //    .OrderBy(o => o.Ord1).ToList();
        //    IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
        //    return result;
        //}

        public List<AvailableOrdersView> GetAvailableOrders(StationView station)
        {
            var recs = new List<AvailableOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@STATIONID", value: station.StationId);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<AvailableOrdersView>("usp_GetAvailableOrders @STATIONID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get Available Orders Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }



        public List<AvailableOrdersView> GetAvailableOrders(StationView station, string search, bool serialPicking, bool showSkips = false)
        {
            var recs = new List<AvailableOrdersView>();
            var availableSkip = new[] { 1, 3, 9 };
            try
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    recs = _repoOrders.AllInclude(s => s.OrderDetails)
                        .Where(o => availableSkip.Contains(o.OrderStatusId) && (o.Ord1.ToLower().Contains(search) || o.Ord2.ToLower().Contains(search)))
                        .Where(s => s.OrderDetails.All(d => d.StationNumber == station.StationNumber))
                        .Select(r => new AvailableOrdersView
                        {
                            Id = r.Id
                            ,
                            Ord1 = r.Ord1
                            ,
                            Ord2 = r.Ord2
                            ,
                            Priority = r.Priority
                            //, Starter = r.OrderDetails.Min(o => o.StationNumber).ToString()
                            ,
                            Lines = r.OrderDetails.Count()
                            ,
                            Available = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Available)
                            ,
                            Picked = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Complete)
                            ,
                            Skipped = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Skipped)
                            ,
                            Pieces = r.OrderDetails.Sum(d => d.Quantity)
                            ,
                            LoadDate = r.LoadDate
                            ,
                            Order = r
                        }).ToList();
                }
                else
                {
                    recs = _repoOrders.AllInclude(s => s.OrderDetails)
                                      .Where(o => o.OrderStatusId == 1)
                                      .Where(s => s.OrderDetails.All(d => d.StationNumber == station.StationNumber))
                                      .Select(r => new AvailableOrdersView
                                      {
                                          Id = r.Id
                                          ,
                                          Ord1 = r.Ord1
                                          ,
                                          Ord2 = r.Ord2
                                          ,
                                          Priority = r.Priority
                                          //, Starter = r.OrderDetails.Min(o => o.StationNumber).ToString()
                                          ,
                                          Lines = r.OrderDetails.Count()
                                          ,
                                          Available = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Available)
                                          ,
                                          Picked = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Complete)
                                          ,
                                          Skipped = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Skipped)
                                          ,
                                          Pieces = r.OrderDetails.Where(c => availableSkip.Contains(c.LineStatusId)).Sum(d => d.Quantity)
                                          ,
                                          LoadDate = r.LoadDate
                                          ,
                                          Order = r

                                      }).ToList();
                }

                //int i = 0;
                //foreach (var rec in recs)
                //{
                //        i++;
                //        Console.WriteLine($"{i.ToString().PadLeft(3)}   Order: {rec.Ord1.PadLeft(20)}    Lines:{rec.Lines.ToString().PadLeft(3)}       Available:{rec.Available.ToString().PadLeft(3)}    Picked:{rec.Picked.ToString().PadLeft(3)}    Skipped:{rec.Skipped.ToString().PadLeft(3)}    Pieces: {rec.Pieces.ToString().PadLeft(7)}");
                //}
            }
            catch (Exception ex)
            {
                Logger.Log("Get Available Orders View Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }


        //public IEnumerable<OrderView> GetAvailableOrders__OLD
        //    (StationView station, string search, bool serialPicking)
        //{
        //    var station1Orders = new List<OrderView>();
        //    var station2Orders = new List<OrderView>();
        //    var station3Orders = new List<OrderView>();
        //    var station4Orders = new List<OrderView>();
        //    var station5Orders = new List<OrderView>();
        //    var station8Orders = new List<OrderView>();
        //    var result = new List<OrderView>();
        //    var availableRecs = new List<OrderView>();
        //    var recs = new List<OrderView>();  // null;
        //    try
        //    {
        //        recs = _repoOrders.AllInclude(r => r.OrderDetails).Select(s => new OrderView
        //        {
        //            Id = s.Id,
        //            Ord1 = s.Ord1,
        //            Ord2 = s.Ord2,
        //            Starter = "1", // StartOnThisStation(station.StationNumber, s.OrderDetails),
        //            OrderStatusName = s.OrderStatus.Name,
        //            ShipMethodName = s.ShipMethod.Name,
        //            Priority = s.Priority,
        //            Order = s,
        //            Station_1_HasPicks = CheckForPicks3(1, s.OrderDetails),
        //            Station_2_HasPicks = CheckForPicks3(2, s.OrderDetails),
        //            Station_3_HasPicks = CheckForPicks3(3, s.OrderDetails),
        //            Station_4_HasPicks = CheckForPicks3(4, s.OrderDetails),
        //            Station_5_HasPicks = CheckForPicks3(5, s.OrderDetails),
        //            Station_8_HasPicks = CheckForPicks3(8, s.OrderDetails),
        //            FirstPickStation = GetFirstPickStation(s.OrderDetails),
        //            LoadDate = s.LoadDate,
        //            OrderStatusId = s.OrderStatusId,
        //            ShipMethodId = s.ShipMethodId
        //        })
        //           .OrderByDescending(o => o.Priority).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"GetAvailableOrders Error Phase 1 Recs Count: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
        //    }

        //    try
        //    {
        //        foreach (var item in recs)
        //        {
        //            if (!string.IsNullOrEmpty(item.Station_1_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_1_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station1Orders.Add(item);
        //                    item.CurrentPickStation = 1;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station2Orders.Add(item);
        //                    item.CurrentPickStation = 2;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station3Orders.Add(item);
        //                    item.CurrentPickStation = 3;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station4Orders.Add(item);
        //                    item.CurrentPickStation = 4;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station5Orders.Add(item);
        //                    item.CurrentPickStation = 5;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station8Orders.Add(item);
        //                    item.CurrentPickStation = 8;
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Assign Next Station: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
        //    }

        //    // Serial Picking
        //    if (serialPicking)
        //    {
        //        try
        //        {
        //            foreach (var item in recs.Where(r => r.CurrentPickStation == station.StationNumber))
        //            {
        //                var details = item.Order.OrderDetails.Where(r => r.StationNumber == station.StationNumber).ToList();
        //                item.Order.OrderDetails = details;
        //                availableRecs.Add(item);
        //            }
        //            result = availableRecs.Where(s => s.SearchField.Contains(search)).ToList();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Assign Detail Lines this Station - {station.StationNumber}: Rec Count: {recs.Count()}\r\n  {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
        //        }
        //        //End Serial Picking
        //    }
        //    //Parallel Picking
        //    else
        //    {
        //        station1Orders.Clear();
        //        station2Orders.Clear();
        //        station3Orders.Clear();
        //        station4Orders.Clear();
        //        station5Orders.Clear();
        //        station8Orders.Clear();
        //        try
        //        {
        //            foreach (var item in recs)
        //            {
        //                if (!string.IsNullOrEmpty(item.Station_1_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_1_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station1Orders.Add(item);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station2Orders.Add(item);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station3Orders.Add(item);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station4Orders.Add(item);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station5Orders.Add(item);
        //                    }
        //                }

        //                if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
        //                {
        //                    if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                    {
        //                        station8Orders.Add(item);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Assign Next Station: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
        //        }

        //        switch (station.StationNumber)
        //        {
        //            case 1:
        //                {
        //                    result = station1Orders;
        //                    break;
        //                }
        //            case 2:
        //                {
        //                    result = station2Orders;
        //                    break;
        //                }
        //            case 3:
        //                {
        //                    result = station3Orders;
        //                    break;
        //                }
        //            case 4:
        //                {
        //                    result = station4Orders;
        //                    break;
        //                }
        //            case 5:
        //                {
        //                    result = station5Orders;
        //                    break;
        //                }
        //            case 8:
        //                {
        //                    result = station8Orders;
        //                    break;
        //                }
        //        }
        //        result = result.Where(s => s.SearchField.Contains(search)).ToList();
        //    }

        //    return result;
        //}

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

        //public IEnumerable<OrderView> GetCompletedOrders()
        //{
        //    IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails).Select(s => new OrderView
        //    {
        //        Id = s.Id,
        //        Ord1 = s.Ord1,
        //        Ord2 = s.Ord2,
        //        OrderStatusName = s.OrderStatus.Name,
        //        ShipMethodName = s.ShipMethod.Name,
        //        Priority = s.Priority,
        //        Order = s,
        //        Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
        //        Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
        //        Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
        //        Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
        //        Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
        //        Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
        //        LoadDate = s.LoadDate,
        //        OrderStatusId = s.OrderStatusId,
        //        ShipMethodId = s.ShipMethodId
        //    }).Where(r => r.OrderStatusId == 6)
        //    .OrderByDescending(o => o.Priority).ToList();

        //    return recs;
        //}

        public IEnumerable<OrderView> GetCompletedOrders(string find = "")
        {
            IEnumerable<OrderView> recs = _repoOrders.All().Select(s => new OrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    Order = s,
                    Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
                    Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                    Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                    Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                    Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                    Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).Where(r => r.OrderStatusId == (int)OrderStatus.Complete)
                .OrderByDescending(o => o.Priority).ToList();
            var result = recs.Where(s => s.SearchField.Contains(find));
            return result;
        }

        //public IEnumerable<OrderView> GetCompletedOrders(string search)
        //{
        //    IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
        //        .Where(r => r.OrderStatusId == (int)NeutronCore.Enums.OrderStatus.Complete).Select(s => new OrderView
        //        {
        //            Id = s.Id,
        //            Ord1 = s.Ord1,
        //            Ord2 = s.Ord2,
        //            OrderStatusName = s.OrderStatus.Name,
        //            ShipMethodName = s.ShipMethod.Name,
        //            Priority = s.Priority,
        //            Order = s,
        //            Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
        //            Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
        //            Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
        //            Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
        //            Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
        //            Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
        //            LoadDate = s.LoadDate,
        //            OrderStatusId = s.OrderStatusId,
        //            ShipMethodId = s.ShipMethodId
        //        }).OrderByDescending(o => o.Ord1).ToList();
        //    return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search)) : recs;
        //}

        public Order GetOrder()
        {
            var ord = _repoOrders.All().FirstOrDefault();

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
            List<Order> result = _repoOrders.AllInclude(n => n.OrderDetails).Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
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
                        PickedQty = 0,
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
            IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails).Select(s => new OrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                Order = s,
                Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
                Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
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
                            if (item.LineStatusId == (int)LineStatus.Skipped)
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
                List<Order> orders = _repoOrders.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
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
                    List<Order> orders = _repoOrders.AllInclude(n => n.OrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
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
            var records = _repoOrderDetails.AllInclude(r => r.Order)
                .Where(o => o.LineStatusId == (int)LineStatus.Skipped
                && o.Order.OrderStatusId != (int)NeutronCore.Enums.OrderStatus.Complete).ToList();
            foreach (var rec in records)
            {
                var totalInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == rec.ItemDefinitionId)
                    .Sum(s => s.Quantity);


                var skip = new SkipView();
                skip.OrderDetail = rec;
                skip.StationNumber = rec.StationNumber;
                skip.Description = rec.PartDesc;
                skip.Id = rec.Id;
                skip.OrderDetailId = rec.Id;
                skip.Ord1 = rec.Order.Ord1;
                skip.Ord2 = rec.Order.Ord2;
                skip.Item = rec.PartNum;
                skip.Priority = rec.Order.Priority.ToString();
                skip.OrderStatusName = $"{((LineStatus)rec.LineStatusId).GetEnumDescription()}  {totalInventory}";
                skip.Picked = rec.PickedQuantity;
                skip.Quantity = rec.Quantity;
                skip.OrderId = rec.OrderId;
                skip.LoadDate = rec.Order.LoadDate;

                skipViews.Add(skip);
            }

            return skipViews;
        }

        //public IEnumerable<OrderViewWithDetails> GetRackOrdersAll(int rackStationId)
        //{
        //    IEnumerable<OrderViewWithDetails> recs = _repoOrders.AllInclude(r => r.OrderDetails)
        //        .Select(s => new OrderViewWithDetails
        //        {
        //            Id = s.Id,
        //            Ord1 = s.Ord1,
        //            Ord2 = s.Ord2,
        //            OrderStatusName = s.OrderStatus.Name,
        //            ShipMethodName = s.ShipMethod.Name,
        //            Priority = s.Priority,
        //            Order = s,
        //            StationOrderDetails = GetStationOrderDetails(s.Id, s.OrderDetails),
        //            //OrderStatus1 = s.OrderDetails.FirstOrDefault() == null ? string.Empty : s.OrderDetails.FirstOrDefault()?.LineStatus.Name.Substring(0, 1),
        //            OrderStatus1 = CheckForPicks(1, s.OrderDetails),
        //            Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
        //            //Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
        //            //Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
        //            //Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
        //            //Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
        //            //Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
        //            LoadDate = s.LoadDate,
        //            OrderStatusId = s.OrderStatusId,
        //            ShipMethodId = s.ShipMethodId
        //        }).OrderByDescending(o => o.Priority).ToList();                                                 //.Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))


        //    return recs;
        //}

        private List<StationOrderDetail> GetStationOrderDetails(int orderId, ICollection<OrderDetail> orderDetails)
        {
            var stationOrderDetails = new List<StationOrderDetail>();

            foreach (var pickStation in _pickStations)
            {
                var details = orderDetails.Where(r => r.OrderId == orderId && r.StationNumber == pickStation.StationNumber).ToList();
                var stationOrderDetail = new StationOrderDetail { PickStation = pickStation, OrderId = orderId };
                if (details.Any())
                {
                    stationOrderDetail.OrderDetails = details;
                    stationOrderDetail.Lines = details.Count;
                    stationOrderDetail.Pieces = details.Sum(r => r.Quantity);
                    stationOrderDetail.OrderStatus = ((LineStatus)details.First().LineStatusId).GetEnumDescription();
                }
                stationOrderDetails.Add(stationOrderDetail);
            }

            return stationOrderDetails;
        }

        //Tested
        public IEnumerable<OrderView> GetRackOrders(string search = "")
        {
            IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
                .Where(r => r.OrderStatusId != (int)NeutronCore.Enums.OrderStatus.Complete)
                .Select(s => new OrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                Order = s,
                Station_1_HasPicks = HasPicks(_pickStationIds, 1, s.OrderDetails),
                Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.OrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderByDescending(o => o.Priority).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search)) : recs;
        }

        private string HasPicks(int[] moveableStationIds, int hasPickNum, ICollection<OrderDetail> orderDetails)
        {
            var result = string.Empty;
            if (hasPickNum == 1)
            {
                _totalOrderDetailCount = orderDetails.Count;
                _runningOrderDetailCount = 0;
            }
            else if (_runningOrderDetailCount == _totalOrderDetailCount)
            {
                return result;
            }

            if (moveableStationIds.Length > 0 && hasPickNum <= moveableStationIds.Length && orderDetails.Any())
            {
                result = CheckForPicks4(moveableStationIds[hasPickNum - 1], orderDetails);
            }
            return result;
        }

        private string HasRackPicks(int rackStationId, ICollection<OrderDetail> orderDetails)
        {
            var result = string.Empty;
            if (_runningOrderDetailCount == _totalOrderDetailCount)
            {
                return result;
            }
            result = CheckForPicks4(rackStationId, orderDetails);
            return result;
        }

        // This will not get called unless there is a Station NUmber and OrderDetails
        private string CheckForPicks4(int stationNumber, ICollection<OrderDetail> orderDetails)
        {
            string result = string.Empty;
            var sb = new StringBuilder();
            if (_runningOrderDetailCount == _totalOrderDetailCount)
            {
                return result;
            }

            int key;
            var groups = orderDetails.GroupBy(g => new { g.LineStatusId, g.StationNumber })
                .Select(r => new { Key = r.Key }).Where(s => s.Key.StationNumber == stationNumber)
                .ToList();
            switch (groups.Count == 1 ? "One" : groups.Count > 1 ? "Multiple" : "None")
            {
                case "One":
                    {
                        key = groups.First().Key.LineStatusId;
                        _runningOrderDetailCount += orderDetails.Count(r => r.LineStatusId == key && r.StationNumber == stationNumber);
                        var s = (LineStatus)key;
                        result = s.GetEnumDescription().Substring(0, 1);
                        break;
                    }
                case "Multiple":
                    {
                        var codes = new List<string>();
                        foreach (var group in groups)
                        {
                            key = group.Key.LineStatusId;
                            _runningOrderDetailCount += orderDetails.Count(r => r.LineStatusId == key && r.StationNumber == stationNumber);
                            var code = ((LineStatus)key).GetEnumDescription().Substring(0, 1);
                            if (!codes.Contains(code)) { codes.Add(code); }
                        }
                        codes.ForEach(c => sb.Append(c));
                        result = sb.ToString();
                        break;
                    }
            }

            return result;
        }

        //public IEnumerable<OrderView> GetRackOrders()
        //{
        //    IEnumerable<OrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails).Select(s => new OrderView
        //    {
        //        Id = s.Id,
        //        Ord1 = s.Ord1,
        //        Ord2 = s.Ord2,
        //        OrderStatusName = s.OrderStatus.Name,
        //        ShipMethodName = s.ShipMethod.Name,
        //        Priority = s.Priority,
        //        Order = s,
        //        Station_1_HasPicks = CheckForPicks(1, s.OrderDetails),
        //        Station_2_HasPicks = CheckForPicks(2, s.OrderDetails),
        //        Station_3_HasPicks = CheckForPicks(3, s.OrderDetails),
        //        Station_4_HasPicks = CheckForPicks(4, s.OrderDetails),
        //        Station_5_HasPicks = CheckForPicks(5, s.OrderDetails),
        //        Station_8_HasPicks = CheckForPicks(8, s.OrderDetails),
        //        LoadDate = s.LoadDate,
        //        OrderStatusId = s.OrderStatusId,
        //        ShipMethodId = s.ShipMethodId
        //    }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
        //        .OrderBy(o => o.Ord1).ToList();
        //    IEnumerable<OrderView> result = recs.Where(s => s.SearchField.Contains(search));
        //    return result;
        //}

        public IEnumerable<RackOrderView> GetRackOrdersView(int rackStationNumber, string search = @"")
        {
            IEnumerable<RackOrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
                .Where(o => o.OrderStatusId != (int)NeutronCore.Enums.OrderStatus.Complete && o.OrderDetails.Count > 0)
                .Select(s => new RackOrderView
                {
                    StationNumber = rackStationNumber,
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Priority = s.Priority,
                    Order = s,
                    LoadDate = s.LoadDate,
                    OrderDetails = s.OrderDetails.Where(o => o.LineStatusId != (int)LineStatus.Complete && o.StationNumber == rackStationNumber).ToList()
                }).OrderBy(o => o.Ord2).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search) && s.OrderDetails.Count > 0) : recs.Where(r => r.OrderDetails.Count > 0);
        }

        public Order GetOrderAndOrderDetails(int? orderId, int stationNumber)
        {
            var ord = new Order();
            var availableSkip = new int[] { 1, 9 };
            // Order ord;
            if (orderId != null)
            {
                ord = _repoOrders.FindByKey(orderId);
                //using (var db = new NeutronDb())
                //{
                //     ord = db.Orders
                //        //.Include(s => s.Shipper)
                //        //.Include(s => s.ShipMethod)
                //        //.Include(s => s.OrderStatus)
                //        //.Include(s => s.OrderDetails.Select(x => x.ItemDefinition))
                //        //.Include(s => s.OrderDetails.Select(x => x.LineStatus))
                //        .FirstOrDefault(s => s.Id == orderId);

                if (ord != null)
                {
                    //ord.OrderDetails = null;

                    // var details = _repoOrderDetails.All()
                    ord.OrderDetails = ord.OrderDetails.Where(x => x.OrderId == orderId && x.StationNumber == stationNumber && availableSkip.Contains(x.LineStatusId)).ToList();
                    //ord.OrderDetails = details;
                }

                //}
            }
            return ord;
        }
    }
}


//.Include(s => s.OrderDetails.Select(x => x.StationNumber == stationNumber).ToList())
// 
//foreach (var ord in ords)
////{
////    foreach (var detail in ord.Order.OrderDetails)
////    {
////        detail.ItemDefinition = context.ItemDefinitions
////            .Include("UnitOfIssue")
////            .Include("SizeCode")
////            .Include("HeightCode")
////            .Include("VelocityCode")
////            .Include("Station")
////            .FirstOrDefault(d => d.Id == detail.ItemDefinitionId);
////    }
////}