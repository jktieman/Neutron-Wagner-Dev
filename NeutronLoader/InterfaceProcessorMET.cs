
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AlliedLogger;
using HighJump;
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
using NovaLoader.Models;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    public class InterfaceProcessorMET : IInterfaceProcessor
    {
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Shipper> _repoShippers = new GenericRepository<Shipper>(new NeutronDb());
        private readonly GenericRepository<ShipMethod> _repoShipMethods = new GenericRepository<ShipMethod>(new NeutronDb());

        private readonly IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;

        public InterfaceProcessorMET(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView)
        {
            Initialize();           
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _logger = NeutronCore.Global.Logger.SetupLogger("InterfaceProcessor");
        }

        private void Initialize()
        {
            _fileProcessor = new METFileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData, _workstationView);
        }

        public void StartProcessingInterfaceFiles()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
            _timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
        }

        private void LoadOrders()
        {
            if (_loadOrdersBusy) return;
            _loadOrdersBusy = true;
            _ = _logger.LogDetailAsync("Load Orders");

            var hostOrderLines = GetNewOrdersSql();
            if (hostOrderLines.Any())
            {
                UpdateNeutronOrders(hostOrderLines);
            }

            _loadOrdersBusy = false;
        }

        private void UpdateNeutronOrders(List<HostOrderLine> hostOrderLines)
        {
            int shipperId = 0;
            int shipMethodId = 0;

            var shipper = _repoShippers.All().FirstOrDefault();
            if (shipper != null) shipperId = shipper.Id;

            var shipMethod = _repoShipMethods.All().FirstOrDefault();
            if (shipMethod != null) shipMethodId = shipMethod.Id;
            var orderNumbers = hostOrderLines.Select(r => r.OrderNumber).Distinct().ToList();

            foreach (var orderNumber in orderNumbers)
            {
                var rec = hostOrderLines.FirstOrDefault(r => r.OrderNumber == orderNumber);
                if (rec == null) continue;
                var order = new Order
                {
                    Ord1 = rec.Order,
                    Ord2 = rec.Invoice,
                    Priority = int.Parse(rec.Priority),
                    LoadDate = DateTime.Now,
                    OrderStatusId = (int)OrderStatus.Available,
                    ShipMethodId = shipMethodId,
                    ShipperId = shipperId

                };

                try
                {
                    _repoOrder.Insert(order);
                    var orderId = order.Id;
                    var orderDetails = hostOrderLines.Where(r => r.OrderNumber == orderNumber).ToList();
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
                                    PartDesc = orderDetail.Description,
                                    Quantity = int.Parse(orderDetail.Quantity),
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,
                                    OrderDetailInfo = orderDetail.CountryOfOrigin,
                                    OrderId = orderId,
                                    JobNum = orderDetail.Order,
                                    ItemDefinitionId = itemDef.Id
                                };
                                _repoOrderDetail.Insert(detail);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = $"HighJump to Neutron Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private List<HostOrderLine> GetNewOrdersSql()
        {


            _ = _logger.LogDetailAsync("Get New Orders Sql");
            List<HostOrderLine> orderLines = new List<HostOrderLine>();
            try
            {
                using (var context = new HighJumpContext())
                {
                    //try 3 times to get equal count
                    for (int i = 0; i < 3; i++)
                    {
                        _ = _logger.LogDetailAsync("Try Number " + i + " to get equal counts.");
                        var newRecords = context.t_al_host_carousel_outbound.Where(o => o.status == "N")
                            .OrderBy(o => o.container_label).ToList();
                        Thread.Sleep(1000);
                        var newRecordsSecondPass = context.t_al_host_carousel_outbound.Where(o => o.status == "N").ToList();

                        // if there are zero records return an empty list
                        if (!newRecords.Any() && !newRecordsSecondPass.Any())
                        {
                            _ = _logger.LogDetailAsync("No records in HighJump.");
                            return orderLines;
                        }

                        if (newRecords.Count() != 0 && newRecords.Count() == newRecordsSecondPass.Count())
                        {

                            _ = _logger.LogDetailAsync("Counts Match.  " + newRecords.Count + " Records to Process.");
                            foreach (var rec in newRecords)
                            {
                                // Check for existing order
                                var existingOrder = _repoOrder.FindBy(r => r.Ord1 == rec.container_label.Substring(10, 10)).FirstOrDefault();

                                if (existingOrder != null) continue;
                                
                                HostOrderLine h = new HostOrderLine();
                                h.OrderNumber = rec.container_label;
                                h.Sku = rec.item_number;
                                h.Quantity = rec.pick_quantity;
                                h.Description = rec.item_description;
                                h.CountryOfOrigin = rec.country_of_origin;
                                h.Priority = "00";
                                orderLines.Add(h);
                            }
                            UpdateOutboundToComplete(newRecords);
                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string msg = "Get New Orders " + ex.Message + "  " + ex.InnerException;
                _ = _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
            return orderLines;
        }

        private void UpdateOutboundToComplete(List<t_al_host_carousel_outbound> newRecords)
        {
            _ = _logger.LogDetailAsync("Update Outbound to Processing.");
            try
            {
                using (var context = new HighJumpContext())
                {
                    foreach (var rec in newRecords)
                    {
                        var outBound = context.t_al_host_carousel_outbound.Find(rec.host_carousel_outbound_id);
                        outBound.status = "C";
                        outBound.updated_by = "CAROUSEL";
                        outBound.updated_date = DateTime.Now;
                    }
                    context.SaveChanges();
                    _ = _logger.LogDetailAsync("Update Outbound to (C)omplete was Successful.");
                }
            }
            catch (Exception ex)
            {
                var msg = "Update Outbound To Complete Error. " + ex.Message + "  " + ex.InnerException;
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
            if (_timer != null) _timer.Dispose();

        }

        public void RunLoaderOnce()
        {
            LoadOrders();
        }

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
        //        _ = _logger.LogDetailAsync($"METFileProcessor: Number of Files: {files.Count}");
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
