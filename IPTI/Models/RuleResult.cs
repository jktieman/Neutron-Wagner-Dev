using System.Linq;

namespace IPTI.Models
{
    public class RuleResult
    {

        public string Request { get; set; }
        public string Response { get; set; }
        public string Message { get; set; }
        public string CleanResponse { get; set; }
        public string CleanRequest { get; set; }
        public string RespondCommand { get; set; }
        public bool Polling { get; set; }
        public bool Success { get; set; }
        public string ReturnValue { get; set; }
        public int ResponseLength { get; set; }

        public RuleResult(string request, string response)
        {
            Request = request;
            Response = response;
            var cleanRequest = new string(request.Where(c => !char.IsControl(c)).ToArray());
            CleanRequest = cleanRequest.Length < 2 ? cleanRequest : cleanRequest.Substring(startIndex: 2, length: cleanRequest.Length - 4);

            var cleanResponse = new string(response.Where(c => !char.IsControl(c)).ToArray());
            CleanResponse = cleanResponse.Length < 2 ? cleanResponse : cleanResponse.Substring(startIndex: 2, length: cleanResponse.Length - 4);

            ReturnValue = CleanResponse.Length > 2 ? CleanResponse.Substring(2) : "";
        }

        public RuleResult(string response)
        {
            Success = false;
            ReturnValue = "FAIL";
        }
    }
}
