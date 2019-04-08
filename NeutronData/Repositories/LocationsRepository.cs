using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronCore;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Repositories
{

    public class LocationsRepository : ILocationsRepository
    {
        private readonly GenericRepository<Location> _repo = new GenericRepository<Location>(new NeutronDb());
        private DynamicLogger _logger;

        public LocationsRepository()
        {
            SetupLogger();
        }

        private void SetupLogger()
        {
            string logFileDir = LoaderSettings.GetLogFileDirectory();
            const string folderName = @"Locations";
            string logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        public IEnumerable<Location> AvailableLocations()
        {
            var list = new List<Location>();
            list = _repo.AllInclude(h => h.HeightCode, h => h.SizeCode, h => h.VelocityCode
                , h => h.LocationCode, h => h.Station).Where(h => h.InUse == false).ToList();
            return list;
        }

        public IEnumerable<Location> AvailableLocationsAll()
        {
            //, h => h.Station, h => h.SizeCode, h => h.VelocityCode, h => h.LocationCode
            var list = new List<Location>();
            list = _repo.AllInclude(h => h.HeightCode).ToList();
            return list;
        }

        public async Task<IEnumerable<LocationView>> GetAllLocationViewsExact(int stationId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int locationCodeId, bool inUse)
        {
            var recs = new List<LocationView>();
            try
            {
                using (var context = new NeutronDb())
                {
                    var param1 = new SqlParameter("@StationId", stationId);
                    var param2 = new SqlParameter("@SizeCodeId", sizeCodeId);
                    var param3 = new SqlParameter("@VelocityCodeId", velocityCodeId);
                    var param4 = new SqlParameter("@HeightCodeId", heightCodeId);
                    var param5 = new SqlParameter("@LocationCodeId", locationCodeId);
                    var param6 = new SqlParameter("@InUse", inUse);

                    recs = await context.Database.SqlQuery<LocationView>(
                        "usp_GetLocationViewsExact @StationId, @SizeCodeId, @VelocityCodeId, @HeightCodeId, @LocationCodeId, @InUse"
                        , param1, param2, param3, param4, param5, param6).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                await Task.Run(() =>
                    _logger.Log($"Get All Location Views Exact Error.  {ex.Message} \r\n {ex.InnerException}"));
            }

            return recs;
        }

        public IEnumerable<LocationView> FindLocationViewsByStation(string find = "", int stationId = 0)
        {
            var recs = new List<LocationView>();
            if (stationId != 0)
            {

                Task.Run(() => _logger.Log(@"Get All Location Views Start"));
                try
                {
                    using (var context = new NeutronDb())
                    {
                        var param = new SqlParameter("@Find", find);
                        var paramInUse = new SqlParameter("@StationId", stationId);
                        recs = context.Database.SqlQuery<LocationView>("usp_GetLocationViewsFindByStation @Find, @StationId", param, paramInUse).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Task.Run(
                        () => _logger.Log($"Get All Location Views Error.   {ex.Message} \r\n {ex.InnerException}"));
                }

                Task.Run(() => _logger.Log($"Get All Location Views End: {recs.Count}"));
            }

            return recs;
        }

        public IEnumerable<LocationView> FindLocationViews(string find = "")
        {
            var recs = new List<LocationView>();

            Task.Run(() => _logger.Log(@"Get All Location Views Start"));
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@Find", find);
                    recs = context.Database.SqlQuery<LocationView>(sql: "usp_GetLocationViewsFind @Find "
                        , parameters: new object[] {param}).ToList();
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Get All Location Views Error.   {ex.Message} \r\n {ex.InnerException}"));
            }

            Task.Run(() => _logger.Log($"Get All Location Views End:  {recs.Count}"));

            return recs;
        }

        public void SetLocationInUse(int locationId, bool b)
        {
            try
            {
                Location location = _repo.FindByKey(locationId);
                location.InUse = b;
                _repo.Update(location);
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Set Location In Use Error.   {ex.Message} \r\n {ex.InnerException}"));
            }
        }
    }
}