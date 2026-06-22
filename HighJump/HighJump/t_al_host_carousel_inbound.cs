using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighJump
{
    public class t_al_host_carousel_inbound
    {
        [Key]
        public int host_carousel_inbound_id { get; set; }
        public string container_label { get; set; }
        public string item_number { get; set; }
        public double pick_quantity { get; set; }
        public string employee_id { get; set; }
        public string status { get; set; }
        public string inserted_by { get; set; }
        public DateTime? inserted_date { get; set; }
        public string updated_by { get; set; }
        public DateTime? updated_date { get; set; }
    }
}
