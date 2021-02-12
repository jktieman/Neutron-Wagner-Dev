using EnumsNET;
using Neutron.Enums;
using Neutron.Global;
using NeutronData.Models;
using NeutronData.ModelViews;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neutron.Models
{
    public class ReplenDeviceManager
    {
        private readonly List<DeviceMover> _deviceMovers = new List<DeviceMover>();
        private readonly bool _shuttleEnabled = true;
        private Dictionary<int, Location> _currentLocations = new Dictionary<int, Location>();

        public ReplenDeviceManager(List<List<ReplenPickStop>> carList, bool shuttleEnabled)
        {
            for (int i = 0; i < carList.Count; i++)
            {
                var mover = CreateDeviceMover(deviceNumber: i + 1, carList: carList[i]);
                _currentLocations[i] = null;
                _deviceMovers.Add(mover);
            }
            _shuttleEnabled = shuttleEnabled;
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
            var deviceMover = _deviceMovers.FirstOrDefault(r => r.MoverNumber == deviceNumber);
            var location = deviceMover?.MoveNext();
            if (location == null) return;
            _currentLocations[deviceNumber] = location;
            if (!_shuttleEnabled) return;
            if (GlobalVar.Shuttle != null)
            {
                var loc1 = location.Loc1;
                var loc2 = location.Loc2;
                var response = Task.Run(() => GlobalVar.Shuttle.PositionDevice(loc1, loc2));
                if (response.Result != DeviceResponse.Success)
                {
                    MessageBox.Show(response.Result.AsString(EnumFormat.Description), caption: "Device Response Move Next"
                        , buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
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
