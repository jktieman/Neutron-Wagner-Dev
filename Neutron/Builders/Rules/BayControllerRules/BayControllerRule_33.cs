using Neutron.Builders.Interfaces;
using NeutronCore.Extensions;
using Neutron.Models;
using NeutronEvents;

namespace Neutron.Builders.Rules.BayControllerRules
{
    public class BayControllerRule_33 : IBayControllerRule
    {
        public bool IsMatch(string code, ResponseInfo responseInfo)
        {
            if (code != "33") return false;
            responseInfo.Command = "33";
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            // RuleResult result = GetResult(response);


            responseInfo.Response = response;

            if (response.Length == 12)
                if (response.Substring(7, 1) == "1")
                {
                    responseInfo.Success = true;
                    responseInfo.ReturnValue = "SUCCESS";
                }

            if (response.Length == 18)
            {
                var cmd = $"{responseInfo.ControllerNumber}{responseInfo.Command}{responseInfo.DisplayNumber}{Models.Global.ACK}";
                responseInfo.RespondCommand = (Models.Global.SOH + cmd + cmd.GetCheckDigit() + Models.Global.ETX);
                int qty;
                responseInfo.DisplayNumber = response.Substring(5, 2);
                var b = int.TryParse(response.Substring(7, 4), out qty);
                if (b)
                {
                    responseInfo.Quantity = qty;
                }
            }









            // var sb = new StringBuilder();
            // sb.AppendLine($"Actual Response: {response}");
            //// sb.AppendLine($"Clean Response: {result.CleanResponse}");
            // sb.AppendLine($"What does it mean?");
            //// sb.AppendLine(result.Message);
            // sb.AppendLine($"");
            // sb.AppendLine($"Display Based Order Detail Line");
            // sb.AppendLine($"Action Code: 33");
            // sb.AppendLine();



            //sb.AppendLine($"Function: Used to check the OC modules attached to the bay controller. The bay");
            //sb.AppendLine($"functioning properly. This command is similar to the Turn All Off With Verify command,");
            //sb.AppendLine($"controller will then loop from 1 to the maximum number of OCs (15) and check to see if");
            //sb.AppendLine($"that OC exists in the bay. The bay controller will then return a list of all OCs it found.");
            //sb.AppendLine($"Also, the response described below will be initiated by the bay controller whenever the");
            //sb.AppendLine($"status of the main control OC changes. When this happens, the format of the packet");
            //sb.AppendLine($"will match the response shown below, but without the ACK character, and the PC");
            //sb.AppendLine($"should acknowledge this packet to prevent it from being resent.");
            //sb.AppendLine($"");
            //sb.AppendLine($"Initiated By: PC or bay controller (See above)");
            //sb.AppendLine($"");
            //sb.AppendLine($"Bay Controller Version: 7.0 or greater");
            //sb.AppendLine($"");
            //sb.AppendLine($"Format of packet: 30");
            //sb.AppendLine($"30 = Literal \"30\" - Action code for the Get Valid OCs command");
            //sb.AppendLine($"");
            //sb.AppendLine($"Response from bay controller: 30SXXYY..<ACK>");
            //sb.AppendLine($"30 = Literal \"30\" - Action code for the Get Valid OCs command");
            //sb.AppendLine($"S = Status of the main control OC - 1=Present, 0=Absent or not responding");
            //sb.AppendLine($"X X, Y Y, etc. = IDs of OCs found in the bay");
            //sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06. If the status of the main control OC");
            //sb.AppendLine($"changes, this packet will be initiated by the bay controller, in which case this");
            //sb.AppendLine($"acknowledgement character will not be included.");


            //sb.AppendLine($"");

            //responseInfo.Information = sb.ToString();

        }

        private RuleResult GetResult(string response)
        {

            var result = new RuleResult(response);
            result.ResponseLength = response.Length;
            //result.Request = request;
            //result.Response = response;
            //var cleanRequest = new string(request.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanRequest = cleanRequest.Length < 2 ? cleanRequest : cleanRequest.Substring(startIndex: 2, length: cleanRequest.Length - 4);
            //var cleanResponse = new string(response.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanResponse = cleanResponse.Length < 2 ? cleanResponse : cleanResponse.Substring(startIndex: 2, length: cleanResponse.Length - 4);
            //if (result.ReturnValue.Length >= 3)
            //{
            //    string s = result.ReturnValue.Substring(0, 1) == "1" ? "Present" : "Absent or Not Responding";
            //    result.Message = ($"Status of the main control OC: {s}{Environment.NewLine} OC modules: {result.ReturnValue.Substring(1)}");
            //}
            //else
            //{
            //    result.Message = "NOT GET VALID OCS";
            //}
            if (result.ResponseLength == 12)
                if (response.Substring(7, 1) == "1")
                {
                    result.Success = true;
                    result.ReturnValue = "SUCCESS";
                }

            if (result.ResponseLength == 18)
            {
                var cmd = "123304" + Models.Global.ACK;
                result.RespondCommand = (Models.Global.SOH + cmd + cmd.GetCheckDigit() + Models.Global.ETX);
                var bArray = result.RespondCommand.StringToByteArray();
            }

            //if (Convert.ToChar(response.Substring(7, 1)) == 1)
            //    {
            //        result.Success = true;
            //        result.ReturnValue = "SUCCESS";
            //    }
            return result;
        }
    }
}
