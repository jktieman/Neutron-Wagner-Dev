using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    //Off Carousel Location
    public class ResDefType
    {
        [StringLength(10)]
        public string Slot { get; set; }
        public int InUse { get; set; }
        [StringLength(1)]
        public string Zone { get; set; }
        [StringLength(1)]
        public string VelClass { get; set; }
        [StringLength(1)]
        public string SizeClass { get; set; }
        [StringLength(1)]
        public string Class { get; set; }
        [StringLength(10)]
        public string PickSeq { get; set; }
        public int Release { get; set; }
        public int Id { get; set; }
    }
}
