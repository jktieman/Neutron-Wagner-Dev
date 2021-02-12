using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    [Table("Activities")]
    public class Activity
    {
        public int ActivityId { get; set; }
        public string Name { get; set; }
        public int Seq { get; set; }
    }
}
