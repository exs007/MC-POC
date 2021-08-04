using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Repositories
{
    public interface IRepository<TEntity> : IReadRepository<TEntity> where TEntity : Entity
    {
        TEntity Insert(TEntity entity);

        Task<TEntity> InsertAsync(TEntity entity, CancellationToken? cancellationChangeToken = null);

        IEnumerable<TEntity> InsertRange(IEnumerable<TEntity> entities);

        Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities,
            CancellationToken? cancellationChangeToken = null);

        TEntity InsertGraph(TEntity entity);

        Task<TEntity> InsertGraphAsync(TEntity entity, CancellationToken? cancellationChangeToken = null);

        TEntity Update(TEntity entity, Func<TEntity, object> keySelector = null,
            bool suppressOptimisticConcurrency = false);

        Task<TEntity> UpdateAsync(TEntity entity, Func<TEntity, object> keySelector = null,
            CancellationToken? cancellationChangeToken = null, bool suppressOptimisticConcurrency = false);

        IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities, bool suppressOptimisticConcurrency = false);

        Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities,
            CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false);

        bool Delete(TEntity entity, bool suppressOptimisticConcurrency = false);

        Task<bool> DeleteAsync(TEntity entity, CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false);

        bool DeleteRange(IEnumerable<TEntity> entities, bool suppressOptimisticConcurrency = false);

        Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false);

        void Detach(TEntity entity);

        void DetachRange(IEnumerable<TEntity> entities);
    }
}