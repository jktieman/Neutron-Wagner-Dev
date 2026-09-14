
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronCore.StaticClasses;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronDllu;
using NeutronEvents;
using ReplenService;
using SAPServer;
using OrderStatus = NeutronCore.Enums.OrderStatus;
using Timer = System.Timers.Timer;


namespace NeutronLoader
{
    // ReSharper disable once InconsistentNaming
    public class InterfaceProcessorWAG : IInterfaceProcessor
    {
        private readonly GenericRepository<Order> _repoOrder;
        private readonly GenericRepository<OrderDetail> _repoOrderDetail;
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder;
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail;
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        private readonly GenericRepository<Shipper> _repoShippers;
        private readonly GenericRepository<ShipMethod> _repoShipMethods;
        private ReplenProcessor _replenProcessor;
        private ItemDefinitionProcessor _itemDefinitionProcessor;
        private const int AreaEight = AreaNumber.Eight;
        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private bool _loadOrdersBusy;
        private readonly ISapService _sapService;
        private readonly IReplenRepository _replenRepository;
        private readonly IOrdersRepository _ordersRepository;
        private DocumentToPrint _documentToPrint;
        private DocumentPrinterPreferences _documentPrinter;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Func<NeutronDb> _contextFactory;
        private CancellationTokenSource _cancellationTokenSource;
        
        public InterfaceProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, ISapService sapService
            , IReplenRepository replenRepository, IOrdersRepository ordersRepository, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _contextFactory = contextFactory;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _sapService = sapService;
            _replenRepository = replenRepository;
            _repoOrder = new GenericRepository<Order>(contextFactory);
            _repoOrderDetail = new GenericRepository<OrderDetail>(contextFactory);
            _repoReplenOrder = new GenericRepository<ReplenOrder>(contextFactory);
            _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(contextFactory);
            _repoItemDefinition = new GenericRepository<ItemDefinition>(contextFactory);
            _repoShippers = new GenericRepository<Shipper>(contextFactory);
            _repoShipMethods = new GenericRepository<ShipMethod>(contextFactory);

            _ordersRepository = ordersRepository;

            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("NeutronLoader");
            _replenProcessor = new ReplenProcessor(_contextFactory, _replenRepository);
            _documentToPrint = new DocumentToPrint();
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            _itemDefinitionProcessor = new ItemDefinitionProcessor(_jsonData, _contextFactory);
            try
            {
                _sapService.Init();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error Initializing Loader. {Environment.NewLine} {ex.Message}");
            }

        }
        
