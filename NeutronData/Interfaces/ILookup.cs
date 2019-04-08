using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Interfaces
{
    public interface ILookup
    {
        int Id { get; set; }
        string Name { get; set; }
        int Sequence { get; set; }
    }
}
