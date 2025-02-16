using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace NeutronData.Repositories
{
    public class OrderDetailsRepository  : IOrderDetailsRepository
    {
        private readonly Func<NeutronDb> _contextFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<MemoryCacheOptions> _cacheOptions;

        // private readonly DbContext _context;
        private readonly GenericRepository<OrderDetail> _repoOrderDetails;

        public OrderDetailsRepository(Func<NeutronDb> contextFactory, IMemoryCache memoryCache, IOptions<MemoryCacheOptions> cacheOptions)
        {
            _contextFactory = contextFactory;
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions;
            //  _context = context;
            _repoOrderDetails = new GenericRepository<OrderDetail>(contextFactory);
        }

        public void SetOrderDetailStatusToPicking(int[] currentOrderDetailIds)
        {
            
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                try
                {
                    DataTable idTable = new DataTable();
                    idTable.Columns.Add("Id", typeof(int));
                    // Add each integer from the list to the DataTable
                    foreach (int id in currentOrderDetailIds)
                    {
                        idTable.Rows.Add(id);
                    }

                    // Create the SqlParameter for the table-valued parameter
                    var idParameter = new SqlParameter
                    {
                        ParameterName = "@IDS",
                        SqlDbType = SqlDbType.Structured,
                        TypeName = "dbo.IdTableType", // The name of the user-defined table type
                        Value = idTable
                    };
                    var newValueParameter = new SqlParameter
                    {
                        ParameterName = "@NEWVALUE",
                        SqlDbType = SqlDbType.Int,
                        Value = (int)(LineStatus.Picking)
                    };
                    using (var context = new NeutronDb())
                    {
                         context.Database.ExecuteSqlCommandAsync("usp_UpdateStatus_OrderDetails @IDS, @NEWVALUE", idParameter,
                        newValueParameter);
                    }
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                stopWatch.Stop();
                Console.WriteLine($"Set OrderDetail StatusToPicking - Elapsed time: {stopWatch.ElapsedMilliseconds} ms");
             
        }


        public void PreloadCache()
        {
            using (var context = _contextFactory())
            {
                // how do I get the last 500 records of a table using Entity Framework
                var orderDetails = context.OrderDetails.Include("ItemDefinitions")
                    .OrderByDescending(r => r.Id)
                    .Take(500)
                    .ToList();

                // Cache entry options
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                };
                foreach (var orderDetail in orderDetails)
                {
                    _memoryCache.Set(orderDetail.Id, orderDetail, cacheEntryOptions);
                }
            }
        }

        // Pass in an array of OrderDetail Ids and set each OrderDetail records LineStatusId to 3

        public void SetOrderDetailStatus(int[] orderDetailIds, int statusId)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                DataTable idTable = new DataTable();
                idTable.Columns.Add("Id", typeof(int));
                // Add each integer from the list to the DataTable
                foreach (int id in orderDetailIds)
                {
                    idTable.Rows.Add(id);
                }
                
                // Create the SqlParameter for the table-valued parameter
                var idParameter = new SqlParameter
                {
                    ParameterName = "@IDS",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.IdTableType", // The name of the user-defined table type
                    Value = idTable
                };
                var newValueParameter = new SqlParameter
                {
                    ParameterName = "@STATUS",
                    SqlDbType = SqlDbType.Int,
                    Value = statusId
                };
                using (var context = new NeutronDb())
                {
                    context.Database.ExecuteSqlCommand("EXEC usp_UpdateStatus_OrderDetails @IDS, @STATUS", idParameter,
                        newValueParameter);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            stopWatch.Stop();
            Console.WriteLine($"Set OrderDetail StatusToPicking - Elapsed time: {stopWatch.ElapsedMilliseconds} ms");

        }

        //public async Task SaveAsync()
        //{
        //    await _context.SaveChangesAsync();
        //}

        //public void Save()
        //    {
        //        _context.SaveChanges();
        //    }


        //public List<OrderDetailsView> GetOrderDetailsView()
        //{
        //    var statusToGet = new int[] { 1, 2, 3, 4 };
        //    List<OrderDetailsView> recs = new List<OrderDetailsView>();
        //    try
        //    {

        //        recs = _repoOrderDetails.AllInclude(r => r.Order, r => r.ItemDefinition)
        //        .Where(r => statusToGet.Contains(r.LineStatusId))
        //        .Select(s => new OrderDetailsView
        //        {
        //            OrderId = s.OrderId,
        //            Ord1 = s.Order.Ord1,
        //            Ord2 = s.Order.Ord2,
        //            OrderDetailId = s.Id,
        //            ItemId = s.ItemDefinitionId,
        //            Item = s.ItemDefinition.Item,
        //            Description = s.ItemDefinition.Description,
        //            Quantity = s.Quantity,
        //            PickedQuantity = s.PickedQuantity,
        //            LineStatusId = s.LineStatusId,
        //            LineStatusName = s.LineStatus.Name,
        //            StationNumber = s.StationNumber
        //        }).Where(s => statusToGet.Contains(s.LineStatusId))
        //    .OrderBy(o => o.Ord1).ToList();
        //    }
        //    catch (Exception e)
        //    {
        //        // ignored
        //    }
        //    return recs;
        //}

        public List<OrderDetailsView> GetOrderDetailsViewByOrder(int orderId)
            {
                var recs = new List<OrderDetailsView>();
                try
                {
                    recs = _repoOrderDetails.AllInclude(r => r.Order, r => r.ItemDefinition)
                        .Where(r => r.OrderId == orderId).Select(s => new OrderDetailsView
                        {
                            OrderId = s.OrderId,
                            Ord1 = s.Order.Ord1,
                            Ord2 = s.Order.Ord2,
                            OrderDetailId = s.Id,
                            ItemId = s.ItemDefinitionId,
                            Item = s.ItemDefinition.Item,
                            Description = s.ItemDefinition.Description,
                            Quantity = s.Quantity,
                            PickedQuantity = s.PickedQuantity,
                            LineStatusId = s.LineStatusId,
                            LineStatusName = ((LineStatus)s.LineStatusId).GetEnumDescription(),
                            AreaId = s.AreaId
                        }).OrderBy(o => o.AreaId).ThenBy(p => p.Item).ToList();

                }
                catch (Exception)
                {
                    // ignored
                }

                return recs;
            }

            //public List<OrderDetailsView> GetOrderDetailsViewByOrderAndStation(int orderId, int stationNumber)
            //{
            //    List<OrderDetailsView> recs = new List<OrderDetailsView>();
            //    try
            //    {
            //        recs = _repoOrderDetails.AllInclude(r => r.Order, r => r.ItemDefinition)
            //        .Where(r => r.OrderId == orderId && r.StationNumber == stationNumber).Select(s => new OrderDetailsView
            //        {
            //            OrderId = s.OrderId,
            //            Ord1 = s.Order.Ord1,
            //            Ord2 = s.Order.Ord2,
            //            OrderDetailId = s.Id,
            //            ItemId = s.ItemDefinitionId,
            //            Item = s.ItemDefinition.Item,
            //            Description = s.ItemDefinition.Description,
            //            Quantity = s.Quantity,
            //            PickedQuantity = s.PickedQuantity,
            //            LineStatusId = s.LineStatusId,
            //            LineStatusName = s.LineStatus.Name,
            //            StationNumber = s.StationNumber
            //        }).OrderBy(o => o.StationNumber).ThenBy(p => p.Item).ToList();

            //    }
            //    catch (Exception e)
            //    {
            //        // ignored
            //    }

            //    return recs;
            //}

            public List<OrderDetail> GetOrderDetailsByOrderAndWorkstation(int orderId, WorkstationView workstationView)
            {
                var orderDetails = new List<OrderDetail>();
                if (workstationView.AreaId <= 0) return orderDetails;
                {
                    var recs = _repoOrderDetails.All()
                        .Where(r => r.OrderId == orderId && r.AreaId == workstationView.AreaId).ToList();
                    orderDetails.AddRange(recs);
                }
                return orderDetails;
            }

            public Order GetOrder(int orderDetailId)
            {
                var order = _repoOrderDetails.FindByKey(orderDetailId).Order;
                return order;
            }
        
    }
}
