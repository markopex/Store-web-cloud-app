using ProductService.Dto;

namespace ProductService.Interfaces
{
    public interface IProductsService
    {
        ProductDto GetProduct(int id);
        Task<ProductDto> AddProduct(CreateUpdateProductDto productDto);
        List<ProductDto> GetProducts(); 
    }
}
