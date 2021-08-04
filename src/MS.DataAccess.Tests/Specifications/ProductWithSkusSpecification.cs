using System.Collections.Generic;
using System.Linq;
using MS.DataAccess.Specifications;
using MS.DataAccess.Tests.Entities;

namespace MS.DataAccess.Tests.Specifications
{
    public class ProductWithSkusSpecification : BaseSpecification<Product>
    {
        public ProductWithSkusSpecification(IEnumerable<int> productIds)
        {
            AddCriteria(p => productIds.Contains(p.ProductId));
            AddIncludes(p => p.Include(p => p.Skus));
            ApplyOrderBy(p => p.ProductId);
        }
    }
}