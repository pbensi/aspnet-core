using app.migrator.Contexts;
using app.repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace app.repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int? id, CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<TEntity?> FindAsync(CancellationToken cancellationToken = default, params object[] keyValues);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter);
        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes);
        IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes);
        IQueryable<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> includeExpression, params Expression<Func<TProperty, object>>[] thenIncludes);
        IQueryable<TEntity> AsQueryable();

        void Add(TEntity entity);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Update(TEntity entity);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Delete(TEntity entity);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        void AddRange(IEnumerable<TEntity> entities);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void UpdateRange(IEnumerable<TEntity> entities);
        Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        void DeleteRange(IEnumerable<TEntity> entities);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }

    internal sealed class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DatabaseContext _databaseContext;

        public Repository(DatabaseContext dbContext)
        {
            _databaseContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private IQueryable<TEntity> AsNoTrackingQuery() => _databaseContext.Set<TEntity>().AsNoTracking();

        public async Task<TEntity?> GetByIdAsync(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return null;
            }
            return await _databaseContext.Set<TEntity>().FindAsync(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) =>
            await AsNoTrackingQuery().FirstOrDefaultAsync(filter, cancellationToken).ConfigureAwait(false);

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter) => AsNoTrackingQuery().Where(filter);

        public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = AsNoTrackingQuery();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null && includes.Any())
            {
                query = query.IncludeIf(true, includes);
            }

            return query;
        }

        public IQueryable<TEntity> AsQueryable() => AsNoTrackingQuery().AsQueryable();

        public IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includes)
        {
            var query = AsNoTrackingQuery();
            return query.IncludeIf(includes.Any(), includes);
        }

        public IQueryable<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> includeExpression, params Expression<Func<TProperty, object>>[] thenIncludes)
        {
            var query = AsNoTrackingQuery();
            query = query.Include(includeExpression);

            foreach (var thenInclude in thenIncludes)
            {
                query = query.Include(includeExpression).ThenInclude(thenInclude);
            }

            return query;
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) =>
            await AsNoTrackingQuery().AnyAsync(filter, cancellationToken).ConfigureAwait(false);

        public async Task<TEntity?> FindAsync(CancellationToken cancellationToken = default, params object[] keyValues) =>
            await _databaseContext.Set<TEntity>().FindAsync(keyValues, cancellationToken).ConfigureAwait(false);

        public void Add(TEntity entity) => _databaseContext.Set<TEntity>().Add(entity);

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            await _databaseContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);

        public void Update(TEntity entity) => _databaseContext.Set<TEntity>().Update(entity);

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            await Task.Run(() => _databaseContext.Set<TEntity>().Update(entity), cancellationToken).ConfigureAwait(false);

        public void Delete(TEntity entity) => _databaseContext.Set<TEntity>().Remove(entity);

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) =>
            await Task.Run(() => _databaseContext.Set<TEntity>().Remove(entity), cancellationToken).ConfigureAwait(false);

        public void AddRange(IEnumerable<TEntity> entities) => _databaseContext.Set<TEntity>().AddRange(entities);

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            await _databaseContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

        public void UpdateRange(IEnumerable<TEntity> entities) => _databaseContext.Set<TEntity>().UpdateRange(entities);

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            await Task.Run(() => _databaseContext.Set<TEntity>().UpdateRange(entities), cancellationToken).ConfigureAwait(false);

        public void DeleteRange(IEnumerable<TEntity> entities) => _databaseContext.Set<TEntity>().RemoveRange(entities);

        public async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
            await Task.Run(() => _databaseContext.Set<TEntity>().RemoveRange(entities), cancellationToken).ConfigureAwait(false);
    }
}
