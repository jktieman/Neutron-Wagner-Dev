using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AlliedLogger;
using Allied2Nova.Models;
using Allied2Nova.Processors;
using AlliedFileSystemWatcher;
using Core.Common;
using Core.Common.Extensions;
using NeutronCore;
using System.ComponentModel;
using NeutronData.Repositories;
using NeutronData.Models;
using NeutronData.DataContexts;
using NeutronCore.Extensions;

namespace HostLoader
{
    public class Pr1Processor
    {
        private readonly GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly InventoryRepository repoInv = new InventoryRepository();
        private readonly GenericRepository<LocationCount> repoLocationCount = new GenericRepository<LocationCount>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly HostPreferences prefs;

        private readonly List<string> excluedItems;
        //private readonly IList<CdefRec> cdefRecords;
        //private readonly IList<OcDefRec> ocDefRecords;
        //private readonly CdefProcessor cdefProcessor;
        private readonly BlockingCollection<FileInfo> orderQueue = new BlockingCollection<FileInfo>();
        static BackgroundWorker orderQueueProcessor;

        public Pr1Processor(HostPreferences prefs, IList<CdefRec> cdefRecords
            , IList<OcDefRec> ocDefRecords, CdefProcessor cdefProcessor)
        {
            this.prefs = prefs;
            this.cdefRecords = cdefRecords;
            this.ocDefRecords = ocDefRecords;
            this.cdefProcessor = cdefProcessor;
            excluedItems = new List<string> { "Discount" };
            InitBackgroundWorker();
        }

        private void InitBackgroundWorker()
        {
            orderQueueProcessor = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            orderQueueProcessor.DoWork += OrderQueueProcessorDoWork;
            orderQueueProcessor.RunWorkerCompleted += OrderQueueProcessorRunWorkerCompleted;
        }

