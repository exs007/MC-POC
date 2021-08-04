using System;
using Microsoft.EntityFrameworkCore.Storage;
using MS.DataAccess.Context;

namespace MS.DataAccess.Transactions
{
    public interface IScopedTransaction : IDbContextTransaction
    {
        IDataContext DbContext { get; }
        bool Closed { get; }
        event Action EndTransactionCommit;
        event Action EndTransactionRollback;
    }
}