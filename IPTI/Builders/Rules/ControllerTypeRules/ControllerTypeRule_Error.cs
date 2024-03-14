using IPTI.Builders.Interfaces;
using NeutronEvents;

namespace IPTI.Builders.Rules.ControllerTypeRules
{
    public class ControllerTypeRule_Error : IControllerTypeRule
    {
        public bool IsMatch(string controllerType, ResponseInfo responseInfo)
        {
            responseInfo.ControllerNumber = "ER";
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            responseInfo.Response = response;
            responseInfo.Information = "Controller Type Error.";
        }
    }
}
