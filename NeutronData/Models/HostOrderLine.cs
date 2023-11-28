using System;
using System.Linq;

namespace NeutronData.Models {

    public class HostOrderLine {

        public string StationNumber { get; set; }

        public string OrderNumber { get; set; }

        public string Sku { get; set; }

        public string Quantity { get; set; }

        public string Description { get; set; }

        public string CountryOfOrigin { get; set; }

        public string Priority { get; set; }

        public string Order => OrderNumber.Substring(10, 10);

        public string Invoice => OrderNumber.Substring(0, 10);
    }
}