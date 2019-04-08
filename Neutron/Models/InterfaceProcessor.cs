using NeutronData.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using NeutronData.Repositories;
using NeutronData.DataContexts;
using AlliedFileSystemWatcher;
using Neutron.Extensions;
using System.Windows.Forms;
using Neutron.Global;

namespace Neutron.Models
{
    public class InterfaceProcessor
    {
        private readonly BlockingCollection<FileInfo> interfaceFileQueue = new BlockingCollection<FileInfo>();
        private static BackgroundWorker interfaceFileQueueProcessor;
        private readonly GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Order> repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());
        private readonly HistoryManager historyManager = new HistoryManager();
        private readonly DirectoryInfo interfaceDirectory;
        private readonly DirectoryInfo archiveDirectory;

        private AlliedFileWatcher interfaceWatcher;

        public InterfaceProcessor()
        {
            string configFilePath = String.Format("{0}", Properties.Settings.Default.ConfigFilePath);
            LoaderSettings.Init(configFilePath);
            interfaceDirectory = new DirectoryInfo(LoaderSettings.HostOrderDirectory);
            archiveDirectory = new DirectoryInfo(LoaderSettings.HostOrderDirectory + "\\Archive\\");
        }

        public void StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();
            FileInfo[] files = GetFiles();
            ProcessFiles(files);
            interfaceWatcher = new AlliedFileWatcher(interfaceDirectory.ToString(), filter: @"*.csv", includeSubdirectories: false);
            interfaceWatcher.FileCreated += FileCreated;
            interfaceWatcher.Start();
            Variables.LoaderRunning = true;
        }

        public void StopProcessingInterfaceFiles()
        {
            if (Variables.LoaderRunning)
            {
                StopBackgroundWorker();
                interfaceWatcher.Stop();
                Variables.LoaderRunning = false;
            }
        }

        public FileInfo[] GetFiles()
        {
            var result = new FileInfo[] { };
            try
            {
                result = interfaceDirectory.GetFiles(searchPattern: "*.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Get Files Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
            }
            return result;
        }

        public void ProcessFiles(FileInfo[] files)
        {
            var orders = new List<HostOrder>();
            Thread.Sleep(millisecondsTimeout: 1000);
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
                    ArchiveFile(fileInfo);
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

        private void StopBackgroundWorker()
        {
            interfaceFileQueueProcessor.CancelAsync();
        }

        private void InitBackgroundWorker()
        {
            interfaceFileQueueProcessor = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            interfaceFileQueueProcessor.DoWork += InterfaceFileQueueProcessorDoWork;
            interfaceFileQueueProcessor.RunWorkerCompleted += InterfaceFileQueueProcessorRunWorkerCompleted;
        }

        private void InterfaceFileQueueProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            if (interfaceFileQueueProcessor.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            var orders = new List<HostOrder>();
            Thread.Sleep(millisecondsTimeout: 1000);
            foreach (var fileInfo in interfaceFileQueue.GetConsumingEnumerable())
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
                    ArchiveFile(fileInfo);
                }
            }
            e.Result = orders;
            if (orders.Count() > 0)
            {
                ProcessOrdersToNeutron(orders);
            }

        }

        private void InterfaceFileQueueProcessorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }

            else
            {
                object result = e.Result;

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
                    parser.SetDelimiters(delimiters: new string[] { "|" });
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

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
            catch (Exception ex)
            {
                MessageBox.Show("Process Interface File Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
            }
            return hostOrderList;
        }

        internal void FileCreated(object sender, FileInfoArgs e)
        {
            var prevFiles = new FileInfo[] { };
            var currentFiles = new FileInfo[] { }; ;
            try
            {
                prevFiles = GetFiles();
                Thread.Sleep(millisecondsTimeout: 1000);
                currentFiles = GetFiles();
                while (prevFiles.Count() != currentFiles.Count())
                {
                    prevFiles = currentFiles;
                    Thread.Sleep(millisecondsTimeout: 1000);
                    currentFiles = GetFiles();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Created Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
            }
            ProcessFiles(currentFiles);
        }

        public bool ProcessOrdersToNeutron(IList<HostOrder> hostOrderLines)
        {
            bool result = false;
            string typeCode = hostOrderLines.First().TypeCode;

            switch (typeCode)
            {
                case "2":
                    Put(hostOrderLines);

                    break;
                case "3":
                    Store(hostOrderLines);

                    break;
                case "4":
                    INVADJ(hostOrderLines);

                    break;

                default:
                    break;
            }

            return result;
        }

        private void INVADJ(IEnumerable<HostOrder> hostOrderLines)
        {
            if (hostOrderLines.Count() > 0)
            {
                HostOrder firstRec = hostOrderLines.FirstOrDefault();
                if (firstRec != null)
                {
                    foreach (var hostOrder in hostOrderLines)
                    {
                        Inventory inventoryItem = repoInventory.FindByInclude(r => r.ItemDefinition.Item == hostOrder.PartNum
                                   && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                        if (inventoryItem != null)
                        {
                            inventoryItem.Quantity = (hostOrder.Qty).ParseInt();
                            repoInventory.Update(inventoryItem);
                            historyManager.SaveHistory(18, inventoryItem);
                        }
                        else
                        {
                            hostOrder.TroubleBit = "1";
                            hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not Found At That Location");
                            var hostfile = new HostFile(hostOrder);
                        }
                    }
                }
            }
        }

        private void Store(IEnumerable<HostOrder> hostOrderLines)
        {
            if (hostOrderLines.Count() > 0)
            {
                IEnumerable<string> distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct();
                int orderId = 0;

                foreach (var item in distinctOrders)
                {
                    //var order = new ReplenOrder();
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
                                    var replenOrderDetail = new ReplenOrderDetail()
                                    {
                                        ReplenOrderId = orderId,
                                        ItemDefinitionId = rec.ItemDefinitionId,
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
                                        LineStatusId = 1
                                    };
                                    try
                                    {
                                        repoReplenOrderDetail.Insert(replenOrderDetail);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Insert Store Item Error " + ex.Message);
                                    }
                                }
                                else
                                {
                                    hostOrder.TroubleBit = "1";
                                    hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not In Inventory Location");
                                    var hostFile = new HostFile(hostOrder);
                                }
                            }
                            else
                            {
                                hostOrder.TroubleBit = "1";
                                hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not Defined in Shuttle");
                                var hostFile = new HostFile(hostOrder);
                            }

                        }
                    }
                }
            }
        }

        private int CreateReplenOrderRecord(HostOrder hostOrder)
        {
            HostOrder firstRec = hostOrder;
            var order = new ReplenOrder();
            if (firstRec != null)
            {
                order = new ReplenOrder()
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
                    repoReplenOrder.Insert(order);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert Replen Order Error " + ex.Message);
                }

            }
            return order.Id;
        }

        public void Put(IList<HostOrder> hostOrderLines)
        {
            if (hostOrderLines.Count() > 0)
            {
                IEnumerable<string> distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct();
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
                                orderId = CreateOrderRecord(linesInOrder[0]);
                            }

                            HostOrder hostOrder = linesInOrder[i];
                            ItemDefinition usedItemDefinition = null;
                            if (Variables.CheckForUsedItem)
                            {
                                usedItemDefinition = CreateUsedItemDefinition(orderId, hostOrder);
                            }
                            if (usedItemDefinition == null)
                            {
                                ItemDefinition itemDefinition = repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                                if (itemDefinition != null)
                                {
                                    Inventory primeInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                                    if (primeInventory != null)
                                    {
                                        var orderDetail = new OrderDetail()
                                        {
                                            OrderId = orderId,
                                            ItemDefinitionId = primeInventory.ItemDefinitionId,
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
                                            LineStatusId = 1
                                        };
                                        try
                                        {
                                            repoOrderDetail.Insert(orderDetail);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("Insert Order Detail Item Error " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Inventory altInventory = repoInventory.FindBy(r => r.ItemDefinitionId == itemDefinition.Id).FirstOrDefault();
                                        if (altInventory != null)
                                        {
                                            var orderDetail = new OrderDetail()
                                            {
                                                OrderId = orderId,
                                                ItemDefinitionId = altInventory.ItemDefinitionId,
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
                                                LineStatusId = 1
                                            };
                                            try
                                            {
                                                repoOrderDetail.Insert(orderDetail);
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show("Insert Alternate Order Detail Item Error " + ex.Message);
                                            }
                                            hostOrder.TroubleBit = "1";
                                            hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not In Prime Bin");
                                            var hostaFile = new HostFile(hostOrder);
                                        }
                                        else  //NOT is Alternate Inventory  
                                        {
                                            Inventory inv = CreatePrimeBinAndInventory(hostOrder, itemDefinition);
                                            try
                                            {
                                                var orderDetail = new OrderDetail()
                                                {
                                                    OrderId = orderId,
                                                    ItemDefinitionId = itemDefinition.Id,
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
                                                    LineStatusId = 1
                                                };

                                                repoOrderDetail.Insert(orderDetail);

                                                hostOrder.TroubleBit = "1";  //RTS
                                                hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Location Not Defined, but created in Shuttle");
                                                var hosFile = new HostFile(hostOrder); //Create the File

                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show("Insert Order Detail Item Error " + ex.Message);
                                            }

                                            hostOrder.TroubleBit = "1";
                                            hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not In Inventory");
                                            var hostaFile = new HostFile(hostOrder);
                                        }
                                    }
                                }
                                else
                                {
                                    ItemDefinition def = CreateNewItemDefinition(hostOrder);
                                    Inventory inv = CreatePrimeBinAndInventory(hostOrder, def);
                                    if (inv != null)
                                    {
                                        try
                                        {

                                            var orderDetail = new OrderDetail()
                                            {
                                                OrderId = orderId,
                                                ItemDefinitionId = def.Id,
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
                                                LineStatusId = 1
                                            };

                                            repoOrderDetail.Insert(orderDetail);

                                            hostOrder.TroubleBit = "1";  //RTS
                                            hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not Defined in Shuttle");
                                            var hostaFile = new HostFile(hostOrder); //Create the File

                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show("Insert Order Detail Item Error " + ex.Message);
                                        } 
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private ItemDefinition CreateUsedItemDefinition(int orderId, HostOrder hostOrder)
        {
            var usedItemDefinition = new ItemDefinition();
            string usedItem = string.Format("8{0}", hostOrder.PartNum.Substring(startIndex: 1));
            Inventory rec = null;
            usedItemDefinition = repoItemDefinition.FindBy(r => r.Item == usedItem).FirstOrDefault();
            if (usedItemDefinition != null)
            {
                rec = repoInventory.FindBy(r => r.ItemDefinitionId == usedItemDefinition.Id).FirstOrDefault();
            }

            if (usedItemDefinition != null && rec != null)
            {
                var orderDetail = new OrderDetail()
                {
                    OrderId = orderId,
                    ItemDefinitionId = rec.ItemDefinitionId,
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
                    LineStatusId = 1
                };
                try
                {
                    repoOrderDetail.Insert(orderDetail);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert Used Item Error " + ex.Message);
                }
            }
            return usedItemDefinition;
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
                    MessageBox.Show("Insert Order Error " + ex.Message);
                }

            }
            return order.Id;
        }

        private Inventory CreatePrimeBinAndInventory(HostOrder hostOrder, ItemDefinition def)
        {
            var inv = new Inventory();
            var loc = new Location();
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
            }
            repoLocation.Insert(loc);
            historyManager.SaveHistory(10, loc);

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
                        Quantity = 0
                    };
                    repoInventory.Insert(inv);
                    historyManager.SaveHistory(17, inv);
                }
            }
            return inv;
        }

        private ItemDefinition CreateNewItemDefinition(HostOrder hostOrder)
        {
            var newDefinition = new ItemDefinition();
            var def = new ItemDefinition();
            try
            {
                def = repoItemDefinition.FindBy(r => r.Item == "Default").FirstOrDefault();
                if (def != null)
                {
                    newDefinition = new ItemDefinition
                    {
                        StationId = def.StationId
                        ,
                        Item = hostOrder.PartNum
                        ,
                        Description = hostOrder.PartDesc
                        ,
                        LocationMax = def.LocationMax
                        ,
                        LocationMin = def.LocationMin
                        ,
                        SystemMax = def.SystemMax
                        ,
                        SystemMin = def.SystemMin
                        ,
                        Weight = def.Weight
                        ,
                        Scale = def.Scale
                        ,
                        StorageTypeId = def.StorageTypeId
                        ,
                        UnitOfIssueId = def.UnitOfIssueId
                        ,
                        SizeCodeId = def.SizeCodeId
                        ,
                        VelocityCodeId = def.VelocityCodeId
                        ,
                        HeightCodeId = def.HeightCodeId
                        ,
                        LocationCodeId = def.LocationCodeId
                    };
                    repoItemDefinition.Insert(newDefinition);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to create New Item Definition. " + ex.Message + ex.InnerException.Message);
            }
            return newDefinition;
        }

        private void ShowFileLockMessages(OperationResult fileLockFailure)
        {
            foreach (var msg in fileLockFailure.MessageList)
            {
                MessageBox.Show("Lock Message " + msg);
            }
        }

        public void ArchiveFile(FileInfo fileInfo)
        {
            string archiveDir = string.Format(@"{0}\Archive\{1}"
        , LoaderSettings.HostOrderDirectory, fileInfo.Name);
            try
            {
                if (File.Exists(fileInfo.FullName))
                {
                    if (File.Exists(archiveDir))
                    {
                        File.Delete(archiveDir);
                    }

                    fileInfo.MoveTo(archiveDir);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Archive File Error: " + ex.Message);
            }
        }
    }
}
