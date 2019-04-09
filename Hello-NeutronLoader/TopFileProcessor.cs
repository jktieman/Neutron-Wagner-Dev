using AlliedLogger;
using JsonManager;
using Microsoft.VisualBasic.FileIO;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeutronLoader
{
    class TopFileProcessor
    {
        private readonly GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Order> repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());

        readonly NeutronVariables neutronVariables;
        readonly NeutronLicense neutronLicense;
        DynamicLogger logger;

        public TopFileProcessor()
        {
        }

        public TopFileProcessor(FileInfo fileInfo, NeutronVariables neutronVariables, NeutronLicense neutronLicense)
        {
            this.neutronVariables = neutronVariables;
            this.neutronLicense = neutronLicense;
            CreateLog(name: "File Processor", stationNumber: neutronVariables.StationNumber);
            var orders = new List<HostOrder>();

            string filename = fileInfo.FullName;
            if (File.Exists(filename))
            {
                orders = ProcessInterfaceFile(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);
                ArchiveFile.Archive(fileInfo);
            }
            else
            {
                ShowFileLockMessages(fileInfo.FileLockFailure());
            }

            if (orders.Count() > 0)
            {
                ProcessOrdersToNeutron(orders);
            }
        }

        public TopFileProcessor(List<FileInfo> files, NeutronVariables neutronVariables, NeutronLicense neutronLicense)
        {
            this.neutronVariables = neutronVariables;
            this.neutronLicense = neutronLicense;
            CreateLog(name: "File Processor", stationNumber: neutronVariables.StationNumber);
            var orders = new List<HostOrder>();
            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in files)
            {
                string filename = fileInfo.FullName;
                if (File.Exists(filename))
                {
                    //accumulate all orders into one list of HostOrders
                    var tempOrders = new List<HostOrder>();
                    tempOrders = ProcessInterfaceFile(fileInfo);

                    foreach (var ord in tempOrders)
                    {
                        orders.Add(ord);
                    }

                    Thread.Sleep(millisecondsTimeout: 100);
                    ArchiveFile.Archive(fileInfo);
                }
                else
                {
                    ShowFileLockMessages(fileInfo.FileLockFailure());
                }
            }
            if (orders.Count() > 0)
            {
                ProcessOrdersToNeutron(orders);
            }
        }

        private void CreateLog(string name, int stationNumber)
        {
            string logFileDir = LoaderSettings.GetLogFileDirectory();
            string folderName = ($"{name}_{stationNumber.ToString()}");
            string logActivity = LoaderSettings.EnableLogging;
            logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        public List<HostOrder> ProcessInterfaceFile(FileInfo fileInfo)
        {
            var hostOrderList = new List<HostOrder>();
            try
            {
                using (var parser = new TextFieldParser(fileInfo.FullName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiters: new string[] { "|" });
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields.Length >= 10)
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
                logger.Log("Process Interface File Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
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
            List<HostOrder> orders = hostOrderLines.Where(s => s.TypeCode == "2").ToList();
            if (orders.Count > 0)
            {
                Put(orders);
            }
        }

        private void ProcessType3(IList<HostOrder> hostOrderLines)
        {
            List<HostOrder> orders = hostOrderLines.Where(s => s.TypeCode == "3").ToList();
            if (orders.Count > 0)
            {
                Store(orders);
            }
        }

        private void ProcessType4(IList<HostOrder> hostOrderLines)
        {
            List<HostOrder> orders = hostOrderLines.Where(s => s.TypeCode == "4").ToList();
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
                    Inventory inventoryItem = repoInventory.FindByInclude(r => r.ItemDefinition.Item == hostOrder.PartNum
                               && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                    if (inventoryItem != null)
                    {
                        logger.Log($"216 Inventory Adjust From {inventoryItem.Quantity} To {hostOrder.Qty}");
                        inventoryItem.Quantity = (hostOrder.Qty).ParseInt();
                        repoInventory.Update(inventoryItem);
                    }
                    else
                    {
                        hostOrder.TroubleBit = "1";
                        hostOrder.EmpId = ($"EmpId:--- Note: Item Not Found At That Location");
                        
                        var hostFile = new HostFile(neutronLicense, neutronVariables);
                        hostFile.CreateHostFile(hostOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log($"231 Inventory Adjust Error:  {ex.Message} \r\n  {ex.InnerException}");
            }
        }

        private void Store(List<HostOrder> hostOrderLines)
        {
            bool orderHasRecords = false;
            List<string> distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct().ToList();
            int orderId = 0;

            foreach (var item in distinctOrders)
            {
                List<HostOrder> linesInOrder = hostOrderLines.Where(h => h.JobNum == item).ToList();
                if (linesInOrder.Count > 0)
                {
                    for (int i = 0; i < linesInOrder.Count(); i++)
                    {
                        if (i == 0)
                        {
                            {
                                orderId = CreateReplenOrderRecord(linesInOrder[0]);
                            }
                        }
                        HostOrder hostOrder = linesInOrder[i];

                        ItemDefinition storeItemDefinition = repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                        if (storeItemDefinition != null)
                        {
                            Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
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
                                    StationNumber = rec.Location.Station.StationNumber
                                };
                                try
                                {
                                    repoReplenOrderDetail.Insert(replenOrderDetail);
                                    orderHasRecords = true;
                                }
                                catch (Exception ex)
                                {
                                    logger.Log("267 Insert Store Item Error " + ex.Message);
                                }
                            }
                            else
                            {
                                Location location = repoLocation.FindBy(r => r.Slot == hostOrder.PrimeBin).FirstOrDefault();
                                if (location != null)
                                {
                                    bool result = CreateInventoryRecord(storeItemDefinition, location);

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
                                            StationNumber = location.Station.StationNumber
                                        };
                                        try
                                        {
                                            repoReplenOrderDetail.Insert(replenOrderDetail);
                                            orderHasRecords = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            logger.Log("300 Insert Store Item in new location Error " + ex.Message);
                                        }
                                    }
                                }
                                else   //location is null
                                {
                                    logger.Log($"{hostOrder.PrimeBin} is not set up in Locations. ");
                                    hostOrder.TroubleBit = "1";
                                    hostOrder.EmpId = ($"EmpId:--- Note: Location is not set up in Neutron");
                                    var hostFile = new HostFile(neutronLicense, neutronVariables);
                                    hostFile.CreateHostFile(hostOrder);
                                }
                            }
                        }
                        else
                        {
                            logger.Log($"{hostOrder.PartNum} is not set up in the System.");
                            hostOrder.TroubleBit = "1";
                            hostOrder.EmpId = ($"EmpId:--- Note: Item Not Defined in Shuttle");
                            var hostFile = new HostFile(neutronLicense, neutronVariables);
                            hostFile.CreateHostFile(hostOrder);
                        }
                    }
                }
            }
            if (!orderHasRecords)
            {
                logger.Log($"Order has no records. Deleting Order: {orderId}");
                DeleteReplenOrderRecord(orderId);
            }
        }

        private bool CreateInventoryRecord(ItemDefinition storeItemDefinition, Location location)
        {
            bool result = false;
            var inventory = new Inventory
            {
                ItemDefinitionId = storeItemDefinition.Id,
                LocationId = location.Id,
                Quantity = 0,
                ReceivedDate = DateTime.Now,
                PrimeBin = true,
                StationId = location.StationId,
                StorageTypeId = 1
            };
            try
            {
                repoInventory.Insert(inventory);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Log($"291 Error Inserting new Inventory record.  {ex.Message} \r\n {ex.InnerException}");
            }

            return result;
        }

        private int CreateReplenOrderRecord(HostOrder hostOrder)
        {
            HostOrder firstRec = hostOrder;
            var order = new ReplenOrder();
            if (firstRec != null)
            {
                order = new ReplenOrder()
                {
                    Ord1 = firstRec.PartNum, //.JobNum,
                    Ord2 = firstRec.PartDesc, // .EmpId,
                    Priority = GetTrayNumber(firstRec.PrimeBin),
                    LoadDate = System.DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = 1
                };
                try
                {
                    repoReplenOrder.Insert(order);
                }
                catch (Exception ex)
                {
                    logger.Log($"Create Replen Order Error. OrderId: {hostOrder.Id}) " + ex.Message);
                }

            }
            return order.Id;
        }

        private int GetTrayNumber(string primeBin)
        {
            int result = 99;
            if (neutronLicense.CompanyCode == "TOP")
            {
                result = int.Parse(primeBin.Substring(2, 2));
            }
            return result;
        }

        private void DeleteReplenOrderRecord(int id)
        {
            try
            {
                repoReplenOrder.Delete(id);
            }
            catch (Exception ex)
            {
                logger.Log($"Delete Replen Order Error.  Order Id: {id} ." + ex.Message);
            }
        }

        public void Put(List<HostOrder> hostOrderLines)
        {

            IEnumerable<string> distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct();
            var hostFile = new HostFile(neutronLicense, neutronVariables);

            foreach (var item in distinctOrders)
            {
                string empId = string.Empty;
                int orderId = 0;
                var hostOrder = new HostOrder();
                List<HostOrder> linesInOrder = hostOrderLines.Where(h => h.JobNum == item).ToList();
                if (linesInOrder.Count > 0)
                {
                    for (int i = 0; i < linesInOrder.Count(); i++)
                    {
                        if (i == 0)
                        {
                            orderId = CreateOrderRecord(linesInOrder[0]);
                        }

                        hostOrder = linesInOrder[i];
                        empId = hostOrder.EmpId;
                        bool pickUsedItem = false;
                        if (neutronVariables.CheckForUsedItem)
                        {
                            pickUsedItem = PickUsedItem(orderId, hostOrder); //true if used inventory found
                        }
                        if (!pickUsedItem)
                        {
                            logger.Log($"384 Used Item Failed.  Start New Item Load. ");
                            ItemDefinition newItemExists = NewItemExists(hostOrder.PartNum);
                            if (newItemExists != null)
                            {
                                bool pickNewItemWithPrimeBin = PickNewItemWithPrimeBin(newItemExists, orderId, hostOrder);
                                if (!pickNewItemWithPrimeBin)
                                {
                                    hostOrder.TroubleBit = "1";
                                    hostOrder.EmpId = ($"EmpId: {empId} Note: Item Defined, but Not In Prime Bin");
                                    
                                    hostFile.CreateHostFile(hostOrder);

                                    bool pickNewItemWithAlternateLocation = PickNewItemWithAlternateLocation(newItemExists, orderId, hostOrder);
                                    if (!pickNewItemWithAlternateLocation)
                                    {
                                        logger.Log($"457 Item Definition found but Inventory Not found anywhere");
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
                                logger.Log($"378 Item Definition Not Found");
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
                        }
                    }
                }
                try
                {
                    int count = repoOrderDetail.FindBy(r => r.OrderId == orderId).Count();
                    if (count <= 0)
                    {
                        logger.Log($" 497 Order ID: {orderId}  Order Number: {hostOrder.JobNum}  Part Number: {hostOrder.PartNum} Deleted.");
                        repoOrder.Delete(orderId);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"415 Unable to delete Empty Order Id: {orderId}. \r\n {ex.Message} \r\n {ex.InnerException}");
                }
            }
        }


        private bool PickNewItemWithAlternateLocation(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            bool result = false;
            List<Inventory> altInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id).ToList();
            if (altInventory.Count > 0)
            {
                logger.Log($"424 In Alternate Locations of New Items");
                int qty = hostOrder.Qty.ParseInt();
                Inventory inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
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
                        StationNumber = inv.Location.Station.StationNumber
                    };
                    try
                    {
                        repoOrderDetail.Insert(orderDetail);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("449 Insert Alternate Order Detail Item Error " + ex.Message);
                    }
                    result = true;
                }
            }
            return result;
        }

        private ItemDefinition NewItemExists(string partNum)
        {
            ItemDefinition itemDefinition = repoItemDefinition.FindBy(r => r.Item == partNum).FirstOrDefault();
            return itemDefinition;
        }

        private bool PickNewItemWithPrimeBin(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            bool result = false;

            List<Inventory> allInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id && r.Quantity > 0)
               .OrderBy(p => p.PrimeBin)
               .ThenByDescending(t => t.Quantity)
               .ToList();
            Inventory inv = allInventory.FirstOrDefault();

            if (inv != null)
            {
                logger.Log($"471 Has Prime Bin");
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
                    StationNumber = inv.Location.Station.StationNumber
                };
                try
                {
                    repoOrderDetail.Insert(orderDetail);
                }
                catch (Exception ex)
                {
                    logger.Log($"611 Pick New Item With Prime Bin Insert Order Detail Item Error \r\n {ex.Message}\r\n {ex.InnerException}");
                }
                result = true;
            }
            return result;
        }

        private bool PickUsedItemWithPrimeBin(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            bool result = false;
            List<Inventory> allInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id && r.Quantity > 0)
                .OrderBy(p => p.PrimeBin)
                .ThenByDescending(t => t.Quantity)
                .ToList();
            Inventory inv = allInventory.FirstOrDefault();

            if (inv != null)
            {
                logger.Log($"509 Picking Used Item Prime Bin Last.");
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
                    StationNumber = inv.Location.Station.StationNumber
                };
                try
                {
                    repoOrderDetail.Insert(orderDetail);
                }
                catch (Exception ex)
                {
                    logger.Log($"529 Pick New Item With Prime Bin Insert Order Detail Item Error \r\n {ex.Message}\r\n {ex.InnerException}");
                }
                result = true;
            }
            //}
            return result;
        }

        private bool PickUsedItemWithAlternateLocation(ItemDefinition itemDef, int orderId, HostOrder hostOrder)
        {
            bool result = false;
            List<Inventory> altInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDef.Id).ToList();
            if (altInventory.Count > 0)
            {
                logger.Log($"543 In Alternate Locations of Used Items");
                int qty = hostOrder.Qty.ParseInt();
                Inventory inv = altInventory.Where(s => s.Quantity > qty).FirstOrDefault();
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
                        StationNumber = inv.Location.Station.StationNumber
                    };
                    try
                    {
                        repoOrderDetail.Insert(orderDetail);
                    }
                    catch (Exception ex)
                    {
                        logger.Log("576 Insert Alternate Order Detail Item Error " + ex.Message);
                    }
                    result = true;
                }
            }
            return result;
        }

        private bool PickUsedItem(int orderId, HostOrder hostOrder)
        {
            bool result = false;
            var usedItemDefinition = new ItemDefinition();
            string usedItem = string.Format(format: "8{0}", arg0: hostOrder.PartNum.Substring(startIndex: 1));
            var recs = new List<Inventory>();

            usedItemDefinition = repoItemDefinition.FindBy(r => r.Item == usedItem).FirstOrDefault();

            if (usedItemDefinition != null)
            {
                bool pickUsedItemWithPrimeBin = PickUsedItemWithPrimeBin(usedItemDefinition, orderId, hostOrder);
                result = pickUsedItemWithPrimeBin;
                if (!pickUsedItemWithPrimeBin)
                {
                    logger.Log($"599 Used Item Defined, but Not In Prime Bin");

                    bool pickUsedItemWithAlternateLocation = PickUsedItemWithAlternateLocation(usedItemDefinition, orderId, hostOrder);
                    result = pickUsedItemWithAlternateLocation;
                    if (!pickUsedItemWithAlternateLocation)
                    {
                        logger.Log($"605 Used Item Definition found but Inventory Not found anywhere");
                    }
                }
            }
            else
            {
                logger.Log($"623 Used Item Not Defined");
            }
            return result;
        }

        private int CreateOrderRecord(HostOrder hostOrder)
        {
            HostOrder firstRec = hostOrder;
            var order = new Order();
            if (firstRec != null)
            {
                order = new Order()
                {
                    Ord1 = firstRec.JobNum,
                    Ord2 = firstRec.EmpId,
                    Priority = 99,
                    LoadDate = System.DateTime.Now,
                    ShipperId = 1,
                    ShipMethodId = 1,
                    OrderStatusId = 1
                };
                try
                {
                    repoOrder.Insert(order);
                }
                catch (Exception ex)
                {
                    logger.Log("Insert Order Error " + ex.Message);
                }

            }
            return order.Id;
        }

        private Inventory CreatePrimeBinAndInventory(HostOrder hostOrder, ItemDefinition def)
        {
            Inventory inv = null;
            Location loc = null;
            //is the location available

            Location location = repoLocation.All().Where(l => l.Slot == hostOrder.PrimeBin).FirstOrDefault();
            if (location == null)
            {
                loc = new Location();
                loc.StationId = def.StationId;
                loc.Loc1 = hostOrder.PrimeBin.Substring(1, 1).ParseInt();
                loc.Loc2 = hostOrder.PrimeBin.Substring(2, 2).ParseInt();
                loc.Loc3 = hostOrder.PrimeBin.Substring(5, 2).ParseInt();
                loc.Loc4 = hostOrder.PrimeBin.Substring(8, 2).ParseInt();
                loc.Loc5 = 1;
                loc.Slot = hostOrder.PrimeBin;
                loc.SizeCodeId = def.SizeCodeId;
                loc.HeightCodeId = def.HeightCodeId;
                loc.VelocityCodeId = def.VelocityCodeId;
                loc.LocationCodeId = def.LocationCodeId;
                loc.InUse = true;

                repoLocation.Insert(loc);

            }

            location = repoLocation.All().Where(l => l.Slot == hostOrder.PrimeBin).FirstOrDefault();
            if (location != null)
            {
                //is there anything already in the location/Inventory
                Inventory inventory = repoInventory.All().Where(l => l.LocationId == location.Id).FirstOrDefault();
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
                    repoInventory.Insert(inv);
                }
                else
                {
                    logger.Log($"Inventory Location exists.  Can't create new Inventory Item for: {hostOrder.PartNum} Prime: {hostOrder.PrimeBin}");
                }
            }
            return inv;

        }


        //private ItemDefinition CreateNewItemDefinition(HostOrder hostOrder)
        //{
        //    var newDefinition = new ItemDefinition();
        //    var def = new ItemDefinition();
        //    try
        //    {
        //        def = repoItemDefinition.FindBy(r => r.Item == "Default").FirstOrDefault();
        //        if (def != null)
        //        {
        //            newDefinition = new ItemDefinition
        //            {
        //                StationId = def.StationId
        //                ,
        //                Item = hostOrder.PartNum
        //                ,
        //                Description = hostOrder.PartDesc
        //                ,
        //                LocationMax = def.LocationMax
        //                ,
        //                LocationMin = def.LocationMin
        //                ,
        //                SystemMax = def.SystemMax
        //                ,
        //                SystemMin = def.SystemMin
        //                ,
        //                Weight = def.Weight
        //                ,
        //                Scale = def.Scale
        //                ,
        //                StorageTypeId = def.StorageTypeId
        //                ,
        //                UnitOfIssueId = def.UnitOfIssueId
        //                ,
        //                SizeCodeId = def.SizeCodeId
        //                ,
        //                VelocityCodeId = def.VelocityCodeId
        //                ,
        //                HeightCodeId = def.HeightCodeId
        //                ,
        //                LocationCodeId = def.LocationCodeId
        //            };
        //            repoItemDefinition.Insert(newDefinition);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Log("Unable to create New Item Definition. " + ex.Message + ex.InnerException.Message);
        //    }
        //    return newDefinition;
        //}

        private void ShowFileLockMessages(OperationResult fileLockFailure)
        {
            foreach (var msg in fileLockFailure.MessageList)
            {
                logger.Log("Lock Message " + msg);
            }
        }
    }
}


