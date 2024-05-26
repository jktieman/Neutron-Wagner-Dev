namespace IPTI.Models
{
    /// <summary>
    /// Represents a specific type of IPTI display, the IptiMax4.
    /// </summary>
    /// <remarks>
    /// This class implements the IIptiDisplay interface and provides specific
    /// implementations for the TurnOn and TurnOff methods.
    /// </remarks>
    public class IptiMax4 : IptiDisplayBase, IIptiDisplay
    {
        private readonly string _recall = "0";
        private readonly string _extended = "0";
        private readonly string _infrared = "0";
        private readonly string _flash = "0";
        private readonly string _displayColor = "00";
        private readonly string _handSensor = "0";
        private readonly string _optional = "0";
        private readonly string _fourSpaces = $"    ";

        /// <summary>
        /// Gets the unique identifier for the IptiMax4 display.
        /// </summary>
        /// <value>
        /// The display identifier, represented as a string.
        /// </value>
        /// <remarks>
        /// The display identifier is used in the TurnOn and TurnOff methods to specify which display to control.
        /// </remarks>
        public string DisplayId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IptiMax4"/> class.
        /// </summary>
        /// <param name="displayId">The unique identifier for the display.</param>
        /// <param name="aStateColor"></param>
        /// <param name="bStateColor"></param>
        /// <param name="onTime"></param>
        /// <param name="offTime"></param>
        /// <param name="configTransmitDelay"></param>
        public IptiMax4(int displayId, string aStateColor = "3", string bStateColor = "0", string onTime = "300",
            string offTime = "300")
        {
            DisplayId = displayId.ToString().PadLeft(2, '0');
            AStateColor = aStateColor;
            BStateColor = bStateColor;
            OnTime = onTime;
            OffTime = offTime;
            TurnOnCommand = "33";
            TurnOffCommand = "14";
            ButtonControl = "0";
            Arrows = "0";
            LedState = "4";
        }
        
        /// <summary>
        /// Turns on the IptiMax4 display with a specified quantity.
        /// </summary>
        /// <param name="quantity">The quantity to be displayed on the IptiMax4 display.</param>
        /// <returns>A string representing the state of the IptiMax4 display after being turned on.</returns>
        public override string TurnOn(string quantity) => TurnOnCommand + DisplayId
                                                      + quantity.PadLeft(4, ' ')
                                                      + _fourSpaces
                                                      + Arrows
                                                      + _recall
                                                      + _extended
                                                      + _infrared
                                                      + _flash
                                                      + _displayColor
                                                      + _handSensor
                                                      + AStateColor
                                                      + OnTime
                                                      + BStateColor
                                                      + OffTime
                                                      + _optional;


        public override string TurnOff() => TurnOffCommand + DisplayId;

        /// <summary>
        /// Turns on the IptiMax4 display with the word "END".
        /// </summary>

        /// <returns>A string representing the state of the IptiMax4 display after being turned on.</returns>
        public override string TurnOnEnd() => TurnOnCommand + DisplayId
                                                                        + _fourSpaces
                                                                        + " END"
                                                                        + Arrows
                                                                        + _recall
                                                                        + _extended
                                                                        + _infrared
                                                                        + _flash
                                                                        + _displayColor
                                                                        + _handSensor
                                                                        + "1"
                                                                        + OnTime
                                                                        + "1"
                                                                        + OffTime
                                                                        + _optional;


    }
}
