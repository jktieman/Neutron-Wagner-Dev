using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace IPTI.Models
{
    /// <summary>
    /// The primary function is to return a properly formatted
    /// string that can be sent to the Ipti Transmitter
    /// </summary>
    public class TcpIptiCommandCenter
    {
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;
        private readonly WorkstationView _workStationView;
        public List<BayController> BayControllers = new List<BayController>();
        public List<BayController> BlastBayControllers = new List<BayController>();
        public BayController BatchBayController = null;


        private string _turnOff = "14";
        private string _turnAllOff = "1400";
        private string _ledState = "2";
        private IptiConfig _iptiConfig;
        public string LedState
        {
            get => _ledState;
            set => _ledState = value;
        }

        public TcpIptiCommandCenter(IJsonData jsonData, IDynamicLogger logger, WorkstationView workStationView)
        {
            _jsonData = jsonData;
            _logger = logger;
            _workStationView = workStationView;
            Init();

        }

        private void Init()
        {
            _logger.LogDetailAsync($"Init Start");

            _iptiConfig = _jsonData.LoadFile<IptiConfig>();

            _logger.LogDetailAsync($"IptiConfig: {_iptiConfig.OrderControlButton}").SafeFireAndForget();
            
            LoadBayControllers();

        }

        public BayController GetBayController(string bayId)
        {
            var bay = bayId.PadLeft(2, '0');

            return BayControllers.FirstOrDefault(bc => bc.BayId == bay);
        }

        public BayController GetBayController(int bayId)
        {
            var bay = bayId.ToString().PadLeft(2, '0');

            return BayControllers.FirstOrDefault(bc => bc.BayId == bay);
        }

        /// <summary>
        /// Turn off a singe display
        /// </summary>
        /// <param name="bayId"></param>
        /// <param name="displayId"></param>
        /// <returns></returns>
        public string TurnOffDisplay(string bayId, int displayId)
        {
            //var bayController = GetBayController(bayId);
            //var text  = bayController.TurnOnDisplay(displayId, text);
            //return text;
            //var display = displayId.ToString().PadLeft(2, '0');
            return GetBayController(bayId).TurnOffDisplay(displayId);
        }
        public string TurnOnDisplay(string bayId, int displayId, string text)
        {
            //var bayController = GetBayController(bayId);
            //var text  = bayController.TurnOnDisplay(displayId, quantity);
            //return text;

            return GetBayController(bayId).TurnOnDisplay(displayId, text.Trim());
        }

        public string ClearBayController(string bayId)
        {
            return GetBayController(bayId).BayId + _turnAllOff;
        }

        public string ClearOrderControlModule(string bayId)
        {
            return GetBayController(bayId).TurnOffOrderControlModule();
        }

        public void LoadBayControllers()
        {

            _logger.LogDetailAsync($"Start");
            try
            {
                var buttonColorOne = string.IsNullOrEmpty(_iptiConfig.ButtonColorOne) ? "2" : GetButtonColorId(_iptiConfig.ButtonColorOne);
                var buttonColorTwo = string.IsNullOrEmpty(_iptiConfig.ButtonColorTwo) ? "2" : GetButtonColorId(_iptiConfig.ButtonColorTwo);
                var onTime = string.IsNullOrEmpty(_iptiConfig.ButtonOnTime) ? "300" : _iptiConfig.ButtonOnTime;
                var offTime = string.IsNullOrEmpty(_iptiConfig.ButtonOffTime) ? "300" : _iptiConfig.ButtonOffTime;
                var orderControlButton = string.IsNullOrEmpty(_iptiConfig.OrderControlButton) ? "0" : GetOrderControlButton(_iptiConfig.OrderControlButton);

                if (_workStationView.Blastzones.Any())
                {
                    foreach (var blastzone in _workStationView.Blastzones)
                    {
                        var bayId = blastzone.DeviceNumber.ToString().PadLeft(2, '0');
                        var numberOfDisplays = blastzone.NumberOfCarriers;
                        var newBlastzone = new BayController("Blast", bayId, numberOfDisplays, "Micro", _iptiConfig, blastzone.Enabled);
                        BlastBayControllers.Add(newBlastzone);
                        BayControllers.Add(newBlastzone);
                    }
                }

                if (_workStationView.BatchTable != null)
                {
                    var bayId = _workStationView.BatchTable.DeviceNumber.ToString().PadLeft(2, '0');
                    var numberOfDisplays = _workStationView.BatchTable.NumberOfCarriers;
                    var newBatchBayController = new BayController("Batch", bayId, numberOfDisplays, "Max4", _iptiConfig, _workStationView.BatchTable.Enabled);
                    BatchBayController = newBatchBayController;
                    BayControllers.Add(newBatchBayController);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Exception: {ex.Message}");
            }
            _logger.LogDetailAsync($"End");
        }

        private string GetButtonColorId(string buttonColor)
        {
            switch (buttonColor)
            {
                case "Off":
                    return "0";
                case "Green":
                    return "1";
                case "Red":
                    return "2";
                case "Blue":
                    return "3";
                case "Cyan":
                    return "4";
                case "Magenta":
                    return "5";
                case "Orange":
                    return "6";
                case "White":
                    return "7";
                default:
                    return "0";
            }
        }

        private string GetOrderControlButton(string orderControlButton)
        {
            switch (orderControlButton)
            {
                case "Off":
                    return "0";
                case "Blue Flash Fast":
                    return "1";
                case "Blue Flash Slow":
                    return "2";
                case "Blue Solid":
                    return "3";
                default:
                    return "0";
            }
        }
    }
}
