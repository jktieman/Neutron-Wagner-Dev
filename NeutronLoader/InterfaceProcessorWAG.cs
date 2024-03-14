
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
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
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Shipper> _repoShippers = new GenericRepository<Shipper>(new NeutronDb());
        private readonly GenericRepository<ShipMethod> _repoShipMethods = new GenericRepository<ShipMethod>(new NeutronDb());
        private ReplenProcessor _replenProcessor;

        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private readonly ISapService _sapService;
        private readonly IOrdersRepository _ordersRepository;
        private DocumentToPrint _documentToPrint;
        private DocumentPrinterPreferences _documentPrinter;

        public InterfaceProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, ISapService sapService, IOrdersRepository ordersRepository)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _sapService = sapService;
            _ordersRepository = ordersRepository;
            
            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("NeutronLoader");
            _replenProcessor = new ReplenProcessor();
            _documentToPrint = new DocumentToPrint();
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            try
            {
                _sapService.Init();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error Initializing Loader. {Environment.NewLine} {ex.Message}");
            }

        }
        /// <summary>
        /// Asynchronously loads orders from the SAP service and updates the Neutron orders and Neutron Replen orders.
        /// </summary>
        /// <remarks>
        /// This method first checks if the loading process is already busy. If it is, it logs a message and returns.
        /// If not, it sets the loading process to busy and starts loading orders from the SAP service.
        /// After loading, it updates the Neutron orders and Neutron Replen orders based on the loaded orders.
        /// If any exceptions occur during this process, it logs the error message and sends an email with the error message.
        /// Finally, it sets the loading process to not busy.
        /// </remarks>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task  StartProcessingInterfaceFiles()
        {
            try
            {
                //var startTimeSpan = TimeSpan.Zero;
                //var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
                //_timer = new Timer(t => { _ = LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
                _timer = new Timer(_neutronVariables.LoaderDelay * 1000);
                _timer.Elapsed += async (sender, e) => await LoadOrders();
                _timer.Start();
                await Task.Delay(10);
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
            _timer?.Stop();
            
            if (_loadOrdersBusy)
            {
                _logger.LogDetailAsync("Load Orders is currently busy.").SafeFireAndForget();
                return;
            }

            try
            {
                _loadOrdersBusy = true;
                _logger.LogDetailAsync("Load Orders Testing Waiting 2 Seconds").SafeFireAndForget();


                // MessageBox.Show($"Bypassing call to SAP Server during Testing", "Test Loader",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                _sapService.Run();

                var hostOrderLines = await GetNewOrdersFromSap();

                if (hostOrderLines.Any())
                {
                    UpdateNeutronOrders(hostOrderLines);
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

            _timer?.Start();
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

                    _ = _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.");

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
            var itemDef = _repoItemDefinition.FindBy(r => r.Item == line.Sku && r.AreaId == 8).FirstOrDefault();

            return itemDef;
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
                    _repoOrder.Insert(order);
                    var orderId = order.Id;
                    var orderDetails = hostOrderLines.Where(r => r.Order == orderNumber).ToList();
                    if (orderDetails.Any())
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            var itemDef = GetItemDefinition(orderDetail);

                            if (itemDef != null)
                            {
                                var detail = new OrderDetail()
                                {
                                    PartNum = orderDetail.Sku,
                                    PartDesc = orderDetail.Des,
                                    Quantity = orderDetail.Qty,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,            
                                    OrderDetailInfo = $"{orderDetail.TransId}|{rec.Priority}|{rec.Division}|{@"Route"}|{rec.Upc}",
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

        private ItemDefinition GetItemDefinition(NeutronInput line)
        {
            ItemDefinition itemDef = null;
            // the ItemDefinition could be in multiple Areas and rules determine what Area to pick from.
            // If there are multiple Areas that have the ItemDefinition
            // There is a PickMax value in the ItemDefinition that determines where to pick from.
            // that is also the Area that the ItemDefinition needs to point to

            // Let's get all the Areas where the ItemDefinition might be.
            var itemDefs = _repoItemDefinition.FindBy(r => r.Item == line.Sku).OrderBy(o => o.AreaId).ToList();
            // All ItemDefinitions have a PickMax value unless it is in Area 8
            // Area 8 is the option if the PickMax number is reached
            // If there are multiple ItemDefinitions
            // We need to know if there is a PickMax rule to follow
            // If no PickMax rule, pick All from the lowest/first Area
            if (itemDefs.Count > 0)
            {
                var firstItemDef = itemDefs[0];
                // More than one ItemDefinition
                // Get the first one
                itemDef = firstItemDef;

                // PickMax has a value, now we need to see if the Qty to Pick
                // is larger than the PickMax value
                // if it is, we'll us the ItemDefinition from the Last ItemDefinition

                if (firstItemDef.PickMax > 0)
                {
                    if (line.Qty > firstItemDef.PickMax)
                    {
                        // It is larger, so Pick from the last ItemDefinition
                        itemDef = itemDefs.Last();
                    }
                }

            }

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

            if (itemDefs.Count == 0)
            {
                // create a new ItemDefinition in Area 8 
                // using default values
                var newItemDefinition = _jsonData.LoadFile<ItemDefinition>();
                if (newItemDefinition != null)
                {
                    newItemDefinition.Item = line.Sku;
                    newItemDefinition.Description = line.Des;

                }
                _repoItemDefinition.Insert(newItemDefinition);
                itemDef = newItemDefinition;
            }


            // All ItemDefinitions have a PickMax value unless it is in Area 8
            // Area 8 is the option if the PickMax number is reached
            // If there are multiple ItemDefinitions
            // We need to know if there is a PickMax rule to follow
            // If no PickMax rule, pick All from the lowest/first Area
            if (itemDefs.Count > 1)
            {
                itemDef = itemDefs[1];
            }
            else if (itemDefs.Count == 1)
            {
                itemDef = itemDefs[0];
            }
            return itemDef;
        }

        private async Task<List<NeutronInput>> GetNewOrdersFromReplenishments()
        {
            _logger.LogDetailAsync("Get New Orders From Replenishments").SafeFireAndForget();
            var orderLines = new List<NeutronInput>();
            try
            {
                var recs = _replenProcessor.GetNewAndUpdatedReplenishments();

                if (recs.Any())
                {

                    _ = _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.");
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

                    _ = _logger.LogDetailAsync("Counts Match.  " + recs.Count + " Records to Process.");
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
                    _ = _logger.LogDetailAsync("Update Outbound to Processed was Successful.");
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
            _timer?.Dispose();
        }

        public async Task RunLoaderOnce() => await LoadOrders();
    }
}
