using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MS.DataAccess.Context;

namespace MS.DataAccess.Transactions
{
    public class ScopedTransaction : IScopedTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public ScopedTransaction(IDbContextTransaction transaction, IDataContext dbContext)
        {
            _transaction = transaction;
            DbContext = dbContext;
        }

        public Guid TransactionId => _transaction.TransactionId;
        public IDataContext DbContext { get; }
        public bool Closed { get; private set; }
        public event Action EndTransactionCommit;
        public event Action EndTransactionRollback;

        public void Dispose()
        {
            _transaction.Dispose();
            Closed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await _transaction.DisposeAsync();
            Closed = true;
        }

        public void Commit()
        {
            _transaction.Commit();
            EndTransactionCommit?.Invoke();
        }

        public void Rollback()
        {
            _transaction.Commit();
            EndTransactionRollback?.Invoke();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = new())
        {
            await _transaction.CommitAsync(cancellationToken);
            EndTransactionCommit?.Invoke();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = new())
        {
            await _transaction.RollbackAsync(cancellationToken);
            EndTransactionRollback?.Invoke();
        }
    }
}