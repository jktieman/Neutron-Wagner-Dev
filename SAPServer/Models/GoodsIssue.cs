namespace SAPServer.Models
{
    public class GoodsIssue
    {
        public decimal TRANSID { get; set; }
        public string TANUM { get; set; }
        public string TAPOS { get; set; }
        public string MATNR { get; set; }
        public string VSBED { get; set; }
        public string MEINS { get; set; }
        public string SPART { get; set; }
        public decimal NSOLM { get; set; }
        public decimal NISTA { get; set; }
        public string EAN11 { get; set; }
        public string MATKL { get; set; }
        public string VBELN { get; set; }
        public string KDMAT { get; set; }
        public string TEXT { get; set; }
        public bool Processed { get; set; }
    }
}
