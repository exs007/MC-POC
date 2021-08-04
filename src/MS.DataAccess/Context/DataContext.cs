using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MS.DataAccess.Extensions;
using MS.DataAccess.Setup;
using MS.DataAccess.Transactions;

namespace MS.DataAccess.Context
{
    public abstract class DataContext : DbContext, IDataContext
    {
        private readonly IDALTypesResolver _dALTypesResolver;
        private IScopedTransaction _transaction;

        protected DataContext(DbContextOptions contextOptions, IDALTypesResolver dALTypesResolver,
            string environment = null) : base(contextOptions)
        {
            InstanceId = Guid.NewGuid();
            _dALTypesResolver = dALTypesResolver;
            Environment = environment ?? "EMPTY";
        }

        public Guid InstanceId { get; }
        public string Environment { get; }
        public bool InTransaction => _transaction != null && !_transaction.Closed;

        public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            return Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        public Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        public int ExecuteSqlRaw(string sql, IEnumerable<object> parameters)
        {
            return Database.ExecuteSqlRaw(sql, parameters.ToArray());
        }

        public int ExecuteSqlInterpolated(FormattableString sql)
        {
            return Database.ExecuteSqlInterpolated(sql);
        }

        public IScopedTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (_transaction == null || _transaction.Closed)
            {
                var facadeDependencies = this.GetService<IDatabaseFacadeDependencies>();
                IDbContextTransaction dbTransaction;
                // create transaction with isolation level which we need, only in use for relational DB
                if (facadeDependencies?.TransactionManager is IRelationalTransactionManager)
                    dbTransaction = (facadeDependencies.TransactionManager as IRelationalTransactionManager)
                        .BeginTransaction(level);
                else
                    dbTransaction = Database.BeginTransaction();
                _transaction = new ScopedTransaction(dbTransaction, this);
            }

            return _transaction;
        }

        public void TrackGraphForAdd(object entity)
        {
            TrackGraph(entity, p => p.Entry.State = EntityState.Added);
        }

        public void TrackGraph(object entity, Action<EntityEntryGraphNode> callback)
        {
            ChangeTracker.TrackGraph(entity, callback);
        }

        public void SetState<T>(T entity, EntityState state) where T : Entity
        {
            Entry(entity).State = state;
        }

        public void UpdateValues<T>(T existingObject, T newObject) where T : Entity
        {
            Entry(existingObject).CurrentValues.SetValues(newObject);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssemblyForContext(_dALTypesResolver, this);
        }

        public static string GenerateReadReplicaConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return connectionString;
            if (!connectionString.EndsWith(";")) connectionString += ";";
            connectionString = connectionString + "ApplicationIntent=ReadOnly";

            return connectionString;
        }
    }
}