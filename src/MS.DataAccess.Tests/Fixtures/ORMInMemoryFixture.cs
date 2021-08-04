using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MS.Core.Extensions;
using MS.DataAccess.Extensions;
using MS.DataAccess.Setup;
using MS.DataAccess.Tests.Contexts;
using Serilog;

namespace MS.DataAccess.Tests.Fixtures
{
    public class ORMInMemoryFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }
        public InMemoryContext InMemoryContext { get; }
        public Fixture AutoFixture { get; }

        public ORMInMemoryFixture()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var services = new ServiceCollection();
            services.AddCoreOptions();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(p => p.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
                .Returns(loggerMock.Object);
            services.AddSingleton(loggerMock.Object);
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<IEnvironmentResolver, CoreEnvironmentResolver>();
            services.AddSingleton<IDALTypesResolver, DALTypesResolver>(p =>
                new DALTypesResolver(new List<Assembly> {Assembly.GetExecutingAssembly()}));
            services.AddDbContext<InMemoryContext>((provider, builder) =>
            {
                builder.UseSqlite(CreateInMemoryDatabase());
                provider.AddSEQDataContextLogger(builder);
            }, ServiceLifetime.Transient); // force a new context to prevent using cached values
            services.AddRespositoriesAndUOW(new List<Tuple<Type, Type>>
            {
                // the same context as readonly context
                new(typeof(InMemoryContext), typeof(InMemoryContext))
            }, new List<Assembly> {Assembly.GetExecutingAssembly()});
            ServiceProvider = services.BuildServiceProvider();
            InMemoryContext = ServiceProvider.GetService<InMemoryContext>();
            InMemoryContext.Database.EnsureCreated();
            AutoFixture = new Fixture();
        }

        private DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        public void Dispose()
        {
            ServiceProvider.Dispose();
        }
    }
}