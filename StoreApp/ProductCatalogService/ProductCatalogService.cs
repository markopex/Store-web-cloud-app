using System.Fabric;
using Common.Dto;
using Common.Models;
using Common.Services;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ProductCatalogService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductCatalogService : StatefulService, ICategoryService, IProductService
    {
        private const string CategoryCollectionName = "categories";
        private const string ProductCollectionName = "products";

        public ProductCatalogService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<Category> GetCategory(string id)
        {
            IReliableDictionary<string, Category> categories = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Category>>(CategoryCollectionName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await categories.TryGetValueAsync(tx, id);
                return result.HasValue ? result.Value : null;
            }
        }

        public async Task<List<Category>> GetCategories()
        {
            var result = new List<Category>();
            IReliableDictionary<string, Category> categories = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Category>>(CategoryCollectionName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var allCategories = await categories.CreateEnumerableAsync(tx);

                using (var enumerator = allCategories.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(System.Threading.CancellationToken.None))
                    {
                        result.Add(enumerator.Current.Value);
                    }
                }
            }

            return result;
        }

        public async Task<Product> AddProduct(CreateUpdateProductDto productDto)
        {
            var categories = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Category>>(CategoryCollectionName);
            var products = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Product>>(ProductCollectionName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var categoryExists = await categories.ContainsKeyAsync(tx, productDto.CategoryId);
                if (!categoryExists)
                {
                    throw new KeyNotFoundException($"Category with ID {productDto.CategoryId} does not exist.");
                }
                var random = new Random();
                var product = new Product
                {
                    Id = random.Next(),
                    Name = productDto.Name,
                    Price = productDto.Price,
                    Description = productDto.Description,
                    CategoryId = productDto.CategoryId,
                    ImageUrl = productDto.ImageUrl
                };

                // Assuming you have a mechanism to generate unique product IDs
                await products.AddAsync(tx, product.Id, product);
                await tx.CommitAsync();

                return product;
            }
        }

        public async Task<Product> GetProduct(int id)
        {
            var products = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Product>>(ProductCollectionName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var result = await products.TryGetValueAsync(tx, id);
                return result.HasValue ? result.Value : null;
            }
        }

        public async Task<List<Product>> GetProducts()
        {
            var result = new List<Product>();
            var products = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Product>>(ProductCollectionName);

            using (var tx = this.StateManager.CreateTransaction())
            {
                var allProducts = await products.CreateEnumerableAsync(tx);

                using (var enumerator = allProducts.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(System.Threading.CancellationToken.None))
                    {
                        result.Add(enumerator.Current.Value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceReplicaListeners();
        }
    }
}
