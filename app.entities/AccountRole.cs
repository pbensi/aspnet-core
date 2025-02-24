namespace app.entities
{
    public class AccountRole
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string PageName { get; set; }
        public string LabelName { get; set; }
        public bool IsPost { get; set; }
        public bool IsPut { get; set; }
        public bool IsDelete { get; set; }
        public bool IsGet { get; set; }
        public bool IsOptions { get; set; }

        public Account Account { get; set; }
    }
}
