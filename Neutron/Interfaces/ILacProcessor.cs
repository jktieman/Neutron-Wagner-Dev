using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Interfaces
{
    public interface ILacProcessor
    {

        List<Carrier> LacProfile { get; set; }
        void ReprocessLacSet(int userId);
        bool MovePermitted(int station, int device, int carrier);
    }
}
