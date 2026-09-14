using Neutron.Global;
using NeutronData.Models;
using NeutronData.ModelViews;
using System.Collections.Generic;
using System.Linq;
using AlliedLogger;
using AsyncAwaitBestPractices;

namespace Neutron.Models
{
    public class ReplenDeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled;
        private readonly IDynamicLogger _logger;
        private readonly int _logLevel;
        private readonly Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();

        public ReplenDeviceManager(List<List<ReplenPickStop>> carList, bool shuttleEnabled, int logLevel = 2)
        {
            _logLevel = logLevel;
            _logger = NeutronCore.Global.Logger.SetupLogger(@"ReplenDeviceManager");
            for (int i = 0; i < carList.Count; i++)
            {
                var mover = CreateDeviceMover(deviceNumber: i + 1, carList: carList[i]);
                // _currentLocations[i] = null;
                _deviceMovers.Add(mover);
            }
            _shuttleEnabled = shuttleEnabled;
        }

        private DeviceMover CreateDeviceMover(int deviceNumber, List<ReplenPickStop> carList)
        {
            if (_logLevel > 0)
                _logger.LogDetailAsync($"CreateDeviceMover device Number {deviceNumber}").SafeFireAndForget();

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
            return new DeviceMover(deviceNumber, locs, _logger);
        }

        public void MoveNext(int deviceNumber)
        {
            _logger.LogDetailAsync($"MoveNext device Number {deviceNumber}").SafeFireAndForget();

            var deviceMover = _deviceMovers.FirstOrDefault(r => r.MoverNumber == deviceNumber);

            if (deviceMover == null)
            {
                _logger.LogDetailAsync($"Device Mover is NULL for device number {deviceNumber}").SafeFireAndForget();
                return;
            }

            var location = deviceMover.MoveNext();

            if (location == null)
            {
                _logger.LogDetailAsync($"Location is NULL").SafeFireAndForget();
                return;
            }

            _currentLocations[deviceNumber] = location;
            if (!_shuttleEnabled) return;

            var loc1 = location.Loc1;
            var loc2 = location.Loc2;
            var loc3 = location.Loc3;
            var loc4 = location.Loc4;

            _logger.LogDetailAsync($"Location {loc1}--{loc2}").SafeFireAndForget();

            if (GlobalVar.Shuttle != null)
            {
                _logger.LogDetailAsync($"GlobalVar.Shuttle.PositionDevice Loc1:{loc1}  Loc2:{loc2}").SafeFireAndForget();
                GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4);
            }
            if (GlobalVar.Hanel != null)
            {
                _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4}").SafeFireAndForget();
                GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
            }
        }

        public void Reset()
        {
            _logger.LogDetailAsync($"Reset:").SafeFireAndForget();
            _logger.LogDetailAsync($"Starting a LOOP over Current Locations").SafeFireAndForget();

            foreach (KeyValuePair<int, Location> kvp in _currentLocations)
            {
                if (kvp.Value == null) continue;
                if (!_shuttleEnabled) continue;

                var loc1 = kvp.Value.Loc1;
                var loc2 = kvp.Value.Loc2;
                var loc3 = kvp.Value.Loc3;
                var loc4 = kvp.Value.Loc4;

                _logger.LogDetailAsync($"Reset: Loc1: {loc1}  Loc2: {loc2}").SafeFireAndForget();

                if (GlobalVar.Shuttle != null)
                {
                    _logger.LogDetailAsync($"GlobalVar.Shuttle.PositionDevice Loc1:{loc1}  Loc2:{loc2}").SafeFireAndForget();
                    GlobalVar.Shuttle.PositionDevice(loc1, loc2, loc3, loc4);
                }
                if (GlobalVar.Hanel != null)
                {
                    _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4}").SafeFireAndForget();
                    GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                }
            }

            _logger.LogDetailAsync($"Reset Complete").SafeFireAndForget();
        }

        public void ResetMoveNext(int moveNext = default(int))
        {
            _logger.LogDetailAsync($"Reset MoveNext: {moveNext}").SafeFireAndForget();
            foreach (var kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    var loc1 = kvp.Value.Loc1;
                    var loc2 = kvp.Value.Loc2;
                    var loc3 = kvp.Value.Loc3;
                    var loc4 = kvp.Value.Loc4;

                    if (loc1 == moveNext)
                    {
                        _logger.LogDetailAsync($"Reset MoveNext Move Later - Loc1: {loc1}  Loc2: {loc2}").SafeFireAndForget();
                        continue;
                    }

                    _logger.LogDetailAsync($"Reset: Loc1: {loc1}  Loc2: {loc2}").SafeFireAndForget();
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
                _logger.LogDetailAsync($"Reset MoveNext Device: {moveNext}").SafeFireAndForget();
                MoveNext(moveNext);
            }
        }
    }
}
