using app.shared.Dto.Account;

namespace app.shared.Dto.PersonalDetail
{
    public class UpdatePersonalDetailDto
    {
        public Guid UserGuid { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }


        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public List<AccountRoleDto> Role { get; set; }
    }
}
