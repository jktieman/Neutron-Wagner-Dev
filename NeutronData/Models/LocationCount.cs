using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class LocationCount : IEntity
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int ItemDefinitionId { get; set; }
        public int LocationId { get; set; }
        public int UserId { get; set; }
        public int PreviousQty { get; set; }
        public int NewQty { get; set; }
        public DateTime CountDate { get; set; }
        //[ForeignKey("InventoryId")]
        //public virtual Inventory Inventory { get; set; }
        //[ForeignKey("ItemDefinitionId")]
        //public virtual ItemDefinition ItemDefinition { get; set; }
        //[ForeignKey("LocationId")]
        //public virtual Location Location { get; set; }
        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }
    }
}
