using System;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOS.NetworkTheSailTwo;

public static class DhcpClient
{
    public static bool RequestConfiguration()
    {
        using (var dhcp = new DHCPClient())
        {
            if (dhcp.SendDiscoverPacket() != -1)
            {
                Console.WriteLine("DHCP configuration applied!");
                Console.WriteLine($"IP Address: {NetworkConfiguration.CurrentAddress}");
                
                return true;
            }
            else
            {
                Console.WriteLine("Failed to obtain DHCP configuration.");
                return false;
            }
        }
    }

    public static void ReleaseConfiguration()
    {
        using (var dhcp = new DHCPClient())
        {
            dhcp.SendReleasePacket();
            NetworkConfiguration.ClearConfigs();
            Console.WriteLine("DHCP configuration released.");
        }
    }
}