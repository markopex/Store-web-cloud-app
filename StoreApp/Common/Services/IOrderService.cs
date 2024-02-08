using Common.Dto;
using Common.Dto.Order;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Services
{
    public interface IOrderService: IService
    {
        Task<OrderCreatedSuccessfullyDto> CreateOrder(string customerEmail, CreateOrderDto orderDto);
        Task<List<Order>> GetOrdersByUser(string userEmail);
        Task<Order> GetOrder(string id);
        Task CaptureOrder(string token);
        Task CancelOrder(string token);
    }
}
