using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders.Rules.BayControllerRules
{
    public class BayControllerRule_03 : IBayControllerRule
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
            sb.AppendLine($"");
            sb.AppendLine($"Turn All Off With Verify");
            sb.AppendLine($"Action Code: 03");
            sb.AppendLine();
            sb.AppendLine($"Function: Turns all lights in the bay on individually, and responds to the PC with any");
            sb.AppendLine($"lights that failed to respond.");
            sb.AppendLine($"");
            sb.AppendLine($"In order for this command to work properly, the settings must be correct for how many");
            sb.AppendLine($"and what type of modules the bay has. Use the 81 (Set Number Of Devices) command");
            sb.AppendLine($"to set this information.");

            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"Bay Controller Version: 4.0 or greater");
            sb.AppendLine();
            sb.AppendLine($"Format of packet: 03");
            sb.AppendLine($"03 = Literal \"03\" - Action Code for Turn All Off With Verify ");
            sb.AppendLine();
            sb.AppendLine($"Response from bay controller: 03XXYY..<ACK>");
            sb.AppendLine($"03 = Literal \"03\" - Action code for Turn All Off With Verify");
            sb.AppendLine($"X X, Y Y = Devices that did not respond. If more modules failed than what the bay");
            sb.AppendLine($"controller can fit in its buffer, as many modules will be included as can fit, followed by");
            sb.AppendLine($"two periods to indicate there are more modules that could not be listed.");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");
            sb.AppendLine($"Note: If there is overlapping in the addresses of display based devices and light based");
            sb.AppendLine($"devices (known as 4-digit modules and light pick modules), the response changes. For");
            sb.AppendLine($"example, if there are 4-digit modules 1-10 and light picks 1-10, the system cannot");
            sb.AppendLine($"simply say that device 3 failed because the PC will not know which device 3 (4-digit or");
            sb.AppendLine($"light pick) failed. So in this case the response will be:");
            sb.AppendLine($"");
            sb.AppendLine($"034DXXYY..LPxxyy<ACK>");
            sb.AppendLine($"");
            sb.AppendLine($"03 = Literal \"03\" - Action code for Turn All Off With Verify");
            sb.AppendLine($"4D = Literal \"4D\" – Specifies the start of the listing of failed 4-digit modules");
            sb.AppendLine($"XX, YY = 4-digit modules that did not come on. It is possible to have no devices listed");
            sb.AppendLine($"in this section.");
            sb.AppendLine($"LP = Literal “LP” – Specifies the start of the listing of failed light pick modules");
            sb.AppendLine($"xx,yy = Light pick modules that did not come on. It is possible to have no devices");
            sb.AppendLine($"listed in this section.");
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