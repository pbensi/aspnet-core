namespace app.shared.Dto.Account
{
    public class AccountSecurityDto
    {
        public string EncryptedKey { get; set; }
        public string EncryptedIV { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public string OS { get; set; }

        public Guid UserGuid { get; set; }
    }
}
