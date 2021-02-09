using System.Collections.Generic;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IStationRepository
    {
        StationView GetStationView(int stationId);
        int GetStationId(int stationNumber);
        Station GetStation(int id);
        List<Station> Lookup();
        List<string> GetPickStationNumbers();
        int[] GetPickStationIds();
        List<Station> GetPickStations();
        int[] GetMoveablePickStationIds();
        List<Station> GetMovablePickStations();
        Station GetRackStation();
        StationView GetRackStationView();
        int[] GetMoveableDeviceTypeIds();

    }
}