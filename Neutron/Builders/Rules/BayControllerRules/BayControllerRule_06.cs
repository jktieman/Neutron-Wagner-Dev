using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders.Rules.BayControllerRules
{ 
    public class BayControllerRule_06 : IBayControllerRule
    {
        public bool IsMatch(string code)
        {
            return code == "06";
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
            sb.AppendLine($"Turn All Lights On - Broadcast");
            sb.AppendLine($"Action Code: 06");
            sb.AppendLine();
            sb.AppendLine($"Function: Used to check whether the lights attached to the bay controller are");
            sb.AppendLine($"functioning properly. This command is similar to the Turn All On With Verify command,");
            sb.AppendLine($"except that it sends one global command to all the modules instead of checking each");
            sb.AppendLine($"one individually, and therefore does not return any information back to the PC.");
            sb.AppendLine($"");
            sb.AppendLine($"The bay controller must be correctly set up with the type of modules before sending this");
            sb.AppendLine($"command.  If the controller actually has 4-digit modules but is configured for light picks,");
            sb.AppendLine($"this command will cause a broadcast to all light picks and the 4-digits will not go on.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Bay Controller Version: 4.0 or greater");
            sb.AppendLine();
            sb.AppendLine($"Format of packet: 06");
            sb.AppendLine($"06 = Literal \"06\" - Action code for the Turn All Lights On Broadcast command");
            sb.AppendLine();
            sb.AppendLine($"Response from bay controller: 06<ACK>");
            sb.AppendLine($"06 = Literal \"06\" - Action code for the Turn All Lights On Broadcast command");
            sb.AppendLine($"");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine();

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
            if (result.CleanResponse == "06")
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
