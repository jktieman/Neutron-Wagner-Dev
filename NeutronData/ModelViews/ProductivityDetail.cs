using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class ProductivityDetail
    {
        public string Date { get; set; }
        public string Employee { get; set; }
        public string Action { get; set; }
        public int OrderId { get; set; }
        public string Order { get; set; }
        public string Reservation { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Requested { get; set; }
        public int Issued { get; set; }
        public int Station { get; set; }
        public int UserId { get; set; }
        public int ActionCodeId { get; set; }
        public int StationId { get; set; }
        public int ActionCode { get; set; }
        public int TotalLines { get; set; }
        public int TotalPieces { get; set; }
        public int TotalOrders { get; set; }
    }
}
