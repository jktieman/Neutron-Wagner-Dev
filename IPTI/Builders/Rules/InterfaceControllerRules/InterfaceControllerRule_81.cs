using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders
{
    public class InterfaceControllerRule_81 : IInterfaceControllerRule
    {
        public bool IsMatch(string code)
        {
            return code == "81";
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
            sb.AppendLine($"Enable/Disable Bay Controller Polling");
            sb.AppendLine($"");
            sb.AppendLine($"Action Code: 81");
            sb.AppendLine($"");
            sb.AppendLine($"Function: Used to set the flag for whether the interface controller is continuously");
            sb.AppendLine($"polling the bay controllers in the background. Polling must be enabled in order for the");
            sb.AppendLine($"03 (Get Status of Bay Controllers) command to return valid data.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"");
            sb.AppendLine($"Interface Controller Version: 3.03 and higher");
            sb.AppendLine($"");
            sb.AppendLine($"To set the polling flag:");
            sb.AppendLine($"");
            sb.AppendLine($"Format of Packet: 81E");
            sb.AppendLine($"81 = Literal \"81\" - Action code for Enable/Disable Bay Controller Polling");
            sb.AppendLine($"E = Flag for whether background polling is enabled. 1=Enabled, 0=Disabled");
            sb.AppendLine($"");
            sb.AppendLine($"To query the setting");
            sb.AppendLine($"");
            sb.AppendLine($"Format of Packet: 81");
            sb.AppendLine($"81 = Literal \"81\" - Action code for Enable/Disable Bay Controller Polling");
            sb.AppendLine($"");
            sb.AppendLine($"Response from interface controller: 81E<ACK>");
            sb.AppendLine($"81 = Literal \"81\" - Action Code for Enable/Disable Bay Controller Polling");
            sb.AppendLine($"E = Flag for whether background polling is enabled. 1=Enabled, 0=Disabled");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");

            responseWithInfo.Information = sb.ToString();
            responseWithInfo.Response = response;
            responseWithInfo.Polling = result.Polling;
            return responseWithInfo;
        }

        private RuleResult GetResult(string request, string response)
        {

            var result = new RuleResult(request, response);
            //result.Request = request;
            //result.Response = response;
            //var cleanRequest = new string(request.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanRequest = cleanRequest.Length < 2 ? cleanRequest : cleanRequest.Substring(startIndex: 2, length: cleanRequest.Length - 4);
            //var cleanResponse = new string(response.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanResponse = cleanResponse.Length < 2 ? cleanResponse : cleanResponse.Substring(startIndex: 2, length: cleanResponse.Length - 4);
            if (result.CleanRequest == "81")
            {
                if (result.CleanResponse.Substring(2, 1) == "1")
                {
                    result.Message = "Polling is On.";
                    result.Polling = true;
                }
                if (result.CleanResponse.Substring(2, 1) == "0")
                {
                    result.Message = "Polling is Off.";
                    result.Polling = false;
                }
            }
            if (result.CleanRequest == "811")
            {
                result.Message = "Polling is On.";
                result.Polling = true;
            }
            if (result.CleanRequest == "810")
            {
                result.Message = "Polling is Off.";
                result.Polling = false;
            }

            return result;
        }
    }
}
