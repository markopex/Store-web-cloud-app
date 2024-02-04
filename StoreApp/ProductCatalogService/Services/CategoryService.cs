using AutoMapper;
using Azure.Data.Tables;
using ProductCatalogService.Dto;
using ProductCatalogService.Models;
using ProductCatalogService.Interfaces;

namespace ProductCatalogService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly TableClient _tableClient;

        public CategoryService(IMapper mapper)
        {
            _mapper = mapper;
            string connectionString = "UseDevelopmentStorage=true;"; // For Azure Storage Emulator
            string tableName = "Categories";
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();

        }
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            List<CategoryEntity> entities = new List<CategoryEntity>();
            await foreach (var entity in _tableClient.QueryAsync<CategoryEntity>())
            {
                entities.Add(entity);
            }
            return _mapper.Map<List<CategoryDto>>(entities);
        }

        public async Task<CategoryDto> GetCategoryAsync(string id)
        {
            var entities = _tableClient.QueryAsync<CategoryEntity>(filter: $"RowKey eq '{id}'");
            await foreach (var entity in entities)
            {
                return _mapper.Map<CategoryDto>(entity); // Assuming id is unique, return the first match
            }
            return null; // or throw an appropriate exception if not found
        }
    }
}
