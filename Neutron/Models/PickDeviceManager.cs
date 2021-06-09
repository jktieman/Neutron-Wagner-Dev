using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlliedLogger;
using EnumsNET;
using Neutron.Enums;
using Neutron.Global;
using NeutronCore;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace Neutron.Models
{
    public class PickDeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled;
        private DynamicLogger _logger;
        private readonly Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();

        public PickDeviceManager(IReadOnlyList<List<PickStop>> carList, bool shuttleEnabled)
        {
            SetupLogger();
            for (var i = 0; i < carList.Count; i++)
            {
                var mover = CreateDeviceMover(i +1, carList[i]);
                _currentLocations[i] = null;
                _deviceMovers.Add(mover);
            }
            _shuttleEnabled = shuttleEnabled;

        }

        private void SetupLogger()
        {
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = @"PickDeviceManager";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        }

        private DeviceMover CreateDeviceMover(int deviceNumber, IEnumerable<PickStop> carList)
        {
            _logger.Log($"CreateDeviceMover device Number {deviceNumber}");
            var firstLocation = true;
            var locs = new List<Location>();
            foreach (var pickStop in carList)
            {
                locs.Add(pickStop.CurrentInventoryLocation.Location);
                if (!firstLocation) continue;
                _currentLocations[deviceNumber] = pickStop.CurrentInventoryLocation.Location;
                firstLocation = false;
            }
            return new DeviceMover(deviceNumber, locs);
        }

        public void MoveNext(int deviceNumber)
        {
            _logger.Log($"MoveNext device Number {deviceNumber}");
            var deviceMover = _deviceMovers.FirstOrDefault(r => r.MoverNumber == deviceNumber);
            if (deviceMover != null)
            {
                var location = deviceMover.MoveNext();
                _logger.Log($"Location {location?.Loc1}--{location?.Loc2}");
                _currentLocations[deviceNumber] = location;
                if (location != null)
                {
                    var loc1 = location.Loc1;
                    var loc2 = location.Loc2;
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            _logger.Log($"GlobalVar.Shuttle.PositionDevice Loc1:{loc1}  Loc2:{loc2}");
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        }
                    }
                }
            }

            //if (!_shuttleEnabled) return;
            //if (GlobalVar.Shuttle == null) return;



            //var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
            //if (response.Result != DeviceResponse.Success)
            //{
            //    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Response Move Next"
            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            //}
        }

        public void Reset()
        {
            _logger.Log($"Reset:");
            foreach (var kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            var loc1 = kvp.Value.Loc1;
                            var loc2 = kvp.Value.Loc2;
                            _logger.Log($"Reset: Loc1: {loc1}  Loc2: {loc2}");
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                            //Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                            //if (response.Result != DeviceResponse.Success)
                            //{
                            //    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: @"Device Response Reset"
                            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            //}
                        }
                    }
                }
            }
        }
    }
}