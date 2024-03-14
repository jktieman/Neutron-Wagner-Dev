using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeutronData.ModelViews;
using CurrentDeviceIndicator;
using AlliedLogger;
using System.Windows.Forms;
using System.Drawing;
using NeutronCore.Enums;
using NeutronCore.Global;
using NeutronData.Models;
using Logger = NeutronCore.Global.Logger;

namespace DeviceIndicatorService
{
    public class DeviceIndicatorManager
    {

        private readonly WorkstationView _workstationView;
        private readonly Point _panelLocation;
        private readonly Size _panelSize;
        private readonly NeutronVariables _neutronVariables;
        private List<DeviceIndicator> _deviceIndicators;
        private List<HardwareDevice> _hardwareDevices;
        private int _numDevices;
        private int _flashRate;
        private readonly IDynamicLogger _logger;

        public Panel DeviceIndicatorPanel { get; set; }


        public DeviceIndicatorManager(WorkstationView workstationView, Point panelLocation, Size panelSize, NeutronVariables neutronVariables)
        {

            _workstationView = workstationView;
            _panelLocation = panelLocation;
            _panelSize = panelSize;
            _neutronVariables = neutronVariables;
            _logger = Logger.SetupLogger(@"DeviceIndicators");
            Init();
        }

        //private void SetupLogger()
        //{
        //    var logFileDir = LoaderSettings.GetLogFileDirectory();
        //    var folderName = @"DeviceIndicators";
        //    var logActivity = LoaderSettings.EnableLogging;
        //    _logger = new DynamicLogger(logFileDir, folderName, logActivity);
        //}

        private void Init()
        {
            _ = _logger.LogDetailAsync("Initialize Device Indicators - InitDeviceIndicators");
            _deviceIndicators = new List<DeviceIndicator>();
            if (_workstationView.Hanels.Any())
            {
                _hardwareDevices = _workstationView.Hanels;
            }
            
            if (_hardwareDevices.Count == 0)
            {
                return;
            }
            
            _flashRate = _neutronVariables.DeviceFlashRate;
            _numDevices = _hardwareDevices.Count;

            CreatePanel();

            foreach (var hardwareDevice in _hardwareDevices)
            {
                var device = new DeviceIndicator(hardwareDevice.DeviceNumber, _flashRate, Color.Yellow
                    , Color.Transparent);
                device.Name = $"DeviceIndicator{hardwareDevice.DeviceNumber}";
                device.DeviceNumber = hardwareDevice.DeviceNumber;
                device.Location = GetLocation(_panelSize.Width, _numDevices, hardwareDevice.DeviceNumber);
                _deviceIndicators.Add(device);
                DeviceIndicatorPanel.Controls.Add(device);
            }
        }

        private void CreatePanel()
        {
            DeviceIndicatorPanel = new Panel();
            DeviceIndicatorPanel.Location = _panelLocation;
            DeviceIndicatorPanel.Size = _panelSize;
            DeviceIndicatorPanel.BackColor = Color.Transparent;
            DeviceIndicatorPanel.Name = "PanelDeviceIndicators";
        }

        public Point GetLocation(int sizeWidth, int numDevices, int deviceNumber)
        {
            Point point;
            var eachBlock = sizeWidth / numDevices;
            var centerBlock = eachBlock / 2;
            var positionInBlock = centerBlock - 60;
            if (deviceNumber == 1)
            {
                point = new Point(positionInBlock, 5);
            }
            else
            {
                var pos = positionInBlock + (deviceNumber - 1) * eachBlock;
                point = new Point(pos, 5);
            }
            return point;

        }

        private void RemovePanelControls()
        {
            // Removes the DeviceIndicators from the Panel  
            DeviceIndicatorPanel.Controls.Clear();

        }

        /// <summary>
        /// Adds the  DeviceIndicators to the Panel and
        /// turns ON the device that should flash
        /// </summary>
        private void AddDeviceIndicatorsToPanel(int loc = 0)
        {
            foreach (var hardwareDevice in _workstationView.Hanels)
            {
                //if (hardwareDevice.DeviceTypeId != (int)DeviceTypeEnum.Hanel12D &&
                //    hardwareDevice.DeviceTypeId != (int)DeviceTypeEnum.Hanel12N &&
                //    hardwareDevice.DeviceTypeId != (int)DeviceTypeEnum.Carousel &&
                //    hardwareDevice.DeviceTypeId != (int)DeviceTypeEnum.Shuttle) continue;

                var device = new DeviceIndicator(hardwareDevice.DeviceNumber, _flashRate, Color.Yellow
                    , Color.Transparent);
                device.Name = $"DeviceIndicator{hardwareDevice.DeviceNumber}";
                device.DeviceNumber = hardwareDevice.DeviceNumber;
                device.Location = GetLocation(_panelSize.Width, _numDevices, hardwareDevice.DeviceNumber);
                if (device.DeviceNumber == loc) device.BlinkOn();
                // add the DeviceIndicator to the List of DeviceIndicators
                _deviceIndicators.Add(device);
                // Add the DeviceIndicator Control to the Panel
                DeviceIndicatorPanel.Controls.Add(device);

            }
            DeviceIndicatorPanel.Refresh();
        }

        public async void UpdateCurrentDeviceIndicator(int loc)
        {
            await _logger.LogDetailAsync($"Set Current Device Indicator BLINK ON -- {loc}");

            RemovePanelControls();
            AddDeviceIndicatorsToPanel(loc);
        }

        public async void ClearActiveDeviceIndicators()
        {
            await _logger.LogDetailAsync("Clear Active Device Indicators START");

            RemovePanelControls();
            AddDeviceIndicatorsToPanel();
        }

        public async void ClearAllDeviceIndicators()
        {
            await _logger.LogDetailAsync("Clear ALL Active Device Indicators START");

            RemovePanelControls();
            AddDeviceIndicatorsToPanel();
        }
    }
}
