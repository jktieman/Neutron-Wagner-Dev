using System.Runtime.InteropServices.WindowsRuntime;
using Ninject.Modules;
using JsonManager;
using Neutron.Interfaces;
using Neutron.Classes;
using Neutron.Models;
using NeutronData.General;
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
            Bind<IOrdersRepository>().To<OrdersRepository>();
            Bind<IReplenOrdersRepository>().To<ReplenOrdersRepository>();
            Bind<IInventoryManager>().To<InventoryManager>();
            Bind<FrmMain>().To<FrmMain>().InSingletonScope();
            Bind<IEnumManager>().To<EnumManager>().InSingletonScope();
        }
    }
}
