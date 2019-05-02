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

namespace Neutron.Global
{

    public class HistoryManager
    {
        readonly GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        readonly InventoryRepository _repoInventory = new InventoryRepository();

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
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = order.OrderInfo,
                OrderDetailInfo = string.Empty
            };
            Save(history);

        }

        public void SaveHistory(ActionCode actionCode, PickStop pickStop)
        {
            foreach (PickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    var cCenter = "          ";
                    if (pickView.OrderDetail.OrderDetailInfo.Length >= 10)
                    {
                        cCenter = pickView.OrderDetail.OrderDetailInfo.Substring(0, 10);
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
                        StationId = pickLocation.Inventory.Location.Station.StationNumber,
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
            foreach (ReplenPickView pickView in pickStop.PickViews)
            {
                foreach (PickLocation pickLocation in pickView.PickLocations)
                {
                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionCodeName = EnumExtensions.GetEnumDescription(actionCode),
                        ActionDateTime = pickLocation.PickDate,
                        Ord1 = pickView.Ord1,
                        Ord2 = pickView.Ord2,
                        Item = pickView.Item,
                        Description = pickView.Description,
                        RequestedQuantity = pickView.Quantity,
                        IssuedQuantity = pickLocation.Quantity,
                        StationId = pickLocation.Inventory.Location.StationId,
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
        // Off Carousel Complete
        public void SaveHistory(ActionCode actionCode, OrderDetail value)
        {
            //var cCenter = "          ";
            //var orderDetailInfo = string.Empty;


            //if (!string.IsNullOrEmpty(value.OrderDetailInfo))
            //{
            //    var info = value.OrderDetailInfo.Trim();
            //    if (info.EndsWith("261") || info.Length == 24)
            //    {
            //        orderDetailInfo = cCenter + info;
            //    }

            //    if (info.Length == 36)
            //    {
            //        cCenter = info.Substring(0, 10);
            //        orderDetailInfo = info;
            //    }
            //}

            var cCenter = "          ";
            var orderDetailInfo = string.Empty;
            var info = value.OrderDetailInfo;

            if (!string.IsNullOrEmpty(info))
            {
                if (info.EndsWith("261") || info.Length == 24)
                {
                    orderDetailInfo = $"{info}";
                }

                if (info.Length == 36)
                {
                    cCenter = info.Substring(0, 10);
                    orderDetailInfo = info;
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
                StationId = value.StationNumber,
                OrderDetailId = value.Id,
                CostCenter = cCenter,
                OrderInfo = value.Order.OrderInfo,
                OrderDetailInfo = orderDetailInfo
            };
            Save(history);
        }

        public void SaveHistory(ActionCode actionCode, Inventory inventory)
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                // Ord1 = pickStop.Ord1,
                // Ord2 = pickStop.Ord2,
                //OrderId = pickStop.OrderId,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = inv.Quantity,
                //RequestedQuantity = inv.Quantity,
                StationId = inv.Location.StationId,
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

                if (info.Length == 36)
                {
                    cCenter = info.Substring(0, 10);
                    orderDetailInfo = info;
                }
            }

            var inv = _repoInventory.GetInventoryViewById(inventory.Id);
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
                StationId = inv.Location.StationId,
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

        //Hot Action
        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty)
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                // Ord1 = pickView.Ord1,
                // Ord2 = pickView.Ord2,
                // OrderId = pickView.OrderId,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                // RequestedQuantity = pickView.Quantity,
                StationId = inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                // OrderInfo = string.Empty,
                // OrderDetailInfo = string.Empty
            };
            Save(history);
        }


        //Hot Action with Cost Center
        public void SaveHistory(ActionCode actionCode, Inventory inventory, int pickedQty, string costCenter)
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = "HOTPICK",
                Ord2 = "HOTPICK",
                //OrderId = pickStop.OrderId,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = pickedQty,
                RequestedQuantity = pickedQty,
                StationId = inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                EmpId = GlobalVar.User.EmpId,
                CostCenter = costCenter,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }


        public void SaveHistory(ActionCode actionCode, LocationCount cnt)
        {
            InventoryView inv = _repoInventory.GetInventoryViewById(cnt.InventoryId);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = cnt.CountDate,
                Item = inv.Item,
                Description = inv.Description,
                RequestedQuantity = cnt.PreviousQty,
                IssuedQuantity = cnt.NewQty,
                StationId = inv.Location.StationId,
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

        public async Task SaveHistoryAsync(ActionCode actionCode, Location location)
        {
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Item = null,
                Description = null,
                StationId = location.StationId,
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
                StationId = location.StationId,
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
                StationId = itemDefinition.StationId

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
                StationId = itemDefinition.StationId

            };
            Save(history);
        }

        private async Task SaveAsync(History history)
        {
            try
            {
                await Task.Run(() => _repoHistory.Insert(history));
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
                _repoHistory.Insert(history);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error, unable to save history record. " + ex.Message);
            }
        }

        public void SaveHistory(ActionCode actionCode, ReplenOrderDetail value)
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
                EmpId = GlobalVar.User.EmpId,
                CostCenter = string.Empty,
                OrderInfo = string.Empty,
                OrderDetailInfo = string.Empty
            };
            Save(history);
        }

        public List<HistoryView> GetHistoryRecordsByUser(string empId)
        {
            DateTime today = DateTime.Now;
            var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
            var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
            string codes = GetCodes();
            string find = string.Empty;

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
                        history = hist.Where(h => h.EmpId == empId).OrderByDescending(o => o.ActionDateTime).ToList();
                    }
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
            DateTime today = DateTime.Now;
            var fromDate = new DateTime(2015, 1, 1, 23, 59, 59, 999);
            var toDate = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59, 999);
            string codes = GetCodes();
            string find = string.Empty;

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
            string[] values = Enum.GetValues(typeof(ActionCode)).Cast<int>().Select(x => x.ToString()).ToArray();
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
                StationId = skipView.StationNumber,
                OrderDetailId = skipView.Id,
                CostCenter = skipView.OrderDetail.OrderDetailInfo.Substring(0, 5),
                OrderInfo = skipView.OrderDetail.Order.OrderInfo,
                OrderDetailInfo = skipView.OrderDetail.OrderDetailInfo
            };
            Save(history);
        }
    }
}
