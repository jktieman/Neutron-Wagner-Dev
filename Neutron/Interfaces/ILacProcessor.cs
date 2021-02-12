using NeutronData.Models;
using System.Collections.Generic;

namespace Neutron.Interfaces
{
    public interface ILacProcessor
    {

        List<Carrier> LacProfile { get; set; }
        void ReprocessLacSet(int userId);
        bool MovePermitted(int station, int device, int carrier);
    }
}
