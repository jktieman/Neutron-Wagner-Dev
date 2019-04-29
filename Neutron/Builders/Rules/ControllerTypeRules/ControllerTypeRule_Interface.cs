using Neutron.Builders.Interfaces;
using NeutronEvents;

namespace Neutron.Builders.Rules.ControllerTypeRules
{
    public class ControllerTypeRule_Interface : IControllerTypeRule
    {

        public bool IsMatch(string controllerType, ResponseInfo responseInfo)
        {
            if (controllerType != "EF") return false;
            responseInfo.ControllerNumber = "EF";
            responseInfo.IsInterfaceController = true;
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            var builder = new InterfaceControllerBuilder();
            builder.BuildInterfaceController(response, responseInfo);
        }
    }
}
