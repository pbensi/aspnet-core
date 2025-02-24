namespace app.migrator.Contexts
{
    public enum AllowedRole
    {
        None = 0,
        Client = 1,
        Server = 2
    }

    public class RequestContext
    {
        public Guid UserGuid { get; set; }
        public string PageName { get; set; } = string.Empty;
        public AllowedRole AllowedRole { get; set; }
    }
}
