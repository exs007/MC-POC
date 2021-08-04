using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using MS.DataAccess.Context;
using MS.DataAccess.Repositories;
using MS.DataAccess.Setup;
using MS.DataAccess.Transactions;

namespace MS.DataAccess.UOW
{
    public interface IUnitOfWork<TContext> : IDisposable where TContext : IDataContext
    {
        /// <summary>
        ///     Returns read/write repository
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IRepository<TEntity> GetRepository<TEntity>()
            where TEntity : Entity;

        /// <summary>
        ///     Returns read only repository
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IReadRepository<TEntity> GetReadOnlyRepository<TEntity>()
            where TEntity : Entity;

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken? cancellationToken = null);
        IScopedTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);
    }
}