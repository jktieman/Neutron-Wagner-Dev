using System;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Neutron.Global;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using Ninject;
using Ninject.Parameters;
using AlliedPostOffice;
using JsonManager;
using Neutron.Forms;
using NeutronData.Interfaces;
using NeutronData.PrintModels;

namespace Neutron.Ninject
{
    public static class DI
    {
        private static StandardKernel _kernel;

        public static void Initialize()
        {
            _kernel = new StandardKernel();
            _kernel.Load(Assembly.GetExecutingAssembly());
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
            , HistoryManager historyManager)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("historyManager", historyManager));
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
            , NeutronVariables neutronVariables)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("workstationView", workstationView)
                , new ConstructorArgument("neutronVariables", neutronVariables));
        }

        public static T Create<T>(
            NeutronVariables neutronVariables
            , NeutronLicense neutronLicense)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense));
        }

        public static FrmUtilities CreateUtilitiesForm(
            NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , WorkstationView workstationView)
        {
            return _kernel.Get<FrmUtilities>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("workstationView", workstationView));
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
