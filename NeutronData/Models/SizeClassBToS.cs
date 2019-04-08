using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("SizeClassBToS")]
    public class SizeClassBToS
    {
        [Key]
        public byte ByteSize { get; set; }
        public string StringSize { get; set; }
    }
}
