using ProductService.Dto;

namespace ProductService.Interfaces
{
    public interface ICategoryService
    {
        CategoryDto GetCategory(string id);
        List<CategoryDto> GetCategories();
    }
}
