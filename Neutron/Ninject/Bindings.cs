using AlliedLogger;
using IPTI.Models;
using JsonManager;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Neutron.Classes;
using Neutron.Forms;
using Neutron.Global;
using Neutron.Interfaces;
using Neutron.Models;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.General;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.Repositories;
using NeutronData.UnitOfWorks;
using NeutronEvents;
using NeutronLoader;
using NeutronMaintenance;
using Ninject;
using Ninject.Modules;
using ProliteController;
using SqlSchemaManager;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using ReplenService;
using IDisplayController = IPTI.Models.IDisplayController;


namespace Neutron.Ninject
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<DbConnection>().To<SqlConnection>().InSingletonScope();
           // Bind<DbContext>().To<NeutronDb>().InThreadScope();
            Bind<NeutronDb>().ToSelf();
            // Add the binding for Func<NeutronDb>
            Bind<Func<NeutronDb>>().ToMethod(context =>
            {
                return () => context.Kernel.Get<NeutronDb>(); 
            });
            Bind(typeof(GenericRepository<>)).ToSelf().InTransientScope();
            Bind<IJsonData>().To<JsonData>().InSingletonScope();
            Bind<IAkaRepository>().To<AkaRepository>().InSingletonScope();
            Bind<ISecurityProcessor>().To<SecurityProcessor>().InSingletonScope();
            Bind<ILacProcessor>().To<LacProcessor>().InSingletonScope();
            Bind<ILocationUnitOfWork>().To<LocationUnitOfWork>().InSingletonScope();
            Bind<IInventoryUnitOfWork>().To<InventoryUnitOfWork>().InSingletonScope();
            Bind<INeutronRootDirectory>().To<NeutronRootDirectory>().InSingletonScope();
            Bind<IImageManager>().To<ImageManager>().InSingletonScope();
            Bind<IOrdersRepository>().To<OrdersRepository>().WithConstructorArgument("workstation");
            Bind<IReplenOrdersRepository>().To<ReplenOrdersRepository>().InSingletonScope();
            Bind<IReplenRepository>().To<ReplenRepository>().InSingletonScope();
            Bind<IInventoryManager>().To<InventoryManager>().InSingletonScope();
            Bind<IInventoryRepository>().To<InventoryRepository>().InSingletonScope();
            Bind<IDialogService>().To<DialogService>().InSingletonScope();

            
            Bind<Mediator>().ToSelf().InSingletonScope();
            Bind<FrmMain>().ToSelf();
           Bind<FrmSystem>().To<FrmSystem>()
                
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

            Bind<IOrderDetailsRepository>().To<OrderDetailsRepository>().InSingletonScope();
            Bind<IWorkstationRepository>().To<WorkstationRepository>().InSingletonScope();
            Bind<IAreaRepository>().To<AreaRepository>().InSingletonScope();
            Bind<IRFIDManager>().To<RFIDManager>().InSingletonScope();
            Bind<ILocationsRepository>().To<LocationsRepository>().InSingletonScope();
            Bind<IBlastzone>().To<Blastzone>().InSingletonScope();
            Bind<IProLiteManager>().To<ProLiteManager>().InSingletonScope();
            Bind<IDisplayController>().To<TcpIptiController>().InSingletonScope();
            Bind<IPrintJobRepository>().To<PrintJobRepository>().InSingletonScope();
            Bind<IOptions<MemoryCacheOptions>>().ToConstant(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
            Bind<IMemoryCache>().To<MemoryCache>().InSingletonScope();
            Bind<IHistoryRepository>().To<HistoryRepository>().InSingletonScope();

            Bind<DeleteHelper>().ToSelf().InSingletonScope();
            
            // Bind<IIptiDisplayFunctions>().To<IptiDisplayFunctions>().InSingletonScope();
            //Bind<ISendEmail>().To<SendEmail>().InSingletonScope();
            //Bind<ISapService>().To<SAPService>().InSingletonScope();
        }
    }
}
