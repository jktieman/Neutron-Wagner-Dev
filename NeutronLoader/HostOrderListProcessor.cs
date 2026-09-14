using AlliedLogger;
using NeutronCore.Extensions;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using JsonManager;
using NeutronData.Models.Lookups;

namespace NeutronLoader
{
    public class HostOrderListProcessor
    {
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        private readonly GenericRepository<Location> _repoLocation;
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly GenericRepository<Order> _repoOrder;
        private readonly GenericRepository<OrderDetail> _repoOrderDetail;
        private readonly GenericRepository<SizeCode> _repoSizeCode;
        private readonly GenericRepository<VelocityCode> _repoVelocityCode;
        private readonly GenericRepository<HeightCode> _repoHeightCode;
        private readonly GenericRepository<UnitOfIssue> _repoUnitOfIssue;

        private IDynamicLogger _logger;
        private readonly Func<NeutronDb> _contextFactory;
        private readonly IJsonData _jsonData;

        public HostOrderListProcessor(List<HostOrder> hostOrderlist, IJsonData jsonData, IDynamicLogger logger, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            
            _jsonData = jsonData;
            _logger = logger;
            _contextFactory = contextFactory;
            _repoItemDefinition = new GenericRepository<ItemDefinition>(contextFactory);
            _repoLocation = new GenericRepository<Location>(contextFactory);
            _repoInventory = new GenericRepository<Inventory>(contextFactory);
            _repoOrder = new GenericRepository<Order>(contextFactory);
            _repoOrderDetail = new GenericRepository<OrderDetail>(contextFactory);
            _repoSizeCode = new GenericRepository<SizeCode>(contextFactory);
            _repoVelocityCode = new GenericRepository<VelocityCode>(contextFactory);
            _repoHeightCode = new GenericRepository<HeightCode>(contextFactory);
            _repoUnitOfIssue = new GenericRepository<UnitOfIssue>(contextFactory);

            if (hostOrderlist != null) ProcessHostOrderList(hostOrderlist);
        }

        private void ProcessHostOrderList(List<HostOrder> hostOrderlist)
        {
            var hostControlsInventoryLocation = true;
            if (hostControlsInventoryLocation)
            {
                var counter = 0;
                foreach (var hostOrder in hostOrderlist)
                {
                    counter++;
                 _ = _logger.LogDetailAsync($"Update Inventory: Line: {counter}  hostOrder: Job: {hostOrder.JobNum}  Part: {hostOrder.PartNum}  Bin: {hostOrder.PrimeBin}");
                    UpdateInventory(hostOrder);
                }
            }

            foreach (var hostOrder in hostOrderlist)
            {
                InsertOrUpdateOrder(hostOrder);
                InsertOrUpdateOrderDetail(hostOrder);
            }
            RemoveOldOrders(hostOrderlist);
        }

