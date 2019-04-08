using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("SizeClassSToB")]
    public class SizeClassSToB
    {
        [Key]
        public string StringSize { get; set; }
        public byte ByteSize { get; set; }
    }
}
