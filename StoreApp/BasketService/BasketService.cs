using AutoMapper;
using BasketService.Mapping;
using Common.Dto;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using System.Fabric;
using Common.Services;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Dto.Order;

namespace BasketService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BasketService : StatefulService, IBasketsService
    {
        private readonly string productValidatorServicePath = @"fabric:/StoreApp/ProductCatalogService";
        private readonly string orderServicePath = @"fabric:/StoreApp/OrderService";
        private readonly IMapper _mapper = (new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        })).CreateMapper();
        public BasketService(StatefulServiceContext context)
            : base(context)
        { }

        private IProductValidationService? _productValidationService
        {
            get
            {
                return ServiceProxy.Create<IProductValidationService>(new Uri(productValidatorServicePath));
            }
        }

        private IOrderService? GetOrderService(string customerId)
        {
            return ServiceProxy.Create<IOrderService>(new Uri(orderServicePath), new ServicePartitionKey(customerId.GetHashCode()));
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<Basket> GetBasketAsync(string customerId)
        {
            var baskets = await StateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            using (var tx = StateManager.CreateTransaction())
            {
                var result = await baskets.TryGetValueAsync(tx, customerId);
                if (result.HasValue)
                {
                    return _mapper.Map<Basket>(result.Value);
                }
                else
                {
                    return new Basket()
                    {
                        BasketItems = new List<BasketItem>()
                    };
                }
            }
        }

        public async Task<Basket> AddItemToBasketAsync(string customerId, BasketItemDto dto)
        {
            var baskets = await StateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            var item = _mapper.Map<BasketItem>(dto);
            using (var tx = StateManager.CreateTransaction())
            {
                var basket = await baskets.GetOrAddAsync(tx, customerId, new Basket()
                {
                    BasketItems = new List<BasketItem> { }
                });
                var basketItem = basket.BasketItems.Find(i => i.ProductId == item.ProductId);
                var updatedQuantity = new BasketItem()
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                };
                if (basketItem == null)
                {
                    basket.BasketItems.Add(item);
                }
                else
                {
                    basketItem.Quantity += item.Quantity;
                }
                var basketDto = _mapper.Map<BasketDto>(basket);
                bool isBasketValid = await _productValidationService.CheckIsBasketValid(basketDto);
                if (!isBasketValid)
                {
                    tx.Abort();
                    throw new InvalidOperationException();
                }
                await baskets.SetAsync(tx, customerId, basket);

                await tx.CommitAsync();

                return basket;
            }
        }

        public async Task<Basket> SetBasketAsync(string customerId, BasketDto dto)
        {
            var baskets = await StateManager.GetOrAddAsync<IReliableDictionary<string, Basket>>("baskets");
            using (var tx = StateManager.CreateTransaction())
            {
                var basket = _mapper.Map<Basket>(dto);
                var basketDto = _mapper.Map<BasketDto>(basket);
                bool isBasketValid = await _productValidationService.CheckIsBasketValid(basketDto);
                if (!isBasketValid)
                {
                    throw new InvalidOperationException();
                }
                await baskets.SetAsync(tx, customerId, basket);

                await tx.CommitAsync();

                return basket;
            }
        }

        public async Task<OrderCreatedSuccessfullyDto> CheckoutAsync(string customerId, string address, string comment, string paymentMethod)
        {
            try
            {
                var basket = await GetBasketAsync(customerId);
                var retVal = await GetOrderService(customerId).CreateOrder(customerId, new CreateOrderDto()
                {
                    OrderDetails = basket.BasketItems.Select(i => new CreateOrderDetailDto() {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                    }).ToList(),
                    Comment = comment,
                    Address = address,
                    PaymentMethod = paymentMethod,
                });
                // clear basket
                await SetBasketAsync(customerId, new BasketDto() { BasketItems = new List<BasketItemDto>() });
                return retVal;
            } catch (Exception ex)
            {
                return null;
            }
        }
    }
}
