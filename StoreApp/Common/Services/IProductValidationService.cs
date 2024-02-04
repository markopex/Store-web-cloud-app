using System.ServiceModel;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Services
{
    [ServiceContract]
    public interface IProductValidationService: IService
    {
        [OperationContract]
        Task<bool> AreProductsValid(List<int> productIds);

        [OperationContract]
        Task<List<Product>> GetProductsByIds(List<int> productIds);
    }

}
