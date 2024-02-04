using ProductCatalogService.Dto;

namespace ProductCatalogService.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetCategoryAsync(string id);
        Task<List<CategoryDto>> GetCategoriesAsync();
    }
}
