using System;
using System.Reflection;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.ModelViews;
using Ninject;
using Ninject.Parameters;
using Neutron.Forms;
using IPTI.Models;
using NeutronData.DataContexts;


namespace Neutron.Ninject
{
    // ReSharper disable once InconsistentNaming
    public static class DI
    {
        private static StandardKernel _kernel;
        private static readonly object _lock = new object();
        public static void Initialize()
        {
            lock (_lock)
            {
                try
                {
                    if (_kernel != null)
                    {
                        _kernel.Dispose();
                    }
                    _kernel = new StandardKernel();
                    _kernel.Load(Assembly.GetExecutingAssembly());
                   
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it appropriately
                    Console.WriteLine($"Error initializing DI kernel: {ex.Message}");
                    throw;
                }
            }
        }

        public static T Create<T>()
        {
            try
            {
                return _kernel.Get<T>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message} {Environment.NewLine} {ex.InnerException?.Message}");
                throw;
            }
        }

        public static T Create<T>(params IParameter[] parameters)
        {
            return _kernel.Get<T>(parameters);
        }
        // FrmPick and FrmHotAction
        public static T Create<T>(NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , WorkstationView workstationView
            , IHistoryManager historyManager)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("historyManager", historyManager));
        }

        // FrmPick and FrmHotAction with Injected Display Driver
        public static T Create<T>(NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , WorkstationView workstationView
            , IHistoryManager historyManager
            , IptiDisplayFunctions iptiDisplayFunctions)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("historyManager", historyManager)
                , new ConstructorArgument("iptiDisplayFunctions", iptiDisplayFunctions));
        }

        //public static T Create<T>(IJsonData jsonData, NeutronVariables neutronVariables
        //    , NeutronLicense neutronLicense
        //    , WorkstationView workstationView, IWorkstationRepository workstationRepository)
        //{
        //    return _kernel.Get<T>(
        //        new ConstructorArgument("jsonData", jsonData)
        //        ,new ConstructorArgument("neutronVariables", neutronVariables)
        //        , new ConstructorArgument("neutronLicense", neutronLicense)
        //        , new ConstructorArgument("workstationView",workstationView));
        //}

        public static T Create<T>(NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , WorkstationView workstationView)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView));
        }

        public static T Create<T>(NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , bool standAlone)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("standAlone", standAlone));
        }

        public static T Create<T>(
            WorkstationView workstationView
            , NeutronVariables neutronVariables
            , IIptiDisplayFunctions iptiDisplayFunctions)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("iptiDisplayFunctions", iptiDisplayFunctions)
                );
        }

        public static T Create<T>(
            WorkstationView workstationView
            , NeutronVariables neutronVariables
            , IptiDisplayFunctions iptiDisplayFunctions)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("iptiDisplayFunctions", iptiDisplayFunctions));
        }

        public static T Create<T>(
            NeutronVariables neutronVariables
            , NeutronLicense neutronLicense)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense));
        }

        public static FrmUtilities CreateUtilitiesForm(NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , WorkstationView workstationView
            , IptiDisplayFunctions iptiDisplayFunctions)
        {
            return _kernel.Get<FrmUtilities>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("iptiDisplayFunctions",iptiDisplayFunctions));
        }


        public static T Create<T>(
            WorkstationView workstationView)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("workstationView", workstationView));
        }

        public static T Create<T>(int value)
        {
            return _kernel.Get<T>(new ConstructorArgument("workstationId", value));
        }
    }
}
