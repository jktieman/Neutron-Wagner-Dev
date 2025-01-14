using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore.Enums;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronEvents;


namespace ReplenService
{
    public class ReplenProcessor
    {
        private readonly GenericRepository<Order> _repoOrder = new GenericRepository<Order>(new NeutronDb());
        private readonly GenericRepository<OrderDetail> _repoOrderDetail = new GenericRepository<OrderDetail>(new NeutronDb());
        private readonly GenericRepository<ReplenOrder> _repoReplenOrder = new GenericRepository<ReplenOrder>(new NeutronDb());
        private readonly GenericRepository<ReplenOrderDetail> _repoReplenOrderDetail = new GenericRepository<ReplenOrderDetail>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> _repoItemDefinitions = new GenericRepository<ItemDefinition>(new NeutronDb());

        private ReplenRepository _replenRepository;
        private IDynamicLogger _logger;

        public ReplenProcessor()
        {
            Init();
        }
        private void Init()
        {
            _replenRepository = new ReplenRepository();
            _logger = NeutronCore.Global.Logger.SetupLogger("ReplenProcessor");
            Mediator.GetInstance().OrderComplete += (s, e) => CheckForReplenOrder(e.Order);
        }

        private void CheckForReplenOrder(Order order)
        {
            if (order == null) return;
            var ord = _repoOrder.FindByKey(order.Id);
            if (ord.Ord2.Contains("REPLEN"))
            {
                CreateReplenishmentStoreOrder(order);
            }
        }
        
        
        /// <summary>
        /// Retrieves a list of new and updated replenishments.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Replenishment"/> objects that are either new or have been updated.
        /// </returns>
        /// <remarks>
        /// This method fetches all replenishments from the repository, processes each replenishment, and filters out any null records.
        /// Processing of a replenishment includes checks and operations related to quantity needed, existing replenishment picks, and existing replenishment stores.
        /// </remarks>
        public async Task<List<Replenishment>> GetNewAndUpdatedReplenishments()
        {
            var result = new List<Replenishment>();
            try
            {
                await _replenRepository.DeleteReplenishmentsAboveSystemMin();

                // Get all replenishments
                var replenishments = await _replenRepository.GetReplenishments();
                result = replenishments.Select(ProcessReplenishment).Where(rec => rec != null).ToList();

            }
            catch (Exception ex)
            {
               await _logger.LogDetailAsync(
                    $"Delete Replenishments Above System Min. {Environment.NewLine}{ex.Message} ");
            }

            return result;
        }
        /// <summary>
        /// Processes the given replenishment.
        /// </summary>
        /// <param name="replenishment">The replenishment to process.</param>
        /// <returns>The processed replenishment. If the replenishment does not need to be processed, returns null.</returns>
        /// <remarks>
        /// This method processes a replenishment by checking its quantity and existing Pick or Store orders.
        /// If the quantity needed is less than zero, the method returns null.
        /// If there is an existing replenishment pick that is not complete, the method either deletes it, adjusts its quantity, or leaves it alone and returns null.
        /// If the existing replenishment pick is complete, the method checks for an open replenishment store order.
        /// If there is no existing replenishment pick, the method checks for a store order that has not been picked.
        /// </remarks>
        public Replenishment ProcessReplenishment(Replenishment replenishment)
        {
            ReplenOrder replenStoreOrder;
            // Quantity must be greater than zero to be considered for replenishment
            // if not, return null value
            //if (replenishment.QuantityNeeded < 0) return null;
            // now we have a valid replenishment
            // is it already in the replenishment orders
            var existingReplenishmentPick = GetExistingReplenishmentPick(replenishment.Item);
            // if there is an existing pick and it is not complete
            // either delete, adjust quantity or leave it alone
            if (existingReplenishmentPick != null)
            {
                if (existingReplenishmentPick.OrderStatusId != (int)OrderStatus.Complete)
                {
                    // Replenishment already exists
                    // If the quantityNeeded is zero or a negative number
                    // Remove the order
                    if (replenishment.QuantityNeeded <= 0)
                    {
                        RemoveExistingReplenishmentPick(existingReplenishmentPick);
                    }
                    else
                    {
                        // update the quantity if new 
                        // quantity is not the same as the current ordered quantity
                        //
                        // get the details
                        var detail = _repoOrderDetail.FindBy(r => r.OrderId == existingReplenishmentPick.Id).FirstOrDefault();
                        if (detail != null)
                        {
                            if (replenishment.QuantityNeeded != detail.Quantity)
                            {
                                detail.Quantity = replenishment.QuantityNeeded;
                                _repoOrderDetail.Update(detail);
                            }
                        }
                    }
                    return null;
                }
                else // replenPick order status is Complete
                {
                    // replenPick order status is Complete, but has it been stored, check for open Store order
                    replenStoreOrder = GetExistingReplenishmentStore(replenishment.Item);
                    // if there is a Store Order, do nothing
                    if (replenStoreOrder != null)
                    {
                        return null;
                    }
                    else
                    {
                        // Create a Store Order
                        CreateReplenishmentStoreOrder(existingReplenishmentPick);
                        return null;
                    }
                }
            }

            // Replenishment Pick does not exist
            // it could still be in a Store Order that has Not been put away
            // , add it to the replenishment orders
            if (replenishment.QuantityNeeded == 0)
            {
                return null;
            }
            //check for the replenStore order
            // the replenPick could have been Compressed out
            // if there is a replenStore and not a replenPick
            // assume the replenPick was compressed or deleted
            // return null so you don't create a new replenPick while
            // a replenStore is still available
            replenStoreOrder = GetExistingReplenishmentStore(replenishment.Item);
            if (replenStoreOrder != null)
            {
                // there is a store order that has not been picked
                return null;
            }

            return replenishment;

        }

