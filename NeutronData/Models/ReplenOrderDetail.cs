using NeutronData.Interfaces;
using NeutronData.Models.Lookups;
using System.ComponentModel.DataAnnotations.Schema;
using NeutronCore.Enums;

namespace NeutronData.Models
{
    public class ReplenOrderDetail : IHostOrder, IEntity
    {
        public int Id { get; set; }
        public int ReplenOrderId { get; set; }
        public int ItemDefinitionId { get; set; }
        public int Quantity { get; set; }
        public int PickedQuantity { get; set; }
        public string DateTime { get; set; }
        public string EmpId { get; set; }
        public string JobNum { get; set; }
        public string NewBin { get; set; }
        public string PartDesc { get; set; }
        public string PartNum { get; set; }
        public string PrimeBin { get; set; }
        public string Qty { get; set; }
        public string TroubleBit { get; set; }
        public string TypeCode { get; set; }
        public string OrderDetailInfo { get; set; }
        public int LineStatusId { get; set; }
        public int AreaId { get; set; }

        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }
        [ForeignKey("ReplenOrderId")]
        public virtual ReplenOrder ReplenOrder { get; set; }
        [ForeignKey("ItemDefinitionId")]
        public virtual ItemDefinition ItemDefinition { get; set; }
    }
}


