using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;

namespace IPTI_Tester.Builders.Rules.BayControllerRules
{
    class BayControllerRule_Unknown : IBayControllerRule
    {
        public bool IsMatch(string code)
        {
            return true;
        }

        public ResponseInfo BuildInfo(string response)
        {
            RuleResult result = GetResult(response);

            var responseWithInfo = new ResponseInfo();
            var sb = new StringBuilder();
            sb.AppendLine($"Actual Response: {response}");
            sb.AppendLine($"Clean Response: {result.CleanResponse}");
            sb.AppendLine($"What does it mean?");
            sb.AppendLine(result.Message);
            sb.AppendLine($"");
            responseWithInfo.Information = sb.ToString();
            responseWithInfo.Response = response;
            return responseWithInfo;
        }

        private RuleResult GetResult(string response)
        {
            var result = new RuleResult(response);
            result.Response = response;
            result.CleanResponse = "";
            result.Message = $"Unknown Bay Controller Rule Error.  {response}";

            return result;
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
            sb.AppendLine($"Bay Controller Heartbeat");
            sb.AppendLine($"Action Code: 01");
            sb.AppendLine();
            sb.AppendLine($"Function: Used to check communication to the bay controller.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Bay Controller Version: All");
            sb.AppendLine();
            sb.AppendLine($"Format of packet: 01");
            sb.AppendLine($"01 = Literal \"01\" - Action Code for the heartbeat");
            sb.AppendLine();
            sb.AppendLine($"Response from bay controller: 01<ACK>");
            sb.AppendLine($"01 = Literal \"01\" - Action code for the Heartbeat");
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
            if (result.CleanResponse == "01")
            {
                result.Message = "Connection to Interface Controller was Successful.";
            }
            else
            {
                result.Message = "Interface Controller Not Found.";
            }

            return result;
        }

    }
}
