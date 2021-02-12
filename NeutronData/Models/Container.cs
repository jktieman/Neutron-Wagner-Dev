using System.ComponentModel.DataAnnotations.Schema;

namespace NeutronData.Models
{
    public class Container
    {
        public int Id { get; set; }
        public string ContainerName { get; set; }
        public int OrderId { get; set; }
        public int ItemDefinitionId { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        [ForeignKey("ItemDefinitionId")]
        public virtual ItemDefinition ItemDefinition { get; set; }
    }
}
