namespace NeutronData.ModelViews
{
    public class SkipInventoryView
    {
        public int InventoryId { get; set; }
        public int AreaId { get; set; }
        public string StorageType { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string Slot { get; set; }
        public int PickSequence { get; set; }
        public int Quantity { get; set; }
        public int Required { get; set; }
        public int Picked { get; set; }

    }
}
