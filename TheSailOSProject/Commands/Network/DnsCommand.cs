using System;
using Cosmos.System.Network.IPv4;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class DnsCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dns <hostname>");
            return;
        }

        string hostname = args[0];
        Address ipAddress = DnsResolver.GetAddress(hostname);

        if (ipAddress != null)
        {
            Console.WriteLine($"IP address for {hostname}: {ipAddress}");
        }
        else
        {
            Console.WriteLine($"Could not resolve {hostname}.");
        }
    }
}