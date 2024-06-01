using api.entities;

namespace api.repository.IRepository
{
    public interface IUserRepository
    {
        Task<(IEnumerable<User> Users, int totalPages)> GetAllUsersAsync(string search, int pageNumber, int pageSize);
        Task<User> FindUserByIdAsync(int id);
        Task<User> SignInUserAsync(string accountName, string password);
        void CreateUser(User user);
        void UpdateUser(User user);
        void DeleteUser(User user);
    }
}
