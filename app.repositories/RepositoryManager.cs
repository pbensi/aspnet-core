using app.migrator.Contexts;
using System.Collections.Concurrent;

namespace app.repositories
{
    public interface IRepositoryManager
    {
        IRepository<TEntity> Entity<TEntity>() where TEntity : class;
        IUnitOfWork UnitOfWork { get; }
    }

    public sealed class RepositoryManager : IRepositoryManager
    {
        private readonly DatabaseContext _databaseContext;
        private readonly RequestContext _requestContext;
        private readonly Lazy<IUnitOfWork> _lazyUnitOfWork;
        private readonly ConcurrentDictionary<Type, Lazy<object>> _repositories;

        public RepositoryManager(DatabaseContext databaseContext, RequestContext requestContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
            _lazyUnitOfWork = new Lazy<IUnitOfWork>(() => new UnitOfWork(_databaseContext, _requestContext));
            _repositories = new ConcurrentDictionary<Type, Lazy<object>>();
        }

        public IRepository<TEntity> Entity<TEntity>() where TEntity : class
        {
            var repositoryLazy = _repositories.GetOrAdd(typeof(TEntity), type => CreateLazyRepository<TEntity>());
            return (IRepository<TEntity>)repositoryLazy.Value;
        }

        private Lazy<object> CreateLazyRepository<TEntity>() where TEntity : class
        {
            return new Lazy<object>(() => new Repository<TEntity>(_databaseContext));
        }

        public IUnitOfWork UnitOfWork => _lazyUnitOfWork.Value;
    }
}
