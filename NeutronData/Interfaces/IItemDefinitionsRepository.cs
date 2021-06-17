using System.Collections.Generic;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IItemDefinitionsRepository
    {
        IEnumerable<ItemDefinitionView> GetAllItemDefinitionViews(string find = "");
        IEnumerable<ItemDefinitionView> FindItemDefinitionViewsByStation(string find = "", int stationId = 0);
        IEnumerable<ItemDefinitionView> FindItemDefinitionViews(string find = "");
        IEnumerable<NewItemView> GetNewItemViews(string find = "");
    }
}