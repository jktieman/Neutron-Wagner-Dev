using System;
using System.Collections.Generic;
using System.Linq;
using NeutronCore.Enums;
using NeutronCore.Extensions;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class ReplenRackOrderView
    {
        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Priority { get; set; }
        private int _lines;
        private int _pieces;
        private string _searchField;
        private string _statusName;
        public DateTime LoadDate { get; set; }
        public ReplenOrder Order { get; set; }
        public ICollection<ReplenOrderDetail> OrderDetails { get; set; }
        public int AreaId { get; set; }

        public string StatusName
        {
            get
            {
                if (!string.IsNullOrEmpty(_statusName)) return _statusName;
                if (OrderDetails.FirstOrDefault() == null) return _statusName;
                var id = OrderDetails.FirstOrDefault()?.LineStatusId;
                if (id == null) return _statusName;
                var lineStatus = (LineStatus)id;
                _statusName = lineStatus.GetEnumDescription();
                return _statusName;
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
                _lines = Order.ReplenOrderDetails.Where(o => o.AreaId == AreaId).ToList().Count;
                return _lines;
            }
            set { _lines = value; }
        }

        public int Pieces
        {
            get
            {
                _pieces = Order.ReplenOrderDetails.Where(o => o.AreaId == AreaId).Sum(s => s.Quantity);
                return _pieces;
            }
            set { _pieces = value; }
        }
    }
}