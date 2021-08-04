using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MS.DataAccess.Setup;
using MS.DataAccess.Transactions;

namespace MS.DataAccess.Context
{
    public interface IDataContext : IDisposable
    {
        string Environment { get; }

        ChangeTracker ChangeTracker { get; }

        bool InTransaction { get; }

        DbSet<T> Set<T>() where T : class;

        Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters,
            CancellationToken cancellationToken = default);

        Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql,
            CancellationToken cancellationToken = default);

        int ExecuteSqlRaw(string sql, IEnumerable<object> parameters);

        int ExecuteSqlInterpolated(FormattableString sql);

        IScopedTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted);

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        void TrackGraphForAdd(object entity);

        void TrackGraph(object entity, Action<EntityEntryGraphNode> callback);

        void SetState<T>(T entity, EntityState state) where T : Entity;

        void UpdateValues<T>(T entry, T newObject) where T : Entity;
    }
}