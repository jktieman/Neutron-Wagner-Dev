using System.Collections.Generic;
using System.Linq;

namespace IPTI.Models
{
    /// <summary>
    /// Represents a controller for a bay in the IPTI system.
    /// </summary>
    /// <remarks>
    /// The BayController class is responsible for managing the displays and the order control module of a bay.
    /// It provides methods to turn on and off displays, clear displays and the order control module, and get the ID of the bay.
    /// </remarks>
    public class BayController
    {
        /// <summary>
        /// Gets the unique identifier for the bay controlled by this BayController.
        /// </summary>
        /// <value>
        /// The unique identifier for the bay.
        /// </value>
        public string BayId { get; }

        /// <summary>
        /// Gets or sets the type of the bay controller.
        /// </summary>
        /// <value>
        /// The type of the bay controller.
        /// </value>
        /// <remarks>
        /// This property is used to distinguish between different types of bay controllers in the IPTI system.
        /// For example, it can be used to identify a bay controller as a "Blast" type, as seen in the `FrmPick.ClearBlastzone()` method.
        /// </remarks>
        public string BayControllerType
        {
            get => _bayControllerType;
            set => _bayControllerType = value;
        }

        /// <summary>
        /// Gets the number of displays managed by the BayController.
        /// </summary>
        /// <value>
        /// The number of displays.
        /// </value>
        public int NumberOfDisplays
        {
            get => _numberOfDisplays; 
            set => _numberOfDisplays = value;
        }

        /// <summary>
        /// Specifies the type of display used in the bay.
        /// </summary>
        /// <remarks>
        /// This field is used to determine which type of display (e.g., "Max4" or "Micro") to load during the initialization of the BayController.
        /// </remarks>
        private readonly string _displayType;

        /// <summary>
        /// Represents a list of IPTI displays managed by the BayController.
        /// </summary>
        /// <remarks>
        /// This field is used to store and manage the different types of IPTI displays (Max4 and Micro) associated with the BayController.
        /// Each display in the list can be individually turned on and off.
        /// </remarks>
        private List<IIptiDisplay> _displays;

        /// <summary>
        /// Represents the order control module associated with the bay.
        /// </summary>
        /// <remarks>
        /// This field is used to manage the order control operations in the bay, such as turning on the order control module and clearing it.
        /// </remarks>
        private readonly OrderControlModule _orderControlModule;

        /// <summary>
        /// Specifies the type of the bay controller.
        /// </summary>
        /// <remarks>
        /// This field is used to distinguish between different types of bay controllers in the IPTI system.
        /// </remarks>
        private string _bayControllerType;

        private int _numberOfDisplays;

        /// <summary>
        /// Initializes a new instance of the BayController class.
        /// </summary>
        /// <param name="bayControllerType">The type of the bay controller.</param>
        /// <param name="bayId">The ID of the bay.</param>
        /// <param name="numberOfDisplays">The number of displays in the bay.</param>
        /// <param name="displayType">The type of display used in the bay.</param>
        /// <param name="iptiConfig"></param>
        /// <remarks>
        /// The constructor initializes the BayController with the provided parameters, creates a new OrderControlModule, and loads the displays.
        /// </remarks>
        public BayController(string bayControllerType, string bayId, int numberOfDisplays, string displayType, IptiConfig iptiConfig)
        {
            BayId = bayId;
            _bayControllerType = bayControllerType;
            _displayType = displayType;

            _numberOfDisplays = numberOfDisplays;
            _orderControlModule = new OrderControlModule("01", iptiConfig.OrderControlButton);
            LoadDisplays(iptiConfig);
        }

        /// <summary>
        /// Initializes the list of displays for the bay controller based on the specified display type and number of displays.
        /// </summary>
        /// <remarks>
        /// This method creates a new list of IIptiDisplay objects and populates it based on the display type ("Max4" or "Micro") and the number of displays.
        /// Each display is assigned a unique ID starting from 1 up to the number of displays.
        /// </remarks>
        private void LoadDisplays(IptiConfig iptiConfig)
        {
            var config = iptiConfig;
            _displays = new List<IIptiDisplay>();
            if (_displayType == "Max4")

                for (int i = 1; i <= _numberOfDisplays; i++)
                {
                    _displays.Add(new IptiMax4(i, config.ButtonColorOne, config.ButtonColorTwo, config.ButtonOnTime, config.ButtonOffTime));

                }
            else if(_displayType == "Micro")
                for (int i = 1; i <= _numberOfDisplays; i++)
                {
                    _displays.Add(new IptiMicro(i, config.ButtonColorOne, config.ButtonColorTwo, config.ButtonOnTime, config.ButtonOffTime));
                }
        }

        /// <summary>
        /// Turns on the specified display with the given quantity.
        /// </summary>
        /// <param name="displayId">The ID of the display to be turned on.</param>
        /// <param name="quantity">The quantity to be displayed.</param>
        /// <returns>A string that represents the Bay ID concatenated with the result of the TurnOn operation on the display.</returns>
        /// <remarks>
        /// This method first finds the display with the given ID from the list of displays. If such a display is found, it turns on the display with the specified quantity and returns the Bay ID concatenated with the result of the TurnOn operation. If the display is not found, it returns an empty string.
        /// </remarks>
        public string TurnOnDisplay(int displayId, string quantity)
        {
            var display = _displays.FirstOrDefault(d => d.DisplayId == displayId.ToString().PadLeft(2, '0'));
            if(display == null)
                return string.Empty;
            return BayId + display.TurnOn(quantity);
        }

        /// <summary>
        /// Turns off the specified display in the bay.
        /// </summary>
        /// <param name="displayId">The ID of the display to be turned off.</param>
        /// <returns>A string representing the state of the display after being turned off. If the display is not found, an empty string is returned.</returns>
        /// <remarks>
        /// This method first searches for the display with the provided ID in the list of displays managed by the BayController. 
        /// If the display is found, it is turned off and the method returns a string that includes the BayId and the state of the display after being turned off.
        /// If the display is not found, the method returns an empty string.
        /// </remarks>
        public string TurnOffDisplay(int displayId)
        {
            var display = _displays.FirstOrDefault(d => d.DisplayId == displayId.ToString().PadLeft(2, '0'));
            if (display == null)
                return string.Empty;
            return BayId + display.TurnOff();
        }
        
        public string ClearDisplays()
        {
            return BayId + "14";
        }

        public string TurnOffOrderControlModule()
        {
            return BayId + _orderControlModule.Clear();
        }

        public string TurnOnOrderControlModule(string text)
        {
            return BayId + _orderControlModule.TurnOn(text);
        }

        public string GetBayId()
        {
            return BayId;
        }
    }
}
