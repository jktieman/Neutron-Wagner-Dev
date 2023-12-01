using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models;
public class NeutronInput
{
    public string TransId { get; set; }
    public string Sku { get; set; }
    public int Qty { get; set; }
    public string Division { get; set; }
    public string Order { get; set; }
    public string Priority { get; set; }
    public string Invoice { get; set; }
    public string Des { get; set; }
    public string Upc { get; set; }
    public string LineNo { get; set; }
    public string Name { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string Text { get; set; }

}
