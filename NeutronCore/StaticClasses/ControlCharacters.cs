namespace NeutronCore.StaticClasses
{
    /// <summary>
    /// Provides a set of ASCII control characters commonly used in Ethernet transmissions.
    /// </summary>
    /// <remarks>
    /// This class contains constants for the Start of Header (SOH), Start of Text (SOT), 
    /// End of Text (ETX), and Acknowledge (ACK) control characters.
    /// </remarks>
    public static class ControlCharacters
    {
        /// <summary>
        /// Represents the Start of Header (SOH) control character in ASCII. 
        /// This character is used to indicate the start of a text header in a transmission.
        /// </summary>
        /// public char SOH = Convert.ToChar(1);
        public const char SOH = '\x01';

        /// <summary>
        /// Represents the Start of Text (SOT) control character in ASCII.
        /// This character is used to indicate the start of a text body in a transmission.
        /// </summary>
        public const char SOT = '\x02';

        /// <summary>
        /// Represents the Start of Header (SOH) control character in ASCII. 
        /// This character is used to indicate the start of a text header in a transmission.
        /// </summary>
        /// public char ETX = Convert.ToChar(3);   
        public const char ETX = '\x03';

        /// <summary>
        /// Represents the Start of Header (SOH) control character in ASCII. 
        /// This character is used to indicate the start of a text header in a transmission.
        /// </summary>
        /// public char ACK = Convert.ToChar(6);       
        public const char ACK = '\x06';

    }
}
