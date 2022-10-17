using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class HostOrder : IHostOrder
    {
        public int Id { get; set; }
        public string TypeCode { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string JobNum { get; set; }
        public string PrimeBin { get; set; }
        public string NewBin { get; set; }
        public string Qty { get; set; }
        public string TroubleBit { get; set; }
        public string DateTime { get; set; }
        public string EmpId { get; set; }
        public string BaseNum { get; set; }
        public string Machine { get; set; }
        public string Dept { get; set; }
        public string LineStatusId { get; set; }
        public OrderDetail OrderDetail { get; set; }
    }
}
