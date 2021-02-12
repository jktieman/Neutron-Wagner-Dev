using System.Collections.Generic;

namespace NeutronData.Models
{
    public class SecureItem
    {
        public SecureItem()
        {
            Groups = new List<Group>();
        }

        public int SecureItemId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
    }
}
