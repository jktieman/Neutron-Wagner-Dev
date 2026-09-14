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
using System;



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
        private readonly Color _onColor = Color.Yellow;
        private readonly Color _offColor = Color.Transparent;
        private readonly int _defaultFlashRate = 200;
        private readonly IDynamicLogger _logger;

        public Panel DeviceIndicatorPanel { get; set; }


        public DeviceIndicatorManager(WorkstationView workstationView, Point panelLocation, Size panelSize, NeutronVariables neutronVariables)
        {
            try
            {
                _workstationView = workstationView ?? throw new ArgumentNullException(nameof(workstationView));
                _panelLocation = panelLocation;
                _panelSize = panelSize;
                _neutronVariables = neutronVariables ?? throw new ArgumentNullException(nameof(neutronVariables));
                _logger = Logger.SetupLogger(@"DeviceIndicators") ?? throw new InvalidOperationException("Logger setup failed.");

                Init();
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Failed to initialize DeviceIndicatorManager.", ex);
            }
        }
        private void Init()
        {
            //_logger.LogDetailAsync("Initialize Device Indicators - InitDeviceIndicators").SafeFireAndForget();
            _deviceIndicators = new List<DeviceIndicator>();
            if (_neutronVariables.DeviceDriver == "RCC2")
            {
                if (_workstationView.CarouselShuttles == null || !_workstationView.CarouselShuttles.Any())
                {
                    return;
                }

                _hardwareDevices = _workstationView.CarouselShuttles;
            }

            if (_neutronVariables.DeviceDriver == "MP12D")
            {
                if (_workstationView.Hanels == null || !_workstationView.Hanels.Any())
                {
                    return;
                }
                _hardwareDevices = _workstationView.Hanels;
            }



            _flashRate = _neutronVariables.DeviceFlashRate > 0 ? _neutronVariables.DeviceFlashRate : _defaultFlashRate;
            _numDevices = _hardwareDevices.Count;

            CreatePanel();

            foreach (var hardwareDevice in _hardwareDevices)
            {
                try
                {
                    var device = new DeviceIndicator(hardwareDevice.DeviceNumber, _flashRate, _onColor
                                        , _offColor);
                    device.Name = $"DeviceIndicator{hardwareDevice.DeviceNumber}";
                    device.DeviceNumber = hardwareDevice.DeviceNumber;
                    device.Location = GetLocation(_panelSize.Width, _numDevices, hardwareDevice.DeviceNumber);
                    _deviceIndicators.Add(device);
                    DeviceIndicatorPanel.Controls.Add(device);
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failed to initialize device indicator for device {hardwareDevice.DeviceNumber}: {ex.Message}");
                }

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
            const int offset = 60;
            const int defaultYCoordinate = 5;

            if (numDevices <= 0)
            {
                throw new ArgumentException(@"Number of devices must be greater than zero.", nameof(numDevices));
            }
            if (deviceNumber < 1 || deviceNumber > numDevices)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceNumber), @"Device number must be between 1 and the total number of devices.");
            }
            if (sizeWidth <= 0)
            {
                throw new ArgumentException(@"Size width must be greater than zero.", nameof(sizeWidth));
            }

            var eachBlock = sizeWidth / numDevices;
            var centerBlock = eachBlock / 2;
            var positionInBlock = centerBlock - offset;

            //if (deviceNumber == 1)
            //{
            //    point = new Point(positionInBlock, defaultYCoordinate);
            //}
            //else
            //{
            //    var pos = positionInBlock + (deviceNumber - 1) * eachBlock;
            //    point = new Point(pos, defaultYCoordinate);
            //}
            //return point;
            var xCoordinate = deviceNumber == 1
                ? positionInBlock
                : positionInBlock + (deviceNumber - 1) * eachBlock;
            return new Point(xCoordinate, defaultYCoordinate);
        }

        private void RemovePanelControls()
        {
            // Removes the DeviceIndicators from the Panel  
            DeviceIndicatorPanel?.Controls.Clear();

        }

        /// <summary>
        /// Adds the  DeviceIndicators to the Panel and
        /// turns ON the device that should flash
        /// </summary>
        private void AddDeviceIndicatorsToPanel(int loc = 0)
        {
           // foreach (var hardwareDevice in _workstationView.Hanels)
            foreach (var hardwareDevice in _hardwareDevices)
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
            DeviceIndicatorPanel?.Refresh();
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
