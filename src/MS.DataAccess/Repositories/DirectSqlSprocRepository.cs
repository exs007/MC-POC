using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MS.DataAccess.Context;

namespace MS.DataAccess.Repositories
{
    public abstract class DirectSqlSprocRepository<TContext> where TContext : IDataContext
    {
        protected readonly IDataContext Context;

        protected DirectSqlSprocRepository(IDataContext context)
        {
            Context = context;
        }

        protected async Task<ICollection<TEntity>> FromSqlInterpolatedToListAsync<TEntity>(FormattableString sql)
            where TEntity : class
        {
            return await Context.Set<TEntity>().FromSqlInterpolated(sql).AsNoTracking().ToListAsync();
        }

        protected async Task<TEntity> FromSqlInterpolatedFirstOrDefaultAsync<TEntity>(FormattableString sql)
            where TEntity : class
        {
            return (await FromSqlInterpolatedToListAsync<TEntity>(sql)).FirstOrDefault();
        }

        protected async Task<ICollection<TEntity>> FromSqlRawToListAsync<TEntity>(string sql,
            params object[] parameters)
            where TEntity : class
        {
            return await Context.Set<TEntity>().FromSqlRaw(sql, parameters).AsNoTracking().ToListAsync();
        }

        protected async Task<TEntity> FromSqlRawFirstOrDefaultAsync<TEntity>(string sql, params object[] parameters)
            where TEntity : class
        {
            return (await FromSqlRawToListAsync<TEntity>(sql, parameters)).FirstOrDefault();
        }

        protected ICollection<TEntity> FromSqlInterpolatedToList<TEntity>(FormattableString sql)
            where TEntity : class
        {
            return Context.Set<TEntity>().FromSqlInterpolated(sql).AsNoTracking().ToList();
        }

        protected TEntity FromSqlInterpolatedFirstOrDefault<TEntity>(FormattableString sql)
            where TEntity : class
        {
            return FromSqlInterpolatedToList<TEntity>(sql).FirstOrDefault();
        }

        protected ICollection<TEntity> FromSqlRawToList<TEntity>(string sql, params object[] parameters)
            where TEntity : class
        {
            return Context.Set<TEntity>().FromSqlRaw(sql, parameters).AsNoTracking().ToList();
        }

        protected TEntity FromSqlRawFirstOrDefault<TEntity>(string sql, params object[] parameters)
            where TEntity : class
        {
            return FromSqlRawToList<TEntity>(sql, parameters).FirstOrDefault();
        }

        protected Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return Context.ExecuteSqlRawAsync(sql, parameters);
        }

        protected Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            return Context.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        protected Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql,
            CancellationToken cancellationToken = default)
        {
            return Context.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        protected int ExecuteSqlRaw(string sql, params object[] parameters)
        {
            return Context.ExecuteSqlRaw(sql, parameters);
        }

        protected int ExecuteSqlRaw(string sql, IEnumerable<object> parameters)
        {
            return Context.ExecuteSqlRaw(sql, parameters);
        }

        protected int ExecuteSqlInterpolated(FormattableString sql)
        {
            return Context.ExecuteSqlInterpolated(sql);
        }
    }
}