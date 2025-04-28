namespace app.shared.Dto.Account
{
    public class UserNotificationDto : AuditEntityDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Contents { get; set; }
        public bool IsSeen { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime SeenAt { get; set; }
    }
}
