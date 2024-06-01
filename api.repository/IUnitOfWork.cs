using api.repository.Utilities;

namespace api.repository
{
    public interface IUnitOfWork
    {
        Task<string> SaveChangesWithUserRolesAsync(string pageName, OperationType operationType, CancellationToken cancellationToken = default);
        Task<string> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
