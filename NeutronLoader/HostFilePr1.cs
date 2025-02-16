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
using System.Threading.Tasks;
using NeutronData.Interfaces;
using AsyncAwaitBestPractices;


namespace NeutronLoader
{
    public class HostFilePr1
    {
        private readonly DirectoryInfo _hostUploadDirectory;
        private bool _usePr1Processor;
        private readonly GenericRepository<User> _repoUser;
        private readonly IWorkstationRepository _workstationRepository;
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
       private readonly Workstation _workStation;
        private readonly IDynamicLogger _logger;

        public HostFilePr1(NeutronLicense neutronLicense, NeutronVariables neutronVariables
            , IWorkstationRepository workstationRepository, Func<NeutronDb> contextFactory
            , Workstation workStation = null)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _workstationRepository = workstationRepository;
            _workStation = workStation;
            LoaderSettings.Init();
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            _logger = NeutronCore.Global.Logger.SetupLogger("HostFile");
            _repoUser = new GenericRepository<User>(contextFactory);
        }

        public async Task<bool> CreateHostFile(List<History> historyRecs)
        {
            _logger.LogDetailAsync($"CreateHostFile with HistoryRecs - Start").SafeFireAndForget();
            _usePr1Processor = _neutronVariables.UsePr1StyleOutputProcessor;

            if (_hostUploadDirectory == null) return false;
            if (_usePr1Processor)
            {
                _logger.LogDetailAsync($"HostFile List<History> Calling SaveUploadDatFile HistoryRecs").SafeFireAndForget();
                if (await SaveUploadDatFile(historyRecs))
                {
                    return true;
                }
            }
            else
            {
                _logger.LogDetailAsync($"HostFile List<HostOrder> Calling SaveFile groups").SafeFireAndForget();
                SaveFile(historyRecs);
                return true;
            }

            return false;
        }

