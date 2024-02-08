using Azure.Data.Tables;
using Common.Dto;
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

        public Task<List<OrderDetail>> CancelReservationOnProducts(List<OrderDetail> orderDetailDtos)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckIsBasketValid(BasketDto basket)
        {
            var productIds = basket.BasketItems.Select(x => x.ProductId).ToList();
            var validProductIds = new List<int>();

            // Assuming RowKey is used as the product ID
            foreach (var product in basket.BasketItems)
            {
                var productId = product.ProductId;
                var response = _tableClient.QueryAsync<ProductEntity>(filter => filter.RowKey == productId.ToString());
                await foreach (var entity in response)
                {
                    if (entity.Quantity < product.Quantity) return false;
                    validProductIds.Add(int.Parse(entity.RowKey));
                }
            }
            return true;
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
                        ImageUrl = entity.ImageUrl,
                        Quantity = entity.Quantity,
                    };
                    products.Add(productDto);
                }
            }

            return products;
        }

        public Task<List<OrderDetail>> ReserveProducts(List<CreateOrderDetailDto> orderDetailDtos)
        {
            throw new NotImplementedException();
        }
    }
}
