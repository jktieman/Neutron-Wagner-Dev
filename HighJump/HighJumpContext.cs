using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighJump
{
    public class HighJumpContext : DbContext
    {
        public HighJumpContext() : base("name=HighJumpData") { }

        public DbSet<t_al_host_carousel_inbound> t_al_host_carousel_inbound { get; set; }
        public DbSet<t_al_host_carousel_outbound> t_al_host_carousel_outbound { get; set; }
        public DbSet<v_inv_for_carousel> v_inv_for_carousel { get; set; }
    }
}
