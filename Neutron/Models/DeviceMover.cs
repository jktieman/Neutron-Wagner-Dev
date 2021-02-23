using NeutronData.Models;
using System.Collections.Generic;

namespace Neutron.Models
{
    public class DeviceMover
    {
        public readonly List<Location> Locations;
        public int MoverNumber = 0;
        public int Position = 0;

        public DeviceMover(int moverNumber, List<Location> locations)
        {
            MoverNumber = moverNumber;
            Locations = locations;
            Position = 0;
        }

        public Location MoveNext()
        {
            Location result = null;
            if (Locations.Count > 0)
            {
                if (Position + 1 < Locations.Count)
                {
                    Position += 1;
                    result = Locations[Position];
                }
            }
            return result;
        }
    }
}