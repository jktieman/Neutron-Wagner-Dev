using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace NeutronData.Models
{
    public class ReplenOrder : IEntity
    {
        public int Id { get; set; }
        
        [StringLength(maximumLength:100)]
        public string Ord1 { get; set; }

        [StringLength(maximumLength: 100)]
        public string Ord2 { get; set; }
        
        public int Priority { get; set; }

        [StringLength(maximumLength: 1000)]
        public string OrderInfo { get; set; }
        
        public DateTime LoadDate { get; set; }
        public int ShipperId { get; set; }
        public int ShipMethodId { get; set; }
        public int OrderStatusId { get; set; }
        public virtual ICollection<ReplenOrderDetail> ReplenOrderDetails { get; set; } = new List<ReplenOrderDetail>();
        public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

        [ForeignKey("ShipperId")]
        public virtual Shipper Shipper { get; set; }
        [ForeignKey("ShipMethodId")]
        public virtual ShipMethod ShipMethod { get; set; }

        [ForeignKey("OrderStatusId")]
        public virtual OrderStatus OrderStatus { get; set; }

        public int Pieces()
        {
            var num = 0;
            foreach (var item in ReplenOrderDetails)
            {
                num += item.Quantity;
            }
            return num;
        }

        public int Lines()
        {
            return ReplenOrderDetails.Count;
        }
    }
}
