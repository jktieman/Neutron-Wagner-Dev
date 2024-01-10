using AlliedLogger;
using JsonManager;
using Microsoft.VisualBasic.FileIO;
using NeutronCore;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NeutronLoader
{
    internal class VidirFileProcessor
    {
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> _repoLocation = new GenericRepository<Location>(new NeutronDb());

        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IDynamicLogger _logger;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;

        public VidirFileProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense
            , IJsonData jsonData, WorkstationView workstationView)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _logger = NeutronCore.Global.Logger.SetupLogger("FileProcessor");
            _jsonData = jsonData;
            _workstationView = workstationView;
        }

        public void LoadFile(FileInfo fileInfo)
        {
            var orders = new List<HostOrder>();
            var filename = fileInfo.FullName;
            if (File.Exists(filename))
            {
                orders = ProcessInterfaceFile(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);
                ArchiveFile.Archive(fileInfo, _logger);
            }
            else
            {
                ShowFileLockMessages(fileInfo.FileLockFailure());
            }

            if (orders.Any())
            {
                ProcessOrdersToNeutron(orders);
            }
        }

        public void LoadFiles(List<FileInfo> files)
        {
            var orders = new List<HostOrder>();
            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in files)
            {
                var filename = fileInfo.FullName;
                if (File.Exists(filename))
                {
                    //accumulate all orders into one list of HostOrders
                    var tempOrders = ProcessInterfaceFile(fileInfo);

                    foreach (var ord in tempOrders)
                    {
                        orders.Add(ord);
                    }

                    Thread.Sleep(millisecondsTimeout: 100);
                    ArchiveFile.Archive(fileInfo, _logger);
                }
                else
                {
                    ShowFileLockMessages(fileInfo.FileLockFailure());
                }
            }
            if (orders.Any())
            {
                ProcessOrdersToNeutron(orders);
            }
        }

        public List<HostOrder> ProcessInterfaceFile(FileInfo fileInfo)
        {
            var hostOrderList = new List<HostOrder>();
            try
            {
                using (var parser = new TextFieldParser(fileInfo.FullName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiters: new string[] { _neutronVariables.FieldDelimiter });
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        if (fields != null && fields.Length == 10)
                        {
                            var hostOrder = new HostOrder()
                            {
                                TypeCode = fields[0].ToString(),
                                PartNum = fields[1].ToString(),
                                PartDesc = fields[2].ToString(),
                                JobNum = fields[3].ToString(),
                                PrimeBin = fields[4].ToString(),
                                NewBin = fields[5].ToString(),
                                Qty = fields[6].ToString(),
                                TroubleBit = fields[7].ToString(),
                                DateTime = fields[8].ToString(),
                                EmpId = fields[9].ToString(),
                            };
                            hostOrderList.Add(hostOrder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Process Interface File Error.{Environment.NewLine}{ex.Message}{Environment.NewLine}" +
                            $"{ex.InnerException?.Message}{Environment.NewLine}{ex.InnerException?.InnerException?.Message}");
            }
            return hostOrderList;
        }

        public void ProcessOrdersToNeutron(IList<HostOrder> hostOrderLines)
        {
            ProcessType2(hostOrderLines);

            ProcessType3(hostOrderLines);

            ProcessType4(hostOrderLines);
        }

        private void ProcessType2(IList<HostOrder> hostOrderLines)
        {
            var orders = hostOrderLines.Where(s => s.TypeCode == "2").ToList();
            if (orders.Count > 0)
            {
                Put(orders);
            }
        }

        private void ProcessType3(IList<HostOrder> hostOrderLines)
        {
            var orders = hostOrderLines.Where(s => s.TypeCode == "3").ToList();
            if (orders.Count > 0)
            {
                Store(orders);
            }
        }

        private void ProcessType4(IList<HostOrder> hostOrderLines)
        {
            var orders = hostOrderLines.Where(s => s.TypeCode == "4").ToList();
            if (orders.Count > 0)
            {
                InventoryAdjust(orders);
            }
        }

        private void InventoryAdjust(List<HostOrder> hostOrderLines)
        {
            try
            {
                foreach (var hostOrder in hostOrderLines)
                {
                    var inventoryItem = _repoInventory.FindByInclude(r => r.ItemDefinition.Item == hostOrder.PartNum
                               && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                    if (inventoryItem != null)
                    {
                        _ = _logger.LogDetailAsync($"216 Inventory Adjust From {inventoryItem.Quantity} To {hostOrder.Qty}");
                        inventoryItem.Quantity = (hostOrder.Qty).ParseInt();
                        _repoInventory.Update(inventoryItem);
                    }
                    else
                    {
                        hostOrder.TroubleBit = "1";
                        hostOrder.EmpId = ($"EmpId:--- Note: Item Not Found At That Location");
                        
                        var hostFile = new HostFile(_neutronLicense, _neutronVariables, _workstationView);
                        hostFile.CreateHostFile(hostOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"231 Inventory Adjust Error:  {ex.Message} \r\n  {ex.InnerException}");
            }
        }

        private void Store(List<HostOrder> hostOrderLines)
        {
            var orderHasRecords = false;
            var distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct().ToList();
            var orderId = 0;

            foreach (var item in distinctOrders)
            {
                var linesInOrder = hostOrderLines.Where(h => h.JobNum == item).ToList();
                if (linesInOrder.Count > 0)
                {
                    for (var i = 0; i < linesInOrder.Count(); i++)
                    {
                        if (i == 0)
                        {
                            {
                                orderId = CreateReplenOrderRecord(linesInOrder[0]);
                            }
                        }
                        var hostOrder = linesInOrder[i];

                        var storeItemDefinition = _repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                        if (storeItemDefinition != null)
                        {
                            var rec = _repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                            if (rec != null)
                            {
                                var replenOrderDetail = new ReplenOrderDetail
                                {
                                    ReplenOrderId = orderId,
                                    ItemDefinitionId = storeItemDefinition.Id,
                                    Quantity = (hostOrder.Qty).ParseInt(),
                                    DateTime = hostOrder.DateTime,
                                    EmpId = hostOrder.EmpId,
                                    JobNum = hostOrder.JobNum,
                                    NewBin = hostOrder.NewBin,
                                    PartDesc = hostOrder.PartDesc,
                                    PartNum = hostOrder.PartNum,
                                    PrimeBin = hostOrder.PrimeBin,
                                    Qty = hostOrder.Qty,
                                    TroubleBit = hostOrder.TroubleBit,
                                    TypeCode = hostOrder.TypeCode,
                                    LineStatusId = 1,
                                    AreaId = rec.AreaId
                                };
                                try
                                {
                                    _repoReplenOrderDetail.Insert(replenOrderDetail);
                                    orderHasRecords = true;
                                }
                                catch (Exception ex)
                                {
                                    _ = _logger.LogDetailAsync("267 Insert Store Item Error " + ex.Message);
                                }
                            }
                            else
                            {
                                var location = _repoLocation.FindBy(r => r.Slot == hostOrder.PrimeBin).FirstOrDefault();
                                if (location != null)
                                {
                                    var result = CreateInventoryRecord(storeItemDefinition, location);

                                    if (result)
                                    {
                                        var replenOrderDetail = new ReplenOrderDetail
                                        {
                                            ReplenOrderId = orderId,
                                            ItemDefinitionId = storeItemDefinition.Id,
                                            Quantity = (hostOrder.Qty).ParseInt(),
                                            DateTime = hostOrder.DateTime,
                                            EmpId = hostOrder.EmpId,
                                            JobNum = hostOrder.JobNum,
                                            NewBin = hostOrder.NewBin,
                                            PartDesc = hostOrder.PartDesc,
                                            PartNum = hostOrder.PartNum,
                                            PrimeBin = location.Slot,
                                            Qty = hostOrder.Qty,
                                            TroubleBit = hostOrder.TroubleBit,
                                            TypeCode = hostOrder.TypeCode,
                                            LineStatusId = 1,
                                            AreaId = location.AreaId
                                        };
                                        try
                                        {
                                            _repoReplenOrderDetail.Insert(replenOrderDetail);
                                            orderHasRecords = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            _ = _logger.LogDetailAsync("300 Insert Store Item in new location Error " + ex.Message);
                                        }
                                    }
                                }
                                else   //location is null
                                {
                                    _ = _logger.LogDetailAsync($"{hostOrder.PrimeBin} is not set up in Locations. ");
                                    hostOrder.TroubleBit = "1";
                                    hostOrder.EmpId = ($"EmpId:--- Note: Location is not set up in Neutron");
                                    var hostFile = new HostFile(_neutronLicense, _neutronVariables, _workstationView);
                                    hostFile.CreateHostFile(hostOrder);
                                }
                            }
                        }
                        else
                        {
                            _ = _logger.LogDetailAsync($"{hostOrder.PartNum} is not set up in the System.");
                            hostOrder.TroubleBit = "1";
                            hostOrder.EmpId = ($"EmpId:--- Note: Item Not Defined in Shuttle");
                            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _workstationView);
                            hostFile.CreateHostFile(hostOrder);
                        }
                    }
                }
            }
            if (!orderHasRecords)
            {
                _ = _logger.LogDetailAsync($"Order has no records. Deleting Order: {orderId}");
                DeleteReplenOrderRecord(orderId);
            }
        }

        private bool CreateInventoryRecord(ItemDefinition storeItemDefinition, Location location)
        {
            var result = false;
            var inventory = new Inventory
            {
                ItemDefinitionId = storeItemDefinition.Id,
                LocationId = location.Id,
                Quantity = 0,
                ReceivedDate = DateTime.Now,
                PrimeBin = true,
                AreaId = location.AreaId,
                StorageTypeId = 1
            };
            try
            {
                _repoInventory.Insert(inventory);
                result = true;
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"291 Error Inserting new Inventory record.  {ex.Message} \r\n {ex.InnerException}");
            }

            return result;
        }

        private int CreateReplenOrderRecord(HostOrder hostOrder)
        {
            var firstRec = hostOrder;
            var order = new ReplenOrder();
            if (firstRec != null)
            {
                order = new ReplenOrder()
                {
                    Ord1 = firstRec.PartNum, 
                    Ord2 = firstRec.PartDesc, 
                    Priority = GetTrayNumber(firstRec.PrimeBin),
                    LoadDate = DateTime.Now,
                   // ShipperId = 1,
                   // ShipMethodId = 1,
                    OrderStatusId = 1
                };
                try
                {
                    _repoReplenOrder.Insert(order);
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"Create Replen Order Error. OrderId: {hostOrder.Id}) " + ex.Message);
                }

            }
            return order.Id;
        }

        private int GetTrayNumber(string primeBin)
        {
            var result = 99;
            if (_neutronLicense.CompanyCode == "VID")
            {
                result = int.Parse(primeBin.Substring(2, 2));
            }
            return result;
        }

        private void DeleteReplenOrderRecord(int id)
        {
            try
            {
                _repoReplenOrder.Delete(id);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Delete Replen Order Error.  Order Id: {id} ." + ex.Message);
            }
        }

        public void Put(List<HostOrder> hostOrderLines)
        {

            var distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct();
            var hostFile = new HostFile(_neutronLicense, _neutronVariables, _workstationView);

            foreach (var item in distinctOrders)
            {
                var empId = string.Empty;
                var orderId = 0;
                var hostOrder = new HostOrder();
                var linesInOrder = hostOrderLines.Where(h => h.JobNum == item).ToList();
                if (linesInOrder.Count > 0)
                {
                    for (var i = 0; i < linesInOrder.Count(); i++)
                    {
                        if (i == 0)
                        {
                            orderId = CreateOrderRecord(linesInOrder[0]);
                        }

                        hostOrder = linesInOrder[i];
                        empId = hostOrder.EmpId;
                        //var pickUsedItem = false;
                        //if (_neutronVariables.CheckForUsedItem)
                        //{
                         //  pickUsedItem = PickUsedItem(orderId, hostOrder); //true if used inventory found
                        //}
                       // if (!pickUsedItem)
                       //{
                            _ = _logger.LogDetailAsync($"444  Start New Item Load. ");
                            var newItemExists = NewItemExists(hostOrder.PartNum);
                            if (newItemExists != null)
                            {
                                var pickNewItemWithPrimeBin = PickNewItemWithPrimeBin(newItemExists, orderId, hostOrder);
                                if (!pickNewItemWithPrimeBin)
                                {
                                    hostOrder.TroubleBit = "1";
                                    hostOrder.EmpId = ($"EmpId: {empId} Note: Item Defined, but Not In Prime Bin");
                                    
                                    hostFile.CreateHostFile(hostOrder);

                                    var pickNewItemWithAlternateLocation = PickNewItemWithAlternateLocation(newItemExists, orderId, hostOrder);
                                    if (!pickNewItemWithAlternateLocation)
                                    {
                                        _ = _logger.LogDetailAsync($"457 Item Definition found but Inventory Not found anywhere");
                                        hostOrder.TroubleBit = "1";  //RTS
                                        hostOrder.EmpId = ($"EmpId:{empId} Note: Item Definition found, but Inventory Not found anywhere");

                                        hostFile.CreateHostFile(hostOrder);
                                        hostOrder.TypeCode = "1";
                                        hostFile.CreateHostFile(hostOrder);

                                        var sb = new StringBuilder();
                                        sb.AppendLine($"Job Number: {hostOrder.JobNum}");
                                        sb.AppendLine($"Part Number: {hostOrder.PartNum} ");
                                        sb.AppendLine($"Description: {hostOrder.PartDesc}");
                                        sb.AppendLine($"The Item is defined in Neutron, but there is no inventory.");
                                        sb.AppendLine($"A Return To Stock request for this item has been sent to Epicor.");
                                        MessageBox.Show(sb.ToString(), "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                            else
                            {
                                _ = _logger.LogDetailAsync($"378 Item Definition Not Found");
                                hostOrder.TroubleBit = "1";  //RTS
                                hostOrder.EmpId = ($"EmpId:{empId} Note: Item Definition Not Found");
                                hostFile.CreateHostFile(hostOrder);
                                hostOrder.TypeCode = "1";
                                hostFile.CreateHostFile(hostOrder);

                                var sb = new StringBuilder();
                                sb.AppendLine($"Job Number: {hostOrder.JobNum}");
                                sb.AppendLine($"Part Number: {hostOrder.PartNum} ");
                                sb.AppendLine($"Description: {hostOrder.PartDesc}");
                                sb.AppendLine($"The Item is has not been defined in Neutron.");
                                sb.AppendLine($"A Return To Stock request for this item has been sent to Epicor.");
                                MessageBox.Show(sb.ToString(), "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                       // }
                    }
                }
                try
                {
                    var count = _repoOrderDetail.FindBy(r => r.OrderId == orderId).Count();
                    if (count <= 0)
                    {
                        _ = _logger.LogDetailAsync($" 497 Order ID: {orderId}  Order Number: {hostOrder.JobNum}  Part Number: {hostOrder.PartNum} Deleted.");
                        _repoOrder.Delete(orderId);
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"415 Unable to delete Empty Order Id: {orderId}. \r\n {ex.Message} \r\n {ex.InnerException}");
                }
            }
        }


        private bool PickNewItemWithAlternateLocation(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            var result = false;
            var altInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id).ToList();
            if (altInventory.Count > 0)
            {
                _ = _logger.LogDetailAsync($"424 In Alternate Locations of New Items");
                var qty = hostOrder.Qty.ParseInt();
                var inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
                if (inv == null)
                {
                    inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
                }
                if (inv != null)
                {
                    var orderDetail = new OrderDetail()
                    {
                        OrderId = orderId,
                        ItemDefinitionId = inv.ItemDefinitionId,
                        Quantity = (hostOrder.Qty).ParseInt(),
                        DateTime = hostOrder.DateTime,
                        EmpId = hostOrder.EmpId,
                        JobNum = hostOrder.JobNum,
                        NewBin = hostOrder.NewBin,
                        PartDesc = hostOrder.PartDesc,
                        PartNum = hostOrder.PartNum,
                        PrimeBin = hostOrder.PrimeBin,
                        Qty = hostOrder.Qty,
                        TroubleBit = hostOrder.TroubleBit,
                        TypeCode = hostOrder.TypeCode,
                        LineStatusId = 1,
                        AreaId = inv.Location.AreaId
                    };
                    try
                    {
                        _repoOrderDetail.Insert(orderDetail);
                    }
                    catch (Exception ex)
                    {
                        _ = _logger.LogDetailAsync("449 Insert Alternate Order Detail Item Error " + ex.Message);
                    }
                    result = true;
                }
            }
            return result;
        }

        private ItemDefinition NewItemExists(string partNum)
        {
            var itemDefinition = _repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            return itemDefinition;
        }

        private bool PickNewItemWithPrimeBin(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            var result = false;

            var allInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id && r.Quantity > 0)
               .OrderBy(p => p.PrimeBin)
               .ThenByDescending(t => t.Quantity)
               .ToList();
            var inv = allInventory.FirstOrDefault();

            if (inv != null)
            {
                _ = _logger.LogDetailAsync($"471 Has Prime Bin");
                var orderDetail = new OrderDetail()
                {
                    OrderId = orderId,
                    ItemDefinitionId = inv.ItemDefinitionId,
                    Quantity = (hostOrder.Qty).ParseInt(),
                    DateTime = hostOrder.DateTime,
                    EmpId = hostOrder.EmpId,
                    JobNum = hostOrder.JobNum,
                    NewBin = hostOrder.NewBin,
                    PartDesc = hostOrder.PartDesc, //inv.ItemDefinition.Description,  // 
                    PartNum = hostOrder.PartNum, //inv.ItemDefinition.Item,    //
                    PrimeBin = hostOrder.PrimeBin, //inv.Location.Slot,  // 
                    Qty = hostOrder.Qty,
                    TroubleBit = hostOrder.TroubleBit,
                    TypeCode = hostOrder.TypeCode,
                    LineStatusId = 1,
                    AreaId = inv.Location.AreaId
                };
                try
                {
                    _repoOrderDetail.Insert(orderDetail);
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"611 Pick New Item With Prime Bin Insert Order Detail Item Error \r\n {ex.Message}\r\n {ex.InnerException}");
                }
                result = true;
            }
            return result;
        }

        private bool PickUsedItemWithPrimeBin(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            var result = false;
            var allInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id && r.Quantity > 0)
                .OrderBy(p => p.PrimeBin)
                .ThenByDescending(t => t.Quantity)
                .ToList();
            var inv = allInventory.FirstOrDefault();

            if (inv != null)
            {
                _ = _logger.LogDetailAsync($"509 Picking Used Item Prime Bin Last.");
                var orderDetail = new OrderDetail()
                {
                    OrderId = orderId,
                    ItemDefinitionId = inv.ItemDefinitionId,
                    Quantity = (hostOrder.Qty).ParseInt(),
                    DateTime = hostOrder.DateTime,
                    EmpId = hostOrder.EmpId,
                    JobNum = hostOrder.JobNum,
                    NewBin = hostOrder.NewBin,
                    PartDesc = inv.ItemDefinition.Description,  // hostOrder.PartDesc,
                    PartNum = inv.ItemDefinition.Item,    // hostOrder.PartNum,
                    PrimeBin = inv.Location.Slot,  // hostOrder.PrimeBin,
                    Qty = hostOrder.Qty,
                    TroubleBit = hostOrder.TroubleBit,
                    TypeCode = hostOrder.TypeCode,
                    LineStatusId = 1,
                    AreaId = inv.Location.AreaId
                };
                try
                {
                    _repoOrderDetail.Insert(orderDetail);
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"529 Pick New Item With Prime Bin Insert Order Detail Item Error \r\n {ex.Message}\r\n {ex.InnerException}");
                }
                result = true;
            }
            //}
            return result;
        }

        private bool PickUsedItemWithAlternateLocation(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            var result = false;
            var altInventory = _repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id).ToList();
            if (altInventory.Count > 0)
            {
                _ = _logger.LogDetailAsync($"543 In Alternate Locations of Used Items");
                var qty = hostOrder.Qty.ParseInt();
                var inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
                if (inv == null)
                {
                    inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
                }
                if (inv != null)
                {
                    var orderDetail = new OrderDetail()
                    {
                        OrderId = orderId,
                        ItemDefinitionId = inv.ItemDefinitionId,
                        Quantity = (hostOrder.Qty).ParseInt(),
                        DateTime = hostOrder.DateTime,
                        EmpId = hostOrder.EmpId,
                        JobNum = hostOrder.JobNum,
                        NewBin = hostOrder.NewBin,
                        PartDesc = hostOrder.PartDesc,
                        PartNum = hostOrder.PartNum,
                        PrimeBin = hostOrder.PrimeBin,
                        Qty = hostOrder.Qty,
                        TroubleBit = hostOrder.TroubleBit,
                        TypeCode = hostOrder.TypeCode,
                        LineStatusId = 1,
                        AreaId = inv.Location.AreaId
                    };
                    try
                    {
                        _repoOrderDetail.Insert(orderDetail);
                    }
                    catch (Exception ex)
                    {
                        _ = _logger.LogDetailAsync("576 Insert Alternate Order Detail Item Error " + ex.Message);
                    }
                    result = true;
                }
            }
            return result;
        }

        private bool PickUsedItem(int orderId, HostOrder hostOrder)
        {
            var result = false;
            var usedItemDefinition = new ItemDefinition();
            var usedItem = string.Format(format: "8{0}", arg0: hostOrder.PartNum.Substring(startIndex: 1));
            var recs = new List<Inventory>();

            usedItemDefinition = _repoItemDefinition.FindBy(r => r.Item == usedItem).FirstOrDefault();

            if (usedItemDefinition != null)
            {
                var pickUsedItemWithPrimeBin = PickUsedItemWithPrimeBin(usedItemDefinition, orderId, hostOrder);
                result = pickUsedItemWithPrimeBin;
                if (!pickUsedItemWithPrimeBin)
                {
                    _ = _logger.LogDetailAsync($"599 Used Item Defined, but Not In Prime Bin");

                    var pickUsedItemWithAlternateLocation = PickUsedItemWithAlternateLocation(usedItemDefinition, orderId, hostOrder);
                    result = pickUsedItemWithAlternateLocation;
                    if (!pickUsedItemWithAlternateLocation)
                    {
                        _ = _logger.LogDetailAsync($"605 Used Item Definition found but Inventory Not found anywhere");
                    }
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"623 Used Item Not Defined");
            }
            return result;
        }

        private int CreateOrderRecord(HostOrder hostOrder)
        {
            var firstRec = hostOrder;
            var order = new Order();
            if (firstRec != null)
            {
                order = new Order()
                {
                    Ord1 = firstRec.JobNum,
                    Ord2 = firstRec.EmpId,
                    Priority = 99,
                    LoadDate = DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = 1
                };
                try
                {
                    _repoOrder.Insert(order);
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync("Insert Order Error " + ex.Message);
                }

            }
            return order.Id;
        }

        private Inventory CreatePrimeBinAndInventory(HostOrder hostOrder, ItemDefinition def)
        {
            Inventory inv = null;
            Location loc = null;
            //is the location available

            var location = _repoLocation.All().Where(l => l.Slot == hostOrder.PrimeBin).FirstOrDefault();
            if (location == null)
            {
                loc = new Location();
                loc.AreaId = def.AreaId;
                loc.Loc1 = hostOrder.PrimeBin.Substring(1, 1).ParseInt();
                loc.Loc2 = hostOrder.PrimeBin.Substring(2, 2).ParseInt();
                loc.Loc3 = hostOrder.PrimeBin.Substring(5, 2).ParseInt();
                loc.Loc4 = hostOrder.PrimeBin.Substring(8, 2).ParseInt();
                loc.Loc5 = 1;
                loc.Slot = hostOrder.PrimeBin;
                loc.SizeCodeId = def.SizeCodeId;
                loc.HeightCodeId = def.HeightCodeId;
                loc.VelocityCodeId = def.VelocityCodeId;
                loc.LocationCode = string.Empty;
                loc.InUse = true;

                _repoLocation.Insert(loc);

            }

            location = _repoLocation.All().Where(l => l.Slot == hostOrder.PrimeBin).FirstOrDefault();
            if (location != null)
            {
                //is there anything already in the location/Inventory
                var inventory = _repoInventory.All().Where(l => l.LocationId == location.Id).FirstOrDefault();
                if (inventory == null)
                {
                    //create the inventory record with zero quantity
                    inv = new Inventory
                    {
                        ItemDefinitionId = def.Id
                        ,
                        LocationId = location.Id
                        ,
                        PrimeBin = true
                        ,
                        StorageTypeId = 1
                        ,
                        ReceivedDate = DateTime.Now
                        ,
                        Quantity = (hostOrder.Qty).ParseInt()
                    };
                    _repoInventory.Insert(inv);
                }
                else
                {
                    _ = _logger.LogDetailAsync($"Inventory Location exists.  Can't create new Inventory Item for: {hostOrder.PartNum} Prime: {hostOrder.PrimeBin}");
                }
            }
            return inv;

        }
        private void ShowFileLockMessages(OperationResult fileLockFailure)
        {
            foreach (var msg in fileLockFailure.MessageList)
            {
                _ = _logger.LogDetailAsync("Lock Message " + msg);
            }
        }
    }
}


