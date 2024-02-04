using Azure.Data.Tables;
using Common.Models;
using Common.Services;
using ProductCatalogService.Dto;
using ProductCatalogService.Models;
using System.Transactions;

namespace ProductCatalogService.Services
{
    public class ProductValidationService: IProductValidationService
    {
        private readonly TableClient _tableClient;

        public ProductValidationService()
        {
            string connectionString = "UseDevelopmentStorage=true;"; // For Azure Storage Emulator
            string tableName = "Products";
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<bool> AreProductsValid(List<int> productIds)
        {
            var validProductIds = new List<int>();

            // Assuming RowKey is used as the product ID
            foreach (var productId in productIds)
            {
                var response = _tableClient.QueryAsync<ProductEntity>(filter => filter.RowKey == productId.ToString());
                await foreach (var entity in response)
                {
                    validProductIds.Add(int.Parse(entity.RowKey));
                }
            }

            // Compare lists to ensure all input IDs were valid
            return validProductIds.Count == productIds.Count && !productIds.Except(validProductIds).Any();
        }

        public async Task<List<Product>> GetProductsByIds(List<int> productIds)
        {
            var products = new List<Product>();

            // Fetch each product by ID
            foreach (var productId in productIds)
            {
                var response = _tableClient.QueryAsync<ProductEntity>(filter => filter.RowKey == productId.ToString());
                await foreach (var entity in response)
                {
                    // Assuming you have configured AutoMapper or a similar mapping tool
                    var productDto = new Product // This should be mapped from entity to DTO
                    {
                        Id = int.Parse(entity.RowKey),
                        Name = entity.Name,
                        Price = entity.Price,
                        Description = entity.Description,
                        CategoryId = entity.CategoryId,
                        ImageUrl = entity.ImageUrl
                    };
                    products.Add(productDto);
                }
            }

            return products;
        }
    }
}
