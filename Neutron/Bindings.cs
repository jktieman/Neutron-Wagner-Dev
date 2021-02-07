using Ninject.Modules;
using JsonManager;
using Neutron.Interfaces;
using Neutron.Classes;
using Neutron.Models;
using NeutronCore.Global;
using NeutronData.Interfaces;
using NeutronData.Repositories;

namespace Neutron
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {

            Bind<IJsonData>().To<JsonData>();
            Bind<IAkaRepository>().To<AkaRepository>().InSingletonScope();
            Bind<ISecurityProcessor>().To<SecurityProcessor>().InSingletonScope();
            Bind<ILacProcessor>().To<LacProcessor>().InSingletonScope();
            Bind<IStationRepository>().To<StationRepository>();
            Bind<INeutronRootDirectory>().To<NeutronRootDirectory>().InSingletonScope();
            Bind<IImageManager>().To<ImageManager>().InSingletonScope();
        }
    }
}
