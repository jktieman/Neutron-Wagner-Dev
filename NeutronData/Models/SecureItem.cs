using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
