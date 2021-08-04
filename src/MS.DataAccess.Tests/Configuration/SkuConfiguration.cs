using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Configuration
{
    public class SkuConfiguration : IEntityTypeConfiguration<Sku>
    {
        public void Configure(EntityTypeBuilder<Sku> builder)
        {
            builder
                .ToTable("Skus")
                .HasKey(p => p.SkuId);
        }
    }
}