using System.Collections.Generic;
using Neutron.Models;
using NeutronData.Models;
using NeutronData.ModelViews;

namespace NeutronData.Interfaces
{
    public interface IOrdersRepository
    {
        Order GetOrder(int id);
        List<AvailableOrdersView> GetAvailableOrdersForInductionScreen(WorkstationView workstationView, string searchField);
        IEnumerable<OrderView> GetOrderViews(string orderStatus, string searchField);
        IEnumerable<OrderView> GetOrderView();
        List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstationView);
        List<AvailableOrdersView> GetAvailableOrders(WorkstationView workstationView, string search, bool serialPicking, bool showSkips = false);
        IEnumerable<OrderView> GetCompletedOrders(string search);
        Order GetOrder();
        List<PickView> GetOrderLines();
        IEnumerable<OrderView> GetAvailableOrders(string search = "");
        List<PickView> GetOrderLines(List<BatchPosition> ordersToPick);
        IEnumerable<PickView> GetPickViewsByItem(List<BatchPosition> ordersToPick, string partNum);
        List<SkipView> GetSkippedOrders();
        IEnumerable<OrderView> GetRackOrders(string search);
        IEnumerable<RackOrderView> GetRackOrdersView(int rackStationNumber, string search = @"");
        Order GetOrderAndOrderDetails(int? orderId, int[] areaIdsForThisWorkstation);
        Order GetOrderAndOrderDetails(int? orderId, int areaId);
    }
}