using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    [Table("History")]
    public class History : IEntity
    {
        public int Id { get; set; }
        public int ActionCode { get; set; }
        public string ActionCodeName { get; set; }
        public DateTime ActionDateTime { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int RequestedQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public int StationId { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public int? OrderId { get; set; }
        public int? OrderDetailId { get; set; }
        public string EmpId { get; set; }
        public DateTime? TransmitDateTime { get; set; }
        public string CostCenter { get; set; }
        public int Priority { get; set; }
        public DateTime? LoadDate { get; set; }
        public string OrderInfo { get; set; }
        public string OrderDetailInfo { get; set; }
        public string TypeCode { get; set; }
        public string PrimeBin { get; set; }
        public string NewBin { get; set; }
        public string TroubleBit { get; set; }
    }
}
