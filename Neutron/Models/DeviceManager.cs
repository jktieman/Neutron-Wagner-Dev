using EnumsNET;
using Neutron.Enums;
using Neutron.Global;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;

namespace Neutron.Models
{
    public class DeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled = true;
        // private Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();
        private DynamicLogger _logger;
        private Location[] _currentLocations = new Location[5];
        private bool _firstMove = true;


        public DeviceManager(List<PickStop> car1List, List<PickStop> car2List
            , List<PickStop> car3List, List<PickStop> car4List, bool shuttleEnabled, DynamicLogger logger)
        {

            _logger = logger;
            _firstMove = true;

            //_currentLocations[1] = null;
            //_currentLocations[2] = null;
            //_currentLocations[3] = null;
            //_currentLocations[4] = null;

            DeviceMover mover = CreateDeviceMover(deviceNumber: 1, carList: car1List);
            _deviceMovers.Add(mover);


            mover = CreateDeviceMover(deviceNumber: 2, carList: car2List);
            _deviceMovers.Add(mover);

            mover = CreateDeviceMover(deviceNumber: 3, carList: car3List);
            _deviceMovers.Add(mover);

            mover = CreateDeviceMover(deviceNumber: 4, carList: car4List);
            _deviceMovers.Add(mover);

            PrintCarLists();

            _shuttleEnabled = shuttleEnabled;

        }

        private void PrintCurrentLocations()
        {
            _logger.Log($"");
            var location = _currentLocations[1];
            _logger.Log(location != null
                ? $"Current Location 1: {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4}"
                : $"Current Location 1:  NULL");
            _logger.Log($"");

            location = _currentLocations[2];
            _logger.Log(location != null
                ? $"Current Location 2: {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4}"
                : $"Current Location 2:  NULL");
            _logger.Log($"");

            location = _currentLocations[3];
            _logger.Log(location != null
                ? $"Current Location 3 {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4}"
                : $"Current Location 3:  NULL");
            _logger.Log($"");

            location = _currentLocations[4];
            _logger.Log(location != null
                ? $"Current Location 4 {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4}"
                : $"Current Location 4:  NULL");
            _logger.Log($"");
        }

        private void PrintCarLists()
        {
            foreach (var deviceMover in _deviceMovers)
            {
                _logger.Log($"");
                _logger.Log($"Device Mover Number: {deviceMover.MoverNumber}");
                _logger.Log($"Next Location Index: {deviceMover.NextIndex}");
                foreach (var location in deviceMover.Locations)
                {
                    _logger.Log(location != null
                        ? $"Current Location {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4}"
                        : $"Current Location :  NULL");
                }
            }
        }

        private DeviceMover CreateDeviceMover(int deviceNumber, List<PickStop> carList)
        {
            bool firstLocation = true;
            var locs = new List<Location>();
            foreach (var pickStop in carList)
            {
                locs.Add(pickStop.CurrentInventoryLocation.Location);
                if (firstLocation)
                {
                    _currentLocations[deviceNumber] = pickStop.CurrentInventoryLocation.Location;
                    firstLocation = false;
                }
            }
            return new DeviceMover(deviceNumber, locs);
        }

