namespace api.repository.Authorization
{
    public interface IPermissionBuilder
    {
        List<Permission> GetPermissions();
        Permission CreateChildPermission(string name, string displayName);
    }

    public class PermissionBuilder : IPermissionBuilder
    {
        private List<Permission> Permissions { get; }

        public PermissionBuilder()
        {
            Permissions = new List<Permission>();
        }

        public Permission CreateChildPermission(string name, string displayName)
        {
            var permission = new Permission(name, displayName);
            Permissions.Add(permission);

            return permission;
        }

        public List<Permission> GetPermissions()
        {
            return Permissions;
        }
    }
}
