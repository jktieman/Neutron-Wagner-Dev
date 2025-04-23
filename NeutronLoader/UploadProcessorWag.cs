using AlliedLogger;
using NeutronCore.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.DataContexts;
using NeutronData.Models;
using static System.Int32;
using System.Timers;
using NeutronData.Repositories;
using NeutronCore.Extensions;
using AsyncAwaitBestPractices;
using System.Threading;

namespace NeutronLoader
{
    // ReSharper disable once InconsistentNaming
    public class UploadProcessorWAG : IUploadProcessor
    {
        private readonly NeutronVariables _neutronVariables;
        private readonly IDynamicLogger _logger;
        private System.Timers.Timer _timer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _uploadBusy;
        private readonly GenericRepository<NOVA_INPUT> _repoNovaInput;
        private readonly GenericRepository<NOVA_OUTPUT> _repoNovaOutput;
        private readonly GenericRepository<History> _repoHistory;
        private readonly GenericRepository<OrderDetail> _repoOrderDetails;
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetails;
        private readonly Func<NeutronDb> _contextFactory;

        public UploadProcessorWAG(NeutronVariables neutronVariables, IDynamicLogger logger, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _neutronVariables = neutronVariables;
            _logger = logger;
            _repoNovaInput = new GenericRepository<NOVA_INPUT>(contextFactory);
            _repoNovaOutput = new GenericRepository<NOVA_OUTPUT>(contextFactory);
            _repoHistory = new GenericRepository<History>(contextFactory);
            _repoOrderDetails = new GenericRepository<OrderDetail>(contextFactory);
            _repoReplenOrderDetails = new GenericRepository<ReplenOrderDetail>(contextFactory);

        }

        public async Task RunUploadOnce()
        {
            _logger.LogDetailAsync("RunUploadOnce Start").SafeFireAndForget();

            await CreateHostFile();
        }

        public void StartProcessingUploadFiles()
        {
            _logger.LogDetailAsync("StartProcessingUploadFiles Start").SafeFireAndForget();
           // var startTimeSpan = TimeSpan.Zero;
           // var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            //_timer = new Timer(s => { CreateHostFile().SafeFireAndForget(); }, null, startTimeSpan, periodTimeSpan);
            _timer = new System.Timers.Timer(_neutronVariables.UploadDelay * 1000);
            //_timer.Elapsed += async (sender, e) => await CreateHostFile();
            _timer.Elapsed += async (sender, e) =>
            {
                if (_semaphore.CurrentCount == 0)
                {
                    return;
                }
                await _semaphore.WaitAsync();
                try
                {
                    await CreateHostFile();
                }
                finally
                {
                    _semaphore.Release();
                }
            };

            _timer.Start();
        }

