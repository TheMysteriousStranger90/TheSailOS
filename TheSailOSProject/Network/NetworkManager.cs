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
    
    public static bool Initialize()
    {
        try
        {
            Console.WriteLine("[NetworkManager] Starting network initialization...");
            Console.WriteLine("[NetworkManager] Step 1: Checking devices...");

            int deviceCount = 0;
            try
            {
                deviceCount = NetworkDevice.Devices.Count;
                Console.WriteLine($"[INFO] Device count: {deviceCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Cannot check device count: {ex.Message}");
                Console.WriteLine($"[ERROR] Exception type: {ex.GetType().Name}");
                return false;
            }

            if (deviceCount < 1)
            {
                Console.WriteLine("[ERROR] No network devices detected!");
                Console.WriteLine("[INFO] Check your VM network adapter settings:");
                Console.WriteLine("  - Adapter should be 'Connected' and 'Connect at power on'");
                Console.WriteLine("  - Use NAT or Bridged mode");
                Console.WriteLine("  - Try adapter type: Intel E1000 or AMD PCNet");
                return false;
            }

            Console.WriteLine("[NetworkManager] Step 2: Waiting for hardware...");
            Global.PIT.Wait(3000);

            Console.WriteLine("[NetworkManager] Step 3: Getting network device...");
            _activeDevice = GetNetworkDevice();

            if (_activeDevice == null)
            {
                Console.WriteLine("[ERROR] Could not access network device!");
                return false;
            }

            Console.WriteLine($"[OK] Network device found: {_activeDevice.Name}");
            
            Console.WriteLine("[NetworkManager] Step 4: Reading MAC address...");
            try
            {
                Console.WriteLine($"[OK] MAC Address: {_activeDevice.MACAddress}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Cannot read MAC: {ex.Message}");
            }

            Console.WriteLine("[NetworkManager] Step 5: Starting DHCP...");
            try
            {
                if (ConfigureDHCP())
                {
                    _isInitialized = true;
                    Console.WriteLine("[SUCCESS] Network initialized via DHCP");
                    DisplayCurrentConfiguration();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DHCP crashed: {ex.Message}");
                Console.WriteLine($"[ERROR] Type: {ex.GetType().Name}");
            }

            Console.WriteLine("[WARNING] DHCP failed, trying manual configuration...");

            Console.WriteLine("[NetworkManager] Step 6: Manual configuration...");
            try
            {
                if (ConfigureManually())
                {
                    _isInitialized = true;
                    Console.WriteLine("[SUCCESS] Network initialized manually");
                    DisplayCurrentConfiguration();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Manual config crashed: {ex.Message}");
                Console.WriteLine($"[ERROR] Type: {ex.GetType().Name}");
            }

            Console.WriteLine("[ERROR] Network initialization failed completely");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FATAL ERROR] Network initialization exception: {ex.Message}");
            Console.WriteLine($"[FATAL ERROR] Type: {ex.GetType().Name}");
            return false;
        }
    }

    private static NetworkDevice GetNetworkDevice()
    {
        try
        {
            Console.WriteLine("[GetDevice] Trying eth0...");
            try
            {
                var device = NetworkDevice.GetDeviceByName("eth0");
                if (device != null)
                {
                    Console.WriteLine("[INFO] Found device by name: eth0");
                    return device;
                }
                Console.WriteLine("[GetDevice] eth0 not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetDevice] eth0 failed: {ex.Message}");
            }

            Console.WriteLine("[GetDevice] Trying first device from list...");
            try
            {
                if (NetworkDevice.Devices != null && NetworkDevice.Devices.Count > 0)
                {
                    Console.WriteLine($"[GetDevice] Accessing device at index 0...");
                    var device = NetworkDevice.Devices[0];
                    if (device != null)
                    {
                        Console.WriteLine("[INFO] Using first available device");
                        return device;
                    }
                    Console.WriteLine("[GetDevice] Device at index 0 is null");
                }
                else
                {
                    Console.WriteLine("[GetDevice] Devices list is null or empty");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetDevice] List access failed: {ex.Message}");
                Console.WriteLine($"[GetDevice] Exception type: {ex.GetType().Name}");
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
        DHCPClient dhcpClient = null;
        
        try
        {
            Console.WriteLine("[DHCP] Step 1: Creating DHCP client...");
            try
            {
                dhcpClient = new DHCPClient();
                Console.WriteLine("[DHCP] Client created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DHCP ERROR] Cannot create client: {ex.Message}");
                Console.WriteLine($"[DHCP ERROR] Type: {ex.GetType().Name}");
                return false;
            }
            
            Console.WriteLine("[DHCP] Step 2: Sending discover packet...");
            try
            {
                dhcpClient.SendDiscoverPacket();
                Console.WriteLine("[DHCP] Discover packet sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DHCP ERROR] Cannot send discover: {ex.Message}");
                Console.WriteLine($"[DHCP ERROR] Type: {ex.GetType().Name}");
                return false;
            }
            
            Console.WriteLine("[DHCP] Step 3: Checking immediate response...");
            try
            {
                var currentAddress = NetworkConfiguration.CurrentAddress;

                if (currentAddress != null &&
                    !currentAddress.ToString().Equals("0.0.0.0") &&
                    !currentAddress.ToString().Equals("255.255.255.255"))
                {
                    Console.WriteLine("[DHCP] Address assigned immediately");
                    Console.WriteLine("[DHCP] Step 4: Closing client...");
                    dhcpClient.Close();
                    Console.WriteLine("[DHCP] Client closed");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DHCP ERROR] Check failed: {ex.Message}");
            }

            Console.WriteLine("[DHCP] Step 5: Waiting for response...");
            Global.PIT.Wait(5000);

            Console.WriteLine("[DHCP] Step 6: Checking delayed response...");
            try
            {
                var currentAddress = NetworkConfiguration.CurrentAddress;

                if (currentAddress != null &&
                    !currentAddress.ToString().Equals("0.0.0.0") &&
                    !currentAddress.ToString().Equals("255.255.255.255"))
                {
                    Console.WriteLine("[DHCP] Address assigned successfully");
                    Console.WriteLine("[DHCP] Step 7: Closing client...");
                    dhcpClient.Close();
                    Console.WriteLine("[DHCP] Client closed");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DHCP ERROR] Check failed: {ex.Message}");
            }

            Console.WriteLine("[DHCP] No address assigned");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DHCP FATAL ERROR] {ex.Message}");
            Console.WriteLine($"[DHCP FATAL ERROR] Type: {ex.GetType().Name}");
            return false;
        }
        finally
        {
            Console.WriteLine("[DHCP] Cleanup: Closing client...");
            try
            {
                if (dhcpClient != null)
                {
                    dhcpClient.Close();
                    Console.WriteLine("[DHCP] Cleanup complete");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DHCP] Cleanup error: {ex.Message}");
            }
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
                Console.WriteLine("[MANUAL] Configuration applied successfully");
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
                try
                {
                    Console.WriteLine($"Available devices: {NetworkDevice.Devices.Count}");
                }
                catch
                {
                    Console.WriteLine("Available devices: Unknown");
                }
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