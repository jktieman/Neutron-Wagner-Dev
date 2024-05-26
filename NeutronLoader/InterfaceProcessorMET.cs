
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
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
using OrderStatus = NeutronCore.Enums.OrderStatus;
using Timer = System.Timers.Timer;

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

        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private IFileProcessor _fileProcessor;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        public InterfaceProcessorMET(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView)
        {
          
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
             Init();            
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("InterfaceProcessor");
 
            _fileProcessor = new METFileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData, _workstationView);
        }

        public async Task StartProcessingInterfaceFiles()
        {
            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
            //_timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            _timer = new System.Timers.Timer(_neutronVariables.LoaderDelay * 1000);
            //_timer.Elapsed += async (sender, e) => await LoadOrders();
            _timer.Elapsed += async (sender, e) =>
            {
                if (_semaphore.CurrentCount == 0)
                {
                    return;
                }
                await _semaphore.WaitAsync();
                try
                {
                    await LoadOrders();
                }
                finally
                {
                    _semaphore.Release();
                }
            };
            _timer.Start();
            await Task.Delay(10);
        }

        private async Task LoadOrders()
        {
            if (_loadOrdersBusy) return;
            
            //_timer?.Stop();
            _loadOrdersBusy = true;
            _logger.LogDetailAsync("Load Orders").SafeFireAndForget();

            var hostOrderLines = GetNewOrdersSql();
            if (hostOrderLines.Any())
            {
                UpdateNeutronOrders(hostOrderLines);
            }

            await Task.Delay(10);
            _loadOrdersBusy = false;
           // _timer?.Start();
        }

        private void UpdateNeutronOrders(List<HostOrderLine> hostOrderLines)
        {
            var shipperId = 0;
            var shipMethodId = 0;

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


            _logger.LogDetailAsync("Get New Orders Sql").SafeFireAndForget();
            var orderLines = new List<HostOrderLine>();
            try
            {
                using (var context = new HighJumpContext())
                {
                    //try 3 times to get equal count
                    for (var i = 0; i < 3; i++)
                    {
                        _logger.LogDetailAsync("Try Number " + i + " to get equal counts.").SafeFireAndForget();
                        var newRecords = context.t_al_host_carousel_outbound.Where(o => o.status == "N")
                            .OrderBy(o => o.container_label).ToList();
                        Thread.Sleep(1000);
                        var newRecordsSecondPass = context.t_al_host_carousel_outbound.Where(o => o.status == "N").ToList();

                        // if there are zero records return an empty list
                        if (!newRecords.Any() && !newRecordsSecondPass.Any())
                        {
                            _logger.LogDetailAsync("No records in HighJump.").SafeFireAndForget();
                            return orderLines;
                        }

                        if (newRecords.Count() != 0 && newRecords.Count() == newRecordsSecondPass.Count())
                        {

                            _logger.LogDetailAsync("Counts Match.  " + newRecords.Count + " Records to Process.").SafeFireAndForget();
                            foreach (var rec in newRecords)
                            {
                                // Check for existing order
                                var existingOrder = _repoOrder.FindBy(r => r.Ord1 == rec.container_label.Substring(10, 10)).FirstOrDefault();

                                if (existingOrder != null) continue;
                                
                                var h = new HostOrderLine();
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
                var msg = "Get New Orders " + ex.Message + "  " + ex.InnerException;
                _logger.LogDetailAsync(msg).SafeFireAndForget();
                ErrorAlert(msg);
            }
            return orderLines;
        }

        private void UpdateOutboundToComplete(List<t_al_host_carousel_outbound> newRecords)
        {
            _logger.LogDetailAsync("Update Outbound to Processing.").SafeFireAndForget();
            try
            {
                using (var context = new HighJumpContext())
                {
                    foreach (var rec in newRecords)
                    {
                        var outBound = context.t_al_host_carousel_outbound.Find(rec.host_carousel_outbound_id);
                        if (outBound == null) continue;
                        outBound.status = "C";
                        outBound.updated_by = "CAROUSEL";
                        outBound.updated_date = DateTime.Now;
                    }
                    context.SaveChanges();
                    _logger.LogDetailAsync("Update Outbound to (C)omplete was Successful.").SafeFireAndForget();
                }
            }
            catch (Exception ex)
            {
                var msg = "Update Outbound To Complete Error. " + ex.Message + "  " + ex.InnerException;
                _logger.LogDetailAsync(msg).SafeFireAndForget();
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

        public async Task RunLoaderOnce() => await LoadOrders();
    }
}
