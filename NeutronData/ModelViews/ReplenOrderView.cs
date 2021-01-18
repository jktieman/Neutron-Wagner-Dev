using System.Linq;
using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class ReplenOrderView
    {
        private readonly int[] _statusToGet = { 1, 2, 3, 4 };

        public int Id { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int Starter { get; set; }
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
        private int _lines;
        private int _pieces;
        private string _searchField;
        public string LoadDate { get; set; }
        public int ShipMethodId { get; set; }
        public int OrderStatusId { get; set; }
        public ReplenOrder ReplenOrder { get; set; }
        public int CurrentPickStation { get; set; }
        public int FirstPickStation { get; set; }

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
                _lines = 0;
                foreach (var item in ReplenOrder.ReplenOrderDetails)
                {
                    if (_statusToGet.Contains(item.LineStatusId))
                    {
                        _lines += 1;
                    }
                }
                return _lines;
            }
            set { _lines = value; }
        }

        public int Pieces
        {
            get
            {
                _pieces = 0;
                foreach (var item in ReplenOrder.ReplenOrderDetails)
                {
                    if (_statusToGet.Contains(item.LineStatusId))
                    {
                        _pieces += item.Quantity;
                    }
                }
                return _pieces;
            }
            set { _pieces = value; }
        }
    }
}
