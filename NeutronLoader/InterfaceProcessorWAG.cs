
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using JsonManager;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using NeutronEvents;
using SAPServer;
using SAPServer.Models;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using Timer = System.Threading.Timer;


namespace NeutronLoader
{
    // ReSharper disable once InconsistentNaming
    public class InterfaceProcessorWAG : IInterfaceProcessor
    {
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Shipper> _repoShippers = new GenericRepository<Shipper>(new NeutronDb());
        private readonly GenericRepository<ShipMethod> _repoShipMethods = new GenericRepository<ShipMethod>(new NeutronDb());

        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;
        private ISapService _sapService;

        public InterfaceProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, ISapService sapService)
        {
            Initialize();
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _sapService = sapService;
        }

        private void Initialize()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("InterfaceProcessor");
            // _fileProcessor = new WAGFileProcessor(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
        }

        public void StartProcessingInterfaceFiles()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
            _timer = new Timer(t => { _ = LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
        }

        private async Task LoadOrders()
        {
            if (_loadOrdersBusy) return;

            try
            {
                _loadOrdersBusy = true;
                await _logger.LogDetailAsync("Load Orders Testing Waiting 2 Seconds");

                _sapService.Init();
                
                

                var hostOrderLines = await GetNewOrdersFromSap();

                if (hostOrderLines.Any())
                {
                    UpdateNeutronOrders(hostOrderLines);
                }

                // process the 02/Replen orders
                var hostReplenOrderLines = await GetNewReplenOrdersFromSap();

                if (hostReplenOrderLines.Any())
                {
                    UpdateNeutronReplenOrders(hostReplenOrderLines);
                }

            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Load Orders Error. {Environment.NewLine}{ex.Message}");
                throw new Exception($"Load Orders Error. {Environment.NewLine}{ex.Message}");
            }
            finally
            {
                _loadOrdersBusy = false;
            }
        }

        private async Task<List<NeutronInput>> GetNewReplenOrdersFromSap()
        {
            await _logger.LogDetailAsync("Get New Replen Orders From SAP");
            var orderLines = new List<NeutronInput>();
            try
            {
                // get records from SAP Server
                List<NOVA_INPUT> recs;
                using (var db = new WagnerDb())
                {
                    recs = db.NOVA_INPUT.Where(r => r.PROCESSED == "N" && r.TRANSTYPE == "02").ToList();
                }

                if (recs.Any())
                {

                    _ = _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.");
                    foreach (var rec in recs)
                    {
                        // Check for existing order
                        var existingOrder = _repoReplenOrder.FindBy(r => r.Ord1 == rec.ORDERNO.ToString()).FirstOrDefault();

                        if (existingOrder != null) continue;

                        var h = new NeutronInput();
                        h.TransId = rec.TRANSID.ToString(CultureInfo.CurrentCulture);
                        h.Sku = rec.SKU;
                        h.Qty = (int)rec.QTY;
                        h.Division = rec.DIVISION;
                        h.Order = rec.ORDERNO.ToString(CultureInfo.CurrentCulture);
                        h.Priority = rec.PRIORITY;
                        h.Invoice = rec.INVOICENO.ToString(CultureInfo.CurrentCulture);
                        h.Des = rec.SKUDESC;
                        h.Upc = rec.UPC;
                        h.LineNo = rec.TOTENO.ToString(CultureInfo.CurrentCulture);
                        h.Name = rec.NAME1;
                        h.Street = rec.STREET;
                        h.City = rec.CITY1;
                        h.Region = rec.REGION;
                        h.ZipCode = rec.POST_CODE1;
                        h.Country = rec.COUNTRY;
                        h.Text = rec.TEXT;
                        orderLines.Add(h);
                    }
                    UpdateToProcessed(recs);
                }

            }
            catch (Exception ex)
            {
                var msg = "Get New Replen Orders From SAP " + ex.Message + "  " + ex.InnerException;
                await _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
            return orderLines;
        }

        private void UpdateNeutronReplenOrders(List<NeutronInput> hostReplenOrderLines)
        {
            var shipperId = 0;
            var shipMethodId = 0;

            var shipper = _repoShippers.All().FirstOrDefault();
            if (shipper != null) shipperId = shipper.Id;

            var shipMethod = _repoShipMethods.All().FirstOrDefault();
            if (shipMethod != null) shipMethodId = shipMethod.Id;

            var orderNumbers = hostReplenOrderLines.Select(r => r.Order).Distinct().ToList();

            foreach (var orderNumber in orderNumbers)
            {
                var rec = hostReplenOrderLines.FirstOrDefault(r => r.Order == orderNumber);
                if (rec == null) continue;
                var order = new ReplenOrder
                {
                    Ord1 = "REPLENOPRP",
                    Ord2 = rec.Sku,
                    Priority = 0,
                    LoadDate = DateTime.Now,
                    OrderStatusId = (int)OrderStatus.Available,
                    ShipMethodId = shipMethodId,
                    ShipperId = shipperId,
                    OrderInfo = $"{rec.TransId}"

                };

                try
                {
                    _repoReplenOrder.Insert(order);
                    var orderId = order.Id;
                    var orderDetails = hostReplenOrderLines.Where(r => r.Order == orderNumber).ToList();
                    if (orderDetails.Any())
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            var itemDef = _repoItemDefinition.FindBy(r => r.Item == orderDetail.Sku).FirstOrDefault();
                            if (itemDef != null)
                            {
                                var detail = new ReplenOrderDetail()
                                {
                                    PartNum = orderDetail.Sku,
                                    PartDesc = orderDetail.Des,
                                    Quantity = orderDetail.Qty,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,
                                    OrderDetailInfo = $"{rec.TransId}",
                                    ReplenOrderId = orderId,
                                    JobNum = orderDetail.Order.ToString(),
                                    ItemDefinitionId = itemDef.Id
                                    
                                };
                                _repoReplenOrderDetail.Insert(detail);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = $"SAP to Neutron Replen Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private void UpdateNeutronOrders(List<NeutronInput> hostOrderLines)
        {
            var shipperId = 0;
            var shipMethodId = 0;

            var shipper = _repoShippers.All().FirstOrDefault();
            if (shipper != null) shipperId = shipper.Id;

            var shipMethod = _repoShipMethods.All().FirstOrDefault();
            if (shipMethod != null) shipMethodId = shipMethod.Id;
            var orderNumbers = hostOrderLines.Select(r => r.Order).Distinct().ToList();

            foreach (var orderNumber in orderNumbers)
            {
                var rec = hostOrderLines.FirstOrDefault(r => r.Order == orderNumber);
                if (rec == null) continue;
                var order = new Order();

                var pri = int.TryParse(rec.Priority, out var newPriority);
                
                
                var pri2 = string.IsNullOrEmpty(rec.Priority);

                order.Ord1 = rec.Order;
                order.Ord2 = string.IsNullOrEmpty(rec.Invoice) ? rec.Order : rec.Invoice;
                order.Priority = newPriority;
                order.LoadDate = DateTime.Now;
                order.OrderStatusId = (int)OrderStatus.Available;
                order.ShipMethodId = shipMethodId;
                order.ShipperId = shipperId;
                order.OrderInfo = $"{rec.TransId}|{rec.Priority}|{rec.Division}|{rec.Name}|{rec.Street}|{rec.City}|{rec.Region}|{rec.ZipCode}|{rec.Country}|{rec.Text}";

                

                try
                {
                    _repoOrder.Insert(order);
                    var orderId = order.Id;
                    var orderDetails = hostOrderLines.Where(r => r.Order == orderNumber).ToList();
                    if (orderDetails.Any())
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            var itemDef = _repoItemDefinition.FindBy(r => r.Item == orderDetail.Sku).FirstOrDefault();
                            if (itemDef != null)
                            {
                                var detail = new OrderDetail()
                                {
                                    PartNum = orderDetail.Sku,
                                    PartDesc = orderDetail.Des,
                                    Quantity = orderDetail.Qty,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,
                                    OrderDetailInfo = $"{rec.TransId},{rec.Priority},{rec.Division},{"Route"},{rec.Upc}",
                                    OrderId = orderId,
                                    JobNum = orderDetail.Order.ToString(),
                                    ItemDefinitionId = itemDef.Id
                                };
                                _repoOrderDetail.Insert(detail);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = $"SAP to Neutron Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private async Task<List<NeutronInput>> GetNewOrdersFromSap()
        {
            await _logger.LogDetailAsync("Get New Orders From SAP");
            var orderLines = new List<NeutronInput>();
            try
            {
                // get records from SAP Server
                List<NOVA_INPUT> recs;
                using (var db = new WagnerDb())
                {
                    recs = db.NOVA_INPUT.Where(r => r.PROCESSED == "N" && r.TRANSTYPE == "22").ToList();
                }

                if (recs.Any())
                {

                    _ = _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.");
                    foreach (var rec in recs)
                    {
                        // Check for existing order

                        var existingOrder = _repoOrder.FindBy(r => r.Ord1 == rec.ORDERNO.ToString()).FirstOrDefault();

                        if (existingOrder != null) continue;

                        var h = new NeutronInput();
                        h.TransId = rec.TRANSID.ToString(CultureInfo.CurrentCulture);
                        h.Sku = rec.SKU;
                        h.Qty = (int)rec.QTY;
                        h.Division = rec.DIVISION;
                        h.Order = rec.ORDERNO.ToString(CultureInfo.CurrentCulture);
                        h.Priority = string.IsNullOrEmpty(rec.PRIORITY) ? "0" : rec.PRIORITY;
                        h.Invoice = rec.INVOICENO == 0 ? String.Empty   : rec.INVOICENO.ToString(CultureInfo.CurrentCulture);
                        h.Des = rec.SKUDESC;
                        h.Upc = rec.UPC;
                        h.LineNo = rec.TOTENO.ToString(CultureInfo.CurrentCulture);
                        h.Name = rec.NAME1;
                        h.Street = rec.STREET;
                        h.City = rec.CITY1;
                        h.Region = rec.REGION;
                        h.ZipCode = rec.POST_CODE1;
                        h.Country = rec.COUNTRY;
                        h.Text = rec.TEXT;

                        orderLines.Add(h);
                    }
                    UpdateToProcessed(recs);
                }

                // var sapService = new SAPService(_jsonData);

            }
            catch (Exception ex)
            {
                var msg = "Get New Orders From SAP " + ex.Message + "  " + ex.InnerException;
                await _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
            return orderLines;
        }

        //private List<HostOrderLine> GetNewOrdersSql()
        //{
        //try
        //{
        //    using (var context = new HighJumpContext())
        //    {
        //        //try 3 times to get equal count
        //        for (var i = 0; i < 3; i++)
        //        {
        //            _ = _logger.LogDetailAsync("Try Number " + i + " to get equal counts.");
        //            var newRecords = context.t_al_host_carousel_outbound.Where(o => o.status == "N")
        //                .OrderBy(o => o.container_label).ToList();
        //            Thread.Sleep(1000);
        //            var newRecordsSecondPass = context.t_al_host_carousel_outbound.Where(o => o.status == "N").ToList();

        //            // if there are zero records return an empty list
        //            if (!newRecords.Any() && !newRecordsSecondPass.Any())
        //            {
        //                _ = _logger.LogDetailAsync("No records in HighJump.");
        //                return orderLines;
        //            }

        //            if (newRecords.Count() != 0 && newRecords.Count() == newRecordsSecondPass.Count())
        //            {

        //                _ = _logger.LogDetailAsync("Counts Match.  " + newRecords.Count + " Records to Process.");
        //                foreach (var rec in newRecords)
        //                {
        //                    // Check for existing order
        //                    var existingOrder = _repoOrder.FindBy(r => r.Ord1 == rec.container_label.Substring(10, 10)).FirstOrDefault();

        //                    if (existingOrder != null) continue;

        //                    var h = new HostOrderLine();
        //                    h.OrderNumber = rec.container_label;
        //                    h.Sku = rec.item_number;
        //                    h.Quantity = rec.pick_quantity;
        //                    h.Description = rec.item_description;
        //                    h.CountryOfOrigin = rec.country_of_origin;
        //                    h.Priority = "00";
        //                    orderLines.Add(h);
        //                }
        //                UpdateOutboundToComplete(newRecords);
        //                break;
        //            }
        //        }
        //    }

        //}
        //catch (Exception ex)
        //{
        //    var msg = "Get New Orders " + ex.Message + "  " + ex.InnerException;
        //    _ = _logger.LogDetailAsync(msg);
        //    ErrorAlert(msg);
        //}
        //return orderLines;
        //}

        private void UpdateToProcessed(List<NOVA_INPUT> newRecords)
        {
            _ = _logger.LogDetailAsync("Update To Processed.");
            try
            {
                using (var context = new WagnerDb())
                {
                    foreach (var rec in newRecords)
                    {
                        var outBound = context.NOVA_INPUT.Find(rec.TRANSID);
                        if (outBound == null) continue;
                        outBound.PROCESSED = "Y";
                    }
                    context.SaveChanges();
                    _ = _logger.LogDetailAsync("Update Outbound to Processed was Successful.");
                }
            }
            catch (Exception ex)
            {
                var msg = "Update Outbound To Processed Error. " + ex.Message + "  " + ex.InnerException;
                _ = _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }

        public void StopProcessingInterfaceFiles()
        {
            _timer?.Dispose();
        }

        public void RunLoaderOnce() => _ = LoadOrders();

        //public FileInfo[] GetFiles()
        //{
        //    _ = _logger.LogDetailAsync("Call to Get Files Function.");
        //    var result = new FileInfo[] { };
        //    try
        //    {
        //        result = _hostOrderDirectory.GetFiles(_inputFileFilter);
        //    }
        //    catch (Exception ex)
        //    {
        //        _ = _logger.LogDetailAsync(
        //            $"Get Files Error.  {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}");
        //    }

        //    return result;
        //}

        //private void StopBackgroundWorker()
        //{
        //    _backgroundWorker?.CancelAsync();
        //}

        //private void InitBackgroundWorker()
        //{
        //    _backgroundWorker = new BackgroundWorker
        //    {
        //        WorkerReportsProgress = false,
        //        WorkerSupportsCancellation = true
        //    };
        //    _backgroundWorker.DoWork += BackgroundWorkerDoWork;
        //    _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
        //    _backgroundWorker.RunWorkerAsync();
        //}

        //private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        //{
        //    _ = _logger.LogDetailAsync("Queue Processor Do Work");
        //    if (_backgroundWorker.CancellationPending)
        //    {
        //        _ = _logger.LogDetailAsync($"BackgroundWorker Cancel.");
        //        e.Cancel = true;
        //        return;
        //    }

        //    Thread.Sleep(millisecondsTimeout: 100);
        //    foreach (var fileInfo in _interfaceFileQueue.GetConsumingEnumerable())
        //    {
        //        _ = _logger.LogDetailAsync($"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ");
        //        var files = new List<FileInfo>();
        //        if (!File.Exists(fileInfo.FullName)) continue;
        //        files.Add(fileInfo);
        //        Thread.Sleep(millisecondsTimeout: 100);
        //        _ = _logger.LogDetailAsync($"WAGFileProcessor: Number of Files: {files.Count}");
        //        _fileProcessor.LoadFiles(files);
        //    }
        //}

        //private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //    }
        //    else
        //    {
        //        object result = e.Result;
        //    }
        //}

        //private string CheckForBackSlash(string neutronBusyPath)
        //{
        //    return neutronBusyPath.EndsWith(@"\") ? string.Empty : @"\";
        //}
    }
}