        private void RemoveExistingReplenishmentPick(Order existingReplenishment)
        {
            try
            {
                // have to delete the OrderDetail first
                // get the OrderDetail
                var orderDetail = _repoOrderDetail.FindBy(o => o.OrderId == existingReplenishment.Id).FirstOrDefault();
                if (orderDetail != null)
                {
                    // _repoOrderDetail.Delete(orderDetail.Id);
                }
                // _repoOrder.Delete(existingReplenishment.Id);
            }
            catch (Exception ex)
            {
               _logger.LogDetail($"Error {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Returns a Replenishment Pick order, even if it is complete
        /// 
        /// </summary>
        /// <param name="replenishmentOrder"></param>
        /// <returns></returns>
        private Order GetExistingReplenishmentPick(string replenishmentOrder)
        {
            var rec = _repoOrder.FindBy(r => r.Ord1 == replenishmentOrder && r.Ord2.Contains("REPLEN")).FirstOrDefault();
            return rec;
        }
        /// <summary>
        /// Returns a Store Order if it is NOT Complete
        /// </summary>
        /// <param name="replenishmentOrder"></param>
        /// <returns></returns>
        private ReplenOrder GetExistingReplenishmentStore(string replenishmentOrder)
        {
            var rec = _repoReplenOrder.FindBy(r => r.Ord1 == replenishmentOrder && r.Ord2.Contains("REPLEN") && r.OrderStatusId != (int)OrderStatus.Complete).FirstOrDefault();
            return rec;
        }
        /// <summary>
        /// Creates a new replenishment store order based on the provided pick order.
        /// </summary>
        /// <param name="pickOrder">The pick order to base the new replenishment store order on.</param>
        /// <remarks>
        /// This method will only create a new replenishment store order if the provided pick order is not null and does not already exist in the repository.
        /// The new replenishment store order will be created with the same Ord1 and Ord2 values as the pick order, and will be inserted into the repository.
        /// If the pick order's Ord2 value ends with a digit that is in the list of possible areas (1, 2, 3, 4), a new replenishment order detail will also be created and inserted into the repository.
        /// If any exceptions occur during this process, they will be logged.
        /// </remarks>
        private void CreateReplenishmentStoreOrder(Order pickOrder)
        {
            if (pickOrder == null) return;
            // <summary>
            // Represents a ReplenOrder object that matches the Ord1 property of the provided pickOrder.
            // This is used to check if a replenishment order already exists for the given pick order.
            // </summary>
            var replenOrderTest = _repoReplenOrder.FindBy(r => r.Ord1 == pickOrder.Ord1).FirstOrDefault();

            if (replenOrderTest != null) return;
            var possibleAreas = new List<int> { 1, 2, 3, 4 };

            try
            {
                var a = pickOrder.Ord2.Last();
                var storeArea = a - '0';
                if (!possibleAreas.Contains(storeArea)) return;
                var storeItemDefinition = GetStoreItemDefinition(pickOrder.Ord1, storeArea);
                var replenOrder = new ReplenOrder()
                {
                    Ord1 = pickOrder.Ord1,
                    Ord2 = pickOrder.Ord2,
                    LoadDate = DateTime.Now,
                    OrderStatusId = (int)OrderStatus.Available,
                    Priority = 0,
                    ShipMethodId = 1,
                    ShipperId = 1,
                    OrderInfo = string.Empty
                };
                _repoReplenOrder.Insert(replenOrder);
                var detail = pickOrder.OrderDetails.FirstOrDefault();
                if (detail == null) return;

                var replenOrderDetail = new ReplenOrderDetail()
                {
                    ReplenOrderId = replenOrder.Id,
                    AreaId = storeArea,
                    DateTime = DateTime.Now.ToShortDateString(),
                    ItemDefinitionId = storeItemDefinition.Id,
                    Quantity = detail.PickedQuantity,
                    PartNum = detail.PartNum,
                    PartDesc = detail.PartDesc,
                    LineStatusId = (int)LineStatus.Available,
                };
                _repoReplenOrderDetail.Insert(replenOrderDetail);

            }
            catch (Exception ex)
            {
                _logger.LogDetail(
                    $"Create Store Order From Replenishment Pick Order. {Environment.NewLine}{ex.Message} ");

            }
        }

        private ItemDefinition GetStoreItemDefinition(string item, int areaId)
        {
            var itemDefinition =
                _repoItemDefinitions.FindBy(r => r.Item == item && r.AreaId == areaId).FirstOrDefault();
            return itemDefinition;
        }
    }
}
