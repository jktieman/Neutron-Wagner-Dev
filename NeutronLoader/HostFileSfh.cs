using AlliedLogger;
using NeutronCore;
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
using System.Windows.Forms;
using NeutronData.Interfaces;

namespace NeutronLoader
{
    public class HostFileSfh
    {
        private readonly DirectoryInfo _hostUploadDirectory;
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<User> _repoUser = new GenericRepository<User>(new NeutronDb());
        private readonly IStationRepository _stationRepository;
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly Station _rackStation;
        private readonly DynamicLogger _logger;
        private readonly int _rackStationId;

        public HostFileSfh( NeutronLicense neutronLicense, NeutronVariables neutronVariables, Station rackStation = null)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _rackStation = rackStation;
            LoaderSettings.Init();
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = @"HostFile";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);

            var rackStationIsNull = true;
            // SAP requires a 9 for the Off Carousel station number
            if (_rackStation == null)
            {
                _rackStationId = 8;
            }
            else
            {
                rackStationIsNull = false;
                _rackStationId = _rackStation.Id;
            }
            _stationRepository = new StationRepository(_logger);
            _logger.Log($"Rack Station is NULL: {rackStationIsNull}  Rack Station Id: {_rackStationId}");
        }

        public bool CreateHostFile(List<History> historyRecs)
        {
            _logger.Log($"Create SFH HostFile with HistoryRecs");
            if (_hostUploadDirectory == null) return false;
            _logger.Log($"HostFile List<History> Calling SaveUploadDatFile HistoryRecs");
            return SaveUploadDatFile(historyRecs);
        }

        private bool SaveUploadDatFile(List<History> historyRecs)
        {
            _logger.Log($"History Record Count: {historyRecs.Count}");
            var result = false;
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            var fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                var fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);
                _logger.Log($"FullName: {fullName}");
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        _logger.Log($"Stream Writer: {tw.ToString()}");
                        foreach (var history in historyRecs)
                        {
                            _logger.Log($"Stream Writer - Starting For Loop: {tw.ToString()}");
                            var rec = GetUploadDatRecord(history);
                            _logger.Log($"Rec = {rec}");
                            if (rec != null) tw.WriteLine(rec);
                        }
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Save Upload Dat File Error: {ex.Message}{Environment.NewLine}" +
                                    $"{ex.InnerException.Message}{Environment.NewLine}{ex.StackTrace}");
                    _logger.Log($"Save Upload Dat File Error: {ex.Message}{Environment.NewLine}" +
                                $"{ex.InnerException.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            else
            {
                _logger.Log($"File Name is Empty.  Go to Options and enter an upload file name.");
            }

            return result;
        }

        // Saint Francis Upload Format
        public string GetUploadDatRecord(History history)
        {
            _logger.Log($"Get Upload Dat Record - START");
            var result = string.Empty;
            if (history == null)
            {
                _logger.Log($"Get Upload Dat Error: History is NULL");
                return result;
            }
            try
            {
                string invoice;
                string order;
                string costCenter;
                string info;
                string empName;

                _logger.Log($"Get Upload Dat Record - Begin Try");
                var stat = _stationRepository.GetStation(history.StationId);
                _logger.Log($"Get Upload Dat Record - 1");
                var station = stat.Id == _rackStationId ? "9" : stat.StationNumber.ToString();
                _logger.Log($"Get Upload Dat Record - Station Number: {station}");

                order = history.Ord1 == null ? string.Empty.PadRight(10) : history.Ord1.PadRight(10);
                _logger.Log($"Get Upload Dat Record - Order - Check For Hot Pick");
                order = CheckForHot(order);
                _logger.Log($"Get Upload Dat Record Order: {order} - 3");

                invoice = history.Ord2 == null ? string.Empty.PadRight(10) : history.Ord2.PadRight(10);
                _logger.Log($"Get Upload Dat Record - Invoice - Check For Hot Pick");
                invoice = CheckForHot(invoice);
                _logger.Log($"Get Upload Dat Record Invoice: {invoice} - 3");


                costCenter = history.CostCenter ?? string.Empty;
                _logger.Log($"Get Upload Dat Record CostCenter {costCenter} - 4");
                var orderDetailInfo = string.Empty;
                // Rightmost 24 characters of the OrderDetailInfo field
                info = history.OrderDetailInfo ?? string.Empty;
                _logger.Log($"Get Upload Dat Record Info {info} - 5");
                if (!string.IsNullOrEmpty(info))
                {
                    _logger.Log($"Get Upload Dat Record - 6");
                    if (info.Length >= 24)
                    {
                        _logger.Log($"Get Upload Dat Record - 7");
                        orderDetailInfo = info.Substring(info.Length - 24);
                        _logger.Log($"Get Upload Dat Record - 8");
                    }
                    _logger.Log($"Get Upload Dat Record OrderDetailInfo {orderDetailInfo} - 9");
                }
                _logger.Log($"Get Upload Dat Record - 10");
                empName = string.Empty;
                if (history.EmpId != null)
                {
                    var emp = _repoUser.FindBy(u => u.EmpId == history.EmpId).FirstOrDefault();
                    _logger.Log($"Get Upload Dat Record - 11");
                    if (emp != null)
                    {
                        _logger.Log($"Get Upload Dat Record - 12");
                        empName = emp.Firstname.PadRight(10);
                    }
                }

                _logger.Log($"Get Upload Dat Record - 13");
                var upCode = ($"02");
                var time = DateTime.Now.ToString(format: "HH:mm");

                _logger.Log($"Get Upload Dat Record - Begin StringBuilder 14");
                var sb = new StringBuilder(new string(' ', 170));
                sb.Insert(0, $"{station}O");
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
                sb.Insert(129, station);
                sb.Insert(130, orderDetailInfo);
                sb.Length = 154;

                result = sb.ToString();

            }
            catch (Exception ex)
            {
                _logger.Log($"Get Upload Dat Record Error: {ex.Message}{Environment.NewLine}" +
                            $"{ex.InnerException.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            _logger.Log($"Get Upload Dat Record - END  Result:{result}");
            return result;
        }

        private string CheckForHot(string order)
        {
            switch (order)
            {
                case "  HOT PICK":
                    return @"HOTPICK   ";
                case " HOT STORE":
                    return @"HOTSTORE  ";
                default:
                    return order;
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
    }
}
