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
using DeviceType = NeutronCore.Enums.DeviceType;

namespace NeutronData.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly GenericRepository<HardwareDevice> _repoHardwareDevices = new GenericRepository<HardwareDevice>(new NeutronDb());

        private readonly GenericRepository<Station> _repoStation = new GenericRepository<Station>(new NeutronDb());
        private readonly GenericRepository<CommunicationType> _repoCommunicationTypes = new GenericRepository<CommunicationType>(new NeutronDb());
        private readonly GenericRepository<TcpConfiguration> _repoTcpConfiguration = new GenericRepository<TcpConfiguration>(new NeutronDb());
        private readonly GenericRepository<SerialConfiguration> _repoSerialConfiguration = new GenericRepository<SerialConfiguration>(new NeutronDb());
        private readonly Dictionary<int, string> _dicCommunicationTypes;
        private readonly IDynamicLogger _logger;


        public StationRepository(IDynamicLogger dynamicLogger)
        {
            _logger = dynamicLogger;
            _dicCommunicationTypes = _repoCommunicationTypes.All().ToDictionary(d => d.Id, d => d.Name);
        }

        public StationView GetStationView(int stationId)
        {
            _logger.FolderName = $"{nameof(StationRepository)}_{stationId}";
            //var logFileDirectory = LoaderSettings.GetLogFileDirectory();
            //var folderName = $"StationView_{stationId.ToString()}";
            //var logger = new AlliedLogger.DynamicLogger(logFileDirectory, folderName, @"true");
            StationView stationView = null;
            Station station;
           // _logger.Log($"Happy New Year");
            try
            {
                station = _repoStation.FindByKey(stationId);

                if (station != null)
                {
                    Task.Run(() => _logger.LogDetailAsync($"Station Name: {station.Name}"));
                    //get all the hardware devices on this station carousel, lights scale, etc
                    stationView = new StationView();
                    try
                    {
                        var hardwareDevices = _repoHardwareDevices.All().Where(r => r.StationId == station.Id).ToList();
                        Task.Run(() => _logger.LogDetailAsync($"Station Name: " + station.Name + " Number of Devices: " + hardwareDevices.Count));
                        foreach (var device in hardwareDevices)
                        {
                            Task.Run(() => _logger.LogDetailAsync($"Hardware Device: {device.Name}"));
                            int key;
                            switch (device.DeviceTypeId)
                            {
                                case (int)DeviceType.Shuttle:
                                    {
                                        //key = _dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                                        Task.Run(() => _logger.LogDetailAsync($"This is a Shuttle Device"));
                                        //if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
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
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        //if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
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
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case (int)DeviceType.Carousel:
                                    {
                                        Task.Run(() => _logger.LogDetailAsync(@"This is a Carousel Device"));
                                        //if (device.CommunicationTypeId == (int) CommunicationType.TCP)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
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
                                            stationView.HardwareDevices.Add(device);

                                        }
                                        //if (device.CommunicationTypeId == (int)CommunicationType.Serial)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
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
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "None").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            Task.Run(() => _logger.LogDetailAsync($"This Device is not controlled."));
                                            Task.Run(() => _logger.LogDetailAsync("Configuration set to null"));
                                            device.SerialConfiguration = null;
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case 3:   //Rack
                                    {
                                        stationView.HardwareDevices.Add(device);
                                        break;
                                    }
                                case 4:   //Remstar BPI 
                                    {
                                        break;
                                    }
                                case 5:   //Remstart SHI
                                    {
                                        break;
                                    }
                                case 6:   //Remstar BPI/SHI
                                    {
                                        break;
                                    }
                            }
                        }

                        stationView.StationTypeId = station.StationTypeId;
                        stationView.StationType = station.StationType;
                        stationView.StationId = station.Id;
                        stationView.StationNumber = station.StationNumber;
                        stationView.Name = station.Name;
                        stationView.Sequence = station.Sequence;

                        //Task.Run(() => _logger.LogDetailAsync(@"StationView Serial Configuration Name " + stationView.HardwareDevices.First().SerialConfiguration.Name));
                    }
                    catch (Exception ex)
                    {
                        Task.Run(() => _logger.LogDetailAsync($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}"));
                    }
                }
                else  //station = null
                {
                    Task.Run(() => _logger.LogDetailAsync($"Station is null"));
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.LogDetailAsync($"Error finding station.  {ex.Message}  Inner:  {ex.InnerException}"));
            }
            return stationView;
        }

        public int GetStationId(int stationNumber)
        {
            var stationId = 0;
            var result = _repoStation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();
            if (result != null)
            {
                stationId = result.Id;
            }
            return stationId;
        }

        public Station GetStation(int id)
        {
            return _repoStation.FindByKey(id);
        }

        public List<Station> Lookup()
        {
            var stations = _repoStation.All().ToList();
            return stations;
        }

        public List<string> GetPickStationNumbers()
        {
            var result = new List<string>();
            var stations = GetPickStations();
            if (stations.Count > 0)
            {
                foreach (var station in stations)
                {
                    result.Add(station.Id.ToString());
                }
            }
            return result;
        }

        public int[] GetPickStationIds()
        {
            var result = GetPickStations().Select(r => r.Id).ToArray();

            return result;
        }

        public List<Station> GetPickStations()
        {

            var result = new List<Station>();
            var stations = _repoStation.All().Where(r => r.StationType.Name == NeutronCore.Enums.StationType.Carousel.ToString()
                                                         || r.StationType.Name == NeutronCore.Enums.StationType.Rack.ToString()
                                                         || r.StationType.Name == NeutronCore.Enums.StationType.Vertical.ToString())
                .ToList();

            if (stations.Count > 0)
            {
                result = stations;
            }
            return result;
        }

        public int[] GetMoveablePickStationIds()
        {
            var result = GetMovablePickStations().Select(r => r.Id).ToArray();

            return result;
        }

        public List<Station> GetMovablePickStations()
        {
            var deviceTypesThatMove = new[] { (int)NeutronCore.Enums.StationType.Carousel,
                (int)NeutronCore.Enums.StationType.Vertical };  // 1-Carousel 2-Vertical
            var result = new List<Station>();
            foreach (var station in _repoStation.All().OrderBy(o => o.Sequence))
            {
                var devices = _repoHardwareDevices.All().Where(r => deviceTypesThatMove.Contains(r.DeviceTypeId) && r.StationId == station.Id)
                    .ToList();
                if (!devices.Any()) continue;
                station.HardwareDevices.AddRange(devices);
                result.Add(station);
            }
            return result;
        }

        public Station GetRackStation()
        {
            return _repoStation.FindBy(r => r.StationType.Id == (int)NeutronCore.Enums.StationType.Rack).FirstOrDefault();
        }

        public StationView GetRackStationView()
        {
            var station = _repoStation.All().FirstOrDefault(r => r.StationTypeId == (int)NeutronCore.Enums.StationType.Rack);
            if (station == null) return new StationView();
            var stationView = new StationView
            {
                StationId = station.Id,
                Name = station.Name,
                StationNumber = station.StationNumber,
                StationType = station.StationType,
                Sequence = station.Sequence,
            };
            return stationView;
        }

        public int[] GetMoveableDeviceTypeIds()
        {
            return new[]
            {
                (int) NeutronCore.Enums.StationType.Carousel,
                (int) NeutronCore.Enums.StationType.Vertical
            };
        }
    }
}
