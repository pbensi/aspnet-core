namespace app.services.Authorizations
{
    internal static class PermissionBuilder
    {
        public static async Task<List<Permission>> GetPermissionsAsync()
        {
            var rootPermission = new Permission("root", "root");

            var users = rootPermission.AddChild(PageName.Pages_Users, "Users", true);
            var swagger = rootPermission.AddChild(PageName.Pages_Swagger, "Swagger", true);

            var children = rootPermission.Children.SelectMany(FlattenPermissions).ToList();

            return await Task.FromResult(children);
        }

        private static List<Permission> FlattenPermissions(Permission parent)
        {
            var permissions = new List<Permission> { parent };

            foreach (var child in parent.Children)
            {
                permissions.AddRange(FlattenPermissions(child));
            }

            return permissions;
        }
    }
}
