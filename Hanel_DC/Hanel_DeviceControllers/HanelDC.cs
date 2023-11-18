using System.Collections.Generic;

namespace Hanel_DC.Hanel_DeviceControllers
{
    internal class HanelDC
    {
        /// <summary>
        /// Takes a list of int values
        /// </summary>
        /// <param name="EnabledDeviceUnitNumbers">List of Integers </param>
        /// <returns>Returns a list of sorted int values</returns>
        public static List<int> GetSortedListOfUniqueDeviceUnitNumbers(
            IReadOnlyCollection<int> EnabledDeviceUnitNumbers)
        {
            List<int> deviceUnitNumbers = new List<int>();
            foreach (int deviceUnitNumber in (IEnumerable<int>)EnabledDeviceUnitNumbers)
            {
                if (deviceUnitNumber >= 0 && !deviceUnitNumbers.Contains(deviceUnitNumber))
                    deviceUnitNumbers.Add(deviceUnitNumber);
            }
            if (deviceUnitNumbers.Count > 1)
                deviceUnitNumbers.Sort();
            return deviceUnitNumbers;
        }
    }
}
