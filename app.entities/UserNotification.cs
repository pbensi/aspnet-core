namespace app.entities
{
    public class UserNotification : AuditEntity
    {
        public int Id { get; set; }
        public Guid SenderGuid { get; set; }
        public Guid RecieverGuid { get; set; }
        public string Message { get; set; }
        public bool IsSeen { get; set; }
        public bool IsRead { get; set; }
        public bool IsDelete { get; set; }
        public DateTime SeenAt { get; set; }
    }
}
