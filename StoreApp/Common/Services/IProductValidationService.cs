using System.ServiceModel;
using Common.Dto;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Services
{
    [ServiceContract]
    public interface IProductValidationService: IService
    {
        [OperationContract]
        Task<bool> CheckIsBasketValid(BasketDto basket);

        [OperationContract]
        Task<List<Product>> GetProductsByIds(List<int> productIds);
        [OperationContract]
        Task<List<OrderDetail>> ReserveProducts(List<CreateOrderDetailDto> orderDetailDtos);

        [OperationContract]
        Task<List<OrderDetail>> CancelReservationOnProducts(List<OrderDetail> orderDetailDtos);
    }

}
