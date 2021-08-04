using System.Collections.Generic;
using MS.DataAccess.Setup;
using MS.DataAccess.Tests.Contexts;

namespace MS.DataAccess.Tests.Entities
{
    public class Product : Entity<InMemoryContext>
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public ICollection<Sku> Skus { get; set; }
    }
}