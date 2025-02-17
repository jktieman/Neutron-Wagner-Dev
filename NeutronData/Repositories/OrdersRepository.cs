using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronData.Interfaces;
using Logger = NeutronCore.Global.Logger;
using NeutronData.Models.Lookups;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using System.Web.Routing;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NeutronEvents;

namespace NeutronData.Repositories
{

    public class OrdersRepository : IOrdersRepository
    {
        private readonly GenericRepository<Order> _repoOrders;
        private readonly GenericRepository<OrderDetail> _repoOrderDetails;
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;

        private readonly int _areaId;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<MemoryCacheOptions> _cacheOptions;
        private static int _totalOrderDetailCount;
        private static int _runningOrderDetailCount;
        private IDynamicLogger _logger;
        private readonly Func<NeutronDb> _contextFactory;
        private readonly IWorkstationRepository _workstationRepository;
        private int[] _pickStationIds;

        public OrdersRepository(Func<NeutronDb> contextFactory, Workstation workstation, IWorkstationRepository workstationRepository
            , IMemoryCache memoryCache, IOptions<MemoryCacheOptions> cacheOptions)
        {
            _contextFactory = contextFactory;
            _workstationRepository = workstationRepository;
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions;
            _areaId = workstation.AreaId;
            _repoOrders = new GenericRepository<Order>(contextFactory);
            _repoOrderDetails = new GenericRepository<OrderDetail>(contextFactory);
            _repoInventory = new GenericRepository<Inventory>(contextFactory);
            _repoItemDefinition = new GenericRepository<ItemDefinition>(contextFactory);
            Init();
        }

        private void Init()
        {
            _logger = Logger.SetupLogger("OrdersRepository");
            _pickStationIds = _workstationRepository.GetAllPickStationIds();
            _totalOrderDetailCount = 0;

            _runningOrderDetailCount = 0;
           // PreLoadCache();
        }

        public void PreLoadCache()
        {

            using (var context = _contextFactory())
            {
                var orders = context.Orders.Include("OrderDetails").Where(r => r.OrderStatusId != (int)OrderStatus.Complete).ToList();

                // Cache entry options
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };
                foreach (var order in orders)
                {
                    _memoryCache.Set(order.Id, order, cacheEntryOptions);
                }

            }
        }

        public Order GetOrderWithLineStatusAvailable(int orderId, int areaId)
        {
            using (var context = _contextFactory())
            {
                // Fetch the order with its details where the line status is "Available"
                var order = context.Orders
                    .Include("OrderDetails")
                    .FirstOrDefault(r => r.Id == orderId &&
                                         r.OrderDetails.Any(o => o.LineStatusId == (int)LineStatus.Available
                                         && o.AreaId == areaId));
                return order;
            }
        }

        public Order GetOrder(int id)
        {
            using (var context = _contextFactory())
            {
                //if (!_memoryCache.TryGetValue(id, out Order cacheData))
                //{
                    // use the normal way to get a record
                    // Perform database operations
                    var order = context.Orders.Include("OrderDetails").FirstOrDefault(r => r.Id == id);

                    //var cacheEntryOptions = new MemoryCacheEntryOptions
                    //{
                    //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    //    SlidingExpiration = TimeSpan.FromMinutes(20),
                    //    Size = 1
                    //};
                    return order;
                //}
                //return cacheData;
            }
        }

        public async Task<Order> GetOrderAsync(int id)
        {
            using (var context = _contextFactory())
            {
                //if (!_memoryCache.TryGetValue(id, out Order cacheData))
                //{
                    // use the normal way to get a record
                    // Perform database operations
                    var order = await context.Orders.Include("OrderDetails").FirstOrDefaultAsync(r => r.Id == id);

                    //var cacheEntryOptions = new MemoryCacheEntryOptions
                    //{
                    //    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    //    SlidingExpiration = TimeSpan.FromMinutes(20),
                    //    Size = 1
                    //};
                    return order;
                //}
                //return cacheData;
            }
        }

