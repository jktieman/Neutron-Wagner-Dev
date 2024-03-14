using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders
{
    public class InterfaceControllerRule_97 : IInterfaceControllerRule
    {

        public bool IsMatch(string code)
        {
            return code == "97";
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
            sb.AppendLine();
            sb.AppendLine("Get Interface Controller Version");
            sb.AppendLine("Action Code: 97");
            sb.AppendLine("Function: Returns the version of the firmware running on the interface controller.");
            sb.AppendLine("Initiated By: PC");
            sb.AppendLine("Interface Controller Version: 3.03 or higher");
            sb.AppendLine();
            sb.AppendLine("Response from interface controller: 97VV...V <ACK>");
            sb.AppendLine("97 = Literal \"97\" - Action Code for Get Interface Controller Version");
            sb.AppendLine("VV ...V = Version number on the interface controller firmware");
            sb.AppendLine("<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine();

            responseWithInfo.Information = sb.ToString();
            responseWithInfo.Response = response;
            return responseWithInfo;
        }

        private RuleResult GetResult(string request, string response)
        {

            var result = new RuleResult(request, response);
            if (result.CleanRequest == "97")
            {
                result.Message = ($"Interface Controller Version: {result.ReturnValue} ");
            }
            else
            {
                result.Message = "Error.";
            }

            return result;
        }

        public ResponseInfo BuildInfo(string response)
        {
            throw new NotImplementedException();
        }
    }
}
