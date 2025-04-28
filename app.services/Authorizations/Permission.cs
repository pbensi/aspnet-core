namespace app.services.Authorizations
{
    internal class Permission
    {
        public string PageName { get; }
        public string LabelName { get; set; }
        public bool IsPost { get; set; }
        public bool IsPut { get; set; }
        public bool IsDelete { get; set; }
        public bool IsGet { get; set; }
        public bool Options { get; set; }
        public List<Permission> Children { get; }

        public Permission(string pageName, string labelName, bool isGet = false)
        {
            PageName = pageName;
            LabelName = labelName;
            IsGet = isGet;
            Children = new List<Permission>();
        }

        public Permission AddChild(string pageName, string labelName, bool isGet = false)
        {
            var child = new Permission(pageName, labelName, isGet);
            Children.Add(child);
            return child;
        }
    }
}
