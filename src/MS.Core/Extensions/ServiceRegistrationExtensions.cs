using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MS.Core.DI;
using MS.Core.Options;

namespace MS.Core.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static void AddCoreOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<CommonOptions>()
                .Configure<IConfiguration>((options, config) =>
                {
                    options.Environment = config.GetValue<string>("ENVIRONMENT");
                });
        }

        public static void AddLazyProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof(Lazy<>), typeof(LazyProvider<>));
        }
    }
}