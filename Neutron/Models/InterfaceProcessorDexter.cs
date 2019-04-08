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
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Extensions;

namespace Neutron.Models
{
    public class InterfaceProcessorDexter
    {
        private readonly BlockingCollection<FileInfo> interfaceFileQueue = new BlockingCollection<FileInfo>();
        private static BackgroundWorker interfaceFileQueueProcessor;
        private readonly GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Order> repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly DirectoryInfo interfaceDirectory;
        private readonly DirectoryInfo archiveDirectory;

        private AlliedFileWatcher interfaceWatcher;

        public InterfaceProcessorDexter()
        {
            string configFilePath = String.Format("{0}", Properties.Settings.Default.ConfigFilePath);
            LoaderSettings.Init(configFilePath);
            interfaceDirectory = new DirectoryInfo(LoaderSettings.HostOrderDirectory);
            archiveDirectory = new DirectoryInfo(LoaderSettings.HostOrderDirectory + "\\Archive\\");
        }

        public void StartProcessingInterfaceFiles()
        {
            if (!Variables.LoaderRunning)
            {
                InitBackgroundWorker();
                FileInfo[] files = GetFiles();
                ProcessFiles(files);


                interfaceWatcher = new AlliedFileWatcher(interfaceDirectory.ToString(), filter: @"*.csv", includeSubdirectories: false);
                interfaceWatcher.FileCreated += FileCreated;
                interfaceWatcher.Start();
                Variables.LoaderRunning = true;
            }
            else
            {
                interfaceWatcher.Stop();
                Variables.LoaderRunning = false;
                MessageBox.Show("Loader is not running.");
            }

        }

        public FileInfo[] GetFiles()
        {
            var result = new FileInfo[] { };
            result = interfaceDirectory.GetFiles(searchPattern: "*.csv");
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
                //else
                //{
                //    ShowFileLockMessages(fileInfo.FileLockFailure());
                //}
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
                //    formAdpToLibra.UpdataListboxInfo(string.Format(format: "ADP File Queue Processor - Do Work Canceled."));
            }
            //else if (e.Error != null)
            //{
            //    formAdpToLibra.UpdataListboxInfo(string.Format(format: "ADP File Queue Processor - Do Work Exception: \\n{0}", arg0: e.Error.ToString()));
            //}
            else
            {
                var result = e.Result;
                //    formAdpToLibra.UpdataListboxInfo(string.Format(format: "ADP File Queue Processor - Do Work Completed"));
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
                    parser.SetDelimiters("|");
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
                MessageBox.Show("Process Interface File Error " + ex.Message);
            }
            return hostOrderList;
        }

        internal void FileCreated(object sender, FileInfoArgs e)
        {
            FileInfo[] prevFiles = GetFiles();
            Thread.Sleep(millisecondsTimeout: 1000);
            FileInfo[] currentFiles = GetFiles();

            while (prevFiles.Count() != currentFiles.Count())
            {
                prevFiles = currentFiles;
                Thread.Sleep(millisecondsTimeout: 1000);
                currentFiles = GetFiles();
            }
            ProcessFiles(currentFiles);
            //foreach (var file in currentFiles)
            //{
            //    if (!file.FileLockFailure().Success)
            //    {
            //        interfaceFileQueue.Add(file);
            //    }

            //    if (!interfaceFileQueueProcessor.IsBusy)
            //    {
            //        interfaceFileQueueProcessor.RunWorkerAsync();
            //    }
            //    MessageBox.Show("HJere");
            //}
            //MessageBox.Show("HJere 2");
        }

        public bool ProcessOrdersToNeutron(IEnumerable<HostOrder> hostOrderLines)
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
                HostOrder firstRec = hostOrderLines.FirstOrDefault();
                if (firstRec != null)
                {
                    var replenOrder = new ReplenOrder()
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
                        repoReplenOrder.Insert(replenOrder);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Insert Replen Order Error " + ex.Message);
                    }

                    foreach (var hostOrder in hostOrderLines)
                    {
                        ItemDefinition storeItemDefinition = repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                        if (storeItemDefinition != null)
                        {
                            Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == storeItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                            if (rec != null)
                            {
                                var replenOrderDetail = new ReplenOrderDetail()
                                {
                                    ReplenOrderId = replenOrder.Id,
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


        public void Put(IEnumerable<HostOrder> hostOrderLines)
        {
            if (hostOrderLines.Count() > 0)
            {
                var distinctOrders = hostOrderLines.Select(s => s.JobNum).Distinct();

                foreach (var item in distinctOrders)
                {
                    HostOrder firstRec = hostOrderLines.FirstOrDefault( h => h.JobNum == item);
                    if (firstRec != null)
                    {
                        var order = new Order()
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

                        foreach (var hostOrder in hostOrderLines)
                        {
                            if (hostOrder.JobNum == order.Ord1)
                            {
                                //could add check for used item flag in variables
                                string usedItem = string.Format("8{0}", hostOrder.PartNum.Substring(startIndex: 1));
                                ItemDefinition usedItemDefinition = repoItemDefinition.FindBy(r => r.Item == usedItem).FirstOrDefault();
                                if (usedItemDefinition != null)
                                {
                                    Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == usedItemDefinition.Id).FirstOrDefault();
                                    if (rec != null)
                                    {
                                        var orderDetail = new OrderDetail()
                                        {
                                            OrderId = order.Id,
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
                                }
                                else
                                {
                                    ItemDefinition newItemDefinition = repoItemDefinition.FindBy(r => r.Item == hostOrder.PartNum).FirstOrDefault();
                                    if (newItemDefinition != null)
                                    {
                                        Inventory rec = repoInventory.FindBy(r => r.ItemDefinitionId == newItemDefinition.Id && r.Location.Slot == hostOrder.PrimeBin).FirstOrDefault();
                                        if (rec != null)
                                        {
                                            var orderDetail = new OrderDetail()
                                            {
                                                OrderId = order.Id,
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
                                                MessageBox.Show("Insert Order Detail Item Error " + ex.Message);
                                            }
                                        }
                                        else
                                        {
                                            hostOrder.TroubleBit = "1";
                                            hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not In Inventory");
                                            var hostaFile = new HostFile(hostOrder);
                                        }
                                    }
                                    else
                                    {
                                        hostOrder.TroubleBit = "1";  //RTS
                                        hostOrder.EmpId = ($"EmpId:{Variables.user.EmpId} Note: Item Not Defined in Shuttle");
                                        var hostaFile = new HostFile(hostOrder); //Create the File
                                    }
                                } 
                            }
                        }
                    }

                }
            }
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
