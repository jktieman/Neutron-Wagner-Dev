using NeutronData.Interfaces;

namespace NeutronData.Models
{
    public class PrintJob : IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string JobNum { get; set; }
        public bool PickDocument { get; set; }
        public bool ToteLabel { get; set; }
        
    }
}
