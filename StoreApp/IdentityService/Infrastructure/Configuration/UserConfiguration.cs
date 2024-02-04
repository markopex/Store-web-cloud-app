using Common.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(s => s.Email);
            builder.Property(s => s.Username)
                   .IsRequired()
                   .IsUnicode();
            builder.Property(s => s.Email)
                   .IsRequired();
            builder.Property(s => s.Birthday)
                   .IsRequired();
            builder.Property(s => s.Address)
                   .IsRequired();
            builder.Property(s => s.FirstName)
                   .IsRequired();
            builder.Property(s => s.LastName)
                   .IsRequired();
        }
    }
}
