using System.Net;
using System.Net.Sockets;

namespace app.migrator
{
    public static class NetworkProvider
    {
        public static string Ipv4Address()
        {
            string ipV4 = string.Empty;

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipV4 = ip.ToString();
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ipV4;
        }

        public static string Ipv6Address()
        {
            string ipV6 = string.Empty;

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        ipV6 = ip.ToString();
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ipV6;
        }

        public static string DeviceName()
        {
            return Environment.MachineName;
        }

        public static string OperatingSystem()
        {
            return $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
        }
    }
}
