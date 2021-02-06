using System;
using System.Collections.Generic;
using System.Linq;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class RackReplenOrderView
    {
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Priority { get; set; }
        public int StationNumber { get; set; }

        private int _lines;
        private int _pieces;
        private string _searchField;
        private string _statusName;

        public DateTime LoadDate { get; set; }
        public ReplenOrder Order { get; set; }
        public ICollection<ReplenOrderDetail> OrderDetails { get; set; }
        public string StatusName
        {
            get
            {
                return string.IsNullOrEmpty(_statusName) ? OrderDetails.FirstOrDefault()?.LineStatus.Name : _statusName;
            }
            set
            {
                _statusName = value;
            }
        }
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
                _lines = Order.ReplenOrderDetails.Where(o => o.StationNumber == StationNumber).ToList().Count;
                return _lines;
            }
            set { _lines = value; }
        }

        public int Pieces
        {
            get
            {
                _pieces = Order.ReplenOrderDetails.Where(o => o.StationNumber == StationNumber).Sum(s => s.Quantity);
                return _pieces;
            }
            set { _pieces = value; }
        }
    }
}