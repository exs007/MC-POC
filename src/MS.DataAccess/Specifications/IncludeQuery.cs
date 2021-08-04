using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.DataAccess.Specifications
{
    public class IncludeQuery<TEntity, TPreviousProperty> : IIncludeQuery<TEntity, TPreviousProperty>
    {
        public IncludeQuery(Dictionary<IIncludeQuery, string> pathMap)
        {
            PathMap = pathMap;
        }

        public Dictionary<IIncludeQuery, string> PathMap { get; } = new();
        public IncludeVisitor Visitor { get; } = new();

        public IEnumerable<string> Paths
        {
            get
            {
                // exclude duplicates
                return PathMap.Select(p => p.Value).Distinct(StringComparer.InvariantCultureIgnoreCase);
            }
        }
    }
}