using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronMaintenance.Models
{
    public class LocationLoad
    {
        public string InUse { get; set; }
        public string Station { get; set; }
        public string Carousel { get; set; }
        public string Bin { get; set; }
        public string Level { get; set; }
        public string Partition { get; set; }
        public string Tag { get; set; }
        public string SizeCode { get; set; }
        public string VelocityCode { get; set; }
        public string HeightCode { get; set; }
        public string LocationCode { get; set; }
        public string Id { get; set; }
        public string Slot { get; set; }
    }
}
