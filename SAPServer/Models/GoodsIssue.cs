namespace SAPServer.Models
{
    /// <summary>
    /// SAP Goods Issue
    /// </summary>
    public class GoodsIssue
    {
        // SAP TransId
        public decimal TRANSID { get; set; }
        // Task Number - (TO)
        public string TANUM { get; set; }
        // Line Number
        public string TAPOS { get; set; }
        // SKU/Item Number
        public string MATNR { get; set; }
        // Priority
        public string VSBED { get; set; }
        // UOI/BP
        public string MEINS { get; set; }
        // Division
        public string SPART { get; set; }
        // Unknown
        public decimal MENGE { get; set; }
        // Quantity
        public decimal NSOLM { get; set; }
        // Shipped Quantity
        public decimal NISTA { get; set; }
        //UPC
        public string EAN11 { get; set; }
        // Description
        public string MATKL { get; set; }
        // Order Number
        public string VBELN { get; set; }
        // Customer SKU
        public string KDMAT { get; set; }
        // Additional Information
        public string NAME1 { get; set; }
        public string STREET { get; set; }
        // City
        public string CITY1 { get; set; }
        // State
        public string REGION { get; set; }
        // Postal Code
        public string POST_CODE1 { get; set; }
        // Country
        public string COUNTRY { get; set; }
        public string TEXT { get; set; }
        //Processed record flag
        public bool Processed { get; set; }
    }
}
