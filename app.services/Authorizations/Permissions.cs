namespace app.services.Authorizations
{
    internal class Permissions
    {
        public string PageName { get; }
        public string LabelName { get; set; }
        public bool IsPost { get; set; }
        public bool IsPut { get; set; }
        public bool IsDelete { get; set; }
        public bool IsGet { get; set; }
        public bool Options { get; set; }
        public List<Permissions> Children { get; }

        public Permissions(string pageName, string labelName, bool isGet = false)
        {
            PageName = pageName;
            LabelName = labelName;
            IsGet = isGet;
            Children = new List<Permissions>();
        }

        public Permissions AddChild(string pageName, string labelName, bool isGet = false)
        {
            var child = new Permissions(pageName, labelName, isGet);
            Children.Add(child);
            return child;
        }
    }
}
