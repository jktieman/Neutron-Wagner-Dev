namespace Neutron.Models
{
    public class BatchPosition
    {
        public int PositionNumber { get; set; }
        public int OrderId { get; set; }
        public string Ord1 { get; set; }
        public string Ord2 { get; set; }
        public bool OrderComplete { get; set; }
    }
}
