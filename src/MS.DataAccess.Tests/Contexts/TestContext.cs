using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Contexts
{
    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("DataSource=:memory:");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sku>()
                .ToTable("Skus")
                .HasKey(p => p.SkuId);
            modelBuilder.Entity<Product>()
                .ToTable("Products")
                .HasKey(p => p.ProductId);
            modelBuilder.Entity<Product>().HasMany(p => p.Skus)
                .WithOne(p => p.Product)
                .HasForeignKey(p => p.ProductId)
                .HasPrincipalKey(p => p.ProductId);
        }
    }
}