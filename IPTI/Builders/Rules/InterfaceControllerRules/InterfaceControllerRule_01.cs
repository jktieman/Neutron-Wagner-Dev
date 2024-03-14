using System;
using System.Text;
using IPTI.Builders.Interfaces;
using IPTI.Models;
using NeutronEvents;

namespace IPTI.Builders.Rules.InterfaceControllerRules
{
    public class InterfaceControllerRule_01 : IInterfaceControllerRule
    {

        public bool IsMatch(string code, ResponseInfo responseInfo)
        {
            if (code != "01") return false;
            responseInfo.Command = "01";
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {

            RuleResult result = GetResult(response);

            var sb = new StringBuilder();
            sb.AppendLine($"Actual Response: {response}");
            sb.AppendLine($"Clean Response: {result.CleanResponse}");
            sb.AppendLine();
            sb.AppendLine($"What does it mean?");
            sb.AppendLine(result.Message);
            sb.AppendLine($"");
            sb.AppendLine($"Interface Controller Heartbeat");
            sb.AppendLine($"");
            sb.AppendLine($"Action Code: 01");
            sb.AppendLine($"");
            sb.AppendLine($"Function: Used to check communication to the interface controller.");
            sb.AppendLine($"");
            sb.AppendLine($"Initiated By: PC");
            sb.AppendLine($"");
            sb.AppendLine($"Interface Controller Version: All");
            sb.AppendLine($"");
            sb.AppendLine($"Format of Packet: 01");
            sb.AppendLine($"01 = Literal \"01\" - Action code for the heartbeat");
            sb.AppendLine($"");
            sb.AppendLine($"Response from interface controller: 01<ACK>");
            sb.AppendLine($"01 = Literal \"01\" - Action Code for the Heartbeat");
            sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            sb.AppendLine($"");

            responseInfo.Success = result.Success;
            responseInfo.Information = sb.ToString();
            responseInfo.Response = response;
        }

        private RuleResult GetResult(string response)
        {
            var result = new RuleResult(response);
            if (response.Length == 9)
            {
                //var cleanResponse = new string(response.Where(c => !char.IsControl(c)).ToArray());
                result.CleanResponse = response.Substring(startIndex: 1, length: response.Length - 3);
                if (Convert.ToChar(response.Substring(5, 1)) == Global.ACK)
                {
                    result.Success = true;
                    result.ReturnValue = "SUCCESS";
                }
            }
            else
            {
                result.Message = "Interface Controller Not Found.";
            }
            return result;
        }
    }
}
