using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Logger = NeutronCore.Global.Logger;

namespace ReplenService
{
    public class ReplenRepository : IReplenRepository
    {
        // get Replenishments from database
        private List<Replenishment> _replenishments;
        private readonly DynamicLogger _logger;
        private readonly NeutronDb _context;
        private const int AreaEight = 8;

        public ReplenRepository(NeutronDb context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), @"The database context cannot be null.");
            }
            _logger = Logger.SetupLogger("ReplenRepository") as DynamicLogger;
            if (_logger == null)
            {
                throw new InvalidOperationException("Logger.SetupLogger did not return a DynamicLogger instance.");
            }
            _context = context;
        }

        public async Task<List<Replenishment>> GetReplenishments()
        {
            _replenishments = new List<Replenishment>();

            try
            {
                _replenishments = await _context.Database.SqlQuery<Replenishment>("usp_CreateReplenishments")
                        .ToListAsync();

                var newReplenishments = await _context.Database.SqlQuery<Replenishment>("usp_NewItemsWithoutInventory")
                    .ToListAsync();

                // append newReplenishments to _replenishments
                _replenishments.AddRange(newReplenishments);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    await _logger.LogDetailAsync(
                        $"Get Replenishments Error. {Environment.NewLine} {ex.Message}  {Environment.NewLine}{ex.InnerException} ");
                }
            }

            return _replenishments;
        }

        private async Task<List<ReplenDelete>> GetReplens()
        {
            var replens = await _context.Database.SqlQuery<ReplenDelete>("usp_GetReplenishments").ToListAsync();

            return replens;
        }

        private async Task<List<Inventory>> GetInventory(int itemDefinitionId)
        {
            var inventory = await _context.Inventory
                .Where(inv => inv.ItemDefinitionId == itemDefinitionId)
                .ToListAsync();

            return inventory;
        }
        private async Task<ItemDefinition> GetItemDefinition(string partNumber)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
            {
                throw new ArgumentException(@"Part number cannot be null or empty.", nameof(partNumber));
            }
            if (_context == null)
            {
                throw new InvalidOperationException("Database context is not initialized.");
            }
            try
            {
                var itemDefinition = await _context.ItemDefinitions
                    .FirstOrDefaultAsync(r => r.Item.ToUpper() == partNumber.ToUpper() && r.AreaId != AreaEight);
                return itemDefinition;
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in GetItemDefinition: {ex.Message}");
                throw;
            }
        }

        private async Task<ItemDefinition> GetItemDefinitionInEight(string partNumber)
        {
            var itemDefinition = await _context.ItemDefinitions.FirstOrDefaultAsync(r => r.Item == partNumber && r.AreaId == AreaEight);

            return itemDefinition;
        }

        private async Task<List<OrderDetail>> GetOrderDetails(List<int> orderDetailIds)
        {
            var orderDetails = await _context.OrderDetails
                    .Where(od => orderDetailIds.Contains(od.Id))
                    .ToListAsync();

            return orderDetails;
        }

        public async Task DeleteReplenishmentsAboveSystemMin()
        {
            List<Inventory> inventoryInEight;
            List<Inventory> inventory; 
            int totalInventory = 0;
            int totalInventoryInEight = 0;
            try
            {
                var replens = await GetReplens();
                if (replens == null || replens.Count == 0) return;
               
                
                var orderDetailIds = replens.Select(r => r.OrderDetailId).ToList();
                var orderDetails = await GetOrderDetails(orderDetailIds);

                foreach (var replen in replens)
                {
                    if (replen.OrderId == 547236)
                    {
                        Debug.WriteLine($"Hello {replen.OrderDetailId}");
                    }
                    var orderDetail = orderDetails.FirstOrDefault(od => od.Id == replen.OrderDetailId);
                    if (orderDetail == null) continue;

                    var partNumber = orderDetail.PartNum;

                    var itemDefinition = await GetItemDefinition(partNumber);
                    if (itemDefinition == null) continue;

                    var itemDefinitionInEight = await GetItemDefinitionInEight(partNumber);
                    if (itemDefinitionInEight != null)
                    {
                         inventoryInEight = await GetInventory(itemDefinitionInEight.Id);
                         totalInventoryInEight = inventoryInEight.Sum(inv => inv.Quantity);
                    }
                    else
                    {
                       totalInventoryInEight = 0;
                    }

                    
                    inventory = await GetInventory(itemDefinition.Id);

                    var totalQuantityInInventory = inventory.Sum(inv => inv.Quantity);
                    if (totalQuantityInInventory > itemDefinition.SystemMin || totalInventoryInEight <= 0)
                    {
                        await DeleteReplenishment(orderDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in {nameof(DeleteReplenishmentsAboveSystemMin)}: {ex.Message}", origin: nameof(DeleteReplenishmentsAboveSystemMin));
                throw;
            }
        }

        public async Task DeleteReplenishment(OrderDetail orderDetail)
        {
            try
            {

                using (var transaction = _context.Database.BeginTransaction())
                {
                    if (orderDetail != null)
                    {
                        // Load the entity within this context before deleting
                        var trackedOrderDetail = await _context.OrderDetails
                            .FirstOrDefaultAsync(od => od.Id == orderDetail.Id);

                        if (trackedOrderDetail != null)
                        {
                            _context.OrderDetails.Remove(trackedOrderDetail);

                            var otherOrderDetails = await _context.OrderDetails
                                .Where(od => od.OrderId == trackedOrderDetail.OrderId && od.Id != trackedOrderDetail.Id)
                                .ToListAsync();

                            if (!otherOrderDetails.Any())
                            {
                                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == trackedOrderDetail.OrderId);
                                if (order != null)
                                {
                                    _context.Orders.Remove(order);
                                }
                            }

                            await _context.SaveChangesAsync();
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in {nameof(DeleteReplenishment)}: {ex.Message}", origin: nameof(DeleteReplenishment));
                throw;
            }
        }


        internal class ReplenDelete
        {
            public int OrderId { get; set; }
            public int OrderDetailId { get; set; }
        }
    }
}
