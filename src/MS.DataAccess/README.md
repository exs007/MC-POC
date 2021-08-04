# EF CORE data access layer

EF Core s a lightweight, extensible, object-relational mapper(https://github.com/dotnet/efcore). EF allows to work directly with DB structure and create one composite data access layer.
DAL includes EF CORE and additional wrappers which are based on generic repository, UOW, specification patters. These wrappers provides an easy way to work 
with mapping and operations and enforce common rules/styles during working with EF.

## How to start

### Mapping

Create a context, each DB should have it’s own dedicated context:
```
public class {CONTEXT_TYPE}: DataContext
{
    public {CONTEXT_TYPE}(DbContextOptions<{CONTEXT_TYPE}> options) : base(options)
    {
    }
}
```

Context should be registered in DI, DAL provides helpers to enable Repository and UOW, example:
```
services.AddDbContext<{CONTEXT_TYPE}>((provider, builder) =>
{
    builder.UseSqlServer(configuration.GetConnectionString("{DB_CONNECTION_STRING}"));
    AddSEQDataContextLogger(provider, builder);
});
services.AddRespositoriesAndUOW(new List<Type>()
{
    typeof({CONTEXT_TYPE}), typeof(Aph01Context)
});
```

Add entities into "Entities" folder, each entity represents a class which is mapped to a table, view or a sproc.
Table/View entity should be inherited from:
```
public class Customer : Entity<{CONTEXT_TYPE}>
```

Sproc result should be inherited from:
```
public class Customer : DirectSQLEntity<{CONTEXT_TYPE}>
```

Change tracking is only working for types inherited from Entity<{CONTEXT_TYPE}>
Add mapping for each table/view entity into Configuration folder. Mapping represents how a class is mapped to a specific table/view, class mapper should implement:
```
public class CustomerConfiguration: IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder
            .ToTable("Customers")
            .HasKey(p => p.Id);
        builder
            .Property(b => b.Id)
            .ValueGeneratedOnAdd();
    }
}
```

Place all configuration into appropriate mapping class.
Only use "Fluent API", describe your mapping by calling extension methods on EntityTypeBuilder.
Place all mapping into mapping classes, don't place any mapping in OnModelCreating(ModelBuilder modelBuilder) method directly.

Each mapping should have at least a primary key - HasKey(p => p.Id) and correspondent table - .ToTable("Customers") and references which you would like to see in C#. 
For example one customer has multiple orders, each order has one customer.
References mapping example:
```
builder
  .HasOne(p => p.Member)
  .WithOne()
  .HasForeignKey<Member>(p=>p.MemberId);
  
builder
  .HasOne(p => p.Customer)
  .WithMany()
  .HasForeignKey(r => r.CustomerId);
```

More details about possible mapping syntax can be found here - https://docs.microsoft.com/en-us/ef/core/modeling/

You don't need to add mapping for sproc related classes.

Column level binding should be in use only if we want to have a different name in a class than it was defined in DB. 
By default MSSQL and Azure SQL is case-insensitive, as a result you don’t need to add column level binding if you have a name in different case in DB than in model. 
Columns mapping:

builder.Property(b => b.Alias).HasColumnName("Field");

### Using

There are 2 main ways to use DAL:

**Directly via a repository**
In this case, you will need to inject a repository into a class:
```
private readonly IRepository<Product> _productRepository;

public ORMTestController(IRepository<Product> productRepository)
{
    _productRepository = productRepository;
}
```

And use it. In this case a repository will be resolved to GenericRepository and this implementation
will save all changes to DB right after a call:
```
await _productRepository.InsertGraphAsync(product);
```

**Through UOW**
You need to inject an appropriate UOW:
```
private readonly IUnitOfWork<{CONTEXT_TYPE}>> _unitOfWorkProfile;

public ORMTestController(IUnitOfWork<{CONTEXT_TYPE}>> unitOfWorkProfile)
{
    _unitOfWorkProfile = unitOfWorkProfile;
}
```

And get a repository from UOW:
```
var productRepository = uow.GetRepository<Product>();
```

In this case a repository will be resolved to UOWRespository and this implementation
won't save changes to DB right after a call. UOW is responsible for saving changes:
```
await _unitOfWorkProfile.SaveChangesAsync();
```

If you need to explicitly use transactions, you will need to create a transaction:
```
using (var transaction = uow.BeginTransaction())
{
...
}
```

You can specify an isolation level:
```
using (var transaction = uow.BeginTransaction(IsolationLevel.ReadUncommitted))  
```

You will need to save changes in the context of UOW:
```
await _unitOfWorkProfile.SaveChangesAsync();
```

And commit or rollback your changes:
```
await transaction.CommitAsync();
```

### Rules
- Don’t publicly expose IQueryable, data should be materialized before returning from DAL layer
- Don’t reuse entities in different layers of the app ever if the same entity will work perfectly right now. Prefer to use AutoMapper if you need to map them
- Don’t query data without conditions except you have a very good reason to do it
- Try to avoid using huge number of Includes during loading your entities. EF will generate JOIN or LEFT JOIN in this case. It can be more efficient to load your entities in couple turns. 

### Testing
You can always write integration tests which will work directly with DB or use in Memory provider.

### Troubleshooting
EF DAL writes trace information into logs using Serilog(DataContextTraceLogger). It includes: SQL statements, params, duration, transaction isolation level, transaction commit/rollback. 
Trace example:
```
Committing transaction.
Called DB: {CONTEXT_TYPE}
Calling DB: {CONTEXT_TYPE}
…
Began transaction with isolation level 'ReadCommitted'.
```

“Calling DB {CONTEXT_TYPE}“ will include SQL statement and params which were sent to DB, “Called DB“ will indicate that this call was completed successfully.
You can always replay the same call by coping SQL statements and params into SSMS and executing them again. 

## Advanced

### Specifications

GenericReadRepository provides you the way to use "Specification" pattern for querying data.
This approach allows to combine all read related logic in one place and reuse it in multiple business services.

### Custom repositories

If you need to write custom code which will work with datasets directly or use sprocs, you will need to create
custom repositories. There are 2 sorts of custom repositories:

- Entity based should be in use if you want to work with Set directly:
```
public class OrderRepository: GenericRepository<{ENTITY}>, IOrderRepository
```
Your custom repository should return back materialized data.

- Sproc based should be in use if you want to call sprocs:
```
public class FIRepository: DirectSqlSprocRepository<{CONTEXT_TYPE}>, IFIRepository
```

And you will need to register them in DI:
```
services.AddScoped<IOrderRepository, OrderRepository>(provider =>
    new OrderRepository(GetContextByCustomRepositoryType(provider, typeof(OrderRepository))));
services.AddScoped<IFIRepository, FIRepository>(provider =>
    new FIRepository(provider.GetService<{CONTEXT_TYPE}>()));
```

### Tracking

EF tracks changes internally and sync them with DB when it’s requested. Disabling tracking during reading a list of entities can improve performance.
You always have control on enabling/disabling it for a specific use case on the repository level. More info about tracking(https://docs.microsoft.com/en-us/ef/core/querying/tracking). 

### Projections

Sometimes you want to load limited specific sub-set of columns from an entity or group of entities. In this case you will use projections. DAL supports this use case in Specifications.

### Soft delete

EF supports filters(https://docs.microsoft.com/en-us/ef/core/querying/filters), this approach allows to organize soft delete in more readable way and have only one check in one place. Example:
```
builder.HasQueryFilter(p => !p.IsDeleted);
```
**Important - it can be only one HasQueryFilter in one configuration, if you use the method twice the last filter will be applied**

### Direct update and delete commands

EF Core supports update/delete operations without rereading data from a DB, it’s your responsibility to make sure that an entity with the give key exists in DB. If it’s not, an exception will be thrown.

### Sproc parameters

**Always use parameterization for raw SQL queries**
When introducing any user-provided values into a raw SQL query, care must be taken to avoid SQL injection attacks. In addition to validating that such values don't contain invalid characters, always use parameterization which sends the values separate from the SQL text.
The FromSqlInterpolated and ExecuteSqlInterpolated methods allow using string interpolation syntax in a way that protects against SQL injection attacks.
If you want to use sprocs and pass SqlParameter, remember that order is important. You need to pass parameters in the same order as it’s specified in the sproc - https://docs.microsoft.com/en-us/ef/core/querying/raw-sql

**Parameter Ordering** Entity Framework Core passes parameters based on the order of the SqlParameter[] array. When passing multiple SqlParameters, the ordering in the SQL string must match the order of the parameters in the stored procedure's definition. 
Failure to do this may result in type conversion exceptions and/or unexpected behavior when the procedure is executed.
If your sproc has optional parameters and you don’t want to pass them, please make sure that optional parameters are after required in your sproc definition

### Read replicas

In many case it can be useful to read data from read replicas to distribute load instead of reading from the main writable DB. You will need to request IReadRepository<T> from DI or get a read repository from UOW - uow.GetReadRepository<T>().
IReadRepository<T> will only provide access to read operations and will use ApplicationIntent=ReadOnly during communication with a failover group.

If you try to use IReadRepository for different a DB context, which is using a DB which doesn't provide a read replicate, the operation will be executed in context of the main writable DB.
No app settings updates are required for using read replicas, infrastructure will construct connection string with ApplicationIntent=ReadOnly based on standard connection strings.

### Reverse engineering

If you need to create mapping for existing tables, prefer to use tools for reverse engineering. 
EFCorePowerTools(https://marketplace.visualstudio.com/items?itemName=ErikEJ.EFCorePowerTools) is VS plugin which can help here. It provides the way to automate this process and supports generating classes and sprocs mapping. 