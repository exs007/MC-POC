using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MS.DataAccess.Context;
using MS.DataAccess.Logging;
using MS.DataAccess.Repositories;
using MS.DataAccess.Setup;
using MS.DataAccess.UOW;
using ILogger = Serilog.ILogger;

namespace MS.DataAccess.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        public static IDataContext GetContextByCustomRepositoryType(this IServiceProvider serviceProvider,
            Type repositoryType)
        {
            var contextType =
                GetContextTypeByEntityType(repositoryType.BaseType?.GetGenericArguments().FirstOrDefault());
            return (IDataContext) serviceProvider.GetService(contextType);
        }

        public static void AddSEQDataContextLogger(this IServiceProvider provider, DbContextOptionsBuilder builder)
        {
            var seqLogger = provider.GetService<ILogger>();
            if (seqLogger != null)
            {
                var factory = new LoggerFactory(new[] {new DataContextTraceLoggerProvider(seqLogger)});
                builder.UseLoggerFactory(factory).EnableSensitiveDataLogging();
            }
        }

        public static IServiceCollection AddRespositoriesAndUOW(this IServiceCollection services,
            ICollection<Tuple<Type, Type>> contextConfigurationItems, IEnumerable<Assembly> assemblies)
        {
            // create UOW
            var uowInterface = typeof(IUnitOfWork<>);
            var uowImplementation = typeof(UnitOfWork<>);
            foreach (var contextType in contextConfigurationItems)
            {
                var dataType = new[] {contextType.Item1};
                var combinedInterfaceType = uowInterface.MakeGenericType(dataType);
                var combinedImplementationType = uowImplementation.MakeGenericType(dataType);
                services.AddScoped(combinedInterfaceType, p =>
                {
                    var context = (IDataContext) p.GetService(contextType.Item1);
                    var readReplicaContext = (IDataContext) p.GetService(contextType.Item2);
                    return Activator.CreateInstance(combinedImplementationType, context, readReplicaContext);
                });
            }

            // create repositories
            var entityTypes = GetEntityTypes(assemblies);
            var repositoryInterface = typeof(IRepository<>);
            var repositoryImplementation = typeof(GenericRepository<>);
            var readRepositoryInterface = typeof(IReadRepository<>);
            var readRepositoryImplementation = typeof(GenericReadRepository<>);
            foreach (var entityType in entityTypes)
            {
                var assignedContextType = GetContextTypeByEntityType(entityType);
                if (assignedContextType != null)
                {
                    var configurationItem =
                        contextConfigurationItems.FirstOrDefault(p => p.Item1 == assignedContextType);
                    if (configurationItem != null)
                    {
                        // register IRepository
                        var combinedInterfaceType = repositoryInterface.MakeGenericType(entityType);
                        var combinedImplementationType = repositoryImplementation.MakeGenericType(entityType);
                        services.AddScoped(combinedInterfaceType, p =>
                        {
                            var context = (IDataContext) p.GetService(configurationItem.Item1);
                            return Activator.CreateInstance(combinedImplementationType, context);
                        });

                        // register IReadRepository
                        var combinedReadInterfaceType = readRepositoryInterface.MakeGenericType(entityType);
                        var combinedReadImplementationType = readRepositoryImplementation.MakeGenericType(entityType);
                        services.AddScoped(combinedReadInterfaceType, p =>
                        {
                            var context = (IDataContext) p.GetService(configurationItem.Item2);
                            return Activator.CreateInstance(combinedReadImplementationType, context);
                        });
                    }
                }
            }

            return services;
        }

        private static Type GetContextTypeByEntityType(Type entityType)
        {
            return entityType.BaseType?.GetGenericArguments().FirstOrDefault();
        }

        private static ICollection<Type> GetEntityTypes(IEnumerable<Assembly> assemblies)
        {
            var toReturn = new List<Type>();
            foreach (var type in assemblies.SelectMany(p => p.GetConstructibleTypes()))
                if (type.BaseType != null &&
                    type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(Entity<>))
                    toReturn.Add(type);

            return toReturn;
        }
    }
}