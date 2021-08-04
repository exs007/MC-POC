using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MS.DataAccess.Extensions;

namespace MS.DataAccess.Setup
{
    public class DALTypesResolver : IDALTypesResolver
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public DALTypesResolver(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<TypeInfo> GetConstructibleTypes()
        {
            return _assemblies.SelectMany(p => p.GetConstructibleTypes()).ToList();
        }
    }
}