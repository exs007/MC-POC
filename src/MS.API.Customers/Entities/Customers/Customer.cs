using MS.API.Customers.Contexts;
using MS.DataAccess.Setup;

namespace MS.API.Customers.Entities.Customers
{
    public class Customer : Entity<CustomersContext>
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
    }
}