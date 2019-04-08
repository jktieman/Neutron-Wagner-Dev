using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class Role
    {
        public Role()
        {
            Carriers = new List<Carrier>();
            Users = new List<User>();
        }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Carrier> Carriers { get; set; }
    }
}
