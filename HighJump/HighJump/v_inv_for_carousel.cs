using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighJump
{
    [Table("v_inv_for_carousel")]
    public class v_inv_for_carousel
    {

        public string wh_id { get; set; }
        public string location_id { get; set; }
        [Key]
        public string item_number { get; set; }
        public Double actual_qty { get; set; }
        public string pick_location { get; set; }
    }
}