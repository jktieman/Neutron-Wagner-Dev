using AlliedLogger;
using HighJump;
using NeutronCore;
using NeutronCore.Enums;
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
    public class HostFile
    {
        private DirectoryInfo _hostUploadDirectory;
        private DirectoryInfo _logFileDirectory;
        private bool _usePr1Processor;
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<User> _repoUser = new GenericRepository<User>(new NeutronDb());
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly WorkstationView _workstationView;
        private readonly IDynamicLogger _logger;
        private string _neutronUpFileName;

        public HostFile(NeutronLicense neutronLicense, NeutronVariables neutronVariables, WorkstationView workstationView)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _workstationView = workstationView;
            LoaderSettings.Init();
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            _logger = NeutronCore.Global.Logger.SetupLogger("HostFile");
        }

        public bool CreateHostFile(List<History> historyRecs)
        {
            _ = _logger.LogDetailAsync($"49 CreateHostFile with HistoryRecs");
            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;

            if (_hostUploadDirectory == null) return false;
            if (_usePr1Processor)
            {
                _ = _logger.LogDetailAsync($"61 HostFile List<History> Calling SaveUploadDatFile HistoryRecs");
                if (SaveUploadDatFile(historyRecs))
                {
                    return true;
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"66 HostFile List<HostOrder> Calling SaveFile groups");
                SaveFile(historyRecs);
                return true;
            }

            return false;
        }


        public void CreateMetHostFile(List<History> recs)
        {
            SendUploadToHostSql(recs);
        }

        public void SendUploadToHostSql(List<History> history)
        {
            if (history.Count == 0) return ;

            _ = _logger.LogDetailAsync("Send Upload To Host Sql.");

            try
            {
                using (var db = new HighJumpContext())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var line in history)
                            {
                                var inBound = new t_al_host_carousel_inbound();
                                inBound.container_label = $"{line.Ord2}{line.Ord1}";
                                inBound.employee_id = line.EmpId;
                                inBound.item_number = line.Item;
                                inBound.pick_quantity = Double.Parse(line.IssuedQuantity.ToString());
                                inBound.status = "N";
                                inBound.inserted_by = "CAROUSEL";
                                inBound.inserted_date = DateTime.Now;
                                inBound.updated_by = "CAROUSEL";
                                inBound.updated_date = DateTime.Now;

                                db.t_al_host_carousel_inbound.Add(inBound);
                            }

                            db.SaveChanges();
                            transaction.Commit();
                            _ = _logger.LogDetailAsync("Sql Table Updated With " + history.Count() + "Records.");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            var msg = $"Process Upload To Sql Table Transaction Error. Records: {history.Count} {Environment.NewLine}" +
                                         $"{ex.Message}{Environment.NewLine}{ex.InnerException}";
                            _ = _logger.LogDetailAsync(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = $"Process Upload To Sql Table.  {Environment.NewLine}" +
                             $"{ex.Message}{Environment.NewLine}{ex.InnerException}";
                _ = _logger.LogDetailAsync(msg);
            }
            _ = _logger.LogDetailAsync("Send Upload To Host Sql - Success.");
        }


        private void SaveFile(List<History> historyRecs)
        {
            historyRecs = historyRecs.OrderBy(o => o.Ord1).ThenBy(o => o.Item).ToList();
            var groups = historyRecs.GroupBy(g => g.Ord1).ToList();
            _ = _logger.LogDetailAsync("76 SaveFile History Groups");
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            var fullName = $"{_hostUploadDirectory.FullName}{GetFileName(string.Empty)}";
            _ = _logger.LogDetailAsync($"83 SaveFile History Groups: {fullName}");
            try
            {
                using (var tw = new StreamWriter(fullName, true))
                {
                    foreach (var group in groups)
                    {
                        foreach (var item in group)
                        {
                            var csv = GetCsvString(item);
                            if (csv.Length > 0)
                            {
                                tw.WriteLine(csv);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"103 Save History Group File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save Group File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

        }

        private string GetCsvString(History item)
        {
            _ = _logger.LogDetailAsync($"111 GetCsvString history: {item.Ord1}");

            _ = _logger.LogDetailAsync($"113 History Get CSV String Company Code: {_neutronLicense.CompanyCode}");
            string operation = string.Empty;
            var sb = new StringBuilder();

            if (_neutronVariables.UsePr1StyleOutputProcessor)
            {
                _ = _logger.LogDetailAsync($"119 NOVA/SFH - UsePr1Processor: true");
                //Saint Francis Hospital
                string loc = @"        ";
                if (item.OrderDetailInfo.Length >= 8)
                {
                    loc = item.OrderDetailInfo.Substring(startIndex: 0, length: 9);
                }
                string reqQty = item.RequestedQuantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');


                sb.Length = 127;
                sb.Insert(index: 0, value: "3O");
                sb.Insert(index: 2, value: item.Ord1.PadRight(10));
                sb.Insert(index: 13, value: item.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 24, value: item.ActionDateTime);
                sb.Insert(index: 33, value: item.LoadDate);
                sb.Insert(index: 42, value: item.Item.PadRight(totalWidth: 35));
                sb.Insert(index: 78, value: reqQty);
                sb.Insert(index: 88, value: item.IssuedQuantity.ToString().PadLeft(9, paddingChar: '0'));
                sb.Insert(index: 98, value: "07:14");
                sb.Insert(index: 104, value: "02");
                sb.Insert(index: 107, value: item.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 118, value: loc);
            }
            else if (_neutronLicense.CompanyCode == "TMG")
            {
                //TMG
                _ = _logger.LogDetailAsync($"146 GetCsvString History TMG - NOT use PR1 Processor");
                _ = _logger.LogDetailAsync($"147 GetCsvString History.NewBin: {item.NewBin}");
                if (!string.IsNullOrEmpty(item.OrderDetailInfo))
                {
                    var fields = item.OrderDetailInfo.Split(separator: new char[] { '|' });
                    operation = fields.Length == 3 ? fields[2] : string.Empty;
                }
                sb.Append(item.Ord1 + _neutronVariables.FieldDelimiter);
                sb.Append(operation + _neutronVariables.FieldDelimiter);
                sb.Append(item.Item + _neutronVariables.FieldDelimiter);
                sb.Append(item.IssuedQuantity + _neutronVariables.FieldDelimiter);
            }
            else if (_neutronLicense.CompanyCode == "TOP")
            {
                _ = _logger.LogDetailAsync($"160 GetCsvString History TOP - NOT use PR1 Processor");
                // sb.Append(item.TypeCode + _neutronVariables.FieldDelimiter);
                sb.Append(item.Item + _neutronVariables.FieldDelimiter);
                sb.Append(item.Description + _neutronVariables.FieldDelimiter);
                sb.Append(item.Ord1 + _neutronVariables.FieldDelimiter);
                //sb.Append(item.PrimeBin + _neutronVariables.FieldDelimiter);
                // sb.Append(item.NewBin + _neutronVariables.FieldDelimiter);
                sb.Append(item.IssuedQuantity + _neutronVariables.FieldDelimiter);
                // sb.Append(item.TroubleBit + _neutronVariables.FieldDelimiter);
                sb.Append(item.ActionDateTime + _neutronVariables.FieldDelimiter);
                sb.Append(item.EmpId);
                sb.AppendLine();
            }
            else if (_neutronLicense.CompanyCode == "VID")
            {
                _ = _logger.LogDetailAsync($"160 GetCsvString History VID - NOT use PR1 Processor");
                sb.Append(item.Item + _neutronVariables.FieldDelimiter);
                sb.Append(item.Description + _neutronVariables.FieldDelimiter);
                sb.Append(item.Ord1 + _neutronVariables.FieldDelimiter);
                sb.Append(item.IssuedQuantity + _neutronVariables.FieldDelimiter);
                sb.Append(item.ActionDateTime + _neutronVariables.FieldDelimiter);
                sb.Append(item.EmpId);
                sb.AppendLine();
            }
            _ = _logger.LogDetailAsync($"173 GetCsvString History result: {sb.ToString()}");
            return sb.ToString();
        }

        private bool SaveUploadDatFile(List<History> historyRecs)
        {
            var result = false;
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            var fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                var fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);
                _ = _logger.LogDetailAsync($"187 FullName: {fullName}");
                try
                {
                    _ = _logger.LogDetailAsync("Set Neutron Busy.");
                    if (!SetNeutronBusy())
                    {
                        using (var tw = new StreamWriter(fullName, append: true))
                        {
                            foreach (var history in historyRecs)
                            {
                                tw.WriteLine(GetUploadDatRecord(history));
                            }
                        }

                        _ = _logger.LogDetailAsync("Clear Neutron Busy.");
                        ClearNeutronBusy();
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"211 Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
                finally
                {
                    _ = _logger.LogDetailAsync("Clear Neutron Busy.");
                    ClearNeutronBusy();
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"221 Go to Options and enter an upload file name.");
            }

            return result;
        }

        private void ClearNeutronBusy()
        {
            if (File.Exists(_neutronUpFileName))
            {
                File.Delete(_neutronUpFileName);
                _ = _logger.LogDetailAsync($"ClearNeutronBusy {_neutronUpFileName} exists, deleting file.");
            }
        }

        private bool SetNeutronBusy()
        {
            var counter = 0;
            var sapFileExist = true;
            string neutronBusyPath;
            string sapBusyPath;
            string sapUpPath;

            var neutronBusy = Environment.GetEnvironmentVariable("NEUTRONBUSY", EnvironmentVariableTarget.Machine);

            if (string.IsNullOrEmpty(neutronBusy))
            {
                Environment.SetEnvironmentVariable("NEUTRONBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }
          
            neutronBusyPath = neutronBusy != null ? neutronBusy.Trim() : LoaderSettings.GetRootDirectory();

            _ = _logger.LogDetailAsync($"NeutronBusyPath: {neutronBusyPath}");

            var sapBusy = Environment.GetEnvironmentVariable("SAPBUSY", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapBusy))
            {
                Environment.SetEnvironmentVariable("SAPBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            sapBusyPath = sapBusy != null ? sapBusy.Trim() : LoaderSettings.GetRootDirectory();

            _ = _logger.LogDetailAsync($"SapBusyPath: {sapBusyPath}");

            var sapUp = Environment.GetEnvironmentVariable("SAPUP", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapUp))
            {
                Environment.SetEnvironmentVariable("SAPUP", LoaderSettings.GetRootDirectory(),EnvironmentVariableTarget.Machine);
            }

            sapUpPath = sapUp != null ? sapUp.Trim() : LoaderSettings.GetRootDirectory();

            _ = _logger.LogDetailAsync($"SapUpPath: {sapUpPath}");

            _neutronUpFileName = neutronBusyPath + CheckForBackSlash(neutronBusyPath) + "UP";
            _ = _logger.LogDetailAsync($"NeutronUpFileName: {_neutronUpFileName}");
            var sapUpFileName = sapBusyPath + CheckForBackSlash(sapBusyPath) + "UP";
            _ = _logger.LogDetailAsync($"SapUpFileName: {sapUpFileName}");

            while (counter <= 10)
            {
                using (var up = new StreamWriter(_neutronUpFileName, append: true))
                {
                    up.WriteLine("Neutron");
                }

                if (File.Exists(sapUpFileName))
                {
                    sapFileExist = true;
                    File.Delete(_neutronUpFileName);
                    counter++;
                    Thread.Sleep(100);
                    _ = _logger.LogDetailAsync($"{sapUpFileName} exists, waiting 1000MS.");
                }
                else
                {
                    sapFileExist = false;
                    _ = _logger.LogDetailAsync($"{sapUpFileName} does not exists");
                    break;
                }
            }

            return sapFileExist;
        }

        private string CheckForBackSlash(string neutronBusyPath)
        {
            return neutronBusyPath.EndsWith(@"\") ? string.Empty : @"\";
        }
        
        // Saint Francis Upload Format
        private string GetUploadDatRecord(History history)
        {
          //  var station = history.AreaId == _workstationView.Id ? "9" : history.AreaId.ToString();
            var areaId = history.AreaId;
            var order = history.Ord1.PadRight(10);
            var costCenter = history.CostCenter;
            var orderDetailInfo = string.Empty;
            // Rightmost 24 characters of the OrderDetailInfo field
            var info = history.OrderDetailInfo;
            if (!string.IsNullOrEmpty(info))
            {
                if (info.Length >= 24)
                {
                    orderDetailInfo = info.Substring(info.Length - 24);
                }
            }

            var empName = string.Empty;
            var emp = _repoUser.FindBy(u => u.EmpId == history.EmpId).FirstOrDefault();
            if (emp != null)
            {
                empName = emp.Firstname.PadRight(10);
            }
            var upCode = ($"02");
            var time = DateTime.Now.ToString(format: "HH:mm");
            var invoice = history.Ord2.PadRight(totalWidth: 10, paddingChar: ' ');

            var sb = new StringBuilder(new string(' ', 170));
            sb.Insert(0, $"{areaId}O");
            sb.Insert(2, order);
            sb.Insert(13, invoice);
            sb.Insert(24, history.ActionDateTime.ToString("yyyyMMdd"));
            sb.Insert(33, history.ActionDateTime.ToString("yyyyMMdd"));
            sb.Insert(42, history.Item.PadRight(35));
            sb.Insert(78, history.RequestedQuantity.ToString().PadLeft(9, '0'));
            sb.Insert(88, history.IssuedQuantity.ToString().PadLeft(9, '0'));
            sb.Insert(98, time);
            sb.Insert(104, upCode);
            sb.Insert(107, empName);
            sb.Insert(118, costCenter);
            sb.Insert(129, areaId);
            sb.Insert(130, orderDetailInfo);
            sb.Length = 154;

            var result = sb.ToString();
            _ = _logger.LogDetailAsync($"{result}");
            return result;
        }

        public void CreateHostFile(HostOrder order)
        {
            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (_hostUploadDirectory != null)
            {
                SaveFile(order);
            }

            _logFileDirectory = GetDirectory(LoaderSettings.GetLogFileDirectory());
            if (_logFileDirectory != null && LoaderSettings.EnableLogging == "true")
            {
                SaveUploadLog(order);
            }
        }


        public void CreateHostFile(ReplenHostOrder order)
        {

            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;

            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (_hostUploadDirectory != null)
            {
                if (_usePr1Processor)
                {
                    SaveUploadDatFile(order);
                }
                else
                {
                    SaveFile(order);
                }
            }

            _logFileDirectory = GetDirectory(LoaderSettings.GetLogFileDirectory());
            if (_logFileDirectory != null && LoaderSettings.EnableLogging == "true")
            {
                SaveUploadLog(order);
            }
        }

        public void CreateHostFile(List<HostOrder> hostOrders)
        {
            _ = _logger.LogDetailAsync($"105 Folder Name: HostFile");
            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;
            hostOrders = hostOrders.OrderBy(o => o.JobNum).ThenBy(o => o.PartNum).ToList();

            List<IGrouping<string, HostOrder>> groups = hostOrders.GroupBy(g => g.JobNum).ToList();

            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (_hostUploadDirectory != null)
            {
                if (_usePr1Processor)
                {
                    _ = _logger.LogDetailAsync($"116 HostFile List<HostOrder> Calling SaveUploadDatFile groups");
                    SaveUploadDatFile(groups);
                }
                else
                {
                    _ = _logger.LogDetailAsync($"121 HostFile List<HostOrder> Calling SaveFile groups");
                    SaveFile(groups);
                }
            }
        }

        public void CreateHostFile(List<ReplenHostOrder> hostOrders)
        {
            List<ReplenHostOrder> orders = hostOrders.OrderBy(o => o.JobNum).ThenBy(o => o.PartNum).ToList();

            List<IGrouping<string, ReplenHostOrder>> groups = orders.GroupBy(g => g.JobNum).ToList();

            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;

            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (_hostUploadDirectory != null)
            {
                if (_usePr1Processor)
                {
                    _ = _logger.LogDetailAsync($"148 HostFile List<ReplenHostOrder> Calling SaveUploadDataFile groups");
                    SaveUploadDatFile(groups);
                }
                else
                {
                    _ = _logger.LogDetailAsync($"153 HostFile List<ReplenHostOrder> Calling SaveFile groups");
                    SaveFile(groups);
                }
            }
        }

        private void SaveFile(List<IGrouping<string, ReplenHostOrder>> groups)
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            string fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);

                DateTime d = DateTime.Now;
                string date = d.ToString(format: "MM-dd-yyyy");
                string time = d.ToString(format: "HH:mm");
                string timeSec = d.ToString(format: "HH:mm:ss");
                var begin = new StringBuilder(capacity: 100);
                begin.Insert(index: 0, value: $"BEGIN    {date}     {timeSec}");
                var end = new StringBuilder(capacity: 100);
                end.Insert(index: 0, value: $"END");
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        tw.WriteLine(begin.ToString());
                        foreach (var group in groups)
                        {
                            ReplenHostOrder endOfOrder = null;
                            foreach (ReplenHostOrder item in group)
                            {
                                endOfOrder = item;
                                tw.WriteLine(GetUploadDatRecord(item));
                            }
                            if (endOfOrder != null)
                            {
                                int status = _repoReplenOrders.FindByKey(endOfOrder.OrderDetail.ReplenOrder.Id).OrderStatusId;
                                if (status == 6)
                                {
                                    tw.WriteLine(GetEndOfOrderRecord(endOfOrder));
                                }
                            }
                        }
                        tw.WriteLine(end.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"205 Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    MessageBox.Show($"Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"211 Go to Options and enter an upload file name.");
                MessageBox.Show(text: "Go to Options and enter an upload file name.");
            }
        }

        private string GetEndOfOrderRecord(ReplenHostOrder order)
        {
            string upCode = ($"50");
            string time = DateTime.Now.ToString(format: "HH:mm");
            order.PartNum = ($"END-OF-ORDER");
            string qty = ($"000000000");
            string loc = string.Empty;
            order.EmpId = string.Empty;
            string area = order.OrderDetail.AreaId.ToString();
            var sb = new StringBuilder();
            sb.Length = 118;
            sb.Insert(index: 0, value: $"{area}O");
            sb.Insert(index: 2, value: order.JobNum.PadRight(totalWidth: 10));
            sb.Insert(index: 13, value: order.OrderDetail.ReplenOrder.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
            sb.Insert(index: 24, value: order.DateTime);
            sb.Insert(index: 33, value: order.DateTime);
            sb.Insert(index: 42, value: order.PartNum.PadRight(totalWidth: 35));
            sb.Insert(index: 78, value: qty);
            sb.Insert(index: 88, value: qty);
            sb.Insert(index: 98, value: time);
            sb.Insert(index: 104, value: upCode);
            sb.Insert(index: 107, value: order.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
            sb.Insert(index: 118, value: loc);

            return sb.ToString();
        }

        private string GetUploadDatRecord(ReplenHostOrder order)
        {
            string upCode = ActionCode.InventoryAdd.ToString();
            string time = DateTime.Now.ToString(format: "HH:mm");
            string reqQty = order.OrderDetail.Quantity.ToString().PadLeft(9, '0');
            string loc = order.OrderDetail.OrderDetailInfo;
            string area = order.OrderDetail.AreaId.ToString();
            string ord1 = order.JobNum.StartsWith(@"R") ? @"REPLENOPRP" : order.JobNum;
            string invoice = order.OrderDetail.ReplenOrder.Ord2.PadRight(totalWidth: 10, paddingChar: ' ');
            var sb = new StringBuilder();
            sb.Length = 160;
            sb.Insert(index: 0, value: $"{area}O");
            sb.Insert(index: 2, value: ord1.PadRight(10));
            sb.Insert(index: 13, value: invoice);
            sb.Insert(index: 24, value: order.DateTime);
            sb.Insert(index: 33, value: order.DateTime);
            sb.Insert(index: 42, value: order.PartNum.PadRight(totalWidth: 35));
            sb.Insert(index: 78, value: reqQty);
            sb.Insert(index: 88, value: order.Qty.PadLeft(totalWidth: 9, paddingChar: '0'));
            sb.Insert(index: 98, value: time);
            sb.Insert(index: 104, value: upCode);
            sb.Insert(index: 107, value: order.EmpId == null ? @"Unknown" : order.EmpId.PadRight(10, ' '));
            sb.Insert(index: 118, value: loc);
            sb.Length = 127;
            return sb.ToString();
        }

        private void SaveUploadDatFile(List<IGrouping<string, ReplenHostOrder>> groups)
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            string fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);

                DateTime d = DateTime.Now;
                string date = d.ToString(format: "MM-dd-yyyy");
                string time = d.ToString(format: "HH:mm");
                string timeSec = d.ToString(format: "HH:mm:ss");
                var begin = new StringBuilder(capacity: 100);
                begin.Insert(index: 0, value: $"BEGIN    {date}     {timeSec}");
                var end = new StringBuilder(capacity: 100);
                end.Insert(index: 0, value: $"END");
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        tw.WriteLine(begin.ToString());
                        foreach (var group in groups)
                        {
                            ReplenHostOrder endOfOrder = null;
                            foreach (ReplenHostOrder item in group)
                            {
                                endOfOrder = item;
                                tw.WriteLine(GetUploadDatRecord(item));
                            }
                            if (endOfOrder != null)
                            {
                                int status = _repoReplenOrders.FindByKey(endOfOrder.OrderDetail.ReplenOrder.Id).OrderStatusId;
                                if (status == 6)
                                {
                                    tw.WriteLine(GetEndOfOrderRecord(endOfOrder));
                                }
                            }
                        }
                        tw.WriteLine(end.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"314 Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    MessageBox.Show($"Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"320 Go to Options and enter an upload file name.");
                MessageBox.Show(text: "Go to Options and enter an upload file name.");
            }
        }

        private DirectoryInfo GetDirectory(string dir)
        {
            DirectoryInfo result = null;

            if (!string.IsNullOrEmpty(dir))
            {
                result = new DirectoryInfo(dir);
            }
            return result;
        }

        private void SaveUploadLog(HostOrder hostOrder)
        {
            if (!Directory.Exists(_logFileDirectory.FullName))
            {
                Directory.CreateDirectory(_logFileDirectory.FullName);
            }

            string fullName = string.Format(@"{0}{1}", _logFileDirectory.FullName, GetLogFileName());
            _ = _logger.LogDetailAsync($"341 SaveUploadLog fullName: {fullName}");

            try
            {
                var file = new FileInfo(fullName);

                if (!File.Exists(file.FullName))
                {
                    PrintHeading(ref file);
                }
                using (var tw = new StreamWriter(file.FullName, true))
                {
                    tw.WriteLine(GetCsvString(hostOrder));
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"359 Save Upload Log File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save Upload Log File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void SaveUploadLog(ReplenHostOrder hostOrder)
        {
            if (!Directory.Exists(_logFileDirectory.FullName))
            {
                Directory.CreateDirectory(_logFileDirectory.FullName);
            }

            string fullName = string.Format(@"{0}{1}", _logFileDirectory.FullName, GetLogFileName());
            _ = _logger.LogDetailAsync($"372 SaveUploadLog fullName: {fullName}");

            try
            {
                var file = new FileInfo(fullName);

                if (!File.Exists(file.FullName))
                {
                    PrintHeading(ref file);
                }
                using (var tw = new StreamWriter(file.FullName, true))
                {
                    tw.WriteLine(GetCsvString(hostOrder));
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"389 Save Upload Log File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save Upload Log File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private void PrintHeading(ref FileInfo file)
        {
            try
            {
                using (var tw = new StreamWriter(file.FullName, append: true))
                {
                    tw.WriteLine(GetHeading());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Print Heading Upload File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        private string GetLogFileName()
        {
            DateTime now = DateTime.Now;
            string str = now.ToString(format: "yyyyMMdd");
            return string.Format(format: "{0}.LOG", arg0: str);
        }

        //TMG
        private string GetFileName(string hostOrderTypeCode)
        {
            _ = _logger.LogDetailAsync($"419 Get File Name Company Code: {_neutronLicense.CompanyCode}");
            string result = string.Empty;
            if (_neutronLicense.CompanyCode == "TOP")
            {
                string d = DateTime.Now.ToString(format: "yyyyMMddHHmmssfff"); // case sensitive
                result = ($"{hostOrderTypeCode}{d}.CSV");
            }
            else
            {
                string fileName = LoaderSettings.GetHostUploadFile();
                if (!string.IsNullOrEmpty(fileName.Trim()))
                {
                    result = fileName;
                }
                else  //nothing in FileName field so create unique
                {
                    string d = DateTime.Now.ToString(format: "yyyyMMddHHmmssfff");
                    result = $"{hostOrderTypeCode}{d}_Upload.dat";
                }
            }
            return result;
        }

        //private string GetUploadExtension()
        //{
        //    string result = string.Empty;
        //    string extension = LoaderSettings.GetMainte nanceFileFilter();
        //    if (!string.IsNullOrEmpty(extension.Trim()))
        //    {
        //        result = extension;
        //    }
        //    else  //nothing in Extension so default to CSV
        //    {
        //        result = $".CSV";
        //    }
        //    return result;


        //}

        public void SaveFile(ReplenHostOrder order)
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            string fullName = string.Format(@"{0}{1}", _hostUploadDirectory.FullName, GetFileName(order.TypeCode));

            try
            {
                using (var tw = new StreamWriter(fullName, true))
                {
                    string csv = GetCsvString(order);
                    if (csv.Length > 0)
                    {
                        tw.WriteLine(csv);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"481 Save ReplenHostOrder File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save ReplenHostOrder File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        public void SaveFile(HostOrder order)
        {
            _ = _logger.LogDetailAsync($"536 SaveFile Order {order.JobNum}");
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            string fullName = string.Format(@"{0}{1}", _hostUploadDirectory.FullName, GetFileName(order.TypeCode));
            _ = _logger.LogDetailAsync($"543 SaveFile HostOrder - FullName: {fullName}");
            try
            {
                using (var tw = new StreamWriter(fullName, true))
                {
                    string csv = GetCsvString(order);
                    if (csv.Length > 0)
                    {
                        tw.WriteLine(csv);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"557 Save HostOrder File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save HostOrder File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        public void SaveFile(List<IGrouping<string, HostOrder>> groups)
        {
            _ = _logger.LogDetailAsync("564 SaveFile Groups");
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            string fullName = string.Format(@"{0}{1}", _hostUploadDirectory.FullName, GetFileName(string.Empty));
            _ = _logger.LogDetailAsync($"571 SaveFile Groups: {fullName}");
            try
            {
                using (var tw = new StreamWriter(fullName, true))
                {
                    foreach (var group in groups)
                    {
                        foreach (HostOrder item in group)
                        {
                            string csv = GetCsvString(item);
                            if (csv.Length > 0)
                            {
                                tw.WriteLine(csv);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"591 Save Group File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                //MessageBox.Show($"Save Group File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }

        public void SaveUploadDatFile(List<IGrouping<string, HostOrder>> groups)
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            string fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);
                _ = _logger.LogDetailAsync($"606 FullName: {fullName}");
                DateTime d = DateTime.Now;
                string date = d.ToString(format: "MM-dd-yyyy");
                string time = d.ToString(format: "HH:mm");
                string timeSec = d.ToString(format: "HH:mm:ss");
                var begin = new StringBuilder(100);
                begin.Insert(0, $"BEGIN    {date}     {timeSec}");
                var end = new StringBuilder(100);
                end.Insert(0, $"END");
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        tw.WriteLine(begin.ToString());
                        foreach (var group in groups)
                        {
                            HostOrder endOfOrder = null;
                            foreach (HostOrder item in group)
                            {
                                endOfOrder = item;
                                tw.WriteLine(GetUploadDatRecord(item));
                            }
                            if (endOfOrder != null)
                            {
                                int status = _repoOrders.FindByKey(endOfOrder.OrderDetail.Order.Id).OrderStatusId;
                                if (status == 6)
                                {
                                    tw.WriteLine(GetEndOfOrderRecord(endOfOrder));
                                }
                            }
                        }
                        tw.WriteLine(end.ToString());
                        //tw.Close();
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"643 Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    //MessageBox.Show($"Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"649 Go to Options and enter an upload file name.");
                //MessageBox.Show($"Go to Options and enter an upload file name.");
            }
        }

        private string GetEndOfOrderRecord(HostOrder hostOrder)
        {
            string upCode = ($"50");
            string time = DateTime.Now.ToString(format: "HH:mm");
            hostOrder.PartNum = ($"END-OF-ORDER");
            string qty = ($"000000000");
            string loc = string.Empty;
            hostOrder.EmpId = string.Empty;
            string area = hostOrder.OrderDetail.AreaId.ToString();
            var sb = new StringBuilder();
            sb.Length = 118;
            sb.Insert(0, value: $"{area}O");
            sb.Insert(2, hostOrder.JobNum.PadRight(totalWidth: 10));
            sb.Insert(13, hostOrder.OrderDetail.Order.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
            sb.Insert(24, hostOrder.DateTime);
            sb.Insert(33, hostOrder.DateTime);
            sb.Insert(42, hostOrder.PartNum.PadRight(totalWidth: 35));
            sb.Insert(78, qty);
            sb.Insert(88, qty);
            sb.Insert(98, time);
            sb.Insert(104, upCode);
            sb.Insert(107, hostOrder.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
            sb.Insert(118, loc);

            return sb.ToString();
        }

        private string GetUploadDatRecord(HostOrder hostOrder)
        {
            string upCode = ($"02");
            string time = DateTime.Now.ToString(format: "HH:mm");
            string reqQty = hostOrder.OrderDetail.Quantity.ToString().PadLeft(9, '0');
            string loc = hostOrder.OrderDetail.OrderDetailInfo;  //.Substring(0, 9)
            string area = hostOrder.OrderDetail.AreaId.ToString();
            string invoice = hostOrder.OrderDetail.Order.Ord2.PadRight(totalWidth: 10, paddingChar: ' ');
            var sb = new StringBuilder();
            sb.Length = 127;
            sb.Insert(0, $"{area}O");
            sb.Insert(2, hostOrder.JobNum.PadRight(10));
            sb.Insert(13, invoice);
            sb.Insert(24, hostOrder.DateTime);
            sb.Insert(33, hostOrder.DateTime);
            sb.Insert(42, hostOrder.PartNum.PadRight(35));
            sb.Insert(78, reqQty);
            sb.Insert(88, hostOrder.Qty.PadLeft(9, '0'));
            sb.Insert(98, time);
            sb.Insert(104, upCode);
            sb.Insert(107, hostOrder.EmpId == null ? @"Unknown" : hostOrder.EmpId.PadRight(10, ' '));
            sb.Insert(118, loc);
            sb.Length = 127;
            return sb.ToString();
        }

        private string GetUploadFileName()
        {
            string d = DateTime.Now.ToString("yyyyMMdd_HHmmss"); // case sensitive
            string fileName = string.Format("{0}{1}.dat", "upload_", d);
            return fileName;
        }

        private string GetCsvString(HostOrder hostOrder)
        {
            _ = _logger.LogDetailAsync($"716 GetCsvString hostOrder: {hostOrder.JobNum}");
            _ = _logger.LogDetailAsync($"717 GetCsvString Order.NewBin: {hostOrder.NewBin}");
            _ = _logger.LogDetailAsync($"718 Get CSV String Company Code: {_neutronLicense.CompanyCode}");
            string operation = string.Empty;
            var sb = new StringBuilder();


            if (_neutronVariables.UsePr1StyleOutputProcessor)
            {
                _ = _logger.LogDetailAsync($"725 NOVA/SFH - UsePr1Processor: true");
                //Saint Francis Hospital
                string loc = @"        ";
                if (hostOrder.OrderDetail.OrderDetailInfo.Length >= 8)
                {
                    loc = hostOrder.OrderDetail.OrderDetailInfo.Substring(startIndex: 0, length: 9);
                }
                string reqQty = hostOrder.OrderDetail.Quantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');


                sb.Length = 127;
                sb.Insert(index: 0, value: "3O");
                sb.Insert(index: 2, value: hostOrder.JobNum.PadRight(10));
                sb.Insert(index: 13, value: hostOrder.OrderDetail.Order.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 24, value: hostOrder.DateTime);
                sb.Insert(index: 33, value: hostOrder.OrderDetail.DateTime);
                sb.Insert(index: 42, value: hostOrder.PartNum.PadRight(totalWidth: 35));
                sb.Insert(index: 78, value: reqQty);
                sb.Insert(index: 88, value: hostOrder.Qty.PadLeft(9, paddingChar: '0'));
                sb.Insert(index: 98, value: "07:14");
                sb.Insert(index: 104, value: "02");
                sb.Insert(index: 107, value: hostOrder.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 118, value: loc);
            }
            else if (_neutronLicense.CompanyCode == "TMG")
            {
                //TMG
                _ = _logger.LogDetailAsync($"752 GetCsvString TMG - NOT use PR1 Processor");
                _ = _logger.LogDetailAsync($"753 GetCsvString Order.NewBin: {hostOrder.NewBin}");
                if (hostOrder.NewBin != null)
                {
                    string[] fields = hostOrder.NewBin.Split(separator: new char[] { '|' });
                    operation = fields.Length == 3 ? fields[2].ToString() : string.Empty;
                }
                sb.Append(hostOrder.JobNum + _neutronVariables.FieldDelimiter);
                sb.Append(operation + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PartNum + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.Qty + _neutronVariables.FieldDelimiter);
            }
            else if (_neutronLicense.CompanyCode == "TOP")
            {
                sb.Append(hostOrder.TypeCode + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PartNum + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PartDesc + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.JobNum + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PrimeBin + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.NewBin + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.Qty + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.TroubleBit + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.DateTime + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.EmpId);
                sb.AppendLine();
            }
            else if (_neutronLicense.CompanyCode == "VID")
            {
                sb.Append(hostOrder.TypeCode + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PartNum + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PartDesc + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.JobNum + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.PrimeBin + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.NewBin + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.Qty + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.TroubleBit + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.DateTime + _neutronVariables.FieldDelimiter);
                sb.Append(hostOrder.EmpId);
                sb.AppendLine();
            }

            _ = _logger.LogDetailAsync($"779 GetCsvString result: {sb.ToString()}");
            return sb.ToString();
        }

        private string GetHeading()
        {
            var sb = new StringBuilder();
            sb.Append(value: "Type Code|");
            sb.Append(value: "Part Num|");
            sb.Append(value: "Part Desc|");
            sb.Append(value: "Job Num|");
            sb.Append(value: "Prime Bin|");
            sb.Append(value: "NewBin|");
            sb.Append(value: "Qty|");
            sb.Append(value: "Trouble Bit|");
            sb.Append(value: "Date Time|");
            sb.Append(value: "Emp Id");
            sb.AppendLine();
            return sb.ToString();
        }


        private string GetCsvString(ReplenHostOrder order)
        {
            string operation = string.Empty;
            var sb = new StringBuilder();
            _ = _logger.LogDetailAsync($"805 GetCsvString ReplenHostOrder: {order.JobNum}");

            if (_usePr1Processor)
            {
                //Saint Francis Hospital
                _ = _logger.LogDetailAsync($"810 SFH - ReplenHostOrder UsePr1Processor: true");
                string loc = @"        ";
                if (order.OrderDetail.OrderDetailInfo.Length >= 8)
                {
                    loc = order.OrderDetail.OrderDetailInfo.Substring(startIndex: 0, length: 9);
                }
                string reqQty = order.OrderDetail.Quantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');

                sb.Length = 127;
                sb.Insert(index: 0, value: "3O");
                sb.Insert(index: 2, value: order.JobNum.PadRight(totalWidth: 10));
                sb.Insert(index: 13, value: order.OrderDetail.ReplenOrder.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 24, value: order.DateTime);
                sb.Insert(index: 33, value: order.OrderDetail.DateTime);
                sb.Insert(index: 42, value: order.PartNum.PadRight(totalWidth: 35));
                sb.Insert(index: 78, value: reqQty);
                sb.Insert(index: 88, value: order.Qty.PadLeft(totalWidth: 9, paddingChar: '0'));
                sb.Insert(index: 98, value: "07:14");
                sb.Insert(index: 104, value: "02");
                sb.Insert(index: 107, value: order.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
                sb.Insert(index: 118, value: loc);
            }
            else
            {
                //TMG
                _ = _logger.LogDetailAsync($"835 GetCsvString ReplenHostOrder TMG - NOT use PR1 Processor");
                _ = _logger.LogDetailAsync($"836 GetCsvString Order.NewBin: {order.NewBin}");
                if (order.NewBin != null)
                {
                    string[] fields = order.NewBin.Split(separator: new char[] { '|' });
                    operation = fields.Length == 3 ? fields[2].ToString() : string.Empty;
                }
                sb.Append(order.JobNum + _neutronVariables.FieldDelimiter);
                sb.Append(operation + _neutronVariables.FieldDelimiter);
                sb.Append(order.PartNum + _neutronVariables.FieldDelimiter);
                sb.Append(order.Qty + _neutronVariables.FieldDelimiter);
            }
            _ = _logger.LogDetailAsync($"847 GetCsvString ReplenHostOrder result: {sb.ToString()}");
            return sb.ToString();
        }

        private void SaveUploadDatFile(ReplenHostOrder order)
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            string fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                string fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);

                DateTime d = DateTime.Now;
                string date = d.ToString(format: "MM-dd-yyyy");
                string time = d.ToString(format: "HH:mm");
                string timeSec = d.ToString(format: "HH:mm:ss");
                var begin = new StringBuilder(capacity: 100);
                begin.Insert(index: 0, value: $"BEGIN    {date}     {timeSec}");
                var end = new StringBuilder(capacity: 100);
                end.Insert(index: 0, value: $"END");
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        tw.WriteLine(begin.ToString());
                        ReplenHostOrder endOfOrder = null;
                        endOfOrder = order;
                        tw.WriteLine(GetUploadDatRecord(order));

                        if (endOfOrder != null)
                        {
                            int status = _repoReplenOrders.FindByKey(endOfOrder.OrderDetail.ReplenOrder.Id).OrderStatusId;
                            if (status == 6)
                            {
                                tw.WriteLine(GetEndOfOrderRecord(endOfOrder));
                            }
                        }
                        tw.WriteLine(end.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync($"892 Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    //MessageBox.Show($"Save Upload Dat File Error. [Replen] {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _ = _logger.LogDetailAsync(@"898 Go to Options and enter an upload file name.");
                MessageBox.Show(@"Go to Options and enter an upload file name.");
            }
        }

    }
}
