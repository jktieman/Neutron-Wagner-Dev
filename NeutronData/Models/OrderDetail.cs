using System;
using System.ComponentModel.DataAnnotations;
using NeutronData.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace NeutronData.Models
{
    public class OrderDetail : IHostOrder, IEntity, IOrderDetail
    {
        public int Id { get; set; }
        public int OrderId {get; set; }
        public int ItemDefinitionId { get; set; }
        public int Quantity { get; set; }
        public int PickedQuantity { get; set; }
        
        [StringLength(30)]
        public string DateTime { get; set; }   = string.Empty;
        
        [StringLength(30)]
        public string EmpId { get; set; } = string.Empty;
        [StringLength(30)]
        public string JobNum { get; set; } = string.Empty;
        [StringLength(30)]
        public string NewBin { get; set; } = string.Empty;
        [StringLength(250)]
        public string PartDesc { get; set; } = string.Empty;
        [StringLength(100)]
        public string PartNum { get; set; } = string.Empty;
        [StringLength(30)]
        public string PrimeBin { get; set; } = string.Empty;
        [StringLength(10)]
        public string Qty { get; set; } = string.Empty;
        [StringLength(30)]
        public string TroubleBit { get; set; } = string.Empty;
        [StringLength(30)]
        public string TypeCode { get; set; } = string.Empty;
        public int LineStatusId { get; set; }
        public int AreaId { get; set; }
        [StringLength(500)]
        public string OrderDetailInfo { get; set; }
        public int TransId { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        [ForeignKey("ItemDefinitionId")]
        public virtual ItemDefinition ItemDefinition { get; set; }
       
    }
}
