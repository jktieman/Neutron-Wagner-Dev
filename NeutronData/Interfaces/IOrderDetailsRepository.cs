using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Interfaces;
public interface IOrderDetailsRepository
{
    void SetOrderDetailStatusToPicking(int[] currentOrderDetailsIds);
    List<OrderDetailsView> GetOrderDetailsViewByOrder(int orderId);
    List<OrderDetail> GetOrderDetailsByOrderAndWorkstation(int orderId, WorkstationView workstationView);
    Order GetOrder(int orderDetailId);
    void SetOrderDetailStatus(int[] orderDetailIds, int statusId);
    //Task SaveAsync();
    //void Save();

}
