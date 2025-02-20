using System;
using Cosmos.System.Network.IPv4;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class NetworkConfigureCommand : ICommand
{
    public void Execute(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                NetworkManager.ConfigureManually();
                return;
            }

            if (args.Length != 3)
            {
                Console.WriteLine("Usage: netconfig [ip_address subnet_mask gateway]");
                Console.WriteLine("Example: netconfig 192.168.1.69 255.255.255.0 192.168.1.254");
                return;
            }

            string[] ipParts = args[0].Split('.');
            if (ipParts.Length != 4)
            {
                Console.WriteLine("[ERROR] Invalid IP address format. Use format: xxx.xxx.xxx.xxx");
                return;
            }

            string[] subnetParts = args[1].Split('.');
            if (subnetParts.Length != 4)
            {
                Console.WriteLine("[ERROR] Invalid subnet mask format. Use format: xxx.xxx.xxx.xxx");
                return;
            }

            string[] gatewayParts = args[2].Split('.');
            if (gatewayParts.Length != 4)
            {
                Console.WriteLine("[ERROR] Invalid gateway format. Use format: xxx.xxx.xxx.xxx");
                return;
            }

            try
            {
                var ipAddress = new Address(
                    byte.Parse(ipParts[0]),
                    byte.Parse(ipParts[1]),
                    byte.Parse(ipParts[2]),
                    byte.Parse(ipParts[3]));

                var subnetMask = new Address(
                    byte.Parse(subnetParts[0]),
                    byte.Parse(subnetParts[1]),
                    byte.Parse(subnetParts[2]),
                    byte.Parse(subnetParts[3]));

                var gateway = new Address(
                    byte.Parse(gatewayParts[0]),
                    byte.Parse(gatewayParts[1]),
                    byte.Parse(gatewayParts[2]),
                    byte.Parse(gatewayParts[3]));

                NetworkManager.ConfigureManuallyWithParameters(ipAddress, subnetMask, gateway);
            }
            catch (FormatException)
            {
                Console.WriteLine("[ERROR] Invalid number format in IP addresses");
            }
            catch (OverflowException)
            {
                Console.WriteLine("[ERROR] Numbers in IP addresses must be between 0 and 255");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to configure network: {ex.Message}");
        }
    }
}