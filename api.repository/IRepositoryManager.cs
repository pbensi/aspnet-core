using api.repository.IRepository;

namespace api.repository
{
    public interface IRepositoryManager
    {
        IUserRepository UserRepository { get; }
        IUnitOfWork UnitOfWork { get; }
    }
}
