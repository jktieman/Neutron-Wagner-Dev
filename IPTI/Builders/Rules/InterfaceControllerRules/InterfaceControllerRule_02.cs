using IPTI_Tester.Builders.Interfaces;
using IPTI_Tester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTI_Tester.Builders
{
    public class InterfaceControllerRule_02 : IInterfaceControllerRule
    {

        public bool IsMatch(string code)
        {
            return code == "02";
        }

        public bool IsMatch(string code, ResponseInfo responseInfo)
        {
            if (code == "02")
            {
                responseInfo.Command = "02";
                return true;
            }
            return false;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            RuleResult result = GetResult(request, response);

            var sb = new StringBuilder();
            sb.AppendLine($"Actual Response: {response}");
            sb.AppendLine($"Clean Response: {result.CleanResponse}");
            sb.AppendLine($"What does it mean?");
            sb.AppendLine(result.Message);
            sb.AppendLine();
            sb.AppendLine($"Set Number Of Bays");
            sb.AppendLine();
            sb.AppendLine($"Action Code: 02");
            sb.AppendLine();
            sb.AppendLine($"Function: Used to tell the interface controller how many bay controllers are connected");
            sb.AppendLine($"to it. The interface controller will not accept any packets from bay controllers beyond");
            sb.AppendLine($"this number. If polling is enabled, it will only poll bay controllers up to this number. This");
            sb.AppendLine($"setting is stored in non-volatile memory, which means it is not lost when the system is");
            sb.AppendLine($"powered down. Therefore this command only needs to be sent when an interface");
            sb.AppendLine($"controller is installed, or if the number of bays connected to it changes.");
            sb.AppendLine($"");
            sb.AppendLine($"In version 3.03 or higher, this command can also be used as a query to return the");
            sb.AppendLine($"current setting for number of bays.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine();
            sb.AppendLine($"Interface Controller Version: All");
            sb.AppendLine($"");
            sb.AppendLine($"To set the number of bays");
            sb.AppendLine($"");
            sb.AppendLine($"Format of Packet: 02NNSS");
            sb.AppendLine($"02 = Literal \"02\" - Action code for Set Number of Bays");
            sb.AppendLine($"N N = Number of bay controllers attached to this interface controller");
            sb.AppendLine($"S S = Starting bay controller number. So if the system consists of 5 bays numbered 11");
            sb.AppendLine($"through 15, NN=”05” and SS=”11”");
            sb.AppendLine($"");
            sb.AppendLine($"Response from interface controller: 02<ACK>");
            sb.AppendLine($"02 = Literal \"02\" - Action Code for Set Number of Bays");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");
            sb.AppendLine($"To query the number of bays (version 3.03 and higher only):");
            sb.AppendLine($"");
            sb.AppendLine($"Format of Packet: 02");
            sb.AppendLine($"02 = Literal \"02\" - Action code for Set Number of Bays");
            sb.AppendLine($"");
            sb.AppendLine($"Response from interface controller: 02NNSS<ACK>");
            sb.AppendLine($"02 = Literal \"02\" - Action code for Set Number of Bays");
            sb.AppendLine($"NN = Number of bay controllers attached to this interface controller");
            sb.AppendLine($"S S = Starting bay controller number. So if the system consists of 5 bays numbered 11");
            sb.AppendLine($"through 15, NN=”05” and SS=”11”");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");

            responseInfo.Information = sb.ToString();
            responseInfo.Response = response;
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
            var sb = new StringBuilder();
            //Get data
            if (result.CleanResponse.Length == 6)
            {
                sb.AppendLine($"Number of Bay Controllers Attached to this Interface Controller: {result.CleanResponse.Substring(2,2)}");
                sb.AppendLine($"Starting Bay Controller Number: {result.CleanResponse.Substring(4, 2)}");
            }
            //Set Data
            else if (result.CleanResponse.Length == 2)
            {
                sb.AppendLine($"Number of Bay Controllers Attached to this Interface Controller Set To: {result.CleanRequest.Substring(2, 2)}");
                sb.AppendLine($"Starting Bay Controller Number Set To: {result.CleanRequest.Substring(4, 2)}");
            }

            result.Message = sb.ToString();
            return result;
        }
    }
}
