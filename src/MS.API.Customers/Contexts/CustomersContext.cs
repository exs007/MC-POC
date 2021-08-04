using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.API.Customers.Contexts
{
    public class CustomersContext : DataContext
    {
        public CustomersContext(DbContextOptions<CustomersContext> options, IEnvironmentResolver environmentResolver,
            IDALTypesResolver typesResolver)
            : base(options, typesResolver, environmentResolver.Environment)
        {
        }
    }
}