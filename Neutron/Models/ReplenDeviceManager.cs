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

namespace Neutron.Models
{
    public class ReplenDeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled = true;
        private Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();

        public ReplenDeviceManager() { }

        public ReplenDeviceManager(List<ReplenPickStop> car1List, List<ReplenPickStop> car2List
            , List<ReplenPickStop> car3List, List<ReplenPickStop> car4List, bool shuttleEnabled)
        {
            DeviceMover mover = CreateDeviceMover(deviceNumber: 1, carList: car1List);
            _deviceMovers.Add(mover);

            mover = CreateDeviceMover(deviceNumber: 2, carList: car2List);
            _deviceMovers.Add(mover);

            mover = CreateDeviceMover(deviceNumber: 3, carList: car3List);
            _deviceMovers.Add(mover);

            mover = CreateDeviceMover(deviceNumber: 4, carList: car4List);
            _deviceMovers.Add(mover);

            _shuttleEnabled = shuttleEnabled;

            _currentLocations[1] = null;
            _currentLocations[2] = null;
            _currentLocations[3] = null;
            _currentLocations[4] = null;
        }

        private DeviceMover CreateDeviceMover(int deviceNumber, List<ReplenPickStop> carList)
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

        public void MoveNext(int deviceNumber)
        {
            foreach (var deviceMover in _deviceMovers)
            {
                if (deviceMover.MoverNumber == deviceNumber)
                {
                    Location location = deviceMover.MoveNext();
                    if (location != null)
                    {
                        _currentLocations[deviceNumber] = location;
                        if (_shuttleEnabled)
                        {
                            if (GlobalVar.Shuttle != null)
                            {
                                int loc1 = location.Loc1;
                                int loc2 = location.Loc2;
                                Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                                if (response.Result != DeviceResponse.Success)
                                {
                                    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Response Move Next"
                                        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        public void Reset()
        {
            foreach (KeyValuePair<int, Location> kvp in _currentLocations)
            {
                if (kvp.Value != null)
                {
                    if (_shuttleEnabled)
                    {
                        if (GlobalVar.Shuttle != null)
                        {
                            int loc1 = kvp.Value.Loc1;
                            int loc2 = kvp.Value.Loc2;
                            Task<DeviceResponse> response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                            if (response.Result != DeviceResponse.Success)
                            {
                                MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: @"Device Response Reset"
                                    , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}
