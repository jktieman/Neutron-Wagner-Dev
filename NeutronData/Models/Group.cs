using System.Collections.Generic;

namespace NeutronData.Models
{
    public class Group
    {
        public Group()
        {
            SecureItems = new List<SecureItem>();
            Users = new List<User>();
        }

        public int GroupId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<SecureItem> SecureItems { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
