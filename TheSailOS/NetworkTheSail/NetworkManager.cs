using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOS.NetworkTheSail;

public static class NetworkManager
{
    private static NetworkDevice _networkDevice;
    
    public static void ConfigureManual(Address ip, Address subnetMask, Address gateway)
    {
        _networkDevice = NetworkDevice.GetDeviceByName("eth0");
        IPConfig.Enable(_networkDevice, ip, subnetMask, gateway);
    }

    public static void ConfigureDHCP()
    {
        using (var dhcpClient = new DHCPClient())
        {
            dhcpClient.SendDiscoverPacket();
        }
    }
    
    public static string GetCurrentIP()
    {
        return NetworkConfiguration.CurrentAddress.ToString();
    }
}