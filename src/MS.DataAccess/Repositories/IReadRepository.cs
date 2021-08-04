using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MS.DataAccess.Setup;
using MS.DataAccess.Specifications;

namespace MS.DataAccess.Repositories
{
    public interface IReadRepository<TEntity> where TEntity : Entity
    {
        Task<ICollection<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null,
            int? pageIndex = null,
            int? pageSize = null,
            bool tracking = true);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null,
            bool tracking = true);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null);

        Task<TEntity> FirstOrDefaultAsync(ISpecification<TEntity> spec);

        Task<ICollection<TEntity>> ToListAsync(ISpecification<TEntity> spec);

        Task<int> CountAsync(ISpecification<TEntity> spec);

        Task<bool> AnyAsync(ISpecification<TEntity> spec);

        Task<TResult> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> spec) where TResult : new();

        Task<ICollection<TResult>> ToListAsync<TResult>(ISpecification<TEntity, TResult> spec) where TResult : new();
    }
}