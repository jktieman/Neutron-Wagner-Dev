using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class ReserveType
    {
        [StringLength(35)]
        public string Sku { get; set; }
        [StringLength(10)]
        public string Slot { get; set; }
        [StringLength(8)]
        public string RcvDate { get; set; }
        [StringLength(8)]
        public string ActDate { get; set; }
        [StringLength(8)]
        public string OvrDate { get; set; }
        public int Qty { get; set; }
        [StringLength(10)]
        public string Employee { get; set; }
        public int Quart { get; set; }
        public int Id { get; set; }
    }
}
