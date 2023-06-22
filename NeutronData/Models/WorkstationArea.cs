using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class WorkstationArea
    {
        [Key]
        [Column(Order = 1)]
        public int WorkstationId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int AreaId { get; set; }

        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }

        [ForeignKey("WorkstationId")]
        public virtual Workstation Workstation { get; set; }
    }
}
