using System.Collections.Generic;
using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class Carrier
    {
        public int Id { get; set; }
        public int AreaId { get; set; }
        public int WorkstationId { get; set; }
        public int DeviceNumber { get; set; }
        public int CarrierNumber { get; set; }
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

        public override string ToString()
        {
            return $"{DeviceNumber.ToString()} - {CarrierNumber.ToString()}";
        }
    }
}
