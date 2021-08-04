using System.Collections.Generic;
using System.Reflection;

namespace MS.DataAccess.Setup
{
    public interface IDALTypesResolver
    {
        IEnumerable<TypeInfo> GetConstructibleTypes();
    }
}