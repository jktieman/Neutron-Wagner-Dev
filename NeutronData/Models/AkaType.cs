using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{

    public class AkaType
    {
        [Key]
        [MaxLength(50)]
        public string Aka { get; set; }
        [MaxLength(35)]
        public string Item { get; set; }
    }
}
