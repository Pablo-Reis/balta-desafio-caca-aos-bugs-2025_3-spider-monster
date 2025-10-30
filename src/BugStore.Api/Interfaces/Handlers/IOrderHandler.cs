using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Responses;

namespace BugStore.Interfaces.Handlers
{
    public interface IOrderHandler
    {
        Task<Response<Order>> CreateOrderAsync(CreateOrderRequest request);
        Task<Response<Order>> GetOrderByIdAsync(GetOrderByIdRequest request);
    }
}
