namespace api.repository.Authorization
{
    public class Permission
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanRemove { get; set; }
        public bool CanView { get; set; }
        public List<Permission> Children { get; set; }

        public Permission(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
            CanAdd = true;
            CanUpdate = true;
            CanRemove = true;
            CanView = true;
            Children = new List<Permission>();
        }

        public void CreateChildPermission(string name, string displayName)
        {
            Children.Add(new Permission(name, displayName));
        }
    }
}
