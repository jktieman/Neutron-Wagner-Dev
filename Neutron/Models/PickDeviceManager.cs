using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlliedLogger;
using AsyncAwaitBestPractices;
using Neutron.Controllers;
using Neutron.Global;
using Neutron.Interfaces;
using NeutronCore.Global;
using NeutronData.ModelViews;

namespace Neutron.Models
{
    public class PickDeviceManager
    {
        private readonly int _logLevel;
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled;
        private readonly IDynamicLogger _logger;
        private readonly Dictionary<int, NeutronData.Models.Location> _currentLocations = new Dictionary<int, NeutronData.Models.Location>();

        public PickDeviceManager(IReadOnlyList<List<PickStop>> carList, bool shuttleEnabled, int logLevel)
        {
            _logLevel = logLevel;
            _logger = NeutronCore.Global.Logger.SetupLogger(@"HanelLog");
            try
            {
                for (var i = 0; i < carList.Count; i++)
                {
                    var mover = CreateDeviceMover(i + 1, carList[i]);
                    //_currentLocations[i + 1] = null;
                    //_currentLocations.Add(mover.MoverNumber, mover.);
                    //_currentLocations[mover.MoverNumber] = null;
                    _deviceMovers.Add(mover);
                }
                _shuttleEnabled = shuttleEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Pick Device Manager Exception: {ex.Message}");
            }


        }

        private DeviceMover CreateDeviceMover(int deviceNumber, IEnumerable<PickStop> carList)
        {
            _logger.LogDetailAsync($"CreateDeviceMover device Number {deviceNumber}").SafeFireAndForget();
            var firstLocation = true;
            var locs = new List<NeutronData.Models.Location>();
            foreach (var pickStop in carList)
            {
                locs.Add(pickStop.CurrentInventoryLocation.Location);
                if (!firstLocation) continue;
                _currentLocations[deviceNumber] = pickStop.CurrentInventoryLocation.Location;
                firstLocation = false;
            }
            return new DeviceMover(deviceNumber, locs, _logger);
        }

        public void MoveNext(int deviceNumber)
        {
            _logger.LogDetailAsync($"MoveNext device Number {deviceNumber}").SafeFireAndForget();

            var deviceMover = _deviceMovers.FirstOrDefault(r => r.MoverNumber == deviceNumber);

            if (deviceMover != null)
            {
                var location = deviceMover.MoveNext();
                if (location != null)
                {
                    _logger.LogDetailAsync($"Location {location?.Loc1}--{location?.Loc2}").SafeFireAndForget();
                    _currentLocations[deviceNumber] = location;
                   // if (location != null)
                   // {
                        var loc1 = location.Loc1;
                        var loc2 = location.Loc2;
                        var loc3 = location.Loc3;
                        var loc4 = location.Loc4;
                        var loc5 = location.Loc5;

                        if (_shuttleEnabled)
                        {
                            if (GlobalVar.Shuttle != null)
                            {
                                _logger.LogDetailAsync($"GlobalVar.Shuttle.PositionDevice Loc1:{loc1}  Loc2:{loc2}").SafeFireAndForget();
                                GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                            }
                            if (GlobalVar.Hanel != null)
                            {
                                _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4} Loc5:{loc5}").SafeFireAndForget();
                                GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                            }
                        }
                    //}
                }
                else
                {
                    _logger.LogDetailAsync($"Location is NULL").SafeFireAndForget();
                }
            }
            else
            {
                _logger.LogDetailAsync($"Device Mover is NULL").SafeFireAndForget();
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

        public void Reset(Form frm, WorkstationView workstationView)
        {
            _logger.LogDetailAsync($"Reset:").SafeFireAndForget();

            // Close Hanel controller
            //GlobalVar.Hanel.CloseController();
           // _logger.LogDetailAsync($"Back from CloseController:").SafeFireAndForget();
            // Create New Hanel controller
           // GlobalVar.Hanel = new Mp12D(frm, workstationView, _logger, _logLevel);
          //  _logger.LogDetailAsync($"Back from Creating a new Mp12D").SafeFireAndForget();

            _logger.LogDetailAsync($"Starting a LOOP over Current Locations").SafeFireAndForget();
            foreach (var kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    var loc1 = kvp.Value.Loc1;
                    var loc2 = kvp.Value.Loc2;
                    var loc3 = kvp.Value.Loc3;
                    var loc4 = kvp.Value.Loc4;
                    var loc5 = kvp.Value.Loc5;

                    _logger.LogDetailAsync($"Reset: Loc1: {loc1}  Loc2: {loc2}").SafeFireAndForget();
                    if (_shuttleEnabled)
                    {
                        _logger.LogDetailAsync($"Shuttles Enabled").SafeFireAndForget();
                        if (GlobalVar.Shuttle != null)
                        {
                           
                            GlobalVar.Shuttle.PositionDevice(loc1, loc2);
                            _logger.LogDetailAsync($"Shuttles Enabled").SafeFireAndForget();
                            //Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                            //if (response.Result != DeviceResponse.Success)
                            //{
                            //    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: @"Device Response Reset"
                            //        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            //}
                        }
                        if (GlobalVar.Hanel != null)
                        { 
                            _logger.LogDetailAsync($"GlobalVar.Hanel.PositionDevice Loc1:{loc1}  Loc2:{loc2} Loc3:{loc3} Loc4:{loc4} Loc5:{loc5}").SafeFireAndForget();

                            GlobalVar.Hanel.PositionDevice(loc1, loc2, loc3, loc4);
                        }
                    }
                }
            }
            _logger.LogDetailAsync($"Reset Complete").SafeFireAndForget();
        }

        public void ResetMoveNext(int moveNext = default)
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