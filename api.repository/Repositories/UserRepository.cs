using api.entities;
using api.migrator;
using api.repository.Exceptions;
using api.repository.IRepository;
using api.repository.Utilities;
using Microsoft.EntityFrameworkCore;

namespace api.repository.Repositories
{
    internal sealed class UserRepository : IUserRepository
    {
        private readonly APIDBContext _dbContext;

        public UserRepository(APIDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IEnumerable<User> Users, int totalPages)> GetAllUsersAsync(string search, int pageNumber, int pageSize)
        {
            var query = _dbContext.User.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.Name.Contains(search));
            }

            int totalCount = await query.CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            int recordsToSkip = (pageNumber - 1) * pageSize;

            var users = await query.Skip(recordsToSkip)
                                  .Take(pageSize)
                                  .OrderBy(x => x.Name)
                                  .ToListAsync();

            return (users, totalPages);
        }

        public async Task<User> FindUserByIdAsync(int id)
        {
            var user = await _dbContext.User
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync(x => x.Id == id);

            return user;
        }

        public async Task<User> SignInUserAsync(string accountName, string password)
        {
            var user = await _dbContext.User
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.AccountName == accountName && x.Password == password && x.IsActive == true);

            if (user != null)
            {
                UserAudit.Id = user.Id;
            }

            return user;
        }

        public void CreateUser(User user)
        {
            if (user is not null)
            {
                _dbContext.Add(user);
            }
        }

        public void UpdateUser(User user)
        {
            if (user is not null)
            {
                _dbContext.User.Update(user);
            }
        }

        public void DeleteUser(User user)
        {
            if (user is not null)
            {
                _dbContext.User.Remove(user);
            }
        }
    }
}
