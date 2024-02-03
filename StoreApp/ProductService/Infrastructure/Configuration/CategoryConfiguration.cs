using Common.Models.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProductService.Infrastructure.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Name).IsRequired();

            builder.Property(s => s.Description).IsRequired();
            builder.HasMany(s => s.Products)
               .WithOne(s => s.Category)
               .HasForeignKey(s => s.CategoryId)
               .IsRequired();
        }
    }
}
