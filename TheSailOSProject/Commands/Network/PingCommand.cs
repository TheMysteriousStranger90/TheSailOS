using System;
using Cosmos.System.Network.Config;
using TheSailOSProject.Network;

namespace TheSailOSProject.Commands.Network;

public class PingCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: ping <ip_address>");
            Console.WriteLine("Example: ping 8.8.8.8");
            return;
        }

        if (NetworkConfiguration.CurrentAddress == null)
        {
            Console.WriteLine("[ERROR] Network is not initialized. Please configure network first.");
            return;
        }

        string ipAddress = args[0];
        var pingClient = new PingClient();
        pingClient.PingIP(ipAddress);
    }
}