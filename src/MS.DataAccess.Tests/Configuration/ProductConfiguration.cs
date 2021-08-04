using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder
                .ToTable("Products")
                .HasKey(p => p.ProductId);
            builder.HasMany(p => p.Skus)
                .WithOne(p => p.Product)
                .HasForeignKey(p => p.ProductId)
                .HasPrincipalKey(p => p.ProductId);
        }
    }
}