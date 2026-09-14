// Neutron\Forms\FrmPickBase.cs
using AlliedLogger;
using JsonManager;
using MetroFramework.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;

namespace Neutron.Forms
{

    public interface IFrmPickBase
    {
        IJsonData JsonData { get; }
    }

    public class FrmPickBase : IFrmPickBase
    {
        public readonly IJsonData JsonData;

        protected FrmPickBase(IJsonData jsonData)
        {
            JsonData = jsonData;
        }

        IJsonData IFrmPickBase.JsonData => JsonData;
    }
}