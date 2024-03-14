using System.Collections.Generic;
using System.Threading.Tasks;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface ILocationsRepository
    {
        IEnumerable<LocationView> GetAllLocationViewsExact(int areaId, int sizeCodeId
            , int velocityCodeId, int heightCodeId, int inUse);

        IEnumerable<LocationView> GetAllLocationViewsExactByInUse(int areaId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int inUse);
        IEnumerable<LocationView> GetAllLocationViewsExactByAreas(string areas, int itemDefinitionSizeCodeId
            , int itemDefinitionVelocityCodeId, int itemDefinitionHeightCodeId, int inUse);
        IEnumerable<LocationView> FindLocationViewsByArea(int areaId);
        IEnumerable<LocationView> FindLocationViewsByAreaAndInUse(int areaId, int inUse);
        IEnumerable<LocationView> FindLocationViewsByAreaAndSlot(int areaId, string slot);
        IEnumerable<LocationView> FindLocationViewsBySlot(string find);
        IEnumerable<LocationView> FindLocationViews(string find = "");
        void SetLocationInUse(int locationId, bool b);
        int GetMaxColumns(int areaId, int device, int tray);
        int GetMaxRows(int areaId, int device, int tray);
        Task<bool> IsInInventory(int locationId);
        void SetLocationCode(int locationId, string locationCode);
        int GetMaxSizeCodeByArea(int areaId);
    }
}