using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public interface ILocationManager
    {
        void Process(NovaRandomLocation location);
    }
}
