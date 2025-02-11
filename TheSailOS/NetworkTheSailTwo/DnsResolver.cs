using System;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DNS;

namespace TheSailOS.NetworkTheSailTwo;

public class DnsResolver
{
    private static Address _dnsServerAddress = new Address(8, 8, 8, 8);

    public static Address GetAddress(string hostname)
    {
        try
        {
            using (var xClient = new DnsClient())
            {
                xClient.Connect(_dnsServerAddress);
                xClient.SendAsk(hostname);
                Address destination = xClient.Receive();
                return destination;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] DNS resolution failed: {ex.Message}");
            return null;
        }
    }
}