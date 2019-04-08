namespace Neutron.Models
{
    public class ResponseInfo
    {
        public bool IsInterfaceController { get; set; }
        public bool IsBayController { get; set; }
        public string ControllerNumber{ get; set; }
        public string RespondCommand { get; set; }
        public string Command { get; set; }
        public string DisplayNumber { get; set; }
        public bool HasAck { get; set; }
        public bool Success { get; set; }
        public string Response { get; set; }
        public string Information { get; set; }
        public bool Polling { get; set; }
        public int Quantity { get; set; }
        public string ReturnValue { get; set; }
    }
}
