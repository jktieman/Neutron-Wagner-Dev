using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.ModelViews;
using NeutronCore.Enums;
using System.Data.SqlClient;
using NeutronCore.Extensions;
using NeutronData.BaseClasses;

namespace Neutron.Global
{

    public class HistoryManager
    {
        private readonly GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        private readonly InventoryRepository _repoInventory = new InventoryRepository();
        private static bool _actionCodesInited = false;

        public HistoryManager()
        {
            if (_actionCodesInited) return;
            SaveActionCodesToDatabase();
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
                EmpId = GlobalVar.User.EmpId,
                OrderInfo = order.OrderInfo,
                OrderDetailInfo = string.Empty
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
            foreach (var pickView in pickStop.PickViews)
            {
                foreach (var pickLocation in pickView.PickLocations)
                {
                    var history = new History
                    {
                        ActionCode = (int)actionCode,
                        ActionCodeName = actionCode.GetEnumDescription(),
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
        //Inventory Modify
        public void SaveHistory(ActionCode actionCode, Inventory inventory)
        {
            var inv = _repoInventory.GetInventoryViewById(inventory.Id);
            var history = new History
            {
                ActionCode = (int)actionCode,
                ActionCodeName = actionCode.GetEnumDescription(),
                ActionDateTime = DateTime.Now,
                Ord1 = string.Empty,
                Ord2 = string.Empty,
                OrderId = null,
                OrderDetailId = null,
                Item = inv.ItemDefinition.Item,
                Description = inv.ItemDefinition.Description,
                IssuedQuantity = inv.Quantity,
                RequestedQuantity = 0,
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
                StationId = inventory.Location.StationId,
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
                StationId = inventory.Location.StationId,
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
            var inv = _repoInventory.GetInventoryViewById(cnt.InventoryId);
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
                StationId = inv.Location.StationId,
                Loc1 = inv.Location.Loc1,
                Loc2 = inv.Location.Loc2,
                Loc3 = inv.Location.Loc3,
                Loc4 = inv.Location.Loc4,
                Loc5 = inv.Location.Loc5,
                Slot = inv.Location.Slot,
                OrderId = null,
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
                StationId = skipView.StationNumber,
                OrderDetailId = skipView.Id,
                CostCenter = skipView.OrderDetail.OrderDetailInfo?.Substring(0, 5),
                OrderInfo = skipView.OrderDetail.Order.OrderInfo,
                OrderDetailInfo = skipView.OrderDetail.OrderDetailInfo
            };
            Save(history);
        }

        public void SaveActionCodesToDatabase()
        {
            //Run this one time at startup
            //break down the ActionCode Enum into a List and save to the database.

            var actionCodes = ((ActionCode[])Enum.GetValues(typeof(ActionCode)))
                .Select(r => new ActionCodeItem { Id = (int)r, Name = r.GetEnumDescription() }).ToList();

            try
            {
                using (var db = new NeutronDb())
                {
                    var exists = db.Database
                                      .SqlQuery<int?>(@"
                         SELECT 1 FROM sys.tables AS T
                         INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                         WHERE S.Name = 'dbo' AND T.Name = 'ActionCodeItems'")
                                      .SingleOrDefault() != null;

                    if (!exists)
                    {
                        db.Database.ExecuteSqlCommand("CREATE TABLE [dbo].[ActionCodeItems] ([Id] [int] NOT NULL, Name varchar(64) not null)");

                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("TRUNCATE TABLE ActionCodeItems");
                    }

                    foreach (var actionCode in actionCodes)
                    {
                        db.ActionCodeItems.Add(actionCode);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save Action Codes to Database Failed.  {Environment.NewLine} {ex.Message}{Environment.NewLine}" +
                                $"{ex.InnerException}{Environment.NewLine} {ex.StackTrace}");
            }

            //set static variable to show the action codes have been created.
            _actionCodesInited = true;
        }
    }
}
