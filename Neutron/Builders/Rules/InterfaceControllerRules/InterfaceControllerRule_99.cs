using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders
{
    public class InterfaceControllerRule_99 : IInterfaceControllerRule
    {

        public bool IsMatch(string code)
        {
            return code == "99";
        }

        public ResponseInfo BuildInfo(Command currentCommand, string request, string response)
        {
            RuleResult result = GetResult(request, response);

            var responseWithInfo = new ResponseInfo();
            var sb = new StringBuilder();
            sb.AppendLine($"Actual Response: {response}");
            sb.AppendLine($"Clean Response: {result.CleanResponse}");
            sb.AppendLine($"Actual Request: {request}");
            sb.AppendLine($"Clean Request {result.CleanRequest}");
            sb.AppendLine($"What does it mean?");
            sb.AppendLine(result.Message);
            sb.AppendLine($"");
            sb.AppendLine($"Interface Controller Reset");
            sb.AppendLine($"Action Code: 99");
            sb.AppendLine($"Function: Resets the interface controller to startup conditions.");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Interface Controller Version: All");
            sb.AppendLine($"");
            sb.AppendLine($"Response from interface controller: 99");
            sb.AppendLine($"99 = Literal \"99\" - Action Code for Reset");
            sb.AppendLine($"Note that the interface controller always sends 99 when it starts up,");
            sb.AppendLine($"whether because of this command or a power up.");
            responseWithInfo.Information = sb.ToString();
            responseWithInfo.Response = response;
            return responseWithInfo;
        }

        private RuleResult GetResult(string request, string response)
        {

            var result = new RuleResult(request, response);
            if (result.CleanResponse == "99")
            {
                result.Message = "Connection to Interface Controller was Successful.";
            }
            else
            {
                result.Message = "Interface Controller Not Found.";
            }

            return result;
        }

        public ResponseInfo BuildInfo(string response)
        {
            throw new NotImplementedException();
        }
    }
}
