using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;
using Ninject;
using System.Reflection;
using JsonManager;
using Neutron.Interfaces;
using System.Threading;
using NeutronCore.Models;
using NeutronData.Interfaces;
using Neutron.Models;
using System.Globalization;
using System.IO;
using AlliedLogger;
using AlliedPostOffice;
using Neutron.Forms;
using NeutronCore;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.General;
using NeutronData.Models;
using Newtonsoft.Json;
using SqlSchemaManager;

namespace Neutron
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        public static NeutronLicense NeutronLicense = new NeutronLicense();
        private static Mutex _mutex = null;

        [STAThread]

        static void Main()
        {


            //Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-CA");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");

            const string appName = "Neutron";

            bool createdNew;
            _mutex = new Mutex(initiallyOwned: true, name: appName, createdNew: out createdNew);
            if (!createdNew)
            {
                //app is already running!  Exiting the application
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(defaultValue: false);
            //Database.SetInitializer(new NullDatabaseInitializer<NeutronDb>());


            //var kernel = new StandardKernel();
            //kernel.Load(Assembly.GetExecutingAssembly());
            //var jsonData = kernel.Get<IJsonData>();
            //jsonData.RootDirectory =  $"{Properties.Settings.Default.RootDirectory}";
            //LoaderSettings.SetRootDirectory($"{Properties.Settings.Default.RootDirectory}");
            //var akaRepository = kernel.Get<IAkaRepository>();
            //var securityProcessor = kernel.Get<ISecurityProcessor>();
            //var lacProcessor = kernel.Get<ILacProcessor>();

            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            var rootDirectory = kernel.Get<INeutronRootDirectory>().RootDirectory;

            var jsonData = kernel.Get<IJsonData>();
            jsonData.RootDirectory = rootDirectory;
            LoaderSettings.SetRootDirectory(rootDirectory);

            //jsonData.RootDirectory = $"{Properties.Settings.Default.RootDirectory}";
            //LoaderSettings.SetRootDirectory($"{Properties.Settings.Default.RootDirectory}");

            var context = new NeutronDb().CheckConnection();

            if (context)
            {
                var stationRepository = kernel.Get<IStationRepository>();
                var ordersRepository = kernel.Get<IOrdersRepository>();
                var replenOrdersRepository = kernel.Get<IReplenOrdersRepository>();
            }
            var akaRepository = kernel.Get<IAkaRepository>();
            var securityProcessor = kernel.Get<ISecurityProcessor>();
            var lacProcessor = kernel.Get<ILacProcessor>();
            var imageManager = kernel.Get<IImageManager>();
            var enumManager = kernel.Get<IEnumManager>();
            var storedProcedureManager = kernel.Get<IStoredProcedureManager>();
            var logger = new DynamicLogger(@"C:\Neutron\Logs\", "Startup.log", "true");

            var neutronVariables = jsonData.LoadFile<NeutronVariables>();

            var cultureInfo = neutronVariables.DefaultLanguage;

            if (cultureInfo == null || cultureInfo.Length != 5 || !cultureInfo.Contains('-'))
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cultureInfo);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo);
            }

            if (context)
            {
                var frmMain = kernel.Get<FrmMain>();
                Application.Run(frmMain);
            }
            else
            {
                var frmSystem = new FrmSystem(jsonData, logger, null, null, storedProcedureManager, true);
                Application.Run(frmSystem);
            }



            //Application.Run(new FrmMain(jsonData, akaRepository, securityProcessor
            //    , lacProcessor, imageManager, stationRepository
            //    , ordersRepository, replenOrdersRepository));

        }


    }
}
