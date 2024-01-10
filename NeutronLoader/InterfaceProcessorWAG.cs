
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using JsonManager;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using NeutronData.PrintModels;
using NeutronData.Repositories;
using NeutronDllu;
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
        private readonly GenericRepository<Inventory> _repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<Shipper> _repoShippers = new GenericRepository<Shipper>(new NeutronDb());
        private readonly GenericRepository<ShipMethod> _repoShipMethods = new GenericRepository<ShipMethod>(new NeutronDb());

        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private readonly ISapService _sapService;
        private DocumentToPrint _documentToPrint;
        private DocumentPrinterPreferences _documentPrinter;

        public InterfaceProcessorWAG(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, ISapService sapService)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _sapService = sapService;
            _documentToPrint = new DocumentToPrint();
            _documentPrinter = _jsonData.LoadFile<DocumentPrinterPreferences>();
            Init();

        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("NeutronLoader");

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
        public void StartProcessingInterfaceFiles()
        {
            try
            {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
                _timer = new Timer(t => { _ = LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error Processing Interface File. {Environment.NewLine} {ex.Message}");
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
                await _logger.LogDetailAsync("Load Orders is currently busy.");
                return;
            }

            try
            {
                _loadOrdersBusy = true;
                await _logger.LogDetailAsync("Load Orders Testing Waiting 2 Seconds");

                _sapService.Run();



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
            await _logger.LogDetailAsync("Get New Replen Orders From SAP");
            var orderLines = new List<NeutronInput>();
            try
            {
                // get records from SAP Server
                List<NOVA_INPUT> recs;
                // using (var db = new WagnerDb())
                using (var db = new NeutronDb())
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
                                    ItemDefinitionId = itemDef.Id,
                                    TransId = orderDetail.TransId.ParseInt(),

                                };
                                _repoReplenOrderDetail.Insert(detail);
                                _documentToPrint.PrintReplenDoc(detail, _documentPrinter, false);
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
                            ItemDefinition itemDef = null;

                            // the ItemDefinition could be in multiple Areas and rules determine what Area to pick from.
                            // If there are multiple Areas that have the ItemDefinition
                            // There is a PickMax value in the ItemDefinition that determines where to pick from.
                            // that is also the Area that the ItemDefinition needs to point to

                            // Let's get all the Areas where the ItemDefinition might be.
                            var itemDefs = _repoItemDefinition.FindBy(r => r.Item == orderDetail.Sku).OrderBy(o => o.AreaId).ToList();
                            // All ItemDefinitions have a PickMax value unless it is in Area 8
                            // Area 8 is the option if the PickMax number is reached
                            // If there are multiple ItemDefinitions
                            // We need to know if there is a PickMax rule to follow
                            // If no PickMax rule, pick All from the lowest/first Area
                            if (itemDefs.Count > 1)
                            {
                                var firstItemDef = itemDefs[0];
                                // More than one ItemDefinition
                                // Get the first one
                                itemDef = firstItemDef;

                                // PickMax has a value, now we need to see if the Qty to Pick
                                // is larger than the PickMax value
                                // if it is, we'll us the ItemDefinition from the Last ItemDefinition

                                if (orderDetail.Qty >= firstItemDef.PickMax)
                                {
                                    // It is larger, so Pick from the last ItemDefinition
                                    itemDef = itemDefs.Last();
                                }

                            }

                            if (itemDefs.Count == 1)
                            {
                                itemDef = itemDefs[0];
                                
                            }

                            if (itemDefs.Count == 0)
                            {
                                // No ItemDefinition
                                _logger.LogDetailAsync($"No Item Definition for {orderDetail.Sku} in Area ? .");
                            }

                            if (itemDef != null)
                            {
                                var detail = new OrderDetail()
                                {
                                    PartNum = orderDetail.Sku,
                                    PartDesc = orderDetail.Des,
                                    Quantity = orderDetail.Qty,
                                    LineStatusId = (int)LineStatus.Available,
                                    AreaId = itemDef.AreaId,            // GetAreaToPickFrom(orderDetail, itemDef),
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
                }
                catch (Exception ex)
                {
                    var message = $"SAP to Neutron Order Conversion Failure. {Environment.NewLine}{ex.Message}";
                    ErrorAlert(message);
                }
            }
        }

        private int GetAreaToPickFrom(NeutronInput orderDetail, ItemDefinition itemDef)
        {
            if (itemDef.PickMax == 0) return itemDef.AreaId;
            if (orderDetail.Qty < itemDef.PickMax) return itemDef.AreaId;
            if (orderDetail.Qty >= itemDef.PickMax)
            {
                var itemDef2 = _repoItemDefinition.FindBy(r => r.Item == orderDetail.Sku && r.AreaId == 8).FirstOrDefault();
                if (itemDef2 != null)
                {
                    var rec = _repoInventory.FindBy(o => o.ItemDefinitionId == itemDef2.Id).FirstOrDefault();

                    if (rec != null)
                    {
                        return rec.AreaId;
                    }
                }
            }
            return itemDef.AreaId;
        }

        private async Task<List<NeutronInput>> GetNewOrdersFromSap()
        {
            await _logger.LogDetailAsync("Get New Orders From SAP");
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
                await _logger.LogDetailAsync(msg);
                ErrorAlert(msg);
            }
            return orderLines;
        }
        private void UpdateToProcessed(List<NOVA_INPUT> newRecords)
        {
            _ = _logger.LogDetailAsync("Update To Processed.");
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
    }
}
