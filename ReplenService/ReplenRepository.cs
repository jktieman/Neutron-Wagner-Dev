using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NeutronCore.Global.Logger;

namespace ReplenService
{
    public class ReplenRepository
    {
        // get Replenishments from database
        private List<Replenishment> _replenishments;
        private readonly DynamicLogger _logger;
        private const int AreaEight = 8;
        public ReplenRepository()
        {
            _logger = (DynamicLogger)Logger.SetupLogger("ReplenRepository");
        }



        public async Task<List<Replenishment>> GetReplenishments()
        {

            _replenishments = new List<Replenishment>();

            try
            {
                List<Replenishment> newReplenishments;
                using (var context = new NeutronDb())
                {
                    _replenishments = await context.Database.SqlQuery<Replenishment>("usp_CreateReplenishments").ToListAsync();

                    newReplenishments = await context.Database.SqlQuery<Replenishment>("usp_NewItemsWithoutInventory").ToListAsync();
                }
                // append newReplenishments to _replenishments
                _replenishments.AddRange(newReplenishments);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    await _logger.LogDetailAsync($"Get Replenishments Error. {Environment.NewLine} {ex.Message}  {Environment.NewLine}{ex.InnerException} ");
                }
            }
            return _replenishments;
        }

        public async Task DeleteReplenishmentsAboveSystemMin()
        {
            try
            {
                using (var context = new NeutronDb())
                using (var transaction = context.Database.BeginTransaction())
                {
                    var replens = await context.Database.SqlQuery<ReplenDelete>("usp_GetReplenishments").ToListAsync();
                    if (replens == null || replens.Count == 0) return;
                    var orderDetailIds = replens.Select(r => r.OrderDetailId).ToList();
                    var orderDetails = await context.OrderDetails
                        .Where(od => orderDetailIds.Contains(od.Id))
                        .ToListAsync();
                    foreach (var replen in replens)
                    {
                        var orderDetail = orderDetails.FirstOrDefault(od => od.Id == replen.OrderDetailId);
                        if (orderDetail == null) continue;
                        var partNumber = orderDetail.PartNum;
                        var itemDefinition = await context.ItemDefinitions.FirstOrDefaultAsync(r => r.Item == partNumber && r.AreaId != AreaEight);
                        if (itemDefinition == null) continue;

                        var inventory = await context.Inventory
                            .Where(inv => inv.ItemDefinitionId == itemDefinition.Id)
                            .ToListAsync();
                        var totalQuantityInInventory = inventory.Sum(inv => inv.Quantity);
                        if (totalQuantityInInventory > itemDefinition.SystemMin)
                        {
                            await DeleteReplenishment(orderDetail);
                        }
                    }
                    await context.SaveChangesAsync();
                    transaction.Commit();
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
                using (var context = new NeutronDb())
                using (var transaction = context.Database.BeginTransaction())
                {
                    if (orderDetail != null)
                    {
                        // Load the entity within this context before deleting
                        var trackedOrderDetail = await context.OrderDetails
                            .FirstOrDefaultAsync(od => od.Id == orderDetail.Id);

                        if (trackedOrderDetail != null)
                        {
                            context.OrderDetails.Remove(trackedOrderDetail);

                            var otherOrderDetails = await context.OrderDetails
                                .Where(od => od.OrderId == trackedOrderDetail.OrderId && od.Id != trackedOrderDetail.Id)
                                .ToListAsync();

                            if (!otherOrderDetails.Any())
                            {
                                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == trackedOrderDetail.OrderId);
                                if (order != null)
                                {
                                    context.Orders.Remove(order);
                                }
                            }

                            await context.SaveChangesAsync();
                            transaction.Commit();
                        }
                    }
                    //if (orderDetail != null)
                    //{
                    //    context.OrderDetails.Remove(orderDetail);
                    //    var otherOrderDetails = await context.OrderDetails
                    //    .Where(od => od.OrderId == orderDetail.OrderId && od.Id != orderDetail.Id)
                    //    .ToListAsync();
                    //    if (!otherOrderDetails.Any())
                    //    {
                    //        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderDetail.OrderId);
                    //        if (order != null)
                    //        {
                    //            context.Orders.Remove(order);
                    //        }
                    //    }
                    //}

                    //await context.SaveChangesAsync();
                    //transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error in {nameof(DeleteReplenishmentsAboveSystemMin)}: {ex.Message}", origin: nameof(DeleteReplenishmentsAboveSystemMin));
                throw;
            }
        }
        //public async Task DeleteReplenishmentsAboveSystemMin()
        //{
        //    try
        //    {
        //        using (var context = new NeutronDb())
        //        {
        //            var replens = await context.Database.SqlQuery<ReplenDelete>("usp_GetReplenishments").ToListAsync();
        //            if (replens == null || replens.Count == 0) return;

        //            foreach (var replen in replens)
        //            {
        //                var orderDetail =
        //                    await context.OrderDetails.FirstOrDefaultAsync(r => r.Id == replen.OrderDetailId);
        //                if (orderDetail != null)
        //                {
        //                    var itemDefinition = await context.ItemDefinitions.FirstOrDefaultAsync(r => r.Id == orderDetail.ItemDefinitionId);
        //                    if (itemDefinition == null) continue;
        //                    var systemMin = itemDefinition.SystemMin;
        //                    var inventory = await context.Inventory.Where(r => r.ItemDefinitionId == itemDefinition.Id).ToListAsync();
        //                    var totalQuantityInInventory = inventory.Sum(r => r.Quantity);
        //                    if (totalQuantityInInventory > systemMin)
        //                    {
        //                        context.OrderDetails.Remove(orderDetail);
        //                        await context.SaveChangesAsync();
        //                        //check if there are any other order details for this order
        //                        var otherOrderDetails = await context.OrderDetails.Where(r => r.OrderId == replen.OrderId).ToListAsync();
        //                        if (otherOrderDetails.Count > 0) continue;
        //                        //if none, delete the order
        //                        var order = await context.Orders.FirstOrDefaultAsync(r => r.Id == replen.OrderId);
        //                        if (order != null)
        //                        {
        //                            context.Orders.Remove(order);
        //                            await context.SaveChangesAsync();
        //                        }
        //                    }
        //                }
        //            }
        //            await context.SaveChangesAsync();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.LogDetailAsync($"Delete Replenishments Error. {Environment.NewLine} {ex.Message}  {Environment.NewLine}{ex.InnerException} ");
        //    }
        //}

        private class ReplenDelete
        {
            public int OrderId { get; set; }
            public int OrderDetailId { get; set; }
        }
    }
}
