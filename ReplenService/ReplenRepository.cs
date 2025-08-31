using AlliedLogger;
using NeutronData.DataContexts;
using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
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
                    //var testRec = _replenishments.FirstOrDefault(r => r.Item == "89957");
                    //if (testRec != null)
                    //{
                    //    var r = testRec;
                    //}

                    newReplenishments = await context.Database.SqlQuery<Replenishment>("usp_NewItemsWithoutInventory").ToListAsync();
                    //testRec = newReplenishments.FirstOrDefault(r => r.Item == "89957");
                    //if (testRec != null)
                    //{
                    //    var r = testRec;
                    //}

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
                {
                    var replens = await context.Database.SqlQuery<ReplenDelete>("usp_GetReplenishments").ToListAsync();
                    if (replens.Count == 0) return;

                    foreach (var replen in replens)
                    {
                        var orderDetail =
                            await context.OrderDetails.FirstOrDefaultAsync(r => r.Id == replen.OrderDetailId);
                        if (orderDetail != null)
                        {
                            var partNumber = orderDetail.PartNum;
                            var itemDefinition = await context.ItemDefinitions.FirstOrDefaultAsync(r => r.Item == partNumber && r.AreaId != 8);
                            if (itemDefinition == null) continue;
                            var systemMin = itemDefinition.SystemMin;
                            var inventory = await context.Inventory.FirstOrDefaultAsync(r => r.ItemDefinitionId == itemDefinition.Id);
                            if (inventory == null) continue;
                            if (inventory.Quantity > systemMin)
                            {
                                context.OrderDetails.Remove(orderDetail);
                                var order = await context.Orders.FirstOrDefaultAsync(r => r.Id == replen.OrderId);
                                if (order != null)
                                {
                                    context.Orders.Remove(order);
                                }
                            }
                        }

                    }

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Delete Replenishments Error. {Environment.NewLine} {ex.Message}  {Environment.NewLine}{ex.InnerException} ");
            }
        }

        private class ReplenDelete
        {
            public int OrderId { get; set; }
            public int OrderDetailId { get; set; }
        }
    }
}
