using System.Collections.Generic;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IItemDefinitionsRepository
    {
        // IEnumerable<ItemDefinitionView> GetAllItemDefinitionViews(string find = "");
        // IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByWorkstation(string find = "", int workstationId = 0);
        IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByArea(string find = "", int areaId = 0);
        IEnumerable<ItemDefinitionView> FindItemDefinitionViews(string find = "");
        IEnumerable<NewItemView> GetNewItemViews(string find = "");
    }
}