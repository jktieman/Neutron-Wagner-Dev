using NeutronData.Models;

namespace NeutronData.ModelViews
{
    public class InventoryView
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public int StationId { get; set; }
        public int StationNumber { get; set; }
        public string StationName { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public int LocationCodeId { get; set; }
        public int StorageTypeId { get; set; }
        public string ReceivedDate { get; set; }
        public string StorageTypeName { get; set; }
        public string SizeCodeName { get; set; }
        public string VelocityCodeName { get; set; }
        public string HeightCodeName { get; set; }
        public string LocationCodeName { get; set; }
        public int ItemDefinitionId { get; set; }
        public int LocationId { get; set; }
        public ItemDefinition ItemDefinition { get; set; }
        public Location Location { get; set; }
    }
}
