namespace api.repository.Authorization
{
    public interface IPermissionProvider
    {
        List<Permission> GetPermissions();
    }

    public class PermissionProvider : IPermissionProvider 
    {
        public readonly IPermissionBuilder _permissionBuilder;

        public PermissionProvider(IPermissionBuilder permissionBuilder)
        {
            _permissionBuilder = permissionBuilder;

            var user = _permissionBuilder.CreateChildPermission(PermissionNames.Pages_Admin, "Admin");
            user.CreateChildPermission(PermissionNames.Pages_User_And_Role, "User");

            var company = _permissionBuilder.CreateChildPermission(PermissionNames.Pages_Company_Title, "Company Title");
        }

        public List<Permission> GetPermissions()
        {
            return _permissionBuilder.GetPermissions();
        }
    }
}
