namespace api.entities
{
    public class User : AuditEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }

    public class UserRoles
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanRemove { get; set; }
        public bool CanView { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
