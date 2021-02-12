using System.Collections.Generic;

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
