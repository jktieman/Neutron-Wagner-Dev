using System.ComponentModel.DataAnnotations;

namespace NeutronData.Models
{
    public class NOVA_OH
    {
        [Key]
        [StringLength(26)]
        public string sku { get; set; }

        [StringLength(10)]
        public string loc { get; set; }

        public decimal? qty { get; set; }

        public bool? inprocess { get; set; }
    }
}
