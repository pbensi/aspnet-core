using System.Net;

namespace app.entities
{
    public class AccountSecurity
    {
        public Guid UserGuid { get; set; }
        public string PublicKey { get; set; }
        public string PublicIV { get; set; }
        public string PrivateKey { get; set; }
        public string PrivateIV { get; set; }
        public string? DeviceName { get; set; }
        public string? Ipv4Address { get; set; }
        public string? Ipv6Address { get; set; }
        public string? OperatingSystem { get; set; }

        public Account Account { get; set; }
    }
}
