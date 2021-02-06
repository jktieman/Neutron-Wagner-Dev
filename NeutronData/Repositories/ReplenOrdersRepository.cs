using Neutron.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;

namespace NeutronData.Repositories
{
    public class ReplenOrdersRepository
    {
        private readonly  GenericRepository<ReplenOrder> _repo = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly  GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly  GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());

        public IEnumerable<ReplenOrderView> GetOrderViewNotCompleted()
        {
            // var statusToGet = new int[] { 1, 2, 3, 4 };
            IEnumerable<ReplenOrderView> recs = _repo.AllInclude(r => r.ReplenOrderDetails)
                // .Where(r => statusToGet.Contains(r.OrderStatusId))
                .Where(r => r.OrderStatusId != 6)
                .Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,
                    Station_1_HasPicks = CheckForPicks3(1, s.ReplenOrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.ReplenOrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.ReplenOrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.ReplenOrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.ReplenOrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetOrderViewNotCompleted(string search)
        {
            IEnumerable<ReplenOrderView> recs = _repo.AllInclude(r => r.ReplenOrderDetails)
                .Where(r => r.OrderStatusId != 6)
                .Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,
                    Station_1_HasPicks = CheckForPicks3(1, s.ReplenOrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.ReplenOrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.ReplenOrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.ReplenOrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.ReplenOrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                })
                .OrderBy(o => o.Ord1).ToList();
            var result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }


        public IEnumerable<ReplenOrderView> GetOrderView()
        {
             var statusToGet = new int[] { 1, 2, 3, 4, 8 };
            IEnumerable<ReplenOrderView> recs = _repo.All()
                // .Where(r => statusToGet.Contains(r.OrderStatusId))
                .Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,
                    Station_1_HasPicks = CheckForPicks3(1, s.ReplenOrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.ReplenOrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.ReplenOrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.ReplenOrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.ReplenOrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId
                }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderBy(o => o.Id).ToList();

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetOrderView(string search)
        {
            var statusToGet = new int[] { 1, 2, 3, 4, 8 };
            IEnumerable<ReplenOrderView> recs = _repo.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = CheckForPicks(1, s.ReplenOrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.ReplenOrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.ReplenOrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.ReplenOrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.ReplenOrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderBy(o => o.Ord1).ToList();
            IEnumerable<ReplenOrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }


        public List<AvailableReplenOrdersView> GetAvailableOrders(StationView station, string search, bool serialPicking)
        {
            var recs = new List<AvailableReplenOrdersView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var records = context.ReplenOrderDetails.Include("ReplenOrder").Where(o => o.StationNumber == station.StationNumber && (o.ReplenOrder.OrderStatusId == 1))
                        .Where(p => p.LineStatusId == 1).ToList();
                   
                    var ords = records.GroupBy(r => new { r.ReplenOrderId, r.ReplenOrder.Ord1, r.ReplenOrder.Ord2, r.ReplenOrder.Priority, r.ReplenOrder.LoadDate })
                         .Select(r => new AvailableReplenOrdersView
                         {
                             Id = r.Key.ReplenOrderId
                             , Ord1 = r.Key.Ord1
                             , Ord2 = r.Key.Ord2
                             , Priority = r.Key.Priority
                             , Lines = r.Count()
                             , Pieces = r.Sum(s => s.Quantity)
                             , LoadDate = r.Key.LoadDate
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

        public IEnumerable<ReplenOrderView> GetAvailableOrders(StationView station, string search)
        {
            var availableRecs = new List<ReplenOrderView>();
            IEnumerable<ReplenOrderView> recs = null;
            List<ReplenOrder> rs = _repo.All().ToList();

            try
            {
                recs = rs.Select(s => new ReplenOrderView
                {
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    //Starter = StartOnThisStation(station.StationNumber, s.ReplenOrderDetails),
                    OrderStatusName = s.OrderStatus.Name,
                    ShipMethodName = s.ShipMethod.Name,
                    Priority = s.Priority,
                    ReplenOrder = s,

             
                Station_1_HasPicks = CheckForPicks3(1, s.ReplenOrderDetails),
                    Station_2_HasPicks = CheckForPicks3(2, s.ReplenOrderDetails),
                    Station_3_HasPicks = CheckForPicks3(3, s.ReplenOrderDetails),
                    Station_4_HasPicks = CheckForPicks3(4, s.ReplenOrderDetails),
                    Station_5_HasPicks = CheckForPicks3(5, s.ReplenOrderDetails),
                    Station_8_HasPicks = CheckForPicks3(8, s.ReplenOrderDetails),
                    FirstPickStation = GetFirstPickStation(s.ReplenOrderDetails),
                    LoadDate = s.LoadDate,
                    OrderStatusId = s.OrderStatusId,
                    ShipMethodId = s.ShipMethodId

                }).ToList();
                
                //})
                //   .OrderByDescending(o => o.Priority).ToList();
                //MessageBox.Show($"Recs");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"GetAvailableReplenOrders Error Phase 1 Recs Count: {recs.Count()} {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
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
                    List<ReplenOrderDetail> details = item.ReplenOrder.ReplenOrderDetails.Where(r => r.StationNumber == station.StationNumber).ToList();
                    item.ReplenOrder.ReplenOrderDetails = details;
                    availableRecs.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assign Detail Lines this Station - {station.StationNumber}: Rec Count: {recs.Count()}\r\n  {ex.Message} \r\n {ex.InnerException} [{System.DateTime.Now.ToLongTimeString()}]");
            }

            IEnumerable<ReplenOrderView> result = availableRecs.Where(s => s.SearchField.Contains(search));

            return result;
        }

        private string StartOnThisStation(int stationNumber, ICollection<ReplenOrderDetail> orderDetails)
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

        private int GetFirstPickStation(ICollection<ReplenOrderDetail> orderDetails)
        {
            return orderDetails.Min(o => o.StationNumber);
        }

        public IEnumerable<ReplenOrderView> GetCompletedOrders(string find = "")
        {
            IEnumerable<ReplenOrderView> recs = _repo.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = CheckForPicks(1, s.ReplenOrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.ReplenOrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.ReplenOrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.ReplenOrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.ReplenOrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => r.OrderStatusId == 6)
            .OrderByDescending(o => o.Priority).ToList();
            var result = recs.Where(s => s.SearchField.Contains(find));
            return result;
        }

        public ReplenOrder GetOrder()
        {
            var ord = _repo.All().FirstOrDefault();

            var lines = ord.Lines();
            var pieces = ord.Pieces();

            return ord;
        }

        public List<ReplenPickView> GetOrderLines()
        {
            var statusToGet = new int[] { 1, 4 };
            var pickList = new List<ReplenPickView>();
            List<ReplenOrder> result = _repo.AllInclude(n => n.ReplenOrderDetails).Where(r => statusToGet.Contains(r.OrderStatusId)).ToList();
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

        public List<AvailableReplenOrdersView> GetAvailableOrders(StationView station)
        {
            var recs = new List<AvailableReplenOrdersView>();

            try
            {
                var parameters = new List<object>();
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@STATIONID", value: station.StationNumber);
                    parameters.Add(param);

                    recs = context.Database.SqlQuery<AvailableReplenOrdersView>("usp_GetAvailableReplenOrdersByStation @STATIONID", parameters.ToArray()).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get Available Replen Orders Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public IEnumerable<ReplenOrderView> GetAvailableOrders(string search)
        {
            var statusToGet = new int[] { 1, 4 };
            IEnumerable<ReplenOrderView> recs = _repo.All().Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = CheckForPicks(1, s.ReplenOrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.ReplenOrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.ReplenOrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.ReplenOrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.ReplenOrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId,
            }).Where(r => statusToGet.Contains(r.OrderStatusId))
            .OrderByDescending(o => o.Ord1).Where(r => r.SearchField.Contains(search.ToLower())).ToList();

            return recs;
        }

        private string CheckForPicks(int stationNumber, ICollection<ReplenOrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 6 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.StationNumber == stationNumber)
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

        private string CheckForPicks2(int stationNumber, ICollection<ReplenOrderDetail> orderDetails)
        {
            var statusToGet = new int[] { 1, 2, 6 };
            string result = @"";
            foreach (var item in orderDetails)
            {
                if (item.StationNumber == stationNumber)
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

        private string CheckForPicks3(int stationNumber, ICollection<ReplenOrderDetail> orderDetails)
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


        public List<ReplenPickView> GetOrderLines(List<BatchPosition> ordersToPick)
        {
            var pickViews = new List<ReplenPickView>();
            var statusToGet = new int[] { 1, 4 };
            int[] orderIds = GetOrderIdArray(ordersToPick);

            if (orderIds.Length > 0)
            {
                List<ReplenOrder> orders = _repo.AllInclude(n => n.ReplenOrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
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
                                    ReplenOrderId = detail.ReplenOrder.Id,
                                    Ord1 = detail.ReplenOrder.Ord1,
                                    Ord2 = detail.ReplenOrder.Ord2,
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

        public IEnumerable<ReplenPickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum)
        {
            var pickViews = new List<ReplenPickView>();
            var statusToGet = new int[] { 1, 4 };
            int[] orderIds = GetOrderIdArray(ordersToPick);
            ItemDefinition newItemDefinition = _repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            if (newItemDefinition != null)
            {
                if (orderIds.Length > 0)
                {
                    List<ReplenOrder> orders = _repo.AllInclude(n => n.ReplenOrderDetails).Where(r => orderIds.Contains(r.Id)).ToList();
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
                                            ReplenOrderId = detail.ReplenOrder.Id,
                                            Ord1 = detail.ReplenOrder.Ord1,
                                            Ord2 = detail.ReplenOrder.Ord2,
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
            foreach (BatchPosition bp in ordersToPick)
            {
                if (bp.OrderId != null)
                {
                    orderIds.Add(Convert.ToInt32(bp.OrderId));
                }
            }
            return orderIds.ToArray();
        }

        public IEnumerable<RackReplenOrderView> GetRackOrdersView(int stationNumber, string search)
        {
            IEnumerable<RackReplenOrderView> recs = _repo.AllInclude(r => r.ReplenOrderDetails).Select(s => new RackReplenOrderView
                {
                    StationNumber = stationNumber,
                    Id = s.Id,
                    Ord1 = s.Ord1,
                    Ord2 = s.Ord2,
                    Priority = s.Priority,
                    Order = s,
                    LoadDate = s.LoadDate,
                    OrderDetails = s.ReplenOrderDetails.Where(o => o.LineStatusId != 6 && o.StationNumber == stationNumber).ToList()
                }).Where(o => o.Order.OrderStatusId != 6)
                .OrderBy(o => o.Ord2).ToList();

            var result = recs.Where(s => s.SearchField.Contains(search) && s.OrderDetails.Count > 0);
            return result;
        }

        public ReplenOrder GetOrderAndOrderDetails(int? orderId, int stationNumber)
        {
            var ord = new ReplenOrder();
            var availableSkip = new int[] { 1, 9 };
            if (orderId != null)
            {
                ord = _repo.FindByKey(orderId);
                if (ord != null)
                {
                    ord.ReplenOrderDetails = ord.ReplenOrderDetails.Where(x => x.ReplenOrderId == orderId && x.StationNumber == stationNumber && availableSkip.Contains(x.LineStatusId)).ToList();
                }
            }
            return ord;
        }

        public IEnumerable<ReplenOrderView> GetRackOrders(string search = "")
        {
            IEnumerable<ReplenOrderView> recs = _repo.AllInclude(r => r.ReplenOrderDetails).Select(s => new ReplenOrderView
            {
                Id = s.Id,
                Ord1 = s.Ord1,
                Ord2 = s.Ord2,
                OrderStatusName = s.OrderStatus.Name,
                ShipMethodName = s.ShipMethod.Name,
                Priority = s.Priority,
                ReplenOrder = s,
                Station_1_HasPicks = CheckForPicks(1, s.ReplenOrderDetails),
                Station_2_HasPicks = CheckForPicks(2, s.ReplenOrderDetails),
                Station_3_HasPicks = CheckForPicks(3, s.ReplenOrderDetails),
                Station_4_HasPicks = CheckForPicks(4, s.ReplenOrderDetails),
                Station_5_HasPicks = CheckForPicks(5, s.ReplenOrderDetails),
                Station_8_HasPicks = CheckForPicks(8, s.ReplenOrderDetails),
                LoadDate = s.LoadDate,
                OrderStatusId = s.OrderStatusId,
                ShipMethodId = s.ShipMethodId
            }).Where(r => !string.IsNullOrEmpty(r.Station_8_HasPicks))
                .OrderBy(o => o.Ord1).ToList();
            IEnumerable<ReplenOrderView> result = recs.Where(s => s.SearchField.Contains(search));
            return result;
        }
    }
}
