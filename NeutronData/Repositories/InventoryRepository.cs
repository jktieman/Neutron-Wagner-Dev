using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NeutronData.SqlModelViews;
using System.Data.SqlClient;
using System.Globalization;
using AlliedLogger;
using NeutronData.Interfaces;
using System.Threading.Tasks;
using NeutronCore.Enums;

namespace NeutronData.Repositories
{
    public class InventoryRepository : IDisposable, IInventoryRepository
    {
        private readonly Func<NeutronDb> _contextFactory;
        private IDynamicLogger _logger;

        private GenericRepository<Inventory> _repo;

        public InventoryRepository(Func<NeutronDb> contextFactory, IDynamicLogger logger)
        {
            if (contextFactory == null)
            {
                throw new ArgumentNullException("contextFactory");
            }

            _repo = new GenericRepository<Inventory>(contextFactory);
            _contextFactory = contextFactory;
            _logger = logger;

        }
        public List<InventoryView> GetInventoryViewAll()
        {
            List<InventoryView> projection;

            IEnumerable<Inventory> task = _repo.All().ToList();

            projection = task.Select(r => new InventoryView
            {
                Id = r.Id,
                Item = r.ItemDefinition.Item,
                Description = r.ItemDefinition.Description,
                Quantity = r.Quantity,
                AreaId = r.Location.AreaId,
                AreaName = r.Location.Area.Name,
                AreaNumber = r.Location.Area.AreaNumber,
                Loc1 = r.Location.Loc1,
                Loc2 = r.Location.Loc2,
                Loc3 = r.Location.Loc3,
                Loc4 = r.Location.Loc4,
                Loc5 = r.Location.Loc5,
                Slot = r.Location.Slot,
                SizeCodeId = r.Location.SizeCodeId,
                VelocityCodeId = r.Location.VelocityCodeId,
                HeightCodeId = r.Location.HeightCodeId,
                StorageTypeId = r.StorageTypeId,
                ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                SizeCodeName = r.Location.SizeCode.Name,
                VelocityCodeName = r.Location.VelocityCode.Name,
                HeightCodeName = r.Location.HeightCode.Name,
                PickMax = r.ItemDefinition.PickMax,
                LocationCode = r.Location.LocationCode,
                StorageTypeName = r.StorageType.Name,
                LocationId = r.LocationId,
                ItemDefinitionId = r.ItemDefinitionId,
                RFID = r.RFID,
                ItemDefinition = r.ItemDefinition,
                Location = r.Location
            }).ToList();
            return projection;
        }

        public Inventory GetInventoryById(int id)
        {
            using (var context = _contextFactory())
            {
                return context.Inventory.Find(id);
            }
        }
        public InventoryView GetInventoryViewById(int id)
        {
            var inventoryView = new InventoryView();

            Inventory r = _repo.FindByKey(id);

            if (r != null)
            {
                inventoryView = new InventoryView
                {
                    Id = r.Id,
                    Item = r.ItemDefinition.Item,
                    Description = r.ItemDefinition.Description,
                    Quantity = r.Quantity,
                    AreaId = r.Location.AreaId,
                    AreaName = r.Location.Area.Name,
                    AreaNumber = r.Location.Area.AreaNumber,
                    Loc1 = r.Location.Loc1,
                    Loc2 = r.Location.Loc2,
                    Loc3 = r.Location.Loc3,
                    Loc4 = r.Location.Loc4,
                    Loc5 = r.Location.Loc5,
                    Slot = r.Location.Slot,
                    SizeCodeId = r.Location.SizeCodeId,
                    VelocityCodeId = r.Location.VelocityCodeId,
                    HeightCodeId = r.Location.HeightCodeId,
                    StorageTypeId = r.StorageTypeId,
                    ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                    SizeCodeName = r.Location.SizeCode.Name,
                    VelocityCodeName = r.Location.VelocityCode.Name,
                    HeightCodeName = r.Location.HeightCode.Name,
                    PickMax = r.ItemDefinition.PickMax,
                    LocationCode = r.Location.LocationCode,
                    StorageTypeName = r.StorageType.Name,
                    LocationId = r.LocationId,
                    ItemDefinitionId = r.ItemDefinitionId,
                    RFID = r.RFID,
                    ItemDefinition = r.ItemDefinition,
                    Location = r.Location
                };
            }
            return inventoryView;
        }

