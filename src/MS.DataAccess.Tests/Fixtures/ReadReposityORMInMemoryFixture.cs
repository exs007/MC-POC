using System.Linq;
using AutoFixture;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Fixtures
{
    public class ReadReposityORMInMemoryFixture : ORMInMemoryFixture
    {
        public ReadReposityORMInMemoryFixture()
        {
            AutoFixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList().ForEach(b => AutoFixture.Behaviors.Remove(b));
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var products = AutoFixture.CreateMany<Product>(10).OrderBy(p => p.ProductId).ToList();
            foreach (var product in products) product.Skus = AutoFixture.CreateMany<Sku>(5).ToList();
            InMemoryContext.AddRange(products);
            InMemoryContext.SaveChanges();
        }
    }
}