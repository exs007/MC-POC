using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Specifications
{
    public abstract class BaseSpecification<TEntity, TResult> : BaseSpecification<TEntity>,
        ISpecification<TEntity, TResult>
        where TEntity : Entity
        where TResult : new()
    {
        protected BaseSpecification(Expression<Func<TEntity, TResult>> selector) : this(null, selector)
        {
        }

        protected BaseSpecification(Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, TResult>> selector) : base(criteria, false)
        {
            Selector = selector;
        }

        public Expression<Func<TEntity, TResult>> Selector { get; }
    }

    public abstract class BaseSpecification<TEntity> : ISpecification<TEntity> where TEntity : Entity

    {
        protected BaseSpecification(bool tracking = true) : this(null)
        {
        }

        protected BaseSpecification(Expression<Func<TEntity, bool>> criteria, bool tracking = true)
        {
            Tracking = tracking;
            if (criteria != null) AddCriteria(criteria);
        }

        public ICollection<Expression<Func<TEntity, bool>>> Criterias { get; } =
            new List<Expression<Func<TEntity, bool>>>();

        public ICollection<string> IncludeStrings { get; } = new List<string>();

        public Expression<Func<TEntity, object>> OrderBy { get; private set; }
        public Expression<Func<TEntity, object>> OrderByDescending { get; private set; }
        public bool Tracking { get; }
        public int? PageIndex { get; private set; }
        public int? PageSize { get; private set; }

        protected virtual void AddCriteria(Expression<Func<TEntity, bool>> criteria)
        {
            Criterias.Add(criteria);
        }

        protected virtual void AddIncludes<TProperty>(
            Func<IncludeAggregator<TEntity>, IIncludeQuery<TEntity, TProperty>> includeGenerator)
        {
            var includeQuery = includeGenerator(new IncludeAggregator<TEntity>());
            foreach (var includeQueryPath in includeQuery.Paths) IncludeStrings.Add(includeQueryPath);
        }

        protected virtual void ApplyPaging(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        protected virtual void ApplyOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected virtual void ApplyOrderByDescending(Expression<Func<TEntity, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
    }
}