using System.Collections.Generic;
using System.Threading.Tasks;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.SqlModelViews;

namespace NeutronData.Interfaces
{
    public interface IInventoryRepository
    {
        List<InventoryView> GetInventoryViewAll();
        InventoryView GetInventoryViewById(int id);
        Inventory GetInventoryById(int id);
        Task<List<Inventory>> GetInventoryWithReleaseStorageAndZeroQuantityByArea(int areaId);
        List<InventoryView> GetInventoryViewByItem(string item);
        List<HotStoreListView> GetHotStoreList(string s);
        List<SqlInventoryView> FindInventoryViewsByArea(string find, int areaId);
        List<SqlInventoryView> FindInventoryViews(string find);
        List<SqlInventoryView> GetAllInventoryViewsByItemDefinitionId(int id);
        string GetPrimeBin(int itemDefinitionId);
        int GetAreaNumber(int itemDefinitionId);
        void Dispose();
    }
}