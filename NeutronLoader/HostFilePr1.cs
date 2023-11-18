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
using NeutronData.Interfaces;


namespace NeutronLoader
{
    public class HostFilePr1
    {
        private readonly DirectoryInfo _hostUploadDirectory;
        private bool _usePr1Processor;
        private readonly GenericRepository<Order> _repoOrders = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrders = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<User> _repoUser = new GenericRepository<User>(new NeutronDb());
        private readonly IWorkstationRepository _workstationRepository;
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
       // private readonly IWorkstationAreaRepository _workstationAreaRepository;
        private readonly Workstation _rackStation;
        private readonly int _rackStationId;
        private readonly IDynamicLogger _logger;

        public HostFilePr1(NeutronLicense neutronLicense, NeutronVariables neutronVariables
            , IWorkstationRepository workstationRepository , Workstation rackStation = null)
        {
            //_workstationRepository = workstationRepository;
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _workstationRepository = workstationRepository;
            _rackStation = rackStation;
            LoaderSettings.Init();
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            _logger = NeutronCore.Global.Logger.SetupLogger("HostFile");
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
            //_workstationRepository = new WorkstationRepository(_logger, _workstationAreaRepository, _blastzone, _prolite);

            _ = _logger.LogDetailAsync($"Rack Station is NULL: {rackStationIsNull}  Rack Station Id: {_rackStationId}");
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
            }

        }


        private string GetCsvString(History item)
        {
            _ = _logger.LogDetailAsync($"111 GetCsvString history: {item.Ord1}");
            _ = _logger.LogDetailAsync($"113 History Get CSV String Company Code: {_neutronLicense.CompanyCode}");
            var costcenter = "";
            var sb = new StringBuilder();

            var loc = @"        ";
            if (item.OrderDetailInfo.Length >= 8)
            {
                loc = item.OrderDetailInfo.Substring(startIndex: 0, length: 9);
            }
            string reqQty = item.RequestedQuantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');

            if (string.IsNullOrWhiteSpace(item.OrderDetailInfo))
            {
                if (string.IsNullOrWhiteSpace(item.CostCenter))
                {
                    costcenter = item.Ord1;
                }
                else
                {
                    costcenter = item.CostCenter;
                }
            }
            else
            {
                costcenter = item.OrderDetailInfo;
            }



            var sku = item.Item;
            var issuedQty = item.IssuedQuantity;

            sb.Append($" ,{costcenter},{sku}, ,{issuedQty}, , , , ,0");

            //sb.Length = 127;
            //sb.Insert(index: 0, value: "3O");
            //sb.Insert(index: 2, value: item.Ord1.PadRight(10));
            //sb.Insert(index: 13, value: item.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
            //sb.Insert(index: 24, value: item.ActionDateTime);
            //sb.Insert(index: 33, value: item.LoadDate);
            //sb.Insert(index: 42, value: item.Item.PadRight(totalWidth: 35));
            //sb.Insert(index: 78, value: reqQty);
            //sb.Insert(index: 88, value: item.IssuedQuantity.ToString().PadLeft(9, paddingChar: '0'));
            //sb.Insert(index: 98, value: "07:14");
            //sb.Insert(index: 104, value: "02");
            //sb.Insert(index: 107, value: item.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
            //sb.Insert(index: 118, value: loc);


            _ = _logger.LogDetailAsync($"173 GetCsvString History result: {sb.ToString()}");
            return sb.ToString();
        }

        //private string GetCsvString(History item)
        //{
        //    _ = _logger.LogDetailAsync($"111 GetCsvString history: {item.Ord1}");
        //    _ = _logger.LogDetailAsync($"113 History Get CSV String Company Code: {_neutronLicense.CompanyCode}");

        //    var sb = new StringBuilder();

        //    if (_neutronVariables.UsePr1StyleOutputProcessor)
        //    {
        //        _ = _logger.LogDetailAsync($"119  UsePr1Processor: true");

        //        var loc = @"        ";
        //        if (item.OrderDetailInfo.Length >= 8)
        //        {
        //            loc = item.OrderDetailInfo.Substring(startIndex: 0, length: 9);
        //        }
        //        string reqQty = item.RequestedQuantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');


