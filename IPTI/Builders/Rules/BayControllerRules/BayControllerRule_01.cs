using IPTI.Builders.Interfaces;
using IPTI.Models;
using NeutronCore.Extensions;
using NeutronEvents;

namespace IPTI.Builders.Rules.BayControllerRules
{
    public class BayControllerRule_01 : IBayControllerRule
    {
        public bool IsMatch(string code, ResponseInfo responseInfo)
        {
            if (code != "01") return false;
            responseInfo.Command = "01";
            return true;
        }

        public void BuildInfo(string response, ResponseInfo responseInfo)
        {
            responseInfo.Response = response;

            if (response.Length == 12)
                if (response.Substring(7, 1) == "1")
                {
                    responseInfo.Success = true;
                    responseInfo.ReturnValue = "SUCCESS";
                }

            if (response.Length == 18)
            {
                var cmd =
                    $"{responseInfo.ControllerNumber}{responseInfo.Command}{responseInfo.DisplayNumber}{Global.ACK}";
                responseInfo.RespondCommand = Global.SOH + cmd + cmd.GetCheckDigit() +
                                              Global.ETX;
                int qty;
                responseInfo.DisplayNumber = response.Substring(5, 2);
                var b = int.TryParse(response.Substring(7, 4), out qty);
                if (b) responseInfo.Quantity = qty;
            }


            //RuleResult result = GetResult(request, response);

            //var responseWithInfo = new ResponseInfo();
            //var sb = new StringBuilder();
            //sb.AppendLine($"Actual Response: {response}");
            //sb.AppendLine($"Clean Response: {result.CleanResponse}");
            //sb.AppendLine($"Actual Request: {request}");
            //sb.AppendLine($"Clean Request {result.CleanRequest}");
            //sb.AppendLine($"What does it mean?");
            //sb.AppendLine(result.Message);
            //sb.AppendLine($"");
            //sb.AppendLine($"Bay Controller Heartbeat");
            //sb.AppendLine($"Action Code: 01");
            //sb.AppendLine();
            //sb.AppendLine($"Function: Used to check communication to the bay controller.");
            //sb.AppendLine($"");
            //sb.AppendLine($"Initiated By: PC");
            //sb.AppendLine($"Bay Controller Version: All");
            //sb.AppendLine();
            //sb.AppendLine($"Format of packet: 01");
            //sb.AppendLine($"01 = Literal \"01\" - Action Code for the heartbeat");
            //sb.AppendLine();
            //sb.AppendLine($"Response from bay controller: 01<ACK>");
            //sb.AppendLine($"01 = Literal \"01\" - Action code for the Heartbeat");
            //sb.AppendLine($"");
            //sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
            //sb.AppendLine();

            //responseWithInfo.Information = sb.ToString();
            //responseWithInfo.Response = response;
            //return responseWithInfo;
        }

        //public ResponseInfo BuildInfo(string response)
        //{
        //    RuleResult result = GetResult(response);

        //    var responseWithInfo = new ResponseInfo();
        //    var sb = new StringBuilder();
        //    sb.AppendLine($"Actual Response: {response}");
        //    sb.AppendLine($"Clean Response: {result.CleanResponse}");
        //    sb.AppendLine($"Clean Request {result.CleanRequest}");
        //    sb.AppendLine($"What does it mean?");
        //    sb.AppendLine(result.Message);
        //    sb.AppendLine($"");
        //    sb.AppendLine($"Bay Controller Heartbeat");
        //    sb.AppendLine($"Action Code: 01");
        //    sb.AppendLine();
        //    sb.AppendLine($"Function: Used to check communication to the bay controller.");
        //    sb.AppendLine($"");
        //    sb.AppendLine($"Initiated By: PC");
        //    sb.AppendLine($"Bay Controller Version: All");
        //    sb.AppendLine();
        //    sb.AppendLine($"Format of packet: 01");
        //    sb.AppendLine($"01 = Literal \"01\" - Action Code for the heartbeat");
        //    sb.AppendLine();
        //    sb.AppendLine($"Response from bay controller: 01<ACK>");
        //    sb.AppendLine($"01 = Literal \"01\" - Action code for the Heartbeat");
        //    sb.AppendLine($"");
        //    sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
        //    sb.AppendLine();

        //    responseWithInfo.Information = sb.ToString();
        //    responseWithInfo.Response = response;
        //    return responseWithInfo;
        //}

        //public void BuildInfo(string response, ResponseInfo responseInfo)
        //{
        //    RuleResult result = GetResult(response);

        //    var sb = new StringBuilder();
        //    sb.AppendLine($"Actual Response: {response}");
        //    sb.AppendLine($"Clean Response: {result.CleanResponse}");
        //    sb.AppendLine($"Clean Request {result.CleanRequest}");
        //    sb.AppendLine($"What does it mean?");
        //    sb.AppendLine(result.Message);
        //    sb.AppendLine($"");
        //    sb.AppendLine($"Bay Controller Heartbeat");
        //    sb.AppendLine($"Action Code: 01");
        //    sb.AppendLine();
        //    sb.AppendLine($"Function: Used to check communication to the bay controller.");
        //    sb.AppendLine($"");
        //    sb.AppendLine($"Initiated By: PC");
        //    sb.AppendLine($"Bay Controller Version: All");
        //    sb.AppendLine();
        //    sb.AppendLine($"Format of packet: 01");
        //    sb.AppendLine($"01 = Literal \"01\" - Action Code for the heartbeat");
        //    sb.AppendLine();
        //    sb.AppendLine($"Response from bay controller: 01<ACK>");
        //    sb.AppendLine($"01 = Literal \"01\" - Action code for the Heartbeat");
        //    sb.AppendLine($"");
        //    sb.AppendLine($"<ACK> = Acknowledgement character - Hex 06");
        //    sb.AppendLine();

        //    responseInfo.Information = sb.ToString();
        //    responseInfo.Response = response;

        //}

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
                result.Message = "Connection to Interface Controller was Successful.";
            else
                result.Message = "Interface Controller Not Found.";

            return result;
        }

        private RuleResult GetResult(string response)
        {
            var result = new RuleResult(response);

            //result.Request = request;
            //result.Response = response;
            //var cleanRequest = new string(request.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanRequest = cleanRequest.Length < 2 ? cleanRequest : cleanRequest.Substring(startIndex: 2, length: cleanRequest.Length - 4);
            //var cleanResponse = new string(response.Where(c => !char.IsControl(c)).ToArray());
            //result.CleanResponse = cleanResponse.Length < 2 ? cleanResponse : cleanResponse.Substring(startIndex: 2, length: cleanResponse.Length - 4);
            if (result.CleanResponse == "01")
                result.Message = "Connection to Bay Controller was Successful.";
            else
                result.Message = "Bay Controller Not Found.";

            return result;
        }
    }
}