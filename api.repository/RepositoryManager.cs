using api.migrator;
using api.repository.IRepository;
using api.repository.Repositories;

namespace api.repository
{
    public sealed class RepositoryManager : IRepositoryManager
    {
        
        private readonly Lazy<IUserRepository> _lazyUserRepository;
        private readonly Lazy<IUnitOfWork> _lazyUnitOfWork;

        public RepositoryManager(APIDBContext APIDBContext)
        {
            _lazyUserRepository = new Lazy<IUserRepository>(() => new UserRepository(APIDBContext));
            _lazyUnitOfWork = new Lazy<IUnitOfWork>(() => new UnitOfWork(APIDBContext));
        }

        public IUserRepository UserRepository => _lazyUserRepository.Value;
        public IUnitOfWork UnitOfWork => _lazyUnitOfWork.Value;
    }
}
