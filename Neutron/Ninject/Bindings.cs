using AlliedLogger;
using AlliedPostOffice;
using IPTI.Models;
using Ninject.Modules;
using JsonManager;
using Neutron.Interfaces;
using Neutron.Classes;
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
using SAPServer;
using IDisplayController = IPTI.Models.IDisplayController;


namespace Neutron.Ninject
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
           // Bind<DbContext>().To<NeutronDb>().InThreadScope();
            Bind<IJsonData>().To<JsonData>().InSingletonScope();
            Bind<IAkaRepository>().To<AkaRepository>().InSingletonScope();
            Bind<ISecurityProcessor>().To<SecurityProcessor>().InSingletonScope();
            Bind<ILacProcessor>().To<LacProcessor>().InSingletonScope();
            Bind<INeutronRootDirectory>().To<NeutronRootDirectory>().InSingletonScope();
            Bind<IImageManager>().To<ImageManager>().InSingletonScope();
            Bind<IOrdersRepository>().To<OrdersRepository>().InSingletonScope();
            Bind<IReplenOrdersRepository>().To<ReplenOrdersRepository>().InSingletonScope();
            Bind<IInventoryManager>().To<InventoryManager>().InSingletonScope();
            Bind<IInventoryRepository>().To<InventoryRepository>().InSingletonScope();
            Bind<FrmMain>().To<FrmMain>().InSingletonScope();
            Bind<FrmSystem>().To<FrmSystem>()
                //.WithConstructorArgument("rackStation")
                .WithConstructorArgument("workstationView")
                .WithConstructorArgument("neutronVariables")
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

            Bind<FrmUtilities>().ToSelf()
                .WithConstructorArgument("neutronVariables")
                .WithConstructorArgument("neutronLicense");
               // .WithConstructorArgument("sendEmail");

            Bind<StartStopLoaderManager>().To<StartStopLoaderManager>().InSingletonScope();
            Bind<StartStopUploadManager>().To<StartStopUploadManager>().InSingletonScope();

            Bind<IEnumManager>().To<EnumManager>().InSingletonScope();
            Bind<IItemDefinitionsRepository>().To<ItemDefinitionsRepository>().InSingletonScope();
            Bind<IStoredProcedureManager>().To<StoredProcedureManager>().InSingletonScope();

            Bind<ILocationManager>().To<RandomLocationManager>().InSingletonScope();
            Bind<IVelocityCodeManager>().To<VelocityCodeManager>().InSingletonScope();
            Bind<ISizeCodeManager>().To<SizeCodeManager>().InSingletonScope();
            Bind<IHeightCodeManager>().To<HeightCodeManager>().InSingletonScope();


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
            Bind<IProLiteManager>().To<ProLiteManager>().InSingletonScope();
            Bind<IDisplayController>().To<TcpIptiController>().InSingletonScope();
           // Bind<IIptiDisplayFunctions>().To<IptiDisplayFunctions>().InSingletonScope();
            //Bind<ISendEmail>().To<SendEmail>().InSingletonScope();
            //Bind<ISapService>().To<SAPService>().InSingletonScope();
        }
    }
}
