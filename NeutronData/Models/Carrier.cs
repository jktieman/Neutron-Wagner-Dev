using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class Carrier
    {
        public Carrier()
        {
            Roles = new List<Role>();
        }
        public int CarrierId { get; set; }
        public int StationNumber { get; set; }
        public int DeviceNumber { get; set; }
        public int CarrierNumber { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public override string ToString()
        {
            return string.Format("{0} - {1}",DeviceNumber.ToString(), CarrierNumber.ToString());
        }
    }
}