        public async Task StartProcessingInterfaceFiles()
        {
            try
            {
                if (_neutronVariables.LoaderDelay <= 0)
                {
                    throw new ArgumentException("LoaderDelay must be greater than zero.");
                }

                _cancellationTokenSource = new CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                _ = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        await _semaphore.WaitAsync(token);
                        try
                        {
                            Debug.WriteLine("Starting LoadOrders...");
                            await LoadOrders();
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogDetailAsync("LoadOrders was cancelled.").SafeFireAndForget();
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDetailAsync($"Error in LoadOrders: {ex.Message}").SafeFireAndForget();
                        }
                        finally
                        {
                            _semaphore.Release();
                        }

                        // Wait between executions
                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(_neutronVariables.LoaderDelay), token);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogDetailAsync("Delay was cancelled, stopping loader.").SafeFireAndForget();
                            break;
                        }
                    }
                }, token);

                Debug.WriteLine($"Background loader started with interval: {_neutronVariables.LoaderDelay} seconds");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error Processing Interface File. {Environment.NewLine} {ex.Message}").SafeFireAndForget();
            }
        }

        
        /// <summary>
        /// Asynchronously loads and processes orders from the SAP service.
        /// </summary>
        /// <remarks>
        /// This method first checks if the loading process is already busy. If it is, it logs a message and returns.
        /// If not, it sets the loading process to busy and starts loading orders from the SAP service.
        /// After loading, it updates the Neutron orders and Neutron Replen orders based on the loaded orders.
        /// If any exceptions occur during this process, it logs the error message and sends an email with the error message.
        /// Finally, it sets the loading process to not busy.
        /// </remarks>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task LoadOrders()
        {

            if (_loadOrdersBusy)
            {
                _logger.LogDetailAsync("Load Orders is currently busy.").SafeFireAndForget();
                return;
            }

            try
            {
                _loadOrdersBusy = true;
                _logger.LogDetailAsync("Load Orders Testing Waiting 2 Seconds").SafeFireAndForget();

                _sapService.Run();

                var hostOrderLines = await GetNewOrdersFromSap();

                if (hostOrderLines.Any())
                {
                    await UpdateNeutronOrders(hostOrderLines);
                }

                // process the 02 / Replen orders
                var hostReplenOrderLines = await GetNewReplenOrdersFromSap();

                if (hostReplenOrderLines.Any())
                {
                    UpdateNeutronReplenOrders(hostReplenOrderLines);
                }

                if (_neutronVariables.AutoLoadReplenishments)
                {

                    // Restock/Replenishment lines where Tower or Blastzone is under MIN
                    var replenishmentLines = await GetNewOrdersFromReplenishments();

                    if (replenishmentLines.Any())
                    {
                        LoadReplenishments(replenishmentLines);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Load Orders Error. {Environment.NewLine}{ex.Message}").SafeFireAndForget();
                Mediator.GetInstance().OnSendEmailMessage(this, $"Load Orders Error. {Environment.NewLine}{ex.Message}");

                //throw new Exception($"Load Orders Error. {Environment.NewLine}{ex.Message}");
            }
            finally
            {
                _loadOrdersBusy = false;
            }
        }
        private async Task<List<NeutronInput>> GetNewReplenOrdersFromSap()
        {
            _logger.LogDetailAsync("Get New Replen Orders From SAP").SafeFireAndForget();
            var orderLines = new List<NeutronInput>();
            try
            {
                // get records from SAP Server
                List<NOVA_INPUT> recs;

                using (var db = new NeutronDb())
                {
                    recs = db.NOVA_INPUT.Where(r => r.PROCESSED == "N" && r.TRANSTYPE == "02").ToList();
                }

                if (recs.Any())
                {

                    _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.").SafeFireAndForget();

                    foreach (var rec in recs)
                    {
                        var h = new NeutronInput();
                        h.TransId = rec.TRANSID.ToString(CultureInfo.CurrentCulture);
                        h.Sku = rec.SKU.Trim();
                        h.Qty = (int)rec.QTY;
                        h.Order = rec.ORDERNO.ToString(CultureInfo.CurrentCulture);
                        h.Invoice = rec.INVOICENO.ToString(CultureInfo.CurrentCulture);
                        h.Des = rec.SKUDESC.Trim();
                        h.LineNo = rec.TOTENO.ToString(CultureInfo.CurrentCulture);
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

            //var shipperId = 0;
            //var shipMethodId = 0;

            //var shipper = _repoShippers.All().FirstOrDefault();
            //if (shipper != null) shipperId = shipper.Id;

            //var shipMethod = _repoShipMethods.All().FirstOrDefault();
            //if (shipMethod != null) shipMethodId = shipMethod.Id;

            //
            //
            //var orderNumbers = hostReplenOrderLines.Select(r => r.Order).Distinct().ToList();

            foreach (var line in hostReplenOrderLines)
            {
                var itemDef = GetReplenItemDefinition(line);//    _repoItemDefinition.FindBy(r => r.Item == line.Sku).FirstOrDefault();
                if (itemDef == null) return;

                // var rec = hostReplenOrderLines.FirstOrDefault(r => r.Order == orderNumber);
                // if (rec == null) continue;
                var order = new ReplenOrder
                {
                    Ord1 = line.Sku,
                    Ord2 = $"PUTAWAY{itemDef.AreaId}",
                    Priority = 0,
                    LoadDate = DateTime.Now,
                    OrderStatusId = (int)OrderStatus.Available,
                    ShipMethodId = 1,
                    ShipperId = 1,
                    OrderInfo = line.TransId

                };

                try
                {
                    _repoReplenOrder.Insert(order);
                    var orderId = order.Id;
                    // var orderDetails = hostReplenOrderLines.Where(r => r.Order == orderNumber).ToList();
                    // if (orderDetails.Any())
                    // {
                    //     foreach (var orderDetail in orderDetails)
                    //     {
                    // var itemDef = _repoItemDefinition.FindBy(r => r.Item == line.Sku).FirstOrDefault();

                    var detail = new ReplenOrderDetail()
                    {
                        PartNum = line.Sku,
                        PartDesc = line.Des,
                        Quantity = line.Qty,
                        LineStatusId = (int)LineStatus.Available,
                        AreaId = itemDef.AreaId,
                        OrderDetailInfo = $"{line.TransId}",
                        ReplenOrderId = orderId,
                        ItemDefinitionId = itemDef.Id,
                        TransId = line.TransId.ParseInt(),

                    };
                    _repoReplenOrderDetail.Insert(detail);
                    _documentToPrint.PrintReplenDoc(detail, _documentPrinter, _neutronVariables.PrintPreview);

                }
                catch (Exception ex)
                {
                    var message = $"SAP to Neutron Replen Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private void LoadReplenishments(List<NeutronInput> hostOrderLines)
        {
            var shipperId = 0;
            var shipMethodId = 0;
            var areaToPickFrom = 8;

            var shipper = _repoShippers.All().FirstOrDefault();
            if (shipper != null) shipperId = shipper.Id;

            var shipMethod = _repoShipMethods.All().FirstOrDefault();
            if (shipMethod != null) shipMethodId = shipMethod.Id;
            //var orderNumbers = hostOrderLines.Select(r => r.Order).Distinct().ToList();

            foreach (var rec in hostOrderLines)
            {
                if (rec == null) continue;
                var order = new Order();
                order.Ord1 = rec.Order;
                order.Ord2 = rec.Invoice;
                order.Priority = 0;
                order.LoadDate = DateTime.Now;
                order.OrderStatusId = (int)OrderStatus.Hold;
                order.ShipMethodId = shipMethodId;
                order.ShipperId = shipperId;
                order.OrderInfo = string.Empty;

                try
                {
                    _repoOrder.Insert(order);
                    var orderId = order.Id;
                    var orderDetail = hostOrderLines.FirstOrDefault(r => r.Order == rec.Order);
                    if (orderDetail == null) continue;
                    {
                        var itemDef = GetItemDefinitionFromAreaEight(orderDetail);

                        if (itemDef != null)
                        {
                            var detail = new OrderDetail()
                            {
                                PartNum = orderDetail.Sku,
                                PartDesc = itemDef.Description,
                                Quantity = orderDetail.Qty,
                                LineStatusId = (int)LineStatus.Hold,
                                AreaId = areaToPickFrom,
                                OrderDetailInfo = string.Empty,
                                OrderId = orderId,
                                JobNum = orderDetail.Order,
                                ItemDefinitionId = itemDef.Id,
                                TransId = orderDetail.TransId.ParseInt(),
                            };
                            _repoOrderDetail.Insert(detail);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Replenishment to Neutron Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private ItemDefinition GetItemDefinitionFromAreaEight(NeutronInput line)
        {
            //ItemDefinition itemDef = null;
            // the ItemDefinition could be in multiple Areas and rules determine what Area to pick from.
            // If there are multiple Areas that have the ItemDefinition
            // There is a PickMax value in the ItemDefinition that determines where to pick from.
            // that is also the Area that the ItemDefinition needs to point to

            // Let's get all the Areas where the ItemDefinition might be.
            var itemDef = _repoItemDefinition.FindBy(r => r.Item == line.Sku && r.AreaId == AreaEight).FirstOrDefault();

            return itemDef;
        }

        private async Task UpdateNeutronOrders(List<NeutronInput> hostOrderLines)
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

                int.TryParse(rec.Priority, out var newPriority);


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
                    await _repoOrder.InsertAsync(order);
                    var orderId = order.Id;
                    var orderDetails = hostOrderLines.Where(r => r.Order == orderNumber).ToList();
                    if (orderDetails.Any())
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            var itemDef = await GetItemDefinition(orderDetail);

                            if (itemDef != null)
                            {
                                var detail = new OrderDetail()
                                {
                                    PartNum = orderDetail.Sku,
                                    PartDesc = orderDetail.Des,
                                    Quantity = orderDetail.Qty,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,
                                    OrderDetailInfo = $"{orderDetail.TransId}|{rec.Priority}|{rec.Division}|{@"Route"}|{orderDetail.Upc}",
                                    OrderId = orderId,
                                    JobNum = orderDetail.Order,
                                    ItemDefinitionId = itemDef.Id,
                                    TransId = orderDetail.TransId.ParseInt(),
                                };
                                _repoOrderDetail.Insert(detail);
                            }
                        }
                    }

                    // update the Route information
                    //var detail = _repoOrderDetail.FindBy(r => r.OrderId == orderId).FirstOrDefault();
                    var route = _ordersRepository.GetRoute(orderId);

                    var routeDetails = _repoOrderDetail.FindBy(r => r.OrderId == orderId).ToList();
                    foreach (var routeDetail in routeDetails)
                    {
                        var ordDetail = routeDetail.OrderDetailInfo;
                        var ordDetails = ordDetail.Split('|');
                        ordDetails[3] = route;
                        routeDetail.OrderDetailInfo = string.Join("|", ordDetails);
                        _repoOrderDetail.Update(routeDetail);
                    }
                }
                catch (Exception ex)
                {
                    var message = $"SAP to Neutron Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private async Task<ItemDefinition> GetItemDefinition(NeutronInput line)
        {
            _logger.LogDetailAsync($"NeutronInput --  Delivery: {line.Order} Item: {line.Sku} Qty: {line.Qty}").SafeFireAndForget();

            ItemDefinition itemDef = null;

            // the ItemDefinition could be in multiple Areas and rules determine what Area to pick from.
            // If there are multiple Areas that have the ItemDefinition
            // There is a PickMax value in the ItemDefinition that determines where to pick from.
            // that is also the Area that the ItemDefinition needs to point to
            try
            {

                // Let's get all the Areas where the ItemDefinition might be.
                // For Example it could be in Area 2 with more in Area 8
                // Return them in Area Order
                var itemDefs = _repoItemDefinition.FindBy(r => r.Item == line.Sku).OrderBy(o => o.AreaId).ToList();

                var tasks = new List<Task>();

                if (itemDefs.Any())
                {
                    if (_neutronVariables.UpdateItemDefinitionDescription)
                    {
                        foreach (var def in itemDefs)
                        {
                            if (!def.Description.Equals(line.Des))
                            {
                                tasks.Add(_logger.LogDetailAsync($"Update Item Definition Description: {def.Description} TO {line.Des}"));
                                tasks.Add(_itemDefinitionProcessor.UpdateDescriptionAsync(def, line.Des));
                            }
                        }
                    }
                }
                await Task.WhenAll(tasks);

                // All ItemDefinitions have a PickMax value unless it is in Area 8
                // Area 8 is the option if the PickMax number is reached
                // If there are multiple ItemDefinitions
                // We need to know if there is a PickMax rule to follow
                // If no PickMax rule, pick All from the lowest/first Area

                // Create a switch statement based on the number of itemDefs
                switch (itemDefs.Count)
                {
                    case 0:  // No ItemDefinitions found
                        {
                            // Create a new ItemDefinition using default values
                            itemDef = _itemDefinitionProcessor.GetOrCreate(line.Sku, line.Des, AreaEight);
                            _logger.LogDetailAsync($"New Item Definition: {itemDef.Item} Area: {itemDef.AreaId}").SafeFireAndForget();
                            break;
                        }
                    case 1:
                        {
                            // Only one ItemDefinition found, so use it
                            itemDef = itemDefs.First();
                            _logger.LogDetailAsync($"One Item Definition: {itemDef.Item} Area: {itemDef.AreaId}").SafeFireAndForget();

                            break;
                        }
                    case 2:  // two ItemDefinitions, one should be in Area 1,2,3, or 4
                             // and one should be in Area 8
                        {
                            foreach (var itemDefinition in itemDefs)
                            {
                                _logger.LogDetailAsync(
                                    $"Two Item Definitions -- ItemDefinition Id: {itemDefinition.Id}  Area: {itemDefinition.AreaId}").SafeFireAndForget();
                            }
                            // Areas with PickMax parameter
                            var areaIds = new int[] { 1, 2, 3, 4 };
                            // The first item in the list of ItemDefinitions (sorted by AreaId)
                            var itemDefFirst = itemDefs.FirstOrDefault(r => areaIds.Contains(r.AreaId));
                            if (itemDefFirst != null)
                            {
                                itemDef = itemDefFirst;
                                if (itemDefFirst.PickMax > 0)
                                {
                                    // That means we need to see if the quantity to pick 
                                    // is greater than the PickMax variable
                                    if (line.Qty > itemDefFirst.PickMax)
                                    {
                                        // If it is, then we need to pick from Area Eight
                                        var areaEightItemDef = itemDefs.FirstOrDefault(r => r.AreaId == AreaEight);
                                        if (areaEightItemDef != null)
                                        {
                                            itemDef = areaEightItemDef;
                                        }
                                    }
                                }

                            }
                            break;
                        }
                    default:
                        _logger.LogDetailAsync($"Multiple Item Definitions: {itemDefs.Count}").SafeFireAndForget();
                        foreach (var itemDefinition in itemDefs)
                        {
                            _logger.LogDetailAsync(
                                $"ItemDefinition Id: {itemDefinition.Id}  Area: {itemDefinition.AreaId}").SafeFireAndForget();
                        }

                        itemDef = itemDefs.FirstOrDefault();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error getting Item Definition: {ex.Message}").SafeFireAndForget();
            }

            _logger.LogDetailAsync(
                $"Final Answer => Item: {itemDef.Item} ItemDefinition Id: {itemDef.Id}  Area: {itemDef.AreaId}").SafeFireAndForget();
            return itemDef;
        }

        private ItemDefinition GetReplenItemDefinition(NeutronInput line)
        {
            ItemDefinition itemDef = null;
            // the ItemDefinition could be in multiple Areas and rules determine what Area to pick from.
            // If there are multiple Areas that have the ItemDefinition
            // There is a PickMax value in the ItemDefinition that determines where to pick from.
            // that is also the Area that the ItemDefinition needs to point to

            // Let's get all the Areas where the ItemDefinition might be.
            var itemDefs = _repoItemDefinition.FindBy(r => r.Item == line.Sku).OrderBy(o => o.AreaId).ToList();

            //if (itemDefs.Count == 0)
            //{
            //    // create a new ItemDefinition in Area 8 
            //    // using default values
            //    var newItemDefinition = _jsonData.LoadFile<ItemDefinition>();
            //    if (newItemDefinition != null)
            //    {
            //        newItemDefinition.Item = line.Sku;
            //        newItemDefinition.Description = line.Des;
            //        newItemDefinition.AreaId = AreaEight;

            //    }
            //    _repoItemDefinition.Insert(newItemDefinition);
            //    itemDef = newItemDefinition;
            //}

            // Create a switch statement based on the number of itemDefs
            switch (itemDefs.Count)
            {
                case 0:
                    // No ItemDefinitions found
                    itemDef = _itemDefinitionProcessor.GetOrCreate(line.Sku, line.Des, AreaEight);
                    break;
                case 1:
                    // Only one ItemDefinition found
                    itemDef = itemDefs.First();
                    break;
                default:
                    // Multiple ItemDefinitions found
                    itemDef = itemDefs.Last();
                    break;
            }


            return itemDef;
        }

        private async Task<List<NeutronInput>> GetNewOrdersFromReplenishments()
        {
            _logger.LogDetailAsync("Get New Orders From Replenishments").SafeFireAndForget();
            var orderLines = new List<NeutronInput>();
            try
            {
                var recs = await _replenProcessor.GetNewAndUpdatedReplenishments();

                if (recs.Any())
                {

                    _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.").SafeFireAndForget();
                    foreach (var rec in recs)
                    {
                        var h = new NeutronInput();
                        h.TransId = string.Empty;
                        h.Sku = rec.Item;
                        h.Qty = rec.QuantityNeeded;
                        h.Division = string.Empty;
                        h.Order = rec.Item;
                        h.Priority = "0";
                        h.Invoice = $"REPLEN => {rec.ReplenArea}";
                        h.Des = string.Empty;
                        h.Upc = string.Empty;
                        h.LineNo = string.Empty;
                        h.Name = string.Empty;
                        h.Street = string.Empty;
                        h.City = string.Empty;
                        h.Region = string.Empty;
                        h.ZipCode = string.Empty;
                        h.Country = string.Empty;
                        h.Text = string.Empty;

                        orderLines.Add(h);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = "Get New Replenishment Orders " + ex.Message + "  " + ex.InnerException;
                await _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
            return orderLines;
        }

        private async Task<List<NeutronInput>> GetNewOrdersFromSap()
        {
            _logger.LogDetailAsync("Get New Orders From SAP").SafeFireAndForget();
            await Task.Delay(10);
            var orderLines = new List<NeutronInput>();
            try
            {
                // get records from SAP Server
                List<NOVA_INPUT> recs;
                // using (var db = new WagnerDb())
                using (var db = new NeutronDb())
                {
                    recs = db.NOVA_INPUT.Where(r => r.PROCESSED == "N" && r.TRANSTYPE == "22").ToList();
                }

                if (recs.Any())
                {

                    _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.").SafeFireAndForget();
                    foreach (var rec in recs)
                    {
                        // Check for existing order

                        //var existingOrder = _repoOrder.FindBy(r => r.Ord1 == rec.ORDERNO.ToString() && r.Ord2 == rec.INVOICENO.ToString()).FirstOrDefault();

                        //if (existingOrder != null) continue;

                        var h = new NeutronInput();
                        h.TransId = rec.TRANSID.ToString(CultureInfo.CurrentCulture);
                        h.Sku = rec.SKU;
                        h.Qty = (int)rec.QTY;
                        h.Division = rec.DIVISION;
                        h.Order = rec.ORDERNO.ToString(CultureInfo.CurrentCulture);
                        h.Priority = string.IsNullOrEmpty(rec.PRIORITY) ? "0" : rec.PRIORITY;
                        h.Invoice = rec.TASKNO == 0 ? String.Empty : rec.TASKNO.ToString(CultureInfo.CurrentCulture);
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
                var msg = "Get New Orders From SAP " + ex.Message + "  " + ex.InnerException;
                _logger.LogDetailAsync(msg).SafeFireAndForget();
                ErrorAlert(msg);
            }
            return orderLines;
        }
        private void UpdateToProcessed(List<NOVA_INPUT> newRecords)
        {
            _logger.LogDetailAsync("Update To Processed.").SafeFireAndForget();
            try
            {
                // using (var context = new WagnerDb())
                using (var context = new NeutronDb())
                {
                    foreach (var rec in newRecords)
                    {
                        var outBound = context.NOVA_INPUT.Find(rec.TRANSID);
                        if (outBound == null) continue;
                        outBound.PROCESSED = "Y";
                    }
                    context.SaveChanges();
                    _logger.LogDetailAsync("Update Outbound to Processed was Successful.").SafeFireAndForget();
                }
            }
            catch (Exception ex)
            {
                var msg = "Update Outbound To Processed Error. " + ex.Message + "  " + ex.InnerException;
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            Debug.WriteLine($"Loader Disposed");
        }

        public async Task RunLoaderOnce() => await LoadOrders();
    }
}
