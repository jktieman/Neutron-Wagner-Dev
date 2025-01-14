using System.Collections.Generic;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IAreaRepository
    {

        int GetAreaId(int areaNumber);
        Area GetArea(int id);
        List<Area> Lookup();
        int[] GetAllAreaIds();
        int[] GetAllPickableAreaIds();
        List<Area> GetAllPickableAreas();
       // List<Station> GetStationsByArea(int areaId);
        List<int> GetAllPickableAreaNumbers();
        List<string> GetAllPickableAreaNumbersAsString();
        List<Area> GetAllAreas();
    }
}