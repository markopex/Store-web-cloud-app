using Common.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProductService.Infrastructure.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Price).IsRequired();

            builder.Property(s => s.Name).IsRequired();

            builder.Property(s => s.Price).IsRequired();
            builder.HasOne(s => s.Category)
               .WithMany(s => s.Products)
               .HasForeignKey(s => s.CategoryId)
               .IsRequired();
        }
    }
}
