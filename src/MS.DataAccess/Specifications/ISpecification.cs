using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Specifications
{
    public interface ISpecification<TEntity, TResult> : ISpecification<TEntity>
        where TEntity : Entity
        where TResult : new()
    {
        Expression<Func<TEntity, TResult>> Selector { get; }
    }

    public interface ISpecification<TEntity> where TEntity : Entity
    {
        ICollection<Expression<Func<TEntity, bool>>> Criterias { get; }
        ICollection<string> IncludeStrings { get; }

        Expression<Func<TEntity, object>> OrderBy { get; }
        Expression<Func<TEntity, object>> OrderByDescending { get; }
        bool Tracking { get; }
        int? PageIndex { get; }
        int? PageSize { get; }
    }
}