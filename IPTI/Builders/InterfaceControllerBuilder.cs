using System.Collections.Generic;
using System.Linq;
using IPTI.Builders.Interfaces;
using IPTI.Builders.Rules.InterfaceControllerRules;
using NeutronEvents;

namespace IPTI.Builders
{
    public class InterfaceControllerBuilder : IInterfaceControllerBuilder
    {
        private readonly List<IInterfaceControllerRule> _interfaceControllerRules;

        public InterfaceControllerBuilder()
        {
            _interfaceControllerRules = new List<IInterfaceControllerRule>
            {
                new InterfaceControllerRule_01(),
                //new InterfaceControllerRule_02(),
                //new InterfaceControllerRule_03(),
                //new InterfaceControllerRule_81(),
                //new InterfaceControllerRule_97(),
                //new InterfaceControllerRule_99(),
                //new InterfaceControllerRule_Unknown()
            };
        }

        public void BuildInterfaceController(string response, ResponseInfo responseInfo)
        {
             _interfaceControllerRules.First(c => c.IsMatch(response.Substring(3, 2), responseInfo))
                .BuildInfo(response, responseInfo);
        }
    }
}