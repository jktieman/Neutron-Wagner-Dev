using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("RoleUser")]
    public class RoleUser
    {
        [Key, Column(Order = 0)]
        public int RoleId { get; set; }
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
