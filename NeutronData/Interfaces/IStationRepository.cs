using System.Collections.Generic;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IStationRepository
    {
        StationView GetStationView(int stationNumber);
        int GetStationId(int stationNumber);
        List<Station> Lookup();
        List<string> GetPickStationIds();
    }
}