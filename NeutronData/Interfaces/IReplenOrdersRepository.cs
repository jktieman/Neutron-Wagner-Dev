using System.Collections.Generic;
using System.Threading.Tasks;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.Models.Lookups;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IReplenOrdersRepository
    {
        ReplenOrder GetOrder(int id);
        IEnumerable<SizeCode> GetSizeCodesByArea(int areaId);
        IEnumerable<VelocityCode> GetVelocityCodesByArea(int areaId);
        IEnumerable<HeightCode> GetHeightCodesByArea(int areaId);
        List<AvailableReplenOrdersView> GetAvailableReplenOrdersForInductionScreen(int areaId, string searchField);
        Task<List<AvailableReplenOrdersView>> GetAvailableReplenOrdersForInductionScreenAsync(int areaId, string searchField);
        IEnumerable<ReplenOrderView> GetReplenOrderViews(string orderStatus, string searchField);
        IEnumerable<ReplenOrderView> GetOrderView();
        List<AvailableReplenOrdersView> GetAvailableOrders(WorkstationView station);
        List<AvailableReplenOrdersView> GetAvailableOrders(WorkstationView station, string search, bool serialPicking, bool showSkips = false);
        IEnumerable<ReplenOrderView> GetCompletedOrders(string search);
        ReplenOrder GetOrder();
        List<ReplenPickView> GetOrderLines();
        IEnumerable<ReplenOrderView> GetAvailableOrders(string search = "");
        List<ReplenPickView> GetOrderLines(List<BatchPosition> ordersToPick);
        IEnumerable<ReplenPickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum);
        IEnumerable<ReplenOrderView> GetRackOrders(string search);
        IEnumerable<RackReplenOrderView> GetRackOrdersView(int rackStationNumber, string search = @"");
        ReplenOrder GetOrderAndOrderDetails(int orderId, int areaId);
        IEnumerable<ReplenOrderView> GetAvailableReplenOrderViews(string orderStatus, string findWhat);
        IEnumerable<ReplenOrderView> GetReplenStoreOrderViews(string orderStatus, string searchfield);
        IEnumerable<ReplenOrderView> GetPutawayOrderViews(string orderStatus, string searchField);
        IEnumerable<ReplenOrderView> GetCompletedReplenOrderViews(string orderStatus = "6", string searchField = "");
        int[] GetOrderDetailIds(int[] currentOrderIds, int areaId);
        void SetOrderStatusToAvailableIfNotComplete(int[] orderIds);
    }
}
