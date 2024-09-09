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

        IEnumerable<LocationView> GetAllLocationViewsExact(int areaId, int sizeCodeId
            , int velocityCodeId, int heightCodeId, int inUse, int currentPage);

        IEnumerable<LocationView> GetAllLocationViewsExactByInUse(int areaId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int inUse);
        IEnumerable<LocationView> GetAllLocationViewsExactByAreas(string areas, int itemDefinitionSizeCodeId
            , int itemDefinitionVelocityCodeId, int itemDefinitionHeightCodeId, int inUse);
        IEnumerable<LocationView> FindLocationViewsByArea(int areaId, int currentPage);
        IEnumerable<LocationView> FindLocationViewsByArea(int areaId);
        IEnumerable<LocationView> FindLocationViewsByAreaAndInUse(int areaId, int inUse, int currentPage);
        IEnumerable<LocationView> FindLocationViewsByAreaAndInUse(int areaId, int inUse);
        IEnumerable<LocationView> FindLocationViewsByAreaAndSlot(int areaId, string slot);
        IEnumerable<LocationView> FindLocationViewsBySlot(string find);
        IEnumerable<LocationView> FindLocationViews(string find = "");
        int GetMaxColumns(int areaId, int device, int tray);
        int GetMaxRows(int areaId, int device, int tray);
        Task<bool> IsInInventory(int locationId);
        Task SetLocationCode(int locationId, string locationCode);
        Task SetLocationInUse(int locationId, bool b);
        int GetMaxSizeCodeByArea(int areaId);
        int TotalLocationViewsByArea(int areaId);
        int TotalLocationViewsByAreaAndInUse(int areaId, int inUse);

        int TotalLocationViewsExact(int areaId, int sizeCodeId,
            int velocityCodeId, int heightCodeId, int inUse);
    }
}