namespace app.services.Authorizations
{
    internal static class PermissionBuilder
    {
        public static async Task<List<Permissions>> GetPermissionsAsync()
        {
            var rootPermission = new Permissions("root", "root");

            var users = rootPermission.AddChild(PageNames.Pages_Users, "Users", true);
            var swagger = rootPermission.AddChild(PageNames.Pages_Swagger, "Swagger", true);

            var children = rootPermission.Children.SelectMany(FlattenPermissions).ToList();

            return await Task.FromResult(children);
        }

        private static List<Permissions> FlattenPermissions(Permissions parent)
        {
            var permissions = new List<Permissions> { parent };

            foreach (var child in parent.Children)
            {
                permissions.AddRange(FlattenPermissions(child));
            }

            return permissions;
        }
    }
}
