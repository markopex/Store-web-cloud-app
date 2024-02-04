using Common.Models.Product;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using ProductService.Infrastructure;
using System.Transactions;

namespace ProductService.Services
{
    public class ProductValidationService: IProductValidationService
    {
        private readonly ProductsDbContext _dbContext;

        public ProductValidationService(ProductsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AreProductsValid(List<int> productIds)
        {
            var validProductIds = new List<int>();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                validProductIds = await _dbContext.Products
                    .Where(product => productIds.Contains(product.Id))
                    .Select(product => product.Id)
                    .ToListAsync();

                scope.Complete();
            }

            // Compare lists to ensure all input IDs were valid
            return validProductIds.Count == productIds.Count && !productIds.Except(validProductIds).Any();
        }

        public async Task<List<Product>> GetProductsByIds(List<int> productIds)
        {
            var products = new List<Product>();

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                products = await _dbContext.Products
                    .Where(product => productIds.Contains(product.Id))
                    .ToListAsync();

                scope.Complete();
            }

            return products;
        }
    }
}
