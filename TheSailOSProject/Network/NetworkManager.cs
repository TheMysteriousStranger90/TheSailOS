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
        try
        {
            if (Cosmos.HAL.NetworkDevice.Devices.Count < 1)
            {
                throw new Exception("No network devices found.");
            }

            Console.WriteLine("[INFO] Creating new DHCP client...");
            _dhcpClient = new DHCPClient();

            Console.WriteLine("[INFO] Sending DHCP discover packet...");
            _dhcpClient.SendDiscoverPacket();

            Console.WriteLine("[INFO] Retrieving local IP address...");
            Console.WriteLine("[INFO] Closing DHCP client...");
            _dhcpClient.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] DHCP Discover failed.\n" + ex.Message);
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
            using (var dhcp = new DHCPClient())
            {
                Console.WriteLine("Sending DHCP Discover packet...");
                if (dhcp.SendDiscoverPacket() != -1)
                {
                    Console.WriteLine($"IP Address: {NetworkConfiguration.CurrentAddress}");
                }
                else
                {
                    Console.WriteLine("[WARNING] DHCP failed. Please configure network manually.");

                    NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0");
                    if (nic != null)
                    {
                        IPConfig.Enable(nic,
                            new Address(192, 168, 146, 0), // VM IP
                            new Address(255, 255, 255, 0), // Subnet mask
                            new Address(192, 168, 146, 2)); // Gateway

                        DNSConfig.Add(new Address(8, 8, 8, 8));
                        Console.WriteLine($"Static IP configuration applied: IP={NetworkConfiguration.CurrentAddress}");
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] Network device 'eth0' not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Network initialization failed: {ex.Message}");
        }
    }
}