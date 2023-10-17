using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.Interfaces
{
    public interface IWorkstationRepository
    {
        Task<WorkstationView> GetStationView(int workstationId);
        int GetStationId(int stationNumber);
        Workstation GetStation(int id);
        List<Workstation> Lookup();
        List<string> GetPickStationNumbers();
        int[] GetPickStationIds();
        List<Workstation> GetPickStations();
        int[] GetMoveablePickStationIds();
        List<Workstation> GetMovablePickStations();
        Workstation GetRackStation(int workstationId);
        WorkstationView GetRackStationView();
        int[] GetMoveableDeviceTypeIds();
        int[] GetAllPickStationIds();
        List<Workstation> GetAllPickStations();

    }
}