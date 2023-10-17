using AlliedLogger;
using Ninject.Modules;
using JsonManager;
using Neutron.Interfaces;
using Neutron.Classes;
using Neutron.Controllers;
using Neutron.Forms;
using Neutron.Global;
using Neutron.Models;
using NeutronData.General;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronLoader;
using NeutronMaintenance;
using SqlSchemaManager;
using ProliteController;


namespace Neutron.Ninject
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
           // Bind<DbContext>().To<NeutronDb>().InThreadScope();
            Bind<IJsonData>().To<JsonData>();
            Bind<IAkaRepository>().To<AkaRepository>().InSingletonScope();
            Bind<ISecurityProcessor>().To<SecurityProcessor>().InSingletonScope();
            Bind<ILacProcessor>().To<LacProcessor>().InSingletonScope();
            Bind<INeutronRootDirectory>().To<NeutronRootDirectory>().InSingletonScope();
            Bind<IImageManager>().To<ImageManager>().InSingletonScope();
            Bind<IOrdersRepository>().To<OrdersRepository>();
            Bind<IReplenOrdersRepository>().To<ReplenOrdersRepository>();
            Bind<IInventoryManager>().To<InventoryManager>();
            Bind<IInventoryRepository>().To<InventoryRepository>();
            Bind<FrmMain>().To<FrmMain>().InSingletonScope();
            Bind<FrmSystem>().To<FrmSystem>()
                .WithConstructorArgument("rackStation")
                .WithConstructorArgument("standAlone");

            Bind<FrmInventory>().To<FrmInventory>()
                .WithConstructorArgument("workstationView")
                .WithConstructorArgument("neutronVariables");

            Bind<FrmPick>().ToSelf()
                .WithConstructorArgument("neutronVariables")
                .WithConstructorArgument("neutronLicense")
                .WithConstructorArgument("workstationView")
                .WithConstructorArgument("historyManager");

            Bind<FrmHotAction>().To<FrmHotAction>()
                .WithConstructorArgument("neutronVariables")
                .WithConstructorArgument("neutronLicense")
                .WithConstructorArgument("workstationView")
                .WithConstructorArgument("historyManager");
                //.WithConstructorArgument("Item");
                //.WithConstructorArgument("quantity");
                //.WithConstructorArgument("pickList");

            Bind<FrmReplen>().ToSelf()
                .WithConstructorArgument("neutronVariables")
                .WithConstructorArgument("neutronLicense")
                .WithConstructorArgument("workstationView")
                .WithConstructorArgument("historyManager");

            Bind<StartStopLoaderManager>().To<StartStopLoaderManager>();
            Bind<StartStopUploadManager>().To<StartStopUploadManager>();

            Bind<IEnumManager>().To<EnumManager>().InSingletonScope();
            Bind<IItemDefinitionsRepository>().To<ItemDefinitionsRepository>();
            Bind<IStoredProcedureManager>().To<StoredProcedureManager>();

            Bind<ILocationManager>().To<RandomLocationManager>().InSingletonScope();
            Bind<IVelocityCodeManager>().To<VelocityCodeManager>().InSingletonScope();
            Bind<IMasterMaintenanceProcessor>().To<MasterMaintenanceProcessor>().InSingletonScope();
            Bind<IHistoryManager>().To<HistoryManager>().WithConstructorArgument("workstationView");
            Bind<IDynamicLogger>().To<DynamicLogger>()
                .WithConstructorArgument("logFileDir", string.Empty)
                .WithConstructorArgument("folderName", @"General")
                .WithConstructorArgument("logActivity", "false");

            Bind<IWorkstationRepository>().To<WorkstationRepository>().InSingletonScope();
            Bind<IAreaRepository>().To<AreaRepository>().InSingletonScope();
            Bind<IRFIDManager>().To<RFIDManager>().InSingletonScope();
            Bind<ILocationsRepository>().To<LocationsRepository>().InSingletonScope();
            Bind<IBlastzone>().To<Blastzone>().InSingletonScope();
            Bind<IProliteManager>().To<ProliteManager>().InSingletonScope();
        }
    }
}
