namespace SAPServer.Models
{
    public class GoodsReceipt
    {

        public decimal TRANSID { get; set; }
        public string TANUM { get; set; }
        public string TAPOS { get; set; }
        public string MATNR { get; set; }
        public decimal VERME { get; set; }
        public string MEINS { get; set; }
        public string MAKTX { get; set; }
        public decimal NISTA { get; set; }
        public bool Processed { get; set; }
    }
}
