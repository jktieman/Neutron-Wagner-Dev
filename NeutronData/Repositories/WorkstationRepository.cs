using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronData.Models.Lookups;
using System.Data.SqlClient;
using StationType = NeutronCore.Enums.StationType;
using DeviceType = NeutronCore.Enums.DeviceTypeEnum;
using Logger = NeutronCore.Global.Logger;
using NeutronCore.Global;


namespace NeutronData.Repositories
{
    public class WorkstationRepository : IWorkstationRepository
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices;
        private readonly GenericRepository<Workstation> _repoWorkstation;
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes;

        private readonly IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        public WorkstationRepository(NeutronVariables neutronVariables, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            _logger = Logger.SetupLogger("WorkStationRepository");
            _neutronVariables = neutronVariables;

            _repoHardwareDevices = new GenericRepository<HardwareDevice>(contextFactory);
            _repoWorkstation = new GenericRepository<Workstation>(contextFactory);
            _repoCommunicationTypes = new GenericRepository<CommunicationType>(contextFactory);
        }
        /// <summary>
        /// Based on the workstationId, get the workstation and all the hardware devices associated with it.
        /// </summary>
        /// <param name="workstationId">The Id of the current workstation </param>
        /// <returns>The Id of an <see cref="WorkstationView"/></returns>
        public WorkstationView GetStationView(int workstationId) // ws
        {
            WorkstationView workstationView = null;

            // Dictionary of Communication Types
            //var dicCommunicationTypes = _repoCommunicationTypes.All().AsNoTracking().ToDictionary(d => d.Id, d => d.Name);

            try
            {
               // var workstation = _repoWorkstation.FindByKey(workstationId);

                var workstation = _repoWorkstation.FindByKeyInclude(
                    r => r.Id == workstationId,
                    r => r.Area,
                    r => r.StationType);


                if (workstation != null)
                {
                    _ = _logger.LogDetailAsync($"Workstation Name: {workstation.Name}");
                    // create the WorkstationView object
                    workstationView = new WorkstationView
                    {
                        AreaId = workstation.AreaId,
                        Area = workstation.Area,
                        StationTypeId = workstation.StationTypeId,
                        StationType = workstation.StationType,
                        WorkstationId = workstation.Id,
                        WorkstationNumber = workstation.StationNumber,
                        Name = workstation.Name,
                        Sequence = workstation.Sequence,
                        Workstation = workstation
                    };

                    // if the workstation is a supervisor, return the workstationView
                    if (workstationView.StationTypeId == (int)StationType.Supervisor) return workstationView;
                }
                else  //workstation = null
                {
                    _ = _logger.LogDetailAsync("Workstation is null");
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error finding workstation.  {ex.Message}  Inner:  {ex.InnerException}");
            }
            return workstationView;
        }

        public int GetStationId(int stationNumber)
        {

            var workstationId = 0;
            var result = _repoWorkstation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();
            if (result != null)
            {
                workstationId = result.Id;
            }
            return workstationId;
        }

        public Workstation GetStation(int id)
        {
            return _repoWorkstation.FindByKeyInclude(
                r => r.Id == id,
                w => w.Area,
                w => w.StationType);
        }

        public List<Workstation> Lookup()
        {
            var stations = _repoWorkstation.AllInclude(null, w => w.Area, w => w.StationType).ToList();
            return stations;
        }

        public List<string> GetPickStationNumbers()
        {
            var result = new List<string>();
            var stations = GetPickStations();
            if (stations.Count > 0)
            {
                foreach (var workstation in stations)
                {
                    result.Add(workstation.Id.ToString());
                }
            }
            return result;
        }

        public int[] GetPickStationIds()
        {
            var result = GetPickStations().Select(r => r.Id).ToArray();

            return result;
        }

        //public List<Workstation> GetPickStations()
        //{

        //    var result = new List<Workstation>();
        //    var stations = _repoWorkstation.All().Where(r => r.StationType.Name == StationType.Carousel.ToString()
        //                                                 || r.StationType.Name == StationType.RackTablet.ToString()
        //                                                 || r.StationType.Name == StationType.Vertical.ToString())
        //        .ToList();

        //    if (stations.Count > 0)
        //    {
        //        result = stations;
        //    }
        //    return result;
        //}

        public List<Workstation> GetPickStations()
        {
            var pickableTypeIds = new[]
            {
                (int)StationType.Carousel,
                (int)StationType.RackTablet,
                (int)StationType.Vertical
            };

            var result = new List<Workstation>();
            var stations = _repoWorkstation.All(r => pickableTypeIds.Contains(r.StationTypeId))
                .ToList();

            if (stations.Count > 0)
            {
                result = stations;
            }
            return result;
        }

        //public List<Workstation> GetAllPickStations()
        //{

        //    var result = new List<Workstation>();

        //    //var stations = _repoWorkstation.AllInclude(r => r.StationType)
        //    //    .Where(r => r.StationType.Name != StationType.Supervisor.ToString()).ToList();

        //    var stations = _repoWorkstation.AllInclude(
        //            r => r.StationType.Name != StationType.Supervisor, // Filtering logic moved here
        //            r => r.StationType)                                           // Include related StationType entity
        //        .ToList();


        //    if (stations.Count > 0)
        //    {
        //        result = stations;
        //    }
        //    return result;
        //}
        public List<Workstation> GetAllPickStations()
        {
            var supervisorTypeId = (int)StationType.Supervisor;

            var result = new List<Workstation>();
            var stations = _repoWorkstation.AllInclude(
                    r => r.StationTypeId != supervisorTypeId,
                    r => r.StationType)
                .ToList();

            if (stations.Count > 0)
            {
                result = stations;
            }
            return result;
        }
        public int[] GetAllPickStationIds()
        {
            var result = GetAllPickStations().Select(r => r.Id).ToArray();

            return result;
        }

        public int[] GetMoveablePickStationIds()
        {
            var result = GetMovablePickStations().Select(r => r.Id).ToArray();

            return result;
        }

        public List<Workstation> GetMovablePickStations()
        {
            var deviceTypesThatMove = new[] { (int)StationType.Carousel,
                (int)StationType.Vertical };

            var result = new List<Workstation>();
            var workstations = _repoWorkstation.AllInclude(null, w => w.HardwareDevices).OrderBy(o => o.Sequence).ToList();

            foreach (var workstation in workstations)
            {
                var devices = workstation.HardwareDevices
                    .Where(r => deviceTypesThatMove.Contains(r.DeviceTypeId))
                    .ToList();

                if (!devices.Any()) continue;
                result.Add(workstation);
            }
            return result;

            //var deviceTypesThatMove = new[] { (int)StationType.Carousel,
            //    (int)StationType.Vertical };  // 1-Carousel 2-Vertical
            //var result = new List<Workstation>();


            //foreach (var workstation in _repoWorkstation.All().OrderBy(o => o.Sequence))
            //{
            //    var devices = _repoHardwareDevices.All().Where(r => deviceTypesThatMove.Contains(r.DeviceTypeId) && r.WorkstationId == workstation.Id)
            //        .ToList();
            //    if (!devices.Any()) continue;
            //    workstation.HardwareDevices.AddRange(devices);
            //    result.Add(workstation);
            //}
            //return result;
        }

        public Workstation GetRackStation(int workstationId)
        {
            return _repoWorkstation.FindByKeyInclude(
                r => r.Id == workstationId,
                w => w.Area,
                w => w.StationType);
        }

        public WorkstationView GetRackStationView()
        {
            var workstation = _repoWorkstation.AllInclude(
                    r => r.StationTypeId == (int)StationType.RackTablet,
                    w => w.StationType)
                .FirstOrDefault();

            if (workstation == null) return new WorkstationView();

            var workstationView = new WorkstationView
            {
                WorkstationId = workstation.Id,
                Name = workstation.Name,
                WorkstationNumber = workstation.StationNumber,
                StationType = workstation.StationType,
                Sequence = workstation.Sequence,
            };
            return workstationView;
        }

        public int[] GetMoveableDeviceTypeIds()
        {
            return new[]
            {
                (int) StationType.Carousel,
                (int) StationType.Vertical
            };
        }

        public List<Workstation> GetStationsByArea(int areaId)
        {
            var recs = new List<Workstation>();

            _ = _logger.LogDetailAsync(@"Get All Stations by AreaId Start");
            try
            {
                using (var context = new NeutronDb())
                {
                    var param = new SqlParameter("@AREAID", areaId);
                    recs = context.Database.SqlQuery<Workstation>(sql: "usp_GetStationsByArea @AREAID "
                        , parameters: new object[] { param }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Get All Stations by AreaId Error.   {ex.Message} \r\n {ex.InnerException}");
            }

            _ = _logger.LogDetailAsync($"Get All Stations by AreaId End:  {recs.Count}");

            return recs;
        }
    }
}
