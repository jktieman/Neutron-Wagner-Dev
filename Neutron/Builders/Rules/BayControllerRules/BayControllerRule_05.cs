using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders.Rules.BayControllerRules
{
    public class BayControllerRule_05 : IBayControllerRule
    {

        public bool IsMatch(string code)
        {
            return code == "05";
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
            sb.AppendLine($"Turn Off Single Device");
            sb.AppendLine($"Action Code: 05");
            sb.AppendLine();
            sb.AppendLine($"Function: Used along with the 04(Turn On Single Module) command to check whether");
            sb.AppendLine($"a device attached to the bay controller is functioning properly.This is used as a");
            sb.AppendLine($"diagnostics tool, and should not be used for actual picking, since it only turns off the");
            sb.AppendLine($"light but does not remove it from the internal poll list used when a pick is displayed.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Bay Controller Version: 4.0 or greater");
            sb.AppendLine();
            sb.AppendLine($"Format of packet: 05DD");
            sb.AppendLine($"05 = Literal \"05\" - Action Code for the Turn Off Single Module command.");
            sb.AppendLine($"D D = Device Number to turn on");
            sb.AppendLine($"");
            sb.AppendLine($"Response from bay controller:");
            sb.AppendLine($"If pick module responded correctly: 05<ACK>");
            sb.AppendLine($"05 = Literal \"05\" - Action code for the Turn Off Single Module command");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");
            sb.AppendLine($"If pick module failed to respond: 05DD <ACK>");
            sb.AppendLine($"05 = Literal \"04\" - Action code for the Turn Off Single Module command");
            sb.AppendLine($"DD = Device Number");
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
            if (result.CleanResponse == "05")
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
