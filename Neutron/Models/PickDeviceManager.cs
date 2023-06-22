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
using static DGVPrinterHelper.DGVPrinter;

namespace Neutron.Models
{
    public class PickDeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled;
        private DynamicLogger _logger;
        private readonly Dictionary<int, NeutronData.Models.Location> _currentLocations = new Dictionary<int, NeutronData.Models.Location>();

        public PickDeviceManager(IReadOnlyList<List<PickStop>> carList, bool shuttleEnabled)
        {
            SetupLogger();
            for (var i = 0; i < carList.Count; i++)
            {
                var mover = CreateDeviceMover(i + 1, carList[i]);
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
            _logger.LogDetailAsync($"CreateDeviceMover device Number {deviceNumber}");
            var firstLocation = true;
            var locs = new List<NeutronData.Models.Location>();
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
            _logger.LogDetailAsync($"MoveNext device Number {deviceNumber}");
            var deviceMover = _deviceMovers.FirstOrDefault(r => r.MoverNumber == deviceNumber);
            if (deviceMover != null)
            {
                var location = deviceMover.MoveNext();
                _logger.LogDetailAsync($"Location {location?.Loc1}--{location?.Loc2}");
                _currentLocations[deviceNumber] = location;
                if (location != null)
                {
                    var loc1 = location.Loc1;
                    var loc2 = location.Loc2;
                    var loc3 = location.Loc3;
                    var loc4 = location.Loc4;
                    var loc5 = location.Loc5;
                    
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            _logger.LogDetailAsync($"GlobalVar.Shuttle.PositionDevice Loc1:{loc1}  Loc2:{loc2}");
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        }
                        if (GlobalVar.Hanel != null)
                        {
                            _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4} Loc5:{loc5}");
                            GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4 );
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
            _logger.LogDetailAsync($"Reset:");
            foreach (var kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    var loc1 = kvp.Value.Loc1;
                    var loc2 = kvp.Value.Loc2;
                    var loc3 = kvp.Value.Loc3;
                    var loc4 = kvp.Value.Loc4;
                    var loc5 = kvp.Value.Loc5;

                    _logger.LogDetailAsync($"Reset: Loc1: {loc1}  Loc2: {loc2}");
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                            //Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                            //if (response.Result != DeviceResponse.Success)
                            //{
                            //    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: @"Device Response Reset"
                            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            //}
                        }
                        if (GlobalVar.Hanel != null)
                        {
                            _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4} Loc5:{loc5}");
                            GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                        }
                    }
                }
            }
        }

        public void ResetMoveNext(int moveNext = default(int))
        {
            _logger.LogDetailAsync($"Reset MoveNext: {moveNext}");
            foreach (var kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    var loc1 = kvp.Value.Loc1;
                    var loc2 = kvp.Value.Loc2;
                    var loc3 = kvp.Value.Loc3;
                    var loc4 = kvp.Value.Loc4;
                    var loc5 = kvp.Value.Loc5;

                    if (loc1 == moveNext)
                    {
                        _logger.LogDetailAsync($"Reset MoveNext Move Later - Loc1: {loc1}  Loc2: {loc2}");
                        continue;
                    }

                    _logger.LogDetailAsync($"Reset: Loc1: {loc1}  Loc2: {loc2}");
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                        }
                        if (GlobalVar.Hanel != null)
                        {
                            GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                        }
                    }
                }
            }

            if (moveNext != default(int))
            {
                _logger.LogDetailAsync($"Reset MoveNext Device: {moveNext}");
                MoveNext(moveNext);
            }
        }
    }
}