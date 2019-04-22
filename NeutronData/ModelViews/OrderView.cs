using NeutronData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.ModelViews
{
    public class OrderView
    {
        readonly int[] _statusToGet = new int[] { 1, 2, 3, 4 };

        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public string Starter { get; set; }
        public int Priority { get; set; }
        public string ShipMethodName { get; set; }
        public string OrderStatusName { get; set; }
        public int BatchPosition { get; set; }
        public string Station_1_HasPicks { get; set; }
        public string Station_2_HasPicks { get; set; }
        public string Station_3_HasPicks { get; set; }
        public string Station_4_HasPicks { get; set; }
        public string Station_5_HasPicks { get; set; }
        public string Station_8_HasPicks { get; set; }
        private int lines;
        private int pieces;
        private string searchField;
        public DateTime LoadDate { get; set; }
        public int ShipMethodId { get; set; }
        public int OrderStatusId { get; set; }
        public Order Order { get; set; }
        public int CurrentPickStation { get; set; }
        public int FirstPickStation { get; set; }
        public string SearchField
        {
            get
            {
                return string.Format("{0}{1}", Ord1.ToLower()
               , Ord2.ToLower());
            }
            set { searchField = value; }
        }

        public int Lines
        {
            get
            {
                lines = Order.OrderDetails.Count;
                //lines = 0;
                //foreach (var item in Order.OrderDetails)
                //{
                //    if (statusToGet.Contains(item.LineStatusId))
                //    {
                //        lines += 1;
                //    }
                //}
                return lines;
            }
            set { lines = value; }
        }

        public int Pieces
        {
            get
            {
                pieces = Order.OrderDetails.Sum(s => s.Quantity);
                //pieces = 0;
                //foreach (var item in Order.OrderDetails)
                //{
                //    if (statusToGet.Contains(item.LineStatusId))
                //    {
                //        pieces += item.Quantity;
                //    }
                //}
                return pieces;
            }
            set { pieces = value; }
        }
    }
}
