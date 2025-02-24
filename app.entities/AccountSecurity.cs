namespace app.entities
{
    public class AccountSecurity
    {
        public Guid UserGuid { get; set; }
        public string EncryptedKey { get; set; }
        public string EncryptedIV { get; set; }
        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public string OS { get; set; }

        public Account Account { get; set; }
    }
}
