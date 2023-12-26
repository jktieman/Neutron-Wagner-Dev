using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlliedLogger;
using JsonManager;

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
        public List<BayController> BayControllers = new List<BayController>();
        private string _turnOff = "14";
        private string _turnAllOff = "1400";
        private string _ledState = "2";
        private IptiConfig _iptiConfig;
        public string LedState
        {
            get => _ledState;
            set => _ledState = value;
        }

        public TcpIptiCommandCenter(IJsonData jsonData, IDynamicLogger logger)
        {
            _jsonData = jsonData;
            _logger = logger;
            _iptiConfig = _jsonData.LoadFile<IptiConfig>();
            LoadBayControllers();
        }
        public BayController GetBayController(string bayId)
        {
            var bay = bayId.PadLeft(2, '0');
            
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
            var buttonColorOne = string.IsNullOrEmpty(_iptiConfig.ButtonColorOne) ? "2" : GetButtonColorId(_iptiConfig.ButtonColorOne);
            var buttonColorTwo = string.IsNullOrEmpty(_iptiConfig.ButtonColorTwo) ? "2" : GetButtonColorId(_iptiConfig.ButtonColorTwo);
            var onTime = string.IsNullOrEmpty(_iptiConfig.ButtonOnTime) ? "300" : _iptiConfig.ButtonOnTime;
            var offTime = string.IsNullOrEmpty(_iptiConfig.ButtonOffTime) ? "300" : _iptiConfig.ButtonOffTime;
            var orderControlButton = string.IsNullOrEmpty(_iptiConfig.OrderControlButton) ? "0" : GetOrderControlButton(_iptiConfig.OrderControlButton);
            BayControllers = new List<BayController>
            {
                new BayController("Blast", "01", 32, "Micro", _iptiConfig), 
                new BayController("Blast", "02", 32, "Micro", _iptiConfig),
                new BayController("Blast", "03", 64, "Micro", _iptiConfig),
                new BayController("Batch", "04", 16, "Max4", _iptiConfig)
            };
        }

        private string GetButtonColorId(string buttonColor)
        {
            switch (buttonColor)
            {
                case "Off":
                    return "0";
                    break;
                case "Green":
                    return "1";
                break;
                case "Red":
                    return "2";
                break;
                case "Blue":
                    return "3";
                break;
                case "Cyan":
                    return "4";
                break;
                case "Magenta":
                    return "5";
                    break;
                case "Orange":
                    return "6";
                    break;
                case "White":
                    return "7";
                    break;
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
                    break;
                case "Blue Flash Fast":
                    return "1";
                    break;
                case "Blue Flash Slow":
                    return "2";
                    break;
                case "Blue Solid":
                    return "3";
                    break;
                default:
                    return "0";
            }
        }
    }
}
