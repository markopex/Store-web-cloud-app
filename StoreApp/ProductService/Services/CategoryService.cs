using AutoMapper;
using ProductService.Dto;
using ProductService.Infrastructure;
using ProductService.Interfaces;

namespace ProductService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly ProductsDbContext _dbContext;

        public CategoryService(IMapper mapper, ProductsDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }
        public List<CategoryDto> GetCategories()
        {
            return _mapper.Map<List<CategoryDto>>(_dbContext.Categories.ToList());
        }

        public CategoryDto GetCategory(string id)
        {
            return _mapper.Map<CategoryDto>(_dbContext.Categories.Find(id));
        }
    }
}
