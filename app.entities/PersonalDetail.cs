namespace app.entities
{
    public class PersonalDetail : AuditEntity
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public bool IsDelete { get; set; }

        public Account Account { get; set; }
    }
}
