using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class NOVA_INPUT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal TRANSID { get; set; }

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

        [StringLength(1)]
        public string PRIORITY { get; set; }

        public decimal ORDERNO { get; set; }

        public decimal INVOICENO { get; set; }

        [StringLength(5)]
        public string DIVISION { get; set; }

        public decimal TOTENO { get; set; }

        public decimal BATCHNO { get; set; }

        public decimal SEQUENCENO { get; set; }

        public decimal TASKNO { get; set; }

        [StringLength(30)]
        public string SKUDESC { get; set; }

        [StringLength(10)]
        public string USERID { get; set; }

        [StringLength(30)]
        public string UPC { get; set; }

        [StringLength(1)]
        public string ACTION { get; set; }

        public DateTime? TRANSDATE { get; set; }

        [StringLength(1)]
        public string PROCESSED { get; set; }
        public string NAME1 { get; set; }
        public string STREET { get; set; }
        public string CITY1 { get; set; }
        public string REGION { get; set; }
        public string POST_CODE1 { get; set; }
        public string COUNTRY { get; set; }
        public string TEXT { get; set; }
    }
}