        public void StopProcessingUploadFiles()
        {
            _logger.LogDetailAsync("StopProcessingUploadFiles Dispose of Timer").SafeFireAndForget();
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
        public async Task CreateHostFile()
        {
            
            if (_uploadBusy)
            {
                _logger.LogDetailAsync($"Upload currently busy.  Exiting CreateHostFile").SafeFireAndForget();
                return;
            }

            
            _logger.LogDetailAsync("CreateHostFile Start").SafeFireAndForget();

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
                _logger.LogDetailAsync($"No Upload Codes Defined.  ActionCodes: {actionCodes}").SafeFireAndForget();
                return;
            }

            _uploadBusy = true;

            try
            {
                _logger.LogDetailAsync(
                    $"Find History Records where the Requested and Issued Quantities are equal").SafeFireAndForget();
                var recs = _repoHistory.FindBy(h => !h.TransmitDateTime.HasValue
                                                     && actionCodes.Contains(h.ActionCode)
                    && h.RequestedQuantity == h.IssuedQuantity)
                      .ToList();

                if (recs.Any())
                {
                    _logger.LogDetailAsync($"Found {recs.Count} History Records").SafeFireAndForget();
                    //var hostFile = new HostFilePr1(_neutronLicense, _neutronVariables, _workstationRepository);
                    //var result = hostFile.CreateHostFile(recs);
                    // for each record, get the TransId from the OrderDetailInfo field (rec.OrderDetailInfo)
                    // then get the NOVA_INPUT record with that TransId
                    // then create a new NOVA_OUTPUT record with the information from the NOVA_INPUT record
                    // add the beginning quantity, BEGINNINGQTY = (rec.RequestedQuantity) 
                    // and the ending quantity QTY = (rec.IssuedQuantity)
                    // then set the NOVA_OUTPUT record to PROCESSED = "N"  (the SAP process will set it to "Y" when it is processed)

                    foreach (var history in recs)
                    {
                        if (history == null) continue;

                        _logger.LogDetailAsync(
                            $"History Record Order Detail Item: {history.Item} Info: {history.OrderDetailInfo}").SafeFireAndForget();

                        if (string.IsNullOrEmpty(history.OrderDetailInfo))
                        {
                            _logger.LogDetailAsync(
                                $"History Record Order Detail Info is Null or Empty.  Continue to Next Item").SafeFireAndForget();
                            continue;
                        }


                        var orderDetailInfo = history.OrderDetailInfo.Split('|');
                        var transId = orderDetailInfo[0];
                        // convert transId to decimal
                        var transIdDec = Convert.ToDecimal(transId);

                        var input = _repoNovaInput.FindBy(n => n.TRANSID == transIdDec).FirstOrDefault();

                        if (input != null)
                        {

                            _logger.LogDetailAsync($"Record Found in NOVA_INPUT.  TransId: {transIdDec}").SafeFireAndForget();

                            _logger.LogDetailAsync($"Build NOVA_OUTPUT record.").SafeFireAndForget();
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
                            await LogInput(input, history);
                            await _repoNovaOutput.InsertAsync(output);
                        }

                        history.TransmitDateTime = DateTime.Now;
                        _repoHistory.Update(history);
                    }
                }
                // Check for orphan records
                // for each kind of pick(1) or putaway(2)
                _logger.LogDetailAsync($"Check for Orphans.").SafeFireAndForget();

                foreach (var actionCode in actionCodes)
                {

                    if (actionCode == 1)
                    {
                        await ProcessPickOrphans();
                    }
                    else if (actionCode == 2)
                    {
                        await ProcessReplenOrphans();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                    $"Upload Process Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }
            finally
            {
                _uploadBusy = false;
            }

            _uploadBusy = false;
           // _timer?.Start();
        }

        private async Task LogInput(NOVA_INPUT input, History history)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Output.TRANSID = {input.TRANSID}{Environment.NewLine}");
            sb.AppendLine($"Output.TASKNO = {input.TASKNO}{Environment.NewLine}");
            sb.AppendLine($"Output.TOTENO = {input.TOTENO}{Environment.NewLine}");
            sb.AppendLine($"Output.BP = {input.BP}{Environment.NewLine}");
            sb.AppendLine($"Output.SKU = {input.SKU}{Environment.NewLine}");
            sb.AppendLine($"Output.BEGINNINGQTY = {history.RequestedQuantity}{Environment.NewLine}");
            sb.AppendLine($"Output.QTY = {history.IssuedQuantity}{Environment.NewLine}");
            sb.AppendLine($"Output.TRANSDATE = {history.ActionDateTime}{Environment.NewLine}");
            sb.AppendLine($"Output.TRANSTYPE = {input.TRANSTYPE}{Environment.NewLine}");
            sb.AppendLine($"Output.PROCESSED = N {Environment.NewLine}");
            sb.AppendLine($"Output.EXPLANATION = {string.Empty}{Environment.NewLine}");

            await _logger.LogDetailAsync($"Output Values From Input Values{Environment.NewLine}{sb}");
        }

        private async Task ProcessPickOrphans()
        {
            _logger.LogDetailAsync($"Process Pick Orphans").SafeFireAndForget();
            // Are there any History records where the RequestedQuantity and IssuedQuantity aren't the same
            var orphans = _repoHistory.FindBy(h => !h.TransmitDateTime.HasValue
                                                   && h.ActionCode == 1
                                                   && h.RequestedQuantity != h.IssuedQuantity).ToList();
            if (orphans.Any())
            {
                _logger.LogDetailAsync($"Found {orphans.Count} Orphan Pick Records").SafeFireAndForget();
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
                        var orderDetail = await _repoOrderDetails.FindByFirstOrDefaultAsync(od => od.TransId == transId);
                        if (orderDetail != null)
                        {
                            _logger.LogDetailAsync(
                                 $"Order Detail Item: {orderDetail.PartNum}  Line Status: {orderDetail.LineStatusId}").SafeFireAndForget();
                            // 6 means that the line is complete
                            if (orderDetail.LineStatusId == 6)
                            {
                                // Get the information needed to send back to SAP
                                await SendToSapAsync(transId, orderDetail.Quantity, orderDetail.PickedQuantity);
                                _logger.LogDetailAsync(
                                    $"Send to SAP: TransId: {orderDetail.TransId}  Quantity: {orderDetail.Quantity}  Picked: {orderDetail.PickedQuantity}").SafeFireAndForget();
                                var transactionId = transId.ToString();
                                var recs = await _repoHistory
                                    .FindByAsync(r => r.OrderDetailInfo.StartsWith(transactionId));
                                foreach (var rec in recs)
                                {
                                    rec.TransmitDateTime = DateTime.Now;
                                    await _repoHistory.UpdateAsync(rec);
                                    _logger.LogDetailAsync(
                                        $"Set Transmit DateTime Item: {rec.Item} DateTime: {rec.TransmitDateTime}").SafeFireAndForget();
                                }
                            }
                        }
                    }
                }
            }
        }


