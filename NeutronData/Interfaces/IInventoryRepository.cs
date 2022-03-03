using System.Collections.Generic;
using NeutronData.ModelViews;
using NeutronData.SqlModelViews;

namespace NeutronData.Interfaces
{
    public interface IInventoryRepository
    {
        List<InventoryView> GetInventoryViewAll();
        InventoryView GetInventoryViewById(int id);
        List<InventoryView> GetInventoryViewByItem(string item);
        List<HotStoreListView> GetHotStoreList(string s);
        List<SqlInventoryView> FindInventoryViewsByStation(string find, int stationId);
        List<SqlInventoryView> FindInventoryViews(string find);
        List<SqlInventoryView> GetAllInventoryViewsByItemDefinitionId(int id);
        string GetPrimeBin(int itemDefinitionId);
        int GetStationNumber(int itemDefinitionId);
        void Dispose();
    }
}