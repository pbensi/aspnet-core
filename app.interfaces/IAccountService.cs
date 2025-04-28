using app.shared.Dto.Account;

namespace app.interfaces
{
    public interface IAccountService
    {
        Task<(AccountDto? account, string message)> SignInAccountAsync(SignInDto signIn);
        Task<AccountDto> GetAccountAsync(Guid userGuid);
        Task<PermissionCheckDto> CheckAccountPermissionAsync(string pageName, string requestMethod, string requestPath = "", string allowedRole = "None");
        Task<List<AccountRoleDto>> CheckPermissionPageNamesAsync(List<string> pageName);
    }
}
