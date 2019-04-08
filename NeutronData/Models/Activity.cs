using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    [Table("Activities")]
    public class Activity
    {
        public int ActivityId { get; set; }
        public string Name { get; set; }
        public int Seq { get; set; }
    }
}
