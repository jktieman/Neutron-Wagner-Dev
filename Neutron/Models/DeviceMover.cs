using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Models
{
    public class DeviceMover
    {
        public readonly List<Location> Locations;
        public int MoverNumber = 0;
        public int NextIndex = 0;

        public DeviceMover(int moverNumber, List<Location> locations)
        {
            this.MoverNumber = moverNumber;
            this.Locations = locations;
        }

        public Location MoveNext()
        {
            Location result = null;
            if (Locations.Count > 0)
            {
                if (NextIndex < Locations.Count)
                {
                    result = Locations[NextIndex];
                    NextIndex += 1;
                }
            }
            return result;
        }
    }
}