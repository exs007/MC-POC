using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Extensions
{
    public static class DataExtensions
    {
        public static ModelBuilder ApplyConfigurationsFromAssemblyForContext(this ModelBuilder builder,
            IDALTypesResolver dALTypesResolver,
            DataContext context)
        {
            var applyEntityConfigurationMethod = GetApplyEntityConfigurationMethod();

            var entityTypeConfiguration = typeof(IEntityTypeConfiguration<>);
            var directSQLEntity = typeof(DirectSQLEntity<>);
            var directSQLEntitiesMappedWithCustomConfig = new List<Type>();
            var contextType = context.GetType();
            var configurationWithEnvironmentType = typeof(ConfigurationWithEnvironment<>);
            var constructibleTypes = dALTypesResolver.GetConstructibleTypes();

            // apply IEntityTypeConfiguration
            SetupEntitiesWithConfiguration(builder, constructibleTypes, context, entityTypeConfiguration, contextType,
                applyEntityConfigurationMethod, configurationWithEnvironmentType, directSQLEntity,
                directSQLEntitiesMappedWithCustomConfig);

            // if not in specific configuration, try to add default configuration for types inherited from DirectSQLEntity<T>
            SetupDirectSQLEntities(builder, constructibleTypes, directSQLEntitiesMappedWithCustomConfig,
                directSQLEntity, contextType);

            return builder;
        }

        private static void SetupEntitiesWithConfiguration(ModelBuilder builder,
            IEnumerable<TypeInfo> constructibleTypes, DataContext context,
            Type entityTypeConfiguration, Type contextType, MethodInfo applyEntityConfigurationMethod,
            Type configurationWithEnvironmentType, Type directSQLEntity,
            List<Type> directSQLEntitiesMappedWithCustomConfig)
        {
            foreach (var type in constructibleTypes)
            foreach (var @interface in type.GetInterfaces())
            {
                if (!@interface.IsGenericType) continue;

                if (@interface.GetGenericTypeDefinition() == entityTypeConfiguration)
                {
                    var entityType = @interface.GetGenericArguments()[0];
                    var baseEntityType = entityType.BaseType;
                    if (baseEntityType == null || !baseEntityType.IsGenericType) continue;
                    var entityContextType = baseEntityType.GetGenericArguments()[0];

                    if (entityContextType.IsAssignableFrom(contextType))
                    {
                        var target =
                            applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        var configuration = GetConfiguration(context, type, configurationWithEnvironmentType);
                        target.Invoke(builder, new[] {configuration});
                        if ((baseEntityType?.IsGenericType ?? false) &&
                            baseEntityType.GetGenericTypeDefinition() == directSQLEntity)
                            directSQLEntitiesMappedWithCustomConfig.Add(entityType);
                        break;
                    }
                }
            }
        }

        private static object GetConfiguration(DataContext context, TypeInfo type,
            Type configurationWithEnvironmentType)
        {
            object configuration;
            if (type.BaseType != null && type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == configurationWithEnvironmentType)
                configuration = Activator.CreateInstance(type, context.Environment);
            else
                configuration = Activator.CreateInstance(type);
            return configuration;
        }

        private static MethodInfo GetApplyEntityConfigurationMethod()
        {
            var applyEntityConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                         && e.ContainsGenericParameters
                         && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition()
                         == typeof(IEntityTypeConfiguration<>));
            return applyEntityConfigurationMethod;
        }

        private static void SetupDirectSQLEntities(ModelBuilder builder, IEnumerable<TypeInfo> constructibleTypes,
            List<Type> directSQLEntitiesMappedWithCustomConfig, Type directSQLEntity, Type contextType)
        {
            foreach (var type in constructibleTypes)
            {
                if (directSQLEntitiesMappedWithCustomConfig.Contains(type)) continue;

                var baseType = type.BaseType;
                if ((baseType?.IsGenericType ?? false) && baseType.GetGenericTypeDefinition() == directSQLEntity)
                {
                    var entityContextType = baseType.GetGenericArguments()[0];
                    if (entityContextType.IsAssignableFrom(contextType)) builder.Entity(type).HasNoKey();
                }
            }
        }

        internal static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
        {
            return assembly.GetLoadableDefinedTypes().Where(
                t => !t.IsAbstract
                     && !t.IsGenericTypeDefinition);
        }

        internal static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }

        internal static IQueryable<T> SafeInclude<T>(this IQueryable<T> query, Func<IQueryable<T>, IQueryable<T>> func)
            where T : class
        {
            return func == null ? query : func(query);
        }

        internal static IQueryable<T> SafeWhere<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression)
        {
            return expression == null ? query : query.Where(expression);
        }

        internal static IQueryable<T> SafeOrderBy<T>(this IQueryable<T> query,
            Func<IQueryable<T>, IOrderedQueryable<T>> func)
        {
            return func == null ? query : func(query);
        }

        internal static IQueryable<T> SafePaginate<T>(this IQueryable<T> query, int? pageIndex, int? pageSize)
        {
            if (pageIndex.HasValue && !pageSize.HasValue ||
                !pageIndex.HasValue && pageSize.HasValue)
                throw new Exception("Pagination improperly setup to function properly");

            if (!pageIndex.HasValue) return query;

            return query.Skip(pageSize.Value * pageIndex.Value).Take(pageSize.Value);
        }

        internal static IQueryable<T> SafeAsNoTracking<T>(this IQueryable<T> query, bool tracking)
            where T : class
        {
            return tracking ? query : query.AsNoTracking();
        }
    }
}