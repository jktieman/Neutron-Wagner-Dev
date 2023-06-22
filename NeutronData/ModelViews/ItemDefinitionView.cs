using NeutronData.Models;
using System.Collections.Generic;

namespace NeutronData.ModelViews
{
    public class ItemDefinitionView
    {
        public ItemDefinitionView()
        {
            Images = new List<ItemImage>();
        }
        public int Id { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public int AreaNumber { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int UnitOfIssueId { get; set; }
        public int SizeCodeId { get; set; }
        public int VelocityCodeId { get; set; }
        public int HeightCodeId { get; set; }
        public int LocationMax { get; set; }
        public int LocationMin { get; set; }
        public int SystemMax { get; set; }
        public int SystemMin { get; set; }
        public int StorageTypeId { get; set; }
        public float Weight { get; set; }
        public bool Scale { get; set; }
        public List<ItemImage> Images { get; set; }
        public string StorageTypeName { get; set; }
        public string UnitOfIssueName { get; set; }
        public string SizeCodeName { get; set; }
        public string VelocityCodeName { get; set; }
        public string HeightCodeName { get; set; }
    }
}
