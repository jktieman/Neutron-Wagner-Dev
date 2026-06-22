using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighJump
{
    public class t_al_host_carousel_outbound
    {
        [Key]
        public int host_carousel_outbound_id { get; set; }
        public string wh_id { get; set; }
        public string container_label { get; set; }
        public string item_number { get; set; }
        public string pick_quantity { get; set; }
        public string item_description { get; set; }
        public string country_of_origin { get; set; }
        public string record_type { get; set; }
        public string status { get; set; }
        public string inserted_by { get; set; }
        public System.DateTime inserted_date { get; set; }
        public string updated_by { get; set; }
        public System.DateTime updated_date { get; set; }
    }
}
