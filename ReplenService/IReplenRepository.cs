using NeutronData.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReplenService
{
    public interface IReplenRepository
    {
        Task<List<Replenishment>> GetReplenishments();
        Task DeleteReplenishmentsAboveSystemMin();
        Task DeleteReplenishment(OrderDetail orderDetail);
    }
}
