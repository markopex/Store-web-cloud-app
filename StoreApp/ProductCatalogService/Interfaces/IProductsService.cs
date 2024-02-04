using Common.Models;
using ProductCatalogService.Dto;

namespace ProductCatalogService.Interfaces
{
    public interface IProductsService
    {
        Product GetProduct(int id);
        Task<Product> AddProduct(CreateUpdateProductDto productDto);
        List<Product> GetProducts(); 
    }
}
