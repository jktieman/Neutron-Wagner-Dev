using System.Collections.Generic;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IOrdersRepository
    {
        Order GetOrder(int id);
        
        IEnumerable<OrderView> GetReplenishmentOrders(string search = "");
        List<AvailableOrdersView> GetAvailableOrdersForInductionScreen(int areaId, string searchField, bool serialPicking);
        IEnumerable<OrderView> GetOrderViews(string orderStatus, string searchField);
        IEnumerable<OrderView> GetOrderView();
        IEnumerable<OrderView> GetAvailableOrderViews(string orderStatus = "1,2,3,4,5,7,8", string searchField = "");
        List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstationView);
        List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstationView, string search, bool serialPicking, bool showSkips = false);
        IEnumerable<Order> GetCompletedOrders();
        IEnumerable<OrderView> GetCompletedOrderViews(string orderStatus = "6", string searchField = "");
        Order GetOrder();
        List<PickView> GetOrderLines();
        IEnumerable<OrderView> GetReplenPickOrderViews(string orderStatus = "1,2,3,4,5,6,7,8,9", string searchField = "");
        IEnumerable<OrderView> GetAvailableOrders(string search = "");
        List<PickView> GetOrderLines(List<BatchPosition> ordersToPick);
        IEnumerable<PickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum);
        List<SkipView> GetSkippedOrders();
        IEnumerable<OrderView> GetRackOrders(string search);
        IEnumerable<RackOrderView> GetRackOrdersView(int rackStationNumber, string search = @"");
        Order GetOrderAndOrderDetails(int? orderId, int areaId);
        Order GetOrderWithOrderDetails(int orderId, int areaId);
        List<OrderDetail> GetOrderDetailsByOrderAndArea(int orderId, int areaId);
        string GetRoute(int orderId);
        IEnumerable<int> GetSizeCodesByArea(int areaId);

    }
}