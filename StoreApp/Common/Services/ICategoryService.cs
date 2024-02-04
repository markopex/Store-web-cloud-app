using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Services
{
    [ServiceContract]
    public interface ICategoryService : IService
    {
        [OperationContract]
        Task<Category> GetCategory(string id);
        [OperationContract]
        Task<List<Category>> GetCategories();
    }
}