        private void OrderQueueProcessorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Logger.Log("OrderQueueProcessorDoWork was Canceled.");
            }
            else if (e.Error != null)
            {
                Logger.Log("OrderQueueProcessorDoWork Exception: " + e.Error.ToString());
            }
            else
            {
                Logger.Log("OrderQueueProcessor DoWork Completed");
            }
        }

        private void OrderQueueProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            Logger.Log(msg: "OrderQueueProcessorDoWork has started");
            Thread.Sleep(millisecondsTimeout: 2000);
            foreach (var fileInfo in orderQueue.GetConsumingEnumerable())
            {
                string fileName = fileInfo.FullName;
                if (File.Exists(fileName))
                {
                    ProcessOrder(fileInfo);
                    Logger.Log("PR1 File has been processed: " + fileInfo.FullName);
                    Thread.Sleep(millisecondsTimeout: 1000);
                    ArchiveFile(fileInfo);
                }
                else
                {
                    ShowFileLockMessages(fileInfo.FileLockFailure());
                }
            }
            Logger.Log(msg: "OrderQueueProcessor DoWork has finished");
        }

        public Pr1Processor(HostPreferences prefs)
        {
            this.prefs = prefs;
            excluedItems = new List<string> { "Discount" };
        }

        public void FileCreated(object sender, FileInfoArgs e)
        {
            FileInfo fileInfo = e.FileInfo;
            if (!fileInfo.FileLockFailure().Success)
            {
                Logger.Log("File Created - Adding to Queue " + fileInfo.FullName);
                orderQueue.Add(fileInfo);
                if (!orderQueueProcessor.IsBusy)
                {
                    orderQueueProcessor.RunWorkerAsync();
                }
                else
                {
                    Logger.Log("ProcessOrderQueue is running.");
                }
            }
        }

        private void ShowFileLockMessages(OperationResult fileLockFailure)
        {
            foreach (var msg in fileLockFailure.MessageList)
            {
                Logger.Log("Lock Message " + msg);
            }
        }

        public void ProcessExistingPR1s()
        {
            try
            {
                var di = new DirectoryInfo(prefs.HostFolder);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles(prefs.Filter);
                    foreach (var fileInfo in files)
                    {
                        if (!fileInfo.FileLockFailure().Success)
                        {
                            orderQueue.Add(fileInfo);
                        }
                    }

                    if (!orderQueueProcessor.IsBusy)
                    {
                        Logger.Log(msg: "ProcessOrderQueue started from ProcessExistingPR1s");
                        orderQueueProcessor.RunWorkerAsync();
                    }
                    else
                    {
                        Logger.Log("ProcessOrderQueue is running.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("ERROR Processing Existing PR1  Files: {0}", ex.Message));
            }
        }

        private void ProcessOrder(FileInfo fileInfo)
        {
            Logger.Log(string.Format("Process PR1 Order File: {0}", fileInfo.Name));
            string orderLine = string.Empty;
            string type3Line = string.Empty;
            var type2Lines = new List<string>();
            string line;
            try
            {
                using (StreamReader sr = new StreamReader(fileInfo.FullName))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        switch (line.Substring(0, 1))
                        {
                            case "1":
                                orderLine = line;
                                Logger.Log(string.Format("Order Line: {0}", line));
                                break;
                            case "2":

                                if (!ExcludedItem(line))
                                {
                                    line = CheckOcOnly(line);
                                    type2Lines.Add(line);
                                    Logger.Log(string.Format("Item Line: {0}", line));
                                }
                                else
                                {
                                    Logger.Log(string.Format("EXCLUDED Item Line: {0}", line));
                                }

                                break;
                            default:
                                Logger.Log(string.Format("Extra Line in Order: {0}", line));
                                break;
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(orderLine) && type2Lines.Count > 0)
                {
                    type3Line = CreateType3Line(orderLine, type2Lines);
                    Logger.Log(string.Format("Type 3 Line created: {0}", type3Line));

                    var newPr1 = CreateNewPr1(orderLine, type3Line, type2Lines);
                    MoveToNovaHost(newPr1);
                }
                else
                {
                    Logger.Log(string.Format("ERROR - Invalid Order File: {0}", fileInfo.FullName));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("ERROR - Processing Order File: {0} {1}", fileInfo.FullName, ex.Message));
            }
        }

        private string CheckOcOnly(string line)
        {
            Logger.Log("Check OC Only: " + line);
            StringBuilder sb = new StringBuilder(line);

            var sku = line.Substring(2, 35).Trim();
            Logger.Log("SKU to find: " + sku);
            var oc = cdefProcessor.GetFromNova(sku);
            if (oc != null && oc.OcOnly == 1)
            {
                Logger.Log("OC " + oc.Sku + " - " + oc.OcOnly);
                sb[48] = 'O';
            }
            else

                Logger.Log("Return line: " + sb.ToString());
            return sb.ToString();
        }

        private bool ExcludedItem(string line)
        {
            string sku = line.Substring(2, 35).Trim();

            foreach (var item in excluedItems)
            {
                if (sku == item)
                {
                    Logger.Log("Excluded Item: " + line);
                    return true;
                }
            }
            return false;
        }

        private void MoveToNovaHost(string newPr1)
        {
            string newFileName = string.Format("{0}{1}{2}", prefs.NovaHostFolder, @"\PR1.", GetExtension());
            Logger.Log(string.Format("New PR1 Name: {0}", newFileName));
            try
            {
                if (File.Exists(newFileName))
                {
                    File.Delete(newFileName);
                }
                Logger.Log("Move New PR1 to Novahost");
                File.Move(newPr1, newFileName);
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("ERROR Moving New Order File: {0} {1}", newFileName, ex.Message));
            }
        }

        private void ArchiveFile(FileInfo fileInfo)
        {
            Logger.Log("Dexter PR1 File to Move to Archive: " + fileInfo.FullName);
            string archiveDir = string.Format(@"{0}\{1}\", prefs.DexterFolder, @"\ARCHIVE\");
            string backupFileName = BackupFile.GetBackupFileName(archiveDir, fileInfo);
            //string archiveFile = string.Format("{0}{1}{2}", prefs.DexterFolder, @"\ARCHIVE\", fileInfo.Name);
            try
            {
                if (File.Exists(backupFileName))
                {
                    Logger.Log("Deleting Archive File: " + backupFileName);
                    File.Delete(backupFileName);
                }

                Logger.Log(string.Format("Move {0} to {1}", fileInfo.FullName, backupFileName));
                fileInfo.MoveTo(backupFileName);
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("ERROR Archiving File: {0} {1} {2}", fileInfo.FullName, backupFileName, ex.Message));
            }
            Logger.Log("All PR1 Files Moved");
        }

        private string CreateNewPr1(string orderLine, string type3Line, List<string> type2Lines)
        {
            Logger.Log("Create New Pr1 File");
            string newPr1 = "PR1.TMP";
            try
            {
                using (StreamWriter sw = new StreamWriter(newPr1))
                {
                    sw.WriteLine(orderLine);
                    sw.WriteLine(type3Line);
                    foreach (var line in type2Lines)
                    {
                        sw.WriteLine(line);
                    }
                }
                return newPr1;
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format("ERROR Creating New PR1 File: {0} {1} {2} {3}", newPr1, orderLine, type3Line, ex.Message));
            }
            return null;
        }

        private string GetExtension()
        {
            System.DateTime current = System.DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(current.Year.ToString());
            sb.Append(current.Month.ToString());
            sb.Append(current.Day.ToString());
            sb.Append(current.Hour.ToString());
            sb.Append(current.Minute.ToString());
            sb.Append(current.Second.ToString());
            sb.Append(current.Millisecond.ToString());
            return sb.ToString();
        }

        private string CreateType3Line(string orderLine, List<string> type2Lines)
        {
            string lineType = "3";
            string oneSpace = @" ";
            string conveyorRoute = @" 0010000";
            string dealer = @"                         ";
            string rush = @"    ";
            string defaultStations = @"          " + rush + "  ";
            List<int> stations = new List<int>();
            Logger.Log("Create Type 3 Line");
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(lineType);
                sb.Append(oneSpace);
                sb.Append(conveyorRoute);

                stations = GetStations(type2Lines);

                sb.Append(stations.Contains(1) ? @"C1" : @"  ");
                sb.Append(stations.Contains(2) ? @"C2" : @"  ");
                sb.Append(stations.Contains(3) ? @"C3" : @"  ");
                sb.Append(stations.Contains(4) ? @"C4" : @"  ");
                if (stations.Contains(4))
                    sb[9] = '1';
                sb.Append(stations.Contains(9) ? @"OC" : @"  ");
                if (stations.Contains(9))
                    sb[8] = '1';
                sb.Append(rush);
                sb.Append(stations.Contains(5) ? @"C5" : @"  ");
                if (stations.Contains(5))
                    sb[9] = '1';

                var orderNumber = orderLine.Substring(8, 10).Trim().PadLeft(9, '0');
                sb.Append(orderNumber);
                sb.Append(dealer);
                return sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Logger.Log(string.Format("ERROR - {0}", ex.Message));

            }
            finally
            {

            }

            Logger.Log("ERROR - Default Type 3 String");
            StringBuilder sbDefault = new StringBuilder();
            sbDefault.Append(lineType);
            sbDefault.Append(oneSpace);
            sbDefault.Append(conveyorRoute);
            sbDefault.Append(defaultStations);
            var orderNum = orderLine.Substring(8, 10).Trim().PadLeft(9, '0');
            sbDefault.Append(orderNum);
            sbDefault.Append(dealer);
            Logger.Log("ERROR - Using Default Type 3");
            Logger.Log(sbDefault.ToString());
            return sbDefault.ToString();
        }

        private List<int> GetStations(List<string> type2Lines)
        {
            List<int> stations = new List<int>();
            string sku;
            int station = 0;

            foreach (var line in type2Lines)
            {
                StringBuilder sb = new StringBuilder(line);
                if (sb[48] == 'O')
                {
                    station = 9;
                }
                else
                {
                    sku = line.Substring(2, 35).Trim();

                    IDefRec def = cdefRecords.Where(c => c.Sku.Trim() == sku).FirstOrDefault();
                    if (def == null)
                    {
                        def = ocDefRecords.Where(c => c.Sku.Trim() == sku).FirstOrDefault();
                    }

                    Logger.Log(string.Format("Get Station for Item: {0}", sku));
                    if (def != null)
                    {
                        station = def.Station;
                        Logger.Log(string.Format("Station: {0}", station.ToString()));

                    }
                }

                if (station == 0)
                {
                    Logger.Log("ERROR - Station is 0");
                }
                if (!stations.Contains(station))
                {
                    stations.Add(station);
                }
            }
            return stations;
        }
    }
}
