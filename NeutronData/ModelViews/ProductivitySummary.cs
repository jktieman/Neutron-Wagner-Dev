namespace NeutronData.ModelViews
{
    public class ProductivitySummary
    {
        public string Date { get; set; }
        public string Employee { get; set; }
        public string Action { get; set; }
        public int Lines { get; set; }
        public int Pieces { get; set; }
        public int Orders { get; set; }
        public int AreaId { get; set; }
        public int UserId { get; set; }
        public int ActionCodeId { get; set; }
        public int WorkstationId { get; set; }
        public int ActionCode { get; set; }
        public int TotalLines { get; set; }
        public int TotalPieces { get; set; }
        public int TotalOrders { get; set; }
    }
}
