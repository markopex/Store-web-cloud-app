using Azure.Data.Tables;
using Azure;

namespace ProductCatalogService.Models
{
    public class ProductEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // CategoryId as string for logical partitioning
        public string RowKey { get; set; } // Id as string for unique identification within a partition
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
}
