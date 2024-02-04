using BasketService.Dto;
using Common.Models.Basket;

namespace BasketService.Interfaces
{
    public interface IBasketsService
    {
        Task<Basket> AddItemToBasketAsync(string customerId, BasketItemDto item);
        Task<BasketDto> GetBasketAsync(string customerId);
        Task<Basket> SetBasketAsync(string customerId, BasketDto dto);
    }
}