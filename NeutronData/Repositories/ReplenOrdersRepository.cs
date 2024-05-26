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
using System.Windows.Forms;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using OrderStatus = NeutronCore.Enums.OrderStatus;

namespace NeutronData.Repositories
{
    public class ReplenOrdersRepository : IReplenOrdersRepository
    {
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        private readonly WorkstationView _workstationView;
        private readonly IWorkstationRepository _workstationRepository;
        private readonly IAreaRepository _areaRepository;
        private static int _totalOrderDetailCount;
        private static int _runningOrderDetailCount;
        // private int[] _moveablePickStationIds;
        // private Workstation _rackStation;
        private List<Workstation> _pickStations;
        private int[] _allPickableAreaIds;
        private IDynamicLogger _logger;

        public ReplenOrdersRepository(WorkstationView workstationView
            , IWorkstationRepository workstationRepository
            , IAreaRepository areaRepository)
        {
            _workstationView = workstationView;
            _workstationRepository = workstationRepository;
            _areaRepository = areaRepository;
            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger(@"LocationsRepository");
            //_moveablePickStationIds = _workstationRepository.GetMoveablePickStationIds();
            //_rackStation = _workstationRepository.GetRackStation(8);
           // _pickStations = _workstationRepository.GetAllPickStations();
            _allPickableAreaIds = _areaRepository.GetAllAreaIds(); // .GetAllPickStationIds();
            _totalOrderDetailCount = 0;
            _runningOrderDetailCount = 0;
        }
        public IEnumerable<VelocityCode> GetVelocityCodesByArea(int areaId)
        {
            IEnumerable<VelocityCode> velocityCodes = new List<VelocityCode>();
            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get velocityCodeIds: {areaId} ");
                    velocityCodes = context.Database.SqlQuery<VelocityCode>("usp_GetVelocityCodesByArea @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Velocity Code Error. " + ex.Message + " " + ex.InnerException);
            }
            return velocityCodes;
        }

