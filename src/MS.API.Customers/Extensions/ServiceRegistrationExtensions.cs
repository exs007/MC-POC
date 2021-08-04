using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MS.API.Customers.Contexts;
using MS.DataAccess.Context;
using MS.DataAccess.Extensions;
using MS.DataAccess.Setup;

namespace MS.API.Customers.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IEnvironmentResolver, CoreEnvironmentResolver>();
            services.AddSingleton<IDALTypesResolver, DALTypesResolver>(p =>
                new DALTypesResolver(new List<Assembly> {Assembly.GetExecutingAssembly()}));
            services.AddDbContext<CustomersContext>((provider, builder) =>
            {
                builder.UseSqlServer(configuration.GetConnectionString("CustomersDb"));
                provider.AddSEQDataContextLogger(builder);
            });
            services.AddDbContext<CustomersReadOnlyContext>((provider, builder) =>
            {
                builder.UseSqlServer(
                    DataContext.GenerateReadReplicaConnectionString(configuration.GetConnectionString("CustomersDb")));
                provider.AddSEQDataContextLogger(builder);
            });
            services.AddRespositoriesAndUOW(new List<Tuple<Type, Type>>
            {
                new(typeof(CustomersContext), typeof(CustomersReadOnlyContext))
            }, new List<Assembly> {Assembly.GetExecutingAssembly()});
            // custom repositories

            return services;
        }
    }
}