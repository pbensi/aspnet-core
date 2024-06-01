using api.shared.Dto;

namespace api.service.abstractions.IServices
{
    public interface IUserService
    {
        Task<List<PermissionDto>> GetUserPermissionAsync(int id);
        Task<(IEnumerable<UserDto> UsersDto, int totalPages)> GetAllUsersAsync(string search, int pageNumber, int pageSize);
        Task<UserDto> FindUserByIdAsync(int id);
        Task<(UserDto? user, string encryptedId)> SignInUserAsync(string accountName, string password);
        Task<string> CreateUserAsync(UserDto user);
        Task<string> UpdateUserAsync(UserDto user);
        Task<string> DeleteUserAsync(int id);
    }
}
