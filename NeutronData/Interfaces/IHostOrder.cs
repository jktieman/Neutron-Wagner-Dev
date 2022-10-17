namespace NeutronData.Interfaces
{
    public interface IHostOrder
    {
        string DateTime { get; set; }
        string EmpId { get; set; }
        string JobNum { get; set; }
        string NewBin { get; set; }
        string PartDesc { get; set; }
        string PartNum { get; set; }
        string PrimeBin { get; set; }
        string Qty { get; set; }
        string TroubleBit { get; set; }
        string TypeCode { get; set; }

    }
}