        public List<InventoryView> GetInventoryViewByItem(string item)
        {
            List<InventoryView> inventoryViews = new List<InventoryView>();
            using (var context = _contextFactory())
            {
                var recs = context.Inventory.Include("ItemDefinition").Include(inventory => inventory.StorageType).Include(inventory => inventory.Location.Area).Include(inventory => inventory.Location.SizeCode).Include(inventory => inventory.Location.VelocityCode).Include(inventory =>
                        inventory.Location.HeightCode)
                    .Where(r => r.ItemDefinition.Item == item).ToList();

                foreach (var r in recs)
                {
                    var projection = new InventoryView
                    {
                        Id = r.Id,
                        Item = r.ItemDefinition.Item,
                        Description = r.ItemDefinition.Description,
                        Quantity = r.Quantity,
                        AreaId = r.Location.AreaId,
                        AreaName = r.Location.Area.Name,
                        AreaNumber = r.Location.Area.AreaNumber,
                        Loc1 = r.Location.Loc1,
                        Loc2 = r.Location.Loc2,
                        Loc3 = r.Location.Loc3,
                        Loc4 = r.Location.Loc4,
                        Loc5 = r.Location.Loc5,
                        Slot = r.Location.Slot,
                        PickSequence = r.Location.PickSequence,
                        SizeCodeId = r.Location.SizeCodeId,
                        VelocityCodeId = r.Location.VelocityCodeId,
                        HeightCodeId = r.Location.HeightCodeId,
                        StorageTypeId = r.StorageTypeId,
                        ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                        SizeCodeName = r.Location.SizeCode.Name,
                        VelocityCodeName = r.Location.VelocityCode.Name,
                        HeightCodeName = r.Location.HeightCode.Name,
                        PickMax = r.ItemDefinition.PickMax,
                        LocationCode = r.Location.LocationCode,
                        StorageTypeName = r.StorageType.Name,
                        LocationId = r.LocationId,
                        ItemDefinitionId = r.ItemDefinitionId,
                        RFID = r.RFID,
                        ItemDefinition = r.ItemDefinition,
                        Location = r.Location

                    };
                    inventoryViews.Add(projection);
                }
            }
            return inventoryViews;
        }

        public List<HotStoreListView> GetHotStoreList(string s)
        {
            var projection = new List<HotStoreListView>();

            IEnumerable<Inventory> task = _repo.All().Where(d => d.ItemDefinition.Item.ToLower().Contains(s)
                            || d.ItemDefinition.Description.ToLower().Contains(s)
                            || d.Location.Slot.Contains(s)).ToList();

            if (task.Any())
            {
                projection = task.Select(r => new HotStoreListView
                {
                    Id = r.Id,
                    Item = r.ItemDefinition.Item,
                    Description = r.ItemDefinition.Description,
                    Quantity = r.Quantity,
                    Slot = r.Location.Slot,
                    PrimeBin = r.PrimeBin

                }).OrderBy(o => o.Item).ThenBy(p => p.Slot).ToList();
            }
            return projection;
        }

        public async Task<List<SqlInventoryView>> FindInventoryViewsByArea(string find, int areaId)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter(parameterName: "@FIND", value: find);
                    var paramArea = new SqlParameter(parameterName: "@AREAID", value: areaId);
                    recs = await context.Database.SqlQuery<SqlInventoryView>(sql: "usp_GetInventoryViewFind_Area @FIND, @AREAID", parameters: new object[] { param, paramArea }).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get All Inventory Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public async Task<List<SqlInventoryView>> FindInventoryViews(string find)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter("@Find", find);
                    recs = await context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryViewFind @Find", param).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get All Inventory Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public List<SqlInventoryView> GetAllInventoryViewsByItemDefinitionId(int id)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = _contextFactory())
                {
                    var param = new SqlParameter("@ItemId", id);
                    recs = context.Database.SqlQuery<SqlInventoryView>("usp_GetAllInventoryViewsByItemDefinitionId @ItemId", param).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get All Location Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public SqlInventoryView GetInventoryViewByItemDefinitionIdAndLocationId(int itemId, int locationId)
        {
            var rec = new SqlInventoryView();
            try
            {
                using (var context = _contextFactory())
                {
                    var paramItemId = new SqlParameter("@ITEMID", itemId);
                    var paramLocationId = new SqlParameter("@LOCATIONID", locationId);
                    rec = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryViewByItemDefinitionIdAndLocationId @ITEMID, @LOCATIONID", paramItemId, paramLocationId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync("Get All Location Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return rec;
        }



        /// <summary>
        /// Gets the Slot field from the Locations Table if the
        /// Inventory record is a Prime Bin
        /// </summary>
        /// <param name="itemDefinitionId">The Id of an <see cref="ItemDefinition"/></param>
        /// <returns>Returns and empty string if there is not a Prime Bin
        /// otherwise it returns the Slot field from the Locations table
        /// </returns>
        public string GetPrimeBin(int itemDefinitionId)
        {
            var slot = string.Empty;
            // Get a list of Inventory records that have this Item
            var recs = _repo.FindBy(r => r.ItemDefinitionId == itemDefinitionId).ToList();

            if (recs.Count <= 0) return slot;
            // If there are records, find the first one that is a PrimeBin
            // and return the Slot from the Locations table
            foreach (var item in recs.Where(item => item.PrimeBin))
            {
                slot = item.Location.Slot;
                break;
            }

            return slot;
        }

        public async Task<List<Inventory>> GetInventoryWithReleaseStorageAndZeroQuantityByArea(int areaId)
        {
            using (var context = _contextFactory())
            {
                return await context.Inventory.Where(i => i.AreaId == areaId && i.Quantity == 0 &&
                                                          i.StorageTypeId == (int)StorageType.Release).ToListAsync();
            }
        }

        public int GetAreaNumber(int itemDefinitionId)
        {
            var areaNumber = 1;
            var rec = _repo.FindBy(r => r.ItemDefinitionId == itemDefinitionId).FirstOrDefault();

            if (rec != null)
            {
                areaNumber = rec.Location.Area.AreaNumber;
            }

            return areaNumber;
        }

        public void Dispose()
        {
            _contextFactory?.Invoke()?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}