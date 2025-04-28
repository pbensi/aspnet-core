namespace app.entities
{
    public class AccountSecurityLog : AuditEntity
    {
        public int Id { get; set; }
        public Guid UserGuid { get; set; }
        public string OldPublicKey { get; set; }
        public string OldPublicIV { get; set; }
        public string OldPrivateKey { get; set; }
        public string OldPrivateIV { get; set; }
        public string? DeviceName { get; set; }
        public string? Ipv4Address { get; set; }
        public string? Ipv6Address { get; set; }
        public string? OperatingSystem { get; set; }
    }
}
