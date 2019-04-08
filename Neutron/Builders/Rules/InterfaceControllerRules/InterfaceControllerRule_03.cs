using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders
{
    public class InterfaceControllerRule_03 : IInterfaceControllerRule
    {

        public bool IsMatch(string code)
        {
            return code == "03";
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
            sb.AppendLine($"Get Status of Bay Controllers");
            sb.AppendLine();
            sb.AppendLine($"Action Code: 03");
            sb.AppendLine();
            sb.AppendLine($"Function: Used to check the status of all bay controllers attached to the interface");
            sb.AppendLine($"controller. The interface controller will respond with all bay IDs that are not connected,");
            sb.AppendLine($"so the 02 (Set Number Of Bays) command should be sent prior to using this command.");
            sb.AppendLine($"Further, the interface controller responds based on the results of the polling done in the");
            sb.AppendLine($"background, so polling must be turned on (See the Enable/Disable Bay Controller");
            sb.AppendLine($"Polling command), or this command will always return all bay controllers are connected.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine();
            sb.AppendLine($"Interface Controller Version: All");
            sb.AppendLine();
            sb.AppendLine($"Format of Packet: 03");
            sb.AppendLine($"03 = Literal \"03\" - Action code for Get Status of Bay Controllers");
            sb.AppendLine();
            sb.AppendLine($"Response from interface controller: 03XXYY..<ACK>");
            sb.AppendLine($"03 = Literal \"03\" - Action Code for Get Status of Bay Controllers");
            sb.AppendLine($"XX, YY, .. = Bay controllers that did not respond. ");
            sb.AppendLine($"If all bay controllers responded, the packet will be simply 03");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");

            responseWithInfo.Information = sb.ToString();
            responseWithInfo.Response = response;
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
            if (result.CleanResponse == "03")
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
