using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders.Rules.BayControllerRules
{
    public class BayControllerRule_04 : IBayControllerRule
    {
        public bool IsMatch(string code)
        {
            return code == "04";
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
            sb.AppendLine($"Turn On Single Device");
            sb.AppendLine($"Action Code: 04");
            sb.AppendLine();
            sb.AppendLine($"Function: Used to check whether a device attached to the bay controller is functioning");
            sb.AppendLine($"properly. This is used as a diagnostics tool, and should not be used for actual picking,");
            sb.AppendLine($"since it does not return any information when the task complete button is pressed.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Bay Controller Version: 4.0 or greater");
            sb.AppendLine();
            sb.AppendLine($"Format of packet: 04DDLTTTT");
            sb.AppendLine($"04 = Literal \"04\" - Action Code for the Turn On Single Module command.");
            sb.AppendLine($"D D = Device Number to turn on");
            sb.AppendLine($"L = LED State. In versions below 5, this was omitted and LED rate of Flash Fast was");
            sb.AppendLine($"always used.");
            sb.AppendLine($"T T T T = Text to display, if module is a four digit module. If not, or to just display");
            sb.AppendLine($"“TEST”, omit these characters.");
            sb.AppendLine();
            sb.AppendLine($"Response from bay controller: 04XXYY …<ACK>");
            sb.AppendLine($"If pick module responded correctly: 04<ACK>");
            sb.AppendLine($"04 = Literal \"04\" - Action code for the Turn On Single Module command");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");
            sb.AppendLine($"If pick module failed to respond: 04DD <ACK>");
            sb.AppendLine($"04 = Literal \"04\" - Action code for the Turn On Single Module command");
            sb.AppendLine($"D D = Device Number");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");
            
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
            if (result.CleanResponse == "04")
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
