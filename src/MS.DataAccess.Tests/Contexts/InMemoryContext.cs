using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Tests.Contexts
{
    public class InMemoryContext : DataContext
    {
        public InMemoryContext(DbContextOptions<InMemoryContext> options, IEnvironmentResolver environmentResolver,
            IDALTypesResolver typesResolver)
            : base(options, typesResolver, environmentResolver.Environment)
        {
        }
    }
}