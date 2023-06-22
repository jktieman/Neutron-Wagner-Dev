using NeutronData.Models;
using System.Collections.Generic;

namespace Neutron.Interfaces
{
    public interface ILacProcessor
    {

        List<Carrier> LacProfile { get; set; }
        bool UseLacProcessor { get; set; }

        void ReprocessLacSet(int userId);
        bool MovePermitted(int workstationId, int device, int carrier);
    }
}
