using System;

namespace NeutronData.SqlModelViews
{
    public class SqlInventoryView
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public bool PrimeBin { get; set; }
        public int Loc1 { get; set; }
        public int Loc2 { get; set; }
        public int Loc3 { get; set; }
        public int Loc4 { get; set; }
        public int Loc5 { get; set; }
        public string Slot { get; set; }
        public int PickSequence { get; set; }
        public bool InUse { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public int StorageTypeId { get; set; }
        public string StorageTypeName { get; set; }
        public int SizeCodeId { get; set; }
        public string SizeCodeName { get; set; }
        public int VelocityCodeId { get; set; }
        public string VelocityCodeName { get; set; }
        public int HeightCodeId { get; set; }
        public string HeightCodeName { get; set; }
        public string LocationCode { get; set; }
        public int ItemDefinitionId { get; set; }
        public int LocationId { get; set; }
        public int UnitOfIssueId { get; set; }
        public string UnitOfIssueName { get; set; }
        public int LocationMax { get; set; }
        public string RFID { get; set; }

    }
}
