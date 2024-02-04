using Common.Dto;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Services
{
    [ServiceContract]
    public interface IProductService : IService
    {
        [OperationContract]
        Task<Product> GetProduct(int id);
        [OperationContract]
        Task<Product> AddProduct(CreateUpdateProductDto productDto);
        [OperationContract]
        Task<List<Product>> GetProducts();
    }
}
