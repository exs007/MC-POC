using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.API.Customers.Contexts
{
    public class CustomersReadOnlyContext : DataContext
    {
        public CustomersReadOnlyContext(DbContextOptions<CustomersReadOnlyContext> options,
            IEnvironmentResolver environmentResolver, IDALTypesResolver typesResolver)
            : base(options, typesResolver, environmentResolver.Environment)
        {
        }
    }
}