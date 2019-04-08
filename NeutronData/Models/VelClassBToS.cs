using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("VelClassBToS")]
    public class VelClassBToS
    {
        [Key]
        public byte ByteSize { get; set; }
        public string StringSize { get; set; }
    }
}
