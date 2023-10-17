using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlliedLogger;
using NeutronData.Models.Lookups;
using NeutronCore.Enums;
using System.Data.SqlClient;
using StationType = NeutronCore.Enums.StationType;
using Logger = NeutronCore.Global.Logger;
using NeutronCore.Global;

namespace NeutronData.Repositories
{
    public class AreaRepository : IAreaRepository
    {
        private const int Ebin = 4;

        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        private readonly GenericRepository<Workstation> _repoWorkstation = new GenericRepository<Workstation>(new NeutronDb());
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());

        private readonly IDynamicLogger _logger;

        private readonly GenericRepository<Area> _repoArea = new GenericRepository<Area>(new NeutronDb());

        public AreaRepository()
        {
            _logger = Logger.SetupLogger("AreaRepository");
        }

        public int GetAreaId(int areaNumber)
        {
            var areaId = 0;
            var result = _repoArea.FindBy(r => r.AreaNumber == areaNumber).FirstOrDefault();
            if (result != null)
            {
                areaId = result.Id;
            }
            return areaId;
        }

        public Area GetArea(int id)
        {
            return _repoArea.FindByKey(id);
        }
        public List<Area> Lookup()
        {
            var areas = _repoArea.All().OrderBy(o => o.AreaNumber).ToList();
            return areas;
        }

        //public List<string> GetPickStationNumbers()
        //{
        //    var result = new List<string>();
        //    var stations = GetPickStations();
        //    if (stations.Count > 0)
        //    {
        //        foreach (var station in stations)
        //        {
        //            result.Add(station.Id.ToString());
        //        }
        //    }
        //    return result;
        //}

        //public int[] GetPickStationIds()
        //{
        //    var result = GetPickStations().Select(r => r.Id).ToArray();

        //    return result;
        //}

        //public List<Station> GetPickStations()
        //{

        //    var result = new List<Station>();
        //    var stations = _repoWorkstation.All().Where(r => r.StationType.Name == StationType.Carousel.ToString()
        //                                                 || r.StationType.Name == StationType.Rack.ToString()
        //                                                 || r.StationType.Name == StationType.Vertical.ToString())
        //        .ToList();

        //    if (stations.Count > 0)
        //    {
        //        result = stations;
        //    }
        //    return result;
        //}

        //public List<Station> GetAllPickStations()
        //{

        //    var result = new List<Station>();
        //    var stations = _repoWorkstation.All()
        //        .Where(r => r.StationType.Name != StationType.Supervisor.ToString()).ToList();

        //    if (stations.Count > 0)
        //    {
        //        result = stations;
        //    }
        //    return result;
        //}


        public int[] GetAllAreaIds()
        {
            return _repoArea.All().Select(r => r.Id).ToArray();
        }

        public int[] GetAllPickableAreaIds()
        {
            var rec = new int[]{};
            try
            {
                rec = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.Id).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return rec;

        }

        public List<Area> GetAllAreas()
        {
            var recs = new List<Area>();
            try
            {
                recs = _repoArea.All().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return recs;
        }

        public List<Area> GetAllPickableAreas()
        {
            var recs = new List<Area>();
            try
            {
                recs = _repoArea.All().Where(r => r.Pickable == true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return recs;
        }

        public List<int> GetAllPickableAreaNumbers()
        {
            var recs = new List<int>();
            try
            {
                recs = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.AreaNumber).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return recs;
        }

        public List<string> GetAllPickableAreaNumbersAsString()
        {
            var recs = new List<string>();
            try
            {
                recs = _repoArea.All().Where(r => r.Pickable == true).Select(r => r.AreaNumber.ToString()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return recs;
        }

        //public int[] GetAllPickStationIds()
        //{
        //    var result = GetAllPickStations().Select(r => r.Id).ToArray();

        //    return result;
        //}

        //public int[] GetMoveablePickStationIds()
        //{
        //    var result = GetMovablePickStations().Select(r => r.Id).ToArray();

        //    return result;
        //}

        //public List<Station> GetMovablePickStations()
        //{
        //    var deviceTypesThatMove = new[] { (int)StationType.Carousel,
        //        (int)StationType.Vertical };  // 1-Carousel 2-Vertical
        //    var result = new List<Station>();
        //    foreach (var station in _repoWorkstation.All().OrderBy(o => o.Sequence))
        //    {
        //        var devices = _repoHardwareDevices.All().Where(r => deviceTypesThatMove.Contains(r.DeviceTypeId) && r.WorkstationId == station.Id)
        //            .ToList();
        //        if (!devices.Any()) continue;
        //        station.HardwareDevices.AddRange(devices);
        //        result.Add(station);
        //    }
        //    return result;
        //}

        //public Station GetRackStation(int workstationId)
        //{
        //    return _repoWorkstation.FindByKey(workstationId);
        //    //return _repoWorkstation.FindBy(r => r.StationType.Id == (int)NeutronCore.Enums.StationType.Rack).FirstOrDefault();
        //}

        //public WorkstationView GetRackStationView()
        //{
        //    var station = _repoWorkstation.All().FirstOrDefault(r => r.StationTypeId == (int)StationType.Rack);
        //    if (station == null) return new WorkstationView();
        //    var workstationView = new WorkstationView
        //    {
        //        WorkstationId = station.Id,
        //        Name = station.Name,
        //        WorkstationNumber = station.StationNumber,
        //        StationType = station.StationType,
        //        Sequence = station.Sequence,
        //    };
        //    return workstationView;
        //}

        //public int[] GetMoveableDeviceTypeIds()
        //{
        //    return new[]
        //    {
        //        (int) NeutronCore.Enums.StationType.Carousel,
        //        (int) NeutronCore.Enums.StationType.Vertical
        //    };
        //}

        //public List<Station> GetStationsByArea(int areaId)
        //{
        //    var recs = new List<Station>();

        //    Task.Run(() => _logger.Log(@"Get All Stations by AreaId Start"));
        //    try
        //    {
        //        using (var context = new NeutronDb())
        //        {
        //            var param = new SqlParameter("@AREAID", areaId);
        //            recs = context.Database.SqlQuery<Station>(sql: "usp_GetStationsByArea @AREAID "
        //                , parameters: new object[] { param }).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Task.Run(() => _logger.Log($"Get All Stations by AreaId Error.   {ex.Message} \r\n {ex.InnerException}"));
        //    }

        //    Task.Run(() => _logger.Log($"Get All Stations by AreaId End:  {recs.Count}"));

        //    return recs;
        //}
    }
}