        public void SetOrderStatusToAvailableIfNotComplete(int[] orderIds)
        {
            foreach (var orderId in orderIds)
            {
                var incompleteOrderDetails = _repoOrderDetails
                    .FindBy(r => r.OrderId == orderId && r.LineStatusId != (int)LineStatus.Complete)
                    .ToList();
                var order = _repoOrders.FindByKey(orderId);
                if (!incompleteOrderDetails.Any())
                {
                    // Set order to Complete
                    UpdateOrderStatus(order, OrderStatus.Complete);
                }
                else
                {
                    // Set order to Available
                    UpdateOrderStatus(order, OrderStatus.Available);
                    foreach (var detail in incompleteOrderDetails.Where(d => d.LineStatusId == (int)LineStatus.Picking))
                    {
                        UpdateLineStatus(detail, LineStatus.Available);
                    }
                }
            }
        }
        private void UpdateOrderStatus(Order order, OrderStatus status)
        {
            order.OrderStatusId = (int)status;
            _repoOrders.Update(order);
        }
        private void UpdateLineStatus(OrderDetail detail, LineStatus status)
        {
            detail.LineStatusId = (int)status;
            _repoOrderDetails.Update(detail);
        }


        //public void SetOrderStatusToAvailableIfNotComplete(int[] orderIds)
        //{
        //    foreach (var orderId in orderIds)
        //    {
        //        var inCompleteOrderDetails = _repoOrderDetails.FindBy(r => r.OrderId == orderId).Where(r => r.LineStatusId != (int)LineStatus.Complete)
        //            .ToList();
        //        if (!inCompleteOrderDetails.Any())
        //        {
        //            // set order Complete
        //            var order = _repoOrders.FindByKey(orderId);
        //            order.OrderStatusId = (int)OrderStatus.Complete;
        //            _repoOrders.Update(order);
        //        }
        //        else
        //        {
        //            // set order available
        //            var order = _repoOrders.FindByKey(orderId);
        //            order.OrderStatusId = (int)OrderStatus.Available;
        //            _repoOrders.Update(order);

        //            foreach (var inCompleteOrderDetail in inCompleteOrderDetails)
        //            {
        //                if (inCompleteOrderDetail.LineStatusId == (int)LineStatus.Picking)
        //                {
        //                    inCompleteOrderDetail.LineStatusId = (int)LineStatus.Available;
        //                    _repoOrderDetails.Update(inCompleteOrderDetail);
        //                }
        //            }
        //        }
        //    }
        //}

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

