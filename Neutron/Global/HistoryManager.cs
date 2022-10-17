using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronCore.Enums;
using System.Data.SqlClient;
using NeutronCore.Extensions;
using NeutronData.Interfaces;
using NeutronData.PrintModels;

namespace Neutron.Global
{

    public class HistoryManager : IHistoryManager
    {
        private readonly StationView _stationView;
        private readonly IInventoryRepository _inventoryRepository;

        private readonly GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails =
            new GenericRepository<ReplenOrderDetail>(new NeutronDb());


        public HistoryManager(IInventoryRepository inventoryRepository,  StationView stationView)
        {
            _inventoryRepository = inventoryRepository;
            _stationView = stationView;
        }


        public void SaveHistory(ActionCode actionCode, Order order)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                Priority = order.Priority,
                LoadDate = order.LoadDate,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = order.OrderInfo,
                OrderDetailInfo = string.Empty,
                StationId = _stationView.StationId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, ReplenOrder order)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                Priority = order.Priority,
                LoadDate = order.LoadDate,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = order.OrderInfo,
                OrderDetailInfo = string.Empty,
                StationId = _stationView.StationId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, OrderView order)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                Priority = order.Priority,
                LoadDate = order.LoadDate,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = string.Empty,   //order.Order.OrderInfo,
                OrderDetailInfo = string.Empty,
                StationId = _stationView.StationId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, ReplenOrderView order)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                Priority = order.Priority,
                LoadDate = order.LoadDate,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = string.Empty,   //.ReplenOrder.OrderInfo,
                OrderDetailInfo = string.Empty,
                StationId = _stationView.StationId
            };
             Save(history);

        }

        public void SaveHistory(ActionCode actionCode, OrderDetail orderDetail)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = orderDetail.Order.Ord1,
                Ord2 = orderDetail.Order.Ord2,
                OrderId = orderDetail.OrderId,
                Item = orderDetail.PartNum,
                Description = orderDetail.PartDesc,
                RequestedQuantity = orderDetail.Quantity,
                IssuedQuantity = orderDetail.PickedQuantity,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = orderDetail.Order.OrderInfo,
                OrderDetailInfo = orderDetail.OrderDetailInfo,
                StationId = _stationView.StationId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, ReplenOrderDetail orderDetail)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = orderDetail.ReplenOrder.Ord1,
                Ord2 = orderDetail.ReplenOrder.Ord2,
                OrderId = orderDetail.ReplenOrderId,
                Item = orderDetail.PartNum,
                Description = orderDetail.PartDesc,
                RequestedQuantity = orderDetail.Quantity,
                IssuedQuantity = orderDetail.PickedQuantity,
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = orderDetail.ReplenOrder.OrderInfo,
                OrderDetailInfo = orderDetail.OrderDetailInfo,
                StationId = _stationView.StationId
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, PickStop pickStop)
        {
            foreach (var pickView in pickStop.PickViews)
            {
                foreach (var pickLocation in pickView.PickLocations)
                {
                    var cCenter = "          ";
                    if (pickView.OrderDetail.OrderDetailInfo?.Length >= 10)
                    {
                        cCenter = pickView.OrderDetail.OrderDetailInfo?.Substring(0, 10);
                    }

                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionCodeName = actionCode.GetEnumDescription(),
                        ActionDateTime = pickLocation.PickDate,
                        Ord1 = pickView.Ord1,
                        Ord2 = pickView.Ord2,
                        OrderId = pickView.OrderId,
                        Item = pickView.Item,
                        Description = pickView.Description,
                        RequestedQuantity = pickView.Quantity,
                        IssuedQuantity = pickLocation.Quantity,
                        StationId = _stationView.StationId,   // pickLocation.Inventory.Station.Id,
                        Loc1 = pickLocation.Inventory.Location.Loc1,
                        Loc2 = pickLocation.Inventory.Location.Loc2,
                        Loc3 = pickLocation.Inventory.Location.Loc3,
                        Loc4 = pickLocation.Inventory.Location.Loc4,
                        Loc5 = pickLocation.Inventory.Location.Loc5,
                        Slot = pickLocation.Inventory.Location.Slot,
                        EmpId = GlobalVar.User.EmpId,
                        CostCenter = cCenter,
                        OrderInfo = pickView.OrderDetail.Order.OrderInfo,
                        OrderDetailInfo = pickView.OrderDetail.OrderDetailInfo
                    };
                    Save(history);
                }
            }
        }

        public void SaveHistory(ActionCode actionCode, ReplenPickStop pickStop)
        {
            foreach (var pickView in pickStop.PickViews)
            {
                foreach (var pickLocation in pickView.PickLocations)
                {
                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionCodeName = actionCode.GetEnumDescription(),
                        ActionDateTime = pickLocation.PickDate,
                        OrderId = pickView.OrderId,
                        Ord1 = pickView.Ord1,
                        Ord2 = pickView.Ord2,
                        Item = pickView.Item,
                        Description = pickView.Description,
                        RequestedQuantity = pickView.Quantity,
                        IssuedQuantity = pickLocation.Quantity,
                        StationId = _stationView.StationId,   // pickLocation.Inventory.StationId,
                        Loc1 = pickLocation.Inventory.Location.Loc1,
                        Loc2 = pickLocation.Inventory.Location.Loc2,
                        Loc3 = pickLocation.Inventory.Location.Loc3,
                        Loc4 = pickLocation.Inventory.Location.Loc4,
                        Loc5 = pickLocation.Inventory.Location.Loc5,
                        Slot = pickLocation.Inventory.Location.Slot,
                        EmpId = GlobalVar.User.EmpId,
                        CostCenter = string.Empty,
                        OrderInfo = string.Empty,
                        OrderDetailInfo = string.Empty
                    };
                    Save(history);
                }
            }
        }

        public void SaveHistory(ActionCode actionCode, Order value, int stationId)
        {

            var cCenter = "          ";
            var orderDetailInfo = string.Empty;
            var info = value.OrderInfo;

            if (!string.IsNullOrEmpty(info))
            {
                if (info.EndsWith("261") || info.Length == 24)
                {
                    orderDetailInfo = $"{info}";
                }

                else if (info.Length == 36)
                {
                    cCenter = info.Substring(0, 10);
                    orderDetailInfo = info;
                }
                else
                {
                    orderDetailInfo = info.Trim();
                }
            }

            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = value.Ord1,
                Ord2 = value.Ord2,
                OrderId = value.Id,
                Item = string.Empty,
                Description = string.Empty,
                RequestedQuantity = 0,
                IssuedQuantity = 0,
                Slot = string.Empty,
                EmpId = GlobalVar.User.EmpId,
                StationId = _stationView.StationId,  // stationId,
                //OrderDetailId = value.Id,
                CostCenter = cCenter,
                OrderInfo = value.OrderInfo,
                OrderDetailInfo = orderDetailInfo
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, OrderDetail value, int stationId)
        {

            var cCenter = "          ";
            var orderDetailInfo = string.Empty;
            var info = value.OrderDetailInfo;

            if (!string.IsNullOrEmpty(info))
            {
                if (info.EndsWith("261") || info.Length == 24)
                {
                    orderDetailInfo = $"{info}";
                }

                else if (info.Length == 36)
                {
                    cCenter = info.Substring(0, 10);
                    orderDetailInfo = info;
                }
                else
                {
                    orderDetailInfo = info.Trim();
                }
            }

            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = value.Order.Ord1,
                Ord2 = value.Order.Ord2,
                OrderId = value.OrderId,
                Item = value.PartNum,
                Description = value.PartDesc,
                RequestedQuantity = value.Quantity,
                IssuedQuantity = value.PickedQuantity,
                Slot = value.PrimeBin,
                EmpId = GlobalVar.User.EmpId,
                StationId = _stationView.StationId, // stationId,
                OrderDetailId = value.Id,
                CostCenter = cCenter,
                OrderInfo = value.Order.OrderInfo,
                OrderDetailInfo = orderDetailInfo
            };
            Save(history);
        }
        //Inventory Modify
        public void SaveHistory(ActionCode actionCode, Inventory inventory)
        {
            var inv = _inventoryRepository.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = string.Empty,
                Ord2 = string.Empty,
                OrderId = 0,
                OrderDetailId = null,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = inv.Quantity,
                RequestedQuantity = 0,
                StationId = _stationView.StationId, //  inv.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        //Hot Action
        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, PickView pickView)
        {
            var cCenter = "          ";
            var orderDetailInfo = string.Empty;
            var info = pickView.OrderDetail.OrderDetailInfo;

            if (!string.IsNullOrEmpty(info))
            {
                if (info.EndsWith("261") || info.Length == 24)
                {
                    orderDetailInfo = $"{info}";
                }

                else if (info.Length == 36)
                {
                    cCenter = info.Substring(0, 10);
                    orderDetailInfo = info;
                }
                else
                {
                    orderDetailInfo = info.Trim();
                }
            }

            var inv = _inventoryRepository.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = pickView.Ord1,
                Ord2 = pickView.Ord2,
                OrderId = pickView.OrderId,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                RequestedQuantity = pickView.Quantity,
                StationId = _stationView.StationId, //  inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = cCenter,
                OrderInfo = pickView.OrderDetail.Order.OrderInfo,
                OrderDetailInfo = orderDetailInfo
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, PickList pickList)
        {
            var orderId = _repoReplenOrderDetails.FindByKey(pickList.OrderDetailId.ParseInt()).ReplenOrderId;
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = pickList.Order,
                Ord2 = pickList.Invoice,
                OrderId = orderId,
                Item = inventory.ItemDefinition.Item,
                Description = inventory.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                RequestedQuantity = pickList.Ordered.ParseInt(),
                StationId = _stationView.StationId, //  inventory.Location.StationId,
                Loc1 = inventory.Location.Loc1,
                Loc2 = inventory.Location.Loc2,
                Loc3 = inventory.Location.Loc3,
                Loc4 = inventory.Location.Loc4,
                Loc5 = inventory.Location.Loc5,
                Slot = inventory.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, OrderDetail orderDetail)
        {

            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = orderDetail.Order.Ord1,
                Ord2 = orderDetail.Order.Ord2,
                OrderId = orderDetail.OrderId,
                Item = orderDetail.PartNum,
                Description = orderDetail.PartDesc,
                IssuedQuantity = pickedQty,
                RequestedQuantity = orderDetail.Quantity,
                StationId = _stationView.StationId, //  inventory.Location.StationId,
                Loc1 = inventory.Location.Loc1,
                Loc2 = inventory.Location.Loc2,
                Loc3 = inventory.Location.Loc3,
                Loc4 = inventory.Location.Loc4,
                Loc5 = inventory.Location.Loc5,
                Slot = inventory.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = orderDetail.Order.OrderInfo,
                OrderDetailInfo = orderDetail.OrderDetailInfo
            };
            Save(history);
        }

        //Hot Pick Hot Store Action Without Cost Center
        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty)
        {
            string orderText;
            switch (actionCode)
            {
                case ActionCode.PickHot:
                    {
                        orderText = "  HOT PICK";
                        break;
                    }
                case ActionCode.StoreHot:
                    {
                        orderText = " HOT STORE";
                        break;
                    }
                default:
                    {
                        orderText = string.Empty;
                        break;
                    }
            }

            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = orderText,
                Ord2 = orderText,
                OrderId = 0,
                Item = inventory.ItemDefinition.Item,
                Description = inventory.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                RequestedQuantity = pickedQty,
                StationId = _stationView.StationId, //  inventory.Location.StationId,
                Loc1 = inventory.Location.Loc1,
                Loc2 = inventory.Location.Loc2,
                Loc3 = inventory.Location.Loc3,
                Loc4 = inventory.Location.Loc4,
                Loc5 = inventory.Location.Loc5,
                Slot = inventory.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        //Hot Pick Action With Cost Center
        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, string costCenter)
        {
            var orderText = "  HOT PICK";
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = orderText,
                Ord2 = orderText,
                OrderId = 0,
                Item = inventory.ItemDefinition.Item,
                Description = inventory.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                RequestedQuantity = pickedQty,
                StationId = _stationView.StationId, //  inventory.Location.StationId,
                Loc1 = inventory.Location.Loc1,
                Loc2 = inventory.Location.Loc2,
                Loc3 = inventory.Location.Loc3,
                Loc4 = inventory.Location.Loc4,
                Loc5 = inventory.Location.Loc5,
                Slot = inventory.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = costCenter,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        // LocationCount
        public void SaveHistory(ActionCode actionCode, LocationCount cnt)
        {
            var inv = _inventoryRepository.GetInventoryViewById(cnt.InventoryId);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = cnt.CountDate,
                Ord1 = string.Empty,
                Ord2 = string.Empty,
                Item = inv.Item,
                Description = inv.Description,
                RequestedQuantity = cnt.PreviousQty,
                IssuedQuantity = cnt.NewQty,
                StationId = _stationView.StationId, //  inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                OrderId = 0,
                OrderDetailId = null,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);

        }

        public async Task SaveHistoryAsync(ActionCode actionCode, Location location)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Item = null,
                Description = null,
                StationId = _stationView.StationId, //  location.StationId,
                Loc1 = location.Loc1,
                Loc2 = location.Loc2,
                Loc3 = location.Loc3,
                Loc4 = location.Loc4,
                Loc5 = location.Loc5,
                Slot = location.Slot,

                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            await SaveAsync(history);
        }

        public void SaveHistory(ActionCode actionCode, Location location)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Item = null,
                Description = null,
                StationId = _stationView.StationId, //  location.StationId,
                Loc1 = location.Loc1,
                Loc2 = location.Loc2,
                Loc3 = location.Loc3,
                Loc4 = location.Loc4,
                Loc5 = location.Loc5,
                Slot = location.Slot,

                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        public async Task SaveHistoryAsync(ActionCode actionCode, ItemDefinition itemDefinition)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Item = itemDefinition.Item,
                Description = itemDefinition.Description,
                EmpId = GlobalVar.User.EmpId,
                StationId = _stationView.StationId, //  itemDefinition.StationId

            };
            await SaveAsync(history);
        }

        public void SaveHistory(ActionCode actionCode, ItemDefinition itemDefinition)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Item = itemDefinition.Item,
                Description = itemDefinition.Description,
                EmpId = GlobalVar.User.EmpId,
                StationId = _stationView.StationId, //  itemDefinition.StationId

            };
            Save(history);
        }

        private async Task SaveAsync(History history)
        {
            try
            {
                await _repoHistory.InsertAsync(history);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error, unable to save history record. " + ex.Message);
            }
        }

        private void Save(History history)
        {
            try
            {
                //if (history.ActionCode == 1 || history.ActionCode == 5)
                //{
                //    if (!CheckForExistingHistory(history))
                //    {
                //        _repoHistory.Insert(history);
                //    }
                //}
                //else
                //{
                    _repoHistory.Insert(history);
                //}
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("Error, unable to save history record. " + ex.Message);
            }
        }

        private bool CheckForExistingHistory(History history)
        {
            var his = _repoHistory.All().FirstOrDefault(r => r.ActionCode == history.ActionCode
                                                             && r.OrderId == history.OrderId
                                                             && r.Ord1 == history.Ord1
                                                             && r.Ord2 == history.Ord2
                                                             && r.OrderDetailId == history.OrderDetailId
                                                             && r.Item == history.Item
                                                             && r.RequestedQuantity == history.RequestedQuantity
                                                             && r.IssuedQuantity == history.IssuedQuantity
                                                             && r.Loc1 == history.Loc1
                                                             && r.Loc2 == history.Loc2
                                                             && r.Loc3 == history.Loc3
                                                             && r.Loc4 == history.Loc4
                                                             && r.OrderDetailInfo == history.OrderDetailInfo);
            if (his != null)
            {
                MessageBox.Show(
                    $"This message indicates that Neutron tried to write a duplicate record to the History table." +
                    $"This is not normal and the supervisor should be notified of what was happening or what may have caused this message. {Environment.NewLine} {Environment.NewLine}Click OK to continue picking normally.",
                    "Duplicate History Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        public void SaveHistory(ActionCode actionCode, ReplenOrderDetail value, int stationId)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = value.ReplenOrder.Ord1,
                Ord2 = value.ReplenOrder.Ord2,
                OrderId = value.ReplenOrder.Id,
                Item = value.PartNum,
                Description = value.PartDesc,
                RequestedQuantity = value.Quantity,
                IssuedQuantity = value.PickedQuantity,
                Slot = value.PrimeBin,
                EmpId = GlobalVar.User.EmpId,
                StationId = value.StationNumber,
                OrderDetailId = value.Id,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, ReplenOrder order, int stationId)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = order.Ord1,
                Ord2 = order.Ord2,
                OrderId = order.Id,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty,
                StationId = stationId
            };
            Save(history);
        }

        public List<HistoryView> GetHistoryRecordsByUser(string empId)
        {
            var today = DateTime.Now;
            var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
            var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
            var codes = GetCodes();
            var find = string.Empty;

            var history = new List<HistoryView>();
            using (var context = new NeutronDb())
            {
                var paramCodes = new SqlParameter("@Codes", codes);
                var paramFromDate = new SqlParameter("@FromDate", fromDate);
                var paramToDate = new SqlParameter("@ToDate", toDate);
                var paramFind = new SqlParameter("@Find", find);
                var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
                try
                {
                    var recs = context.History.Where(r => r.EmpId == empId).Take(10).ToList();
                    foreach (var rec in recs)
                    {
                        var historyView = new HistoryView
                        {
                            ActionCode = rec.ActionCode,
                            ActionCodeName = rec.ActionCodeName,
                            ActionDateTime = rec.ActionDateTime.ToShortDateString(),
                            CostCenter = rec.CostCenter,
                            Description = rec.Description,
                            EmpId = rec.EmpId,
                            Item = rec.Item
                        };
                        history.Add(historyView);
                    }

                    //var hist = context.Database.SqlQuery<HistoryView>("usp_GetHistory @Codes, @FromDate, @ToDate, @Find", parameters).ToList();
                    //if (hist.Any())
                    //{
                    //    history = hist.Where(h => h.EmpId == empId).OrderByDescending(o => o.ActionDateTime).Take(10).ToList();
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Connecting to SQL Server (USER).  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            return history;
        }

        public List<HistoryView> GetHistoryRecords()
        {
            var today = DateTime.Now;
            var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
            var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
            var codes = GetCodes();
            var find = string.Empty;

            var history = new List<HistoryView>();
            using (var context = new NeutronDb())
            {
                var paramCodes = new SqlParameter("@Codes", codes);
                var paramFromDate = new SqlParameter("@FromDate", fromDate);
                var paramToDate = new SqlParameter("@ToDate", toDate);
                var paramFind = new SqlParameter("@Find", find);
                var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
                try
                {
                    var hist = context.Database.SqlQuery<HistoryView>("usp_GetHistoryFind @Codes, @FromDate, @ToDate, @Find", parameters);
                    if (hist != null)
                    {
                        history = hist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            return history;
        }

        public List<HistoryView> GetHistoryRecords(string codes, DateTime fromDate, DateTime toDate, string find)
        {
            var history = new List<HistoryView>();
            using (var context = new NeutronDb())
            {
                var paramCodes = new SqlParameter("@Codes", codes);
                var paramFromDate = new SqlParameter("@FromDate", fromDate);
                var paramToDate = new SqlParameter("@ToDate", toDate);
                var paramFind = new SqlParameter("@Find", find);
                var parameters = new object[] { paramCodes, paramFromDate, paramToDate, paramFind };
                try
                {
                    var hist = context.Database.SqlQuery<HistoryView>("usp_GetHistoryFind @Codes, @FromDate, @ToDate, @Find", parameters);
                    if (hist != null)
                    {
                        history = hist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Connecting to SQL Server (ALL).  {ex.Message} \r\n {ex.InnerException}");
                }
            }
            return history;
        }

        private static string GetCodes()
        {
            var sb = new StringBuilder();
            var values = Enum.GetValues(typeof(ActionCode)).Cast<int>().Select(x => x.ToString()).ToArray();
            foreach (var item in values)
            {
                sb.Append(item + ",");
            }
            var result = sb.ToString().TrimEnd(',');

            return result;
        }

        public void SaveHistory(ActionCode actionCode, SkipView skipView)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = skipView.Ord1,
                Ord2 = skipView.Ord2,
                OrderId = skipView.OrderId,
                Item = skipView.Item,
                Description = skipView.Description,
                RequestedQuantity = skipView.Quantity,
                IssuedQuantity = skipView.Picked,
                Slot = "Skip",
                EmpId = GlobalVar.User.EmpId,
                StationId = _stationView.StationId, //  skipView.StationNumber,
                OrderDetailId = skipView.Id,
                CostCenter = skipView.OrderDetail.OrderDetailInfo.Length < 5 ? string.Empty : skipView.OrderDetail.OrderDetailInfo.Substring(0, 5),
                OrderInfo = skipView.OrderDetail.Order.OrderInfo,
                OrderDetailInfo = skipView.OrderDetail.OrderDetailInfo
            };
            Save(history);
        }

        //public void SaveActionCodesToDatabase()
        //{
        //    //Run this one time at startup
        //    //break down the ActionCode Enum into a List and save to the database.
        //    var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
        //        .Select(r => new ActionCodeItem { Id = (int)r, Name = r.GetEnumDescription() }).ToList();
        //    try
        //    {
        //        using (var db = new NeutronDb())
        //        {
        //            var exists = db.Database
        //                              .SqlQuery<int?>(@"
        //                 SELECT 1 FROM sys.tables AS T
        //                 INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
        //                 WHERE S.Name = 'dbo' AND T.Name = 'ActionCodeItems'")
        //                              .SingleOrDefault() != null;

        //            db.Database.ExecuteSqlCommand(!exists
        //                ? "CREATE TABLE [dbo].[ActionCodeItems] ([Id] [int] NOT NULL, Name varchar(64) not null)"
        //                : "TRUNCATE TABLE ActionCodeItems");

        //            foreach (var actionCode in actionCodes)
        //            {
        //                db.ActionCodeItems.Add(actionCode);
        //            }

        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Save Action Codes to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
        //                        $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
        //    }
        //    //set static variable to show the action codes have been created.
        //    _actionCodesInited = true;
        //}
    }
}
