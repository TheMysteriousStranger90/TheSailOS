using System;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOS.NetworkTheSail;

public static class NetworkManager
{
    private static NetworkDevice _networkDevice;
    private static DHCPClientManager _dhcpManager;
    
    public static bool Initialize(Action<string> statusCallback = null)
    {
        try
        {
            if (!NetworkDeviceManager.HasNetworkDevice())
                return false;

            _networkDevice = NetworkDeviceManager.GetPrimaryDevice();
            _dhcpManager = new DHCPClientManager(statusCallback);
            
            return _dhcpManager.Initialize();
        }
        catch
        {
            return false;
        }
    }

    public static bool IsNetworkAvailable()
    {
        return _networkDevice != null && _networkDevice.Ready;
    }
    
    public static void ConfigureManual(Address ip, Address subnetMask, Address gateway)
    {
        _networkDevice = NetworkDevice.GetDeviceByName("wlan0");
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