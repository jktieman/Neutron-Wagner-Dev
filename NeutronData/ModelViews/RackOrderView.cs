using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class RackOrderView
    {
     public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Priority { get; set; }
        public string StatusName { get; set; }
        private int _lines;
        private int _pieces;
        private string _searchField;
        public DateTime LoadDate { get; set; }
        public Order Order { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }

        public string SearchField
        {
            get
            {
                return $"{Ord1.ToLower()}{Ord2.ToLower()}";
            }
            set { _searchField = value; }
        }

        public int Lines
        {
            get
            {
                _lines = Order.OrderDetails.Where(o => o.StationNumber == 8).ToList().Count;
                return _lines;
            }
            set { _lines = value; }
        }

        public int Pieces
        {
            get
            {
                _pieces = Order.OrderDetails.Where(o => o.StationNumber == 8).Sum(s => s.Quantity);
                return _pieces;
            }
            set { _pieces = value; }
        }
    }
}
