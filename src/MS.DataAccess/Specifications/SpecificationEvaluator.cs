using System.Linq;
using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Extensions;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Specifications
{
    public class SpecificationEvaluator<TEntity, TResult>
        where TEntity : Entity
        where TResult : new()
    {
        public static IQueryable<TResult> GetQuery(IQueryable<TEntity> inputQuery,
            ISpecification<TEntity, TResult> specification)
        {
            var query = SpecificationEvaluator<TEntity>.GetQuery(inputQuery, specification);

            // Apply selector
            var selectQuery = query.Select(specification.Selector);

            return selectQuery;
        }
    }

    public class SpecificationEvaluator<TEntity> where TEntity : Entity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> specification)
        {
            var query = inputQuery;

            if (specification.Criterias.Any())
                query = specification.Criterias.Aggregate(query,
                    (current, criteria) => current.Where(criteria));

            if (specification.OrderBy != null)
                query = query.OrderBy(specification.OrderBy);
            else if (specification.OrderByDescending != null)
                query = query.OrderByDescending(specification.OrderByDescending);

            query = query.SafePaginate(specification.PageIndex, specification.PageSize)
                .SafeAsNoTracking(specification.Tracking);

            // Include any string-based include statements
            query = specification.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            return query;
        }
    }
}