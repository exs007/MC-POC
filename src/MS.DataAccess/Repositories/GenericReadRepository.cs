using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Extensions;
using MS.DataAccess.Setup;
using MS.DataAccess.Specifications;

namespace MS.DataAccess.Repositories
{
    public class GenericReadRepository<TEntity> : IReadRepository<TEntity> where TEntity : Entity
    {
        private readonly Lazy<DbSet<TEntity>> _lazyDbSet;
        protected readonly IDataContext Context;

        public GenericReadRepository(IDataContext context)
        {
            Context = context;
            _lazyDbSet = new Lazy<DbSet<TEntity>>(() => context.Set<TEntity>(), LazyThreadSafetyMode.None);
        }

        protected internal DbSet<TEntity> DbSet => _lazyDbSet.Value;

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null,
            bool tracking = true)
        {
            return await BuildQuery(filter, orderBy, includes, tracking: tracking).FirstOrDefaultAsync();
        }

        public async Task<ICollection<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null,
            int? pageIndex = null, int? pageSize = null, bool tracking = true)
        {
            return await BuildQuery(filter, orderBy, includes, pageIndex, pageSize, tracking).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return await BuildQuery(filter, tracking: false).CountAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return await BuildQuery(filter, tracking: false).AnyAsync();
        }

        public async Task<TEntity> FirstOrDefaultAsync(ISpecification<TEntity> spec)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.FirstOrDefaultAsync();
        }

        public async Task<ICollection<TEntity>> ToListAsync(ISpecification<TEntity> spec)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<TEntity> spec)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.CountAsync();
        }

        public async Task<bool> AnyAsync(ISpecification<TEntity> spec)
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.AnyAsync();
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> spec)
            where TResult : new()
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.FirstOrDefaultAsync();
        }

        public async Task<ICollection<TResult>> ToListAsync<TResult>(ISpecification<TEntity, TResult> spec)
            where TResult : new()
        {
            var specificationResult = ApplySpecification(spec);
            return await specificationResult.ToListAsync();
        }

        private IQueryable<TEntity> BuildQuery(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = null,
            int? pageIndex = null,
            int? pageSize = null,
            bool tracking = true)
        {
            return DbSet
                .SafeInclude(includes)
                .SafeWhere(filter)
                .SafeOrderBy(orderBy)
                .SafePaginate(pageIndex, pageSize)
                .SafeAsNoTracking(tracking);
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            return SpecificationEvaluator<TEntity>.GetQuery(DbSet, spec);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> spec)
            where TResult : new()
        {
            return SpecificationEvaluator<TEntity, TResult>.GetQuery(DbSet, spec);
        }
    }
}