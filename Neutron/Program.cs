using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JsonManager;
using Neutron.Interfaces;
using System.Threading;
using NeutronCore.Models;
using System.Globalization;
using AlliedPostOffice;
using AlliedPostOffice.Concrete;
using Neutron.Forms;
using Neutron.Ninject;
using NeutronCore;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.General;

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

            _mutex = new Mutex(initiallyOwned: true, name: appName, createdNew: out var createdNew);
            if (!createdNew)
            {
                //app is already running!  Exiting the application
                MessageBox.Show("Neutron application is already running.", "Neutron Startup", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(defaultValue: false);

            DI.Initialize();
            var rootDirectory = DI.Create<INeutronRootDirectory>().RootDirectory;

            var jsonData = DI.Create<IJsonData>();
            jsonData.RootDirectory = rootDirectory;
            LoaderSettings.SetRootDirectory(rootDirectory);

            // Check the Database connections
            var context = false;
            var neutron = new NeutronDb().CheckConnection();
            var secure = new SecureDb().CheckConnection();
            if (neutron && secure)
            {
                context = true;
            }

            var neutronVariables = jsonData.LoadFile<NeutronVariables>();
            var neutronLicense = jsonData.LoadFile<NeutronLicense>();

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
                var frmMain = DI.Create<FrmMain>(neutronVariables, neutronLicense);
                Application.Run(frmMain);
            }
            else
            {
                var frmSystem = DI.Create<FrmSystem>(true);
                Application.Run(frmSystem);
            }
        }
    }
}
