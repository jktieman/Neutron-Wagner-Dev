using EnumsNET;
using Neutron.Enums;
using Neutron.Global;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using Hart_DeviceControllers;

namespace Neutron.Models
{
    public class DeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled = true;
        // private Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();
        private DynamicLogger _logger;
        private Location[] _currentLocations = new Location[5];
        private bool _firstMove;


        public DeviceManager(List<PickStop> car1List, List<PickStop> car2List
            , List<PickStop> car3List, List<PickStop> car4List, bool shuttleEnabled, DynamicLogger logger)
        {

            _logger = logger;
            _firstMove = true;

            //_currentLocations[1] = null;
            //_currentLocations[2] = null;
            //_currentLocations[3] = null;
            //_currentLocations[4] = null;

            var mover = CreateDeviceMover(deviceNumber: 1, carList: car1List);
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
                _logger.Log($"Current Position: {deviceMover.Position}");
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
            var firstLocation = true;
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

        public async Task FirstMoveAsync()
        {
            _firstMove = true;
            await MoveNext(1);
            await MoveNext(2);
            await MoveNext(3);
            await MoveNext(4);
            _firstMove = false;
        }

        public async Task MoveNext(int deviceNumber)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine($"");
                sb.AppendLine($"Device Number passed in: {deviceNumber}");
                sb.AppendLine($"Start loop thru all devices.");
                for (var index = 0; index < _deviceMovers.Count; index++)
                {
                    var deviceMover = _deviceMovers[index];

                    sb.AppendLine($"Device Mover: {deviceMover.MoverNumber}");
                    if (deviceMover.MoverNumber == deviceNumber)
                    {
                        sb.AppendLine($"This is the targeted Device Mover, move to next Location");
                        var location = deviceMover.MoveNext();

                        if (location != null)
                        {
                            sb.AppendLine(
                                $"Next Location: {location.Loc1}-{location.Loc2}-{location.Loc3}-{location.Loc4} ");

                            _currentLocations[deviceNumber] = location;

                            if (_shuttleEnabled)
                            {
                                if (GlobalVar.Shuttle != null)
                                {
                                    var loc1 = location.Loc1;
                                    var loc2 = location.Loc2;

                                    sb.AppendLine($"Getting Status of {deviceNumber}");
                                    var status = await Task.Run(() => GlobalVar.Shuttle.GetDeviceStatus(deviceNumber));

                                    if (status.In_Motion)
                                    {
                                        sb.AppendLine($"Device {deviceMover.MoverNumber} is in Motion.");
                                        var sb1 = sb;
                                        await Task.Run(() => _logger.Log($"{sb1}"));

                                        var num = deviceMover.MoverNumber;
                                        await Task.Run(() => ProcessInMotion(num, loc2));
                                    }
                                    else
                                    {
                                        sb.AppendLine($"Device: {status.Device}");
                                        sb.AppendLine($"Target Tray: {status.Target_Tray}");
                                        sb.AppendLine($"Current Tray: {status.Current_Tray}");
                                        sb.AppendLine($"In Motion: {status.In_Motion}");
                                        sb.AppendLine($"In Alignment: {status.In_Alignment}");
                                        sb.AppendLine($"Last Command: {status.Last_Command:G}");
                                        sb.AppendLine($"Last Status: {status.Last_Status:G}");
                                        sb.AppendLine($"Message: {status.Status_Message}");
                                        sb.AppendLine();
                                        sb.AppendLine($"Device {deviceMover.MoverNumber} is in Stopped.");
                                        var sb1 = sb;
                                        Task.Run(() => _logger.Log($"{sb1}"));
                                        await Task.Run(() => VerifyMoveLocation(deviceMover.MoverNumber, status.Current_Tray, loc2));
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine("Shuttle Not Enabled.");
                            }
                        }
                        else
                        {
                            sb.AppendLine("Location is null.");
                        }

                        var sb2 = sb;
                        // Task.Run(() => _logger.Log($"{sb2}"));
                    }
                    else
                    {
                        if (!_firstMove)
                        {
                            sb = new StringBuilder();

                            //resend to the others, their current location
                            if (!_shuttleEnabled) continue;
                            if (GlobalVar.Shuttle == null) continue;
                            sb.AppendLine($"Checking/Verifying Device Mover Status and Location of Mover Number: {deviceMover.MoverNumber}");
                            var location = _currentLocations[deviceMover.MoverNumber];
                            if (location == null) continue;
                            var loc1 = location.Loc1;
                            var loc2 = location.Loc2;
                            sb.AppendLine($"Mover Number: {deviceMover.MoverNumber} is supposed to be at Location:  {loc1} - {loc2} ");

                            var status = await Task.Run(() =>
                                GlobalVar.Shuttle.GetDeviceStatus(deviceMover.MoverNumber));

                            if (status.In_Motion)
                            {
                                sb.AppendLine($"Device {deviceMover.MoverNumber} is in Motion.");
                                var sb1 = sb;
                                _logger.Log($"{sb1.ToString()}");
                                await Task.Run(() => ProcessInMotion(deviceMover.MoverNumber, loc2));
                            }
                            else
                            {
                                sb.AppendLine($"Device: {status.Device}");
                                sb.AppendLine($"Target Tray: {status.Target_Tray}");
                                sb.AppendLine($"Current Tray: {status.Current_Tray}");
                                sb.AppendLine($"In Motion: {status.In_Motion}");
                                sb.AppendLine($"In Alignment: {status.In_Alignment}");
                                sb.AppendLine($"Last Command: {status.Last_Command:G}");
                                sb.AppendLine($"Last Status: {status.Last_Status:G}");
                                sb.AppendLine($"Message: {status.Status_Message}");
                                sb.AppendLine();
                                sb.AppendLine($"Device {deviceMover.MoverNumber} is in Stopped.");
                                var sb1 = sb;
                                _logger.Log($"{sb1}");
                                await Task.Run(() => VerifyMoveLocation(deviceMover.MoverNumber, status.Current_Tray, loc2));
                            }
                            var sb2 = sb;
                            _logger.Log($"{sb2}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Task.Run(() => _logger.Log($"Device Manager Move Next Error: {ex.Message} {Environment.NewLine} {ex.InnerException}"));
            }

            //Task.Run(() => _logger.Log($"{sb.ToString()}"));
        }

        private void VerifyMoveLocation(int deviceMoverMoverNumber, int currentTray, int loc2)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"VML-Check to see if Status.Current_Tray: {currentTray} is equal to the _currentLocations[x] Tray Number: {loc2} .");
            if (currentTray != loc2)
            {
                sb.AppendLine($"VML-Trays are not the same.  Current: {currentTray}  New Location: {loc2}");
                sb.AppendLine($"VML-Move it... {deviceMoverMoverNumber}--{loc2}");

                var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(deviceMoverMoverNumber, loc2));

                sb.AppendLine($"VML-DeviceManager MoveNext Response: {response.Result.AsString(EnumFormat.Description)} to Position Tray");
                switch (response.Result)
                {
                    case DeviceResponse.Success:
                        sb.AppendLine($"VML-Success");
                        break;
                    case DeviceResponse.TrayDidNotArrive:
                        sb.AppendLine($"VML-Tray did not Arrive.");
                        break;
                    default:
                        sb.AppendLine(
                            $"VML-Response Result: {response.Result.AsString(EnumFormat.Description)}");
                        MessageBox.Show(response.Result.AsString(EnumFormat.Description),
                            caption: "Device Response Move Next"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        break;
                }
            }
            else
            {
                sb.AppendLine($"VML-Verify Move Location: Carousel in Position. {deviceMoverMoverNumber}--{currentTray}--{loc2}");
            }
            _logger.Log($"{sb}");
        }

        private void ProcessInMotion(int deviceMoverMoverNumber, int loc2)
        {
            var sb = new StringBuilder();
            var counter = 0;
            var inMotion = true;
            var inAlignment = false;
            var status = new Hart_DeviceStatusType();
            sb.AppendLine($"PIM-Process In Motion.  Device: {deviceMoverMoverNumber}--{loc2}");
            while (inMotion == true && inAlignment == false)
            {
                counter++;
                if (counter >= 60)
                {
                    sb.AppendLine($"PIM-Device never stopped. Tried for {counter} seconds.");
                    goto EXITNOW;
                }
                sb.AppendLine($"PIM-Device: {deviceMoverMoverNumber} is in Motion for {counter} seconds.");
                Task.Delay(1000);
                status = GlobalVar.Shuttle.GetDeviceStatus(deviceMoverMoverNumber);
                inMotion = status.In_Motion;
                inAlignment = status.In_Alignment;
            }

            sb.AppendLine($"PIM-Device: {status.Device}");
            sb.AppendLine($"PIM-Target Tray: {status.Target_Tray}");
            sb.AppendLine($"PIM-Current Tray: {status.Current_Tray}");
            sb.AppendLine($"PIM-In Motion: {status.In_Motion}");
            sb.AppendLine($"PIM-In Alignment: {status.In_Alignment}");
            sb.AppendLine($"PIM-Last Command: {status.Last_Command:G}");
            sb.AppendLine($"PIM-Last Status: {status.Last_Status:G}");
            sb.AppendLine($"PIM-Message: {status.Status_Message}");

            sb.AppendLine($"PIM-Check to see if Status.Current_Tray: {status.Current_Tray} is equal to the _currentLocations Tray Number: {loc2} .");
            if (status.Current_Tray != loc2)
            {
                sb.AppendLine($"PIM-Trays are not the same.  Current: {status.Current_Tray}  New Location: {loc2}");
                sb.AppendLine($"PIM-Move it... {deviceMoverMoverNumber}--{loc2}");
                var response = GlobalVar.Shuttle.PositionDevice(deviceMoverMoverNumber, loc2);
                sb.AppendLine($"PIM-DeviceManager MoveNext Response: {response.AsString(EnumFormat.Description)} to Position Tray");

                switch (response)
                {
                    case DeviceResponse.Success:
                        sb.AppendLine($"PIM-Success");
                        break;
                    case DeviceResponse.TrayDidNotArrive:
                        sb.AppendLine($"PIM-Tray did not Arrive.");
                        break;
                    default:
                        sb.AppendLine($"PIM-Response Result: {response.AsString(EnumFormat.Description)}");
                        MessageBox.Show(response.AsString(EnumFormat.Description),
                            caption: "Device Response Move Next"
                            , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                        break;
                }
            }
            else
            {
                sb.AppendLine($"PIM-Process In Motion - Carousel in Position. {deviceMoverMoverNumber}--{loc2}");
            }
            EXITNOW:
            _logger.Log($"{sb}");
        }

        public void Reset()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Reset");
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
                            var loc2 = location.Loc2;

                            sb.AppendLine($"RESET FUNCTION Position Device Back To: {loc1}-{loc2}");

                            var status = GlobalVar.Shuttle.GetDeviceStatus(loc1);

                            if (status.In_Motion)
                            {
                                sb.AppendLine($"Device {loc1} is in Motion.");
                                var sb1 = sb;
                                sb.AppendLine($"{sb1.ToString()}");
                                ProcessInMotion(loc1, loc2);
                            }
                            else
                            {
                                sb.AppendLine($"Device: {status.Device}");
                                sb.AppendLine($"Target Tray: {status.Target_Tray}");
                                sb.AppendLine($"Current Tray: {status.Current_Tray}");
                                sb.AppendLine($"In Motion: {status.In_Motion}");
                                sb.AppendLine($"In Alignment: {status.In_Alignment}");
                                sb.AppendLine($"Last Command: {status.Last_Command:G}");
                                sb.AppendLine($"Last Status: {status.Last_Status:G}");
                                sb.AppendLine($"Message: {status.Status_Message}");
                                sb.AppendLine();
                                sb.AppendLine($"Device {loc1} is in Stopped.");
                                var sb1 = sb;
                                sb.AppendLine($"{sb1}");
                                VerifyMoveLocation(loc1, status.Current_Tray, loc2);
                            }
                        }
                    }
                }
            }
            _logger.Log($"{sb}");
        }
    }
}
