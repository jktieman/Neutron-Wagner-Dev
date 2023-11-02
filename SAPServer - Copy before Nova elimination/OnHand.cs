using System.ComponentModel.DataAnnotations;

namespace SAPServer
{

    [System.ComponentModel.DataAnnotations.Schema.Table("OnHand")]
    public partial class OnHand
    {
        public int ID { get; set; }

        [StringLength(35)]
        public string SKU { get; set; }

        [StringLength(10)]
        public string LOC { get; set; }

        public int? QTY { get; set; }

        public bool InProcess { get; set; }

        [StringLength(3)]
        public string Zone { get; set; }
    }
}
