using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class CycleCount
    {
        public int Id { get; set; }
        public int ItemDefinitionId { get; set; }
        public string CycleCountClass { get; set; }
        public DateTime LastCycleCount { get; set; }

    }
}
