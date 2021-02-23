using System.Collections.Generic;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IReplenOrdersRepository
    {
        ReplenOrder GetOrder(int id);
        IEnumerable<ReplenOrderView> GetOrderViewNotCompleted(string search);
        IEnumerable<ReplenOrderView> GetOrderView();
        List<AvailableReplenOrdersView> GetAvailableOrders(StationView station);
        List<AvailableReplenOrdersView> GetAvailableOrders(StationView station, string search, bool serialPicking, bool showSkips = false);
        IEnumerable<ReplenOrderView> GetCompletedOrders(string search);
        ReplenOrder GetOrder();
        List<ReplenPickView> GetOrderLines();
        IEnumerable<ReplenOrderView> GetAvailableOrders(string search = "");
        List<ReplenPickView> GetOrderLines(List<BatchPosition> ordersToPick);
        IEnumerable<ReplenPickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum);
        IEnumerable<ReplenOrderView> GetRackOrders(string search);
        IEnumerable<RackReplenOrderView> GetRackOrdersView(int rackStationNumber, string search = @"");
        ReplenOrder GetOrderAndOrderDetails(int? orderId, int stationNumber);
    }
}
