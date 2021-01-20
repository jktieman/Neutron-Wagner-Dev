using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronCore;
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
        private Dictionary<int, string> _dicCommunicationTypes;

        public StationRepository()
        {
            _dicCommunicationTypes = _repoCommunicationTypes.All().ToDictionary(d => d.Id, d => d.Name);
        }

        public StationView GetStationView(int stationNumber)
        {
            var logFileDirectory = LoaderSettings.GetLogFileDirectory();
            var folderName = $"StationView_{stationNumber.ToString()}";
            var logger = new AlliedLogger.DynamicLogger(logFileDirectory, folderName, @"true");
            var stationView = new StationView();
            Station station;
            try
            {
                station = _repoStation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();

                if (station != null)
                {
                    logger.Log($"Station Name: {station.Name}");
                    //get all the hardware devices on this station carousel, lights scale, etc
                    try
                    {
                        var hardwareDevices = _repoHardwareDevices.All().Where(r => r.StationId == station.Id).ToList();
                        logger.Log("Station Name: " + station.Name + " Number of Devices: " + station.HardwareDevices.Count.ToString());
                        foreach (var device in hardwareDevices)
                        {
                            logger.Log($"Hardware Device: {device.Name}");
                            int key;
                            switch (device.DeviceTypeId)
                            {
                                case (int) DeviceType.Shuttle:
                                {
                                    key = _dicCommunicationTypes.FirstOrDefault(d => d.Value =="TCP").Key;
                                        logger.Log($"This is a Shuttle Device");
                                        //if (device.CommunicationTypeId == _repoCommunicationTypes.FindBy(c => c.Name.Equals("TCP", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault()?.Id)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            logger.Log($"This is a TCP Device");
                                            var tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                            logger.Log(@"TCP Configuration number: " + tcpConfiguration.ToString());
                                            if (tcpConfiguration != 0)
                                            {
                                                try
                                                {
                                                    TcpConfiguration tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                                    logger.Log($"TCP Name: {tcp.Name}");
                                                    device.TcpConfiguration = tcp;
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Log($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                                                    device.TcpConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                logger.Log("Configuration set to null");
                                                device.TcpConfiguration = null;
                                            }
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        //if (device.CommunicationTypeId == (int) CommunicationType.Serial)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            logger.Log($"This is a Serial Device");
                                            int serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                            logger.Log(@"Serial Configuration number: " + serialConfiguration.ToString());
                                            if (serialConfiguration != 0)
                                            {
                                                try
                                                {
                                                    SerialConfiguration serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                                    logger.Log($"Serial Name: {serial.Name}");
                                                    device.SerialConfiguration = serial;
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Log($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                                                    device.SerialConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                logger.Log("Configuration set to null");
                                                device.SerialConfiguration = null;
                                            }
                                            stationView.HardwareDevices.Add(device);
                                        }
                                        break;
                                    }
                                case (int) DeviceType.Carousel:
                                    {
                                        logger.Log(@"This is a Carousel Device");
                                        //if (device.CommunicationTypeId == (int) CommunicationType.TCP)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "TCP").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            logger.Log($"This is a TCP Device");
                                            int tcpConfiguration = device.TcpConfigurationId.GetValueOrDefault();
                                            logger.Log(@"TCP Configuration number: " + tcpConfiguration.ToString());
                                            if (tcpConfiguration != 0)
                                            {
                                                try
                                                {
                                                    TcpConfiguration tcp = _repoTcpConfiguration.FindByKey(tcpConfiguration);
                                                    logger.Log($"TCP Name: {tcp.Name}");
                                                    device.TcpConfiguration = tcp;
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Log($"Error finding TCP Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                                                    device.TcpConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                logger.Log("Configuration set to null");
                                                device.TcpConfiguration = null;
                                            }
                                            stationView.HardwareDevices.Add(device);

                                        }
                                        //if (device.CommunicationTypeId == (int)CommunicationType.Serial)
                                        key = _dicCommunicationTypes.FirstOrDefault(d => d.Value == "Serial").Key;
                                        if (device.CommunicationTypeId == key)
                                        {
                                            logger.Log($"This is a Serial Device");
                                            int serialConfiguration = device.SerialConfigurationId.GetValueOrDefault();
                                            logger.Log(@"Serial Configuration number: " + serialConfiguration.ToString());
                                            if (serialConfiguration != 0)
                                            {
                                                try
                                                {
                                                    SerialConfiguration serial = _repoSerialConfiguration.FindByKey(serialConfiguration);
                                                    logger.Log($"Serial Name: {serial.Name}  Port: {serial.PortName}");
                                                    device.SerialConfiguration = serial;
                                                }
                                                catch (Exception ex)
                                                {
                                                    logger.Log($"Error finding Serial Configuration.  {ex.Message}  Inner:  {ex.InnerException}");
                                                    device.SerialConfiguration = null;
                                                }
                                            }
                                            else
                                            {
                                                logger.Log("Configuration set to null");
                                                device.SerialConfiguration = null;
                                            }
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
                                default:
                                    break;
                            }
                        }

                        stationView.StationId = station.Id;
                        stationView.StationNumber = station.StationNumber;
                        stationView.Name = station.Name;
                        stationView.Sequence = station.Sequence;

                        //logger.Log(@"StationView Serial Configuration Name " + stationView.HardwareDevices.First().SerialConfiguration.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Error finding hardware devices.  {ex.Message}  Inner:  {ex.InnerException}");
                    }
                }
                else  //station = null
                {
                    logger.Log($"Station is null");
                }
            }
            catch (Exception ex)
            {
                logger.Log($"Error finding station.  {ex.Message}  Inner:  {ex.InnerException}");
            }
            return stationView;
        }

        public int GetStationId(int stationNumber)
        {
            int stationId = 0;
            Station result = _repoStation.FindBy(r => r.StationNumber == stationNumber).FirstOrDefault();
            if (result != null)
            {
                stationId = result.Id;
            }
            return stationId;
        }

        public List<Station> Lookup()
        {
            List<Station> stations = _repoStation.All().ToList();
            return stations;
        }

        public List<string> GetPickStationIds()
        {
            var result = new List<string>();
            var stations = _repoStation.All().Where(r => r.Id <= 8).Select(s => s.Id.ToString()).OrderBy(s => s).ToList();
            if (stations.Count > 0)
            {
                result = stations;
            }
            return result;
        }
    }
}
