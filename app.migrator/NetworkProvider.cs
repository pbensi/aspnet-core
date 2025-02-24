using System.Net;
using System.Net.Sockets;

namespace app.migrator
{
    public static class NetworkProvider
    {
        public static string GetIpV4()
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

        public static string GetIpV6()
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

        public static string GetOperatingSystem()
        {
            return $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
        }
    }
}
