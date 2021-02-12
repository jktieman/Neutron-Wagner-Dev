using System.ComponentModel.DataAnnotations;

namespace NeutronData.Models
{

    public class AkaType
    {
        [Key]
        [MaxLength(50)]
        public string Aka { get; set; }
        [MaxLength(35)]
        public string Item { get; set; }
    }
}
