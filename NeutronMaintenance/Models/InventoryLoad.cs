using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronMaintenance.Models
{
    public class InventoryLoad
    {
        public string Station { get; set; }
        public string StorageType { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Quantity { get; set; }
        public string Slot { get; set; }
        public string PrimeBin { get; set; }
        public string Carousel { get; set; }
        public string Bin { get; set; }
        public string Level { get; set; }
        public string Partition { get; set; }
        public string Tag { get; set; }
        public string SizeCode { get; set; }
        public string VelocityCode { get; set; }
        public string HeightCode { get; set; }
        public string UserCode { get; set; }
        public string ReceivedDate { get; set; }
        public string Id { get; set; }
    }
}
