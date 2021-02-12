using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("GroupSecureItem")]
    public class GroupSecureItem
    {
        [Key, Column(Order = 0)]
        public int GroupId { get; set; }
        [Key, Column(Order = 1)]
        public int SecureItemId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        [ForeignKey("SecureItemId")]
        public virtual SecureItem SecureItem { get; set; }
    }
}
