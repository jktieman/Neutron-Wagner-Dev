using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using Ninject;
using JsonManager;
using Neutron.Interfaces;
using Neutron.Classes;
using NeutronData.Interfaces;
using NeutronData.Repositories;

namespace Neutron
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IJsonData>().To<JsonData>().InSingletonScope();
            Bind<IAkaRepository>().To<AkaRepository>().InSingletonScope();
            Bind<ISecurityProcessor>().To<SecurityProcessor>().InSingletonScope();
            Bind<ILacProcessor>().To<LacProcessor>().InSingletonScope();
        }
    }
}
