namespace app.entities
{
    public class AccountSecurityLog : AuditEntity
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string OldEncryptedKey { get; set; }
        public string OldEncryptedIV { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public string OS { get; set; }
    }
}
