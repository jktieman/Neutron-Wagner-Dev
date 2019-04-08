using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    [Table("LocationGroupLocations")]
    public class LocationGroupLocation
    {
        [Key, Column(Order = 0)]
        public int LocationGroupId { get; set; }
        [Key, Column(Order = 1)]
        public int LocationId { get; set; }
        [ForeignKey("LocationGroupId")]
        public virtual Group Group { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
    }
}
