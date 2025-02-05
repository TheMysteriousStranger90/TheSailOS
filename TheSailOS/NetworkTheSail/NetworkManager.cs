using System;
using System.Threading;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOS.NetworkTheSail;

public static class NetworkManager
{
    private static NetworkDevice _networkDevice;
    private static DHCPClientManager _dhcpManager;
    private static bool _isInitialized;

    public static bool Initialize(Action<string> statusCallback = null)
    {
        try
        {
            if (_isInitialized) return true;

            // Initialize network device first
            InitializeNetworkDevice();
            if (_networkDevice == null) return false;

            // Enable the device
            if (!_networkDevice.Enable())
            {
                throw new Exception("Failed to enable network device");
            }

            // Wait for device to be ready
            Thread.Sleep(1000);

            _dhcpManager = new DHCPClientManager(statusCallback);
            _isInitialized = true;

            return ConfigureNetwork();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network] Initialization failed: {ex.Message}");
            return false;
        }
    }

    private static void InitializeNetworkDevice()
    {
        var devices = Cosmos.HAL.NetworkDevice.Devices;
        Console.WriteLine($"[Network] Found {devices.Count} network devices");

        foreach (var device in devices)
        {
            Console.WriteLine($"[Network] Testing device: {device.Name}");
            try
            {
                if (device.Enable())
                {
                    _networkDevice = device;
                    Console.WriteLine($"[Network] Using device: {device.Name}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network] Device init failed: {ex.Message}");
            }
        }
    }

    private static bool ConfigureNetwork()
    {
        try
        {
            using (var dhcp = new DHCPClient())
            {
                Console.WriteLine("[Network] Starting DHCP configuration");
                var result = dhcp.SendDiscoverPacket();
                Console.WriteLine($"[Network] DHCP result: {result}");
                return result > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Network] DHCP configuration failed: {ex.Message}");
            return false;
        }
    }

    public static bool IsNetworkAvailable()
    {
        return _networkDevice != null && _networkDevice.Ready && _isInitialized;
    }

    public static string GetCurrentIP()
    {
        return NetworkConfiguration.CurrentAddress?.ToString() ?? "Not configured";
    }
}