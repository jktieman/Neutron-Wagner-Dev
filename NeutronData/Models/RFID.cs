using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class RFID
    {
        [Key]
        public string Tag { get; set; }

        public RFID(string tag)
        {
            Tag = tag;
        }

    }
}