        private void SaveFile(List<History> historyRecs)
        {
            historyRecs = historyRecs.OrderBy(o => o.Ord1).ThenBy(o => o.Item).ToList();
            var groups = historyRecs.GroupBy(g => g.Ord1).ToList();
            _logger.LogDetailAsync("SaveFile History Groups").SafeFireAndForget();
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            var fullName = $"{_hostUploadDirectory.FullName}{GetFileName(string.Empty)}";
            _logger.LogDetailAsync($"SaveFile History Groups: {fullName}").SafeFireAndForget();
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
                _logger.LogDetailAsync($"Save History Group File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }

        }


        private string GetCsvString(History item)
        {
            _logger.LogDetailAsync($"GetCsvString history: {item.Ord1}").SafeFireAndForget();
            _logger.LogDetailAsync($"History Get CSV String Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();
            var costcenter = "";
            var sb = new StringBuilder();

            var loc = @"        ";
            if (item.OrderDetailInfo.Length >= 8)
            {
                loc = item.OrderDetailInfo.Substring(startIndex: 0, length: 9);
            }
            var reqQty = item.RequestedQuantity.ToString().PadLeft(totalWidth: 9, paddingChar: '0');

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


            _logger.LogDetailAsync($"173 GetCsvString History result: {sb.ToString()}").SafeFireAndForget();
            return sb.ToString();
        }
        private async Task<bool> SaveUploadDatFile(List<History> historyRecs)
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
                _logger.LogDetailAsync($"FullName: {fullName}").SafeFireAndForget();
                try
                {
                    using (var tw = new StreamWriter(fullName, append: true))
                    {
                        foreach (var history in historyRecs)
                        {
                           await tw.WriteLineAsync(GetUploadDatRecord(history));
                        }
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($"Save Upload Dat File Error. {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
                }
            }
            else
            {
                _logger.LogDetailAsync($"Go to Options and enter an upload file name.").SafeFireAndForget();
            }

            return result;
        }

        private string GetUploadDatRecord(History history)
        {
            _logger.LogDetailAsync($"History Record- Item: {history.Item}").SafeFireAndForget();
            var result = string.Empty;
            try
            {
                _logger.LogDetailAsync($"Get Upload Dat Record - Begin Try").SafeFireAndForget();
                var pickStation = _workstationRepository.GetStation(history.WorkstationId);
               
                if (pickStation == null)
                {
                    _logger.LogDetailAsync($"Get Upload Dat Record - PickStation NULL").SafeFireAndForget();
                    return result;
                }
                _logger.LogDetailAsync($"Get Upload Dat Record - Valid PickStation").SafeFireAndForget();

                var station = pickStation.StationNumber.ToString().PadRight(2,'0');
                _logger.LogDetailAsync($"Get Upload Dat Record - Station Number: {station}").SafeFireAndForget();
                
                var order = history.Ord1 == null ? string.Empty.PadRight(10,' ') : history.Ord1.PadRight(10,' ');
                _logger.LogDetailAsync($"Get Upload Dat Record Order: {order}").SafeFireAndForget();
                
                var invoice = history.Ord2 == null ? string.Empty.PadRight(10, ' ') : history.Ord2.PadRight(10, ' ');
                _logger.LogDetailAsync($"Get Upload Dat Record Invoice: {invoice}").SafeFireAndForget();
                
                var costCenter = history.CostCenter.Trim().PadLeft(10,' ') ?? string.Empty;
                _logger.LogDetailAsync($"Get Upload Dat Record CostCenter {costCenter}").SafeFireAndForget();
                
                var orderDetailInfo = $"{costCenter}{history.OrderDetailInfo}"; // ?? string.Empty;
                // Rightmost 24 characters of the OrderDetailInfo field
               // var info = history.OrderDetailInfo ?? string.Empty;
                _logger.LogDetailAsync($"Get Upload Dat Record CostCenter and OrderDetailInfo: {orderDetailInfo}").SafeFireAndForget();
                
                //if (!string.IsNullOrEmpty(info))
                //{
                //    _logger.LogDetailAsync($"Get Upload Dat Record: There is Info").SafeFireAndForget();
                //    if (info.Length >= 24)
                //    {
                //        _logger.LogDetailAsync($"Get Upload Dat Record: Length of Info >= 24").SafeFireAndForget();
                //        orderDetailInfo = info.Substring(info.Length - 24);
                //        _logger.LogDetailAsync($"Get Upload Dat Record: Get Substring of Info. [info.Length - 24]").SafeFireAndForget();
                //    }
                //    _logger.LogDetailAsync($"Get Upload Dat Record OrderDetailInfo: {orderDetailInfo}").SafeFireAndForget();
                //}
                var empName = string.Empty;
                if (history.EmpId != null)
                {
                 _logger.LogDetailAsync($"Get Upload Dat Record EmpId: {history.EmpId}").SafeFireAndForget();
                   
                    var emp = _repoUser.FindBy(u => u.EmpId == history.EmpId).FirstOrDefault();
                   
                    if (emp != null)
                    {
                        empName = emp.Fullname.Length < 11 ?  $"{emp.Fullname}" : $"{emp.Fullname.Substring(0,10)}";
                        _logger.LogDetailAsync($"Get Upload Dat Record empName: {empName}").SafeFireAndForget();
                    }
                }

                var upCode = "02";
                var time = DateTime.Now.ToString(format: "HH:mm");

                var sb = new StringBuilder(new string(' ', 517));
                sb.Insert(0, $"{station}");
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
                sb.Insert(118, orderDetailInfo);
                sb.Length = 517;

                result = sb.ToString();
                _logger.LogDetailAsync($"{result}").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                    _logger.LogDetailAsync($"Get Upload Dat Record - Item: {history.Item}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.InnerException}").SafeFireAndForget();
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
            _logger.LogDetailAsync($"Get File Name Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();
            string result;
            if (_neutronLicense.CompanyCode == "TOP")
            {
                var d = DateTime.Now.ToString(format: "yyyyMMddHHmmssfff"); // case sensitive
                result = ($"{hostOrderTypeCode}{d}.CSV");
            }
            else
            {
                var fileName = LoaderSettings.GetHostUploadFile();
                if (!string.IsNullOrEmpty(fileName.Trim()))
                {
                    result = fileName;
                }
                else  //nothing in FileName field so create unique
                {
                    var d = DateTime.Now.ToString(format: "yyyyMMddHHmmssfff");
                    result = $"{hostOrderTypeCode}{d}_Upload.dat";
                }
            }
            return result;
        }
    }
}
