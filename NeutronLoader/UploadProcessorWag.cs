using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using SAPServer;
using SAPServer.Models;
using static System.Int32;
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    // ReSharper disable once InconsistentNaming
    public class UploadProcessorWAG : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly IDynamicLogger _logger;
        private readonly WorkstationView _workstationView;
        private readonly IWorkstationRepository _workstationRepository;
        private Timer _timer;
        private bool _uploadBusy;
        private DirectoryInfo _hostUploadDirectory;

        public UploadProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IDynamicLogger logger, WorkstationView workstationView, IWorkstationRepository workstationRepository)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _workstationView = workstationView;
            _workstationRepository = workstationRepository;
        }

        public void RunUploadOnce()
        {
            _ = _logger.LogDetailAsync("RunUploadOnce Start");

            CreateHostFile();
        }

        public void StartProcessingUploadFiles()
        {
            _ = _logger.LogDetailAsync("StartProcessingUploadFiles Start");
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _timer = new Timer(t => { CreateHostFile(); }, null, startTimeSpan, periodTimeSpan);
        }

        public void StopProcessingUploadFiles()
        {
            _ = _logger.LogDetailAsync("StopProcessingUploadFiles Dispose of Timer");
            _timer.Dispose();
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

        public void CreateHostFile()
        {
            _ = _logger.LogDetailAsync("CreateHostFile Start");

            //var appendFile = Convert.ToBoolean(LoaderSettings.AppendFile);
            //_hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            //if (!Directory.Exists(_hostUploadDirectory.FullName))
            //{
            //    Directory.CreateDirectory(_hostUploadDirectory.FullName);
            //}
            //var fileName = LoaderSettings.GetHostUploadFile();
            //if (!string.IsNullOrEmpty(fileName))
            //{
            //    var fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);
            //    _ = _logger.LogDetailAsync($"CreateHostFile: {fullName}");
            //    if (File.Exists(fullName))
            //    {
            //        if (!appendFile)
            //        {
            //            MessageBox.Show(@"Upload file exists.  Append file set to False.", @"File Exists", MessageBoxButtons.OK,
            //                MessageBoxIcon.Information);
            //            _ = _logger.LogDetailAsync($"Upload file exists: {fullName}    Append file set to False.");
            //            return;
            //        }
            //    }
            //}

            //var counter = 0;
            //while (_uploadBusy)
            //{
            //    Task.Delay(200);
            //    ++counter;
            //    if (counter >= 20) return;
            //}

            _uploadBusy = true;
            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                List<History> recs;
                using (var db = new NeutronDb())
                {
                    recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode))
                        .ToList();
                }

                if (recs.Count > 0)
                {
                    //var hostFile = new HostFilePr1(_neutronLicense, _neutronVariables, _workstationRepository);
                    //var result = hostFile.CreateHostFile(recs);
                    // for each record, get the TransId from the OrderDetailInfo field (rec.OrderDetailInfo)
                    // then get the NOVA_INPUT record with that TransId
                    // then create a new NOVA_OUTPUT record with the information from the NOVA_INPUT record
                    // add the beginning quantity, BEGINNINGQTY = (rec.RequestedQuantity) 
                    // and the ending quantity QTY = (rec.IssuedQuantity)
                    // then set the NOVA_OUTPUT record to PROCESSED = "N"  (the SAP process will set it to "Y" when it is processed)

                    // using (var wagDb = new WagnerDb())
                    using (var db = new NeutronDb())
                    {
                        foreach (var rec in recs)
                        {
                            var history = db.History.FirstOrDefault(h => h.Id == rec.Id);
                            if (history == null) continue;
                            
                            if (string.IsNullOrEmpty(history.OrderDetailInfo)) continue;
                            var orderDetailInfo = history.OrderDetailInfo.Split('|');
                            var transId = orderDetailInfo[0];
                            // convert transId to decimal
                            var transIdDec = Convert.ToDecimal(transId);

                            var input = db.NOVA_INPUT.FirstOrDefault(n => n.TRANSID == transIdDec);
                            if (input != null)
                            {
                                var output = new NOVA_OUTPUT();
                                output.TRANSID = input.TRANSID;
                                output.TASKNO = input.TASKNO;
                                output.TOTENO = input.TOTENO;
                                output.SKU = input.SKU;
                                output.BEGINNINGQTY = rec.RequestedQuantity;
                                output.QTY = rec.IssuedQuantity;
                                output.TRANSDATE = rec.ActionDateTime;
                                output.TRANSTYPE = input.TRANSTYPE;
                                output.PROCESSED = "N";
                                output.EXPLANATION = string.Empty;
                                db.NOVA_OUTPUT.Add(output);
                                db.SaveChanges();
                            }
                            history.TransmitDateTime = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                }

                //if (result)
                //{
                //foreach (var rec in recs)
                //{

                //}

                //}
                //else
                //{
                //    MessageBox.Show(@"Create Host File Failed, see Log file in UploadManager.");
                //    _ = _logger.LogDetailAsync($"Create Host File Failed, see Log file in UploadManager.");
                //}
                //}
                // }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Upload Process Failed, see Log file in UploadManager. {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                _ = _logger.LogDetailAsync($"Upload Process Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _uploadBusy = false;
        }
    }
}
