using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class ReplenMilwaukeeTool
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string Item { get; set; }
        public int RequestedQuantity { get; set; }
        public string Size { get; set; }
    }
}
