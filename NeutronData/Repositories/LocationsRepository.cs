using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;
using Logger = NeutronCore.Global.Logger;

namespace NeutronData.Repositories
{

    public class LocationsRepository : ILocationsRepository
    {

        private readonly GenericRepository<Location> _repo = new GenericRepository<Location>(new NeutronDb());
        private readonly IDynamicLogger _logger;

        public LocationsRepository()
        {
            _logger = Logger.SetupLogger(@"LocationsRepository");
        }


        #region Area Related Querys


        #endregion

        public IEnumerable<LocationView> GetAllLocationViewsExact(int areaId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int inUse)
        {
            var recs = new List<LocationView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param1 = new SqlParameter("@AreaId", areaId);
                    var param2 = new SqlParameter("@SizeCodeId", sizeCodeId);
                    var param3 = new SqlParameter("@VelocityCodeId", velocityCodeId);
                    var param4 = new SqlParameter("@HeightCodeId", heightCodeId);
                    var param5 = new SqlParameter("@InUse", inUse);

                    recs = context.Database.SqlQuery<LocationView>(
                        "usp_GetLocationViewsExact @AreaId, @SizeCodeId, @VelocityCodeId, @HeightCodeId, @InUse"
                        , param1, param2, param3, param4, param5).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Exact Error. {ex.Message}{Environment.NewLine} {ex.InnerException}");
            }

            return recs;
        }

        public IEnumerable<LocationView> GetAllLocationViewsExactByInUse(int areaId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int inUse)
        {
            var recs = new List<LocationView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param1 = new SqlParameter("@AreaId", areaId);
                    var param2 = new SqlParameter("@SizeCodeId", sizeCodeId);
                    var param3 = new SqlParameter("@VelocityCodeId", velocityCodeId);
                    var param4 = new SqlParameter("@HeightCodeId", heightCodeId);
                    var param5 = new SqlParameter("@InUse", inUse);

                    recs = context.Database.SqlQuery<LocationView>(
                        "usp_GetLocationViewsExactByInUse @AreaId, @SizeCodeId, @VelocityCodeId, @HeightCodeId, @InUse"
                        , param1, param2, param3, param4, param5).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Exact By InUse Error. {ex.Message}{Environment.NewLine} {ex.InnerException}");
            }