        public async Task MoveNext(int deviceNumber)
        {
            try
            {
                Task.Run(() => _logger.Log($"DeviceManager 1 - Move Next Device Number: {deviceNumber}"));
                for (var index = 0; index < _deviceMovers.Count; index++)
                {
                    Task.Run(() => _logger.Log($"DeviceManager - 2 - Index: {index} DeviceMover Count: {_deviceMovers.Count}"));
                    var deviceMover = _deviceMovers[index];

                    Task.Run(() => _logger.Log($"DeviceManager - 3 Check MoverNumber: {deviceMover.MoverNumber} with deviceNumber {deviceNumber}"));
                    if (deviceMover.MoverNumber == deviceNumber)
                    {
                        Task.Run(() => _logger.Log($"DeviceManager - 4  - Equal Numbers"));

                        Location location = deviceMover.MoveNext();

                        if (location != null)
                        {
                            _logger.Log($"DeviceManager - 5  MoveNext Location: {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4} ");

                            _currentLocations[deviceNumber] = location;
                            Task.Run(() => _logger.Log($"DeviceManager - 7 Moving to next Location"));
                            if (_shuttleEnabled)
                            {
                                if (GlobalVar.Shuttle != null)
                                {
                                    int loc1 = location.Loc1;
                                    int loc2 = location.Loc2;

                                    Task.Run(() => _logger.Log($"DeviceManager MoveNext Location: {loc1} - {loc2}  Getting Status of {deviceNumber}"));
                                    
                                    var status = await Task.Run(() => GlobalVar.Shuttle.GetDeviceStatus(deviceNumber));


                                    var msg = "Device: \t" + status.Device.ToString() + "\n" +
                                          "Target Tray: \t" + status.Target_Tray.ToString() + "\n" +
                                          "Current Tray: \t" + status.Current_Tray.ToString() + "\n" +
                                          "In Motion: \t" + status.In_Motion.ToString() + "\n" +
                                          "In Alignment: \t" + status.In_Alignment.ToString() + "\n" +
                                          "Last Command: \t" + status.Last_Command.ToString("G") + "\n" +
                                          "Last Status: \t" + status.Last_Status.ToString("G") + "\n" +
                                          "Message: \t" + status.Status_Message.ToString();

                                    Task.Run(() => _logger.Log($"Check In-Motion and In_Alignment {Environment.NewLine}{msg}"));


                                    if (status.Current_Tray != loc2)
                                    {
                                        Task.Run(() => _logger.Log($"Current: {status.Current_Tray}  New Location: {loc2}  Position Tray"));

                                        var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));

                                        Task.Run(() => _logger.Log($"DeviceManager MoveNext Response: {response.Result.AsString(EnumFormat.Description)} to Position Tray"));

                                        if (response.Result != DeviceResponse.Success)
                                        {
                                            if (response.Result == DeviceResponse.TrayDidNotArrive)
                                            {
                                                Task.Run(() => _logger.Log($"Tray did not arrive response."));
                                            }
                                            else
                                            {
                                                MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                                    caption: "Device Response Move Next"
                                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        Task.Run(() => _logger.Log($"DeviceManager MoveNext Location - Carousel in Position, no need to turn."));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!_firstMove)
                        {
                            //resend to the others, their current location
                            if (_shuttleEnabled)
                            {
                                if (GlobalVar.Shuttle != null)
                                {

                                    Task.Run(() => _logger.Log($"DeviceManager - 8 Not the MoveNext DeviceMover, just Check and Reset DeviceMover Number: {deviceMover.MoverNumber}"));
                                    var location = _currentLocations[deviceMover.MoverNumber];
                                    if (location != null)
                                    {
                                        Task.Run(() => _logger.Log($"DeviceManager - 10: Have location, do we need to turn?"));
                                        int loc1 = location.Loc1;
                                        int loc2 = location.Loc2;

                                        Task.Run(() => _logger.Log($"DeviceManager Current Location: {loc1} - {loc2}  Getting Status"));

                                        var status = await Task.Run(() => GlobalVar.Shuttle.GetDeviceStatus(deviceMover.MoverNumber));


                                        var msg = "Device: \t" + status.Device.ToString() + "\n" +  
                                                  "Target Tray: \t" + status.Target_Tray.ToString() + "\n" +
                                                  "Current Tray: \t" + status.Current_Tray.ToString() + "\n" +
                                                  "In Motion: \t" + status.In_Motion.ToString() + "\n" +
                                                  "In Alignment: \t" + status.In_Alignment.ToString() + "\n" +
                                                  "Last Command: \t" + status.Last_Command.ToString("G") + "\n" +
                                                  "Last Status: \t" + status.Last_Status.ToString("G") + "\n" +
                                                  "Message: \t" + status.Status_Message.ToString();

                                        Task.Run(() => _logger.Log($"{msg}"));
                          
                                        if (status.Current_Tray != loc2 && status.In_Motion == false)
                                        {
                                            Task.Run(() => _logger.Log($"Says that the Current Tray {status.Current_Tray} is NOT equal to {loc2} AND the carousel is NOT in motion and needs to move."));

                                            var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));

                                            Task.Run(() => _logger.Log($"DeviceManager Reset Response: {response.Result.AsString(EnumFormat.Description)}"));

                                            if (response.Result != DeviceResponse.Success)
                                            {
                                                if (response.Result == DeviceResponse.TrayDidNotArrive)
                                                {
                                                    Task.Run(() => _logger.Log($"Reset - Tray did not arrive response."));
                                                }
                                                else
                                                {
                                                    MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                                        caption: "Device Response Reset"
                                                        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            Task.Run(() => _logger.Log($"Carousel in Position, no need to turn."));
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _firstMove = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Device Manager Move Next Error: {ex.Message} {Environment.NewLine} {ex.InnerException}"));
            }

        }

        //public void MoveNext(int deviceNumber)
        //{
        //    foreach (var deviceMover in _deviceMovers)
        //    {
        //        if (deviceMover.MoverNumber == deviceNumber)
        //        {
        //            Location location = deviceMover.MoveNext();
        //            if (location != null)
        //            {
        //                _currentLocations[deviceNumber] = location;
        //                if (_shuttleEnabled)
        //                {
        //                    if (GlobalVar.Shuttle != null)
        //                    {
        //                        int loc1 = location.Loc1;
        //                        int loc2 = location.Loc2;

        //                        Task.Run(() => _logger.Log($"DeviceManager MoveNext: {loc1} - {loc2}"));

        //                        Task <DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));

        //                        Task.Run(() => _logger.Log($"DeviceManager MoveNext Response: {response.Result}"));

        //                        if (response.Result != DeviceResponse.Success)
        //                        {
        //                            MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Response Move Next"
        //                                , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        else
        //        {
        //            //resend to the others, their current location
        //            if (_shuttleEnabled)
        //            {
        //                if (GlobalVar.Shuttle != null)
        //                {
        //                    var location = _currentLocations[deviceNumber];
        //                    int loc1 = location.Loc1;
        //                    int loc2 = location.Loc2;

        //                    Task.Run(() => _logger.Log($"DeviceManager Current Location: {loc1} - {loc2}"));

        //                    Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));

        //                    Task.Run(() => _logger.Log($"DeviceManager Current Location Response: {response.Result}"));

        //                    if (response.Result != DeviceResponse.Success)
        //                    {
        //                        MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Response Current Location"
        //                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
        //                    }
        //                }
        //            }

        //        }

        //    }
        //}

        public void Reset()
        {
            _logger.Log($"");
            for (var index = 1; index <= 4; index++)
            {
                var location = _currentLocations[index];
                if (location != null)
                {
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            var loc1 = location.Loc1;
                            int loc2 = location.Loc2;

                            Task.Run(() => _logger.Log($"RESET FUNCTION Position Device: {loc1}-{loc2}"));

                            var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));

                            Task.Run(() => _logger.Log($"RESET FUNCTION: Response = {response.Result.AsString(EnumFormat.Description)}"));

                            if (response.Result != DeviceResponse.Success)
                            {
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                                    caption: @"Device Response Reset"
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}
