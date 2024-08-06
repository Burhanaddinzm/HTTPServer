using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace HTTPServer;

public class LocalIpAddressResolver
{
    public IPAddress GetIPAddress()
    {
        try
        {
            Console.WriteLine("Trying to get IP Address...");
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var unicastAddresses = networkInterface.GetIPProperties().UnicastAddresses
                        .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address));

                    foreach (UnicastIPAddressInformation ipAddressInfo in unicastAddresses)
                    {
                        Console.WriteLine($"Found IP Address: {ipAddressInfo.Address}");
                        return ipAddressInfo.Address;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while getting the IP Address: {ex.Message}");
        }

        Console.WriteLine("No suitable IP address found. Returning loopback address.");
        return IPAddress.Loopback;
    }
}
