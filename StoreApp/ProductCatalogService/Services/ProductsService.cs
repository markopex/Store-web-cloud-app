using AutoMapper;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Common.Models;
using ProductCatalogService.Dto;
using ProductCatalogService.Interfaces;
using ProductCatalogService.Models;

namespace ProductCatalogService.Services
{
    public class ProductService : IProductsService
    {
        private readonly IMapper _mapper;
        private readonly TableClient _tableClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobContainerName;

        public ProductService(IMapper mapper)
        {
            _mapper = mapper;
            _blobContainerName = "productimages";//configuration["BlobContainerName"];
            //var tableName = configuration["ProductsTableName"];
            string connectionString = "UseDevelopmentStorage=true;"; // For Azure Storage Emulator
            string tableName = "Products";
            //var connectionString = configuration["AzureTableStorageConnectionString"];

            _blobServiceClient = new BlobServiceClient(connectionString);
            _tableClient = new TableClient(connectionString, tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task<Product> AddProduct(CreateUpdateProductDto createProductDto)
        {
            if (!createProductDto.ImageFile.ContentType.Contains("image"))
            {
                throw new Exception("File is not an image");
            }

            var productEntity = _mapper.Map<ProductEntity>(createProductDto);
            // Assign RowKey and PartitionKey
            productEntity.PartitionKey = "0";// createProductDto.CategoryId.ToString();
            productEntity.RowKey = Guid.NewGuid().ToString().GetHashCode().ToString();

            var imageUrl = await SavePostImageAsync(createProductDto.ImageFile, productEntity.RowKey);
            productEntity.ImageUrl = imageUrl;

            await _tableClient.AddEntityAsync(productEntity);

            return _mapper.Map<Product>(productEntity);
        }

        private async Task<string> SavePostImageAsync(IFormFile formFile, string productName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(productName + ".png");

            using (var stream = formFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
        }

        public Product GetProduct(int id)
        {
            var query = _tableClient.Query<ProductEntity>(filter => filter.RowKey == id.ToString()).FirstOrDefault();
            return _mapper.Map<Product>(query);
        }

        public List<Product> GetProducts()
        {
            var query = _tableClient.Query<ProductEntity>().ToList();
            return _mapper.Map<List<Product>>(query);
        }

        public async Task<Stream> GetProductImageAsync(string productName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

            var blobClient = blobContainerClient.GetBlobClient(productName + ".png");

            if (await blobClient.ExistsAsync())
            {
                var blobDownloadInfo = await blobClient.DownloadAsync();

                return blobDownloadInfo.Value.Content;
            }
            else
            {
                return null;
            }
        }
    }
}
