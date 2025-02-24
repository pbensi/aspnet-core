namespace app.shared.Dto.PersonalDetail
{
    public class CreatePersonalDetailDto
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }

        public string UserName { get; set; }

        private string _Password = string.Empty;
        public string Password
        {
            get => SecurityUtils.PublicDecrypt(_Password);
            set => _Password = value;
        }
    }
}
