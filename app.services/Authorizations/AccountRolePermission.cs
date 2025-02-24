using app.shared.Dto.Account;
using AutoMapper;

namespace app.services.Authorizations
{
    internal static class AccountRolePermission
    {
        public static async Task<List<AccountRoleDto>> RolesAsync(List<AccountRoleDto> role, IMapper mapper)
        {
            var permissions = await PermissionBuilder.GetPermissionsAsync();

            role ??= new List<AccountRoleDto>();

            var pageName = new HashSet<string>(role.Select(r => r.PageName));

            permissions = permissions
                .Where(p => !pageName.Contains(p.PageName))
                .ToList();

            var newUserRoles = mapper.Map<List<AccountRoleDto>>(permissions);

            role.AddRange(newUserRoles);

            return role;
        }
    }
}
