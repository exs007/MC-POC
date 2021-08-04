using MS.DataAccess.Specifications;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Specifications
{
    public class ProductShortListSpecification : BaseSpecification<Product, ProductShortList>
    {
        public ProductShortListSpecification() : base(p => new ProductShortList
        {
            ProductId = p.ProductId
        })
        {
        }
    }
}