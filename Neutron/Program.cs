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
using System.IO;
using System.Resources;
using EnumsNET;
using NeutronCore;
using NeutronCore.Global;
using Newtonsoft.Json;

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

            var akaRepository = kernel.Get<IAkaRepository>();
            var securityProcessor = kernel.Get<ISecurityProcessor>();
            var lacProcessor = kernel.Get<ILacProcessor>();
            var imageManager = kernel.Get<IImageManager>();
            var ordersRepository = kernel.Get<IOrdersRepository>();
            var stationRepository = kernel.Get<IStationRepository>();
            var replenOrdersRepository = kernel.Get<IReplenOrdersRepository>();

            var neutronVariables = jsonData.LoadFile<NeutronVariables>();

            var cultureInfo = neutronVariables.DefaultLanguage;
            if (cultureInfo.Length == 5 && cultureInfo.Contains('-'))
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cultureInfo);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo);
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            }

            Application.Run(new FrmMain(jsonData, akaRepository, securityProcessor
                , lacProcessor, neutronVariables, imageManager, stationRepository
                , ordersRepository, replenOrdersRepository));

        }

        private static string GetRootDirectory()
        {
            var data = new NeutronRootDirectory();
            var fileName = ($"{typeof(NeutronRootDirectory).Name}.json");

            var fileInfo = new FileInfo($"Json\\{fileName}");
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                if (fileInfo.DirectoryName != null) Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            if (!fileInfo.Exists)
            {
                SaveNew(fileInfo, data);
            }

            if (fileInfo.Exists)
            {
                try
                {
                    using (TextReader reader = new StreamReader(fileInfo.FullName))
                    {
                        data = JsonConvert.DeserializeObject<NeutronRootDirectory>(reader.ReadToEnd());
                    }
                }
                catch (Exception)
                {
                    Console.Write($"Error reading from Json file.  {fileInfo.FullName}");
                }
            }
            return data.RootDirectory;
        }

        public static void SaveNew(FileInfo fileInfo, NeutronRootDirectory data)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(fileInfo.FullName, append: false))
                {
                    writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception)
            {
                Console.Write("Error writing NeutronRootDirectory to Json file.");
            }
        }
    }
}
