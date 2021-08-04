using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;
using MS.DataAccess.Setup;

namespace MS.DataAccess.Repositories
{
    public class GenericRepository<TEntity> : GenericReadRepository<TEntity>, IRepository<TEntity>
        where TEntity : Entity
    {
        public GenericRepository(IDataContext context) : base(context)
        {
        }

        public TEntity Insert(TEntity entity)
        {
            DbSet.Add(entity);
            SaveChanges();
            return entity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity, CancellationToken? cancellationChangeToken = null)
        {
            DbSet.Add(entity);
            await SaveChangesAsync(cancellationChangeToken);
            return entity;
        }

        public IEnumerable<TEntity> InsertRange(IEnumerable<TEntity> entities)
        {
            if (entities != null)
            {
                DbSet.AddRange(entities);
                SaveChanges();
            }

            return entities;
        }

        public async Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entities,
            CancellationToken? cancellationChangeToken = null)
        {
            if (entities != null)
            {
                DbSet.AddRange(entities);
                await SaveChangesAsync(cancellationChangeToken);
            }

            return entities;
        }

        public TEntity InsertGraph(TEntity entity)
        {
            Context.TrackGraphForAdd(entity);
            SaveChanges();
            return entity;
        }

        public async Task<TEntity> InsertGraphAsync(TEntity entity, CancellationToken? cancellationChangeToken = null)
        {
            if (entity != null)
            {
                Context.TrackGraphForAdd(entity);
                await SaveChangesAsync(cancellationChangeToken);
            }

            return entity;
        }

        public TEntity Update(TEntity entity, Func<TEntity, object> keySelector = null,
            bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entity == null) return entity;

                // if an entity with the same key exists in tracker, update existing entry
                var entry = keySelector != null
                    ? Context.ChangeTracker.Entries<TEntity>()
                        .FirstOrDefault(p => keySelector(p.Entity) == keySelector(entity))
                    : null;
                if (entry != null)
                    entry.CurrentValues.SetValues(entity);
                else
                    DbSet.Update(entity);
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, Func<TEntity, object> keySelector = null,
            CancellationToken? cancellationChangeToken = null, bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entity == null) return entity;

                // if an entity with the same key exists in tracker, update existing entry
                var entry = keySelector != null
                    ? Context.ChangeTracker.Entries<TEntity>()
                        .FirstOrDefault(p => keySelector(p.Entity) == keySelector(entity))
                    : null;
                if (entry != null)
                    entry.CurrentValues.SetValues(entity);
                else
                    DbSet.Update(entity);
                await SaveChangesAsync(cancellationChangeToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return entity;
        }

        public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities,
            bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entities == null) return entities;

                DbSet.UpdateRange(entities);
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return entities;
        }

        public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities,
            CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entities == null) return entities;

                DbSet.UpdateRange(entities);
                await SaveChangesAsync(cancellationChangeToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return entities;
        }

        public bool Delete(TEntity entity, bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entity == null) return false;

                DbSet.Remove(entity);
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(TEntity entity, CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entity == null) return false;

                DbSet.Remove(entity);
                await SaveChangesAsync(cancellationChangeToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return true;
        }

        public bool DeleteRange(IEnumerable<TEntity> entities, bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entities == null) return false;

                DbSet.RemoveRange(entities);
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return true;
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entities,
            CancellationToken? cancellationChangeToken = null,
            bool suppressOptimisticConcurrency = false)
        {
            try
            {
                if (entities == null) return false;

                DbSet.RemoveRange(entities);
                await SaveChangesAsync(cancellationChangeToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!suppressOptimisticConcurrency) throw;
            }

            return true;
        }

        public void Detach(TEntity entity)
        {
            Context.SetState(entity, EntityState.Detached);
        }

        public void DetachRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities) Context.SetState(entity, EntityState.Detached);
        }

        protected virtual async Task<int> SaveChangesAsync(CancellationToken? cancellationChangeToken)
        {
            return await Context.SaveChangesAsync(cancellationChangeToken ?? CancellationToken.None);
        }

        protected virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }
}