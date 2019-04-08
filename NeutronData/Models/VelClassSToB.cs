using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("VelClassSToB")]
    public class VelClassSToB
    {
        [Key]
        public string StringSize { get; set; }
        public byte ByteSize { get; set; }
    }
}
