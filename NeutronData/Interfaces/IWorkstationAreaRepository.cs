using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.Interfaces
{
    public interface IWorkstationAreaRepository
    {
        List<Area> GetAllAreas();
        List<Area> GetAreasByWorkstationId(int workstationId);
        List<Workstation> GetWorkstationsWithAreas();
        int[] GetAreaIdsByWorkstationId(int workstationId);
    }
}
