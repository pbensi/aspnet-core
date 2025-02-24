namespace app.entities
{
    public class Account : AuditEntity
    {
        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }

        public AccountSecurity AccountSecurity { get; set; }
        public ICollection<AccountRole> AccountRole { get; set; }
        public PersonalDetail PersonalDetail { get; set; }
    }
}
