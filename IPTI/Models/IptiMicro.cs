namespace IPTI.Models
{
    /// <summary>
    /// Represents a Micro IPTI display.
    /// </summary>
    /// <remarks>
    /// This class is a specific implementation of the IIptiDisplay interface for Micro IPTI displays.
    /// It provides the functionality to turn on and off the display, and to get the display ID.
    /// </remarks>
    public class IptiMicro : IptiDisplayBase, IIptiDisplay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IptiMicro"/> class.
        /// </summary>
        /// <param name="displayId">The ID of the display.</param>
        /// <param name="aStateColor">The color of the A state. Default is "3".</param>
        /// <param name="bStateColor">The color of the B state. Default is "0".</param>
        /// <param name="onTime"></param>
        /// <param name="offTime"></param>
        public IptiMicro(int displayId, string aStateColor = "3", string bStateColor = "0", string onTime = "300",
            string offTime = "300")
        {
            DisplayId = displayId.ToString().PadLeft(2, '0');
            AStateColor = aStateColor;
            BStateColor = bStateColor;
            OnTime = onTime;
            OffTime = offTime;
            TurnOnCommand = "10";
            TurnOffCommand = "14";
            ButtonControl = "0";
            Arrows = "0";
            LedState = "4";
        }

        /// <summary>
        /// Gets the unique identifier for the Micro IPTI display.
        /// </summary>
        /// <value>
        /// The display identifier, a string of two characters.
        /// </value>
        public string DisplayId { get; set; }

        /// <summary>
        /// Turns on the Micro IPTI display with a specified quantity.
        /// </summary>
        /// <param name="quantity">The quantity to be displayed on the Micro IPTI display.</param>
        /// <returns>A string representing the state of the Micro IPTI display after being turned on.</returns>
        public override string TurnOn(string quantity)
        {
            if (quantity.Length > 2)
            {
                quantity = "00";
            }
            return TurnOnCommand + DisplayId
                                 + LedState + AStateColor + OnTime
                                 + BStateColor + OffTime + ButtonControl + Arrows
                                 + quantity.PadLeft(2, '0');
        }

        public override string TurnOnEnd()
        {
            var aColor = "1";
            var bColor = "1";
            var quantity = "00";

            return TurnOnCommand + DisplayId
                                 + LedState + aColor + OnTime
                                 + bColor + OffTime + ButtonControl + Arrows
                                 + quantity.PadLeft(2, '0');
        }

        /// <summary>
        /// Turns off the Micro IPTI display.
        /// </summary>
        /// <returns>A string representing the state of the Micro IPTI display after being turned off.</returns>
        public override string TurnOff() => TurnOffCommand + DisplayId;


    }
}
