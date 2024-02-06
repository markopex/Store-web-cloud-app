using Common.Models;
using OrderService.Dto;

namespace OrderService.Interfaces
{
    public interface IOrderService
    {
        Task<Order> AddOrder(string customerEmail, CreateOrderDto orderDto);
        List<Order> GetOrdersByUser(string userEmail);
        Order GetOrder(int id);
    }
}
