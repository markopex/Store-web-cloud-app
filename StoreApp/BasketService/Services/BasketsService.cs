using AutoMapper;
using BasketService.Dto;
using BasketService.Interfaces;
using Common.Models.Basket;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace BasketService.Services
{
    public class BasketsService : IBasketsService
    {
        private readonly IMapper _mapper;
        private readonly IReliableStateManager _stateManager;

        public BasketsService(IReliableStateManager stateManager, IMapper mapper)
        {
            _stateManager = stateManager;
            _mapper = mapper;
        }

        public async Task<BasketDto> GetBasketAsync(string customerId)
        {
            var baskets = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            using (var tx = _stateManager.CreateTransaction())
            {
                var result = await baskets.TryGetValueAsync(tx, customerId);
                if (result.HasValue)
                {
                    return _mapper.Map<BasketDto>(result.Value);
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Basket> AddItemToBasketAsync(string customerId, BasketItemDto dto)
        {
            var baskets = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            var item = _mapper.Map<BasketItem>(dto);
            using (var tx = _stateManager.CreateTransaction())
            {
                var basket = await baskets.GetOrAddAsync(tx, customerId, new Basket());
                var basketItem = basket.BasketItems.Find(i => i.ProductId == item.ProductId);
                if (basketItem == null)
                {
                    basket.BasketItems.Add(item);
                }
                else
                {
                    basketItem.Quantity += item.Quantity;
                }
                await baskets.SetAsync(tx, customerId, basket);

                await tx.CommitAsync();

                return basket;
            }
        }

        public async Task<Basket> SetBasketAsync(string customerId, BasketDto dto)
        {
            var baskets = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            using (var tx = _stateManager.CreateTransaction())
            {
                var basket = _mapper.Map<Basket>(dto);
                await baskets.SetAsync(tx, customerId, basket);

                await tx.CommitAsync();

                return basket;
            }
        }
    }
}
