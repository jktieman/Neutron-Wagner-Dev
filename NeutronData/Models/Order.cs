using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class Order : IEntity
    {
        public Order()
        {
        }
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string OrderInfo { get; set; }
        public int Priority { get; set; }
        public DateTime LoadDate { get; set; }
        public int ShipperId { get; set; }
        public int ShipMethodId { get; set; }
        public int OrderStatusId { get; set; }
        // REMOVE VIRTUAL ON 05/24/2024
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Container> Containers { get; set; } = new List<Container>();

        [ForeignKey("ShipperId")]
        public virtual Shipper Shipper { get; set; }
        [ForeignKey("ShipMethodId")]
        public virtual ShipMethod ShipMethod { get; set; }
        [ForeignKey("OrderStatusId")]
        public virtual OrderStatus OrderStatus { get; set; }

        public int Pieces()
        {
            int num = 0;
            foreach (var item in OrderDetails)
            {
                num += item.Quantity;
            }
            return num;
        }

        public int Lines()
        {
            return OrderDetails.Count;
        }
    }
}
