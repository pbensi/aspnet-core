using api.entities;
using api.repository;
using api.repository.Authorization;
using api.repository.Utilities;
using api.service.abstractions.IServices;
using api.shared.Dto;
using api.shared.Utilities;
using AutoMapper;

namespace api.service.Services
{
    internal sealed class UserService : IUserService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly IPermissionProvider _permissionProvider;

        public UserService(IRepositoryManager repositoryManager,
            IMapper mapper,
            IPermissionProvider permissionProvider)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _permissionProvider = permissionProvider;
        }

        public async Task<List<PermissionDto>> GetUserPermissionAsync(int id)
        {
            try
            {
                var user = await _repositoryManager.UserRepository.FindUserByIdAsync(id);

                if (user is null)
                    return new List<PermissionDto>();

                var permissions = _permissionProvider.GetPermissions();
                var userPermissions = MapUserPermissions(user, permissions);

                return _mapper.Map<List<PermissionDto>>(userPermissions);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private List<Permission> MapUserPermissions(User user, List<Permission> permissions)
        {
            var userPermissions = new List<Permission>();

            foreach (var userRole in user.UserRoles)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == userRole.Name);
                if (permission != null)
                {
                    permission.CanAdd = userRole.CanAdd;
                    permission.CanUpdate = userRole.CanUpdate;
                    permission.CanRemove = userRole.CanRemove;
                    permission.CanView = userRole.CanView;
                    userPermissions.Add(permission);
                }
            }

            return userPermissions;
        }

        public async Task<(IEnumerable<UserDto> UsersDto, int totalPages)> GetAllUsersAsync(string search, int pageNumber, int pageSize)
        {
            try
            {
                var (users, totalPages) = await _repositoryManager.UserRepository.GetAllUsersAsync(search, pageNumber, pageSize);
                var result = _mapper.Map<IEnumerable<UserDto>>(users);

                return (result, totalPages);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<UserDto> FindUserByIdAsync(int id)
        {
            try
            {
                var user = await _repositoryManager.UserRepository.FindUserByIdAsync(id);
                var result = _mapper.Map<UserDto>(user);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<(UserDto? user, string encryptedId)> SignInUserAsync(string accountName, string password)
        {
            try
            {
                var user = await _repositoryManager.UserRepository.SignInUserAsync(accountName, password);
                if (user != null)
                {
                    var result = _mapper.Map<UserDto>(user);

                    AES aes = new AES();

                    byte[] key = aes.GenerateRandomKey();

                    Token.Key = key;
                    Token.UserId = user.Id.ToString();
                    Token.AccountName = user.AccountName;

                    string encryptedData = aes.EncryptData(user.Id.ToString(), key);
                    return (result, encryptedData);
                }

                return (null, string.Empty);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<string> CreateUserAsync(UserDto user)
        {
            try
            {
                var result = _mapper.Map<User>(user);

                _repositoryManager.UserRepository.CreateUser(result);

                return await _repositoryManager.UnitOfWork.SaveChangesWithUserRolesAsync(PermissionNames.Pages_User_And_Role, OperationType.Add);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<string> UpdateUserAsync(UserDto user)
        {
            try
            {
                var result = await _repositoryManager.UserRepository.FindUserByIdAsync(user.Id);

                var updateUser = _mapper.Map(user, result);

                _repositoryManager.UserRepository.UpdateUser(updateUser);

                return await _repositoryManager.UnitOfWork.SaveChangesWithUserRolesAsync(PermissionNames.Pages_User_And_Role, OperationType.Update);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<string> DeleteUserAsync(int id)
        {
            try
            {
                var result = await _repositoryManager.UserRepository.FindUserByIdAsync(id);

                _repositoryManager.UserRepository.DeleteUser(result);

                return await _repositoryManager.UnitOfWork.SaveChangesWithUserRolesAsync(PermissionNames.Pages_User_And_Role, OperationType.Remove);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
