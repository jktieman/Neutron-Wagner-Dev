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
            , StationView stationView
            , HistoryManager historyManager)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("stationView", stationView)
                , new ConstructorArgument("historyManager", historyManager));
        }

        //// FrmHotAction
        //public static T Create<T>(
        //    NeutronVariables neutronVariables
        //    , NeutronLicense neutronLicense
        //    , StationView stationView
        //    , HistoryManager historyManager)
        //    //, string item)
        //    //, int quantity)
        //    //, PickList pickList)
        //{

        //    var result = _kernel.Get<T>(
        //        new ConstructorArgument("neutronVariables", neutronVariables)
        //        , new ConstructorArgument("neutronLicense", neutronLicense)
        //        , new ConstructorArgument("stationView", stationView)
        //        , new ConstructorArgument("historyManager", historyManager));
        //        //, new ConstructorArgument("item", item));
        //        //, new ConstructorArgument("quantity", quantity));
        //        //, new ConstructorArgument("pickList", pickList));

        //        return result;
        //}

        public static T Create<T>(
            NeutronVariables neutronVariables
            , NeutronLicense neutronLicense
            , Station rackStation)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("neutronVariables", neutronVariables)
                , new ConstructorArgument("neutronLicense", neutronLicense)
                , new ConstructorArgument("rackStation", rackStation));
        }

        public static T Create<T>(
            bool standAlone)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("standAlone", standAlone));
        }

        public static T Create<T>(
            StationView stationView
            , NeutronVariables neutronVariables)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("stationView", stationView)
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

        public static T Create<T>(
            StationView stationView)
        {
            return _kernel.Get<T>(
                new ConstructorArgument("stationView", stationView));
        }

        public static T Create<T>(int value)
        {
            return _kernel.Get<T>(new ConstructorArgument("stationId", value));
        }
    }
}
