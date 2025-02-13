using System;
using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;

namespace TheSailOSProject.Commands.Network;

public class NetworkStatusCommand : ICommand
{
    public void Execute(string[] args)
    {
        
        foreach (NetworkConfig config in NetworkConfiguration.NetworkConfigs)
        {
            switch (config.Device.CardType)
            {
                case CardType.Ethernet:
                    Console.Write("Ethernet Card : " + config.Device.NameID + " - " + config.Device.Name);
                    break;
                case CardType.Wireless:
                    Console.Write("Wireless Card : " + config.Device.NameID + " - " + config.Device.Name);
                    break;
            }
            if (NetworkConfiguration.CurrentNetworkConfig.Device == config.Device)
            {
                Console.WriteLine(" (current)");
            }
            else
            {
                Console.WriteLine();
            }

            Console.WriteLine("MAC Address          : " + config.Device.MACAddress.ToString());
            Console.WriteLine("IP Address           : " + config.IPConfig.IPAddress.ToString());
            Console.WriteLine("Subnet mask          : " + config.IPConfig.SubnetMask.ToString());
            Console.WriteLine("Default Gateway      : " + config.IPConfig.DefaultGateway.ToString());
            Console.WriteLine("DNS Nameservers      : ");
            foreach (Address dnsnameserver in DNSConfig.DNSNameservers)
            {
                Console.WriteLine("                       " + dnsnameserver.ToString());
            }
        }
    }
}