using System;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;

namespace TheSailOSProject.Network;

public static class NetworkManager
{
    static DHCPClient _dhcpClient;
    static NetworkDevice _networkDevice;

    public static void Initialize()
    {
        _dhcpClient = new DHCPClient();
        try
        {
            if (Cosmos.HAL.NetworkDevice.Devices.Count < 1)
            {
                throw new Exception("No network devices found.");
            }

            Console.WriteLine("[INFO] Sending DHCP Discover packet...");
            var packetCode = _dhcpClient.SendDiscoverPacket();
            Console.WriteLine($"[DEBUG] SendDiscoverPacket returned: {packetCode}");

            if (packetCode == -1)
            {
                NetworkConfiguration.ClearConfigs();
                Console.WriteLine("[ERROR] DHCP Discover failed (packetCode == -1).");
                throw new TimeoutException("DHCP Discover timed out.");
            }

            if (NetworkConfiguration.CurrentNetworkConfig == null)
            {
                NetworkConfiguration.ClearConfigs();
                Console.WriteLine("[ERROR] NetworkConfiguration.CurrentNetworkConfig is null.");
                throw new NullReferenceException("NetworkConfiguration.CurrentNetworkConfig is null.");
            }

            var ip = NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress;

            Console.WriteLine("Established Network connection via DHCP IPv4: " + ip, 2);
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine("[ERROR] DHCP Discover timed out: " + ex.Message);
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine("[ERROR] Network configuration is null: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] DHCP Discover failed. Can't apply dynamic IPv4 address. " + ex);
        }
        finally
        {
            _dhcpClient.Close();
        }
    }

    public static void Shutdown()
    {
        try
        {
            Console.WriteLine("[INFO] Closing network connections...");
            _dhcpClient.Close();

            Console.WriteLine("[INFO] Disposing DHCP client...");
            _dhcpClient.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
        }
    }

    public static void ConfigureManually()
    {
        try
        {
            Console.WriteLine("[INFO] Network configure manually...");

            NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0");
            if (nic != null)
            {
                Console.WriteLine("[DEBUG] Network device 'eth0' found.");

                NetworkConfiguration.ClearConfigs();
                Cosmos.HAL.Global.PIT.Wait(100);
                Console.WriteLine("[DEBUG] Cleared network configurations.");

                var ipAddress = new Address(192, 168, 1, 69);
                var subnetMask = new Address(255, 255, 255, 0);
                var gateway = new Address(192, 168, 1, 254);

                IPConfig.Enable(nic, ipAddress, subnetMask, gateway);
                
                if (NetworkConfiguration.CurrentNetworkConfig != null &&
                    NetworkConfiguration.CurrentNetworkConfig.IPConfig != null)
                {
                    var ip = NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress;
                    var sn = NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask;
                    var gw = NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway;
                    Console.WriteLine($"Applied! IPv4: {ip} subnet mask: {sn} gateway: {gw}");
                }
                else
                {
                    Console.WriteLine("[ERROR] Failed to retrieve network configuration after manual configuration.");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Network device 'eth0' not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Network initialization failed: {ex.Message}");
        }
    }

    public static void ConfigureManuallyWithParameters(Address ipAddress, Address subnet, Address gateway,
        string networkDevice = "eth0")
    {
        try
        {
            Console.WriteLine("[INFO] Network configure manually...");

            NetworkDevice nic = NetworkDevice.GetDeviceByName(networkDevice);
            if (nic != null)
            {
                Console.WriteLine("[DEBUG] Network device 'eth0' found.");

                NetworkConfiguration.ClearConfigs();
                Cosmos.HAL.Global.PIT.Wait(100);
                Console.WriteLine("[DEBUG] Cleared network configurations.");

                IPConfig.Enable(nic, ipAddress, subnet, gateway);

                if (NetworkConfiguration.CurrentNetworkConfig != null &&
                    NetworkConfiguration.CurrentNetworkConfig.IPConfig != null)
                {
                    var ip = NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress;
                    var sn = NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask;
                    var gw = NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway;
                    Console.WriteLine($"Applied! IPv4: {ip} subnet mask: {sn} gateway: {gw}");
                }
                else
                {
                    Console.WriteLine("[ERROR] Failed to retrieve network configuration after manual configuration.");
                }
            }
            else
            {
                Console.WriteLine("[ERROR] Network device 'eth0' not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Network initialization failed: {ex.Message}");
        }
    }
}