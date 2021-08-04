using AutoMapper;
using MS.API.Customers.Entities.Customers;
using MS.API.Customers.Models.Customers;

namespace MS.API.Customers.AutomapperProfiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerViewModel>();
            CreateMap<Customer, CustomerListModel>();
            CreateMap<CustomerEditModel, Customer>()
                .ForMember(m => m.CustomerId, opt => opt.Ignore());
        }
    }
}