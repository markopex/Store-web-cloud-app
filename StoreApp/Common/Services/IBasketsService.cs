﻿using Common.Dto;
using Common.Dto.Order;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Services
{
    [ServiceContract]
    public interface IBasketsService : IService
    {
        [OperationContract]
        Task<Basket> AddItemToBasketAsync(string customerId, BasketItemDto item);
        [OperationContract]
        Task<Basket> GetBasketAsync(string customerId);
        [OperationContract]
        Task<Basket> SetBasketAsync(string customerId, BasketDto dto);
        [OperationContract]
        Task<OrderCreatedSuccessfullyDto> CheckoutAsync(string customerId, string address, string comment, string paymentMethod);
    }
}
