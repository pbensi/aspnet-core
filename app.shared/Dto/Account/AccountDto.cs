namespace app.shared.Dto.Account
{
    public class AccountDto
    {
        public AccountDto()
        {
            AccountRoleDto = new List<AccountRoleDto>();
        }

        public Guid UserGuid { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public List<AccountRoleDto> AccountRoleDto { get; set; }
    }
}
