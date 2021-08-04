using System.Collections.Generic;

namespace MS.DataAccess.Specifications
{
    public interface IIncludeQuery
    {
        Dictionary<IIncludeQuery, string> PathMap { get; }
        IncludeVisitor Visitor { get; }
        IEnumerable<string> Paths { get; }
    }

    public interface IIncludeQuery<TEntity, out TPreviousProperty> : IIncludeQuery
    {
    }
}