using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronMaintenance.Models
{
    public class ItemDefinitionLoad
    {
        public string Station { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string LocationMax { get; set; }
        public string LocationMin { get; set; }
        public string SystemMax { get; set; }
        public string SystemMin { get; set; }
        public string SizeCode { get; set; }
        public string VelocityCode { get; set; }
        public string HeightCode { get; set; }
        public string LocationCode { get; set; }
        public string StorageType { get; set; }
        public string UnitOfIssue { get; set; }
        public string Weight { get; set; }
        public string Scale { get; set; }
        public string Id { get; set; }
    }
}
