using NeutronData;
using Neutron.Forms;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.DataContexts;
using Ninject;
using System.Reflection;
using JsonManager;
using Neutron.Interfaces;
using System.Threading;
using NeutronCore.Models;
using NeutronData.Interfaces;
using Neutron.Models;
using System.Globalization;
using System.Resources;
using EnumsNET;
using NeutronCore;

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
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

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
            var jsonData = kernel.Get<IJsonData>();
            jsonData.RootDirectory = $"{Properties.Settings.Default.RootDirectory}";
            LoaderSettings.SetRootDirectory($"{Properties.Settings.Default.RootDirectory}");
            var akaRepository = kernel.Get<IAkaRepository>();
            var securityProcessor = kernel.Get<ISecurityProcessor>();
            var lacProcessor = kernel.Get<ILacProcessor>();


            var stationRepository = kernel.Get<IStationRepository>();

            INomenclature nomenclature = jsonData.LoadFile<Nomenclature>();

            Application.Run(new FrmMain(jsonData, akaRepository, securityProcessor, lacProcessor, nomenclature, stationRepository));

        }
    }
}
