using AlliedLogger;
using Microsoft.VisualBasic.FileIO;
using NeutronCore;
using NeutronCore.Models;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonManager;
using NeutronCore.Global;

namespace NeutronLoader
{
    public class FileProcessor
    {
        private readonly List<FileInfo> _files;
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        readonly DynamicLogger _logger;

        public FileProcessor(List<FileInfo> files, NeutronVariables neutronVariables
            , NeutronLicense neutronLicense, IJsonData jsonData, DynamicLogger logger)
        {
            _files = files;
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;

            foreach (var file in files)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                Task.Run(() => _logger.Log($"File Processor - Process File Start."));
                List<HostOrder> hostOrderList = ProcessFile(file);
                if (hostOrderList.Count > 0)
                {
                    var hostOrderListProcessor = new HostOrderListProcessor(hostOrderList, jsonData, logger);
                }
                stopwatch.Stop();
                Task.Run(() => _logger.Log($"Processing Time: {stopwatch.ElapsedMilliseconds.ToString()}"));
                ArchiveFile.Archive(file);
            }
        }

        private List<HostOrder> ProcessFile(FileInfo fileInfo)
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
                        string[] fields = parser.ReadFields();
                        if (fields == null || fields.Length < 10 || fields[0].ToUpper().Trim() == "WO NUMBER") continue;


                        string primeBin = GetPrimeBin(fields[8]);

                        if (_neutronVariables.LoadRackOrders)
                        {
                            if (string.IsNullOrEmpty(primeBin))
                            {
                                var hostOrder = new HostOrder()
                                {
                                    TypeCode = @"2",
                                    PartNum = fields[4],
                                    PartDesc = fields[9],
                                    JobNum = fields[1],
                                    PrimeBin = @"OFF",
                                    NewBin = ($"{fields[0]}|{fields[2]}|{fields[3]}"),
                                    Qty = fields[6],
                                    TroubleBit = @"0",
                                    DateTime = string.Empty,
                                    EmpId = string.Empty,
                                };

                                hostOrderList.Add(hostOrder);

                            }
                            else
                            {
                                var hostOrder = new HostOrder()
                                {
                                    TypeCode = @"2",
                                    PartNum = fields[4],
                                    PartDesc = fields[9],
                                    JobNum = fields[1],
                                    PrimeBin = primeBin,
                                    NewBin = ($"{fields[0]}|{fields[2]}|{fields[3]}"),
                                    Qty = fields[6],
                                    TroubleBit = @"0",
                                    DateTime = string.Empty,
                                    EmpId = string.Empty,
                                };

                                hostOrderList.Add(hostOrder);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(primeBin))
                            {
                                var hostOrder = new HostOrder()
                                {
                                    TypeCode = @"2",
                                    PartNum = fields[4],
                                    PartDesc = fields[9],
                                    JobNum = fields[1],
                                    PrimeBin = primeBin,
                                    NewBin = ($"{fields[0]}|{fields[2]}|{fields[3]}"),
                                    Qty = fields[6],
                                    TroubleBit = @"0",
                                    DateTime = string.Empty,
                                    EmpId = string.Empty
                                };

                                hostOrderList.Add(hostOrder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.InnerException != null)
                    _logger.Log("Process Interface File Error.  \r\n" + ex.Message + "\r\n" +
                               ex.InnerException.Message +
                               "\r\n" + ex.InnerException.InnerException.Message);
            }
            return hostOrderList;
        }

        private string GetPrimeBin(string v)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(v)) return result;
            if (v.Length < 5 || v.Substring(0, 1) != "V") return result;
           // var bins = v.Split(separator: new char[] { ',' });
           // result = bins[0];
            result = v.Substring(0, 5);
            return result;
        }
    }
}
