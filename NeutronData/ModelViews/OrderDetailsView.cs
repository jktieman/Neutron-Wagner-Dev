namespace NeutronData.ModelViews
{
    public class OrderDetailsView
    {
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public int OrderDetailId { get; set; }
        public int ItemId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int PickedQuantity { get; set; }
        public int LineStatusId { get; set; }
        public string LineStatusName { get; set; }
        public int StationNumber { get; set; }
    }
}
