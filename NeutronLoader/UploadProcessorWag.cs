using AlliedLogger;
using NeutronCore.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;
using static System.Int32;
using Timer = System.Threading.Timer;
using NeutronData.Repositories;
using NeutronCore.Extensions;

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
        private readonly GenericRepository<NOVA_INPUT> _repoNovaInput = new GenericRepository<NOVA_INPUT>(new NeutronDb());
        private readonly GenericRepository<NOVA_OUTPUT> _repoNovaOutput = new GenericRepository<NOVA_OUTPUT>(new NeutronDb());
        private readonly GenericRepository<History> _repoHistory = new GenericRepository<History>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetails = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(new NeutronDb());

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
                // recs;
                //using (var db = new NeutronDb())
                //{

                var recs = _repoHistory.FindBy(h => !h.TransmitDateTime.HasValue
                                                     && actionCodes.Contains(h.ActionCode)
                    && h.RequestedQuantity == h.IssuedQuantity)
                      .ToList();

                //recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode))
                //    .ToList();
                //}

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

                    //using (var db = new NeutronDb())
                    //{
                    foreach (var history in recs)
                    {
                        // var history = db.History.FirstOrDefault(h => h.Id == rec.Id);
                        //var history =_repoHistory.FindBy(h => h.Id == rec.Id).FirstOrDefault();

                        if (history == null) continue;
                        if (string.IsNullOrEmpty(history.OrderDetailInfo)) continue;
                        var orderDetailInfo = history.OrderDetailInfo.Split('|');
                        var transId = orderDetailInfo[0];
                        // convert transId to decimal
                        var transIdDec = Convert.ToDecimal(transId);

                        var input = _repoNovaInput.FindBy(n => n.TRANSID == transIdDec).FirstOrDefault();

                        // var input = db.NOVA_INPUT.FirstOrDefault(n => n.TRANSID == transIdDec);
                        if (input != null)
                        {
                            var output = new NOVA_OUTPUT
                            {
                                TRANSID = input.TRANSID,
                                TASKNO = input.TASKNO,
                                TOTENO = input.TOTENO,
                                BP = input.BP,
                                SKU = input.SKU,
                                BEGINNINGQTY = history.RequestedQuantity,
                                QTY = history.IssuedQuantity,
                                TRANSDATE = history.ActionDateTime,
                                TRANSTYPE = input.TRANSTYPE,
                                PROCESSED = "N",
                                EXPLANATION = string.Empty
                            };
                            _repoNovaOutput.InsertAsync(output);
                        }

                        history.TransmitDateTime = DateTime.Now;
                        _repoHistory.Update(history);
                    }
                }
                // Check for orphan records
                // for each kind of pick(1) or putaway(2)
                foreach (var actionCode in actionCodes)
                {

                    if (actionCode == 1)
                    {
                        ProcessPickOrphans();
                    }
                    else if (actionCode == 2)
                    {
                        ProcessReplenOrphans();
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
        private void ProcessPickOrphans()
        {
            // Are there any History records where the RequestedQuantity and IssuedQuantity aren't the same
            var orphans = _repoHistory.FindBy(h => !h.TransmitDateTime.HasValue
                                                   && h.ActionCode == 1
                                                   && h.RequestedQuantity != h.IssuedQuantity).ToList();
            if (orphans.Any())
            {
                // get a list of the TransId's 
                // we can use it to see if the OrderDetail record is complete and use the quantities from OrderDetail
                // to send to SAP
                var transIds = new List<int>();
                foreach (var orphan in orphans)
                {
                    var transId = orphan.OrderDetailInfo.Split('|').First().ParseInt();
                    if (!transIds.Contains(transId))
                    {
                        transIds.Add(transId);
                    }
                }

                if (transIds.Any())
                {
                    foreach (var transId in transIds)
                    {
                        // look in OrderDetail to see if it is complete
                        var orderDetail = _repoOrderDetails.FindBy(od => od.TransId == transId).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            // 6 means that the line is complete
                            if (orderDetail.LineStatusId == 6)
                            {
                                // Get the information needed to send back to SAP
                                SendToSap(transId, orderDetail.Quantity, orderDetail.PickedQuantity);


                                var recs = _repoHistory
                                    .FindBy(r => r.OrderDetailInfo.StartsWith(transId.ToString())).ToList();
                                foreach (var rec in recs)
                                {
                                    rec.TransmitDateTime = DateTime.Now;
                                    _repoHistory.Update(rec);
                                }
                            }
                        }
                    }
                }
            }
        }


        private void ProcessReplenOrphans()
        {
            // Are there any History records where the RequestedQuantity and IssuedQuantity aren't the same
            var orphans = _repoHistory.FindBy(h => !h.TransmitDateTime.HasValue
                                                   && h.ActionCode == 2
                                                   && h.RequestedQuantity != h.IssuedQuantity).ToList();
            if (orphans.Any())
            {
                // get a list of the TransId's 
                // we can use it to see if the OrderDetail record is complete and use the quantities from OrderDetail
                // to send to SAP
                var transIds = new List<int>();
                foreach (var orphan in orphans)
                {
                    var transId = orphan.OrderDetailInfo.Split('|').First().ParseInt();
                    if (!transIds.Contains(transId))
                    {
                        transIds.Add(transId);
                    }
                }

                if (transIds.Any())
                {
                    foreach (var transId in transIds)
                    {
                        // look in OrderDetail to see if it is complete
                        var orderDetail = _repoReplenOrderDetails.FindBy(od => od.TransId == transId).FirstOrDefault();
                        if (orderDetail != null)
                        {
                            // 6 means that the line is complete
                            if (orderDetail.LineStatusId == 6)
                            {
                                // Get the information needed to send back to SAP
                                SendToSap(transId, orderDetail.Quantity, orderDetail.PickedQuantity);

                                var recs = _repoHistory
                                    .FindBy(r => r.OrderDetailInfo.StartsWith(transId.ToString())).ToList();
                                foreach (var rec in recs)
                                {
                                    rec.TransmitDateTime = DateTime.Now;
                                    _repoHistory.Update(rec);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendToSap(int transId, int requested, int issued)
        {
            var transIdDec = Convert.ToDecimal(transId);

            var input = _repoNovaInput.FindBy(n => n.TRANSID == transIdDec).FirstOrDefault();

            if (input != null)
            {
                var output = new NOVA_OUTPUT
                {
                    TRANSID = input.TRANSID,
                    TASKNO = input.TASKNO,
                    TOTENO = input.TOTENO,
                    BP = input.BP,
                    SKU = input.SKU,
                    BEGINNINGQTY = requested,
                    QTY = issued,
                    TRANSDATE = DateTime.Now,
                    TRANSTYPE = input.TRANSTYPE,
                    PROCESSED = "N",
                    EXPLANATION = string.Empty
                };
                _repoNovaOutput.InsertAsync(output);
            }
        }
    }
}
