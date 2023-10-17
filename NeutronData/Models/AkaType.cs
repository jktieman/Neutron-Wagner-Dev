using System.ComponentModel.DataAnnotations;

namespace NeutronData.Models
{

    public class AkaType
    {
        [Key]
        [MaxLength(100)]
        public string Aka { get; set; }
        [MaxLength(100)]
        public string Item { get; set; }
    }
}
