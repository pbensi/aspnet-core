namespace app.shared.Dto.Account
{
    public class AccountRoleDto
    {
        public int Id { get; set; }
        public string PageName { get; set; }
        public string LabelName { get; set; }
        public bool IsPost { get; set; }
        public bool IsPut { get; set; }
        public bool IsDelete { get; set; }
        public bool IsGet { get; set; }
        public bool IsOptions { get; set; }

        public Guid UserGuid { get; set; }
    }
}
