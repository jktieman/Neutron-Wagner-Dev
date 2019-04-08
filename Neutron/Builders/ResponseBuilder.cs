using System.Collections.Generic;
using System.Linq;
using Neutron.Builders.Interfaces;
using Neutron.Builders.Rules.ControllerTypeRules;
using Neutron.Models;
using NeutronEvents;

namespace Neutron.Builders
{
    public class ResponseBuilder : IResponseBuilder
    {
        private readonly List<IControllerTypeRule> _controllerTypeRules;

        public ResponseBuilder()
        {
            _controllerTypeRules = new List<IControllerTypeRule>
            {
                new ControllerTypeRule_Interface()
                , new ControllerTypeRule_Bay()
                , new ControllerTypeRule_Error()
            };
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            _controllerTypeRules.First(c => c.IsMatch(response.Substring(1, 2), responseInfo))
                .BuildInfo(response, responseInfo);
        }
    }
}
