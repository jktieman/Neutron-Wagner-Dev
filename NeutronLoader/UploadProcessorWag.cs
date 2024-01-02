using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
       // private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly IDynamicLogger _logger;
       // private readonly WorkstationView _workstationView;
       // private readonly IWorkstationRepository _workstationRepository;
        private Timer _timer;
        private bool _uploadBusy;
       // private DirectoryInfo _hostUploadDirectory;

        //public UploadProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
        //    IDynamicLogger logger, WorkstationView workstationView, IWorkstationRepository workstationRepository)
        public UploadProcessorWAG(NeutronVariables neutronVariables, IDynamicLogger logger)
        {
           // _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
           // _workstationView = workstationView;
           // _workstationRepository = workstationRepository;
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
        /// <summary>
        /// Creates a host file for the upload process.
        /// </summary>
        /// <remarks>
        /// This method initiates the upload process by creating a host file. It first checks if there are any action codes defined.
        /// If there are no action codes, it logs a message and returns. If there are action codes, it sets the upload process to busy.
        /// It then tries to retrieve a list of history records from the database that have not been transmitted yet and that contain the defined action codes.
        /// If there are records, it processes each record by creating a new NOVA_OUTPUT record with the information from the corresponding NOVA_INPUT record.
        /// It also sets the beginning quantity and the ending quantity for each record and sets the PROCESSED field to "N".
        /// After processing all records, it sets the upload process to not busy.
        /// If an exception occurs during the process, it logs the exception message and sets the upload process to not busy.
        /// </remarks>
        public void CreateHostFile()
        {
            
            if (_uploadBusy)
            {
                _ = _logger.LogDetailAsync($"Upload currently busy.  Exiting CreateHostFile");
                return;
            }
            
            _ = _logger.LogDetailAsync("CreateHostFile Start");
            // <summary>
            // The list of action codes derived from the NeutronVariables.ActionCodes property.
            // </summary>
            // <remarks>
            // The action codes are used to filter the history records that need to be processed during the upload. 
            // They are obtained by splitting the NeutronVariables.ActionCodes string by comma and parsing each resulting string to an integer.
            // </remarks>
            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            if (actionCodes.Count == 0)
            {
                _ = Task.Run(() => _logger.LogDetailAsync($"No Upload Codes Defined.  ActionCodes: {actionCodes}"));
                return;
            }
            
            _uploadBusy = true;

            try
            {
                List<History> recs;
                using (var db = new NeutronDb())
                {
                    recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode))
                        .ToList();
                }

                if (recs.Any())
                {
                    //var hostFile = new HostFilePr1(_neutronLicense, _neutronVariables, _workstationRepository);
                    //var result = hostFile.CreateHostFile(recs);
                    // for each record, get the TransId from the OrderDetailInfo field (rec.OrderDetailInfo)
                    // then get the NOVA_INPUT record with that TransId
                    // then create a new NOVA_OUTPUT record with the information from the NOVA_INPUT record
                    // add the beginning quantity, BEGINNINGQTY = (rec.RequestedQuantity) 
                    // and the ending quantity QTY = (rec.IssuedQuantity)
                    // then set the NOVA_OUTPUT record to PROCESSED = "N"  (the SAP process will set it to "Y" when it is processed)

                    //using (var db = new WagnerDb())
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
                                output.BP = input.BP;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $@"Upload Process Failed, see Log file in UploadManager. {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                _ = _logger.LogDetailAsync(
                    $"Upload Process Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            finally
            {
                _uploadBusy = false;
            }

            _uploadBusy = false;
        }
    }
}
