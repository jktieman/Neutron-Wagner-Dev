namespace IPTI.Models
{
    /// <summary>
    /// Defines the basic operations for an IPTI display.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by different types of IPTI displays, each providing their own implementation for the TurnOn and TurnOff methods.
    /// </remarks>
    public interface IIptiDisplay
    {
        /// <summary>
        /// Gets the display ID.
        /// </summary>
        string DisplayId { get; set; }
        /// <summary>
        /// Turns on the display with a specified quantity.
        /// </summary>
        /// <param name="quantity">The quantity to be displayed.</param>
        /// <returns>A string representing the state of the display after being turned on.</returns>
        string TurnOn(string quantity);
        /// <summary>
        /// Turns off the display.
        /// </summary>
        /// <returns>A string representing the state of the display after being turned off.</returns>
        string TurnOff();
    }

}
