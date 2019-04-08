using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class HistoryView
    {
        public int Id { get; set; }
        public int ActionCode { get; set; }
        public string ActionCodeName { get; set; }
        public string ActionDateTime { get; set; }
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
        public int OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public string EmpId { get; set; }
        public string EmployeeName { get; set; }
        public string CostCenter { get; set; }
        public string TransmitDate { get; set; }
        public string OrderInfo { get; set; }
        public string OrderDetailInfo { get; set; }
    }
}
