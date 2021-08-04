using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MS.API.Customers.Entities.Customers;

namespace MS.API.Customers.Configuration.Customers
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder
                .ToTable("Customers")
                .HasKey(p => p.CustomerId);
        }
    }
}