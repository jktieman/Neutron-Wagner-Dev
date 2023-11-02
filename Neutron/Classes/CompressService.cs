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
using JsonManager;
using Neutron.Global;
using Timer = System.Timers.Timer;
using NeutronData.Interfaces;

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
        private static System.Timers.Timer _compressTimer = new System.Timers.Timer();
        static int alarmCounter = 1;
        
        /// <summary>
        /// Controls the CompressService from running.
        /// Set to false to stop the CompressService from running.
        /// </summary>        
        public bool ExitFlag = false;


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
            _logger = NeutronCore.Global.Logger.SetupLogger("Compress");
        }

        public void StartCompressService()
        {
            // Run every RunCompressInterval time 1 hour (3600000)
           // var interval = _neutronVariables.RunCompressInterval * 60 * 60 * 1000;
            var interval = _neutronVariables.RunCompressInterval * 1000;

            _compressTimer.Interval = interval;
            _compressTimer.Elapsed += async (sender, e) => await OnRunCompress();
            _compressTimer.AutoReset = true;
            _compressTimer.Enabled = true;
            _compressTimer.Start();
        }

        private async Task OnRunCompress()
        {
            while (!ExitFlag)
            {
                try
                {
                    // Do not run if already running
                    if (CompressRunning) return;
                    await _logger.LogDetailAsync("Compress Started");
                    CompressRunning = true;
                    var compressLastRunDate = _jsonData.LoadFile<CompressLastRunDate>();
                    var days = (DateTime.Now.Date - compressLastRunDate.DateTime.Date).Days;
                    //Run once each day
                    if (days >= 0)
                    {
                        var daysToKeep = _neutronVariables.CompressDays * -1;
                        var compressBefore = DateTime.Now.Date.AddDays(daysToKeep);

                        await CompressOrders(compressBefore);

                        Thread.Sleep(2000);
                        await CompressReplenOrders(compressBefore);

                        compressLastRunDate = new CompressLastRunDate { DateTime = DateTime.Now };
                        _jsonData.SaveFile(compressLastRunDate);

                    }
                    CompressRunning = false;
                }
                catch (Exception ex)
                {
                    await _logger.LogDetailAsync($"Error Running Compress {ex.Message}");
                }
                finally
                {
                    await _logger.LogDetailAsync("Compress Finished");
                    CompressRunning = false;
                }
                Thread.Sleep(1000);
            }
        }

        private async Task CompressOrders(DateTime compressBefore)
        {
            CompressRunning = true;
            // Compress Normal Orders
             await _logger.LogDetailAsync($"Compress Orders Before: {compressBefore}");
            var completedOrders = _ordersRepository.GetOrderViews("6", "").ToList();
            var ordersToCompress = completedOrders.Where(r => r.LoadDate < compressBefore).Take(50).ToList();
            await _logger.LogDetailAsync($"Orders to Compress: {ordersToCompress.Count}");
            if (!ordersToCompress.Any()) return;
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
                await _logger.LogDetailAsync($"Error Compressing Orders {Environment.NewLine}{ex.Message}");
            }
        }

        private async Task ArchiveOrders(IEnumerable<OrderView> orders)
        {
            foreach (var order in orders)
            {
                await _logger.LogDetailAsync($"Archive Order ID: {order.Id}  Order: {order.Ord1}");
                _historyManager.SaveHistory(ActionCode.OrderArchived, order);
            }
        }

        private async Task CompressReplenOrders(DateTime compressBefore)
        {
            CompressRunning = true;
            // Compress Replenishment Orders
            await _logger.LogDetailAsync($"Compress Replen Orders Before: {compressBefore}");
            var completedReplenOrders = _replenOrdersRepository.GetReplenOrderViews("6", "").ToList();
            var replenOrdersToCompress = completedReplenOrders.Where(r => r.LoadDate < compressBefore).ToList();
            await _logger.LogDetailAsync($"Replen Orders to Compress: {replenOrdersToCompress.Count}");
            if (!replenOrdersToCompress.Any()) return;
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
                    await context.Database.ExecuteSqlCommandAsync("usp_CompressOrders @ORDERIDS, @ORDERTYPE", paramOrderIds,
                        paramOrderType);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error Compressing Replenishment Orders {Environment.NewLine}{ex.Message}");
            }
        }

        private async Task ArchiveReplenOrders(IEnumerable<ReplenOrderView> orders)
        {
            foreach (var order in orders)
            {
                await _logger.LogDetailAsync($"Archive Replen Order ID: {order.Id}  Order: {order.Ord1}");
                _historyManager.SaveHistory(ActionCode.OrderArchived, order);
            }
        }

    }
}