        //        sb.Length = 127;
        //        sb.Insert(index: 0, value: "3O");
        //        sb.Insert(index: 2, value: item.Ord1.PadRight(10));
        //        sb.Insert(index: 13, value: item.Ord2.PadRight(totalWidth: 10, paddingChar: ' '));
        //        sb.Insert(index: 24, value: item.ActionDateTime);
        //        sb.Insert(index: 33, value: item.LoadDate);
        //        sb.Insert(index: 42, value: item.Item.PadRight(totalWidth: 35));
        //        sb.Insert(index: 78, value: reqQty);
        //        sb.Insert(index: 88, value: item.IssuedQuantity.ToString().PadLeft(9, paddingChar: '0'));
        //        sb.Insert(index: 98, value: "07:14");
        //        sb.Insert(index: 104, value: "02");
        //        sb.Insert(index: 107, value: item.EmpId.PadRight(totalWidth: 10, paddingChar: ' '));
        //        sb.Insert(index: 118, value: loc);
        //    }

        //    _ = _logger.LogDetailAsync($"173 GetCsvString History result: {sb.ToString()}");
        //    return sb.ToString();
        //}

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
                    _ = _logger.LogDetailAsync($"211 Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}");
                }
            }
            else
            {
                _ = _logger.LogDetailAsync($"221 Go to Options and enter an upload file name.");
            }

            return result;
        }

        private string GetUploadDatRecord(History history)
        {
            _ = _logger.LogDetailAsync($"179 History Record- Item: {history.Item}");
            var result = string.Empty;
            try
            {
                string invoice;
                string order;
                string costCenter;
                string info;
                string empName;

                _ = _logger.LogDetailAsync($"Get Upload Dat Record - Begin Try");
                var stat = _workstationRepository.GetStation(history.AreaId);
                _ = _logger.LogDetailAsync($"Get Upload Dat Record - 1");
                // if the workstationId is the same as the Rack Station Id (8 is the normal Rack Id)
                // then change the StationId that goes back to Saint Francis to 9 instead of 8
                // otherwise just use the workstationId
                var station = stat.Id == _rackStationId ? "9" : stat.StationNumber.ToString();
                _ = _logger.LogDetailAsync($"Get Upload Dat Record - 2");
                order = history.Ord1 == null ? string.Empty.PadRight(10) : history.Ord1.PadRight(10);

                _ = _logger.LogDetailAsync($"Get Upload Dat Record Order: {order} - 3");
                invoice = history.Ord2 == null ? string.Empty.PadRight(10) : history.Ord2.PadRight(10);
                _ = _logger.LogDetailAsync($"Get Upload Dat Record Invoice: {invoice} - 3");
                costCenter = history.CostCenter ?? string.Empty;
                _ = _logger.LogDetailAsync($"Get Upload Dat Record CostCenter {costCenter} - 4");
                var orderDetailInfo = string.Empty;
                // Rightmost 24 characters of the OrderDetailInfo field
                info = history.OrderDetailInfo ?? string.Empty;
                _ = _logger.LogDetailAsync($"Get Upload Dat Record Info {info} - 5");
                if (!string.IsNullOrEmpty(info))
                {
                    _ = _logger.LogDetailAsync($"Get Upload Dat Record - 6");
                    if (info.Length >= 24)
                    {
                        _ = _logger.LogDetailAsync($"Get Upload Dat Record - 7");
                        orderDetailInfo = info.Substring(info.Length - 24);
                        _ = _logger.LogDetailAsync($"Get Upload Dat Record - 8");
                    }
                    _ = _logger.LogDetailAsync($"Get Upload Dat Record OrderDetailInfo {orderDetailInfo} - 9");
                }
                _ = _logger.LogDetailAsync($"Get Upload Dat Record - 10");
                empName = string.Empty;
                if (history.EmpId != null)
                {
                    var emp = _repoUser.FindBy(u => u.EmpId == history.EmpId).FirstOrDefault();
                    _ = _logger.LogDetailAsync($"Get Upload Dat Record - 11");
                    if (emp != null)
                    {
                        _ = _logger.LogDetailAsync($"Get Upload Dat Record - 12");
                        empName = emp.Firstname.PadRight(10);
                    }
                }

                var upCode = ($"02");
                var time = DateTime.Now.ToString(format: "HH:mm");

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
                sb.Insert(130, orderDetailInfo);
                sb.Length = 154;

                result = sb.ToString();
                _ = _logger.LogDetailAsync($"{result}");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"227 Get Upload Dat Record - Item: {history.Item}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}");
            }

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
    }
}
