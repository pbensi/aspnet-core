namespace app.shared.Dto.PersonalDetail
{
    public class ViewPersonalDetailDto
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }
    }
}
