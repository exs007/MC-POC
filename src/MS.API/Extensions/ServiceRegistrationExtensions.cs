using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using MS.Core.Extensions;

namespace MS.API.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection AddAPI(this IServiceCollection services,
            IEnumerable<Assembly> assemblies)
        {
            services.AddLazyProvider();
            services.AddHttpContextAccessor();
            services.AddFluentValidation(assemblies);

            return services;
        }

        public static IServiceCollection AddFluentValidation(this IServiceCollection services,
            IEnumerable<Assembly> assemblies)
        {
            services.AddTransient<IValidatorFactory, ServiceProviderValidatorFactory>();
            AssemblyScanner.FindValidatorsInAssemblies(assemblies).ForEach(pair =>
            {
                services.Add(ServiceDescriptor.Transient(pair.InterfaceType, pair.ValidatorType));
            });

            return services;
        }
    }
}