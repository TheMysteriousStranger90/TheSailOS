using System;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOSProject.Network;

public static class NetworkManager
{
    private static bool _isInitialized = false;
    private static NetworkDevice _activeDevice = null;
    public static bool IsInitialized => _isInitialized;
    public static NetworkDevice ActiveDevice => _activeDevice;

    public static bool Initialize()
    {
        try
        {
            Console.WriteLine("[NetworkManager] Starting network initialization...");

            Console.WriteLine("[NetworkManager] Waiting for hardware to be ready...");
            Global.PIT.Wait(3000);

            _activeDevice = GetNetworkDevice();

            if (_activeDevice == null)
            {
                Console.WriteLine("[ERROR] No network device found!");
                Console.WriteLine("[INFO] Check your VM network adapter settings:");
                Console.WriteLine("  - Adapter should be 'Connected' and 'Connect at power on'");
                Console.WriteLine("  - Use NAT or Bridged mode");
                Console.WriteLine("  - Try adapter type: Intel E1000 or AMD PCNet");
                return false;
            }

            Console.WriteLine($"[OK] Network device found: {_activeDevice.Name}");
            Console.WriteLine($"[OK] MAC Address: {_activeDevice.MACAddress}");

            if (ConfigureDHCP())
            {
                _isInitialized = true;
                Console.WriteLine("[SUCCESS] Network initialized via DHCP");
                DisplayCurrentConfiguration();
                return true;
            }

            Console.WriteLine("[WARNING] DHCP failed, trying manual configuration...");

            if (ConfigureManually())
            {
                _isInitialized = true;
                Console.WriteLine("[SUCCESS] Network initialized manually");
                DisplayCurrentConfiguration();
                return true;
            }

            Console.WriteLine("[ERROR] Network initialization failed completely");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FATAL ERROR] Network initialization exception: {ex.Message}");
            return false;
        }
    }

    private static NetworkDevice GetNetworkDevice()
    {
        try
        {
            try
            {
                var device = NetworkDevice.GetDeviceByName("eth0");
                if (device != null)
                {
                    Console.WriteLine("[INFO] Found device by name: eth0");
                    return device;
                }
            }
            catch
            {
            }

            string[] possibleNames = { "nic0", "en0", "rtl8139", "pcnet32", "e1000", "vmxnet3" };

            foreach (var name in possibleNames)
            {
                try
                {
                    var device = NetworkDevice.GetDeviceByName(name);
                    if (device != null)
                    {
                        Console.WriteLine($"[INFO] Found device by name: {name}");
                        return device;
                    }
                }
                catch
                {
                }
            }

            try
            {
                if (NetworkDevice.Devices != null)
                {
                    int count = 0;
                    try
                    {
                        count = NetworkDevice.Devices.Count;
                    }
                    catch
                    {
                        return null;
                    }

                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                var device = NetworkDevice.Devices[i];
                                if (device != null)
                                {
                                    Console.WriteLine($"[INFO] Found device at index {i}");
                                    return device;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetNetworkDevice failed: {ex.Message}");
            return null;
        }
    }

    private static bool ConfigureDHCP()
    {
        try
        {
            Console.WriteLine("[DHCP] Sending DHCP discover packet...");

            using (var dhcpClient = new DHCPClient())
            {
                dhcpClient.SendDiscoverPacket();
            }

            Console.WriteLine("[DHCP] Waiting for DHCP response...");
            Global.PIT.Wait(5000);

            var currentAddress = NetworkConfiguration.CurrentAddress;

            if (currentAddress != null &&
                !currentAddress.ToString().Equals("0.0.0.0") &&
                !currentAddress.ToString().Equals("255.255.255.255"))
            {
                Console.WriteLine("[DHCP] Address assigned successfully");
                return true;
            }

            Console.WriteLine("[DHCP] No address assigned");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DHCP ERROR] {ex.Message}");
            return false;
        }
    }

    internal static bool ConfigureManually()
    {
        var ipAddress = new Address(192, 168, 1, 200);
        var subnetMask = new Address(255, 255, 255, 0);
        var gateway = new Address(192, 168, 1, 1);

        return ConfigureManuallyWithParameters(ipAddress, subnetMask, gateway);
    }

    public static bool ConfigureManuallyWithParameters(Address ip, Address subnet, Address gateway)
    {
        try
        {
            if (_activeDevice == null)
            {
                Console.WriteLine("[ERROR] No active network device");
                return false;
            }

            Console.WriteLine("[MANUAL] Configuring network...");
            Console.WriteLine($"[MANUAL] IP: {ip}");
            Console.WriteLine($"[MANUAL] Subnet: {subnet}");
            Console.WriteLine($"[MANUAL] Gateway: {gateway}");

            IPConfig.Enable(_activeDevice, ip, subnet, gateway);

            Console.WriteLine("[MANUAL] Waiting for configuration to apply...");
            Global.PIT.Wait(2000);

            var currentAddress = NetworkConfiguration.CurrentAddress;

            if (currentAddress != null &&
                !currentAddress.ToString().Equals("0.0.0.0"))
            {
                return true;
            }

            Console.WriteLine("[MANUAL ERROR] Configuration not applied");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MANUAL ERROR] {ex.Message}");
            return false;
        }
    }

    private static void DisplayCurrentConfiguration()
    {
        try
        {
            var ip = NetworkConfiguration.CurrentAddress;

            if (ip != null)
            {
                Console.WriteLine($"  IP Address: {ip}");
            }

            if (NetworkConfiguration.CurrentNetworkConfig?.IPConfig != null)
            {
                var config = NetworkConfiguration.CurrentNetworkConfig.IPConfig;
                Console.WriteLine($"  Subnet Mask: {config.SubnetMask}");
                Console.WriteLine($"  Gateway: {config.DefaultGateway}");
            }
        }
        catch
        {
        }
    }

    public static void ShowStatus()
    {
        try
        {
            Console.WriteLine("\n========== Network Status ==========");

            if (!_isInitialized)
            {
                Console.WriteLine("Status: Not Initialized");
                return;
            }

            Console.WriteLine("Status: Initialized");

            if (_activeDevice != null)
            {
                Console.WriteLine($"Device: {_activeDevice.Name}");
                Console.WriteLine($"MAC: {_activeDevice.MACAddress}");
            }

            var currentIp = NetworkConfiguration.CurrentAddress;

            if (currentIp != null && !currentIp.ToString().Equals("0.0.0.0"))
            {
                Console.WriteLine($"IP Address: {currentIp}");

                try
                {
                    if (NetworkConfiguration.CurrentNetworkConfig?.IPConfig != null)
                    {
                        var config = NetworkConfiguration.CurrentNetworkConfig.IPConfig;
                        Console.WriteLine($"Subnet Mask: {config.SubnetMask}");
                        Console.WriteLine($"Gateway: {config.DefaultGateway}");
                    }
                }
                catch
                {
                }
            }
            else
            {
                Console.WriteLine("IP Address: Not configured");
            }

            Console.WriteLine("====================================\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Cannot display status: {ex.Message}");
        }
    }

    public static void Shutdown()
    {
        try
        {
            Console.WriteLine("[NetworkManager] Shutting down network...");

            if (_isInitialized)
            {
                NetworkConfiguration.ClearConfigs();
                _isInitialized = false;
                _activeDevice = null;
                Console.WriteLine("[NetworkManager] Network shut down successfully");
            }
            else
            {
                Console.WriteLine("[NetworkManager] Network was not initialized");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Shutdown failed: {ex.Message}");
        }
    }

    public static Address GetCurrentAddress()
    {
        try
        {
            return NetworkConfiguration.CurrentAddress;
        }
        catch
        {
            return null;
        }
    }
}