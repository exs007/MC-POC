using MS.DataAccess.Setup;
using MS.DataAccess.Tests.Contexts;

namespace MS.DataAccess.Tests.Entities
{
    public class Sku : Entity<InMemoryContext>
    {
        public int SkuId { get; set; }
        public string Code { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}