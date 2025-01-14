using NeutronData.Models;
using System.Collections.Generic;
using AlliedLogger;

namespace Neutron.Models
{
    public class DeviceMover
    {
        public readonly List<Location> Locations;
        private readonly IDynamicLogger _logger;
        public int MoverNumber = 0;
        public int Position = 0;

        public DeviceMover(int moverNumber, List<Location> locations, IDynamicLogger logger )
        {
            MoverNumber = moverNumber;
            Locations = locations;
            _logger = logger;
            Position = 0;
        }

        public Location MoveNext()
        {
           // _logger.LogDetailAsync($"MoveNext 2");
            Location result = null;
            if (Locations.Count > Position)
            {
               // _logger.LogDetailAsync($"MoveNext 3");
                result = Locations[Position];
               // _logger.LogDetailAsync($"MoveNext 4");
                Position += 1;
               // _logger.LogDetailAsync($"MoveNext 5");
            }
           // _logger.LogDetailAsync($"MoveNext 6");
            return result;
        }
}
}