        public IEnumerable<OrderView> GetOrderViews(string orderStatus = "1,2,3,4,5,7,8", string searchField = "")
        {
            var recs = new List<OrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderView>("usp_GetOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public IEnumerable<OrderView> GetAvailableOrderViews(string orderStatus = "1,2,3,4,5,6,7,8,9", string searchField = "")
        {
            var recs = new List<OrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderView>("usp_GetAvailableOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

        public List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstationView)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OrderView> GetCompletedOrderViews(string orderStatus = "6", string searchField = "")
        {
            var recs = new List<OrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderView>("usp_GetCompletedOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public IEnumerable<OrderView> GetReplenOrderViews(string orderStatus = "1,2,3,4,5,7,8", string searchField = "")
        {
            var recs = new List<OrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderView>("usp_GetReplenOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public IEnumerable<OrderView> GetReplenPickOrderViews(string orderStatus = "1,2,3,4,5,6,7,8,9", string searchField = "")
        {
            var recs = new List<OrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderView>("usp_GetReplenPickOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public List<AvailableOrdersView> GetAvailableOrders(int areaId)
        {
            var recs = new List<AvailableOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<AvailableOrdersView>("usp_GetAvailableOrders @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Available Orders Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        //----------------
        public List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstation, string search
            , bool serialPicking, bool showSkips = false)
        {
            //var areaIds = workstation.Areas.Select(r => r.Id);
            var recs = new List<AvailableOrdersView>();
            //try
            //{
            //    using (var context = _contextFactory())
            //    {
            //        List<OrderDetail> records;
            //        if (showSkips)
            //        {
            //            records = context.OrderDetails.Include("Order").Where(o => areaIds.Contains(o.AreaId) && (o.Order.OrderStatusId == 1))
            //                .Where(p => p.LineStatusId == 1 || p.LineStatusId == 9).ToList();
            //        }
            //        else
            //        {
            //            records = context.OrderDetails.Include("Order").Where(o => areaIds.Contains(o.AreaId) && (o.Order.OrderStatusId == 1))
            //                .Where(p => p.LineStatusId == 1).ToList();
            //        }
            //        var ords = records.GroupBy(r => new { r.OrderId, r.Order.Ord1, r.Order.Ord2, r.Order.Priority, r.Order.LoadDate })
            //             .Select(r => new AvailableOrdersView
            //             {
            //                 Id = r.Key.OrderId
            //                 , Ord1 = r.Key.Ord1
            //                 , Ord2 = r.Key.Ord2
            //                 , Priority = r.Key.Priority
            //                 , Lines = r.Count()
            //                 , Pieces = r.Sum(s => s.Quantity)
            //                 , LoadDate = r.Key.LoadDate
            //             }).ToList();

            //        //var ords = records
            //        //    .Select(r => new AvailableOrdersView
            //        //    {
            //        //        Id = r.Id
            //        //        ,
            //        //        Ord1 = r.Order.Ord1
            //        //        ,
            //        //        Ord2 = r.Order.Ord2
            //        //        ,
            //        //        Priority = r.Order.Priority
            //        //        ,
            //        //        Lines = r.Order.OrderDetails.Count()
            //        //        ,
            //        //        Pieces = r.Order.OrderDetails.Sum(s => s.Quantity)
            //        //        ,
            //        //        LoadDate = r.Order.LoadDate)
            //        //        , Order = r.Order

            //        //    }).Where(s => s.Ord1.Contains(search) || s.Ord2.Contains(search)) .ToList();

            //        foreach (var ord in ords)
            //        {
            //            ord.Order = context.Orders.Find(ord.Id);
            //        }

            //        foreach (var ord in ords)
            //        {
            //            foreach (var detail in ord.Order.OrderDetails)
            //            {
            //                detail.ItemDefinition = context.ItemDefinitions
            //                    .Include("UnitOfIssue")
            //                    .Include("SizeCode")
            //                    .Include("HeightCode")
            //                    .Include("VelocityCode")
            //                    .Include("Area")
            //                    .FirstOrDefault(d => d.Id == detail.ItemDefinitionId);
            //            }
            //        }

            //        recs = !string.IsNullOrEmpty(search) ? ords.Where(o => o.Ord1.ToLower().Contains(search) || o.Ord2.ToLower().Contains(search)).ToList() : ords;
            //    }
            //}
            //catch (Exception ex)
            //{
            // _ = _logger.LogDetailAsync("Get AvailableOrders View Error. " + ex.Message + " " + ex.InnerException);
            //}

            return recs;
        }



        //------------------------

        //public List<AvailableOrdersView> GetAvailableOrders(WorkstationView station, string search, bool serialPicking, bool showSkips = false)
        //{
        //    var temprecs = new List<Order>();
        //    var recs = new List<AvailableOrdersView>();
        //    var availableSkip = new[] { 1, 3, 9 };
        //    try
        //    {
        //        if (!string.IsNullOrWhiteSpace(search))
        //        {
        //            search = search.ToLower();
        //            recs = _repoOrders.AllInclude(s => s.OrderDetails)
        //                .Where(o => availableSkip.Contains(o.OrderStatusId) && (o.Ord1.ToLower().Contains(search) || o.Ord2.ToLower().Contains(search)))
        //                .Where(s => s.OrderDetails.All(d => d.StationNumber == station.StationNumber))
        //                .Select(r => new AvailableOrdersView
        //                {
        //                    Id = r.Id
        //                    ,
        //                    Ord1 = r.Ord1
        //                    ,
        //                    Ord2 = r.Ord2
        //                    ,
        //                    Priority = r.Priority
        //                    //, Starter = r.OrderDetails.Min(o => o.StationNumber).ToString()
        //                    ,
        //                    Lines = r.OrderDetails.Count()
        //                    ,
        //                    Available = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Available)
        //                    ,
        //                    Picked = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Complete)
        //                    ,
        //                    Skipped = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Skipped)
        //                    ,
        //                    Pieces = r.OrderDetails.Sum(d => d.Quantity)
        //                    ,
        //                    LoadDate = r.LoadDate
        //                    ,
        //                    Order = r
        //                }).ToList();
        //        }
        //        else
        //        {
        //            temprecs = _repoOrders.AllInclude(s => s.OrderDetails).Where(r => r.Ord2 == "2408782")
        //                .Where(o => o.OrderStatusId == 1)
        //                .Where(s => s.OrderDetails.All(d => d.StationNumber == station.StationNumber)).ToList();
        //            //.Select(r => new AvailableOrdersView
        //            //{
        //            //    Id = r.Id
        //            //    ,
        //            //    Ord1 = r.Ord1
        //            //    ,
        //            //    Ord2 = r.Ord2
        //            //    ,
        //            //    Priority = r.Priority
        //            //    ,
        //            //    Starter = 0  //r.OrderDetails.Min(o => o.StationNumber).ToString()
        //            //    ,
        //            //    Lines = r.OrderDetails.Count()
        //            //    ,
        //            //    Available = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Available)
        //            //    ,
        //            //    Picked = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Complete)
        //            //    ,
        //            //    Skipped = r.OrderDetails.Count(c => c.LineStatusId == (int)LineStatus.Skipped)
        //            //    ,
        //            //    Pieces = r.OrderDetails.Where(c => availableSkip.Contains(c.LineStatusId)).Sum(d => d.Quantity)
        //            //    ,
        //            //    LoadDate = r.LoadDate
        //            //    ,
        //            //    Order = r

        //            //}).ToList();
        //        }


        //        int i = 0;
        //        foreach (var rec in temprecs)
        //        {
        //            i++;
        //            if (rec.Ord2 == "2408782")
        //            {
        //                Console.WriteLine(
        //                    $"{i.ToString().PadLeft(3)}   Order: {rec.Ord1.PadLeft(20)}  RES: {rec.Ord2.PadLeft(20)} ");
        //            }
        //            //Console.WriteLine($"{i.ToString().PadLeft(3)}   Order: {rec.Ord1.PadLeft(20)}  RES: {rec.Ord2.PadLeft(20)}    Lines:{rec.Lines.ToString().PadLeft(3)}       Available:{rec.Available.ToString().PadLeft(3)}    Picked:{rec.Picked.ToString().PadLeft(3)}    Skipped:{rec.Skipped.ToString().PadLeft(3)}    Pieces: {rec.Pieces.ToString().PadLeft(7)}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //     _ = _logger.LogDetailAsync("Get Available Orders View Error. " + ex.Message + " " + ex.InnerException);
        //    }
        //    var re = recs.Where(r => r.Ord2.Trim() == "2408782").FirstOrDefault();
        //    return recs;
        //}


        //public IEnumerable<OrderView> GetAvailableOrders__OLD
        //    (WorkstationView station, string search, bool serialPicking)
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
        //            FirstPickArea = GetFirstPickStation(s.OrderDetails),
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
        //                    item.CurrentPickArea = 1;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station2Orders.Add(item);
        //                    item.CurrentPickArea = 2;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station3Orders.Add(item);
        //                    item.CurrentPickArea = 3;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station4Orders.Add(item);
        //                    item.CurrentPickArea = 4;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station5Orders.Add(item);
        //                    item.CurrentPickArea = 5;
        //                    continue;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
        //            {
        //                if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
        //                {
        //                    station8Orders.Add(item);
        //                    item.CurrentPickArea = 8;
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
        //            foreach (var item in recs.Where(r => r.CurrentPickArea == station.StationNumber))
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
                    int firstPickStation = orderDetails.Min(o => o.AreaId);
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
            result = orderDetails.Min(o => o.AreaId);
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

        public IEnumerable<Order> GetCompletedOrders()
        {
            var recs = new List<Order>();

            try
            {
                //var parameters = new List<object>();
                using var context = _contextFactory();
                //var param = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                //parameters.Add(param);
                //param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                //parameters.Add(param);

                recs = context.Database.SqlQuery<Order>("usp_GetCompletedOrders").ToList(); // SQL Tested
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Orders Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

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
                        AreaId = detail.AreaId
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
                Station_6_HasPicks = HasPicks(_pickStationIds, 6, s.OrderDetails),
                Station_7_HasPicks = HasPicks(_pickStationIds, 7, s.OrderDetails),
                Station_8_HasPicks = HasPicks(_pickStationIds, 8, s.OrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Ord1).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        private string CheckForPicks(int areaId, ICollection<OrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 3, 6, 9 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.AreaId == areaId)
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

        private string CheckForPicks2(int areaId, ICollection<OrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 6 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.AreaId == areaId)
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

        private string CheckForPicks3(int areaId, ICollection<OrderDetail> orderDetails)
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
                        if (item.AreaId == areaId)
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
            var orderIds = GetOrderIdArray(ordersToPick);

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
                                    AreaId = detail.AreaId
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
                                            AreaId = detail.AreaId
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
            foreach (var bp in ordersToPick)
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
                if (bp.OrderId != 0)
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
                && o.Order.OrderStatusId != (int)OrderStatus.Complete).ToList();
            foreach (var rec in records)
            {
                var totalInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == rec.ItemDefinitionId)
                    .Sum(s => s.Quantity);

                var skip = new SkipView();
                skip.OrderDetail = rec;
                skip.AreaId = rec.AreaId;
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

        //private List<StationOrderDetail> GetStationOrderDetails(int orderId, ICollection<OrderDetail> orderDetails)
        //{
        //    var stationOrderDetails = new List<StationOrderDetail>();

        //    foreach (var pickStation in _pickStations)
        //    {
        //        var details = orderDetails.Where(r => r.OrderId == orderId && r.StationNumber == pickStation.StationNumber).ToList();
        //        var stationOrderDetail = new StationOrderDetail { PickStation = pickStation, OrderId = orderId };
        //        if (details.Any())
        //        {
        //            stationOrderDetail.OrderDetails = details;
        //            stationOrderDetail.Lines = details.Count;
        //            stationOrderDetail.Pieces = details.Sum(r => r.Quantity);
        //            stationOrderDetail.OrderStatus = ((LineStatus)details.First().LineStatusId).GetEnumDescription();
        //        }
        //        stationOrderDetails.Add(stationOrderDetail);
        //    }

        //    return stationOrderDetails;
        //}

        public IEnumerable<OrderView> GetReplenishmentOrders(string search = "")
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
                    Station_6_HasPicks = HasPicks(_pickStationIds, 6, s.OrderDetails),
                    Station_7_HasPicks = HasPicks(_pickStationIds, 7, s.OrderDetails),
                    Station_8_HasPicks = HasPicks(_pickStationIds, 8, s.OrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).OrderByDescending(o => o.Priority).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search)) : recs;
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
                    Station_1_HasPicks = HasPicks(
                    _pickStationIds, 1, s.OrderDetails),
                    Station_2_HasPicks = HasPicks(_pickStationIds, 2, s.OrderDetails),
                    Station_3_HasPicks = HasPicks(_pickStationIds, 3, s.OrderDetails),
                    Station_4_HasPicks = HasPicks(_pickStationIds, 4, s.OrderDetails),
                    Station_5_HasPicks = HasPicks(_pickStationIds, 5, s.OrderDetails),
                    Station_6_HasPicks = HasPicks(_pickStationIds, 6, s.OrderDetails),
                    Station_7_HasPicks = HasPicks(_pickStationIds, 7, s.OrderDetails),
                    Station_8_HasPicks = HasPicks(_pickStationIds, 8, s.OrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderByDescending(o => o.Priority).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search)) : recs;
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
        private string CheckForPicks4(int areaId, ICollection<OrderDetail> orderDetails)
        {
            string result = string.Empty;
            var sb = new StringBuilder();
            if (_runningOrderDetailCount == _totalOrderDetailCount)
            {
                return result;
            }

            int key;
            var groups = orderDetails.GroupBy(g => new { g.LineStatusId, g.AreaId })
                .Select(r => new { Key = r.Key }).Where(s => s.Key.AreaId == areaId)
                .ToList();
            switch (groups.Count == 1 ? "One" : groups.Count > 1 ? "Multiple" : "None")
            {
                case "One":
                    {
                        key = groups.First().Key.LineStatusId;
                        _runningOrderDetailCount += orderDetails.Count(r => r.LineStatusId == key && r.AreaId == areaId);
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
                            _runningOrderDetailCount += orderDetails.Count(r => r.LineStatusId == key && r.AreaId == areaId);
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

        public IEnumerable<RackOrderView> GetRackOrdersView(int areaId, string search = @"")
        {
            IEnumerable<RackOrderView> recs = _repoOrders.AllInclude(r => r.OrderDetails)
                .Where(o => o.OrderStatusId != (int)OrderStatus.Complete && o.OrderDetails.Count > 0)
                .Select(s => new RackOrderView
                {
                    AreaId = areaId,
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Priority = s.Priority,
                    Order = s,
                    LoadDate = s.LoadDate,
                    OrderDetails = s.OrderDetails.Where(o => o.LineStatusId != (int)LineStatus.Complete && o.AreaId == areaId).ToList()
                }).OrderBy(o => o.Ord2).ToList();
            return !string.IsNullOrEmpty(search) ? recs.Where(s => s.SearchField.Contains(search) && s.OrderDetails.Count > 0) : recs.Where(r => r.OrderDetails.Count > 0);
        }

        /// <summary>
        /// Retrieves an order along with its order details based on the provided order ID and area ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to be retrieved.</param>
        /// <param name="areaId">The ID of the area associated with the order.</param>
        /// <returns>
        /// The order with its details if it exists, or null if it doesn't. 
        /// Only order details with a line status that can be picked (available or skipped) are included.
        /// </returns>
        public Order GetOrderWithOrderDetailsInThisArea(int orderId, int areaId)
        {

            // Retrieve the order with the given order ID
            var order = GetOrderWithLineStatusAvailable(orderId, areaId);
            if (order == null) return null;

            // remove the OrderDetail records where AreaId does not equal areaId
            order.OrderDetails = order.OrderDetails.Where(od => od.AreaId == areaId 
                                                                && od.LineStatusId == (int)LineStatus.Available).ToList();



            // We only want to retrieve the order details
            // with a line status that can be picked in this area
            // so we need to set the OrderDetails property to an empty list
            //order.OrderDetails.Clear();
            // OR we can set the OrderDetails property to a list with a capacity of 0
            // which is more efficient than clearing the list
            //order.OrderDetails = new List<OrderDetail>
            //{
            //    Capacity = 0
            //};

            //// Retrieve the order details for the given order ID and area ID
            //// The Stored Procedure makes sure the LineStatus is either 1 or 9 and orders by PartNum
            //var orderDetails = GetOrderDetailsByOrderAndArea(orderId, areaId).ToList();

            //order.OrderDetails = orderDetails;

            return order;
        }

        public List<OrderDetail> GetOrderDetailsByOrderAndArea(int orderId, int areaId)
        {
            var recs = new List<OrderDetail>();

            try
            {
                using (var context = _contextFactory())
                {
                    var parameters = new List<object>();
                    var param = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<OrderDetail>("usp_GetOrderDetailsByOrderAndArea @ORDERID, @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get OrderDetails Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

        public List<AvailableOrdersView> GetAvailableOrdersForInductionScreen(int areaId, string searchField, bool serialPicking)
        {
            var recs = new List<AvailableOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SEARCHFIELD", value: searchField);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SERIALPICKING", value: serialPicking);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get Available Orders Views usp_GetAvailableOrdersForInductionScreenWithRoute. AreaId: {areaId} SearchField: {searchField} SerialPicking: {serialPicking} ");

                    recs = context.Database.SqlQuery<AvailableOrdersView>("usp_GetAvailableOrdersForInductionScreenWithRoute @AREAID, @SEARCHFIELD, @SERIALPICKING", parameters.ToArray()).ToList();


                    //var orderIds = GetInvalidOrderDetails(areaId, recs);
                    //if (orderIds.Any())
                    //{
                    //    foreach (var orderId in orderIds)
                    //    {
                    //        var availableOrdersView = recs.Find(r => r.Id == orderId);
                    //        recs.Remove(availableOrdersView);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Available Orders Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        /// <summary>
        /// Logs the details of invalid orders.
        /// </summary>
        /// <param name="areaId">The identifier of the area where the orders are being processed.</param>
        /// <param name="availableOrders">A list of available orders views.</param>
        /// <remarks>
        /// This method checks the list of available orders views for any orders that should not be appearing in the Induction Screen.
        /// If such orders are found, it logs the details of these orders and sends a message to the supervisor.
        /// </remarks>
        private List<int> GetInvalidOrderDetails(int areaId, List<AvailableOrdersView> availableOrders)
        {
            var orderIds = new List<int>();
            try
            {
                using (var context = _contextFactory())
                {
                    // Perform database operations
                    var availableOrderIds = availableOrders.Select(order => order.Id).ToList();
                    var invalidOrderDetails = context.OrderDetails
                        .Where(r => availableOrderIds.Contains(r.OrderId) && r.AreaId < areaId && r.LineStatusId != (int)LineStatus.Complete)
                        .ToList();
                    if (invalidOrderDetails.Any())
                    {
                        orderIds = invalidOrderDetails.Select(r => r.OrderId).Distinct().ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}").SafeFireAndForget();
            }

            return orderIds;


            //if (!invalidOrderDetails.Any()) return;
            //var errorMessage = CreateErrorMessage(invalidOrderDetails);
            //_logger.LogDetailAsync(errorMessage);
            //Mediator.GetInstance().OnDisplayMessage(this, errorMessage);
        }
        private string CreateErrorMessage(List<OrderDetail> invalidOrderDetails)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Contact Supervisor Immediately!{Environment.NewLine}{Environment.NewLine}");
            foreach (var detail in invalidOrderDetails)
            {
                sb.AppendLine($"This item should NOT be showing up in the Induction Screen. OrderDetail Id: {detail.Id} Item: {detail.PartNum} AreaId: {detail.AreaId} LineStatus: {detail.LineStatusId} ");
            }
            return sb.ToString();
        }

        public string GetRoute(int orderId)
        {
            string route = string.Empty;
            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get Route  OrderId: {orderId} ");
                    route = context.Database.SqlQuery<string>("usp_GetRoute @ORDERID", parameters.ToArray()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Route Error. " + ex.Message + " " + ex.InnerException);
            }
            return route;
        }

        public IEnumerable<int> GetSizeCodesByArea(int areaId)
        {
            IEnumerable<int> sizeCodes = new List<int>();
            try
            {
                var parameters = new List<object>();
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get SizeCodeIds: {areaId} ");
                    sizeCodes = context.Database.SqlQuery<int>("usp_GetSizeCodesByArea @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Route Error. " + ex.Message + " " + ex.InnerException);
            }
            return sizeCodes;
        }

        public int[] GetOrderDetailIds(int[] currentOrderIds, int areaId)
        {
            if (currentOrderIds == null || currentOrderIds.Length == 0) return Array.Empty<int>();

            return _repoOrderDetails.FindBy(od => currentOrderIds.Contains(od.OrderId) 
                                                  && od.AreaId == areaId
                                                  && od.LineStatusId == (int)LineStatus.Available)
                .Select(od => od.Id).ToArray();
        }
    }
}

