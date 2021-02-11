using System.Collections.Generic;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IOrdersRepository
    {
        Order GetOrder(int id);
        IEnumerable<OrderView> GetOrderViewNotCompleted(string search);
        IEnumerable<OrderView> GetOrderView();
        List<AvailableOrdersView> GetAvailableOrders(StationView station);
        List<AvailableOrdersView> GetAvailableOrders(StationView station, string search, bool serialPicking, bool showSkips = false);
        IEnumerable<OrderView> GetCompletedOrders(string search);
        Order GetOrder();
        List<PickView> GetOrderLines();
        IEnumerable<OrderView> GetAvailableOrders(string search = "");
        List<PickView> GetOrderLines(List<BatchPosition> ordersToPick);
        IEnumerable<PickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum);
        List<SkipView> GetSkippedOrders();
        IEnumerable<OrderView> GetRackOrders(string search);
        IEnumerable<RackOrderView> GetRackOrdersView(int rackStationNumber, string search = @"");
        Order GetOrderAndOrderDetails(int? orderId, int stationNumber);
    }
}