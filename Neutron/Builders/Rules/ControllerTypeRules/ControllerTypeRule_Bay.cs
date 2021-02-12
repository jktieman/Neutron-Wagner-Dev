using System.Linq;
using Neutron.Builders.Interfaces;
using NeutronEvents;

namespace Neutron.Builders.Rules.ControllerTypeRules
{
    public class ControllerTypeRule_Bay : IControllerTypeRule
    {
        public bool IsMatch(string controllerType, ResponseInfo responseInfo)
        {
            var controllerIds = new string[100];
            for (var i = 0; i < 100; i++)
            {
                controllerIds[i] = i.ToString().PadLeft(2, '0');
            }

            if (!controllerIds.Contains(controllerType)) return false;
            responseInfo.ControllerNumber = controllerType;
            responseInfo.IsBayController = true;
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            var builder = new BayControllerBuilder();
            builder.BuildBayController(response, responseInfo);
        }
    }
}
