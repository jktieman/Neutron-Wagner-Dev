using System.Collections.Generic;
using System.Threading.Tasks;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface ILocationsRepository
    {
        IEnumerable<Location> AvailableLocations();
        IEnumerable<Location> AvailableLocationsAll();
        Task<IEnumerable<LocationView>> GetAllLocationViewsExact(Station station, int sizeCodeId, int velocityCodeId, int heightCodeId, int locationCodeId, bool inUse );
        IEnumerable<LocationView> FindLocationViewsByStation(Station station);
        IEnumerable<LocationView> FindLocationViews(string find = "");
        void SetLocationInUse(int locationId, bool b);
    }
}