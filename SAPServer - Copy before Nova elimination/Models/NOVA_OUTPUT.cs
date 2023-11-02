using System;
using System.ComponentModel.DataAnnotations;

namespace SAPServer.Models
{
    
    public class NOVA_OUTPUT
    {
        [Key]
        public int TRANSID { get; set; }

        [StringLength(2)]
        public string TRANSTYPE { get; set; }

        [StringLength(26)]
        public string SKU { get; set; }

       public decimal QTY { get; set; }

        [StringLength(20)]
        public string FLOCN { get; set; }

        [StringLength(20)]
        public string TLOCN { get; set; }

        [StringLength(12)]
        public string BP { get; set; }

        [StringLength(29)]
        public string ACCOUNTNO { get; set; }

        [StringLength(5)]
        public string ORDERCOMPANY { get; set; }
        
        public decimal ORDERNO { get; set; }
       
        public decimal INVOICENO { get; set; }

        [StringLength(2)]
        public string PRIORITY { get; set; }

        public decimal TOTENO { get; set; }

         public decimal BATCHNO { get; set; }

        public decimal SEQUENCENO { get; set; }

         public decimal TASKNO { get; set; }

        [StringLength(30)]
        public string SKUDESC { get; set; }

        [StringLength(10)]
        public string USERID { get; set; }

        [StringLength(30)]
        public string BOXID { get; set; }

        public decimal CYCLECOUNTNO { get; set; }

        [StringLength(3)]
        public string REASONCODE { get; set; }

        public DateTime TRANSDATE { get; set; }

        [StringLength(1)]
        public string PROCESSED { get; set; }

        [StringLength(20)]
        public string EXPLANATION { get; set; }

        [StringLength(20)]
        public string FULLFLAG { get; set; }
        public decimal BEGINNINGQTY { get; set; }

        [StringLength(10)]
        public string SUBLEDGER { get; set; }

        [StringLength(10)]
        public string TYPE { get; set; }
    }
}
