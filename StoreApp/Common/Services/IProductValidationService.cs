using System.ServiceModel;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Services
{
    [ServiceContract]
    public interface IProductValidationService: IService
    {
        [OperationContract]
        Task<bool> CheckIsBasketValid(Basket basket);

        [OperationContract]
        Task<List<Product>> GetProductsByIds(List<int> productIds);
    }

}
