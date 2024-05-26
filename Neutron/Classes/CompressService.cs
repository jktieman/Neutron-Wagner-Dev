using Neutron.Models;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using Neutron.Global;
using Timer = System.Timers.Timer;
using NeutronData.Interfaces;
using NeutronData.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Neutron.Classes
{
    public class CompressService : ICompressService
    {
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private readonly NeutronVariables _neutronVariables;
        private readonly IHistoryManager _historyManager;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IReplenOrdersRepository _replenOrdersRepository;
        private IDynamicLogger _logger;
        private Timer _compressTimer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public bool CompressRunning { get; private set; }

        public CompressService(IJsonData jsonData, WorkstationView workstationView, NeutronVariables neutronVariables
        , IHistoryManager historyManager, IOrdersRepository ordersRepository, IReplenOrdersRepository replenOrdersRepository)
        {
            _jsonData = jsonData;
            _workstationView = workstationView;
            _neutronVariables = neutronVariables;
            _historyManager = historyManager;
            _ordersRepository = ordersRepository;
            _replenOrdersRepository = replenOrdersRepository;
            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("Compress");
        }

        public void StartCompressService()
        {
            // Set up a timer that triggers every 
            // The RunCompressInterval is in hours, so the
            // calculation is hours * 60 minutes * 60 seconds * 1000 milliseconds   
            // Testing
            // interval is set to seconds for testing
            // set back to hours for production

            var interval = _neutronVariables.RunCompressInterval * 60 * 1000;
            _compressTimer = new Timer(interval);
           // _compressTimer.Elapsed += async (sender, e) => await OnRunCompress();
            _compressTimer.Elapsed += async (sender, e) =>
            {
                if (_semaphore.CurrentCount == 0)
                {
                    return;
                }
                await _semaphore.WaitAsync();
                try
                {
                    await OnRunCompress();
                }
                finally
                {
                    _semaphore.Release();
                }
            };

            _compressTimer.AutoReset = true;
            _compressTimer.Enabled = true;
            _compressTimer.Start();
        }

        /// <summary>
        /// Stop the CompressService.
        /// </summary>
        /// <returns></returns>
        public async Task StopCompressService()
        {
            await _logger.LogDetailAsync("Stop Compress Service");
            _compressTimer.Stop();
            _compressTimer.Enabled = false;
            _compressTimer.Dispose();
        }

        private async Task OnRunCompress()
        {
            _logger.LogDetailAsync($"On Run Compress: {_compressTimer.Interval}").SafeFireAndForget();
            // Do not run if already running
            if (CompressRunning) return;
           // _compressTimer?.Stop();
            try
            {
                _logger.LogDetailAsync("Compress Started").SafeFireAndForget();
                CompressRunning = true;
                var compressLastRunDate = _jsonData.LoadFile<CompressLastRunDate>();
                var days = (DateTime.Now.Date - compressLastRunDate.DateTime.Date).Days;
                //Run once each day
                if (days > 0)
                {
                    var daysToKeep = _neutronVariables.CompressDays * -1;
                    var compressBefore = DateTime.Now.Date.AddDays(daysToKeep);

                    await CompressOrders(compressBefore);

                    await Task.Delay(2000);
                    await CompressReplenOrders(compressBefore);

                    compressLastRunDate = new CompressLastRunDate { DateTime = DateTime.Now };
                    _jsonData.SaveFile(compressLastRunDate);

                }
                CompressRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Error Running Compress {ex.Message}").SafeFireAndForget();
            }
            finally
            {
                _logger.LogDetailAsync("Compress Finished").SafeFireAndForget();
                CompressRunning = false;
            }
           // _compressTimer?.Stop();
        }

        private async Task CompressOrders(DateTime compressBefore)
        {
            CompressRunning = true;
            // Compress Normal Orders
            _logger.LogDetailAsync($"Compress Orders Before: {compressBefore}").SafeFireAndForget();
            //var completedOrders = _ordersRepository.GetOrderViews("6", "").ToList();
            while (true)
            {
                var completedOrders = _ordersRepository.GetCompletedOrders().ToList();

                if (!completedOrders.Any()) break;

                var ordersToCompress = completedOrders.Where(r => r.LoadDate < compressBefore).Take(50).ToList();

                _logger.LogDetailAsync($"Orders to Compress: {ordersToCompress.Count}").SafeFireAndForget();
                if (!ordersToCompress.Any()) break;
                var orderType = "PICK";
                var sb = new StringBuilder();
                var firstTime = true;
                foreach (var order in ordersToCompress)
                {
                    if (firstTime)
                    {
                        sb.Append(order.Id);
                        firstTime = false;
                    }
                    else
                    {
                        sb.Append(", " + order.Id);
                    }
                }

                var orderIds = sb.ToString();

                try
                {
                    using (var context = new NeutronDb())
                    {
                        var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                        var paramOrderType = new SqlParameter("@ORDERTYPE", orderType);
                        var parameters = new object[] { paramOrderIds, paramOrderType };
                        await context.Database.ExecuteSqlCommandAsync("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds,
                            paramOrderType);
                    }

                    await ArchiveOrders(ordersToCompress);
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync($"Error Compressing Orders {Environment.NewLine}{ex.Message}").SafeFireAndForget();
                }

                await Task.Delay(3000);
            }
        }

        private async Task ArchiveOrders(IEnumerable<Order> orders)
        {
            await Task.Delay(10);
            foreach (var order in orders)
            {
                _logger.LogDetailAsync($"Archive Order ID: {order.Id}  Order: {order.Ord1}").SafeFireAndForget();
                _historyManager.SaveHistory(ActionCode.OrderArchived, order);
            }
        }

        private async Task CompressReplenOrders(DateTime compressBefore)
        {
            CompressRunning = true;
            // Compress Replenishment Orders
            _logger.LogDetailAsync($"Compress Replen Orders Before: {compressBefore}").SafeFireAndForget();

            while (true)
            {
                var completedReplenOrders = _replenOrdersRepository.GetReplenOrderViews("6", "").ToList();

                var replenOrdersToCompress = completedReplenOrders.Where(r => r.LoadDate < compressBefore).Take(50).ToList();
                _logger.LogDetailAsync($"Replen Orders to Compress: {replenOrdersToCompress.Count}").SafeFireAndForget();
                if (!replenOrdersToCompress.Any()) break;
                var orderType = "REPLEN";
                var sb = new StringBuilder();
                var firstTime = true;
                foreach (var order in replenOrdersToCompress)
                {
                    if (firstTime)
                    {
                        sb.Append(order.Id);
                        firstTime = false;
                    }
                    else
                    {
                        sb.Append(", " + order.Id);
                    }
                }

                var orderIds = sb.ToString();

                try
                {

                    await ArchiveReplenOrders(replenOrdersToCompress);

                    using (var context = new NeutronDb())
                    {
                        var paramOrderIds = new SqlParameter("@ORDERIDS", orderIds);
                        var paramOrderType = new SqlParameter("@ORDERTYPE", orderType);
                        var parameters = new object[] { paramOrderIds, paramOrderType };
                        await context.Database.ExecuteSqlCommandAsync("usp_CompressOrders @ORDERIDS, @ORDERTYPE",
                            paramOrderIds,
                            paramOrderType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync(
                        $"Error Compressing Replenishment Orders {Environment.NewLine}{ex.Message}").SafeFireAndForget();
                }
                await Task.Delay(3000);
            }
        }

        private async Task ArchiveReplenOrders(IEnumerable<ReplenOrderView> orders)
        {
            foreach (var order in orders)
            {
                _logger.LogDetailAsync($"Archive Replen Order ID: {order.Id}  Order: {order.Ord1}").SafeFireAndForget();
                _historyManager.SaveHistory(ActionCode.OrderArchived, order);
                await Task.Delay(10);
            }
        }

    }
}