        public IEnumerable<SizeCode> GetSizeCodesByArea(int areaId)
        {
            IEnumerable<SizeCode> sizeCodes = new List<SizeCode>();
            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get SizeCodeIds: {areaId} ");
                    sizeCodes = context.Database.SqlQuery<SizeCode>("usp_GetSizeCodesByArea @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Size Code Error. " + ex.Message + " " + ex.InnerException);
            }
            return sizeCodes;
        }

        public IEnumerable<HeightCode> GetHeightCodesByArea(int areaId)
        {
            IEnumerable<HeightCode> heightCodes = new List<HeightCode>();
            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    _ = _logger.LogDetailAsync($"Get HeightCodeIds: {areaId} ");
                    heightCodes = context.Database.SqlQuery<HeightCode>("usp_GetHeightCodesByArea @AREAID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Height Code Error. " + ex.Message + " " + ex.InnerException);
            }
            return heightCodes;
        }

        public ReplenOrder GetOrder(int orderId)
        {
            ReplenOrder order = null;
            using (var db = new NeutronDb())
            {
                order = db.ReplenOrders.Include("ReplenOrderDetails").FirstOrDefault(r => r.Id == orderId);
            }
            return order;
        }

        public IEnumerable<ReplenOrderView> GetReplenOrderViews(string orderStatus = "1,2,3,4,5,7,8,9", string searchField = "")
        {
            var recs = new List<ReplenOrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<ReplenOrderView>("usp_GetReplenOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

        public IEnumerable<ReplenOrderView> GetCompletedReplenOrderViews(string orderStatus = "6", string searchField = "")
        {
            var recs = new List<ReplenOrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<ReplenOrderView>("usp_GetCompletedReplenOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public IEnumerable<ReplenOrderView> GetReplenStoreOrderViews(string orderStatus, string searchField)
        {
            var recs = new List<ReplenOrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<ReplenOrderView>("usp_GetReplenStoreOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public IEnumerable<ReplenOrderView> GetPutawayOrderViews(string orderStatus, string searchField)
        {
            var recs = new List<ReplenOrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<ReplenOrderView>("usp_GetPutawayOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested

                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }


            return recs;
        }

        public IEnumerable<ReplenOrderView> GetAvailableReplenOrderViews(string orderStatus = "1,2,3,4,5,7,8", string searchField = "")
        {
            var recs = new List<ReplenOrderView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@OrderStatus", value: orderStatus);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SearchField", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<ReplenOrderView>("usp_GetAvailableReplenOrderViews @OrderStatus, @SearchField", parameters.ToArray()).ToList(); // SQL Tested
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Order Views Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

        private string HasPicks(int[] moveableStationIds, int hasPickNum, ICollection<ReplenOrderDetail> orderDetails)
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

        private string HasRackPicks(int rackStationId, ICollection<ReplenOrderDetail> orderDetails)
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
        private string CheckForPicks4(int areaId, ICollection<ReplenOrderDetail> orderDetails)
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

        public IEnumerable<ReplenOrderView> GetOrderView()
        {
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Hold, (int)LineStatus.Picking, (int)LineStatus.Partial, (int)LineStatus.Archive };
            IEnumerable<ReplenOrderView> recs = _repoReplenOrders.All()
                // .Where(r => statusToGet.Contains(r.OrderStatusId))
                .Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                   // ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,
                    Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                    Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                    Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                    Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                    Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                    Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                    Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                    Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                    // Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                  //  ShipMethodId = s.ShipMethodId
                }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetOrderView(string search)
        {

            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Hold, (int)LineStatus.Picking, (int)LineStatus.Partial, (int)LineStatus.Archive };
            IEnumerable<ReplenOrderView> recs = _repoReplenOrders.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
               // ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                //Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
               // ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderBy(o => o.Ord1).ToList();
            IEnumerable<ReplenOrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }


        public List<AvailableReplenOrdersView> GetAvailableOrders(WorkstationView workstationView, string search, bool serialPicking, bool showSkips = false)
        {
            var recs = new List<AvailableReplenOrdersView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var records = context.ReplenOrderDetails.Include("ReplenOrder")
                        .Where(o => o.AreaId == workstationView.AreaId
                                    && o.ReplenOrder.OrderStatusId == (int)OrderStatus.Available)
                        .Where(p => p.LineStatusId == (int)LineStatus.Available).ToList();

                    var ords = records.GroupBy(r => new
                    {
                        r.ReplenOrderId,
                        r.ReplenOrder.Ord1
                            ,
                        r.ReplenOrder.Ord2,
                        r.ReplenOrder.Priority,
                        r.ReplenOrder.LoadDate
                    })
                         .Select(r => new AvailableReplenOrdersView
                         {
                             Id = r.Key.ReplenOrderId
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

                    foreach (var ord in ords)
                    {
                        ord.Order = context.ReplenOrders.Find(ord.Id);
                    }

                    foreach (var ord in ords)
                    {
                        foreach (var detail in ord.Order.ReplenOrderDetails)
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
                MessageBox.Show($"Get AvailableOrders View Error. {ex.Message}{Environment.NewLine}{ex.InnerException}");
            }

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetAvailableOrders(WorkstationView workstationView, string search)
        {
            var availableRecs = new List<ReplenOrderView>();
            IEnumerable<ReplenOrderView> recs = null;
            var replenOrderList = _repoReplenOrders.All().ToList();
            if (replenOrderList.Count == 0) return availableRecs;

            try
            {
                recs = replenOrderList.Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    //Starter = StartOnThisStation(workstationView.StationNumber, s.ReplenOrderDetails),
                    OrderStatusName = s.OrderStatus.Name,
                  //  ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,
                    Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                    Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                    Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                    Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                    Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                    Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                    Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                    Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                    //Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                    FirstPickArea = GetFirstPickArea(s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                   // ShipMethodId = s.ShipMethodId

                }).ToList();

                //})
                //   .OrderByDescending(o => o.Priority).ToList();
                //MessageBox.Show($"Recs");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetAvailableReplenOrders Error Phase 1 Recs Count: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]");
            }

            try
            {
                foreach (var item in recs)
                {
                    if (!string.IsNullOrEmpty(item.Station_1_HasPicks))
                    {
                        if (!string.Equals(item.Station_1_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 1;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_2_HasPicks))
                    {
                        if (!string.Equals(item.Station_2_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 2;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_3_HasPicks))
                    {
                        if (!string.Equals(item.Station_3_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 3;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_4_HasPicks))
                    {
                        if (!string.Equals(item.Station_4_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 4;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_5_HasPicks))
                    {
                        if (!string.Equals(item.Station_5_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 5;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_6_HasPicks))
                    {
                        if (!string.Equals(item.Station_6_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 6;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_7_HasPicks))
                    {
                        if (!string.Equals(item.Station_7_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 7;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(item.Station_8_HasPicks))
                    {
                        if (!string.Equals(item.Station_8_HasPicks, b: @"C", comparisonType: StringComparison.CurrentCultureIgnoreCase))
                        {
                            item.CurrentPickArea = 8;
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign Next Station: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]");
            }

            try
            {
                foreach (var item in recs.Where(r => r.CurrentPickArea == workstationView.AreaId))
                {
                    //todo fix this here "_areaIdsForThisWorkstation.Contains(r.AreaId))" something not right
                    List<ReplenOrderDetail> details = item.ReplenOrder.ReplenOrderDetails.Where(r => r.AreaId == workstationView.AreaId).ToList();
                    item.ReplenOrder.ReplenOrderDetails = details;
                    availableRecs.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign Detail Lines this Station - {workstationView.WorkstationNumber}: Rec Count: {recs.Count()}\r\n  {ex.Message} \r\n {ex.InnerException} [{DateTime.Now.ToLongTimeString()}]");
            }

            IEnumerable<ReplenOrderView> result = availableRecs.Where(s => s.SearchField.Contains(search));

            return result;
        }

        private string StartOnThisArea(int areaId, ICollection<ReplenOrderDetail> orderDetails)
        {
            var result = string.Empty;
            try
            {
                if (orderDetails.Count > 0 && areaId > 1)
                {
                    int firstPickArea = orderDetails.Min(o => o.AreaId);
                    if (firstPickArea == areaId)
                    {
                        result = @"S";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start on this Area Error.  {ex.Message} \r\n {ex.InnerException}");
            }
            return result;
        }

        private int GetFirstPickArea(IEnumerable<ReplenOrderDetail> orderDetails)
        {
            return orderDetails.Min(o => o.AreaId);
        }

        public IEnumerable<ReplenOrderView> GetCompletedOrders(string find = "")
        {
            IEnumerable<ReplenOrderView> recs = _repoReplenOrders.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
              //  ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                //Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
              //  ShipMethodId = s.ShipMethodId
            }).Where(r => r.OrderStatusId == 6)
            .OrderByDescending(o => o.Priority).ToList();
            var result = recs.Where(s => s.SearchField.Contains(find));
            return result;
        }

        public ReplenOrder GetOrder()
        {
            var ord = _repoReplenOrders.All().FirstOrDefault();
            if (ord == null) return null;
            var lines = ord.Lines();
            var pieces = ord.Pieces();

            return ord;
        }

        public List<ReplenPickView> GetOrderLines()
        {
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Partial };
            var pickList = new List<ReplenPickView>();
            List<ReplenOrder> result = _repoReplenOrders.AllInclude(n => n.ReplenOrderDetails)
                .Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
            foreach (var ord in result)
            {
                var orderDetails = ord.ReplenOrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();

                foreach (var detail in orderDetails)
                {
                    var pickView = new ReplenPickView()
                    {
                        OrderId = detail.ReplenOrderId,
                        Ord1 = detail.ReplenOrder.Ord1,
                        Ord2 = detail.ReplenOrder.Ord2,
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

        public List<AvailableReplenOrdersView> GetAvailableOrders(WorkstationView workstationView)
        {
            var recs = new List<AvailableReplenOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@WORKSTATIONID", value: workstationView.WorkstationId);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<AvailableReplenOrdersView>("usp_GetAvailableReplenOrdersByStation @WORKSTATIONID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Available Replen Orders Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetAvailableOrders(string search)
        {
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Partial };
            IEnumerable<ReplenOrderView> recs = _repoReplenOrders.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
               // ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                //Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
               // ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Ord1).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        private string CheckForPicks(int areaId, IEnumerable<ReplenOrderDetail> orderDetails)
        {
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Hold, (int)LineStatus.Complete };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.AreaId == areaId)
                {
                    if (statusToGet.Contains(item.ReplenOrder.OrderStatusId))
                    {
                        result = item.ReplenOrder.OrderStatus.Name.Substring(0, 1);
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

        private string CheckForPicks2(int areaId, ICollection<ReplenOrderDetail> orderDetails)
        {
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Hold, (int)LineStatus.Complete };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.AreaId == areaId)
                {
                    if (statusToGet.Contains(item.ReplenOrder.OrderStatusId))
                    {
                        result = item.ReplenOrder.OrderStatus.Name.Substring(0, 1);
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

        private string CheckForPicks3(int areaId, ICollection<ReplenOrderDetail> orderDetails)
        {
            var lineStatus = new List<int>();
            string result = @"";
            if (orderDetails.Count > 0)
            {
                foreach (var item in orderDetails)
                {
                    if (item.AreaId == areaId)
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


        public List<ReplenPickView> GetOrderLines(List<BatchPosition> ordersToPick)
        {
            var pickViews = new List<ReplenPickView>();
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Partial };
            int[] orderIds = GetOrderIdArray(ordersToPick);

            if (orderIds.Length > 0)
            {
                List<ReplenOrder> orders = _repoReplenOrders.AllInclude(n => n.ReplenOrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                if (orders.Count > 0)
                {
                    foreach (ReplenOrder ord in orders)
                    {
                        int pos = GetPosition(ord.Id, ordersToPick);
                        List<ReplenOrderDetail> orderDetails = ord.ReplenOrderDetails.Where(o => statusToGet.Contains(o.LineStatusId)).ToList();

                        if (orderDetails.Count > 0)
                        {
                            foreach (var detail in orderDetails)
                            {
                                var pickView = new ReplenPickView()
                                {
                                    PickPosition = pos,
                                    OrderId = detail.ReplenOrder.Id,
                                    Ord1 = detail.ReplenOrder.Ord1,
                                    Ord2 = detail.ReplenOrder.Ord2,
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

        public IEnumerable<ReplenPickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum)
        {
            var pickViews = new List<ReplenPickView>();
            var statusToGet = new int[] { (int)LineStatus.Available, (int)LineStatus.Partial };
            int[] orderIds = GetOrderIdArray(ordersToPick);
            ItemDefinition newItemDefinition = _repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            if (newItemDefinition != null)
            {
                if (orderIds.Length > 0)
                {
                    List<ReplenOrder> orders = _repoReplenOrders.AllInclude(n => n.ReplenOrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
                    if (orders.Count > 0)
                    {
                        foreach (ReplenOrder ord in orders)
                        {
                            //put the ItemDefinition back to a New Item
                            List<ReplenOrderDetail> orderDetailsToUpdate = _repoReplenOrderDetails.FindBy(r => r.PartNum == partNum && r.ReplenOrderId == ord.Id).ToList();
                            if (orderDetailsToUpdate.Count > 0)
                            {
                                foreach (var od in orderDetailsToUpdate)
                                {
                                    od.ItemDefinition = newItemDefinition;
                                    od.ItemDefinitionId = newItemDefinition.Id;
                                    _repoReplenOrderDetails.Update(od);
                                }

                                int pos = GetPosition(ord.Id, ordersToPick);
                                List<ReplenOrderDetail> orderDetails = _repoReplenOrderDetails.FindBy(o => statusToGet.Contains(o.LineStatusId) && o.ItemDefinitionId == newItemDefinition.Id).ToList();

                                if (orderDetails.Count > 0)
                                {
                                    foreach (var detail in orderDetails)
                                    {
                                        var pickView = new ReplenPickView()
                                        {
                                            PickPosition = pos,
                                            OrderId = detail.ReplenOrder.Id,
                                            Ord1 = detail.ReplenOrder.Ord1,
                                            Ord2 = detail.ReplenOrder.Ord2,
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

        //public ReplenPickView CreatePickView(int pos, ReplenOrderDetail detail)
        //{

        //    var pickView = new ReplenPickView()
        //    {
        //        PickPosition = pos,
        //        ReplenOrderId = detail.ReplenOrder.Id,
        //        Ord1 = detail.ReplenOrder.Ord1,
        //        Ord2 = detail.ReplenOrder.Ord2,
        //        ItemId = detail.ItemDefinitionId,
        //        Item = string.Empty,
        //        Description = string.Empty,
        //        Quantity = detail.Quantity,
        //        QuantityToBePicked = detail.Quantity,
        //        PickedQty = 0,
        //        Slot = string.Empty,
        //        SlotQty = 0,
        //        ReplenOrderDetail = detail
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
            foreach (var bp in ordersToPick)
            {
                if (bp.OrderId != 0)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }
            return orderIds.ToArray();
        }

        //IEnumerable<RackOrderView> IReplenOrdersRepository.GetRackOrdersView(int rackStationNumber, string search)
        //{
        //    throw new NotImplementedException();
        //}

        public IEnumerable<RackReplenOrderView> GetRackOrdersView(int areaId, string search)
        {
            IEnumerable<RackReplenOrderView> recs = _repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new RackReplenOrderView
            {
                AreaId = areaId,
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                Priority = s.Priority,
                Order = s,
                LoadDate = s.LoadDate,
                OrderDetails = s.ReplenOrderDetails.Where(o => o.LineStatusId != (int)LineStatus.Complete && o.AreaId == areaId).ToList()
            }).Where(o => o.Order.OrderStatusId != (int)LineStatus.Complete)
                .OrderBy(o => o.Ord2).ToList();

            var result = recs.Where(s => s.SearchField.Contains(search) && s.OrderDetails.Count > 0);
            return result;
        }

        //ReplenOrder IReplenOrdersRepository.GetOrderAndOrderDetails(int? orderId, int areaId)
        //{
        //    var ord = new ReplenOrder();
        //    var availableSkip = new int[] { (int)LineStatus.Available, (int)LineStatus.Skipped };

        //    if (orderId == null) return ord;
        //    ord = _repoReplenOrders.FindByKey(orderId);

        //    //using (var db = new NeutronDb())
        //    //{
        //    //    ord = db.ReplenOrders.FirstOrDefault(x => x.Id == orderId);

        //    //    if (ord != null)
        //    //    {
        //    //        var q2 = ord.ReplenOrderDetails.Include("ItemDefinition").Where(d => d.StationNumber == stationNumber
        //    //                    && availableSkip.Contains(d.LineStatusId)).ToList();
        //    //        ord.ReplenOrderDetails = q2.ToList(); 
        //    //    }
        //    //}

        //    if (ord != null)
        //    {
        //        //ord.OrderDetails = null;

        //        // var details = _repoOrderDetails.All()
        //        ord.ReplenOrderDetails = ord.ReplenOrderDetails.Where(x => x.ReplenOrderId == orderId && x.AreaId == areaId && availableSkip.Contains(x.LineStatusId)).ToList();
        //        //ord.OrderDetails = details;
        //    }

        //    //}
        //    return ord;
        //}


        public ReplenOrder GetOrderAndOrderDetails(int orderId, int areaId)
        {
            //var order = new ReplenOrder();
            // var availableSkip = new int[] { (int)LineStatus.Available, (int)LineStatus.Skipped };
            var order = GetOrder(orderId);
           // var order = _repoReplenOrders.FindByKey(orderId);
            if (order != null)
            {
                var details = GetReplenOrderDetailsByOrderAndArea(orderId, areaId);

                order.ReplenOrderDetails = new List<ReplenOrderDetail>();

                foreach (var orderDetail in details.Where(orderDetail => orderDetail.AreaId == areaId).Where(orderDetail => orderDetail.LineStatusId is 1 or 9))
                {
                    order.ReplenOrderDetails.Add(orderDetail);
                }

                //ord.ReplenOrderDetails = ord.ReplenOrderDetails.Where(x => x.ReplenOrderId == orderId && x.AreaId == areaId && availableSkip.Contains(x.LineStatusId)).ToList();
            }

            return order;
        }

        public List<ReplenOrderDetail> GetReplenOrderDetailsByOrderAndArea(int orderId, int areaId)
        {
            var recs = new List<ReplenOrderDetail>();

            try
            {
                var parameters = new List<object>();
                using var context = new NeutronDb();
                var param = new SqlParameter(parameterName: "@ORDERID", value: orderId);
                parameters.Add(param);
                param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                parameters.Add(param);

                recs = context.Database.SqlQuery<ReplenOrderDetail>("usp_GetReplenOrderDetailsByOrderAndArea @ORDERID, @AREAID", parameters.ToArray()).ToList(); // SQL Tested
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get OrderDetails Error. " + ex.Message + " " + ex.InnerException);
            }
            return recs;
        }

        public IEnumerable<ReplenOrderView> GetRackOrders(string search = "")
        {
            IEnumerable<ReplenOrderView> recs = _repoReplenOrders.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
              //  ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = HasPicks(_allPickableAreaIds, 1, s.ReplenOrderDetails),
                Station_2_HasPicks = HasPicks(_allPickableAreaIds, 2, s.ReplenOrderDetails),
                Station_3_HasPicks = HasPicks(_allPickableAreaIds, 3, s.ReplenOrderDetails),
                Station_4_HasPicks = HasPicks(_allPickableAreaIds, 4, s.ReplenOrderDetails),
                Station_5_HasPicks = HasPicks(_allPickableAreaIds, 5, s.ReplenOrderDetails),
                Station_6_HasPicks = HasPicks(_allPickableAreaIds, 6, s.ReplenOrderDetails),
                Station_7_HasPicks = HasPicks(_allPickableAreaIds, 7, s.ReplenOrderDetails),
                Station_8_HasPicks = HasPicks(_allPickableAreaIds, 8, s.ReplenOrderDetails),
                //Station_8_HasPicks = _rackStation == null ? string.Empty : HasRackPicks(_rackStation.Id, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
              //  ShipMethodId = s.ShipMethodId
            }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderBy(o => o.Ord1).ToList();
            IEnumerable<ReplenOrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }

        public List<AvailableReplenOrdersView> GetAvailableReplenOrdersForInductionScreen(int areaId, string searchField)
        {
            _ = _logger.LogDetailAsync($"GetAvailableReplenOrdersForInductionScreen  AREAID: {areaId}  SEARCH: {searchField}");
            var recs = new List<AvailableReplenOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    parameters.Add(param);
                    param = new SqlParameter(parameterName: "@SEARCHFIELD", value: searchField);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<AvailableReplenOrdersView>("usp_GetAvailableReplenOrdersForInductionScreen @AREAID, @SEARCHFIELD", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get Available Replen Order Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }
    }
}
