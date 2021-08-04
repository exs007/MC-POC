using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MS.DataAccess.Context;
using MS.DataAccess.Repositories;
using MS.DataAccess.Setup;
using MS.DataAccess.Transactions;

namespace MS.DataAccess.UOW
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : IDataContext
    {
        private readonly TContext _context;
        private readonly TContext _readReplicaContext;
        private readonly Dictionary<Type, object> _readRepositories;
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(TContext context, TContext readReplicaContext)
        {
            _context = context;
            _readReplicaContext = readReplicaContext;
            _repositories = new Dictionary<Type, object>();
            _readRepositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>()
            where TEntity : Entity
        {
            object result;
            if (_repositories.TryGetValue(typeof(TEntity), out result)) return (IRepository<TEntity>) result;

            var repo = new UOWRespository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repo);
            return repo;
        }

        public IReadRepository<TEntity> GetReadOnlyRepository<TEntity>()
            where TEntity : Entity
        {
            object result;
            if (_readRepositories.TryGetValue(typeof(TEntity), out result)) return (IReadRepository<TEntity>) result;

            var repo = new GenericReadRepository<TEntity>(_readReplicaContext);
            _readRepositories.Add(typeof(TEntity), repo);
            return repo;
        }

        public IScopedTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return _context.BeginTransaction(level);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(CancellationToken? cancellationToken = null)
        {
            return await _context.SaveChangesAsync(cancellationToken ?? CancellationToken.None);
        }

        public void Dispose()
        {
        }
    }
}