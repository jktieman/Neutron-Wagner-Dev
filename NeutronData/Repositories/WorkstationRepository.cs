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
using StationType = NeutronCore.Enums.StationType;
using DeviceType = NeutronCore.Enums.DeviceTypeEnum;
using RJCP.IO.Ports;
using Logger = NeutronCore.Global.Logger;
using NeutronCore.Global;
using NeutronCore.Models;


namespace NeutronData.Repositories
{
    public class WorkstationRepository : IWorkstationRepository
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());

        private readonly GenericRepository<Workstation> _repoWorkstation = new GenericRepository<Workstation>(new NeutronDb());
        private readonly GenericRepository<NeutronData.Models.Lookups.DeviceType> _repoDeviceTypes = new GenericRepository<NeutronData.Models.Lookups.DeviceType>(new NeutronDb());
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());

        private readonly IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;



        public WorkstationRepository(NeutronVariables neutronVariables)
        {
            _logger = Logger.SetupLogger("WorkStationRepository");
            _neutronVariables = neutronVariables;
        }
        /// <summary>
        /// Based on the workstationId, get the workstation and all the hardware devices associated with it.
        /// </summary>
        /// <param name="workstationId">The Id of the current workstation </param>
        /// <returns>The Id of an <see cref="WorkstationView"/></returns>
        public async Task<WorkstationView> GetStationView(int workstationId) // ws
        {
            //_logger.FolderName = $"{nameof(WorkstationRepository)}_{workstationId}";  // ws
            //var logFileDirectory = LoaderSettings.GetLogFileDirectory();
            //var folderName = $"StationView_{workstationId.ToString()}";
            //var logger = new AlliedLogger.DynamicLogger(logFileDirectory, folderName, @"true");
            WorkstationView workstationView = null;



            // Dictionary of Communication Types
            var dicCommunicationTypes = _repoCommunicationTypes.All().ToDictionary(d => d.Id, d => d.Name);


            try
            {
                var workstation = _repoWorkstation.FindByKey(workstationId);

                if (workstation != null)
                {
                    await _logger.LogDetailAsync($"Workstation Name: {workstation.Name}");
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

                    #region Hardware Devices Setup Old
                    // get all the hardware devices on this workstation; carousel, lights scale, etc
                    //try
                    //{
                    //    //get all the hardware devices on this workstation; carousel, lights scale, etc
                    //    var hardwareDevices = _repoHardwareDevices.All().Where(r => r.WorkstationId == workstation.Id).ToList();
                    //    await _logger.LogDetailAsync($"Workstation Name: " + workstation.Name + " Number of Devices: " + hardwareDevices.Count);

                    //    // loop through the hardware devices
                    //    // get the communication type
                    //    // get the tcp or serial configuration
                    //    // create the hardware device view object
                    //    foreach (var device in hardwareDevices)
                    //    {
                    //        var duh = device.WorkstationId;
                    //        var duh2 = device.Workstation;

                    //        //device.Workstation = workstation;
                    //        // device.WorkstationId = workstation.Id;

                    //        await _logger.LogDetailAsync($"Loading Hardware Device: {device.Name}");
                    //        int key;
                    //        // get the device type
                    //        switch (device.DeviceTypeId)
                    //        {
                    //            // DeviceType = 1 or Shuttle
                    //            case (int)DeviceType.Shuttle:
                    //                {
                    //                    //key = dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                    //                    await _logger.LogDetailAsync($"This is a Shuttle Device");
                    //                    //if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                    //                    key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                    //                    if (device.CommunicationTypeId == key)
                    //                    {
                    //                        await _logger.LogDetailAsync($"This is a TCP Device");
                    //                        var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                    //                        await _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString());
                    //                        if (tcpConfiguration != 0)
                    //                        {
                    //                            try
                    //                            {
                    //                                var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                    //                                await _logger.LogDetailAsync($"TCP Name: {tcp.Name}");
                    //                                device.TcpConfiguration = tcp;
                    //                            }
                    //                            catch (Exception ex)
                    //                            {
                    //                                await _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                device.TcpConfiguration = null;
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            await _logger.LogDetailAsync("Configuration set to null");
                    //                            device.TcpConfiguration = null;
                    //                        }
                    //                        workstationView.HardwareDevices.Add(device);
                    //                    }
                    //                    //if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                    //                    key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                    //                    if (device.CommunicationTypeId == key)
                    //                    {
                    //                        await _logger.LogDetailAsync($"This is a Serial Device");
                    //                        var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                    //                        await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString());
                    //                        if (serialConfiguration != 0)
                    //                        {
                    //                            try
                    //                            {
                    //                                var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                    //                                await _logger.LogDetailAsync($"Serial Name: {serial.Name}");
                    //                                device.SerialConfiguration = serial;
                    //                            }
                    //                            catch (Exception ex)
                    //                            {
                    //                                await _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                device.SerialConfiguration = null;
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            await _logger.LogDetailAsync("Configuration set to null");
                    //                            device.SerialConfiguration = null;
                    //                        }
                    //                        workstationView.HardwareDevices.Add(device);
                    //                    }
                    //                    break;
                    //                }
                    //            // DeviceType = 2 or Carousel
                    //            case (int)DeviceType.Carousel:
                    //                {
                    //                    await _logger.LogDetailAsync(@"This is a Carousel Device");
                    //                    //if (device.CommunicationTypeId == (int) CommunicationType.TCP)
                    //                    key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                    //                    if (device.CommunicationTypeId == key)
                    //                    {
                    //                        await _logger.LogDetailAsync($"This is a TCP Device");
                    //                        var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                    //                        await _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString());
                    //                        if (tcpConfiguration != 0)
                    //                        {
                    //                            try
                    //                            {
                    //                                var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                    //                                await _logger.LogDetailAsync($"TCP Name: {tcp.Name}");
                    //                                device.TcpConfiguration = tcp;
                    //                            }
                    //                            catch (Exception ex)
                    //                            {
                    //                                await _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                device.TcpConfiguration = null;
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            await _logger.LogDetailAsync("Configuration set to null");
                    //                            device.TcpConfiguration = null;
                    //                        }
                    //                        workstationView.HardwareDevices.Add(device);

                    //                    }
                    //                    //if (device.CommunicationTypeId == (int)CommunicationType.Serial)
                    //                    key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                    //                    if (device.CommunicationTypeId == key)
                    //                    {
                    //                        await _logger.LogDetailAsync($"This is a Serial Device");
                    //                        var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                    //                        await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString());
                    //                        if (serialConfiguration != 0)
                    //                        {
                    //                            try
                    //                            {
                    //                                var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                    //                                await _logger.LogDetailAsync($"Serial Name: {serial.Name}  Port: {serial.PortName}");
                    //                                device.SerialConfiguration = serial;
                    //                            }
                    //                            catch (Exception ex)
                    //                            {
                    //                                await _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                device.SerialConfiguration = null;
                    //                            }
                    //                        }
                    //                        else
                    //                        {
                    //                            await _logger.LogDetailAsync("Configuration set to null");
                    //                            device.SerialConfiguration = null;
                    //                        }
                    //                        workstationView.HardwareDevices.Add(device);
                    //                    }
                    //                    key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "None").Key;
                    //                    if (device.CommunicationTypeId == key)
                    //                    {
                    //                        await _logger.LogDetailAsync($"This Device is not controlled.");
                    //                        await _logger.LogDetailAsync("Configuration set to null");
                    //                        device.SerialConfiguration = null;
                    //                        workstationView.HardwareDevices.Add(device);
                    //                    }
                    //                    break;
                    //                }
                    //            // DeviceType = 3 or Rack
                    //            case (int)DeviceType.Rack:   //Rack
                    //                {
                    //                    workstationView.HardwareDevices.Add(device);
                    //                    break;
                    //                }
                    //            // DeviceType = 4 or IPTI
                    //            case (int)DeviceType.IptiDisplays:   //IPTI 
                    //                {
                    //                    // get the communication type
                    //                    var communicationType = _repoCommunicationTypes.FindBy(c => c.Id == device.CommunicationTypeId).FirstOrDefault();
                    //                    if (communicationType == null) break;

                    //                    if (communicationType.Name == "TCP")
                    //                    {
                    //                        var tcpConfig = _repoTcpConfiguration.FindBy(t => t.Id == device.TcpConfigurationId).FirstOrDefault();
                    //                        if (tcpConfig == null) break;
                    //                        device.TcpConfiguration = tcpConfig;

                    //                    }
                    //                    else if (communicationType.Name == "Serial")
                    //                    {

                    //                    }

                    //                    workstationView.HardwareDevices.Add(device);
                    //                    break;
                    //                }
                    //            // DeviceType = 5 or Not Used
                    //            case 5:   //Not Used
                    //                {
                    //                    break;
                    //                }
                    //            // DeviceType = 6 or Remstar Displays
                    //            case (int)DeviceType.RemstarDisplays:   //Remstar BPI/SHI
                    //                {
                    //                    workstationView.HardwareDevices.Add(device);
                    //                    break;
                    //                }
                    //            // DeviceType = 7 or Blastzone
                    //            case (int)DeviceType.Blastzone: //Blastzone
                    //                {
                    //                    workstationView.Blastzone = _blastzone;

                    //                    ////key = dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                    //                    //await _logger.LogDetailAsync($"This is a Shuttle Device");
                    //                    ////if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                    //                    //key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                    //                    //if (device.CommunicationTypeId == key)
                    //                    //{
                    //                    //    await _logger.LogDetailAsync($"This is a TCP Device");
                    //                    //    var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                    //                    //    await _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString()));
                    //                    //    if (tcpConfiguration != 0)
                    //                    //    {
                    //                    //        try
                    //                    //        {
                    //                    //            var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                    //                    //            await _logger.LogDetailAsync($"TCP Name: {tcp.Name}");
                    //                    //            device.TcpConfiguration = tcp;
                    //                    //        }
                    //                    //        catch (Exception ex)
                    //                    //        {
                    //                    //            await _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                    //            device.TcpConfiguration = null;
                    //                    //        }
                    //                    //    }
                    //                    //    else
                    //                    //    {
                    //                    //        await _logger.LogDetailAsync("Configuration set to null");
                    //                    //        device.TcpConfiguration = null;
                    //                    //    }
                    //                    //    workstationView.HardwareDevices.Add(device);
                    //                    //}
                    //                    ////if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                    //                    //key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                    //                    //if (device.CommunicationTypeId == key)
                    //                    //{
                    //                    //    await _logger.LogDetailAsync($"This is a Serial Device");
                    //                    //    var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                    //                    //    await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString()));
                    //                    //    if (serialConfiguration != 0)
                    //                    //    {
                    //                    //        try
                    //                    //        {
                    //                    //            var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                    //                    //            await _logger.LogDetailAsync($"Serial Name: {serial.Name}");
                    //                    //            device.SerialConfiguration = serial;
                    //                    //        }
                    //                    //        catch (Exception ex)
                    //                    //        {
                    //                    //            await _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                    //            device.SerialConfiguration = null;
                    //                    //        }
                    //                    //    }
                    //                    //    else
                    //                    //    {
                    //                    //        await _logger.LogDetailAsync("Configuration set to null");
                    //                    //        device.SerialConfiguration = null;
                    //                    //    }
                    //                    //    workstationView.HardwareDevices.Add(device);
                    //                    //}
                    //                    break;
                    //                }
                    //            // DeviceType = 8 or Hanel12D
                    //            case (int)DeviceType.Hanel12D:
                    //                {

                    //                    await _logger.LogDetailAsync($"This is a Hanel 12D Device");
                    //                    if (device.Enabled)
                    //                    {

                    //                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                    //                        if (device.CommunicationTypeId == key)
                    //                        {
                    //                            await _logger.LogDetailAsync($"This is a TCP Device");
                    //                            var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                    //                            await _logger.LogDetailAsync(@"TCP Configuration number: " + tcpConfiguration.ToString());
                    //                            if (tcpConfiguration != 0)
                    //                            {
                    //                                try
                    //                                {
                    //                                    var tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                    //                                    await _logger.LogDetailAsync($"TCP Name: {tcp.Name}");
                    //                                    device.TcpConfiguration = tcp;
                    //                                }
                    //                                catch (Exception ex)
                    //                                {
                    //                                    await _logger.LogDetailAsync($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                    device.TcpConfiguration = null;
                    //                                }
                    //                            }
                    //                            else
                    //                            {
                    //                                await _logger.LogDetailAsync("Configuration set to null");
                    //                                device.TcpConfiguration = null;
                    //                            }
                    //                            workstationView.HardwareDevices.Add(device);
                    //                        }

                    //                        key = dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                    //                        if (device.CommunicationTypeId == key)
                    //                        {
                    //                            await _logger.LogDetailAsync($"This is a Serial Device");
                    //                            var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                    //                            await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString());
                    //                            if (serialConfiguration != 0)
                    //                            {
                    //                                try
                    //                                {
                    //                                    var serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                    //                                    await _logger.LogDetailAsync($"Serial Name: {serial.Name}");
                    //                                    device.SerialConfiguration = serial;
                    //                                }
                    //                                catch (Exception ex)
                    //                                {
                    //                                    await _logger.LogDetailAsync($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                    //                                    device.SerialConfiguration = null;
                    //                                }
                    //                            }
                    //                            else
                    //                            {
                    //                                await _logger.LogDetailAsync("Configuration set to null");
                    //                                device.SerialConfiguration = null;
                    //                            }
                    //                            workstationView.HardwareDevices.Add(device);
                    //                        }
                    //                    }

                    //                    break;
                    //                }
                    //            // DeviceType = 9 or Hanel12N
                    //            case (int)DeviceType.Hanel12N:
                    //                {
                    //                    break;
                    //                }
                    //            // DeviceType = 10 or ProLite
                    //            case (int)DeviceType.ProLite:
                    //                {
                    //                    await _logger.LogDetailAsync($"This is a ProLite Device");

                    //                    // if the workstationView.ProliteManager is null, create a new ProliteManager
                    //                    if (workstationView.ProliteManager == null)
                    //                    {
                    //                        workstationView.ProliteManager = new ProliteManager(_neutronVariables);
                    //                    }

                    //                    if (device.DeviceType == null)
                    //                    {
                    //                        device.DeviceType = _repoDeviceTypes.FindBy(d => d.Id == device.DeviceTypeId).FirstOrDefault();
                    //                    }

                    //                    if (device.CommunicationType == null)
                    //                    {
                    //                        device.CommunicationType = _repoCommunicationTypes.FindBy(c => c.Id == device.CommunicationTypeId).FirstOrDefault();
                    //                    }

                    //                    if (device.CommunicationType != null && device.CommunicationType.Name == "Serial")
                    //                    {
                    //                        await _logger.LogDetailAsync("This is a Serial Device");
                    //                        var serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                    //                        await _logger.LogDetailAsync(@"Serial Configuration number: " + serialConfiguration.ToString());
                    //                        device.SerialConfiguration = _repoSerialConfiguration.FindBy(s => s.Id == device.SerialConfigurationId).FirstOrDefault();

                    //                    }
                    //                    await _logger.LogDetailAsync($"Adding Prolite Device to ProliteManager");
                    //                    workstationView.ProliteManager.AddProlite(device);
                    //                    break;
                    //                }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    await _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}");
                    //} 
                    #endregion
                }
                else  //workstation = null
                {
                    await _logger.LogDetailAsync("Workstation is null");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogDetailAsync($"Error finding workstation.  {ex.Message}  Inner:  {ex.InnerException}");
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
