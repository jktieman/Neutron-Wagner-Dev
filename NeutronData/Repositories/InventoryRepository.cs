using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using NeutronData.SqlModelViews;
using System.Data.SqlClient;
using System.Globalization;
using AlliedLogger;
using NeutronData.Interfaces;

namespace NeutronData.Repositories
{
    public class InventoryRepository : IDisposable, IInventoryRepository
    {

        private readonly GenericRepository<Inventory> _repo = new GenericRepository<Inventory>(new NeutronDb());

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
                    StationId = r.Location.StationId,
                    StationName = r.Location.Station.Name,
                    StationNumber = r.Location.Station.StationNumber,
                    Loc1 = r.Location.Loc1,
                    Loc2 = r.Location.Loc2,
                    Loc3 = r.Location.Loc3,
                    Loc4 = r.Location.Loc4,
                    Loc5 = r.Location.Loc5,
                    Slot = r.Location.Slot,
                    SizeCodeId = r.Location.SizeCodeId,
                    VelocityCodeId = r.Location.VelocityCodeId,
                    HeightCodeId = r.Location.HeightCodeId,
                    LocationCodeId = r.Location.LocationCodeId,
                    StorageTypeId = r.StorageTypeId,
                    ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                    SizeCodeName = r.Location.SizeCode.Name,
                    VelocityCodeName = r.Location.VelocityCode.Name,
                    HeightCodeName = r.Location.HeightCode.Name,
                    LocationCodeName = r.Location.LocationCode.Name,
                    StorageTypeName = r.StorageType.Name,
                    LocationId = r.LocationId,
                    ItemDefinitionId = r.ItemDefinitionId,
                    ItemDefinition = r.ItemDefinition,
                    Location = r.Location
                }).ToList();
            return projection;
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
                    StationId = r.Location.StationId,
                    StationName = r.Location.Station.Name,
                    StationNumber = r.Location.Station.StationNumber,
                    Loc1 = r.Location.Loc1,
                    Loc2 = r.Location.Loc2,
                    Loc3 = r.Location.Loc3,
                    Loc4 = r.Location.Loc4,
                    Loc5 = r.Location.Loc5,
                    Slot = r.Location.Slot,
                    SizeCodeId = r.Location.SizeCodeId,
                    VelocityCodeId = r.Location.VelocityCodeId,
                    HeightCodeId = r.Location.HeightCodeId,
                    LocationCodeId = r.Location.LocationCodeId,
                    StorageTypeId = r.StorageTypeId,
                    ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                    SizeCodeName = r.Location.SizeCode.Name,
                    VelocityCodeName = r.Location.VelocityCode.Name,
                    HeightCodeName = r.Location.HeightCode.Name,
                    LocationCodeName = r.Location.LocationCode.Name,
                    StorageTypeName = r.StorageType.Name,
                    LocationId = r.LocationId,
                    ItemDefinitionId = r.ItemDefinitionId,
                    ItemDefinition = r.ItemDefinition,
                    Location = r.Location
                };
            }
            return inventoryView;
        }

        public List<InventoryView> GetInventoryViewByItem(string item)
        {
            List<InventoryView> inventoryViews = new List<InventoryView>();
            using (var db = new NeutronDb())
            {
                var recs = db.Inventory.Include("ItemDefinition").Where(r => r.ItemDefinition.Item == item).ToList();

                foreach (var r in recs)
                {
                    var projection = new InventoryView
                    {
                        Id = r.Id,
                        Item = r.ItemDefinition.Item,
                        Description = r.ItemDefinition.Description,
                        Quantity = r.Quantity,
                        StationId = r.Location.StationId,
                        StationName = r.Location.Station.Name,
                        StationNumber = r.Location.Station.StationNumber,
                        Loc1 = r.Location.Loc1,
                        Loc2 = r.Location.Loc2,
                        Loc3 = r.Location.Loc3,
                        Loc4 = r.Location.Loc4,
                        Loc5 = r.Location.Loc5,
                        Slot = r.Location.Slot,
                        SizeCodeId = r.Location.SizeCodeId,
                        VelocityCodeId = r.Location.VelocityCodeId,
                        HeightCodeId = r.Location.HeightCodeId,
                        LocationCodeId = r.Location.LocationCodeId,
                        StorageTypeId = r.StorageTypeId,
                        ReceivedDate = r.ReceivedDate.ToString(CultureInfo.CurrentCulture),
                        SizeCodeName = r.Location.SizeCode.Name,
                        VelocityCodeName = r.Location.VelocityCode.Name,
                        HeightCodeName = r.Location.HeightCode.Name,
                        LocationCodeName = r.Location.LocationCode.Name,
                        StorageTypeName = r.StorageType.Name,
                        LocationId = r.LocationId,
                        ItemDefinitionId = r.ItemDefinitionId,
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

        public List<SqlInventoryView> FindInventoryViewsByStation(string find, int stationId)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter(parameterName: "@Find", value: find);
                    var paramStation = new SqlParameter(parameterName: "@StationId", value: stationId);
                    recs = context.Database.SqlQuery<SqlInventoryView>(sql: "usp_GetInventoryViewFind_Station @Find, @StationId", parameters: new object[] { param, paramStation }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get All Inventory Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public List<SqlInventoryView> FindInventoryViews(string find)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@Find", find);
                   recs = context.Database.SqlQuery<SqlInventoryView>("usp_GetInventoryViewFind @Find", param).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get All Inventory Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
        }

        public List<SqlInventoryView> GetAllInventoryViewsByItemDefinitionId(int id)
        {
            var recs = new List<SqlInventoryView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@ItemId", id);
                    recs = context.Database.SqlQuery<SqlInventoryView>("usp_GetAllInventoryViewsByItemDefinitionId @ItemId", param).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Get All Location Views Error. " + ex.Message + " " + ex.InnerException);
            }

            return recs;
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

        public int GetStationNumber(int itemDefinitionId)
        {
            var stationNumber = 1;
            var rec = _repo.FindBy(r => r.ItemDefinitionId == itemDefinitionId).FirstOrDefault();

            if (rec != null)
            {
                stationNumber = rec.Location.Station.StationNumber;
            }

            return stationNumber;
        }

        public void Dispose()
        {
        }
    }
}