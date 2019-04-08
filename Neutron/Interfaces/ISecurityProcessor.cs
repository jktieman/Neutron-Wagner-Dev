using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Interfaces
{
    public interface ISecurityProcessor
    {
        bool[] SecurityProfile { get; set; }
        void ReprocessSecuritySet(string id);
    }
}
