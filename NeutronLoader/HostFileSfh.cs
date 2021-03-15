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

namespace NeutronLoader
{
    public class HostFileSfh
    {
        private readonly DirectoryInfo _hostUploadDirectory;
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<User> _repoUser = new GenericRepository<User>(new NeutronDb());
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly Station _rackStation;
        private readonly DynamicLogger _logger;

        public HostFileSfh(NeutronLicense neutronLicense, NeutronVariables neutronVariables, Station rackStation)
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
                        foreach (var history in historyRecs)
                        {
                            tw.WriteLine(GetUploadDatRecord(history));
                        }
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _logger.Log($"Go to Options and enter an upload file name.");
            }

            return result;
        }

        // Saint Francis Upload Format
        private string GetUploadDatRecord(History history)
        {
            // SAP requires a 9 for the Off Carousel station number
            var station = history.StationId == _rackStation.Id ? "9" : history.StationId.ToString();
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

            var result = sb.ToString();
            _logger.Log($"{result}");
            return result;
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
