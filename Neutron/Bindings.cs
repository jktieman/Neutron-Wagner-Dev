using Ninject.Modules;
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
            Bind<IStationRepository>().To<StationRepository>();
        }
    }
}
