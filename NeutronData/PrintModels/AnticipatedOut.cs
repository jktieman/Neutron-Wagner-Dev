namespace NeutronData.PrintModels
{
    public class AnticipatedOut
    {
        public int Area { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Required { get; set; }
        public int Ordered { get; set; }
        public int OnHand { get; set; }
        public int LocationMax { get; set; }
        public int CapacityMinus { get; set; }
    }
}