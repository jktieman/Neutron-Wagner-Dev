using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Interfaces;

namespace NeutronData.Models;
public class Replenishment
{
    [Key]
    public string Item { get; set; }
    public int AreaEight { get; set; }
    public int QuantityInEight { get; set; }
    public int ReplenArea { get; set; }
    public int ReplenAreaQuantity { get; set; }
    public int SystemMin { get; set; }
    public int SystemMax { get; set; }
    public int QuantityNeeded { get; set; }
    
}