            return recs;
        }

        public int GetMaxSizeCodeByArea(int areaId)
        {
            var result = 9;
            using (var context = new NeutronDb())
            {
                result = context.Locations.Where(r => r.AreaId == areaId).Max(r => r.SizeCodeId);
            }

            return result;
            // return _repo.All().Where(r => r.AreaId == areaId && r.InUse == false).Max(r => r.SizeCodeId);
        }

        public IEnumerable<LocationView> FindLocationViewsByArea(int areaId)
        {
            var recs = new List<LocationView>();

            _ = _logger.LogDetailAsync(@"Get All Location Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var paramAreaId = new SqlParameter("@AREAID", areaId);
                    recs = context.Database.SqlQuery<LocationView>("usp_GetAllLocationViewsByArea @AREAID", paramAreaId).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Error.{Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Location Views End: {recs.Count}");


            return recs;
        }

        public IEnumerable<LocationView> FindLocationViewsByAreaAndInUse(int areaId, int inUse)
        {
            var recs = new List<LocationView>();

            _ = _logger.LogDetailAsync(@"Get All Location Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var paramAreaId = new SqlParameter("@AREAID", areaId);
                    var paramInUse = new SqlParameter("@INUSE", inUse);
                    recs = context.Database.SqlQuery<LocationView>("usp_GetAllLocationViewsByAreaAndInUse @AREAID, @INUSE", paramAreaId, paramInUse).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Error.{Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Location Views End: {recs.Count}");


            return recs;
        }
        public IEnumerable<LocationView> FindLocationViewsByAreaAndSlot(int areaId, string slot)
        {
            var recs = new List<LocationView>();

            _ = _logger.LogDetailAsync(@"Get All Location Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var paramArea = new SqlParameter("@AreaId", areaId);
                    var paramSlot = new SqlParameter("@Slot", slot);
                    recs = context.Database.SqlQuery<LocationView>("usp_GetLocationViewsByAreaAndSlot @AreaId, @Slot", paramArea, paramSlot).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Error.{Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Location Views End: {recs.Count}");

            return recs;
        }
        /// <summary>
        /// Find Locations using any part of the Slot Name 
        /// </summary>
        /// <param name="find">Characters in the Slot Name </param>
        /// <returns></returns>
        public IEnumerable<LocationView> FindLocationViewsBySlot(string find)
        {
            var recs = new List<LocationView>();

            _ = _logger.LogDetailAsync(@"Get All Location Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var paramSlot = new SqlParameter("@FIND", find);
                    recs = context.Database.SqlQuery<LocationView>("usp_GetLocationViewsFind @FIND", paramSlot).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Error.{Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Location Views End: {recs.Count}");

            return recs;  //.Where(r => r.Slot.Contains(find));
        }

        public IEnumerable<LocationView> FindLocationViews(string find = "")
        {
            var recs = new List<LocationView>();

            _ = _logger.LogDetailAsync(@"Get All Location Views Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@Find", find);
                    recs = context.Database.SqlQuery<LocationView>(sql: "usp_GetLocationViewsFind @Find "
                        , parameters: new object[] { param }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Error.   {ex.Message} \r\n {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Location Views End:  {recs.Count}");

            return recs;
        }

        /// <summary>
        /// Sets the Location's InUse field to true or false
        /// depending on what is passed in
        ///  </summary>
        /// <param name="locationId">The Location Id</param>
        /// <param name="b">The true or false value</param>
        public void SetLocationInUse(int locationId, bool b)
        {
            var defaultLocationId = 10954;
            try
            {
                var location = _repo.FindByKey(locationId);
                if (locationId == defaultLocationId)
                {
                    location.InUse = false;
                }
                else
                {
                    location.InUse = b;
                }
                _repo.Update(location);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Set Location In Use Error.   {ex.Message} \r\n {ex.InnerException}");
            }
        }
        /// <summary>
        /// Sets the LocationCode
        /// One use is the record the RFID number of this location 
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="locationCode"></param>
        public void SetLocationCode(int locationId, string locationCode)
        {
            try
            {
                var location = _repo.FindByKey(locationId);
                location.LocationCode = locationCode;
                _repo.Update(location);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Set Location Code Error.   {ex.Message} \r\n {ex.InnerException}");
            }
        }

        /// <summary>
        /// Gets the maximum number of columns 
        /// </summary>
        /// <param name="areaId">The area this is in</param>
        /// <param name="device">The device or section</param>
        /// <param name="tray">The Bin, Level or Tray </param>
        /// <returns>The largest column number on this bin, level or tray</returns>
        public int GetMaxColumns(int areaId, int device, int tray)
        {
            //return 8;
            var i = _repo.All().Where(r => r.AreaId == areaId && r.Loc1 == device && r.Loc2 == tray)
                .Select(s => s.Loc3).Max();
            return i;
        }
        /// <summary>
        /// Gets the maximum number of rows 
        /// </summary>
        /// <param name="areaId">The area this is in</param>
        /// <param name="device">The device or section</param>
        /// <param name="tray">The Bin, Level or Tray </param>
        /// <returns>The largest row number on this bin, level or tray</returns>
        public int GetMaxRows(int areaId, int device, int tray)
        {
            //return 4;
            var i = _repo.All().Where(r => r.AreaId == areaId && r.Loc1 == device && r.Loc2 == tray)
                .Select(s => s.Loc4).Max();
            var j = int.Parse(i.ToString().Substring(0, 1));

            return j;
        }

        /// <summary>
        /// Checks to see if this location is in the Inventory table
        /// It will return true even if the quantity is zero
        /// </summary>
        /// <param name="locationId">This is the Id field in <see cref="Location">Location class</see>/></param>
        /// <returns></returns>
        public async Task<bool> IsInInventory(int locationId)
        {
            var result = false;
            await _logger.LogDetailAsync(@"Check for Location in Inventory Start");
            var parameters = new List<SqlParameter>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter
                    {
                        ParameterName = "@LOCATIONID"
                        ,
                        SqlDbType = SqlDbType.Int
                        ,
                        Value = locationId
                    };
                    parameters.Add(param);

                    var rec = await context.Database.SqlQuery<Inventory>("usp_IsLocationInInventory @LOCATIONID "
                        , parameters.ToArray<object>()).FirstOrDefaultAsync();

                    if (rec != null) result = true;

                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Check for Location in Inventory Error.   {ex.Message} \r\n {ex.InnerException}");
            }

            await _logger.LogDetailAsync($"Check for Location in Inventory End True or False:  {result}");

            return result;
        }
        /// <summary>
        /// This is an overload that takes list of areas 
        /// the LocationCode is for the RFID tag number
        /// </summary>
        /// <param name="areas">List of string Area Numbers</param>
        /// <param name="itemDefinitionSizeCodeId"></param>
        /// <param name="itemDefinitionVelocityCodeId"></param>
        /// <param name="itemDefinitionHeightCodeId"></param>
        /// <param name="inUse"></param>
        /// <returns></returns>
        public IEnumerable<LocationView> GetAllLocationViewsExactByAreas(string areas, int itemDefinitionSizeCodeId
            , int itemDefinitionVelocityCodeId, int itemDefinitionHeightCodeId, int inUse)
        {
            var recs = new List<LocationView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param1 = new SqlParameter("@Areas", areas);
                    var param2 = new SqlParameter("@SizeCodeId", itemDefinitionSizeCodeId);
                    var param3 = new SqlParameter("@VelocityCodeId", itemDefinitionVelocityCodeId);
                    var param4 = new SqlParameter("@HeightCodeId", itemDefinitionHeightCodeId);
                    var param5 = new SqlParameter("@InUse", inUse);

                    recs = context.Database.SqlQuery<LocationView>(
                        "usp_GetLocationViewsExactByAreas @Areas, @SizeCodeId, @VelocityCodeId, @HeightCodeId, @InUse"
                        , param1, param2, param3, param4, param5).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Location Views Exact By Areas Error.  {ex.Message}{Environment.NewLine}{ex.InnerException}");
            }

            return recs;
        }
    }
}