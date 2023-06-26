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
using System.Data.SqlClient;
using BlastzoneController;
using ProliteController;
using StationType = NeutronCore.Enums.StationType;
using DeviceType = NeutronCore.Enums.DeviceType;

namespace NeutronData.Repositories
{
    public class WorkstationRepository : IWorkstationRepository
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());
        
        private readonly GenericRepository<Workstation> _repoWorkstation = new GenericRepository<Workstation>(new NeutronDb());
        
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());
        
        private readonly IDynamicLogger _logger;
        private readonly IBlastzone _blastzone;
        private readonly IProlite _prolite;

        public WorkstationRepository(IDynamicLogger dynamicLogger, IBlastzone blastzone, IProlite prolite)
        {
            _logger = dynamicLogger;
            _blastzone = blastzone;
            _prolite = prolite;
        }

        public WorkstationView GetStationView(int workstationId) // ws
        {
            _logger.FolderName = $"{nameof(WorkstationRepository)}_{workstationId}";  // ws
            //var logFileDirectory = LoaderSettings.GetLogFileDirectory();
            //var folderName = $"StationView_{workstationId.ToString()}";
            //var logger = new AlliedLogger.DynamicLogger(logFileDirectory, folderName, @"true");
            WorkstationView workstationView = null;
            Workstation workstation; 
            var dicCommunicationTypes = _repoCommunicationTypes.All().ToDictionary(d => d.Id, d => d.Name);
            try
            {
                workstation = _repoWorkstation.FindByKey(workstationId);

                if (workstation != null)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Workstation Name: {workstation.Name}"));
                    //get all the hardware devices on this workstation carousel, lights scale, etc
                    workstationView = new WorkstationView();
                    try
                    {
                        var hardwareDevices = _repoHardwareDevices.All().Where(r => r.WorkstationId == workstation.Id).ToList();
                        Task.Run(() => _logger.LogDetailAsync($"Workstation Name: " + workstation.Name + " Number of Devices: " + hardwareDevices.Count));
                        
                        foreach (var device in hardwareDevices)
                        {
                            Task.Run(() => _logger.LogDetailAsync($"Hardware Device: {device.Name}"));
                            int key;
                            switch (device.DeviceTypeId)
                            {
                                case (int)DeviceType.Shuttle:
                                    {
                                        //key = dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                                        Task.Run(() => _logger.LogDetailAsync($"This is a Shuttle Device"));
                                        //if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a TCP Device"));
                                            var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString()));
                                            if (tcpConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"TCP Name: {tcp.Name}"));
                                                    device.TcpConfiguration = tcp;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.TcpConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                                device.TcpConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        //if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a Serial Device"));
                                            var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString()));
                                            if (serialConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"Serial Name: {serial.Name}"));
                                                    device.SerialConfiguration = serial;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.SerialConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                                device.SerialConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case (int)DeviceType.Carousel:
                                    {
                                        Task.Run(() => _logger.LogDetailAsync(@"This is a Carousel Device"));
                                        //if (device.CommunicationTypeId == (int) CommunicationType.TCP)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a TCP Device"));
                                            var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString()));
                                            if (tcpConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"TCP Name: {tcp.Name}"));
                                                    device.TcpConfiguration = tcp;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.TcpConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                                device.TcpConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);

                                        }
                                        //if (device.CommunicationTypeId == (int)CommunicationType.Serial)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a Serial Device"));
                                            var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString()));
                                            if (serialConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"Serial Name: {serial.Name}  Port: {serial.PortName}"));
                                                    device.SerialConfiguration = serial;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.SerialConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null")) ;
                                                device.SerialConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "None").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This Device is not controlled."));
                                            Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                            device.SerialConfiguration = null;
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case (int)DeviceType.Rack:   //Rack
                                    {
                                        workstationView.HardwareDevices.Add(device);
                                        break;
                                    }
                                case (int)DeviceType.IptiDisplays:   //IPTI 
                                    {
                                        workstationView.HardwareDevices.Add(device);
                                        break;
                                    }
                                case 5:   //Not Used
                                    {
                                        break;
                                    }
                                case (int)DeviceType.RemstarDisplays:   //Remstar BPI/SHI
                                    {
                                        workstationView.HardwareDevices.Add(device);
                                        break;
                                    }
                                case 7: //Blastzone
                                {
                                    workstationView.Blastzone = _blastzone;
                                    ;
                                        ////key = dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                                        //Task.Run(() => _logger.LogDetailAsync($"This is a Shuttle Device"));
                                        ////if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                                        //key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        //if (device.CommunicationTypeId == key)
                                        //{
                                        //    Task.Run(() => _logger.LogDetailAsync($"This is a TCP Device"));
                                        //    var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                        //    Task.Run(() => _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString()));
                                        //    if (tcpConfiguration != 0)
                                        //    {
                                        //        try
                                        //        {
                                        //            var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                        //            Task.Run(() => _logger.LogDetailAsync($"TCP Name: {tcp.Name}"));
                                        //            device.TcpConfiguration = tcp;
                                        //        }
                                        //        catch (Exception ex)
                                        //        {
                                        //            Task.Run(() => _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                        //            device.TcpConfiguration = null;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                        //        device.TcpConfiguration = null;
                                        //    }
                                        //    workstationView.HardwareDevices.Add(device);
                                        //}
                                        ////if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                                        //key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        //if (device.CommunicationTypeId == key)
                                        //{
                                        //    Task.Run(() => _logger.LogDetailAsync($"This is a Serial Device"));
                                        //    var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                        //    Task.Run(() => _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString()));
                                        //    if (serialConfiguration != 0)
                                        //    {
                                        //        try
                                        //        {
                                        //            var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                        //            Task.Run(() => _logger.LogDetailAsync($"Serial Name: {serial.Name}"));
                                        //            device.SerialConfiguration = serial;
                                        //        }
                                        //        catch (Exception ex)
                                        //        {
                                        //            Task.Run(() => _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                        //            device.SerialConfiguration = null;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                        //        device.SerialConfiguration = null;
                                        //    }
                                        //    workstationView.HardwareDevices.Add(device);
                                        //}
                                        break;
                                    }
                                case 8:  // (int)DeviceType.Hanel12D:
                                    {
                                        //key = dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                                        Task.Run(() => _logger.LogDetailAsync($"This is a Hanel 12D Device"));
                                        //if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a TCP Device"));
                                            var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString()));
                                            if (tcpConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"TCP Name: {tcp.Name}"));
                                                    device.TcpConfiguration = tcp;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.TcpConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                                device.TcpConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        //if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This is a Serial Device"));
                                            var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                            Task.Run(() => _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString()));
                                            if (serialConfiguration != 0)
                                            {
                                                try
                                                {
                                                    var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                                    Task.Run(() => _logger.LogDetailAsync($"Serial Name: {serial.Name}"));
                                                    device.SerialConfiguration = serial;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Task.Run(() => _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}"));
                                                    device.SerialConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                                device.SerialConfiguration = null;
                                            }
                                            workstationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case 9:  // (int)DeviceType.Hanel12N:
                                    {
                                        break;
                                    }
                                case 10: // (int)DeviceType.ProLite:
                                {
                                    workstationView.Prolite = _prolite;
                                        break;
                                    }
                            }
                        }
                        workstationView.AreaId = workstation.AreaId;
                        workstationView.Area = workstation.Area;
                        workstationView.StationTypeId = workstation.StationTypeId;
                        workstationView.StationType = workstation.StationType;
                        workstationView.WorkstationId = workstation.Id;
                        workstationView.WorkstationNumber = workstation.StationNumber;
                        workstationView.Name = workstation.Name;
                        workstationView.Sequence = workstation.Sequence;

                        //Task.Run(() => _logger.LogDetailAsync(@"WorkstationView Serial Configuration Name " + workstationView.HardwareDevices.First().SerialConfiguration.Name));
                    }
                    catch (Exception ex)
                    {
                        Task.Run(() => _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}"));
                    }
                }
                else  //workstation = null
                {
                    Task.Run(() => _logger.LogDetailAsync($"Workstation is null"));
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Error finding workstation.  {ex.Message}  Inner:  {ex.InnerException}"));
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
            return _repoWorkstation.FindByKey(id);
        }

        public List<Workstation> Lookup()
        {
            var stations = _repoWorkstation.All().ToList();
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

        public List<Workstation> GetPickStations()
        {

            var result = new List<Workstation>();
            var stations = _repoWorkstation.All().Where(r => r.StationType.Name == StationType.Carousel.ToString()
                                                         || r.StationType.Name == StationType.RackTablet.ToString()
                                                         || r.StationType.Name == StationType.Vertical.ToString())
                .ToList();

            if (stations.Count > 0)
            {
                result = stations;
            }
            return result;
        }

        public List<Workstation> GetAllPickStations()
        {

            var result = new List<Workstation>();

            var stations = _repoWorkstation.All()
                .Where(r => r.StationType.Name != StationType.Supervisor.ToString()).ToList();

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
                (int)StationType.Vertical };  // 1-Carousel 2-Vertical
            var result = new List<Workstation>();


            foreach (var workstation in _repoWorkstation.All().OrderBy(o => o.Sequence))
            {
                var devices = _repoHardwareDevices.All().Where(r => deviceTypesThatMove.Contains(r.DeviceTypeId) && r.WorkstationId == workstation.Id)
                    .ToList();
                if (!devices.Any()) continue;
                workstation.HardwareDevices.AddRange(devices);
                result.Add(workstation);
            }
            return result;
        }

        public Workstation GetRackStation(int workstationId)
        {
            return _repoWorkstation.FindByKey(workstationId);
        }

        public WorkstationView GetRackStationView()
        {
            var workstation = _repoWorkstation.All().FirstOrDefault(r => r.StationTypeId == (int)StationType.RackTablet);
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

            Task.Run(() => _logger.Log(@"Get All Stations by AreaId Start"));
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
                Task.Run(() => _logger.Log($"Get All Stations by AreaId Error.   {ex.Message} \r\n {ex.InnerException}"));
            }

            Task.Run(() => _logger.Log($"Get All Stations by AreaId End:  {recs.Count}"));

            return recs;
        }
    }
}