        private void UpdateInventory(HostOrder hostOrder)
        {
            try
            {
                //has to have a primebin we can work with
                if (hostOrder.PrimeBin.Length == 5)
                {
                 _ = _logger.LogDetailAsync($"Start Inventory Update. PrimeBin: {hostOrder.PrimeBin}");
                    //Get the Location and Delete it
                    var loc1 = Convert.ToInt32(hostOrder.PrimeBin.Substring(1, 1));
                    var loc2 = Convert.ToInt32(hostOrder.PrimeBin.Substring(2, 3));
                    var areaId = hostOrder.OrderDetail.AreaId;
                    var sizeCodeId = _repoSizeCode.All().FirstOrDefault().Id;
                    var velocityCodeId = _repoVelocityCode.All().FirstOrDefault().Id;
                    var heightCodeId = _repoHeightCode.All().FirstOrDefault().Id;
                    var unitOfIssue = _repoUnitOfIssue.All().FirstOrDefault().Id;

                 _ = _logger.LogDetailAsync($"Area Id: {areaId}");
                    var location = _repoLocation.FindBy(r => r.Loc1 == loc1 && r.Loc2 == loc2).FirstOrDefault();
                    if (location == null)
                    {
                        var loc = new Location()
                        {
                            AreaId = areaId,
                            Loc1 = loc1,
                            Loc2 = loc2,
                            Loc3 = 1,
                            Loc4 = 1,
                            Loc5 = 1,
                            Slot = hostOrder.PrimeBin,
                            SizeCodeId = sizeCodeId,
                            VelocityCodeId = velocityCodeId,
                            HeightCodeId = heightCodeId,
                            LocationCode = string.Empty,
                            InUse = true
                        };
                     _ = _logger.LogDetailAsync($"Before Inserting new Location Record.{loc.Loc1}-{loc.Loc2}-{loc.Loc3}-{loc.Loc4}-{loc.Loc5}-{loc.Slot}-{loc.SizeCodeId}-{loc.VelocityCodeId}-{loc.HeightCodeId}-{loc.LocationCode}-{loc.InUse}");
                        _repoLocation.Insert(loc);
                     _ = _logger.LogDetailAsync($"After Inserting new Location Record.{loc.Loc1}-{loc.Loc2}-{loc.Loc3}-{loc.Loc4}-{loc.Loc5}-{loc.Slot}-{loc.SizeCodeId}-{loc.VelocityCodeId}-{loc.HeightCodeId}-{loc.LocationCode}-{loc.InUse}");
                        location = loc;

                    }
                    else
                    {
                     _ = _logger.LogDetailAsync($"Location is NOT null.  LocationId: {location.Id}");
                    }

                    var itemDefinition = _repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                    if (itemDefinition == null)
                    {
                        var itemDef = new ItemDefinition()
                        {
                            AreaId = areaId,
                            Item = hostOrder.PartNum,
                            Description = hostOrder.PartDesc,
                            LocationMax = 0,
                            LocationMin = 0,
                            SystemMax = 0,
                            SystemMin = 0,
                            SizeCodeId = sizeCodeId,
                            VelocityCodeId = velocityCodeId,
                            HeightCodeId = heightCodeId,
                            StorageTypeId = 1,
                            UnitOfIssueId = unitOfIssue,
                            Weight = 0,
                            Scale = false
                        };
                     _ = _logger.LogDetailAsync($"Inserting new Item Definition Record. Station Id:  {itemDef.AreaId}");
                        _repoItemDefinition.Insert(itemDef);
                        itemDefinition = itemDef;
                    }

                    else
                    {
                     _ = _logger.LogDetailAsync($"Item Definition is NOT null.  ItemDefinitionId: {itemDefinition.Id}");
                    }


                    var inventoryRecs = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDefinition.Id).ToList();
                    var inventoryItemExists = false;
                    if (inventoryRecs.Count > 0)
                    {
                        foreach (var inv in inventoryRecs)
                        {
                            if (inv.LocationId == location.Id)
                            {
                             _ = _logger.LogDetailAsync($"Loop Inventory Record Exists.  InventoryId: {inv.Id}");
                                inventoryItemExists = true;
                            }
                            else
                            {
                             _ = _logger.LogDetailAsync($"Inventory Record to be Deleted.  InventoryId: {inv.Id}");
                                _repoInventory.Delete(inv.Id);
                            }
                        }
                    }

                    if (inventoryItemExists) return;
                    // create a new inventory item
                 _ = _logger.LogDetailAsync($"Inventory Record DOESN'T Exists. ");
                    var inventory = new Inventory()
                    {
                        ItemDefinitionId = itemDefinition.Id,
                        LocationId = location.Id,
                        Quantity = 999999,
                        StorageTypeId = 1,
                        ReceivedDate = DateTime.Now,
                        PrimeBin = true,
                        AreaId = areaId
                    };
                 _ = _logger.LogDetailAsync($"Inserting new Inventory Record. Area Id:  {inventory.AreaId}");
                    _repoInventory.Insert(inventory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Trouble creating Inventory Item during Order Load Event. {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void InsertOrUpdateOrderDetail(HostOrder hostOrder)
        {
            try
            {
                var orderDetail = _repoOrderDetail.FindBy(r => r.JobNum == hostOrder.JobNum
                    && r.ItemDefinition.Item == hostOrder.PartNum).FirstOrDefault();
                if (orderDetail != null)
                {
                    //update orderDetail
                    orderDetail.PrimeBin = hostOrder.PrimeBin;
                    orderDetail.Quantity = hostOrder.Qty.ParseInt();
                    orderDetail.Qty = hostOrder.Qty;
                    orderDetail.OrderDetailInfo = hostOrder.NewBin;
                    try
                    {
                        _repoOrderDetail.Update(orderDetail);
                    }
                    catch (Exception ex)
                    {
                     _ = _logger.LogDetailAsync($"Error Updating Order Detail.  {ex.Message} \r\n {ex.InnerException}");
                    }
                }
                else
                {
                    try
                    {
                        var order = _repoOrder.FindBy(r => r.Ord1 == hostOrder.JobNum).FirstOrDefault();

                        //make sure the ItemDefinition and Location are defined
                        if (order != null)
                        {
                            var itemDef = new ItemDefinitionProcessor(_jsonData,_contextFactory ).GetOrCreate(hostOrder);
                            var location = new LocationProcessor(_jsonData, _contextFactory).GetOrCreate(hostOrder);

                            if (itemDef != null && location != null)
                            {
                                var ord = new OrderDetail
                                {
                                    OrderId = order.Id,
                                    ItemDefinitionId = itemDef.Id,
                                    Quantity = hostOrder.Qty.ParseInt(),
                                    PickedQuantity = 0,
                                    DateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                                    EmpId = string.Empty,
                                    JobNum = hostOrder.JobNum,
                                    NewBin = hostOrder.NewBin,
                                    PartDesc = hostOrder.PartDesc,
                                    PartNum = hostOrder.PartNum,
                                    PrimeBin = hostOrder.PrimeBin,
                                    Qty = hostOrder.Qty,
                                    TroubleBit = @"0",
                                    TypeCode = @"2",
                                    LineStatusId = 1,
                                    AreaId = location.AreaId,
                                    OrderDetailInfo = hostOrder.NewBin
                                };
                                try
                                {
                                    _repoOrderDetail.Insert(ord);
                                }
                                catch (Exception ex)
                                {
                                 _ = _logger.LogDetailAsync($"Error Inserting Order Detail.  {ex.Message} \r\n {ex.InnerException}");
                                }
                            }
                            else
                            {
                             _ = _logger.LogDetailAsync($"Invalid Location or Item Definition Record.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                     _ = _logger.LogDetailAsync(
                            $"Error Finding Item Definition - {hostOrder.PartNum}.  {ex.Message} \r\n {ex.InnerException}");
                    }
                }
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Error Finding Existing Order Detail.  {ex.Message} \r\n {ex.InnerException}");
            }
        }

        private bool InsertOrUpdateOrder(HostOrder hostOrder)
        {
            var result = false;
            try
            {
                var order = _repoOrder.FindBy(r => r.Ord1 == hostOrder.JobNum).FirstOrDefault();
                if (order == null)
                {
                    // insert
                    var ord = new Order
                    {
                        Ord1 = hostOrder.JobNum,
                        Ord2 = string.Empty,
                        Priority = 99,
                        LoadDate = DateTime.Now,
                        OrderStatusId = 1,
                        ShipperId = 1,
                        ShipMethodId = 1,
                        OrderInfo = string.Empty
                    };

                    try
                    {
                        _repoOrder.Insert(ord);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                     _ = _logger.LogDetailAsync($"Error Inserting Order.  {ex.Message} \r\n {ex.InnerException}");
                    }
                }
                else
                {
                    result = true;
                    // record exist.  Do Nothing now.
                }
            }
            catch (Exception ex)
            {
             _ = _logger.LogDetailAsync($"Error Finding Existing Order.  {ex.Message} \r\n {ex.InnerException}");
            }
            return result;
        }

        private void RemoveOldOrders(List<HostOrder> hostOrderlist)
        {
            var orders = _repoOrder.All(r => r.OrderStatusId == 1).ToList();
            if (orders.Count == 0) return;
            foreach (var rec in orders)
            {
                var hostOrd = hostOrderlist.FirstOrDefault(f => f.JobNum == rec.Ord1);
                if (hostOrd != null) continue;
                DeleteDetailRecords(rec.Id);
                _repoOrder.Delete(rec.Id);
            }
        }

        private void DeleteDetailRecords(int id)
        {
            var recs = _repoOrderDetail.All(r => r.OrderId == id).ToList();
            foreach (var r in recs)
            {
                _repoOrderDetail.Delete(r.Id);
            }
        }
    }
}