        private async Task ProcessReplenOrphans()
        {
            _logger.LogDetailAsync($"Process Replen Orphans").SafeFireAndForget();
            // Are there any History records where the RequestedQuantity and IssuedQuantity aren't the same
            var orphanList = await _repoHistory.FindByAsync(h => !h.TransmitDateTime.HasValue
                                                   && h.ActionCode == 2
                                                   && h.RequestedQuantity != h.IssuedQuantity);
            var orphans = orphanList.ToList();
            if (orphans.Any())
            {
                _logger.LogDetailAsync($"Found {orphans.Count} Orphan Replen Records").SafeFireAndForget();
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
                        var orderDetail = await _repoReplenOrderDetails.FindByFirstOrDefaultAsync(od => od.TransId == transId);
                        
                        if (orderDetail != null)
                        {
                            _logger.LogDetailAsync(
                                $"Order Detail Item: {orderDetail.PartNum}  Line Status: {orderDetail.LineStatusId}").SafeFireAndForget();
                            // 6 means that the line is complete
                            if (orderDetail.LineStatusId == 6)
                            {
                                // Get the information needed to send back to SAP
                                await SendToSapAsync(transId, orderDetail.Quantity, orderDetail.PickedQuantity);
                                _logger.LogDetailAsync(
                                    $"Send to SAP: TransId: {orderDetail.TransId}  Quantity: {orderDetail.Quantity}  Picked: {orderDetail.PickedQuantity}").SafeFireAndForget();
                                var transactionId = transId.ToString();
                                var recs = await _repoHistory
                                    .FindByAsync(r => r.OrderDetailInfo.StartsWith(transactionId));
                                foreach (var rec in recs)
                                {
                                    rec.TransmitDateTime = DateTime.Now;
                                    await _repoHistory.UpdateAsync(rec);
                                    _logger.LogDetailAsync(
                                        $"Set Transmit DateTime Item: {rec.Item} DateTime: {rec.TransmitDateTime}").SafeFireAndForget();
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task SendToSapAsync(int transId, int requested, int issued)
        {
            _logger.LogDetailAsync($"Process Replen Orphans TransId: {transId}  Requested: {requested}  Issued: {issued}").SafeFireAndForget();
            
            var transIdDec = Convert.ToDecimal(transId);

            var input = await _repoNovaInput.FindByFirstOrDefaultAsync(n => n.TRANSID == transIdDec);

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
                await _repoNovaOutput.InsertAsync(output);
            }
        }
    }
}
